<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SiteMap.ascx.cs" Inherits="SiteMap" %>

<asp:UpdatePanel ID="upMarketingCampaigns" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server">
            <div id="pages">
                <asp:Literal ID="lPages" runat="server"></asp:Literal>
            </div>
        </asp:Panel>

        <script>
            $(function () {
                $('#pages')
                    .on('rockTree:selected', function (e, id) {
                        // TODO: Redirect on select if page.
                        // TODO: Open modal on select if block.
                    })
                    .rockTree();
            });
        </script>

    </ContentTemplate>
</asp:UpdatePanel>

