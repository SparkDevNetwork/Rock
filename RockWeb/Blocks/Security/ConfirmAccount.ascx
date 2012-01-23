<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConfirmAccount.ascx.cs" Inherits="RockWeb.Blocks.Security.ConfirmAccount" %>
<asp:UpdatePanel runat="server">
<ContentTemplate>

    <asp:Panel ID="pnlCode" runat="server" Visible="false">

        <asp:Panel ID="pnlInvalid" runat="server" CssClass="alert-message error">
            <asp:Literal ID="lInvalid" runat="server" ></asp:Literal> 
        </asp:Panel>

        <fieldset>
            <legend>Enter Code</legend>
            <Rock:LabeledTextBox ID="tbConfirmationCode" runat="server" LabelText="Code" Required="true" ></Rock:LabeledTextBox>
        </fieldset>

        <div class="actions">
            <asp:Button ID="btnCodeConfirm" runat="server" Text="Confirm Account" CssClass="btn primary" OnClick="btnCodeConfirm_Click" />
            <asp:Button ID="btnCodeReset" runat="server" Text="Change Password" CssClass="btn secondary" OnClick="btnCodeReset_Click" />
            <asp:Button ID="btnCodeDelete" runat="server" Text="Delete Account" CssClass="btn secondary" OnClick="btnCodeDelete_Click" />
        </div>

    </asp:Panel>
    
    <asp:Panel ID="pnlConfirmed" runat="server" Visible="false" CssClass="alert-message success">
        <asp:Literal ID="lConfirmed" runat="server"></asp:Literal>
    </asp:Panel>

    <asp:Panel ID="pnlResetPassword" runat="server" Visible="false">
    
        <asp:Literal ID="lResetPassword" runat="server"></asp:Literal>

        <fieldset>
            <legend>Enter New Password</legend>
            <Rock:LabeledTextBox ID="tbPassword" runat="server" LabelText="New Password" Required="true" ></Rock:LabeledTextBox>
            <Rock:LabeledTextBox ID="tbPasswordConfirm" runat="server" LabelText="Confirm Password" Required="true" ></Rock:LabeledTextBox>
        </fieldset>

        <div class="actions">
            <asp:Button ID="btnResetPassword" runat="server" Text="Change Password" CssClass="btn primary" OnClick="btnResetPassword_Click" />
        </div>

    </asp:Panel>

    <asp:Panel ID="pnlResetSuccess" runat="server" Visible="false" CssClass="alert-message success">
        <asp:Literal ID="lResetSuccess" runat="server"></asp:Literal>
    </asp:Panel>

    <asp:Panel ID="pnlDelete" runat="server" Visible="false">

        <asp:Literal ID="lDelete" runat="server"></asp:Literal>

        <div class="actions">
            <asp:Button ID="btnDelete" runat="server" Text="Yes, Delete the Account" CssClass="btn primary" OnClick="btnDelete_Click" />
        </div>

    </asp:Panel>

    <asp:Panel ID="pnlDeleted" runat="server" Visible="false">
        <asp:Literal ID="lDeleted" runat="server"></asp:Literal>
    </asp:Panel>

</ContentTemplate>
</asp:UpdatePanel>
