<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MarketingCampaignAdList.ascx.cs" Inherits="RockWeb.Blocks.Administration.MarketingCampaignAdList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <Rock:GridFilter ID="rFilter" runat="server" >
            <%-- Approval Status, Priority Range, Ad Type, Date Range --%>
            <Rock:LabeledDropDownList ID="ddlApprovalStatus" runat="server" LabelText="Approval Status" />
            <Rock:NumberRangeEditor ID="pPriorityRange" runat="server" LabelText="Priority Range" />
            <Rock:LabeledDropDownList ID="ddlAdType" runat="server" LabelText="Ad Type" />
            <Rock:DateRangePicker ID="pDateRange" runat="server" LabelText="Date Range" />
        </Rock:GridFilter>
        <div id="pnlMarketingCampaignAds" runat="server">
            <asp:HiddenField ID="hfMarketingCampaignId" runat="server" />
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
            <Rock:Grid ID="gMarketingCampaignAds" runat="server" DisplayType="Full" OnRowSelected="gMarketingCampaignAds_Edit" AllowSorting="true">
                <Columns>
                    <asp:BoundField DataField="MarketingCampaign.Title" HeaderText="Campaign" SortExpression="MarketingCampaign.Title" />
                    <asp:BoundField DataField="MarketingCampaignAdType.Name" HeaderText="Ad Type" SortExpression="MarketingCampaignAdType.Name" />
                    <Rock:DateField DataField="StartDate" HeaderText="Date" SortExpression="StartDate" />
                    <Rock:EnumField DataField="MarketingCampaignAdStatus" HeaderText="Approval Status" SortExpression="MarketingCampaignAdStatus" />
                    <asp:BoundField DataField="Priority" HeaderText="Priority" SortExpression="Priority" />
                    <Rock:DeleteField OnClick="gMarketingCampaignAds_Delete" />
                </Columns>
            </Rock:Grid>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
