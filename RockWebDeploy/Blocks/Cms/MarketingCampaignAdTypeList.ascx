<%@ control language="C#" autoeventwireup="true" inherits="RockWeb.Blocks.Cms.MarketingCampaignAdTypeList, RockWeb" %>

<asp:UpdatePanel ID="upMarketingCampaignAdType" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-lightbulb-o"></i> Ad Types List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gMarketingCampaignAdType" runat="server" AllowSorting="true" OnRowSelected="gMarketingCampaignAdType_Edit">
                        <Columns>
                            <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                            <Rock:DeleteField OnClick="gMarketingCampaignAdType_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
