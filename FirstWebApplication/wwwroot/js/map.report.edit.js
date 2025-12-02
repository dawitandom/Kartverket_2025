// wwwroot/js/map.report.edit.js
(function () {
    function initEditMap() {
        const mapElement = document.getElementById("map");
        if (!mapElement || typeof L === "undefined") {
            return; // ingen kart på siden eller Leaflet ikke lastet
        }

        // --- INIT KART ---
        const map = L.map("map").setView([59.911491, 10.757933], 13);
        L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
            attribution: "&copy; OpenStreetMap contributors",
            maxZoom: 19
        }).addTo(map);

        const drawnItems = new L.FeatureGroup();
        map.addLayer(drawnItems);

        const drawStyles = {
            rectangle: { color: "#FF6F61", weight: 3, opacity: 0.95, fillColor: "#FFB6A6", fillOpacity: 0.35 },
            circle: { color: "#FF6F61", weight: 3, opacity: 0.95, fillColor: "#FFB6A6", fillOpacity: 0.35 },
            polyline: { color: "#0055FF", weight: 4, opacity: 0.95 }
        };

        // EGEN handlers (programmatisk)
        const handlers = {
            marker: new L.Draw.Marker(map),
            line: new L.Draw.Polyline(map, { shapeOptions: drawStyles.polyline }),
            rect: new L.Draw.Rectangle(map, { shapeOptions: drawStyles.rectangle }),
            circle: new L.Draw.Circle(map, { shapeOptions: drawStyles.circle }),
            edit: new L.EditToolbar.Edit(map, { featureGroup: drawnItems, selectedPathOptions: { maintainColor: true } })
        };

        function disableAllModes() {
            handlers.marker.disable();
            handlers.line.disable();
            handlers.rect.disable();
            handlers.circle.disable();
            handlers.edit.disable();
        }

        function setActive(btn) {
            document
                .querySelectorAll(".map-tools .tool")
                .forEach(b => b.classList.remove("active"));
            if (btn) btn.classList.add("active");
        }

        let marker = null; // visuell pekepinne midt i objektet / på punkt

        // ---- PRESISE SENTRE ----
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
            let total = 0;
            const seg = [];
            for (let i = 0; i < latlngs.length - 1; i++) {
                const d = map.distance(latlngs[i], latlngs[i + 1]);
                seg.push(d);
                total += d;
            }
            const target = total / 2;
            let acc = 0;
            for (let i = 0; i < seg.length; i++) {
                if (acc + seg[i] >= target) {
                    const t = seg[i] ? (target - acc) / seg[i] : 0;
                    const A = latlngs[i], B = latlngs[i + 1];
                    return L.latLng(
                        A.lat + (B.lat - A.lat) * t,
                        A.lng + (B.lng - A.lng) * t
                    );
                }
                acc += seg[i];
            }
            return latlngs[latlngs.length - 1];
        }

        function layerToGeoJsonCoords(layer) {
            let latlngs = layer.getLatLngs();

            if (!Array.isArray(latlngs)) {
                if (typeof layer.getLatLng === "function") {
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

        function setGeometryHidden(type, coordsArray, radius) {
            const geo = {
                type: type,
                coordinates: coordsArray
            };

            if (typeof radius === "number") {
                geo.radius = radius;
            }

            const geomField = document.getElementById("geometryHidden");
            if (geomField) {
                geomField.value = JSON.stringify(geo);
            }
        }

        function updateCoordinates(lat, lng) {
            lat = parseFloat(Number(lat).toFixed(9));
            lng = parseFloat(Number(lng).toFixed(9));

            const latStr = lat.toString().replace(",", ".");
            const lngStr = lng.toString().replace(",", ".");

            const latHidden = document.getElementById("latitudeHidden");
            const lngHidden = document.getElementById("longitudeHidden");
            const latManual = document.getElementById("latitudeManual");
            const lngManual = document.getElementById("longitudeManual");
            const coordsDisplay = document.getElementById("coordsDisplay");

            if (latHidden) latHidden.value = latStr;
            if (lngHidden) lngHidden.value = lngStr;
            if (latManual) latManual.value = latStr;
            if (lngManual) lngManual.value = lngStr;

            if (coordsDisplay) {
                coordsDisplay.innerHTML =
                    "Lat: <strong>" + lat.toFixed(6) + "</strong>, Lon: <strong>" + lng.toFixed(6) + "</strong>";
            }

            if (marker) {
                marker.setLatLng([lat, lng]);
            } else {
                marker = L.marker([lat, lng]).addTo(map);
            }
            map.setView([lat, lng], 13);
        }

        function handleLayerUpdate(layer) {
            // --- POINT ---
            if (layer instanceof L.Marker) {
                const ll = layer.getLatLng();
                updateCoordinates(ll.lat, ll.lng);
                setGeometryHidden("Point", [[ll.lng, ll.lat]]);
                return;
            }

            // --- RECTANGLE ---
            if (layer instanceof L.Rectangle) {
                const center = layer.getBounds().getCenter();
                updateCoordinates(center.lat, center.lng);

                const coords = layerToGeoJsonCoords(layer);
                setGeometryHidden("Rectangle", [coords]);

                const coordsDisplay = document.getElementById("coordsDisplay");
                if (coordsDisplay) {
                    coordsDisplay.innerHTML = "Rectangle (" + coords.length + " corners)";
                }
                return;
            }

            // --- CIRCLE ---
            if (layer instanceof L.Circle) {
                const c = layer.getLatLng();
                const radius = layer.getRadius();

                updateCoordinates(c.lat, c.lng);

                const coords = [[
                    parseFloat(c.lng.toFixed(9)),
                    parseFloat(c.lat.toFixed(9))
                ]];

                setGeometryHidden("Circle", coords, radius);

                const coordsDisplay = document.getElementById("coordsDisplay");
                if (coordsDisplay) {
                    coordsDisplay.innerHTML =
                        "Circle center: <strong>" +
                        c.lat.toFixed(6) + ", " + c.lng.toFixed(6) +
                        "</strong> – Radius: <strong>" +
                        Math.round(radius) + " m</strong>";
                }
                return;
            }

            // --- POLYGON ---
            if (layer instanceof L.Polygon) {
                const ring = layer.getLatLngs()[0];
                const center = centroidOfPolygon(ring);

                updateCoordinates(center.lat, center.lng);

                const coords = layerToGeoJsonCoords(layer);
                setGeometryHidden("Polygon", [coords]);

                const coordsDisplay = document.getElementById("coordsDisplay");
                if (coordsDisplay) {
                    coordsDisplay.innerHTML =
                        "Polygon with " + coords.length + " points";
                }
                return;
            }

            // --- LINE ---
            if (layer instanceof L.Polyline) {
                let latlngs = layer.getLatLngs();
                if (Array.isArray(latlngs) && Array.isArray(latlngs[0]) && latlngs[0].lat === undefined) {
                    latlngs = latlngs[0];
                }

                const mid = midpointOfPolyline(latlngs);
                updateCoordinates(mid.lat, mid.lng);

                const coords = layerToGeoJsonCoords(layer);
                setGeometryHidden("LineString", coords);

                const coordsDisplay = document.getElementById("coordsDisplay");
                if (coordsDisplay) {
                    coordsDisplay.innerHTML =
                        "Line: " + coords.length + " points (midpoint shown)";
                }
                return;
            }
        }

        // --- KNAPP-BINDINGER (ekstern toolbar) ---
        const btnLine = document.getElementById("tool-line");
        const btnRect = document.getElementById("tool-rect");
        const btnCircle = document.getElementById("tool-circle");
        const btnEdit = document.getElementById("tool-edit");
        const btnEditDone = document.getElementById("tool-edit-done");

        if (btnLine) {
            btnLine.addEventListener("click", e => {
                disableAllModes(); handlers.line.enable(); setActive(e.currentTarget);
            });
        }
        if (btnRect) {
            btnRect.addEventListener("click", e => {
                disableAllModes(); handlers.rect.enable(); setActive(e.currentTarget);
            });
        }
        if (btnCircle) {
            btnCircle.addEventListener("click", e => {
                disableAllModes(); handlers.circle.enable(); setActive(e.currentTarget);
            });
        }
        if (btnEdit) {
            btnEdit.addEventListener("click", e => {
                disableAllModes(); handlers.edit.enable(); setActive(e.currentTarget);
            });
        }
        if (btnEditDone) {
            btnEditDone.addEventListener("click", () => {
                handlers.edit.disable(); setActive(null);
            });
        }

        // Rydd aktiv-state når noe er ferdig
        map.on("draw:created draw:edited draw:deleted", () => setActive(null));

        // --- HENDELSER ---
        map.on(L.Draw.Event.CREATED, function (e) {
            drawnItems.clearLayers();
            const layer = e.layer;
            drawnItems.addLayer(layer);

            if (layer instanceof L.Polygon || layer instanceof L.Rectangle) {
                if (typeof layer.setStyle === "function") layer.setStyle(drawStyles.rectangle);
            } else if (layer instanceof L.Polyline) {
                if (typeof layer.setStyle === "function") layer.setStyle(drawStyles.polyline);
            }
            handleLayerUpdate(layer);
        });

        // Live oppdatering under redigering
        map.on("draw:editmove", function (e) {
            if (e.layer) handleLayerUpdate(e.layer);
        });
        map.on("draw:editvertex", function (e) {
            if (e.layers && typeof e.layers.eachLayer === "function") {
                e.layers.eachLayer(l => handleLayerUpdate(l));
            } else if (e.layer) {
                handleLayerUpdate(e.layer);
            }
        });

        // Ferdig redigert
        map.on(L.Draw.Event.EDITED, function (e) {
            e.layers.eachLayer(function (layer) {
                handleLayerUpdate(layer);
            });
        });

        // Slettet
        map.on(L.Draw.Event.DELETED, function () {
            drawnItems.clearLayers();
            const latHidden = document.getElementById("latitudeHidden");
            const lngHidden = document.getElementById("longitudeHidden");
            const latManual = document.getElementById("latitudeManual");
            const lngManual = document.getElementById("longitudeManual");
            const geomField = document.getElementById("geometryHidden");
            const coordsDisplay = document.getElementById("coordsDisplay");

            if (latHidden) latHidden.value = "";
            if (lngHidden) lngHidden.value = "";
            if (latManual) latManual.value = "";
            if (lngManual) lngManual.value = "";
            if (geomField) geomField.value = "";
            if (coordsDisplay) coordsDisplay.innerHTML = "Click on the map or use your location";

            if (marker) {
                map.removeLayer(marker);
                marker = null;
            }
        });

        // Klikk i kartet = enkel posisjon
        map.on("click", function (e) {
            drawnItems.clearLayers();
            const geomField = document.getElementById("geometryHidden");
            if (geomField) geomField.value = "";
            updateCoordinates(e.latlng.lat, e.latlng.lng);
        });

        // "Bruk min posisjon"
        const useMyLocationBtn = document.getElementById("useMyLocation");
        if (useMyLocationBtn) {
            useMyLocationBtn.addEventListener("click", function () {
                if (!navigator.geolocation) {
                    alert("Geolocation is not supported by your browser.");
                    return;
                }
                const originalText = this.innerHTML;
                this.innerHTML = "Getting location...";
                this.disabled = true;

                navigator.geolocation.getCurrentPosition(
                    (pos) => {
                        drawnItems.clearLayers();
                        const geomField = document.getElementById("geometryHidden");
                        if (geomField) geomField.value = "";
                        updateCoordinates(pos.coords.latitude, pos.coords.longitude);
                        this.innerHTML = originalText;
                        this.disabled = false;
                    },
                    (err) => {
                        this.innerHTML = originalText;
                        this.disabled = false;
                        alert("Could not get your location. Please check your browser permissions.");
                        console.error(err);
                    },
                    { enableHighAccuracy: true, timeout: 6000 }
                );
            });
        }

        // "Clear"
        const clearLocationBtn = document.getElementById("clearLocation");
        if (clearLocationBtn) {
            clearLocationBtn.addEventListener("click", function () {
                const latHidden = document.getElementById("latitudeHidden");
                const lngHidden = document.getElementById("longitudeHidden");
                const latManual = document.getElementById("latitudeManual");
                const lngManual = document.getElementById("longitudeManual");
                const geomField = document.getElementById("geometryHidden");
                const coordsDisplay = document.getElementById("coordsDisplay");

                if (latHidden) latHidden.value = "";
                if (lngHidden) lngHidden.value = "";
                if (latManual) latManual.value = "";
                if (lngManual) lngManual.value = "";
                if (geomField) geomField.value = "";
                if (coordsDisplay) coordsDisplay.innerHTML = "Click on the map or use your location";

                if (marker) {
                    map.removeLayer(marker);
                    marker = null;
                }
                drawnItems.clearLayers();
            });
        }

        // Manuelle inputs → kart
        const latManualInput = document.getElementById("latitudeManual");
        const lngManualInput = document.getElementById("longitudeManual");

        if (latManualInput) {
            latManualInput.addEventListener("change", function () {
                const lat = parseFloat(this.value.replace(",", "."));
                const lng = lngManualInput ? parseFloat(lngManualInput.value.replace(",", ".")) : NaN;
                if (!isNaN(lat) && !isNaN(lng)) {
                    drawnItems.clearLayers();
                    const geomField = document.getElementById("geometryHidden");
                    if (geomField) geomField.value = "";
                    updateCoordinates(lat, lng);
                }
            });
        }

        if (lngManualInput) {
            lngManualInput.addEventListener("change", function () {
                const lat = latManualInput ? parseFloat(latManualInput.value.replace(",", ".")) : NaN;
                const lng = parseFloat(this.value.replace(",", "."));
                if (!isNaN(lat) && !isNaN(lng)) {
                    drawnItems.clearLayers();
                    const geomField = document.getElementById("geometryHidden");
                    if (geomField) geomField.value = "";
                    updateCoordinates(lat, lng);
                }
            });
        }

        // Preload figur når vi redigerer en lagret rapport
        function preloadPointFromLatLon() {
            const latValRaw = document.getElementById("latitudeHidden")?.value;
            const lonValRaw = document.getElementById("longitudeHidden")?.value;

            if (latValRaw && lonValRaw) {
                const latVal = parseFloat(latValRaw.replace(",", "."));
                const lonVal = parseFloat(lonValRaw.replace(",", "."));

                if (!Number.isNaN(latVal) && !Number.isNaN(lonVal)) {
                    updateCoordinates(latVal, lonVal);
                }
            }
        }

        (function preloadFromGeometryOrCoords() {
            const geomField = document.getElementById("geometryHidden");
            const geomValue = geomField ? geomField.value : "";

            if (geomValue) {
                let geo;
                try {
                    geo = JSON.parse(geomValue);
                } catch (e) {
                    console.error("Could not parse Geometry JSON:", e);
                    geo = null;
                }

                if (!geo || !geo.type) {
                    console.warn("Geometry missing type – falls back to point");
                    preloadPointFromLatLon();
                    return;
                }

                const toLatLng = coord => L.latLng(coord[1], coord[0]);
                let layer = null;

                switch (geo.type) {
                    case "Point": {
                        if (Array.isArray(geo.coordinates) && geo.coordinates[0]) {
                            const [lng, lat] = geo.coordinates[0];
                            layer = L.marker([lat, lng]);
                        }
                        break;
                    }
                    case "LineString": {
                        const latlngs = (geo.coordinates || []).map(toLatLng);
                        layer = L.polyline(latlngs, drawStyles.polyline);
                        break;
                    }
                    case "Polygon":
                    case "Rectangle": {
                        const ring = geo.coordinates?.[0] || [];
                        const latlngs = ring.map(toLatLng);
                        layer = L.rectangle(latlngs, drawStyles.rectangle);
                        break;
                    }
                    case "Circle": {
                        const coord = geo.coordinates?.[0];
                        if (Array.isArray(coord)) {
                            const [lng, lat] = coord;
                            const radius = geo.radius || 200;
                            layer = L.circle([lat, lng], Object.assign({ radius: radius }, drawStyles.circle));
                        }
                        break;
                    }
                }

                if (layer) {
                    drawnItems.clearLayers();
                    drawnItems.addLayer(layer);
                    handleLayerUpdate(layer);
                } else {
                    console.warn("Could not build figure from Geometry – falls back to point");
                    preloadPointFromLatLon();
                }
            } else {
                preloadPointFromLatLon();
            }
        })();
    }

    function initObstaclePopover() {
        const btn = document.getElementById("obstaclePopoverBtn");
        if (!btn || typeof bootstrap === "undefined" || !bootstrap.Popover) return;

        const contentHtml = `
        <p>Select the type that best matches the obstacle. If unsure, pick <strong>Other</strong> and describe it.</p>
        <ul>
          <li><strong>Tower</strong> - large, permanent tower structures (e.g., observation towers, communication towers).</li>
          <li><strong>Crane</strong> - construction cranes, usually temporary structures used for lifting.</li>
          <li><strong>Building</strong> - permanent buildings or large man-made structures.</li>
          <li><strong>Power line</strong> - overhead electrical lines carrying power.</li>
          <li><strong>Mast</strong> - tall structures used for telecom, radio or observation purposes.</li>
          <li><strong>Other</strong> — anything not covered by the categories above (e.g., balloons, temporary objects, unusual structures).</li>
        </ul>
      `;

        const pop = new bootstrap.Popover(btn, {
            content: contentHtml,
            html: true,
            trigger: "manual",
            sanitize: false
        });

        btn.addEventListener("click", (e) => {
            e.preventDefault();
            if (document.querySelector(".popover")) {
                pop.hide();
            } else {
                pop.show();
            }
        });

        function hideIfOutside(ev) {
            const popEl = document.querySelector(".popover");
            if (!popEl) return;
            if (btn.contains(ev.target) || popEl.contains(ev.target)) return;
            pop.hide();
        }

        document.addEventListener("click", hideIfOutside);
        document.addEventListener("keydown", (ev) => {
            if (ev.key === "Escape" || ev.key === "Esc") {
                if (document.querySelector(".popover")) pop.hide();
            }
        });
    }

    document.addEventListener("DOMContentLoaded", function () {
        // Kjør bare på sider som faktisk har kartet
        if (document.getElementById("map")) {
            initEditMap();
        }
        initObstaclePopover();
    });
})();
