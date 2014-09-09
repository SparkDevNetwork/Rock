<%@ control language="C#" autoeventwireup="true" inherits="RockWeb.Blocks.Reporting.Dashboard.LiquidDashboardWidget, RockWeb" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDashboardTitle" runat="server" CssClass="dashboard-title">
            <asp:Literal runat="server" ID="lDashboardTitle" />
        </asp:Panel>
        <asp:Panel ID="pnlDashboardSubtitle" runat="server" CssClass="dashboard-subtitle">
            <asp:Literal runat="server" ID="lDashboardSubtitle" />
        </asp:Panel>
        <asp:Panel ID="phHtml" runat="server" />

        <script type="text/javascript">
            Sys.Application.add_load(function () {
                $.ajax({
                    url: '<%=this.RestUrl %>',
                    dataType: 'json',
                    contentType: 'application/json'
                })
                .done(function (resultHtml) {
                    $('#<%=phHtml.ClientID%>').html(resultHtml);
                }).
                fail(function (a, b, c) {
                    debugger
                });
            });

        </script>

    </ContentTemplate>
</asp:UpdatePanel>
