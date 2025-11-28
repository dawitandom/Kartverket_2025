// wwwroot/js/map.report.create.js
(function () {
    document.addEventListener('DOMContentLoaded', function () {
        // --- INIT KART ---
        function hasPosition() {
            const la = parseFloat((document.getElementById('latitudeHidden').value || '').replace(',', '.'));
            const lo = parseFloat((document.getElementById('longitudeHidden').value || '').replace(',', '.'));
            return !Number.isNaN(la) && !Number.isNaN(lo);
        }

        function updateButtons() {
            const can = hasPosition();
            const btnSave = document.getElementById('btnSave');
            const btnSubmit = document.getElementById('btnSubmit');

            [btnSave, btnSubmit].forEach(btn => {
                if (!btn) return;
                if (can) {
                    btn.classList.remove('btn-blocked');
                    btn.removeAttribute('aria-disabled');
                    btn.removeAttribute('title');
                } else {
                    btn.classList.add('btn-blocked');
                    btn.setAttribute('aria-disabled', 'true');
                    btn.setAttribute('title', 'Select a location on the map first');
                }
            });
        }

        // Stopper innsending av skjema hvis brukeren ikke har valgt posisjon på kartet
        (function initFormGuard() {
            updateButtons();
            const form = document.getElementById('reportForm');
            const positionError = document.getElementById('positionError');

            if (form) {
                form.addEventListener('submit', (e) => {
                    if (!hasPosition()) {
                        e.preventDefault();
                        positionError.classList.remove('d-none');
                        positionError.scrollIntoView({ behavior: 'smooth', block: 'center' });
                    } else {
                        positionError.classList.add('d-none');
                    }
                });
            }
        })();

        const map = L.map('map').setView([59.911491, 10.757933], 13);
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; OpenStreetMap contributors', maxZoom: 19
        }).addTo(map);

        const drawnItems = new L.FeatureGroup();
        map.addLayer(drawnItems);

        const drawStyles = {
            rectangle: { color: '#FF6F61', weight: 3, opacity: 0.95, fillColor: '#FFB6A6', fillOpacity: 0.35 },
            circle: { color: '#FF6F61', weight: 3, opacity: 0.95, fillColor: '#FFB6A6', fillOpacity: 0.35 },
            polyline: { color: '#0055FF', weight: 4, opacity: 0.95 }
        };

        const handlers = {
            marker: new L.Draw.Marker(map),
            line: new L.Draw.Polyline(map, { shapeOptions: drawStyles.polyline }),
            rect: new L.Draw.Rectangle(map, { shapeOptions: drawStyles.rectangle }),
            circle: new L.Draw.Circle(map, { shapeOptions: drawStyles.circle }),
            edit: new L.EditToolbar.Edit(map, { featureGroup: drawnItems, selectedPathOptions: { maintainColor: true } }),
            del: new L.EditToolbar.Delete(map, { featureGroup: drawnItems })
        };

        function disableAllModes() {
            handlers.marker.disable();
            handlers.line.disable();
            handlers.rect.disable();
            handlers.circle.disable();
            handlers.edit.disable();
            handlers.del.disable();
        }

        function setActive(btn) {
            document.querySelectorAll('.map-tools .tool').forEach(b => b.classList.remove('active'));
            if (btn) btn.classList.add('active');
        }

        let marker = null;

        function centroidOfPolygon(latlngs) {
            const pts = latlngs.map(p => ({ x: p.lng, y: p.lat }));
            const n = pts.length;
            let twiceArea = 0, cx = 0, cy = 0;
            for (let i = 0; i < n; i++) {
                const j = (i + 1) % n;
                const cross = pts[i].x * pts[j].y - pts[j].x * pts[i].y;
                twiceArea += cross;
                cx += (pts[i].x + pts[j].x) * cross;
                cy += (pts[i].y + pts[j].y) * cross;
            }
            if (Math.abs(twiceArea) < 1e-12) {
                let slat = 0, slng = 0;
                latlngs.forEach(p => { slat += p.lat; slng += p.lng; });
                return L.latLng(slat / latlngs.length, slng / latlngs.length);
            }
            cx /= (3 * twiceArea);
            cy /= (3 * twiceArea);
            return L.latLng(cy, cx);
        }

        function midpointOfPolyline(latlngs) {
            if (latlngs.length === 1) return latlngs[0];
            let total = 0; const seg = [];
            for (let i = 0; i < latlngs.length - 1; i++) {
                const d = map.distance(latlngs[i], latlngs[i + 1]);
                seg.push(d); total += d;
            }
            const target = total / 2; let acc = 0;
            for (let i = 0; i < seg.length; i++) {
                if (acc + seg[i] >= target) {
                    const t = seg[i] ? (target - acc) / seg[i] : 0;
                    const A = latlngs[i], B = latlngs[i + 1];
                    return L.latLng(A.lat + (B.lat - A.lat) * t, A.lng + (B.lng - A.lng) * t);
                }
                acc += seg[i];
            }
            return latlngs[latlngs.length - 1];
        }

        function firstRing(layer) {
            let latlngs = layer.getLatLngs();
            if (Array.isArray(latlngs) && Array.isArray(latlngs[0])) latlngs = latlngs[0];
            return latlngs;
        }

        function layerToGeoJsonCoords(layer) {
            let latlngs = layer.getLatLngs();

            if (!Array.isArray(latlngs)) {
                if (typeof layer.getLatLng === 'function') {
                    const ll = layer.getLatLng();
                    return [[parseFloat(ll.lng.toFixed(9)), parseFloat(ll.lat.toFixed(9))]];
                }
                return [];
            }

            if (Array.isArray(latlngs[0])) {
                const inner = latlngs[0];
                if (inner.length && inner[0].lat !== undefined) {
                    latlngs = inner;
                }
            }

            if (!latlngs.length || latlngs[0].lat === undefined) {
                return [];
            }

            return latlngs.map(p => [
                parseFloat(p.lng.toFixed(9)),
                parseFloat(p.lat.toFixed(9))
            ]);
        }

        function setGeometryHidden(type, coordsArray, radius = null) {
            const geo = { type: type, coordinates: coordsArray };
            if (radius !== null) geo.radius = radius;
            document.getElementById('geometryHidden').value = JSON.stringify(geo);
        }

        function updateCoordinates(lat, lng) {
            lat = parseFloat(Number(lat).toFixed(9));
            lng = parseFloat(Number(lng).toFixed(9));
            const latStr = lat.toString().replace(',', '.');
            const lngStr = lng.toString().replace(',', '.');

            document.getElementById('latitudeHidden').value = latStr;
            document.getElementById('longitudeHidden').value = lngStr;
            document.getElementById('latitudeManual').value = latStr;
            document.getElementById('longitudeManual').value = lngStr;

            document.getElementById('coordsDisplay').innerHTML =
                `Lat: <strong>${lat.toFixed(6)}</strong>, Lon: <strong>${lng.toFixed(6)}</strong>`;

            if (marker) {
                marker.setLatLng([lat, lng]);
            } else {
                marker = L.marker([lat, lng]).addTo(map);
            }
            map.setView([lat, lng], 13);
            updateButtons();
        }

        function handleLayerUpdate(layer) {
            if (layer instanceof L.Marker) {
                const ll = layer.getLatLng();
                updateCoordinates(ll.lat, ll.lng);
                setGeometryHidden('Point', [[ll.lng, ll.lat]]);
                return;
            }
            if (layer instanceof L.Rectangle) {
                const c = layer.getBounds().getCenter();
                updateCoordinates(c.lat, c.lng);
                const coords = layerToGeoJsonCoords(layer);
                setGeometryHidden('Rectangle', [coords]);
                return;
            }
            if (layer instanceof L.Circle) {
                const c = layer.getLatLng();
                const radius = layer.getRadius();
                updateCoordinates(c.lat, c.lng);
                document.getElementById('coordsDisplay').innerHTML =
                    `Center: <strong>${c.lat.toFixed(6)}, ${c.lng.toFixed(6)}</strong> – Radius: <strong>${Math.round(radius)} m</strong>`;
                setGeometryHidden('Circle', [[c.lng, c.lat]], radius);
                return;
            }
            if (layer instanceof L.Polygon) {
                const ring = firstRing(layer);
                const c = centroidOfPolygon(ring);
                updateCoordinates(c.lat, c.lng);
                const coords = layerToGeoJsonCoords(layer);
                setGeometryHidden('Polygon', [coords]);
                return;
            }
            if (layer instanceof L.Polyline) {
                let latlngs = layer.getLatLngs();
                if (Array.isArray(latlngs) && Array.isArray(latlngs[0]) && latlngs[0].lat === undefined) latlngs = latlngs[0];
                const mid = midpointOfPolyline(latlngs);
                updateCoordinates(mid.lat, mid.lng);
                const coords = layerToGeoJsonCoords(layer);
                setGeometryHidden('LineString', coords);
                document.getElementById('coordsDisplay').innerHTML = `Line: ${coords.length} points (midpoint shown)`;
            }
        }

        // Toolbar knapper
        document.getElementById('tool-line').addEventListener('click', e => {
            disableAllModes(); handlers.line.enable(); setActive(e.currentTarget);
        });
        document.getElementById('tool-rect').addEventListener('click', e => {
            disableAllModes(); handlers.rect.enable(); setActive(e.currentTarget);
        });
        document.getElementById('tool-circle').addEventListener('click', e => {
            disableAllModes(); handlers.circle.enable(); setActive(e.currentTarget);
        });
        document.getElementById('tool-edit').addEventListener('click', e => {
            disableAllModes(); handlers.edit.enable(); setActive(e.currentTarget);
        });
        document.getElementById('tool-edit-done').addEventListener('click', () => {
            handlers.edit.disable(); setActive(null);
        });
        document.getElementById('tool-del').addEventListener('click', e => {
            disableAllModes(); handlers.del.enable(); setActive(e.currentTarget);
        });

        map.on('draw:created draw:edited draw:deleted', () => setActive(null));

        map.on(L.Draw.Event.CREATED, function (e) {
            drawnItems.clearLayers();
            const layer = e.layer;
            drawnItems.addLayer(layer);

            if (layer instanceof L.Polygon || layer instanceof L.Rectangle) {
                if (typeof layer.setStyle === 'function') layer.setStyle(drawStyles.rectangle);
            } else if (layer instanceof L.Polyline) {
                if (typeof layer.setStyle === 'function') layer.setStyle(drawStyles.polyline);
            }
            handleLayerUpdate(layer);
        });

        map.on('draw:editmove', function (e) {
            if (e.layer) handleLayerUpdate(e.layer);
        });
        map.on('draw:editvertex', function (e) {
            if (e.layers && typeof e.layers.eachLayer === 'function') {
                e.layers.eachLayer(l => handleLayerUpdate(l));
            } else if (e.layer) {
                handleLayerUpdate(e.layer);
            }
        });

        map.on(L.Draw.Event.EDITED, function (e) {
            e.layers.eachLayer(function (layer) {
                handleLayerUpdate(layer);
            });
        });

        map.on(L.Draw.Event.DELETED, function () {
            drawnItems.clearLayers();
            document.getElementById('latitudeHidden').value = '';
            document.getElementById('longitudeHidden').value = '';
            document.getElementById('latitudeManual').value = '';
            document.getElementById('longitudeManual').value = '';
            document.getElementById('geometryHidden').value = '';
            document.getElementById('coordsDisplay').innerHTML = 'Click on the map or use your location';
            if (marker) { map.removeLayer(marker); marker = null; }
            updateButtons();
        });

        map.on('click', function (e) {
            drawnItems.clearLayers();
            document.getElementById('geometryHidden').value = '';
            updateCoordinates(e.latlng.lat, e.latlng.lng);
        });

        const useMyLocationBtn = document.getElementById('useMyLocation');
        if (useMyLocationBtn) {
            useMyLocationBtn.addEventListener('click', function () {
                if (!navigator.geolocation) { alert('Geolocation is not supported by your browser.'); return; }
                const originalText = this.innerHTML;
                this.innerHTML = 'Getting location...'; this.disabled = true;

                navigator.geolocation.getCurrentPosition(
                    (pos) => {
                        drawnItems.clearLayers();
                        document.getElementById('geometryHidden').value = '';
                        updateCoordinates(pos.coords.latitude, pos.coords.longitude);
                        this.innerHTML = originalText; this.disabled = false;
                    },
                    (err) => {
                        this.innerHTML = originalText; this.disabled = false;
                        alert('Could not get your location. Please check your browser permissions.');
                        console.error(err);
                    },
                    { enableHighAccuracy: true, timeout: 6000 }
                );
            });
        }

        const clearLocationBtn = document.getElementById('clearLocation');
        if (clearLocationBtn) {
            clearLocationBtn.addEventListener('click', function () {
                document.getElementById('latitudeHidden').value = '';
                document.getElementById('longitudeHidden').value = '';
                document.getElementById('latitudeManual').value = '';
                document.getElementById('longitudeManual').value = '';
                document.getElementById('geometryHidden').value = '';
                document.getElementById('coordsDisplay').innerHTML = 'Click on the map or use your location';
                if (marker) { map.removeLayer(marker); marker = null; }
                drawnItems.clearLayers();
                updateButtons();
            });
        }

        document.getElementById('latitudeManual').addEventListener('change', function () {
            const lat = parseFloat(this.value.replace(',', '.'));
            const lng = parseFloat(document.getElementById('longitudeManual').value.replace(',', '.'));
            if (!isNaN(lat) && !isNaN(lng)) {
                drawnItems.clearLayers();
                document.getElementById('geometryHidden').value = '';
                updateCoordinates(lat, lng);
            }
        });

        document.getElementById('longitudeManual').addEventListener('change', function () {
            const lat = parseFloat(document.getElementById('latitudeManual').value.replace(',', '.'));
            const lng = parseFloat(this.value.replace(',', '.'));
            if (!isNaN(lat) && !isNaN(lng)) {
                drawnItems.clearLayers();
                document.getElementById('geometryHidden').value = '';
                updateCoordinates(lat, lng);
            }
        });
    });
})();