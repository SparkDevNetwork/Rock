﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReservationList.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.RoomManagement.ReservationList" %>
<%@ Register TagPrefix="BEMA" Assembly="com.bemaservices.RoomManagement" Namespace="com.bemaservices.RoomManagement.Web.UI.Controls" %>
<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">Reservation List</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:NotificationBox ID="nbMessage" runat="server" NotificationBoxType="Warning" Visible="false"></Rock:NotificationBox>
                    <Rock:GridFilter ID="gfSettings" runat="server">
                        <Rock:RockTextBox ID="tbName" runat="server" Label="Reservation Name" />
                        <Rock:RockCheckBoxList ID="cblReservationType" RepeatDirection="Horizontal" Label="Reservation Type" runat="server" DataTextField="Name" DataValueField="Id" />
                        <Rock:RockCheckBoxList ID="cblMinistry" RepeatDirection="Horizontal" Label="Ministry" runat="server" DataTextField="Name" DataValueField="Id" />
                        <Rock:RockCheckBoxList ID="cblApproval" runat="server" Label="Approval Status" RepeatDirection="Horizontal" />
                        <Rock:DateTimePicker ID="dtpStartDateTime" runat="server" Label="Start Date" />
                        <Rock:DateTimePicker ID="dtpEndDateTime" runat="server" Label="End Date" />
                        <Rock:PersonPicker ID="ppCreator" runat="server" Label="Created By" EnableSelfSelection="true" />
                        <BEMA:ResourcePicker ID="rpResource" runat="server" Label="Resources" AllowMultiSelect="true" />
                        <Rock:LocationItemPicker ID="lipLocation" runat="server" Label="Locations" AllowMultiSelect="true" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gReservations" runat="server" RowItemText="Reservation" OnRowSelected="gReservations_Edit" TooltipField="Description">
                        <Columns>
                            <Rock:RockBoundField DataField="Id" HeaderText="Id" Visible="false" />
                            <Rock:RockBoundField DataField="ReservationName" HeaderText="Reservation Name" />
                            <Rock:RockBoundField DataField="ReservationType" HeaderText="Reservation Type" />
                            <Rock:RockBoundField DataField="EventDateTimeDescription" HeaderText="Event Time" />
                            <Rock:RockBoundField DataField="ReservationDateTimeDescription" HeaderText="Reservation Time" />
                            <Rock:RockBoundField DataField="Locations" HeaderText="Locations" />
                            <Rock:RockBoundField DataField="Resources" HeaderText="Resources" />
                            <Rock:RockBoundField DataField="ApprovalState" HeaderText="Approval State" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
