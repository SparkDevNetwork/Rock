<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DynamicHeatMap.ascx.cs" Inherits="RockWeb.Blocks.Reporting.DynamicHeatMap" %>

<asp:UpdatePanel ID="upnlContent" runat="server" UpdateMode="Conditional">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbConfigurationWarning" runat="server" NotificationBoxType="Warning" Visible="false" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-map-marker"></i>&nbsp;Dynamic Map</h1>
                <a class="btn btn-xs btn-default pull-right margin-l-sm" onclick="javascript: toggleOptions()"><i title="Options" class="fa fa-gear"></i></a>
            </div>
            <asp:Panel ID="pnlOptions" runat="server" Title="Options" CssClass="panel-body js-options" Style="display: none">
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlUserDataView" runat="server" Label="Dataview" Help="Select the dataview to use to filter the results." Required="true" />
                        <Rock:CampusesPicker ID="cpCampuses" runat="server" Label="Campus Filter" Help="Select the campuses to narrow the results down to families with that home campus." Required="false" />
                        <Rock:GroupPicker ID="gpGroupToMap" runat="server" Label="Geo-fencing Group" Help="Select a Group to show the geofences for that group and its child groups" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbShowCampusLocations" runat="server" Label="Show Campus Locations On Map" Checked="true" />
                        <Rock:RangeSlider ID="rsDataPointRadius" runat="server" MinValue="0" MaxValue="128" Text="32" Label="Radius" Help="The radius of influence for each data point, in pixels" />
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnApplyOptions" runat="server" Text="Apply" CssClass="btn btn-primary" OnClick="btn_ApplyOptionsClick" />
                </div>
            </asp:Panel>

            <div class="margin-all-md">
                <div class="pull-right js-heatmap-actions">
                    <asp:Panel ID="pnlPieSlicer" runat="server" CssClass="btn btn-default btn-xs js-createpieshape">
                        <i class='fa fa-pie-chart' title="Create pie slices from selected circle"></i>
                    </asp:Panel>
                    <asp:Panel ID="pnlSaveShape" runat="server" CssClass="btn btn-default btn-xs js-saveshape">
                        <i class='fa fa-floppy-o' title="Save selected shape to a named location"></i>
                    </asp:Panel>

                    <div class="btn btn-danger btn-xs js-deleteshape" style="display:none"><i class='fa fa-times' title="Delete selected shape"></i></div>
                </div>
            </div>
            <div class="panel-body">
                <asp:Literal ID="lMapStyling" runat="server" />

                <asp:Panel ID="pnlMap" runat="server">

                    <div id="map_wrapper">
                        <div id="map_canvas" class="mapping"></div>
                    </div>
                </asp:Panel>

                <asp:Literal ID="lMessages" runat="server" />
                <asp:Literal ID="lDebug" runat="server" />
            </div>
        </div>

        <asp:HiddenField ID="hfPolygonColors" runat="server" />
        <asp:HiddenField ID="hfCenterLatitude" runat="server" />
        <asp:HiddenField ID="hfCenterLongitude" runat="server" />
        <asp:HiddenField ID="hfZoom" runat="server" />

        <%-- Configuration Panel --%>
        <asp:Panel ID="pnlConfigure" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdConfigure" runat="server" ValidationGroup="vgConfigure" OnSaveClick="mdConfigure_SaveClick">
                <Content>
                    <Rock:RockDropDownList ID="ddlBlockConfigDataView" runat="server" Label="Dataview" Help="Select the dataview to use to filter the results." Required="false" ValidationGroup="vgConfigure" />
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

        <script>

            function toggleOptions() {
                $('.js-options').slideToggle();
            }
            
            Sys.Application.add_load(function () {
                
                // if this is an async postback, the map is already created, so just break out
                if ($('#map_canvas').data("googleMap"))
                {
                    return;
                }

                // hook into rangeslider
                var rangeSlider = $('#<%=rsDataPointRadius.ClientID%>');
                rangeSlider.on("change", function(obj) {
                    var newRadius = parseInt($(this).val());
                    if (heatmap) {
                        heatmap.set('radius', newRadius);
                    }
                });

                // configure/display heatmap
                var pieSlicerState = {
                    SelectedCenterPt: null,
                    SelectedRadius: null,
                    SelectedPieCuts: [],
                    CurrentPieSlices: []
                };
                
                var map;

                var heatMap;
                var drawingManager;

                var mapStyle = <%=this.StyleCode%>;
                var polygonColorIndex = 0;
                var polygonColors;

                initializeMap();

                function initializeMap() {
                    var lat = Number($('#<%=hfCenterLatitude.ClientID%>').val());
                    var long = Number($('#<%=hfCenterLongitude.ClientID%>').val());
                    var zoom = Number($('#<%=hfZoom.ClientID%>').val());
                    var centerLatLng = new google.maps.LatLng(lat,long );

                    polygonColors = $('#<%=hfPolygonColors.ClientID%>').val().split(',');
                    
                    // Set default map options
                    var mapOptions = {
                        mapTypeId: 'roadmap'
                        , styles: mapStyle
                        , center: centerLatLng
                        , zoom: zoom
                        , streetViewControl: false
                    }

                    // Display a map on the page
                    var mapCanvas = document.getElementById('map_canvas');
                    map = new google.maps.Map(mapCanvas, mapOptions);

                    map.AllShapes = [];
                    map.SelectedShape = null;

                    $(mapCanvas).data("googleMap", map);
                    map.setTilt(45);
                    map.setCenter(centerLatLng);

                    // if a GroupId was specified, show geofences
                    function addGroupGeoFence(mapItem){
                        if (typeof mapItem.PolygonPoints !== 'undefined' && mapItem.PolygonPoints.length > 0) {
                            var geoFencePath = Array();

                            $.each(mapItem.PolygonPoints, function(j, point) {
                                geoFencePath.push(new google.maps.LatLng(point.Latitude, point.Longitude));
                            });

                            var geoFencePoly = new google.maps.Polygon({
                                path: geoFencePath,
                                map: map,
                                fillColor: map.GetNextColor(),
                                fillOpacity: 0.6,
                                draggable: false,
                                editable: false,
                            });

                            geoFencePoly.Name = mapItem.Name;
                            geoFencePoly.overlayType = 'polygon';

                            map.AddUpdateShape(geoFencePoly);
                        }
                    }

                    //
                    var heatMapData = [
<%=this.HeatMapData%>]

                    var campusMarkersData = [
<%=this.CampusMarkersData%>]

                    var pinImage = {
                        path: 'M 0,0 C -2,-20 -10,-22 -10,-30 A 10,10 0 1,1 10,-30 C 10,-22 2,-20 0,0 z',
                        fillColor: '#FE7569',
                        fillOpacity: 1,
                        strokeColor: '#000',
                        strokeWeight: 1,
                        scale: 1,
                        labelOrigin: new google.maps.Point(0,-28)
                
                    };

                    campusMarkersData.forEach( function (c) {
                        marker = new google.maps.Marker({
                            position: c.location,
                            map: map,
                            title: c.campusName,
                            icon: pinImage,
                            label: String.fromCharCode(9679)
                        });
                    });

                    var heatMapBounds = new google.maps.LatLngBounds();
                    heatMapData.forEach(function (a) {
                        heatMapBounds.extend(a.location || a);
                    });

                    heatmap = new google.maps.visualization.HeatmapLayer({
                        dissipating: true,
                        data: heatMapData,
                        maxIntensity: 50,
                        radius: <%=this.DataPointRadius%>,
                    });

                    heatmap.setMap(map);

                    var groupId = <%=this.GroupId ?? 0 %>;
                    if (groupId) {
                        $.get( Rock.settings.get('baseUrl') + 'api/Groups/GetMapInfo/' + groupId, function( mapItems ) {
                            $.each(mapItems, function (i, mapItem) {
                                addGroupGeoFence(mapItem);
                            });
                        });

                        // Get Child Groups
                        $.get( Rock.settings.get('baseUrl') + 'api/Groups/GetMapInfo/' + groupId + '/Children', function( mapItems ) {
                            $.each(mapItems, function (i, mapItem) {
                                addGroupGeoFence(mapItem);
                            });
                        });
                    }

                    map.DeleteShape = function(shape) {
                        var allShapesIndex = map.AllShapes.indexOf(shape);
                                    
                        if (allShapesIndex > -1)
                        {
                            map.AllShapes.splice(allShapesIndex, 1);
                            if (map.AllShapes.length == 0)
                            {
                                $('.js-deleteshape').hide();
                            }
                        }
                                    
                        shape.setMap(null);
                        shape.mapCountLabel.setMap(null);
                        if (shape == map.SelectedShape)
                        {
                            map.SelectedShape = null;
                        }
                        
                        shape = null;
                    };

                    map.GetNextColor = function () {
                        if (polygonColors && polygonColors.length) {
                            if (polygonColorIndex >= polygonColors.length) {
                                polygonColorIndex = 0;
                            }

                            return polygonColors[polygonColorIndex++];
                        }

                        return null;
                    }

                    map.AddUpdateShape = function AddUpdateShape(shape, justUpdate) {
                        map.SelectedShape = shape;
                        
                        if (!justUpdate) {
                            google.maps.event.addListener(shape, 'click', function () {

                                map.SelectedShape = shape
                                map.AllShapes.forEach(function (s) {
                                    if (s.mapCountLabel && s.mapCountLabel.text.startsWith("*"))
                                    {
                                        s.mapCountLabel.text = s.mapCountLabel.text.slice(1);
                                        s.mapCountLabel.changed('text');
                                    }
                                });
                                
                                map.SelectedShape.mapCountLabel.text = '*' + map.SelectedShape.mapCountLabel.text;
                                map.SelectedShape.mapCountLabel.changed('text');
                            });

                            // set the color of the next shape
                            if (polygonColors && polygonColors.length) {
                                var color = map.GetNextColor();

                                drawingManager.polygonOptions.fillColor = color;
                                drawingManager.polygonOptions.strokeColor = color;
                                drawingManager.circleOptions.fillColor = color;
                                drawingManager.circleOptions.strokeColor = color;
                                drawingManager.rectangleOptions.fillColor = color;
                                drawingManager.rectangleOptions.strokeColor = color;
                            }

                            map.AllShapes.push(shape);
                            $('.js-deleteshape').show();
                        }
                        
                        // NOTE: bounds is the rectangle bounds of the shape (not the actual shape)
                        var selectedBounds = shape.getBounds();

                        var pointCount = 0;

                        var shapeCenter = null;
                        if (shape.getCenter)
                        {
                            shapeCenter = shape.getCenter();
                        }
                        else
                        {
                            shapeCenter = selectedBounds.getCenter();
                        }
                        
                        heatmapDataArray = heatmap.data.getArray();
                        for (var i = 0, len = heatmap.data.length; i < len; i++) {
                            var latLng = heatmapDataArray[i];
                            var pointWeight = 1;
                            var point = latLng;
                            if (latLng.location)
                            {
                                // weighted location
                                point = latLng.location;
                                pointWeight = latLng.weight;
                            }

                            // first check if within bounds to narrow down (for performance)
                            if (selectedBounds.contains(point)) {

                                if ('polygon' == shape.overlayType) {
                                    if (google.maps.geometry.poly.containsLocation(point, shape)) {
                                        pointCount += pointWeight;
                                    }
                                } else if ('circle' == shape.overlayType) {
                                    if (google.maps.geometry.spherical.computeDistanceBetween(shapeCenter, point) < shape.radius) {
                                        pointCount += pointWeight;
                                    }
                                } else if ('rectangle' == shape.overlayType) {
                                    pointCount += pointWeight;
                                }
                            }
                        }

                        var totalCount = pointCount;
                        var mapLabel = totalCount.toString();
                        if (map.SelectedShape.Name){
                            mapLabel = map.SelectedShape.Name + ': ' + mapLabel;
                        }

                        if (!map.SelectedShape.mapCountLabel) {
                            map.SelectedShape.mapCountLabel = new MapLabel({
                                map:map,
                                fontSize: <%=this.LabelFontSize%>,
                                text:'x',
                                position: shapeCenter
                            });
                        }

                        map.SelectedShape.mapCountLabel.position = shapeCenter;
                        map.SelectedShape.mapCountLabel.changed('position');
                        map.SelectedShape.mapCountLabel.text = mapLabel;
                        map.SelectedShape.mapCountLabel.changed('text');

                        if (!justUpdate) {
                            map.SelectedShape.addListener('bounds_changed', function (event) {
                                var resizedShape = this;
                                map.AddUpdateShape(resizedShape, true);
                            });
                        }
                    }

                    var initialColor = map.GetNextColor();

                    drawingManager = new google.maps.drawing.DrawingManager({
                        drawingMode: null,
                        drawingControl: true,
                        drawingControlOptions: {
                            position: google.maps.ControlPosition.TOP_CENTER,
                            drawingModes: [
                                google.maps.drawing.OverlayType.CIRCLE,
                                google.maps.drawing.OverlayType.POLYGON,
                                google.maps.drawing.OverlayType.RECTANGLE
                            ]
                        },
                        circleOptions: {
                            draggable: true,
                            editable: true,
                            fillColor: initialColor,
                            strokeColor: initialColor
                        },
                        polygonOptions: {
                            draggable: true,
                            editable: true,
                            fillColor: initialColor,
                            strokeColor: initialColor,
                            strokeWeight: 2
                        },
                        polylineOptions: {
                            draggable: true,
                            editable: true,
                            fillColor: initialColor,
                            strokeColor: initialColor
                        },
                        rectangleOptions: {
                            draggable: true,
                            editable: true,
                            fillColor: initialColor,
                            strokeColor: initialColor
                        }
                    });

                    drawingManager.setMap(map);

                    google.maps.event.addListener(drawingManager, 'overlaycomplete', function (event) {
                        var shape = event.overlay;
                        shape.overlayType = event.type;
                        map.AddUpdateShape(event.overlay, false);
                    });

                    google.maps.event.addListener(drawingManager, 'polygoncomplete', function (polygon) {
                        google.maps.event.addListener(polygon, 'dragend', function (a,b,c) {
                            map.AllShapes.forEach( function(s) {
                                map.AddUpdateShape(s, true);
                            });
                        });
                        google.maps.event.addListener(polygon.getPath(), 'insert_at', function (a,b,c) {
                            map.AllShapes.forEach( function(s) {
                                map.AddUpdateShape(s, true);
                            });
                        });
                        google.maps.event.addListener(polygon.getPath(), 'set_at', function (a,b,c) {
                            map.AllShapes.forEach( function(s) {
                                map.AddUpdateShape(s, true);
                            });
                        });
                    });
                }

                $('.js-deleteshape').click(function () {
                    if (map.SelectedShape) {
                        map.DeleteShape(map.SelectedShape);
                    }
                });

                $('.js-saveshape').click(function () {

                    if (map.SelectedShape) {
                        
                        var geoFencePath;
                        if (typeof(map.SelectedShape.getPaths) != 'undefined') {
                            
                            var coordinates = new Array();
                            var vertices = map.SelectedShape.getPaths().getAt(0);
                            // Iterate over the vertices of the shape's path
                            for (var i = 0; i < vertices.length; i++) {
                                var xy = vertices.getAt(i);
                                coordinates[i] = xy.toUrlValue();
                            }

                            // if the last coor is not already the first, then
                            // add the first vertex to the end of the path.
                            if (coordinates[coordinates.length - 1] != coordinates[0]) {
                                coordinates.push(coordinates[0]);
                            }

                            geoFencePath = coordinates.join('|');
                        } else if ('rectangle' == map.SelectedShape.overlayType)
                        {
                            var ne = map.SelectedShape.getBounds().getNorthEast();
                            var sw = map.SelectedShape.getBounds().getSouthWest();

                            geoFencePath = ne.toUrlValue() + '|' + sw.lat() + ',' + ne.lng() + '|' + sw.toUrlValue() + '|' + ne.lat() + ',' + sw.lng() + ' | ' + ne.toUrlValue();
                        } else if ('circle' == map.SelectedShape.overlayType)
                        {
                            var center = map.SelectedShape.getCenter();
                            var radius = map.SelectedShape.radius;
                            geoFencePath = 'CIRCLE|' + center.lng() + ' ' + center.lat() + '|' + radius;
                        }

                        $('#<%=hfLocationSavePath.ClientID%>').val(geoFencePath);
                        
                        Rock.controls.modal.showModalDialog($('#<%=mdSaveLocation.ClientID%>').find('.rock-modal'), '#<%=upSaveLocation.ClientID%>');
                    }
                });

                $('.js-createpieshape').click(function () {

                    // make sure drawing manager mode is the hand so that 'mousemove' will fire
                    drawingManager.setDrawingMode(null);

                    if ((map.SelectedShape && (typeof(map.SelectedShape.overlayType) != 'undefined') && map.SelectedShape.overlayType == 'circle') || (pieSlicerState.SelectedCenterPt && pieSlicerState.SelectedRadius)) {

                        // if we are starting with a new shape (not a pieslice), start over with a new pieslicer
                        if (map.SelectedShape && (typeof(map.SelectedShape.overlayType) != 'undefined') && map.SelectedShape.isPieSlice != true) {
                            if (map.SelectedShape.overlayType == 'circle') {
                                pieSlicerState.SelectedCenterPt = map.SelectedShape.getCenter();
                                pieSlicerState.SelectedRadius = map.SelectedShape.radius;
                            } else {
                                pieSlicerState.SelectedCenterPt = null;
                                pieSlicerState.SelectedRadius = null;
                            }
                            pieSlicerState.SelectedPieCuts = [];
                            pieSlicerState.CurrentPieSlices = [];
                        }

                        // map to the click event which we'll use to make the pieslice position permanent
                        if (!map.pieClickListener) {
                            map.pieClickListener = google.maps.event.addListener(map, 'click', function (event) {
                                if (map.SelectedShape && map.SelectedShape.isPieDrawing){
                                    pieSlicerState.SelectedPieCuts = [];
                                    pieSlicerState.CurrentPieSlices.forEach(function(ps){
                                        pieSlicerState.SelectedPieCuts.push(ps.startArc);
                                    })
                                    
                                    map.SelectedShape.isPieDrawing = false;
                                    map.SelectedShape.deleteOnFirstSlice = false;
                                    map.pieMouseMoveListener.remove();
                                    map.pieMouseMoveListener = null;
                                }
                            });
                        }
                        
                        // when the move moves over the map, draw the pie shapes in realtime based on the mouse position relative to the center of the orig circle
                        if (!map.pieMouseMoveListener) {
                            map.pieMouseMoveListener = google.maps.event.addListener(map, 'mousemove', function (event) {
                                if (pieSlicerState.SelectedCenterPt && pieSlicerState.SelectedRadius && map.SelectedShape){

                                    var heading = google.maps.geometry.spherical.computeHeading(pieSlicerState.SelectedCenterPt, event.latLng);
                                    while (heading < 0)
                                    {
                                        heading += 360;
                                    }

                                    var currentPieCuts = [];
                                    currentPieCuts.push(heading);
                                    pieSlicerState.SelectedPieCuts.forEach(function(pc) {
                                        currentPieCuts.push(pc);
                                    });

                                    currentPieCuts.sort(function(a,b){
                                        return a - b;
                                    });

                                    // if we already have the pieslices, delete them all and we'll redraw them based on the currentPieCuts
                                    pieSlicerState.CurrentPieSlices.forEach(function(ps) {
                                        map.DeleteShape(ps);
                                    });

                                    pieSlicerState.CurrentPieSlices = [];

                                    // if we are starting with a circle, delete it since we are redrawing it as a big pieslice
                                    if (map.SelectedShape && (map.SelectedShape.isPieDrawing || map.SelectedShape.overlayType == 'circle')){
                                        map.DeleteShape(map.SelectedShape);
                                    }
                                    
                                    currentPieCuts.forEach(function(pc, i) {
                                        var centerPt = pieSlicerState.SelectedCenterPt;
                                        var radiusMeters = pieSlicerState.SelectedRadius;
                                        
                                        var pieSlicePath = Array();

                                        var nextRadialPoint = pc;
                                        lastRadialPoint = pc;

                                        if (i < currentPieCuts.length-1){
                                            // find the next arc starting point
                                            lastRadialPoint = currentPieCuts[i+1];
                                        }
                                        else{
                                            // use the first arc of our currentPieCuts
                                            lastRadialPoint = currentPieCuts[0];

                                            // make sure the pieshape colors don't flash to random colors as it is resized
                                            polygonColorIndex = 0;
                                        }

                                        // if the start of the arc is counterclockwise from the current, move it back 360 degrees (because it is probably the last missing piece of the circle)
                                        if (nextRadialPoint >= lastRadialPoint){
                                            nextRadialPoint -= 360;
                                        }
                                    
                                        // create a Google Map Path as an array of all the lines from the center to the outer radius for every full degree to make it look like a pie slice
                                        while (nextRadialPoint < lastRadialPoint) {
                                            pieSlicePath.push(google.maps.geometry.spherical.computeOffset(centerPt, radiusMeters, nextRadialPoint));
                                            nextRadialPoint += 1;
                                        }
                            
                                        // ensure that the last path of the pieslice is there for the last line of the path
                                        var endArc = lastRadialPoint;
                                        pieSlicePath.push(google.maps.geometry.spherical.computeOffset(centerPt, radiusMeters, endArc));
                            
                                        // put the center point to the start and end of the pieSlicePath
                                        pieSlicePath.unshift(centerPt);
                                        pieSlicePath.push(centerPt);

                                        var pieSlicePoly = new google.maps.Polygon({
                                            path: pieSlicePath,
                                            map: map,
                                            fillColor: map.GetNextColor(),
                                            fillOpacity: 0.6,
                                            draggable: false,
                                            editable: false,
                                        });

                                        pieSlicePoly.isPieDrawing = true;
                                        pieSlicePoly.startArc = pc;
                                        pieSlicePoly.overlayType = 'polygon';
                                        pieSlicePoly.isPieSlice = true;
                                        while (pieSlicePoly.startArc < 0){
                                            pieSlicePoly.startArc += 360;
                                        }

                                        pieSlicerState.CurrentPieSlices.push(pieSlicePoly);

                                        map.AddUpdateShape(pieSlicePoly, false );
                                    });
                                }
                            
                            });
                        }
                    }
                });
            });

            // extend polygon to getBounds
            if (!google.maps.Polygon.prototype.getBounds) {
                google.maps.Polygon.prototype.getBounds = function(latLng) {
                    var bounds = new google.maps.LatLngBounds();
                    var paths = this.getPaths();
                    var path;
                    for (var p = 0; p < paths.getLength(); p++) {
                        path = paths.getAt(p);
                        for (var i = 0; i < path.getLength(); i++) {
                            bounds.extend(path.getAt(i));
                        }
                    }
 
                    return bounds;
                }
            } 

            // extend polygon to getBounds
            if (!google.maps.Polyline.prototype.getBounds) {
                google.maps.Polyline.prototype.getBounds = function() {
                    var bounds = new google.maps.LatLngBounds();
                    this.getPath().forEach(function(e) {
                        bounds.extend(e);
                    });
                    return bounds;
                }
            };

        </script>

    </ContentTemplate>
