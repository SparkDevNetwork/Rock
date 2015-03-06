

//
// MAP CAMPUS MODAL
// --------------------------------------------------


// Imports
// -------------------------

// @prepros-prepend "_map.js"


// Map Object
// -------------------------

var campusModalMap = new CCV.infoWindowMap(document.getElementById('campusmodal-holder'))

campusModalMap.draw()

$('document').ready(function(){
  $('body').addClass('modal-open')
})