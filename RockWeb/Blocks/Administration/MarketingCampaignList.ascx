<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MarketingCampaignList.ascx.cs" Inherits="RockWeb.Blocks.Administration.MarketingCampaignList" %>

<asp:UpdatePanel ID="upMarketingCampaigns" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        <Rock:Grid ID="gMarketingCampaigns" runat="server" AllowSorting="true" OnRowSelected="gMarketingCampaigns_Edit">
            <Columns>
                <asp:BoundField DataField="Title" HeaderText="Title" SortExpression="Title" />
                <asp:BoundField DataField="EventGroup.Name" HeaderText="Event Group" SortExpression="EventGroup.Name" />
                <asp:BoundField DataField="ContactFullName" HeaderText="Contact" SortExpression="ContactFullName" />
                <Rock:DeleteField OnClick="gMarketingCampaigns_Delete" />
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>
