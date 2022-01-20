<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MobileLauncher.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.MobileLauncher" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="maWarning" runat="server" />

        <%-- Edit Panel --%>
        <asp:Panel ID="pnlEditSettings" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdEditSettings" runat="server" OnSaveClick="mdEditSettings_SaveClick" Title="Mobile Launcher Settings">
                <Content>
                    <asp:UpdatePanel runat="server" ID="upnlEditSettings">
                        <ContentTemplate>
                            <Rock:RockListBox ID="lbDevices" runat="server" Label="Enabled Devices" Help="The devices to consider when determining a matching device kiosk, or leave blank for all. Typically the selection should include only one device kiosk for each geo-fenced area / campus." AutoPostBack="true" OnSelectedIndexChanged="lbDevices_SelectedIndexChanged" />

                            <Rock:RockDropDownList ID="ddlTheme" runat="server" Label="Theme" />
                            <Rock:RockDropDownList ID="ddlCheckinType" runat="server" Label="Check-in Configuration" OnSelectedIndexChanged="ddlCheckinType_SelectedIndexChanged" AutoPostBack="true" />

                            <Rock:RockListBox ID="lbAreas" runat="server" Label="Check-in Areas" Help="The check-in areas that will be used for the checkin process" />
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

        <%-- View Panel ( normal mode ) --%>
        <asp:Panel ID="pnlView" runat="server">

            <script>
                function tryGeoLocation() {
                    if (geo_position_js.init()) {
                        geo_position_js.getCurrentPosition(geoLocationSuccess_callback, geoLocationError_callback, { enableHighAccuracy: true });
                    }
                    else {
                        geoLocationError_callback(null);
                    }
                }

                function geoLocationSuccess_callback(p) {
                    $(".js-geolocation-latitude").val(p.coords.latitude.toFixed(4));
                    $(".js-geolocation-longitude").val(p.coords.longitude.toFixed(4));

                    window.location = "javascript:__doPostBack('<%=upnlContent.ClientID %>', 'GeoLocationCallback|Success')"
                }

                function geoLocationError_callback(p) {
                    window.location = "javascript:__doPostBack('<%=upnlContent.ClientID %>', 'GeoLocationCallback|Error|" + encodeURIComponent(p.message) + "')";
                }

                Sys.Application.add_load(function () {
                    if ($('.js-get-geo-location').val() == "true") {
                        var $getGeoLocationButton = $(".js-get-geolocation");
                        Rock.controls.bootstrapButton.showLoading($getGeoLocationButton);
                        tryGeoLocation();
                    }
                });
            </script>


            <%-- Hidden fields --%>
            <Rock:HiddenFieldWithClass ID="hfGetGeoLocation" runat="server" CssClass="js-get-geo-location" Value="false" />
            <Rock:HiddenFieldWithClass ID="hfLatitude" runat="server" CssClass="js-geolocation-latitude" />
            <Rock:HiddenFieldWithClass ID="hfLongitude" runat="server" CssClass="js-geolocation-longitude" />

            <%-- Main Panel --%>

            <div class="checkin-header">
                <h1>
                    <asp:Literal ID="lCheckinHeader" runat="server" Text="Mobile Check-in" /></h1>
            </div>

            <div class="checkin-body">

                <div class="checkin-scroll-panel">
                    <div class="scroller">
                        <div class="control-group checkin-body-container">
                            <ol class="checkin-summary">
                                <li><asp:Literal ID="lMessage" runat="server" Text="Before we proceed we'll need to identify you for check-in" /></li>
                            </ol>

                            <div class="controls">
                                <Rock:BootstrapButton ID="bbtnPhoneLookup" runat="server" Text="Phone Lookup" OnClick="bbtnPhoneLookup_Click" CssClass="btn btn-primary btn-block" />
                                <Rock:BootstrapButton ID="bbtnLogin" runat="server" Text="Log In" OnClick="bbtnLogin_Click" CssClass="btn btn-default btn-block" />
                                <Rock:BootstrapButton ID="bbtnGetGeoLocation" runat="server" Text="Next" OnClick="bbtnGetGeoLocation_Click" DataLoadingText="Getting Location..." CssClass="btn btn-primary btn-block js-get-geolocation" />
                                <Rock:BootstrapButton ID="bbtnTryAgain" runat="server" Text="Try Again" OnClick="bbtnTryAgain_Click" DataLoadingText="Check-in..." CssClass="btn btn-primary btn-block js-checkin-tryagain" />
                                <Rock:BootstrapButton ID="bbtnCheckin" runat="server" Text="Check-in" OnClick="bbtnCheckin_Click" DataLoadingText="Check-in..." CssClass="btn btn-primary btn-block js-checkin" />
                            </div>

                            <asp:Literal ID="lCheckinQRCodeHtml" runat="server" />
                        </div>
                    </div>
                </div>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
