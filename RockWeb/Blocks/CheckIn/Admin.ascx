<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Admin.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Admin" %>
<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <script>
            function IsIpadAppWithCamera () {
                var kioskType = $('.js-kiosk-type').val();

                // If KioskType is not null, honor KioskType setting, otherwise use 'auto-detect' logic.
                var isIPadAppWithCamera = false;
                if ('' == kioskType) {
                    // kioskType is not defined, so autodetect
                    isIPadAppWithCamera = typeof window.RockCheckinNative !== 'undefined' && typeof window.RockCheckinNative.StartCamera !== 'undefined';
                }
                else if ('IPad' == kioskType) {
                    // kioskType is defined, so double check if really is
                    isIPadAppWithCamera = typeof window.RockCheckinNative !== 'undefined' && typeof window.RockCheckinNative.StartCamera !== 'undefined';
                }
                else {
                    // kioskType is set, but isn't set to IPad
                    isIPadAppWithCamera = false;
                }

                return isIPadAppWithCamera;
            }

            function handleHtml5CameraSelection (s) {
                var cameraDeviceId = $(s.options[ s.selectedIndex ]).prop('value');
                localStorage.CameraDeviceId = cameraDeviceId;
            }

            // Note that if this is running in the IPad App, but is using a Device that is configured as KioskType.Browser
            // with an HTML5 Camera, the HTML5 cameras won't get listed because of the IPad app's browser permissions.
            // This is OK because we don't really support using HTML5 Camera in the IPad.

            function loadHtml5CameraOptions ($cameraListSelect) {
                // https://blog.minhazav.dev/HTML5-QR-Code-scanning-launched-v1.0.1/#enumerate-all-available-cameras
                // This method will trigger user permissions
                Html5Qrcode.getCameras().then(devices => {
                    /**
                     * devices is an array of objects of type:
                     * { id: "id", label: "label" }
                     */

                    if (devices && devices.length) {
                        var url = new URL(window.location.href);

                        // NOTE: this won't get called if ConfigureFromURL has enough data to redirect to the Welcome page,
                        // But just in case is doesn't, we'll default the camera dropdown to whatever the CameraIndex specified
                        var cameraIndexFromUrl = parseInt(url.searchParams.get("CameraIndex"));

                        var cameraSelected = false;

                        for (var deviceIndex in devices) {
                            var device = devices[ deviceIndex ];

                            var cameraOption = {
                                value: device.id,
                                text: device.label,
                            };

                            if (cameraIndexFromUrl == deviceIndex) {
                                // if CameraIndex was specified in the URL, choose the
                                // camera based on position
                                localStorage.CameraDeviceId = device.id;
                                cameraOption.selected = 'selected';
                                cameraSelected = true;
                            }
                            else if (device.id == localStorage.CameraDeviceId) {
                                if (!cameraSelected) {
                                    cameraOption.selected = 'selected';
                                    cameraSelected = true;
                                }
                            }

                            $cameraListSelect.append($('<option>', cameraOption));
                        }

                        if (!cameraSelected) {
                            // if camera not selected yet (or the localStorge.CameraDeviceId isn't one of the cameras), default to first one
                            if (devices.length > 0) {
                                localStorage.CameraDeviceId = devices[ 0 ].id;
                            }
                        }

                    }
                }).catch(err => {
                    console.log(err);
                    // error enumerating cameras
                });
            }

            Sys.Application.add_load(function () {
                var $html5CameraOptions = $('.js-html5-camera-options')

                var isIPadAppWithCamera = IsIpadAppWithCamera();

                if (isIPadAppWithCamera) {
                    if ($html5CameraOptions.length) {
                        $html5CameraOptions.hide();
                    }
                }
                else {
                    var $html5CameraListSelect = $('.js-detected-html5-camera-list');
                    if ($html5CameraListSelect.length) {
                        loadHtml5CameraOptions($html5CameraListSelect);
                    }
                }

            });
        </script>


        <asp:PlaceHolder ID="phGeoCodeScript" runat="server" />
        <asp:HiddenField ID="hfGeoError" runat="server" />
        <asp:HiddenField ID="hfLatitude" runat="server" />
        <asp:HiddenField ID="hfLongitude" runat="server" />
        <Rock:HiddenFieldWithClass ID="hfKioskType" CssClass="js-kiosk-type" runat="server" />

        <span style="display: none">
            <asp:LinkButton ID="lbCheckGeoLocation" runat="server" OnClick="lbCheckGeoLocation_Click"></asp:LinkButton>
        </span>

        <Rock:ModalAlert ID="maWarning" runat="server" />

        <div class="checkin-header">
            <h1>Check-in Options</h1>
        </div>

        <asp:Panel runat="server" CssClass="checkin-body">

            <div class="checkin-scroll-panel">
                <div class="scroller">
                    <Rock:NotificationBox ID="nbGeoMessage" runat="server" NotificationBoxType="Danger" />

                    <asp:Panel ID="pnlManualConfig" runat="server" Visible="false">
                        <Rock:RockDropDownList ID="ddlTheme" runat="server" CssClass="input-xlarge" Label="Theme" OnSelectedIndexChanged="ddlTheme_SelectedIndexChanged" AutoPostBack="true" />
                        <Rock:RockDropDownList ID="ddlKiosk" runat="server" CssClass="input-xlarge" Label="Kiosk Device" OnSelectedIndexChanged="ddlKiosk_SelectedIndexChanged" AutoPostBack="true" DataTextField="Name" DataValueField="Id" />
                        <Rock:RockDropDownList ID="ddlCheckinType" runat="server" CssClass="input-xlarge" Label="Check-in Configuration" OnSelectedIndexChanged="ddlCheckinType_SelectedIndexChanged" AutoPostBack="true" DataTextField="Name" DataValueField="Id" />

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockCheckBoxList ID="cblPrimaryGroupTypes" runat="server" Label="Check-in Area(s)" DataTextField="Name" DataValueField="Id"></Rock:RockCheckBoxList>
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBoxList ID="cblAlternateGroupTypes" runat="server" Label="Additional Area(s)" DataTextField="Name" DataValueField="Id"></Rock:RockCheckBoxList>
                            </div>
                        </div>
                        <asp:Panel ID="pnlHtml5CameraOptions" runat="server" CssClass="js-html5-camera-options">
                            <Rock:RockDropDownList ID="ddlHtml5Cameras" runat="server" CssClass="input-xlarge js-detected-html5-camera-list" Label="Select Camera for QR Code Scanning " onchange="handleHtml5CameraSelection(this);" />
                        </asp:Panel>
                    </asp:Panel>
                </div>
            </div>

        </asp:Panel>

        <div class="checkin-footer">
            <div class="checkin-actions">
                <asp:LinkButton CssClass="btn btn-primary" ID="lbOk" runat="server" OnClick="lbOk_Click" Text="OK" Visible="false" />
                <a class="btn btn-default" runat="server" id="lbRetry" visible="false" href="javascript:window.location.href=window.location.href">Retry</a>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
