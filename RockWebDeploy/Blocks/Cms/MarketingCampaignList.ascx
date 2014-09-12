<%@ control language="C#" autoeventwireup="true" inherits="RockWeb.Blocks.Cms.MarketingCampaignList, RockWeb" %>

<asp:UpdatePanel ID="upMarketingCampaigns" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-bullhorn"></i> Ad Campaign List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gMarketingCampaigns" runat="server" AllowSorting="true" OnRowSelected="gMarketingCampaigns_Edit">
                        <Columns>
                            <asp:BoundField DataField="Title" HeaderText="Title" SortExpression="Title" />
                            <asp:BoundField DataField="EventGroupName" HeaderText="Event Group" SortExpression="EventGroupName" />
                            <asp:BoundField DataField="ContactFullName" HeaderText="Contact" SortExpression="ContactFullName" />
                            <Rock:DeleteField OnClick="gMarketingCampaigns_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
