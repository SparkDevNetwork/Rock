<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MarketingCampaignAdList.ascx.cs" Inherits="RockWeb.Blocks.Administration.MarketingCampaignAdList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <div id="pnlMarketingCampaignAds" runat="server" class="">
            <asp:HiddenField ID="hfMarketingCampaignId" runat="server" />
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
            <Rock:Grid ID="gMarketingCampaignAds" runat="server" DisplayType="Full" OnRowSelected="gMarketingCampaignAds_Edit">
                <Columns>
                    <asp:BoundField DataField="MarketingCampaignAdTypeName" HeaderText="Ad Type" />
                    <Rock:DateField DataField="StartDate" HeaderText="Date" />
                    <Rock:EnumField DataField="MarketingCampaignAdStatus" HeaderText="Approval Status" />
                    <asp:BoundField DataField="Priority" HeaderText="Priority" />
                    <Rock:DeleteField OnClick="gMarketingCampaignAds_Delete" />
                </Columns>
            </Rock:Grid>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
