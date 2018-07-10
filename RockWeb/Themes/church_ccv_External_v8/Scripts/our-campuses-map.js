

//
// MAP CAMPUS MODAL
// --------------------------------------------------


// Imports
// -------------------------

// @prepros-prepend "_map.js"

// Load the Google maps api dynamically to avoid errors when double loading
// The URL string is from https://github.com/SparkDevNetwork/Rock/blob/582e01a72f41ab33ad41335c01c365523b35be5b/Rock/Web/UI/RockPage.cs#L1045
Rock.controls.util.loadGoogleMapsApi('https://maps.googleapis.com/maps/api/js?sensor=false&libraries=drawing&key=' + googleApiKey)


// Modal Functionality
// -------------------------

window.CCV = window.CCV || {}

CCV.showMapCampusModal = function() {
  if (!CCV.campusModalMapHasBeenDrawn) {
    window.campusModalMap = new CCV.campusInfoWindowMap(document.getElementById('our-campuses-map'))



    options = {
      mapTypeId: 'CCV',
      
      panControl: this.usePanControl,
      zoomControl: true,
      zoomControlOptions: {
        style: google.maps.ZoomControlStyle.SMALL,
        position: google.maps.ControlPosition.TOP_RIGHT
      },
      streetViewControl: false,
      mapTypeControl: false
    }
    window.campusModalMap.mapOptions = options;

    campusModalMap.draw()
    CCV.campusModalMapHasBeenDrawn = true
  }
}

$(window).on('googleMapsIsLoaded', function(){
   CCV.showMapCampusModal();
});