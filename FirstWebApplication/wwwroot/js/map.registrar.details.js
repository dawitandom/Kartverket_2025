// wwwroot/js/map.registrar.details.js
(function () {
    function loadLeaflet(cb) {
        if (window.L) { cb(); return; }

        var link = document.createElement('link');
        link.rel = 'stylesheet';
        link.href = 'https://unpkg.com/leaflet@1.9.4/dist/leaflet.css';
        document.head.appendChild(link);

        var s = document.createElement('script');
        s.src = 'https://unpkg.com/leaflet@1.9.4/dist/leaflet.js';
        s.onload = cb;
        document.body.appendChild(s);
    }

    document.addEventListener('DOMContentLoaded', function () {
        const wrap = document.getElementById('mapWrap');
        const mapEl = document.getElementById('detailMap');
        if (!wrap || !mapEl) return;

        const latAttr = mapEl.getAttribute('data-lat');
        const lngAttr = mapEl.getAttribute('data-lng');
        if (!latAttr || !lngAttr) return;

        const lat = parseFloat(latAttr);
        const lng = parseFloat(lngAttr);

        function initMap() {
            const map = L.map(mapEl, { zoomControl: true }).setView([lat, lng], 14);

            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                attribution: '&copy; OpenStreetMap contributors'
            }).addTo(map);

            const geoEl = document.getElementById('geometryJson');
            const geoJsonStr = geoEl ? geoEl.textContent.trim() : "";
            let layer = null;

            console.log("RegistrarDetails Geometry raw:", geoJsonStr);

            if (geoJsonStr && geoJsonStr !== "" && geoJsonStr !== "null") {
                try {
                    const geom = JSON.parse(geoJsonStr);
                    const type = geom.type;
                    const coords = geom.coordinates;
                    const toLatLng = (pt) => [pt[1], pt[0]];

                    if (type === "Point" && Array.isArray(coords) && coords.length > 0) {
                        const c = coords[0];
                        layer = L.marker(toLatLng(c)).addTo(map);
                    }
                    else if (type === "Circle" && Array.isArray(coords) && coords.length > 0) {
                        const c = coords[0];
                        const radius = geom.radius || 100;
                        layer = L.circle(toLatLng(c), { radius: radius }).addTo(map);
                    }
                    else if ((type === "Rectangle" || type === "Polygon") &&
                        Array.isArray(coords) && coords.length > 0) {
                        const ring = coords[0];
                        const latLngs = ring.map(toLatLng);
                        layer = L.polygon(latLngs).addTo(map);
                    }
                    else if (type === "LineString" && Array.isArray(coords) && coords.length > 1) {
                        const latLngs = coords.map(toLatLng);
                        layer = L.polyline(latLngs).addTo(map);
                    }
                } catch (err) {
                    console.warn("Failed to parse Geometry JSON:", err);
                }
            }

            if (!layer) {
                layer = L.marker([lat, lng]).addTo(map);
            }

            if (layer.getBounds) {
                try {
                    map.fitBounds(layer.getBounds(), { padding: [20, 20] });
                } catch { }
            } else {
                map.setView([lat, lng], 14);
            }

            mapEl.addEventListener('click', function () {
                mapEl.classList.toggle('expanded');
                setTimeout(() => map.invalidateSize(), 260);
            }, { passive: true });

            setTimeout(() => map.invalidateSize(), 120);
        }

        loadLeaflet(initMap);
    });
})();
