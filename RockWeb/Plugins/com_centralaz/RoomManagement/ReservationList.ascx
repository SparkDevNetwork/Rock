<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReservationList.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.RoomManagement.ReservationList" %>
<%@ Register TagPrefix="CentralAZ" Assembly="com.centralaz.RoomManagement" Namespace="com.centralaz.RoomManagement.Web.UI.Controls" %>
<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">Reservation List</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfSettings" runat="server">
                        <Rock:RockTextBox ID="tbName" runat="server" Label="Reservation Name" />
                        <Rock:RockDropDownList ID="ddlMinistry" runat="server" Label="Ministry" DataTextField="Name" DataValueField="Id" />
                        <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" DataTextField="Name" DataValueField="Id" />
                        <Rock:DateTimePicker ID="dtpStartDateTime" runat="server" Label="Start Date" />
                        <Rock:DateTimePicker ID="dtpEndDateTime" runat="server" Label="End Date" />
                        <Rock:PersonPicker ID="ppCreator" runat="server" Label="Created By" />
                        <CentralAZ:ResourcePicker ID="rpResource" runat="server" Label="Resources" AllowMultiSelect="true" />
                        <Rock:LocationItemPicker ID="lipLocation" runat="server" Label="Locations" AllowMultiSelect="true" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gReservations" runat="server" RowItemText="Reservation" OnRowSelected="gReservations_Edit" TooltipField="Description">
                        <Columns>
                            <Rock:RockBoundField DataField="Id" HeaderText="Id" Visible="false" />
                            <Rock:RockBoundField DataField="ReservationName" HeaderText="Reservation Name" />
                            <Rock:RockBoundField DataField="EventDateTimeDescription" HeaderText="Event Time" />
                            <Rock:RockBoundField DataField="ReservationDateTimeDescription" HeaderText="Reservation Time" />
                            <Rock:RockBoundField DataField="Locations" HeaderText="Locations" />
                            <Rock:RockBoundField DataField="Resources" HeaderText="Resources" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
