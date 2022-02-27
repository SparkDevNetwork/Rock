<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MonitorDashboard.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor.MonitorDashboard" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Literal ID="lContent" runat="server" />

        <asp:HiddenField ID="hfRefreshPeriod" runat="server" />
        <asp:LinkButton ID="lbRefresh" runat="server" CssClass="hidden" OnClick="lbRefresh_Click" />
    </ContentTemplate>
</asp:UpdatePanel>

<script>
    $(document).ready(function () {
        function refresh() {
            console.log('Update');
            $('#<%= lbRefresh.ClientID %>').get(0).click();
            scheduleRefresh();
        }
        function scheduleRefresh() {
            var period = $('#<%= hfRefreshPeriod.ClientID %>').val();
            if (period > 0) {
                setTimeout(function () {
                    refresh();
                }, period * 1000);
            }
            else {
                setTimeout(function () {
                    scheduleRefresh();
                }, 1000);
            }
        }

        scheduleRefresh();
    });
</script>