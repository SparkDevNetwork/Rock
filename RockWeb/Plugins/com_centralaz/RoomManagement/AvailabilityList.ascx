<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AvailabilityList.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.RoomManagement.AvailabilityList" %>
<%@ Register TagPrefix="CentralAZ" Assembly="com.centralaz.RoomManagement" Namespace="com.centralaz.RoomManagement.Web.UI.Controls" %>
<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">Available Resources</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfSettings" runat="server">
                        <Rock:DateTimePicker ID="dtpStartDateTime" runat="server" Label="Start Date" />
                        <Rock:RockRadioButtonList ID="rblResourceLocation" runat="server" RepeatDirection="Vertical" CssClass="inputs-list" Label="Search For"
                            OnSelectedIndexChanged="rblResourceLocation_SelectedIndexChanged" AutoPostBack="true">
                            <asp:ListItem Value="Location" Text="Locations" Selected="True"></asp:ListItem>
                            <asp:ListItem Value="Resource" Text="Resources"></asp:ListItem>
                        </Rock:RockRadioButtonList>
                        <Rock:CategoryPicker ID="cpResource" runat="server" Label="Resource Category" Visible="false" EntityTypeName="com.centralaz.RoomManagement.Model.Resource" />
                        <Rock:LocationItemPicker ID="lipLocation" runat="server" Label="Parent Location" />
                        <Rock:DateTimePicker ID="dtpEndDateTime" runat="server" Label="End Date" />
                        <Rock:NumberBox ID="nbMaxOccupants" runat="server" Label="Expected Occupants" NumberType="Integer" MinimumValue="0" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gResources" runat="server" RowItemText="Reservation" OnRowSelected="gResources_RowSelected" TooltipField="Description" Visible="true">
                        <Columns>
                            <Rock:RockBoundField DataField="Id" HeaderText="Id" Visible="false" />
                            <Rock:RockBoundField DataField="IsAvailable" HeaderText="IsAvailable" Visible="false" />
                            <Rock:RockTemplateField HeaderText="Name">
                                <ItemTemplate><%# Convert.ToString( Eval( "LocationName" ) ) == string.Empty ? Convert.ToString( Eval( "Name" ) ) : Convert.ToString( Eval( "Name" ) ) + " <em class='text-muted'>(attached to " + Eval( "LocationName" ) + ")" %></em></ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockBoundField DataField="Availability" HeaderText="Availability" HtmlEncode="false" />
                        </Columns>
                    </Rock:Grid>
                    <Rock:Grid ID="gLocations" runat="server" RowItemText="Location" OnRowSelected="gLocations_RowSelected" TooltipField="Description" Visible="false">
                        <Columns>
                            <Rock:RockBoundField DataField="Id" HeaderText="Id" Visible="false" />
                            <Rock:RockBoundField DataField="IsAvailable" HeaderText="IsAvailable" Visible="false" />
                            <Rock:RockBoundField DataField="Name" HeaderText="Location" HtmlEncode="false" />
                            <Rock:RockBoundField DataField="MaxOccupants" HeaderText="Maximum Occupants" HtmlEncode="false" />
                            <Rock:RockBoundField DataField="Availability" HeaderText="Availability" HtmlEncode="false" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
