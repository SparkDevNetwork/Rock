<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DowntimeList.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor.DowntimeList" %>
<%@ Register Namespace="com.blueboxmoon.WatchdogMonitor.Web.UI.Controls" Assembly="com.blueboxmoon.WatchdogMonitor" TagPrefix="WM" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Danger" Visible="true" />

        <asp:Panel ID="pnlDowntimeList" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-list-ul"></i> Downtime List</h1>
            </div>

            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gDowntime" runat="server" AllowSorting="true" OnRowSelected="gDowntime_RowSelected" RowItemText="Downtime" OnRowDataBound="gDowntime_RowDataBound">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="Schedule.Name" HeaderText="Schedule" />
                            <Rock:BoolField DataField="IsActive" HeaderText="IsActive" SortExpression="IsActive" />
                            <Rock:DeleteField OnClick="gDowntime_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
