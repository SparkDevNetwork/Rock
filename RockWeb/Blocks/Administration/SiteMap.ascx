<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SiteMap.ascx.cs" Inherits="SiteMap" %>

<asp:UpdatePanel ID="upMarketingCampaigns" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server">
            <div id="pages" >
                <asp:Literal ID="lPages" runat="server"></asp:Literal>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