</asp:UpdatePanel>


<asp:UpdatePanel ID="upSaveLocation" runat="server">
    <ContentTemplate>
        <Rock:HiddenFieldWithClass ID="hfLocationSavePath" runat="server" CssClass="js-savelocation-value" />
        <Rock:HiddenFieldWithClass ID="hfLocationId" runat="server" CssClass="js-savelocationid" />

        <script>
            function saveLocationGeofence() {
                var locationId = $('#<%=lpLocation.ClientID%> .js-item-id-value').val();
                var locationName = $('#<%=lpLocation.ClientID%> .js-item-name-value').val();
                $('#<%=hfLocationId.ClientID%>').val(locationId);
                window.location = "javascript:__doPostBack('<%=upSaveLocation.ClientID%>')";

                var map = $('#map_canvas').data().googleMap;

                map.SelectedShape.Name = locationName;
                map.AddUpdateShape(map.SelectedShape, true);
            }
        </script>

        <%-- Save Shape to Location --%>
        <Rock:ModalDialog ID="mdSaveLocation" runat="server" CssClass="js-savelocation-modal" ValidationGroup="vgSaveLocation" OnOkScript="saveLocationGeofence();" Visible="true">
            <Content>
                <Rock:LocationItemPicker ID="lpLocation" runat="server" AllowMultiSelect="false" />
            </Content>
        </Rock:ModalDialog>
        
    </ContentTemplate>
</asp:UpdatePanel>
