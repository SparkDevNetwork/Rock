<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CommunicationDetail.ascx.cs" Inherits="RockWeb.Blocks.Communication.CommunicationDetail" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
 
        <div class="banner">
            <h1><asp:Literal ID="lTitle" runat="server"></asp:Literal></h1>
            <Rock:HighlightLabel ID="hlStatus" runat="server" />
        </div>

        <asp:Panel ID="pnlDetails" runat="server">

            <div class="recipient-status">
                <a id="aPending" runat="server" class="btn btn-lg btn-default"><asp:Literal ID="lPending" runat="server"></asp:Literal><br /><small>Pending</small></a>
                <a id="aDelivered" runat="server" class="btn btn-lg btn-info"><asp:Literal ID="lDelivered" runat="server"></asp:Literal><br /><small>Delivered</small></a>
                <a id="aFailed" runat="server" class="btn btn-lg btn-danger"><asp:Literal ID="lFailed" runat="server"></asp:Literal><br /><small>Failed</small></a>
                <a id="aCanceled" runat="server" class="btn btn-lg btn-warning"><asp:Literal ID="lCancelled" runat="server"></asp:Literal><br /><small>Cancelled</small></a>
                <a id="aOpened" runat="server" class="btn btn-lg btn-success" disabled="disabled"><asp:Literal ID="lOpened" runat="server"></asp:Literal><br /><small>Opened</small></a>
            </div>

            <Rock:PanelWidget ID="wpMessageDetails" runat="server" Title="Message Details">
                <asp:Literal ID="lDetails" runat="server" />
            </Rock:PanelWidget>

            <Rock:PanelWidget ID="wpEvents" runat="server" Title="Events" Expanded="false">
            </Rock:PanelWidget>

            <div class="actions">
                <asp:LinkButton ID="btnApprove" runat="server" Text="Approve" CssClass="btn btn-primary" OnClick="btnApprove_Click" />
                <asp:LinkButton ID="btnDeny" runat="server" Text="Deny" CssClass="btn btn-link" OnClick="btnDeny_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel Send" CssClass="btn btn-link" OnClick="btnCancel_Click" />
                <asp:LinkButton ID="btnCopy" runat="server" Text="Copy Communication" CssClass="btn btn-link" OnClick="btnCopy_Click" />
            </div>

        </asp:Panel>

        <asp:Panel ID="pnlResult" runat="server" Visible="false">
            <Rock:NotificationBox ID="nbResult" runat="server" NotificationBoxType="Success" />
            <br />
            <asp:HyperLink ID="hlViewCommunication" runat="server" Text="View Communication" />
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>


