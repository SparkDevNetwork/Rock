<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConnectionRequests.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.ConnectionRequests" %>
<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <div class="card card-profile overflow-hidden">
            <div class="card-header">Connection Requests</div>
            <asp:Repeater ID="rConnectionTypes" runat="server" OnItemDataBound="rConnectionTypes_ItemDataBound">
                <ItemTemplate>
                    <asp:Literal runat="server" ID="lConnectionOpportunityList" />
                </ItemTemplate>
                <FooterTemplate>
                    <asp:Label ID="lblNoConnectionsFound" runat="server" Visible='<%# rConnectionTypes.Items.Count == 0 %>' Text="" CssClass="card-body" />
                </FooterTemplate>
            </asp:Repeater>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
