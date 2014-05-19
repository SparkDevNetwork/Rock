<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LiquidDashboardWidget.ascx.cs" Inherits="RockWeb.Blocks.Reporting.Dashboard.LiquidDashboardWidget" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="phHtml" runat="server" />

        <script type="text/javascript">
            Sys.Application.add_load(function () {
                $.ajax({
                    url: '<%=ResolveUrl("~/api/Metrics/GetHtml/") + this.BlockId.ToString() %>',
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
