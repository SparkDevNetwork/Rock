(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.geoPicker = (function () {

        var GeoPicker = function (options) {
            var obj = this;
            obj.controlId = options.controlId;
            obj.restUrl = options.restUrl;
            obj.path = options.path;
            obj.centerAddress = options.centerAddress;                     // used when nothing is on map
            obj.centerLatitude = options.centerLatitude || "33.590795";     // used when nothing is on map
            obj.centerLongitude = options.centerLongitude || "-112.126459"; // used when nothing is on map
            obj.drawingMode = options.drawingMode || "Polygon" || "Point"; // the available modes
            obj.strokeColor = options.strokeColor || "#0088cc";
            obj.fillColor = options.fillColor || "#0088cc";

            // This is used to temporarily store in case of user cancel.
            obj.pathTemp = null;

            // the selected polygon or point
            obj.selectedShape = null;

            // these are used to set the map's viewport boundary
            obj.minLat = null;
            obj.maxLat = null;
            obj.minLng = null;
            obj.maxLng = null;

            // our google Map
            obj.map = null;

            // an instance of the Google Maps drawing manager
            obj.drawingManager = null;

            // An array of styles that controls the look of the Google Map
            // http://gmaps-samples-v3.googlecode.com/svn/trunk/styledmaps/wizard/index.html
            obj.styles = [
              {
                  "stylers": [
                    { "visibility": "simplified" },
                    { "saturation": -100 }
                  ]
              }, {
                  "featureType": "road",
                  "elementType": "labels",
                  "stylers": [
                    { "visibility": "on" }
                  ]
              }, {
                  "featureType": "poi",
                  "stylers": [
                    { "visibility": "off" }
                  ]
              }, {
                  "featureType": "landscape",
                  "stylers": [
                    { "visibility": "off" }
                  ]
              }, {
                  "featureType": "administrative.province",
                  "stylers": [
                    { "visibility": "on" }
                  ]
              }, {
                  "featureType": "administrative.locality",
                  "stylers": [
                    { "visibility": "on" }
                  ]
              }
            ];

            /** 
            * Initializes the map viewport boundary coordinates.
            */
            this.initMinMaxLatLng = function () {
                obj.minLat = null;
                obj.maxLat = null;
                obj.minLng = null;
                obj.maxLng = null;
            }

            /**
            * Unselects the selected shape (if selected) and disables the delete button.
            */
            this.clearSelection = function () {
                if (obj.selectedShape) {
                    if (obj.drawingMode == "Polygon") {
                        obj.selectedShape.setEditable(false);
                    }
                    obj.selectedShape = null;
                }
                $('#gmnoprint-delete-button_' + obj.controlId).attr('disabled', '')
                $('#gmnoprint-delete-button_' + obj.controlId + ' .icon-remove').css("color", "#aaa");
            }

            /**
            * Stores the given shape's path points into the path property as a
            * pipe (|) delimited list of lat,long coordinates.  This method also
            * re-enables the delete button so that the polygon can be deleted.
            */
            this.setSelection = function (shape, type) {
                obj.clearSelection();

                // enable delete button
                var $deleteButton = $('#gmnoprint-delete-button_' + obj.controlId);
                $('#gmnoprint-delete-button_' + obj.controlId).removeAttr('disabled');
                $('#gmnoprint-delete-button_' + obj.controlId + ' .icon-remove').css("color", "");

                obj.selectedShape = shape;
                
                if (type == "polygon") {
                    shape.setEditable(true);
                    var coordinates = new Array();
                    var vertices = shape.getPath();
                    // Iterate over the vertices of the shape's path
                    for (var i = 0; i < vertices.length; i++) {
                        var xy = vertices.getAt(i);
                        coordinates[i] = xy.toUrlValue();
                    }

                    // if the last vertex is not already the first, then
                    // add the first vertex to the end of the path.
                    if (vertices.getAt(0).toUrlValue() != coordinates[coordinates.length - 1]) {
                        obj.path = coordinates.join('|') + '|' + vertices.getAt(0).toUrlValue();
                    }
                }
                else if (type == "marker") {
                    obj.path = shape.getPosition().toUrlValue();
                }
            }

            /**
            * Delete the selected shape and enable the drawing controls
            * if they were deleted.  Also removes the polygon from the hidden variable.
            */
            this.deleteSelectedShape = function () {
                if (obj.selectedShape && confirm("Delete selected shape?")) {
                    obj.selectedShape.setMap(null);
                    obj.clearSelection();

                    // delete the path
                    obj.path = null;

                    // enable the drawing controls again
                    obj.drawingManager.setOptions({
                        drawingControlOptions: {
                            drawingModes: obj.getDrawingModes()
                        }
                    });
                }
            }

            /**
            * Returns the appropriate mode array for use with the map's DrawingManager
            * drawing control options.
            */
            this.getDrawingModes = function getDrawingModes() {

                if (obj.drawingMode == "Polygon") {
                    return [google.maps.drawing.OverlayType.POLYGON];
                }
                else if (obj.drawingMode == "Point") {
                    return [google.maps.drawing.OverlayType.MARKER];
                }
            }

            /**
            * Finds the point/polygon boundary and sets the map viewport to fit
            */
            this.fitBounds = function () {
                if (! obj.path) {
                    // if no path, then set the center using the options
                    var newLatLng = new google.maps.LatLng(
                        parseFloat(obj.centerLatitude),
                        parseFloat(obj.centerLongitude));
                    obj.map.setCenter(newLatLng);
                    return;
                }

                var coords = obj.path.split('|');
                var pathArray = new Array();
                // find the most southWest and northEast points of the path.
                for (var i = 0; i < coords.length ; i++) {
                    var latLng = coords[i].split(',');
                    var lat = parseFloat(latLng[0]);
                    var lng = parseFloat(latLng[1]);
                    // find the most southWest and northEast points of the path.
                    obj.findBounds(lat, lng);
                    pathArray.push(new google.maps.LatLng(lat, lng));
                }

                // Set the viewport to contain the given bounds.
                var southWest = new google.maps.LatLng(obj.minLat, obj.minLng);
                var northEast = new google.maps.LatLng(obj.maxLat, obj.maxLng);
                var bounds = new google.maps.LatLngBounds(southWest, northEast);
                obj.map.fitBounds(bounds);

                if (obj.drawingMode == "Point") {
                    obj.map.setZoom(16);
                }
            }

            /**
            * Utility method to determine the most SouthWestern
            * and NorthEastern lat and long.
            */
            this.findBounds = function (lat, lng) {
                if (!obj.minLat || lat < obj.minLat) {
                    obj.minLat = lat;
                }
                if (!obj.maxLat || lat > obj.maxLat) {
                    obj.maxLat = lat;
                }
                if (!obj.minLng || lng < obj.minLng) {
                    obj.minLng = lng;
                }
                if (!obj.maxLng || lng > obj.maxLng) {
                    obj.maxLng = lng;
                }
                //console.log( 'min/max lat: ' + obj.minLat + '/' + obj.maxLat + ' min/max long: ' + obj.minLng + '/' + obj.minLng );
            }

            /**
            * Disable the drawing manager so they cannot add anything to the map.
            */
            this.disableDrawingManager = function () {
                // Switch back to non-drawing mode after drawing a shape.
                if (!obj.drawingManager) {
                    return;
                }

                obj.drawingManager.setDrawingMode(null);

                // disable the drawing controls so we only get one polygon
                // and we'll add it back on deleting the existing polygon.
                obj.drawingManager.setOptions({
                    drawingControlOptions: {
                        drawingModes: [
                        ]
                    }
                });
            }

            /**
            * Takes the path data stored in the path and plots it on the map.
            */
            this.plotPath = function (map) {
                obj.initMinMaxLatLng();
                // only try this if we have a path
                if (obj.path) {
                    //console.log("found an obj.path to plot: " + obj.path);

                    var coords = obj.path.split('|');
                    var pathArray = new Array();
                    // put the polygon coordinates into a path array
                    for (var i = 0; i < coords.length ; i++) {
                        var latLng = coords[i].split(',');
                        var lat = parseFloat(latLng[0]);
                        var lng = parseFloat(latLng[1]);
                        pathArray.push(new google.maps.LatLng(lat, lng));
                    }

                    // reverse geo code the first point to an address and display.
                    var $label = $('#selectedGeographyLabel_' + this.controlId);
                    obj.toAddress(pathArray[0], $label);

                    if (coords.length > 0) {
                        // Add the starting vertex as the last vertex to the path
                        //var latLng = coords[0].split(',');
                        //var lat = parseFloat(latLng[0]);
                        //var lng = parseFloat(latLng[1]);
                        //pathArray.push(new google.maps.LatLng(lat, lng));

                        var polygon;

                        if (obj.drawingMode == "Polygon") {

                            var polygon = new google.maps.Polygon({
                                path: pathArray,
                                clickable: true,
                                editable: true,
                                strokeColor: obj.strokeColor,
                                fillColor: obj.fillColor,
                                strokeWeight: 2
                            });
                            polygon.setMap(map);

                            // Select the polygon
                            obj.setSelection(polygon, google.maps.drawing.OverlayType.POLYGON);

                            // Disable the drawing manager
                            obj.disableDrawingManager();

                            // add listener for moving polygon points.
                            google.maps.event.addListener(polygon.getPath(), 'set_at', function (e) {
                                obj.setSelection(polygon, google.maps.drawing.OverlayType.POLYGON);
                            });

                            // add listener for adding new points.
                            google.maps.event.addListener(polygon.getPath(), 'insert_at', function (e) {
                                obj.setSelection(polygon, google.maps.drawing.OverlayType.POLYGON);
                            });

                            // add listener for "selecting" the polygon
                            // because that's where we stuff the coordinates into the hidden variable
                            google.maps.event.addListener(polygon, 'click', function () {
                                obj.setSelection(polygon, google.maps.drawing.OverlayType.POLYGON);
                            });
                        }
                        else if (obj.drawingMode == "Point") {

                            var point = new google.maps.Marker({
                                position: pathArray[0],
                                map: map,
                                clickable: true
                            });
                            //point.setMap(map);

                            // Select the point
                            obj.setSelection(point, google.maps.drawing.OverlayType.MARKER);

                            // Disable the drawing manager
                            obj.disableDrawingManager();

                            // add listener for "selecting" the point
                            // because that's where we stuff the coordinates into the hidden variable
                            google.maps.event.addListener(point, 'click', function () {
                                obj.setSelection(point, google.maps.drawing.OverlayType.MARKER);
                            });
                        }
                    }
                }
            }

            /**
            * Take the given lat and long and try to reverse-geocode to an Address
            * then stuff that address as the value of the given element.
            */
            this.toAddress = function (latlng, $labelElement) {
                // only try if we have a valid latlng
                if (!latlng || isNaN(latlng.lat()) || isNaN(latlng.lng()) ) {
                    return;
                }
                var geocoder = new google.maps.Geocoder();
                geocoder = geocoder.geocode({ 'latLng': latlng }, function (results, status) {
                    if (status == google.maps.GeocoderStatus.OK) {
                        if (results[0]) {
                            $labelElement.text('near ' + results[0].formatted_address);
                        }
                    }
                    else
                    {
                        $labelElement.html('<i>selected</i>');
                        console.log('Geocoder failed due to: ' + status);
                    }
                });
            }

            /**
            * Geocode an address to a latLng and center the map on that point.
            */
            this.centerMapOnAddress = function () {
                var self = this;
                // only try if a centerAddress is set
                if (!obj.centerAddress) {
                    return;
                }

                var geocoder = new google.maps.Geocoder();

                geocoder.geocode({ 'address': obj.centerAddress }, function (results, status) {
                    if (status == google.maps.GeocoderStatus.OK) {
                        self.map.setCenter(results[0].geometry.location);
                    } else {
                        console.log('Geocode was not successful for the following reason: ' + status);
                    }
                });
            }

            /**
            * Return a latLng for the "first point" of the selected map.
            */
            this.firstPoint = function () {
                if (obj.path) {
                    var coords = obj.path.split('|');
                    if (coords) {
                        var latLng = coords[0].split(',');
                        var lat = parseFloat(latLng[0]);
                        var lng = parseFloat(latLng[1]);
                        return new google.maps.LatLng(lat, lng);
                    }
                }
            }

        }; // sorta end class

        /**
        * initialize our event handlers for the button clicks we're going to be handling.
        */
        GeoPicker.prototype.initializeEventHandlers = function () {
            var controlId = this.controlId,
                restUrl = this.restUrl;
            var self = this;

            /**
            * Toggle the picker on and off when the control's link is clicked.
            */
            $('#' + controlId + ' a.picker-label').click(function (e) {
                e.preventDefault();
                $('#' + controlId).find('.picker-menu').first().toggle();
                if ($('#' + controlId).find('.picker-menu').first().is(":visible")) {
                    google.maps.event.trigger(self.map, "resize");
                    // now we can safely fit the map to any polygon boundary
                    self.fitBounds();
                }
            });

            /**
            * Handle the Cancel button click by hiding the overlay.
            */
            $('#btnCancel_' + controlId).click(function () {
                $(this).closest('.picker-menu').slideUp();
                self.path = self.pathTemp;
                if ( self.selectedShape ) {
                    self.selectedShape.setMap(null);
                    self.clearSelection();
                }
                self.plotPath(self.map);
            });

            /**
            * Handle the Select button click by stuffing the RockGoogleGeoPicker's path value into the hidden field. 
            */
            $('#btnSelect_' + controlId).click(function () {
                var geoInput = $('#' + controlId).find('input:checked'),
                    selectedValue = self.path,
                    selectedGeographyLabel = $('#selectedGeographyLabel_' + controlId),
                    $hiddenField = $('#hfGeoPath_' + self.controlId);

                //console.log('storing coordinates into hf (' + '#hfGeoPath_' + self.controlId + ') self.path:' + self.path);
                $hiddenField.val(self.path);

                selectedGeographyLabel.val(selectedValue);
                self.toAddress( self.firstPoint(), selectedGeographyLabel);

                //clear out any old map positioning
                self.initMinMaxLatLng();

                $(this).closest('.picker-menu').slideUp();
            });
        };

        /**
        * Initialize the GeoPicker.
        */
        GeoPicker.prototype.initialize = function () {
            var self = this;
            var $myElement = $('#geoPicker_' + self.controlId);
            var $hiddenField = $('#hfGeoPath_' + this.controlId);
            var deleteButtonId = 'gmnoprint-delete-button_' + self.controlId;

            // Pull anything in the hidden field onto this object's path
            self.path = $hiddenField.val();

            // store path into pathTemp in case of user cancel.
            self.pathTemp = self.path;

            // Create a new StyledMapType object, passing it the array of styles,
            // as well as the name to be displayed on the map type control.
            var styledMap = new google.maps.StyledMapType(self.styles, { name: "Styled Map" });

            // WARNING: I though about removing the "center:" from the options here but then the
            // map's controls were different and then our delete button was out of alignment.
            var mapOptions = {
                center: new google.maps.LatLng(
                        parseFloat(self.centerLatitude),
                        parseFloat(self.centerLongitude)),
                zoom: 13,
                mapTypeControlOptions: {
                    mapTypeIds: [google.maps.MapTypeId.ROADMAP, 'map_style']
                }
            };
            // center the map on the configured address
            self.centerMapOnAddress();

            self.map = new google.maps.Map(document.getElementById('geoPicker_' + self.controlId), mapOptions);
            //console.log("adding map to element( " + 'geoPicker_' + self.controlId + " )");

            //Associate the styled map with the MapTypeId and set it to display.
            self.map.mapTypes.set('map_style', styledMap);
            self.map.setMapTypeId('map_style');

            // If we have coordinates we should plot them here...
            self.plotPath(self.map);
            
            // Set up the Drawing Manager for creating polygons, circles, etc.
            self.drawingManager = new google.maps.drawing.DrawingManager({
                drawingControl: true,
                drawingControlOptions: {
                    drawingModes: self.getDrawingModes()
                },
                polygonOptions: {
                    editable: true,
                    strokeColor: self.strokeColor,
                    fillColor: self.fillColor,
                    strokeWeight: 2
                },
            });
            
            self.drawingManager.setMap(self.map);

            // but disable the drawing manager if we already have a point/polygon selected:
            if (self.path) {
                self.disableDrawingManager();
            }

            // Handle when the polygon shape drawing is "complete"
            google.maps.event.addListener(self.drawingManager, 'overlaycomplete', function (e) {
                if (e.type == google.maps.drawing.OverlayType.POLYGON || e.type == google.maps.drawing.OverlayType.MARKER ) {

                    // Disable the drawing manager once they've drawn an overlay.
                    self.disableDrawingManager();

                    // Add an event listener that selects the newly-drawn shape when the user
                    // mouses down on it.
                    var newShape = e.overlay;
                    newShape.type = e.type;
                    google.maps.event.addListener(newShape, 'click', function () {
                        self.setSelection(newShape, e.type);
                    });
                    self.setSelection(newShape, e.type);
                }
            });

            // Clear the current selection when the drawing mode is changed, or when the
            // map is clicked.
            google.maps.event.addListener(self.drawingManager, 'drawingmode_changed', self.clearSelection);
            google.maps.event.addListener(self.map, 'click', self.clearSelection);

            // as per http://stackoverflow.com/questions/832692/how-to-check-if-google-maps-is-fully-loaded
            google.maps.event.addListenerOnce(self.map, 'idle', function () {
                // move the custom delete button to the second to last item in the gmnoprint list
                $('#' + deleteButtonId).insertAfter($myElement.find('div.gmnoprint:nth-last-child(2)'));
                $('#' + deleteButtonId).fadeIn();
                //console.log("insert delete button after:");
                //console.log($myElement.find('div.gmnoprint:nth-last-child(2)'));
                // wire up an event handler to the delete button
                google.maps.event.addDomListener(document.getElementById(deleteButtonId), 'click', self.deleteSelectedShape);
            });

            self.initializeEventHandlers();
        };

        var exports = {
            geoPickers: {},
            findControl: function (controlId) {
                return exports.geoPickers[controlId];
            },
            initialize: function (options) {
                if (!options.controlId) throw '`controlId` is required.';
                var geoPicker = new GeoPicker(options);
                exports.geoPickers[options.controlId] = geoPicker;
                geoPicker.initialize();
            }
        };

        return exports;
    }());
}(jQuery));