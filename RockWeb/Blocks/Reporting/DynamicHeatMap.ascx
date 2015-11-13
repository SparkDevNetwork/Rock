<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DynamicHeatMap.ascx.cs" Inherits="RockWeb.Blocks.Reporting.DynamicHeatMap" %>

<script type="text/javascript" src="https://google-maps-utility-library-v3.googlecode.com/svn/trunk/maplabel/src/maplabel-compiled.js"></script>


<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbConfigurationWarning" runat="server" NotificationBoxType="Warning" Visible="false" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-map-marker"></i>&nbsp;Dynamic Map</h1>
            </div>


            <div class="panel-body">
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlUserDataView" runat="server" Label="Dataview" Help="Select the dataview to use to filter the reults." Required="true" />
                        <asp:Panel ID="pnlFilter" runat="server">
                            <Rock:CampusesPicker ID="cpCampuses" runat="server" Label="Campuses" Required="false" />
                        </asp:Panel>
                        <div class="actions margin-t-md">
                            <asp:LinkButton ID="btnFilter" runat="server" AccessKey="m" Text="Filter" CssClass="btn btn-primary btn-sm" OnClick="btnFilter_Click" />
                        </div>
                    </div>
                </div>

                <asp:Literal ID="lMapStyling" runat="server" />


                <asp:Panel ID="pnlMap" runat="server">
                    <div id="map_wrapper">
                        <div id="map_canvas" class="mapping"></div>
                    </div>
                </asp:Panel>
                <div class="btn btn-danger btn-xs js-deleteshape">Delete selected shape</div>
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
                    <Rock:RockDropDownList ID="ddlBlockConfigDataView" runat="server" Label="Dataview" Help="Select the dataview to use to filter the reults." Required="false" ValidationGroup="vgConfigure" />
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

        <script>
            
            Sys.Application.add_load(function () {
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
                    }

                    // Display a map on the page
                    var mapCanvas = document.getElementById('map_canvas');
                    map = new google.maps.Map(mapCanvas, mapOptions);
                    map.setTilt(45);
                    map.setCenter(centerLatLng);

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
                        radius: 32
                    });

                    heatmap.setMap(map);

                    var initialColor = GetNextColor();

                    drawingManager = new google.maps.drawing.DrawingManager({
                        drawingMode: google.maps.drawing.OverlayType.RECTANGLE,
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
                        AddUpdateShape(event.overlay, false);
                    });

                    google.maps.event.addListener(drawingManager, 'polygoncomplete', function (polygon) {
                        google.maps.event.addListener(polygon, 'dragend', function (a,b,c) {
                            allShapes.forEach( function(s) {
                                AddUpdateShape(s, false);
                            });
                        });
                        google.maps.event.addListener(polygon.getPath(), 'insert_at', function (a,b,c) {
                            allShapes.forEach( function(s) {
                                AddUpdateShape(s, false);
                            });
                        });
                    });

                    function GetNextColor() {
                        if (polygonColors && polygonColors.length) {
                            if (polygonColorIndex >= polygonColors.length) {
                                polygonColorIndex = 0;
                            }

                            return polygonColors[polygonColorIndex++];
                        }

                        return null;
                    }

                    function AddUpdateShape(shape, justUpdate) {
                        selectedShape = shape;
                        
                        if (!justUpdate) {
                            google.maps.event.addListener(shape, 'click', function () {
                                selectedShape = shape
                            });

                            if (polygonColors && polygonColors.length) {
                                var color = GetNextColor();

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
                        var mapCountLabel = new MapLabel({
                            position: selectedBounds.getCenter(),
                            map: map,
                            fontSize: 24,
                            text: totalCount.toString()
                        });

                        if (selectedShape.mapCountLabel) {
                            selectedShape.mapCountLabel.setMap(null);
                        }

                        selectedShape.mapCountLabel = mapCountLabel;

                        if (!justUpdate) {
                            selectedShape.addListener('bounds_changed', function (event) {
                                var resizedShape = this;
                                AddUpdateShape(resizedShape, true);
                            });
                        }
                    }
                }

                $('.js-deleteshape').click(function () {
                    if (selectedShape) {
                        selectedShape.setMap(null);
                        selectedShape.mapCountLabel.setMap(null);
                        selectedShape = null;
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
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
