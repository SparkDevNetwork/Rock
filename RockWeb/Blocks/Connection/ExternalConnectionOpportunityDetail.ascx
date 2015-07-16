<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ExternalConnectionOpportunityDetail.ascx.cs" Inherits="RockWeb.Blocks.Connection.ExternalConnectionOpportunityDetail" %>
<asp:UpdatePanel ID="upnlOpportunityDetail" runat="server">
    <ContentTemplate>
        <h3>
            <asp:Literal ID="lIcon" runat="server" />
            <asp:Literal ID="lTitle" runat="server" />
        </h3>
        <asp:Panel ID="pnlDetails" runat="server">
            <div class="panel panel-block">
                <div class="panel-body">

                    <p class="description">
                        <asp:Literal ID="lDescription" runat="server"></asp:Literal>
                    </p>

                    <asp:PlaceHolder ID="phAttributes" runat="server"></asp:PlaceHolder>

                    <div class="actions">
                        <asp:LinkButton ID="btnConnect" runat="server" AccessKey="m" Text="Connect" CssClass="btn btn-primary" OnClick="btnConnect_Click" CausesValidation="false" />
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
