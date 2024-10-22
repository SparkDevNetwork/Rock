<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DeviceDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.DeviceDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfAddLocationId.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upnlDevice" runat="server">
    <ContentTemplate>

        <%-- Edit Device Details --%>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block js-device-panel" runat="server" Visible="false">

            <asp:HiddenField ID="hfDeviceId" runat="server" />
            <asp:HiddenField ID="hfTypeId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-desktop"></i>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlInactive" runat="server" CssClass="js-inactivedevice-label" LabelType="Danger" Text="Inactive" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <fieldset>

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    <Rock:NotificationBox ID="nbDuplicateDevice" runat="server" NotificationBoxType="Warning" Title="Sorry" Visible="false" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.Device, Rock" PropertyName="Name" Required="true" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbIsActive" runat="server" CssClass="js-isactivedevice" Label="Active" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.Device, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbIpAddress" runat="server" SourceTypeName="Rock.Model.Device, Rock" PropertyName="IPAddress"
                                Help="What is the IP Address or Hostname of this device? Note: when using Hostname to match a Check-in Kiosk to this device, the 'Enable Kiosk Match By Name' setting on the check-in Admin block must be enabled." LabelTextFromPropertyName="false" Label="IP Address / Hostname" />
                            <asp:CustomValidator ID="cvIpAddress" runat="server" ControlToValidate="tbIpAddress" Display="None"
                                OnServerValidate="cvIpAddress_ServerValidate" ErrorMessage="IP address must be unique to the device type." />
                            <Rock:DefinedValuePicker ID="dvpDeviceType" runat="server" SourceTypeName="Rock.Model.Device, Rock" PropertyName="DeviceTypeValueId" Required="true" Label="Device Type"
                                Help="What type of device is this?" AutoPostBack="true" OnSelectedIndexChanged="ddlDeviceType_SelectedIndexChanged" />
                            <Rock:GeoPicker ID="geopPoint" runat="server" Required="false" Label="Point" DrawingMode="Point" />
                            <Rock:GeoPicker ID="geopFence" runat="server" Required="false" Label="Geo-fence" DrawingMode="Polygon" />
                        </div>
                        <div class="col-md-6">
                            <asp:Panel ID="pnlPrinterSettings" runat="server" Visible="true">
                                <div class="well">
                                    <h4>Print Settings</h4>
                                    <Rock:RockDropDownList ID="ddlPrintTo" runat="server" Label="Print Using" AutoPostBack="true" OnSelectedIndexChanged="ddlPrintTo_SelectedIndexChanged"
                                        Help="When this device needs to print, should it use the printer configured in next setting (Device Printer), the printer configured for the location (Location Printer), or should the Group Type's 'Print Using' setting determine the printer to use (Group Type)?">
                                        <asp:ListItem Text="Device Printer" Value="1" />
                                        <asp:ListItem Text="Location Printer" Value="2" />
                                        <asp:ListItem Text="Group Type" Value="0" />
                                    </Rock:RockDropDownList>
                                    <Rock:RockDropDownList ID="ddlPrinter" runat="server" Label="Printer" DataTextField="Name" DataValueField="Id"
                                        Help="The printer that this device should use for printing" EnhanceForLongLists="true" />
                                    <Rock:RockDropDownList ID="ddlPrintFrom" runat="server" Label="Print From" Required="false"
                                        Help="When this device needs to print, where should the printing be initiated from? Either the server running Rock, or from the actual client device? " />
                                </div>
                            </asp:Panel>

                            <Rock:RockRadioButtonList ID="rblKioskType" runat="server" Label="Kiosk Type" RepeatDirection="Horizontal" AutoPostBack="true" Required="true" OnSelectedIndexChanged="rblKioskType_SelectedIndexChanged" />
                            <Rock:RockCheckBox ID="cbHasCamera" runat="server" Label="Has Camera" Help="Determines if the device has a camera. This is currently only used for reading barcodes on kiosks." AutoPostBack="true" OnCheckedChanged="cbHasCamera_CheckedChanged" />
                            <Rock:RockDropDownList ID="ddlIPadCameraBarcodeConfigurationType" runat="server" Label="iPad Camera Barcode Configuration" Help="If set, this value will override any block setting for check-in." />

                            <Rock:RockDropDownList ID="ddlProxyDevice" runat="server" Label="Proxy Device" DataTextField="Name" DataValueField="Id" Help="The proxy device that will handle communicating with this printer when performing server based printing." />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:AttributeValuesContainer ID="avcAttributes" runat="server" />
                        </div>
                    </div>

                    <h3>Locations</h3>
                    <Rock:Grid ID="gLocations" runat="server" DisplayType="Light" RowItemText="Location" ShowConfirmDeleteDialog="false">
                        <Columns>
                            <Rock:RockBoundField DataField="LocationPath" HeaderText="Name" />
                            <Rock:DeleteField OnClick="gLocations_Delete" />
                        </Columns>
                    </Rock:Grid>

                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

        </asp:Panel>

        <%-- Location Modal --%>
        <Rock:ModalDialog ID="mdLocationPicker" runat="server" SaveButtonText="Save" OnSaveClick="btnAddLocation_Click" Title="Select Check-in Location" OnCancelScript="clearActiveDialog();" ValidationGroup="Location">
            <Content ID="mdLocationPickerContent">
                <asp:HiddenField ID="hfAddLocationId" runat="server" />
                <Rock:LocationItemPicker ID="locationPicker" runat="server" Label="Check-in Location" ValidationGroup="Location" />
            </Content>
        </Rock:ModalDialog>

          <script>

            Sys.Application.add_load(function () {
                function setIsActiveControls(activeCheckbox) {

                    var $inactiveLabel = $(activeCheckbox).closest(".js-device-panel").find('.js-inactivedevice-label');
                    if ($(activeCheckbox).is(':checked')) {
                        $inactiveLabel.hide();
                    }
                    else {
                        $inactiveLabel.show();
                    }
                }

                $('.js-isactivedevice').on('click', function () {
                    setIsActiveControls(this);
                });

                  $('.js-isactivedevice').each(function (i) {
                    setIsActiveControls(this);
                });

            });

        </script>

    </ContentTemplate>
</asp:UpdatePanel>
