<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupMapper.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupMapper" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        
        <asp:Literal ID="lMapStyling" runat="server" />
        
        <asp:Panel ID="pnlMap" runat="server">
            <div id="map_wrapper">
                <div id="map_canvas" class="mapping"></div>
            </div>

            <asp:Literal ID="lGroupJson" runat="server" />
            <script>
                Sys.Application.add_load(function () {

                    initializeMap();

                    function initializeMap() {
                        var map;
                        var bounds = new google.maps.LatLngBounds();
                        var mapOptions = {
                            mapTypeId: 'roadmap'
                        };

                        // Display a map on the page
                        map = new google.maps.Map(document.getElementById("map_canvas"), mapOptions);
                        map.setTilt(45);

                        // Display multiple markers on a map
                        var infoWindow = new google.maps.InfoWindow(), marker, i;

                        // Loop through our array of markers & place each one on the map
                        $.each(groupData.groups, function (i, group) {
                            console.log(group);
                            var position = new google.maps.LatLng(group.latitude, group.longitude);
                            bounds.extend(position);

                            marker = new google.maps.Marker({
                                position: position,
                                map: map,
                                title: group.name
                            });

                            // Allow each marker to have an info window    
                            google.maps.event.addListener(marker, 'click', (function (marker, i) {
                                return function () {
                                    infoWindow.setContent(groupData.groups[i].name);
                                    infoWindow.open(map, marker);
                                }
                            })(marker, i));

                            map.fitBounds(bounds);
                       
                        });

                        // Override our map zoom level once our fitBounds function runs (Make sure it only runs once)
                        var boundsListener = google.maps.event.addListener((map), 'bounds_changed', function (event) {
                            this.setZoom(14);
                            google.maps.event.removeListener(boundsListener);
                        });
                    }
                });

            
            </script>
        </asp:Panel>

        <asp:Literal ID="lMessages" runat="server" />

    </ContentTemplate>
</asp:UpdatePanel>
