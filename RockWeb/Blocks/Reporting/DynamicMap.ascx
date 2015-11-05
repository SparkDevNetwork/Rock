<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DynamicMap.ascx.cs" Inherits="RockWeb.Blocks.Reporting.DynamicMap" %>

<script type="text/javascript" src="http://google-maps-utility-library-v3.googlecode.com/svn/trunk/maplabel/src/maplabel-compiled.js"></script>


<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbConfigurationWarning" runat="server" NotificationBoxType="Warning" Visible="false" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-map-marker"></i>&nbsp;Dynamic Map</h1>
            </div>
            <div class="panel-body">

                <asp:Literal ID="lMapStyling" runat="server" />

                <div class="btn btn-danger js-deleteshape">Delete selected</div>
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
                    <Rock:RockDropDownList ID="ddlDataView" runat="server" Label="Dataview" Help="Select the dataview to use to filter the reults." Required="false" ValidationGroup="vgConfigure" />
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

        <script>
            
            Sys.Application.add_load(function () {
                var allShapes = [];
                var allMarkers = [];
                var selectedShape;
                var map;

                var heatMap;
                var bounds = new google.maps.LatLngBounds();

                var mapStyle = <%=this.StyleCode%>;
                var polygonColorIndex = 0;
                var polygonColors = $('#<%=hfPolygonColors.ClientID%>').val();

                initializeMap();

                function initializeMap() {
                    var lat = Number($('#<%=hfCenterLatitude.ClientID%>').val());
                    var long = Number($('#<%=hfCenterLongitude.ClientID%>').val());
                    var zoom = Number($('#<%=hfZoom.ClientID%>').val());
                    var centerLatLng = new google.maps.LatLng(lat,long );
                    
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

                    if (!bounds.isEmpty()) {
                        map.fitBounds(bounds);
                    }

                    var heatMapData = [
<%=this.HeatMapData%>]

                    var heatMapBounds = new google.maps.LatLngBounds();
                    heatMapData.forEach(function (a) {
                        heatMapBounds.extend(a.location || a);
                    });

                    heatmap = new google.maps.visualization.HeatmapLayer({
                        dissipating: true,
                        data: heatMapData,
                        radius: 128
                    });

                    heatmap.setMap(map);

                    var drawingManager = new google.maps.drawing.DrawingManager({
                        drawingMode: google.maps.drawing.OverlayType.RECTANGLE,
                        drawingControl: true,
                        drawingControlOptions: {
                            position: google.maps.ControlPosition.TOP_CENTER,
                            drawingModes: [
                                google.maps.drawing.OverlayType.CIRCLE,
                                google.maps.drawing.OverlayType.POLYGON,
                                google.maps.drawing.OverlayType.POLYLINE,
                                google.maps.drawing.OverlayType.RECTANGLE
                            ]
                        },
                        circleOptions: {
                            draggable: true,
                            editable: true
                        },
                        polygonOptions: {
                            draggable: true,
                            editable: true,
                            strokeColor: self.strokeColor,
                            fillColor: self.fillColor,
                            strokeWeight: 2
                        },
                        polylineOptions: {
                            draggable: true,
                            editable: true
                        },
                        rectangleOptions: {
                            draggable: true,
                            editable: true
                        }
                    });

                    drawingManager.setMap(map);

                    google.maps.event.addListener(drawingManager, 'overlaycomplete', function (event) {
                        AddUpdateShape(event.overlay, false);
                    });

                    function AddUpdateShape(shape, justUpdate) {
                        selectedShape = shape;
                        
                        if (!justUpdate) {
                            google.maps.event.addListener(shape, 'click', function () {
                                selectedShape = shape
                                selectedShape.set('fillColor', 'blue');
                            });

                            allShapes += shape;
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
                        var shapeCenter = selectedBounds.getCenter();
                        var infoHtml = totalCount.toString();

                        var mapLabel = new MapLabel({
                            position: shapeCenter,
                            map: map,
                            fontSize: 34,
                            text: infoHtml
                        });

                        selectedShape.mapLabel = mapLabel;

                        allMarkers += mapLabel;
                        if (!justUpdate) {
                            selectedShape.addListener('bounds_changed', function (event) {
                                var resizedShape = this;
                                resizedShape.mapLabel.setMap(null);
                                AddUpdateShape(resizedShape, true);
                            });
                        }

                        

                    }
                }

                $('.js-deleteshape').click(function () {
                    if (selectedShape) {
                        selectedShape.setMap(null);
                        selectedShape.mapLabel.setMap(null);
                        selectedShape = null;
                    }
                });
            });

      


        </script>

    </ContentTemplate>
</asp:UpdatePanel>
