<%@ Control Language="C#" AutoEventWireup="true" CodeFile="NotificationGroupList.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor.NotificationGroupList" %>
<%@ Register Namespace="com.blueboxmoon.WatchdogMonitor.Web.UI.Controls" Assembly="com.blueboxmoon.WatchdogMonitor" TagPrefix="WD" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlNotificationGroupList" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-envelope"></i> Notification Groups</h1>
            </div>

            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gNotificationGroup" runat="server" AllowSorting="true" OnRowSelected="gNotificationGroup_RowSelected" OnRowDataBound="gNotificationGroup_RowDataBound" RowItemText="Notification Group">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="Schedule.Name" HeaderText="Schedule" SortExpression="Schedule" />
                            <Rock:DeleteField OnClick="gNotificationGroup_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
