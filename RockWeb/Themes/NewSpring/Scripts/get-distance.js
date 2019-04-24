document.addEventListener("DOMContentLoaded", function(){

    // Get query string value by key
    var getQueryString = function ( field, url ) {
        var href = url ? url : window.location.href;
        var reg = new RegExp( '[?&]' + field + '=([^&#]*)', 'i' );
        var string = reg.exec(href);
        return string ? string[1] : null;
    };

    // Used for inserting distance indicators on location cards
    function insertAfter(el, referenceNode) {
        referenceNode.parentNode.insertBefore(el, referenceNode.nextSibling);
    }

    function distance(lat1, lon1, lat2, lon2, unit) {
        var radlat1 = Math.PI * lat1/180
        var radlat2 = Math.PI * lat2/180
        var theta = lon1-lon2
        var radtheta = Math.PI * theta/180
        var dist = Math.sin(radlat1) * Math.sin(radlat2) + Math.cos(radlat1) * Math.cos(radlat2) * Math.cos(radtheta);
        if (dist > 1) {
            dist = 1;
        }
        dist = Math.acos(dist)
        dist = dist * 180/Math.PI
        dist = dist * 60 * 1.1515
        if (unit=="K") { dist = dist * 1.609344 }
        if (unit=="N") { dist = dist * 0.8684 }
        return dist
    }

    function reorderLocations(origin) {

        // Define geocoder
        var geocoder = new google.maps.Geocoder();
        var originLatitude;
        var originLongitude;

        // Find and remove all previous distance indicators
        var distanceIndicators = document.querySelectorAll(".display-miles");

        // If distance indicators are present in cards, remove them
        if (distanceIndicators.length > 0) {
            for (var i = 0; i < distanceIndicators.length; i++) {
                distanceIndicators[i].parentNode.removeChild(distanceIndicators[i]);
            }
        }

        // Get lat/lon of origin
        geocoder.geocode( { 'address': origin}, function(results, status) {
            if (status == google.maps.GeocoderStatus.OK) {
                originLatitude = results[0].geometry.location.lat();
                originLongitude = results[0].geometry.location.lng();
            } 
        
            // Loop through locations to get distances
            for (var i = 0; i < locationsCount; i++) {

                // Setup Card
                var locationCard = locations[i];

                // Get lat/lon values from data attribute
                var latlon = locationCard.dataset.latlon;
                latlon = latlon.split(',');
                lat = latlon[0];
                lon = latlon[1];

                // Calculate distance from query location to campus
                var locationDistance = distance(originLatitude, originLongitude, lat, lon, 'M');

                // Round to nearest tenth
                locationDistance = Math.round(10*locationDistance)/10;

                // Send distance back to card in new data attribute
                locationCard.setAttribute('data-distance', locationDistance);

                // Get the card heading so we can add the distance indicator after it
                var locationCardHeading = locationCard.querySelector('h2');

                // Create distance indicator element and add it
                var newElement = document.createElement('p');
                newElement.classList.add('display-miles', 'display-inline-block', 'label', 'label-info', 'push-half-bottom', 'sans-serif', 'circular');
                newElement.innerHTML = locationDistance + ' miles away';
                if(lat && lon) {
                    insertAfter(newElement, locationCardHeading);
                }

            }

            var items = distanceSearchLocations.childNodes;

            var itemsArr = [];

            for (var i in items) {
                if (items[i].nodeType == 1) { // get rid of the whitespace text nodes
                    itemsArr.push(items[i]);
                }
            }

            itemsArr.sort(function(a, b) {
              return a.dataset.distance == b.dataset.distance
                      ? 0
                      : (Number(a.dataset.distance) > Number(b.dataset.distance) ? 1 : -1);
            });

            for (i = 0; i < itemsArr.length; ++i) {
                distanceSearchLocations.appendChild(itemsArr[i]);
            }

            swiper.update();
            swiper.slideTo(0, 50, false);

            // Scroll to locations card slider after sorting
            var anchor = document.querySelector('#locations');

            // Scroll constructor located in _smooth-scroll.js, compiled into main.js
            // scroll.animateScroll(anchor);
        });
    }

    // Get location query string value
    var origin = getQueryString('location');

    // If origin exists as query string, go ahead and reorder locations
    if (origin) {

        // Reorder location cards
        reorderLocations(origin);
    }

    // Get distance finder instances
    var distanceSearchForm = document.querySelector("[data-locations-search]");
    var distanceSearchLocations = document.querySelector("[data-locations]");
    var locations = distanceSearchLocations.children;
    var locationsCount = locations.length;

    // Find a location query search
    distanceSearchForm.addEventListener("submit", function(e){
        e.preventDefault();

        // Get origin location data from form input field
        var origin = distanceSearchForm.querySelector('input').value;

        // Reorder location cards
        reorderLocations(origin);

    });
});