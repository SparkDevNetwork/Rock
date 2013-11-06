<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MarketingCampaignAdTypeList.ascx.cs" Inherits="RockWeb.Blocks.Administration.MarketingCampaignAdTypeList" %>

<asp:UpdatePanel ID="upMarketingCampaignAdType" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        <Rock:Grid ID="gMarketingCampaignAdType" runat="server" AllowSorting="true" OnRowSelected="gMarketingCampaignAdType_Edit">
            <Columns>
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                <Rock:DeleteField OnClick="gMarketingCampaignAdType_Delete" />
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>
