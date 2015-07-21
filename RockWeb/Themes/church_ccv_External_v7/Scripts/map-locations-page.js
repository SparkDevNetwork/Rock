

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

  locationsMap.buildInfoWindow = function(campus) {
    var result
    result  = '<div class="infowindow">'
    result += '  <div class="name">'+campus.name+' Campus</div>'
    result += '  <div class="group">'
    result += '    <img class="photo" src="'+campus.photo+'&width=75" style="width: 75px; height: 75px;">'
    result += '    <div class="details">'
    result += '      <span class="address">'+campus.street+'<br>'+campus.city+', '+campus.state+' '+campus.zip+'</span>'
    result += '      <span class="phone">'+campus.phone+'</span>'
    result += '    </div>'
    result += '  </div>'
    if (typeof this.selectCampus == 'function')
      result += '  <a class="select" href="'+campus.url+'">Learn More</a>'
    result += '</div>'
    return result
  }

  locationsMap.useScrollZoom = false;
  locationsMap.usePanControl = false;

  locationsMap.draw()


})