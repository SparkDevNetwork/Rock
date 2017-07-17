<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionLinks.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.TransactionLinks" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">
            <asp:LinkButton ID="btnAddTransaction" runat="server" CssClass="btn btn-default btn-block" Text="Add One-time Gift" OnClick="btnAddTransaction_Click" />
            <asp:LinkButton ID="btnAddScheduledTransaction" runat="server" CssClass="btn btn-default btn-block" Text="New Scheduled Transaction" OnClick="btnAddScheduledTransaction_Click" />
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>