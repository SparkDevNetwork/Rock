<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LiquidDashboardWidget.ascx.cs" Inherits="RockWeb.Blocks.Reporting.Dashboard.LiquidDashboardWidget" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="dashboard-title">
            <asp:Literal runat="server" ID="lDashboardTitle" />
        </div>
        <div class="dashboard-subtitle">
            <asp:Literal runat="server" ID="lDashboardSubtitle" />
        </div>
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
