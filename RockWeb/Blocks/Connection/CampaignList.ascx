<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CampaignList.ascx.cs" Inherits="RockWeb.Blocks.Connection.CampaignList" %>
<asp:UpdatePanel ID="pnlAssetStorageProviderListUpdatePanel" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-plug"></i> Connection Campaigns
                </h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:Grid ID="gCampaigns" runat="server" RowItemText="Connection Campaign" OnRowSelected="gCampaigns_RowSelected" AllowSorting="true">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Campaign Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="DataView.Name" HeaderText="DataView" SortExpression="DataView.Name" />
                            <Rock:RockBoundField DataField="ConnectionOpportunity.Name" HeaderText="Connection Opportunity" SortExpression="ConnectionOpportunity.Name" />
                            <Rock:RockBoundField DataField="ActiveRequests" HeaderText="Active Requests" SortExpression="ActiveRequests" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                            <Rock:RockBoundField DataField="PendingConnections" HeaderText="Pending Connections" SortExpression="PendingConnections" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                            <Rock:DeleteField OnClick="gCampaigns_DeleteClick" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>