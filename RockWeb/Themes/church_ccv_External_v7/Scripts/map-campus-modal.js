

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

  window.campusModalMap = new CCV.infoWindowMap(document.getElementById('campusmodal-holder'))

  campusModalMap.selectCampus = function (campusId) {
    Rock.utility.setContext('campuses', campusId)

    // wait until API adds the cookie
    $(document).ajaxComplete(function( event, xhr, settings ) {
      if (settings.url.indexOf('/api/campuses/SetContext/') > -1) {
        // reloads page from cache
        location.reload();
      }
    })
  }

  campusModalMap.draw()

  $('document').ready(function(){
    $('body').addClass('modal-open')
  })

})