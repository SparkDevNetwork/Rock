

//
// MAP CAMPUS MODAL
// --------------------------------------------------


// Imports
// -------------------------

// @prepros-prepend "_map.js"

// Load the Google maps api dynamically to avoid errors when double loading
// The URL string is from https://github.com/SparkDevNetwork/Rock/blob/582e01a72f41ab33ad41335c01c365523b35be5b/Rock/Web/UI/RockPage.cs#L1045
Rock.controls.util.loadGoogleMapsApi('https://maps.googleapis.com/maps/api/js?sensor=false&libraries=drawing')


// Map Object
// -------------------------

$(window).on('googleMapsIsLoaded', function(){

  window.locationsMap = new CCV.campusInfoWindowMapGeo(document.getElementById('locationsmap'))

  locationsMap.selectCampus = function (campusId) {
    alert(campusId)
  }

  locationsMap.bindUi = function() {
    var _this = this
    $(window).resize(function() {
      _this.fitMarkers()
    })
    $('.js-find-nearest').click(function(){
      _this.findNearestCampus(_this.openInfoWindow, this)
    })
  }

  locationsMap.useScrollZoom = false;
  locationsMap.usePanControl = false;

  locationsMap.draw()


})