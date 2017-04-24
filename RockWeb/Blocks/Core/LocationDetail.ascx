<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LocationDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.LocationDetail" %>

<asp:UpdatePanel ID="upnlLocationList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server">
            <asp:HiddenField ID="hfLocationId" runat="server" />

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-map-marker"></i> <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
                    
                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                        <Rock:HighlightLabel ID="hlType" runat="server" LabelType="Type" />
                    </div>
                </div>
                <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
                <div class="panel-body">

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                    <asp:CustomValidator ID="cvLocation" runat="server" Display="None" />

                    <div id="pnlEditDetails" runat="server">

                        <div class="row">
                            <div id="divAdvSettings" runat="server" class="col-md-6">
                                <Rock:LocationPicker ID="gpParentLocation" runat="server" Required="false" Label="Parent Location" AllowedPickerModes="Named" />
                                <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.Location, Rock" PropertyName="Name" />
                                <Rock:RockDropDownList ID="ddlLocationType" runat="server" DataTextField="Value" DataValueField="Id" Label="Location Type" AutoPostBack="true" OnSelectedIndexChanged="ddlLocationType_SelectedIndexChanged"  />
                                <Rock:RockDropDownList ID="ddlPrinter" runat="server" Label="Printer" DataTextField="Name" DataValueField="Id" 
                                    Help="The printer that this location should use for printing" />
                                <Rock:ImageEditor ID="imgImage" runat="server" Label="Image" BinaryFileTypeGuid="DAB74416-3272-4411-BA69-70944B549A4B" />
                                <Rock:DynamicPlaceHolder ID="phAttributeEdits" runat="server" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbIsActive" runat="server" Text="Active" />
                                <Rock:AddressControl ID="acAddress" runat="server" UseStateAbbreviation="true" ShowCounty="true" />
                                <asp:Button ID="btnStandardize" runat="server" OnClick="btnStandardize_Click" Text="Verify Address" CssClass="btn btn-action margin-b-md" />
                                <asp:Literal ID="lStandardizationUpdate" runat="server" />
                                <Rock:RockCheckBox ID="cbGeoPointLocked" runat="server" Label="Location Locked" Text="Yes" Help="Locks the location to prevent verification services (standardization/geocoding) from updating the address or point." />
                                <div class="row">
                                    <div class="col-sm-7">
                                        <Rock:GeoPicker ID="geopPoint" runat="server" DrawingMode="Point" Label="Point" />
                                        <Rock:GeoPicker ID="geopFence" runat="server" DrawingMode="Polygon" Label="Geo-fence" />
                                    </div>
                                    <div class="col-sm-5">
                                        <Rock:NumberBox ID="nbSoftThreshold" runat="server" MinimumValue="0" NumberType="Integer" Label="Threshold"
                                            Help="The maximum number of people that room allows before a check-in will require a manager override." />
                                        <Rock:NumberBox ID="nbFirmThreshold" runat="server" MinimumValue="0" NumberType="Integer" Label="Threshold (Absolute)"
                                            Help="The absolute maximum number of people that room allows. Check-in will not allow check-in after this number of people have checked in." />
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="actions">
                            <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                            <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                        </div>

                    </div>

                    <fieldset id="fieldsetViewDetails" runat="server">

                        <div class="row">
                            <div class="col-md-3">
                                <asp:Literal ID="lImage" runat="server" />
                            </div>
                            <div class="col-md-5">
                                <asp:Literal ID="lblMainDetails" runat="server" />
                                <asp:PlaceHolder ID="phAttributes" runat="server"></asp:PlaceHolder>
                            </div>
                            <div class="col-md-4 location-maps">
                                <asp:PlaceHolder ID="phMaps" runat="server" />
                            </div>
                        </div>

                        <div class="actions">
                            <asp:LinkButton ID="btnEdit" runat="server" AccessKey="m" ToolTip="Alt+m" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                            <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                            <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
                            <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-security pull-right" />
                        </div>

                    </fieldset>

                </div>
            </div>


            
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
