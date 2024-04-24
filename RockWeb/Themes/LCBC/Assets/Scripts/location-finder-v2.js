var markers = [];
var map = null;
var currentInfoWindow  = null; // keep track of the info window that is currently open
var storedLat = null;
var storedLng = null;
var activeMarker = null;
var activeMarkerType = null;

$(function() {
    var swiper = new Swiper(".campus-swiper", {
      lazy: true,
      loop: true,
      pagination: {
        el: ".swiper-pagination",
        clickable: true
      },
      navigation: {
        nextEl: ".swiper-button-next",
        prevEl: ".swiper-button-prev",
      },
    });

  
    $('.js-close-filters').click(function(e) {
      e.preventDefault();
      $('.location-filter').addClass('d-none');
    });

    $('.js-open-filters').click(function(e) {
        e.preventDefault();
        $('.location-filter').removeClass('d-none');
    });
});

// window.addEventListener('load', function () {
//   google.maps.event.addDomListener(window, 'load', initMap);
// });

function initMap () {
        console.log('initMap');
        var locationMap = document.getElementById('location-finder-map');
        var locationList = document.getElementById('location-list');
        var bounds = new google.maps.LatLngBounds(); 
        var mapOptions = {
            scrollwheel: true,
            draggable: true,
            mapTypeId: 'roadmap',
            zoomControl: true,
            mapTypeControl: false,
            gestureHandling: 'cooperative',
            streetViewControl: false,
            fullscreenControl: false,
            keyboardShortcuts: false,
            minZoom: 7,
            styles: [
                {
                    "featureType": "all",
                    "elementType": "labels.text",
                    "stylers": [
                        {
                            "color": "#878787"
                        }
                    ]
                },
                {
                    "featureType": "all",
                    "elementType": "labels.text.stroke",
                    "stylers": [
                        {
                            "visibility": "off"
                        }
                    ]
                },
                {
                    "featureType": "administrative.country",
                    "elementType": "all",
                    "stylers": [
                        {
                            "visibility": "off"
                        }
                    ]
                },
                {
                    "featureType": "administrative.province",
                    "elementType": "all",
                    "stylers": [
                        {
                            "visibility": "off"
                        }
                    ]
                },
                {
                    "featureType": "administrative.locality",
                    "elementType": "all",
                    "stylers": [
                        {
                            "visibility": "simplified"
                        }
                    ]
                },
                {
                    "featureType": "administrative.neighborhood",
                    "elementType": "all",
                    "stylers": [
                        {
                            "visibility": "off"
                        }
                    ]
                },
                {
                    "featureType": "administrative.land_parcel",
                    "elementType": "all",
                    "stylers": [
                        {
                            "visibility": "off"
                        }
                    ]
                },
                {
                    "featureType": "landscape",
                    "elementType": "all",
                    "stylers": [
                        {
                            "color": "#eef9ed"
                        }
                    ]
                },
                {
                    "featureType": "landscape.man_made",
                    "elementType": "all",
                    "stylers": [
                        {
                            "color": "#eef9ed"
                        }
                    ]
                },
                {
                    "featureType": "landscape.natural",
                    "elementType": "all",
                    "stylers": [
                        {
                            "color": "#eef9ed"
                        }
                    ]
                },
                {
                    "featureType": "landscape.natural.landcover",
                    "elementType": "all",
                    "stylers": [
                        {
                            "color": "#eef9ed"
                        }
                    ]
                },
                {
                    "featureType": "landscape.natural.terrain",
                    "elementType": "all",
                    "stylers": [
                        {
                            "color": "#eef9ed"
                        }
                    ]
                },
                {
                    "featureType": "poi",
                    "elementType": "labels",
                    "stylers": [
                        {
                            "visibility": "off"
                        }
                    ]
                },
                {
                    "featureType": "poi.attraction",
                    "elementType": "all",
                    "stylers": [
                        {
                            "visibility": "off"
                        }
                    ]
                },
                {
                    "featureType": "poi.business",
                    "elementType": "all",
                    "stylers": [
                        {
                            "visibility": "off"
                        }
                    ]
                },
                {
                    "featureType": "poi.government",
                    "elementType": "all",
                    "stylers": [
                        {
                            "visibility": "off"
                        }
                    ]
                },
                {
                    "featureType": "poi.medical",
                    "elementType": "all",
                    "stylers": [
                        {
                            "visibility": "off"
                        }
                    ]
                },
                {
                    "featureType": "poi.park",
                    "elementType": "all",
                    "stylers": [
                        {
                            "visibility": "off"
                        }
                    ]
                },
                {
                    "featureType": "poi.park",
                    "elementType": "geometry.fill",
                    "stylers": [
                        {
                            "visibility": "on"
                        }
                    ]
                },
                {
                    "featureType": "poi.park",
                    "elementType": "labels.text",
                    "stylers": [
                        {
                            "visibility": "on"
                        }
                    ]
                },
                {
                    "featureType": "poi.park",
                    "elementType": "labels.text.stroke",
                    "stylers": [
                        {
                            "visibility": "off"
                        }
                    ]
                },
                {
                    "featureType": "poi.place_of_worship",
                    "elementType": "all",
                    "stylers": [
                        {
                            "visibility": "off"
                        }
                    ]
                },
                {
                    "featureType": "poi.school",
                    "elementType": "all",
                    "stylers": [
                        {
                            "visibility": "off"
                        }
                    ]
                },
                {
                    "featureType": "poi.sports_complex",
                    "elementType": "all",
                    "stylers": [
                        {
                            "visibility": "off"
                        }
                    ]
                },
                {
                    "featureType": "road.highway",
                    "elementType": "all",
                    "stylers": [
                        {
                            "color": "#dff1dd"
                        }
                    ]
                },
                {
                    "featureType": "road.highway",
                    "elementType": "geometry.fill",
                    "stylers": [
                        {
                            "visibility": "on"
                        }
                    ]
                },
                {
                    "featureType": "road.highway",
                    "elementType": "geometry.stroke",
                    "stylers": [
                        {
                            "color": "#c9c9c9"
                        }
                    ]
                },
                {
                    "featureType": "road.highway",
                    "elementType": "labels",
                    "stylers": [
                        {
                            "visibility": "off"
                        }
                    ]
                },
                {
                    "featureType": "road.highway",
                    "elementType": "labels.text",
                    "stylers": [
                        {
                            "visibility": "on"
                        }
                    ]
                },
                {
                    "featureType": "road.highway.controlled_access",
                    "elementType": "labels",
                    "stylers": [
                        {
                            "visibility": "off"
                        }
                    ]
                },
                {
                    "featureType": "road.arterial",
                    "elementType": "geometry.fill",
                    "stylers": [
                        {
                            "visibility": "on"
                        },
                        {
                            "color": "#ffffff"
                        }
                    ]
                },
                {
                    "featureType": "road.arterial",
                    "elementType": "labels",
                    "stylers": [
                        {
                            "visibility": "off"
                        }
                    ]
                },
                {
                    "featureType": "road.arterial",
                    "elementType": "labels.text.fill",
                    "stylers": [
                        {
                            "visibility": "on"
                        }
                    ]
                },
                {
                    "featureType": "road.local",
                    "elementType": "all",
                    "stylers": [
                        {
                            "visibility": "off"
                        }
                    ]
                },
                {
                    "featureType": "road.local",
                    "elementType": "geometry.fill",
                    "stylers": [
                        {
                            "visibility": "on"
                        },
                        {
                            "color": "#ffffff"
                        }
                    ]
                },
                {
                    "featureType": "road.local",
                    "elementType": "labels.text",
                    "stylers": [
                        {
                            "visibility": "on"
                        }
                    ]
                },
                {
                    "featureType": "road.local",
                    "elementType": "labels.text.fill",
                    "stylers": [
                        {
                            "visibility": "on"
                        }
                    ]
                },
                {
                    "featureType": "road.local",
                    "elementType": "labels.text.stroke",
                    "stylers": [
                        {
                            "visibility": "off"
                        }
                    ]
                },
                {
                    "featureType": "transit",
                    "elementType": "all",
                    "stylers": [
                        {
                            "visibility": "off"
                        }
                    ]
                },
                {
                    "featureType": "transit.station",
                    "elementType": "all",
                    "stylers": [
                        {
                            "visibility": "off"
                        }
                    ]
                },
                {
                    "featureType": "water",
                    "elementType": "all",
                    "stylers": [
                        {
                            "color": "#aee0f4"
                        }
                    ]
                }
            ],
        }

        var map = new google.maps.Map(locationMap, mapOptions);
        map.fitBounds(bounds);

        // get map markers from .location-list-items .card-campus and add to map
        
        var infoWindows = [];
        var campusCards = document.querySelectorAll('.location-list-items .card-campus');
        campusCards.forEach(function (campusCard) {
            var lat = campusCard.dataset.lat;
            var lng = campusCard.dataset.lng;
            var title = campusCard.dataset.title;
            var type = campusCard.dataset.type;
            var address = campusCard.dataset.address1;
            var address2 = campusCard.dataset.address2;
            var phone = campusCard.dataset.phone;
            var email = campusCard.dataset.email;
            
            if (lat && lng) {
              var content = `
              <div class="card card-map">
                  <div class="card-body d-flex flex-row align-items-start">
                      ${type === 'Community Gatherings' ? `<svg class="icon campus-icon flex-shrink-0 mr-3 d-none d-sm-block"><use xlink:href="#communityNew" /></svg>` : `<svg class="icon campus-icon flex-shrink-0 mr-3 d-none d-sm-block"><use xlink:href="#campusNew" /></svg>`}

                      <div>
                          <h3 class="h6 card-title mb-2">${title}</h3>
                          <div class="card-text">
                              ${address}<br>
                              ${address2}
                              ${phone ? `<br><br><a href="tel:${phone}" class="outline-0">${phone}</a>` : ''}
                          </div>
                      </div>
                  </div>
                  <div class="card-footer">
                      <a href="/locations/${campusCard.dataset.url}" class="btn btn-primary btn-sm"><span class="icon-alignment">
                          <svg class="icon"><use xlink:href="#info" /></svg> Details
                      </span></a>

                      ${email ? `<a href="mailto:${email}" class="btn btn-action btn-sm"><span class="icon-alignment">
                          <svg class="icon"><use xlink:href="#envelope" /></svg> Send Email
                      </span></a>` : ''}
                      
                      <a href="https://www.google.com/maps/dir/?api=1&destination=${encodeURIComponent(title)}+${encodeURIComponent(address)}+${encodeURIComponent(address2)}" target="_blank" class="btn btn-action btn-sm">
                        <i class="fa fa-directions"></i> Directions
                      </a>
                  </div>
              </div>
              `;


              var marker = new google.maps.Marker({
                  id: campusCard.getAttribute('data-id'),
                  position: new google.maps.LatLng(lat, lng),
                  map: map,
                  title: title,
                  icon: {
                      // if campus type is campus, show icon
                      url: type === 'Community Gatherings' ? '/Themes/LCBC/Assets/Images/Icons/map-marker-gathering.svg' : '/Themes/LCBC/Assets/Images/Icons/map-marker.svg',
                      scaledSize: new google.maps.Size(30, 30),
                      anchor: new google.maps.Point(15, 30)
                  }
              });

              var infoWindow = new google.maps.InfoWindow({
                  content: content
              });
              
              marker.addListener('click', function () {
                  if (currentInfoWindow) {
                      currentInfoWindow.close();
                  }

                  infoWindow.open(map, marker);
                  // center map on marker with transition
                  map.panTo(marker.getPosition());

                  // setMarkerActive(marker, type);

                  currentInfoWindow = infoWindow;
                  locationList.scrollTop = campusCard.offsetTop - 10;
                  
                    // Check if there is an active marker
                    if (activeMarker) {
                        // Set the active marker back to its original state
                        setActiveMarkerState(activeMarker, activeMarkerType, false);
                    }
                    // Set the clicked marker as active
                    setActiveMarkerState(marker, type, true);
                    // Set the clicked marker as the active marker
                    activeMarker = marker;
                    activeMarkerType = type;
              });


              markers.push(marker);
              infoWindows.push(infoWindow);

              // if click on js-map-view button, center map on marker
              // var mapViewButton = campusCard.querySelector('.js-map-view');
              campusCard.addEventListener('click', function (e) {
                  // if clicked on card and not on button, center map on marker
                  if (e.target.classList.contains('btn') || e.target.classList.contains('abbr')) {
                      return;
                  }

                  map.panTo(marker.getPosition());
                  map.setZoom(15);
                  
                  if (currentInfoWindow) {
                      currentInfoWindow.close(); // close currently open info window
                  }
                  infoWindow.open(map, marker);
                  currentInfoWindow = infoWindow; // set current info window to newly opened one
                  
                    // Check if there is an active marker
                    if (activeMarker) {
                        // Set the active marker back to its original state
                        setActiveMarkerState(activeMarker, activeMarkerType, false);
                    }
                    // Set the clicked marker as active
                    setActiveMarkerState(marker, type, true);
                    // Set the clicked marker as the active marker
                    activeMarker = marker;
                    activeMarkerType = type;
              });
            }
        });

        // Redraw map to fit all markers
        var bounds = new google.maps.LatLngBounds();
        for (var i = 0; i < markers.length; i++) {
            bounds.extend(markers[i].getPosition());
        }
        map.fitBounds(bounds);


        window.addEventListener('resize', function () {
            if (map) {
                google.maps.event.trigger(map, 'resize');
                if (currentInfoWindow) {
                    currentInfoWindow.close();
                }
                map.fitBounds(bounds);
            }
        });

        // request user's location
        if (navigator.geolocation) {
          // if user allows location, reorder campus cards
            navigator.geolocation.getCurrentPosition(function (position) {
                storedLat = position.coords.latitude;
                storedLng = position.coords.longitude;
                document.body.classList.add('has-geolocation');
                reorderCampusCards(position.coords.latitude, position.coords.longitude);
            });
        }

}

