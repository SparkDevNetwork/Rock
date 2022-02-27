<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DeviceList.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor.DeviceList" %>
<%@ Register Namespace="com.blueboxmoon.WatchdogMonitor.Web.UI.Controls" Assembly="com.blueboxmoon.WatchdogMonitor" TagPrefix="WM" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDeviceList" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-list-ul"></i> Devices</h1>
            </div>

            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gDevice" runat="server" AllowSorting="true" OnRowSelected="gDevice_RowSelected" RowItemText="Device">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="Address" HeaderText="Address" SortExpression="Address" />
                            <Rock:RockBoundField DataField="ParentDevice.Name" HeaderText="Parent" SortExpression="ParentDevice.Name" />
                            <Rock:RockBoundField DataField="DeviceProfile.Name" HeaderText="Profile" SortExpression="DeviceProfile.Name" />
                            <Rock:DeleteField OnClick="gDevice_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
