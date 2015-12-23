<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DynamicHeatMap.ascx.cs" Inherits="RockWeb.Blocks.Reporting.DynamicHeatMap" %>

<script type="text/javascript" src="https://google-maps-utility-library-v3.googlecode.com/svn/trunk/maplabel/src/maplabel-compiled.js"></script>


<asp:UpdatePanel ID="upnlContent" runat="server">
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
                        <Rock:CampusesPicker ID="cpCampuses" runat="server" Label="Campuses" Help="Select the campuses to narrow the results down to families with that home campus." Required="false" />
                        <Rock:GroupPicker ID="gpGroupToMap" runat="server" Label="Group" Help="Select a Group to show the geofences for that group and it's child groups" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbShowCampusLocations" runat="server" Label="Show Campus locations on map" Checked="true" />
                        <Rock:RangeSlider ID="rsDataPointRadius" runat="server" MinValue="0" MaxValue="128" Text="32" Label="Radius" Help="The radius of influence for each data point, in pixels" />
                        <Rock:NumberBox ID="nbFontSize" runat="server" NumberType="Integer" Label="Label Font Size" />
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnApplyOptions" runat="server" Text="Apply" CssClass="btn btn-primary" OnClick="btn_ApplyOptionsClick" />
                </div>
            </asp:Panel>

            <div class="margin-all-md">
                <div class="pull-right">
                    <div class="btn btn-default btn-xs js-createpieshape "><i class='fa fa-pie-chart' title="Create pie slices from circle"></i></div>
                    <div class="btn btn-danger btn-xs js-deleteshape"><i class='fa fa-times' title="Delete selected shape"></i></div>
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

                // hook into rangeslider
                var rangeSlider = $('#<%=rsDataPointRadius.ClientID%>');
                rangeSlider.on("change", function(obj) {
                    var newRadius = parseInt($(this).val());
                    if (heatmap) {
                        heatmap.set('radius', newRadius);
                    }
                });

                // configure/display heatmap
                var allShapes = [];
                var selectedShape;
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

                            map.AddUpdateShape(geoFencePoly);
                        }
                    }

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

                    //
                    var heatMapData = [
<%=this.HeatMapData%>]

                    var campusMarkersData = [
<%=this.CampusMarkersData%>]
                    
                    var pinImage = new google.maps.MarkerImage('//chart.googleapis.com/chart?chst=d_map_pin_letter&chld=%E2%80%A2|' + 'FE7569',
                        new google.maps.Size(21, 34),
                        new google.maps.Point(0,0),
                        new google.maps.Point(10, 34));

                    var pinShadow = new google.maps.MarkerImage('//chart.googleapis.com/chart?chst=d_map_pin_shadow',
                        new google.maps.Size(40, 37),
                        new google.maps.Point(0, 0),
                        new google.maps.Point(12, 35));

                    campusMarkersData.forEach( function (c) {
                        marker = new google.maps.Marker({
                            position: c.location,
                            map: map,
                            title: c.campusName,
                            icon: pinImage,
                            shadow: pinShadow
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

                    map.GetNextColor = function GetNextColor() {
                        if (polygonColors && polygonColors.length) {
                            if (polygonColorIndex >= polygonColors.length) {
                                polygonColorIndex = 0;
                            }

                            return polygonColors[polygonColorIndex++];
                        }

                        return null;
                    }

                    map.AddUpdateShape = function AddUpdateShape(shape, justUpdate) {
                        selectedShape = shape;
                        
                        if (!justUpdate) {
                            google.maps.event.addListener(shape, 'click', function () {
                                selectedShape = shape
                            });

                            if (polygonColors && polygonColors.length) {
                                var color = map.GetNextColor();

                                drawingManager.polygonOptions.fillColor = color;
                                drawingManager.polygonOptions.strokeColor = color;
                                drawingManager.circleOptions.fillColor = color;
                                drawingManager.circleOptions.strokeColor = color;
                                drawingManager.rectangleOptions.fillColor = color;
                                drawingManager.rectangleOptions.strokeColor = color;
                            }

                            allShapes.push(shape);
                        }

                        var selectedBounds = shape.getBounds();

                        var pointCount = 0;
                        heatmap.data.forEach(function (latLng) {
                            if (latLng.location) {
                                if (selectedBounds.contains(latLng.location)) {
                                    pointCount += latLng.weight;
                                }
                            } else {
                                if (selectedBounds.contains(latLng)) {
                                    pointCount++;
                                }
                            }
                        });

                        var totalCount = pointCount;
                        var mapLabel = totalCount.toString();
                        if (selectedShape.Name){
                            mapLabel = selectedShape.Name + ': ' + mapLabel;
                        }
                        var mapCountLabel = new MapLabel({
                            position: selectedBounds.getCenter(),
                            map: map,
                            fontSize: <%=this.LabelFontSize%>,
                            text: mapLabel
                        });

                        if (selectedShape.mapCountLabel) {
                            selectedShape.mapCountLabel.setMap(null);
                        }

                        selectedShape.mapCountLabel = mapCountLabel;

                        if (!justUpdate) {
                            selectedShape.addListener('bounds_changed', function (event) {
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
                            allShapes.forEach( function(s) {
                                map.AddUpdateShape(s, false);
                            });
                        });
                        google.maps.event.addListener(polygon.getPath(), 'insert_at', function (a,b,c) {
                            allShapes.forEach( function(s) {
                                map.AddUpdateShape(s, false);
                            });
                        });
                        google.maps.event.addListener(polygon.getPath(), 'set_at', function (a,b,c) {
                            allShapes.forEach( function(s) {
                                map.AddUpdateShape(s, false);
                            });
                        });
                    });
                }

                $('.js-deleteshape').click(function () {
                    if (selectedShape) {
                        selectedShape.setMap(null);
                        selectedShape.mapCountLabel.setMap(null);
                        selectedShape = null;
                    }
                });

                $('.js-createpieshape').click(function () {
                    if (selectedShape && selectedShape.overlayType == 'circle') {
                        var centerPt = selectedShape.center;
                        var radiusMeters = selectedShape.radius;
                        selectedShape.setMap(null);
                        selectedShape.mapCountLabel.setMap(null);
                        selectedShape = null;

                        var i = 0;
                        for (; i < 6; i++) {
                            var startDegrees = i*60;

                            var pieSlicePath = Array();
                            pieSlicePath.push(google.maps.geometry.spherical.computeOffset(centerPt, radiusMeters, startDegrees));
                            pieSlicePath.push(google.maps.geometry.spherical.computeOffset(centerPt, radiusMeters, startDegrees+60));
                            pieSlicePath.unshift(centerPt);
                            pieSlicePath.push(centerPt);
                            var pieSlicePoly = new google.maps.Polygon({
                                path: pieSlicePath,
                                map: map,
                                fillColor: map.GetNextColor(),
                                fillOpacity: 0.6,
                                draggable: true,
                                editable: true,
                            });

                            google.maps.event.trigger(drawingManager, 'polygoncomplete', pieSlicePoly);
                            map.AddUpdateShape(pieSlicePoly, false);
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