// Function to set marker state (active or default)
function setActiveMarkerState(marker, type, isActive) {
    // Determine the appropriate icon URL based on the marker type and state
    var iconUrl = isActive ? '/Themes/LCBC/Assets/Images/Icons/map-marker-active.svg' : (type === 'Community Gatherings' ? '/Themes/LCBC/Assets/Images/Icons/map-marker-gathering.svg' : '/Themes/LCBC/Assets/Images/Icons/map-marker.svg');
    
    // Set the icon
    marker.setIcon({
        url: iconUrl,
        scaledSize: new google.maps.Size(30, 30),
        anchor: new google.maps.Point(15, 30)
    });
}

// on load request user's location
window.addEventListener('load', function () {


    // if user enters a zip code, reorder campus cards
    var zipCodeInput = document.getElementById('zip-input');
    zipCodeInput.addEventListener('change', function () {
        // if zip code is 5 digits, reorder campus cards
        if (zipCodeInput.value.length === 5) {
            getZipAndReorder(zipCodeInput.value);
        }
    });

    // capture enter key on zip code input
    zipCodeInput.addEventListener('keypress', function (e) {
        if (e.keyCode === 13) {
            e.preventDefault();
            if (zipCodeInput.value.length === 5) {
                getZipAndReorder(zipCodeInput.value);
            }
        }
    });

    // if user clicks on a type-filter, set filter and reorder campus cards
    var typeFilters = document.querySelectorAll('.js-type-filter');
    typeFilters.forEach(function (typeFilter) {
        typeFilter.addEventListener('click', function () {
            var type = typeFilter.getAttribute('data-type');
            // if parent element of typeFilter has active class remove it
            if (typeFilter.parentElement.classList.contains('active')) {
                typeFilter.parentElement.classList.remove('active');
                reorderCampusCards(storedLat, storedLng, null);
                return;
            }

            var typeFilters = document.querySelectorAll('.js-type-filter');
            typeFilters.forEach(function (typeFilter) {
                typeFilter.parentElement.classList.remove('active');
            });
            // add active class to parent element of typeFilter
            typeFilter.parentElement.classList.add('active');
            reorderCampusCards(storedLat, storedLng, type);
        });
    });

    // if user clicks .js-clear-filters, remove active class from all type filters
    var clearFilters = document.querySelector('.js-clear-filters');
    clearFilters.addEventListener('click', function () {
        var typeFilters = document.querySelectorAll('.js-type-filter');
        typeFilters.forEach(function (typeFilter) {
            typeFilter.parentElement.classList.remove('active');
        });
        // add class d-none to .js-clear-filters
        clearFilters.classList.add('d-none');

        reorderCampusCards(storedLat, storedLng, null);
    });

});

