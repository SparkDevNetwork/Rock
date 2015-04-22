(function(){

  window.CCV = window.CCV || {}

  function loadMap() {

    // Styles
    CCV.mapStyles = [{
      "stylers": [{
        "visibility": "simplified"
      }, {
        "saturation": -100
        }]
      }, {
        "featureType": "road.arterial",
          "elementType": "labels",
          "stylers": [{
          "visibility": "on"
        }, {
          "gamma": 3.05
        }]
      }, {
        "featureType": "poi",
          "stylers": [{
          "visibility": "off"
        }]
      }, {
        "featureType": "transit",
          "stylers": [{
          "visibility": "off"
        }]
      }, {
        "featureType": "administrative.country",
          "stylers": [{
          "visibility": "off"
        }]
      }, {
        "featureType": "administrative.locality",
          "stylers": [{
          "visibility": "off"
        }]
      }, {
        "featureType": "administrative.neighborhood",
          "stylers": [{
          "visibility": "off"
        }]
      }, {
        "featureType": "water",
          "stylers": [{
          "visibility": "off"
        }]
      },
    {}];

    // MapType used during map init
    CCV.mapType = new google.maps.StyledMapType(CCV.mapStyles);

    // Marker Styles
    CCV.markerFile = (window.devicePixelRatio > 1.5) ? 'http://www.ccvonline.com/Arena/Content/v6/img/maps/marker@2x.png' : 'http://www.ccvonline.com/Arena/Content/v6/img/maps/marker.png';
    CCV.marker = new google.maps.MarkerImage(CCV.markerFile, null, null, null, new google.maps.Size(30,38));



    // Begin map object

    CCV.baseMap = function (holder, points) {
      this.holder = holder
      this.points = points
      this.markers = []
      this.bounds = new google.maps.LatLngBounds()
      this.zoom = this.zoom || 12
      this.useZoom = this.useZoom || false
      this.useScrollZoom = this.useScrollZoom || true
    }
    CCV.baseMap.prototype = {
      draw: function () {
        var options = {}
        if (this.useScrollZoom) {
          options = {
            mapTypeId: 'CCV',
            disableDefaultUI: true
          }
        }
        else {
          options = {
            mapTypeId: 'CCV',
            scrollwheel: false,
            zoomControl: true,
            zoomControlOptions: {
              style: google.maps.ZoomControlStyle.SMALL,
              position: google.maps.ControlPosition.TOP_RIGHT
            },
            streetViewControl: false,
            mapTypeControl: false
          }
        }
        this.mapOptions = this.mapOptions || options

        if (this.holder)
          this.map = new google.maps.Map(this.holder, this.mapOptions)
        else
          throw "Can't find map holder"
        this.map.mapTypes.set('CCV', CCV.mapType)
        this.dropMarkers()
        this.fitMarkers()
        this.bindUi()
      },
      dropMarkers: function () {
        for (var i = 0; i < this.points.length; i++) {
          var point = this.points[i]
          this.dropMarker(point)
        }
      },
      dropMarker: function (point) {
        var marker = new google.maps.Marker({
          position: new google.maps.LatLng(point.lat,point.lng),
          icon: CCV.marker,
          map: this.map,
          title: point.title,
          animation: google.maps.Animation.DROP
        })
        this.markers.push(marker)
        this.bounds.extend(marker.position)
        this.afterDropMarker.call(this, point, marker)
      },
      afterDropMarker: function (point, marker) {
      },
      fitMarkers: function () {
        this.map.setCenter(this.bounds.getCenter())
        if (this.useZoom || this.markers.length == 1)
          this.map.setZoom(this.zoom)
        else
          this.map.fitBounds(this.bounds)
      },
      bindUi: function () {
        var _this = this
        $(window).resize(function() {
          _this.fitMarkers()
        })
      },
      getInstanceName: function () {
        for (var name in window)
          if (window[name] == this)
            return name
      },
    }


    // Campus Map

    // Inherit baseMap and add campus details
    CCV.campusMap = function (holder, campusToDraw) {
      CCV.baseMap.call(this, holder, campusToDraw)
      this.campusToDraw = campusToDraw || 'all'
    }
    CCV.campusMap.prototype = new CCV.baseMap()
    CCV.campusMap.prototype.constructor = CCV.campusMap

    CCV.campusMap.prototype.dropMarkers = function () {
      if (this.campusToDraw == 'all') {
        for (var i = 0; i < CCV.locations.length; i++) {
          var campus = CCV.locations[i]
          this.dropMarker(campus)
        }
      }
      else {
        var campus = CCV.findCampusById(this.campusToDraw)
        this.dropMarker(campus)
        this.useZoom = true
      }
    }
    CCV.campusMap.prototype.dropMarker = function (campus) {
      var marker = new google.maps.Marker({
        position: new google.maps.LatLng(campus.geo.lat,campus.geo.lng),
        icon: CCV.marker,
        map: this.map,
        title: campus.name,
        campusid: campus.id,
        animation: google.maps.Animation.DROP
      })
      this.markers.push(marker)
      this.bounds.extend(marker.position)
      this.afterDropMarker.call(this, campus, marker)
    }


    // Infowindow map

    // Inherit campusMap and add initial infowindow
    CCV.campusInfoWindowMap = function (holder, campusToDraw) {
      CCV.campusMap.call(this, holder, campusToDraw)
      this.infowindow = new google.maps.InfoWindow({ content: 'Loading...' })
    }
    CCV.campusInfoWindowMap.prototype = new CCV.campusMap()
    CCV.campusInfoWindowMap.prototype.constructor = CCV.campusInfoWindowMap

    // Custom & override methods
    CCV.campusInfoWindowMap.prototype.afterDropMarker = function (campus, marker) {
      var _this = this
      google.maps.event.addListener(marker, 'click', function () {
        _this.infowindow.setContent(_this.buildInfoWindow(campus))
        _this.infowindow.open(_this.map, this)
      })
    }
    CCV.campusInfoWindowMap.prototype.buildInfoWindow = function(campus) {
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
        result += '  <a class="select" onclick="'+this.getInstanceName()+'.selectCampus('+campus.id+')">Select this Campus</a>'
      result += '</div>'
      return result
    }
  }

  if (typeof google == 'object' && typeof google.maps == 'object')
    loadMap()
  else {
    $(window).on('googleMapsIsLoaded', function(){
      loadMap()
    })
  }

})();
