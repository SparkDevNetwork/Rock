

//
// MAP CAMPUS DETAIL PAGE
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

  holder = document.getElementById('campus-detail-page-map')
  campusId = $(holder).attr('data-campus-id')

  window.campusDetailPageMap = new CCV.campusMap(holder, campusId)

  campusDetailPageMap.useScrollZoom = false

  campusDetailPageMap.draw()

})