// on resize, redraw map

function getZipAndReorder (zipCode) {
    var url = `https://maps.googleapis.com/maps/api/geocode/json?components=postal_code:${zipCode}|country:US&key=${googleApiKey}`;
    fetch(url)
        .then(function (response) {
            return response.json();
        })
        .then(function (data) {
            // if data status is OK, reorder campus cards
            if (data.status === 'OK') {
                var lat = data.results[0].geometry.location.lat;
                var lng = data.results[0].geometry.location.lng;
                storedLat = lat;
                storedLng = lng;

                reorderCampusCards(lat, lng, null);
            }
        });

    if (map) {
        // if no info window is open, redraw map to fit all markers
        if (currentInfoWindow) {
            currentInfoWindow.close();
        }
        map.fitBounds(bounds);
    }
}

function getDistanceFromLatLonInMiles (lat1, lon1, lat2, lon2) {
    var R = 3958.8; // Radius of the earth in miles
    var dLat = deg2rad(lat2 - lat1);  // deg2rad below
    var dLon = deg2rad(lon2 - lon1);
    var a =
        Math.sin(dLat / 2) * Math.sin(dLat / 2) +
        Math.cos(deg2rad(lat1)) * Math.cos(deg2rad(lat2)) *
        Math.sin(dLon / 2) * Math.sin(dLon / 2)
        ;
    var c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    var d = R * c; // Distance in miles
    return d;
}

