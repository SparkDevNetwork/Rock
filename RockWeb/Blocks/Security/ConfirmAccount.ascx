<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConfirmAccount.ascx.cs" Inherits="RockWeb.Blocks.Security.ConfirmAccount" %>
<asp:UpdatePanel runat="server">
<ContentTemplate>

    <asp:Panel ID="pnlConfirmed" runat="server" Visible="false">
        <asp:Literal ID="lConfirmed" runat="server"></asp:Literal>
    </asp:Panel>

    <asp:Panel ID="pnlDelete" runat="server" Visible="false">

        <asp:Literal ID="lDelete" runat="server"></asp:Literal>

        <div class="actions">
            <asp:Button ID="btnDelete" runat="server" Text="Next" CssClass="btn primary" OnClick="btnDelete_Click" />
        </div>

    </asp:Panel>

    <asp:Panel ID="pnlDeleted" runat="server" Visible="false">
        <asp:Literal ID="lDeleted" runat="server"></asp:Literal>
    </asp:Panel>

    <asp:Panel ID="pnlInvalid" runat="server" Visible="false">
        <asp:Literal ID="lInvalid" runat="server"></asp:Literal>
    </asp:Panel>

</ContentTemplate>
</asp:UpdatePanel>
