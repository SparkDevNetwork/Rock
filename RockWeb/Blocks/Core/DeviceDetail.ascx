<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DeviceDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.DeviceDetail" %>

<asp:UpdatePanel ID="upnlDevice" runat="server">
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

                            <Rock:GeoPicker ID="geopPoint" runat="server" Required="false" Label="Point" DrawingMode="Point" />
                            <Rock:GeoPicker ID="geopFence" runat="server" Required="false" Label="Geo-fence" DrawingMode="Polygon" />
                            
                        </div>
                        <div class="col-md-6">
                            <div class="well">
                                <h4>Print Settings</h4>
                                <Rock:RockDropDownList ID="ddlPrintTo" runat="server" Label="Print Using" AutoPostBack="true" OnSelectedIndexChanged="ddlPrintTo_SelectedIndexChanged"
                                    Help="When this device needs to print, should it use the printer configured in next setting (Device Printer), the printer configured for the location (Location Printer), or should the Group Type's 'Print Using' setting determine the printer to use (Group Type)?">
                                    <asp:ListItem Text="Device Printer" Value="1" />
                                    <asp:ListItem Text="Location Printer" Value="2" />
                                    <asp:ListItem Text="Group Type" Value="0" />
                                </Rock:RockDropDownList>
                                <Rock:RockDropDownList ID="ddlPrinter" runat="server" Label="Printer" DataTextField="Name" DataValueField="Id" 
                                    Help="The printer that this device should use for printing" />
                                <Rock:RockDropDownList ID="ddlPrintFrom" runat="server" Label="Print From" Required="false" 
                                    Help="When this device needs to print, where should the printing be initiated from?  Either the server running Rock, or from the actual client device? " />
                            </div>
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