function deg2rad (deg) {
    return deg * (Math.PI / 180)
}


// reorder campus cards based on distance from user
function reorderCampusCards(userLat, userLng, type) {
    // scroll the locationList to the top
    var locationList = document.querySelector('.location-list');
    locationList.scrollTop = 0;

    var campusCards = document.querySelectorAll('.location-list-items .card-campus');
    var campusCardsArray = Array.prototype.slice.call(campusCards);
    campusCardsArray.sort(function (a, b) {
        var aLat = a.dataset.lat;
        var aLng = a.dataset.lng;
        var bLat = b.dataset.lat;
        var bLng = b.dataset.lng;
        var aDistance = getDistanceFromLatLonInMiles(userLat, userLng, aLat, aLng);
        var bDistance = getDistanceFromLatLonInMiles(userLat, userLng, bLat, bLng);
        return aDistance - bDistance;
    });

    // if first campus card is more than 25 miles away, put the online campus card first
    var firstCampusCard = campusCardsArray[0];
    var firstCampusCardDistance = getDistanceFromLatLonInMiles(userLat, userLng, firstCampusCard.dataset.lat, firstCampusCard.dataset.lng);
    var onlineCampusCard = campusCardsArray.find(function (campusCard) {
      return campusCard.dataset.type === 'Online';
    });
    var onlineCampusCardIndex = campusCardsArray.indexOf(onlineCampusCard);
    campusCardsArray.splice(onlineCampusCardIndex, 1);
    if (firstCampusCardDistance >= 25) {
        campusCardsArray.unshift(onlineCampusCard);
    } else {
        campusCardsArray.push(onlineCampusCard);
    }

    // remove all campus cards from DOM
    campusCardsArray.forEach(function (campusCard) {
        campusCard.remove();
    });

    // add campus cards back to DOM in sorted order
    campusCardsArray.forEach(function (campusCard) {

        // if type filter is set, only add campus cards that match type

        document.querySelector('.location-list-items').appendChild(campusCard);
        // update map-distance element
        var lat = campusCard.dataset.lat;
        var lng = campusCard.dataset.lng;

        // if lat and lng are null, hide map-distance element
        if (!lat || !lng) {
          return;
      }

        var marker = markers.find(function (marker) {
            return marker.id === campusCard.getAttribute('data-id');
        });

        var openFiltersButton = document.querySelector('.js-open-filters');
        var clearFiltersButton = document.querySelector('.js-clear-filters');
        if (type) {
            openFiltersButton.classList.add('text-primary');
            clearFiltersButton.classList.remove('d-none');

            var campusType = campusCard.getAttribute('data-type');
            if (campusType !== type) {
                campusCard.classList.add('d-none');
                marker.setVisible(false);
            } else {
                campusCard.classList.remove('d-none');
                marker.setVisible(true);
            }
        } else {
            campusCard.classList.remove('d-none');
            clearFiltersButton.classList.add('d-none');
            openFiltersButton.classList.remove('text-primary');
            marker.setVisible(true);
        }

        var distance = getDistanceFromLatLonInMiles(userLat, userLng, lat, lng);
        // if distance is less than 20 miles, round to nearest tenth
        if (distance < 20) {
            distance = distance.toFixed(1);
        } else {
            distance = Math.round(distance).toLocaleString();
        }

        campusCard.querySelector('.map-distance').innerText = distance + ' miles away';
    });

    // update count of campus cards
    var campusCardCount = document.querySelectorAll('.location-list-items .card-campus:not(.d-none)').length;
    document.querySelector('.js-campus-card-count').innerText = campusCardCount + ' Locations';

    // if type is null, update .js-campus-card-count to show distance to closest campus
    if (!type) {
        var closestCampusCard = document.querySelector('.location-list-items .card-campus:not(.d-none)');
        if (closestCampusCard.dataset.lat && closestCampusCard.dataset.lng) {
          var closestDistance = closestCampusCard.querySelector('.map-distance').innerText;
          document.querySelector('.js-campus-card-count').innerText = `You\u0027re ${closestDistance} from our closest location.`;
        }
    }
}

$(function () {
  // on display of bootstrap tooltip element, hide all other tooltips
  $('[data-toggle="tooltip"]').on('show.bs.tooltip', function () {
    $('[data-toggle="tooltip"]').not(this).tooltip('hide');
  });
});