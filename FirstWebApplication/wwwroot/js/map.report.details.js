// wwwroot/js/map.report.details.js
(function () {
    function initDetailMap() {
        // Samme id i både Details og RegistrarDetails
        var mapEl = document.getElementById("detailMap");
        var wrap = document.getElementById("mapWrap");

        if (!mapEl || !wrap) return;
        if (typeof L === "undefined") {
            console.error("Leaflet (L) is not loaded");
            return;
        }

        // Lat/Lng kommer fra data-attributter på div-en
        var lat = parseFloat(mapEl.getAttribute("data-lat") || "");
        var lng = parseFloat(mapEl.getAttribute("data-lng") || "");

        if (Number.isNaN(lat) || Number.isNaN(lng)) {
            console.warn("Missing or invalid lat/lng on detailMap");
            return;
        }

        var map = L.map(mapEl, { zoomControl: true }).setView([lat, lng], 14);

        L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
            attribution: "&copy; OpenStreetMap contributors",
            maxZoom: 19
        }).addTo(map);

        // --- Les Geometry fra hidden input (Details) eller <pre> (RegistrarDetails) ---
        var geoStr = "";

        var hidden = document.getElementById("geometryHidden");
        if (hidden && hidden.value && hidden.value !== "null") {
            geoStr = hidden.value.trim();
        }

        var pre = document.getElementById("geometryJson");
        if (!geoStr && pre && pre.textContent) {
            geoStr = pre.textContent.trim();
        }

        var layer = null;

        if (geoStr) {
            try {
                var geom = JSON.parse(geoStr);
                var type = geom.type;
                var coords = geom.coordinates;
                var toLatLng = function (c) { return [c[1], c[0]]; };

                if (type === "Point" && Array.isArray(coords) && coords[0]) {
                    layer = L.marker(toLatLng(coords[0]));
                }
                else if (type === "LineString" && Array.isArray(coords)) {
                    layer = L.polyline(coords.map(toLatLng));
                }
                else if ((type === "Polygon" || type === "Rectangle") &&
                    Array.isArray(coords) && Array.isArray(coords[0])) {
                    var ring = coords[0];
                    layer = L.polygon(ring.map(toLatLng));
                }
                else if (type === "Circle" &&
                    Array.isArray(coords) && coords[0]) {
                    var lngC = coords[0][0];
                    var latC = coords[0][1];
                    var radius = geom.radius || 200;
                    layer = L.circle([latC, lngC], { radius: radius });
                }
            } catch (e) {
                console.warn("Could not parse Geometry JSON in Details:", e);
            }
        }

        // Fallback: hvis ingen figur ble tegnet → vanlig marker på punktet
        if (layer) {
            layer.addTo(map);

            if (typeof layer.getBounds === "function") {
                try {
                    map.fitBounds(layer.getBounds(), { padding: [20, 20] });
                } catch (e) {
                    // hvis getBounds feiler, zoom til punkt
                    if (typeof layer.getLatLng === "function") {
                        map.setView(layer.getLatLng(), 14);
                    }
                }
            } else if (typeof layer.getLatLng === "function") {
                map.setView(layer.getLatLng(), 14);
            }
        } else {
            L.marker([lat, lng]).addTo(map);
        }

        // Klikk for å ekspandere kartet, som i koden din før
        mapEl.addEventListener("click", function () {
            mapEl.classList.toggle("expanded");
            setTimeout(function () { map.invalidateSize(); }, 260);
        }, { passive: true });

        // Fix for at Leaflet skal tegne riktig størrelse
        setTimeout(function () { map.invalidateSize(); }, 120);
    }

    document.addEventListener("DOMContentLoaded", initDetailMap);
})();
