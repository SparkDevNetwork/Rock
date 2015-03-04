

//
// Map Landing Page
// --------------------------------------------------


// Imports
// -------------------------

// @prepros-prepend "_map.js"


// Map Object
// -------------------------

var landingMap = new CCV.infoWindowMap(document.getElementById('map-holder'))

// Overwritten methods
landingMap.bindUi = function() {
  var _this = this
  $(window).resize(function() {
    _this.fitMarkers()
  })
  $('.js-find-nearest-campus').click(function(){
    _this.findNearestCampus(_this.openInfoWindow, this)
  })
}

// New methods
landingMap.selectCampus = function (campusId) {
  var campus = CCV.findCampusById(campusId)
  alert(campus.name)
}
landingMap.openInfoWindow = function (campus) {
  var marker = this.markers.filter(function (marker) {
    return marker.campusid == campus.id
  })[0]
  this.infowindow.setContent(this.buildInfoWindow(campus))
  this.infowindow.open(this.map, marker)
}
landingMap.findNearestCampus = function (callback, trigger) {
  var _this = this,
      $trigger = $(trigger),
      userLocation,
      service = new google.maps.DistanceMatrixService(),
      result

  if(navigator.geolocation) {
    $trigger.addClass('is-loading')
    navigator.geolocation.getCurrentPosition(function(p) {
      userLocation = new google.maps.LatLng(p.coords.latitude, p.coords.longitude)
      service.getDistanceMatrix(
        {
          origins: [userLocation],
          destinations: _this.allLocationsGeoArray(),
          travelMode: google.maps.TravelMode.DRIVING,
        }, parseResults);
    })
  }

  function parseResults(response, status) {
    if (status == google.maps.DistanceMatrixStatus.OK) {

      var lowest = Number.POSITIVE_INFINITY,
        lowestArrayIndex,
        tmp,
        responseArray = response.rows[0].elements

      for (var i = 0; i < responseArray.length; i++) {
        tmp = responseArray[i].duration.value
        if (tmp < lowest) {
          lowest = tmp
          lowestArrayIndex = i
        }
      }

      $trigger.removeClass('is-loading')
      _this.nearestCampus = CCV.locations[lowestArrayIndex]
      typeof callback === 'function' && callback.call(_this, _this.nearestCampus)
    }
  }
}
landingMap.allLocationsGeoArray = function() {
  var r = []
  for (var i = 0; i < CCV.locations.length; i++) {
    var location = CCV.locations[i].geo
    var geo = new google.maps.LatLng(location.lat,location.lng)
    r.push(geo)
  }
  return r
}

landingMap.draw();