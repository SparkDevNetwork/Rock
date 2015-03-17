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

    CCV.baseMap = function (holder, campusToDraw) {
      this.holder = holder
      this.campusToDraw = campusToDraw || 'all'
      this.markers = []
      this.bounds = new google.maps.LatLngBounds()
      this.zoom = this.zoom || 12
      this.useZoom = this.useZoom || false
      this.mapOptions = this.mapOptions || {
        mapTypeId: 'CCV',
        disableDefaultUI: true
      }
    }
    CCV.baseMap.prototype = {
      draw: function () {
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
      },
      dropMarker: function (campus) {
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
        this.afterDropMarker(this, campus, marker)
      },
      afterDropMarker: function (_this, campus, marker) {
      },
      fitMarkers: function () {
        this.map.setCenter(this.bounds.getCenter())
        if (this.useZoom)
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


    // Infowindow map

    // Inherit baseMap and add initial infowindow
    CCV.infoWindowMap = function (holder, campusToDraw) {
      CCV.baseMap.call(this, holder, campusToDraw)
      this.infowindow = new google.maps.InfoWindow({ content: 'Loading...' })
    }
    CCV.infoWindowMap.prototype = new CCV.baseMap()
    CCV.infoWindowMap.prototype.constructor = CCV.infoWindowMap

    // Custom & override methods
    CCV.infoWindowMap.prototype.afterDropMarker = function (_this, campus, marker) {
      google.maps.event.addListener(marker, 'click', function () {
        _this.infowindow.setContent(_this.buildInfoWindow(campus))
        _this.infowindow.open(_this.map, this)
      })
    }
    CCV.infoWindowMap.prototype.buildInfoWindow = function(campus) {
      var result
      result  = '<div class="infowindow">'
      result += '  <div class="name">'+campus.name+'</div>'
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
