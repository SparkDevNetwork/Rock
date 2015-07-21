

//
// MAP CAMPUS MODAL
// --------------------------------------------------


// Imports
// -------------------------

// @prepros-prepend "_map.js"

// Load the Google maps api dynamically to avoid errors when double loading
// The URL string is from https://github.com/SparkDevNetwork/Rock/blob/582e01a72f41ab33ad41335c01c365523b35be5b/Rock/Web/UI/RockPage.cs#L1045
Rock.controls.util.loadGoogleMapsApi('https://maps.googleapis.com/maps/api/js?sensor=true&libraries=drawing')


// Map Object
// -------------------------

$(window).on('googleMapsIsLoaded', function(){

  window.locationsMap = new CCV.campusInfoWindowMapGeo(document.getElementById('locationsmap'))

  locationsMap.selectCampus = function (campusId) {
    alert(campusId)
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

  // locationsMap.useScrollZoom = false;
  // locationsMap.usePanControl = false;

  locationsMap.mapOptions = {
    mapTypeId: 'CCV',
    scrollwheel: false,
    panControl: false,
    zoomControl: false,
    // zoomControl: true,
    // zoomControlOptions: {
    //   style: google.maps.ZoomControlStyle.SMALL,
    //   position: google.maps.ControlPosition.TOP_RIGHT
    // },
    streetViewControl: false,
    mapTypeControl: false
  }

  locationsMap.draw()


  // Find nearest button

  function FindNearest(controlDiv, map) {
    var controlUI = document.createElement('div');
    controlUI.className = "btn btn-default btn-primary"
    controlUI.title = 'Click to find the nearest campus to you';
    controlUI.innerHTML = '<i class="fa fa-fw fa-location-arrow"></i> Find Nearest Campus';
    controlDiv.appendChild(controlUI);

    google.maps.event.addDomListener(controlUI, 'click', function() {
      locationsMap.findNearestCampus(locationsMap.openInfoWindow, this)
    });
  }
  var findNearestDiv = document.createElement('div');
  findNearestDiv.style.marginTop = '2.5%';
  findNearestDiv.style.marginLeft = '2.5%';
  var findNearest = new FindNearest(findNearestDiv, locationsMap.map);

  findNearestDiv.index = 1;
  locationsMap.map.controls[google.maps.ControlPosition.TOP_LEFT].push(findNearestDiv);


  // Custom Zoom buttons

  function CustomZoom(controlDiv, map) {
    var zoomIn = document.createElement('div');
    zoomIn.className = "btn btn-default btn-md"
    zoomIn.title = 'Zoom In';
    zoomIn.innerHTML = '<i class="fa fa-fw fa-plus"></i>';
    controlDiv.appendChild(zoomIn);

    google.maps.event.addDomListener(zoomIn, 'click', function() {
      locationsMap.map.setZoom(locationsMap.map.getZoom() + 1);
    });

    var zoomOut = document.createElement('div');
    zoomOut.className = "btn btn-default btn-md"
    zoomOut.title = 'Zoom Out';
    zoomOut.innerHTML = '<i class="fa fa-fw fa-minus"></i>';
    controlDiv.appendChild(zoomOut);

    google.maps.event.addDomListener(zoomOut, 'click', function() {
      locationsMap.map.setZoom(locationsMap.map.getZoom() - 1);
    });
  }
  var customZoomDiv = document.createElement('div');
  customZoomDiv.className = "btn-group-vertical"
  customZoomDiv.style.marginTop = '2.5%';
  customZoomDiv.style.marginRight = '2.5%';
  var customZoom = new CustomZoom(customZoomDiv, locationsMap.map);

  customZoomDiv.index = 1;
  locationsMap.map.controls[google.maps.ControlPosition.RIGHT_TOP].push(customZoomDiv);

})
