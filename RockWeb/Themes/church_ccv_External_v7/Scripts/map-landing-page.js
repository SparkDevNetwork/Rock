

//
// MAP LANDING PAGE
// --------------------------------------------------


// Imports
// -------------------------

// @prepros-prepend "_map.js"


// Map Object
// -------------------------

var landingMap = new CCV.campusInfoWindowMapGeo(document.getElementById('map-holder'))

// Custom properties
// landingMap.zoom = 11
// landingMap.useZoom = true

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
  Rock.utility.setContext('campuses', campusId)

  // wait until API adds the cookie
  $(document).ajaxComplete(function( event, xhr, settings ) {
    if (settings.url.indexOf('/api/campuses/SetContext/') > -1) {
      // using location.replace removes the current page from history
      // .homePageRoute is set in LandingMap.aspx
      if (CCV.homePageRoute)
        window.location.replace('/'+CCV.homePageRoute)
    }
  })
}
landingMap.openInfoWindow = function (campus) {
  var marker = this.markers.filter(function (marker) {
    return marker.campusid == campus.id
  })[0]
  this.infowindow.setContent(this.buildInfoWindow(campus))
  this.infowindow.open(this.map, marker)
}

landingMap.draw();