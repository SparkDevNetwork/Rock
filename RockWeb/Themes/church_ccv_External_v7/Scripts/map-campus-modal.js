

//
// MAP CAMPUS MODAL
// --------------------------------------------------


// Imports
// -------------------------

// @prepros-prepend "_map.js"


// Map Object
// -------------------------

var campusModalMap = new CCV.infoWindowMap(document.getElementById('campusmodal-holder'))

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