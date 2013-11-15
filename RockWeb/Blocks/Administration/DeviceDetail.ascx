<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DeviceDetail.ascx.cs" Inherits="RockWeb.Blocks.Administration.DeviceDetail" %>

<asp:UpdatePanel ID="upDevice" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server" CssClass="panel panel-default" Visible="false">
            
            <asp:HiddenField ID="hfDeviceId" runat="server" />

            <div class="panel-body">

                <div class="banner"><h1><asp:Literal ID="lActionTitle" runat="server" /></h1></div>

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <fieldset>

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.Device, Rock" PropertyName="Name" Required="true" />
                        </div>
                        <div class="col-md-6">
                            
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
                                Help="What is the IP Address of this device?" />
                            <Rock:DataDropDownList ID="ddlDeviceType" runat="server" SourceTypeName="Rock.Model.Device, Rock" PropertyName="DeviceTypeValueId" Required="true"
                                Help="What type of device is this?" />

                            <Rock:GeoPicker ID="gpGeoPoint" runat="server" Required="false" Label="Geo Point" DrawingMode="Point" />
                            <Rock:GeoPicker ID="gpGeoFence" runat="server" Required="false" Label="Geo Fence" DrawingMode="Polygon" />
                            
                        </div>
                        <div class="col-md-6">
                            <Rock:DataDropDownList ID="ddlPrintTo" runat="server" SourceTypeName="Rock.Model.Device, Rock" PropertyName="PrintToOverride" Required="false" 
                                Help="When this device needs to print, should printing be done to this device's printer (Kiosk), the printer defined by the location using this device (Location), or should this decision be deferred to the Group Type using the device (Default)?" />
                            <Rock:DataDropDownList ID="ddlPrinter" runat="server" SourceTypeName="Rock.Model.Device, Rock" PropertyName="PrinterDeviceId" Required="false" 
                                Help="The printer that this device should use for printing" DataTextField="Name" DataValueField="Id" />
                            <Rock:DataDropDownList ID="ddlPrintFrom" runat="server" SourceTypeName="Rock.Model.Device, Rock" PropertyName="PrintFrom" Required="false" 
                                Help="If printing is being sent to this device's printer, where should the printing be initiated from?" />
                        </div>
                    </div>

                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
