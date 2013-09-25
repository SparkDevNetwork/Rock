<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConfirmAccount.ascx.cs" Inherits="RockWeb.Blocks.Security.ConfirmAccount" %>
<asp:UpdatePanel runat="server">
<ContentTemplate>

    <asp:Panel ID="pnlCode" runat="server" Visible="false">

        <asp:Panel ID="pnlInvalid" runat="server" CssClass="alert alert-error error">
            <asp:Literal ID="lInvalid" runat="server" ></asp:Literal> 
        </asp:Panel>

        <fieldset>
            <legend>Enter Code</legend>
            <Rock:RockTextBox ID="tbConfirmationCode" runat="server" Label="Code" Required="true" ></Rock:RockTextBox>
        </fieldset>

        <div class="actions">
            <asp:Button ID="btnCodeConfirm" runat="server" Text="Confirm Account" CssClass="btn btn-primary" OnClick="btnCodeConfirm_Click" />
            <asp:Button ID="btnCodeReset" runat="server" Text="Change Password" CssClass="btn" OnClick="btnCodeReset_Click" />
            <asp:Button ID="btnCodeDelete" runat="server" Text="Delete Account" CssClass="btn" OnClick="btnCodeDelete_Click" />
        </div>

    </asp:Panel>
    
    <asp:Panel ID="pnlConfirmed" runat="server" Visible="false" CssClass="alert alert-success success">
        <asp:Literal ID="lConfirmed" runat="server"></asp:Literal>
    </asp:Panel>

    <asp:Panel ID="pnlResetPassword" runat="server" Visible="false">
    
        <asp:Literal ID="lResetPassword" runat="server"></asp:Literal>

        <fieldset>
            <legend>Enter New Password</legend>
            <Rock:RockTextBox ID="tbPassword" runat="server" Label="New Password" Required="true" TextMode="Password" ></Rock:RockTextBox>
            <Rock:RockTextBox ID="tbPasswordConfirm" runat="server" Label="Confirm Password" Required="true" TextMode="Password" ></Rock:RockTextBox>
            <asp:CompareValidator ID="cvPassword" runat="server" ControlToCompare="tbPassword" ControlToValidate="tbPasswordConfirm" ErrorMessage="New Password and Confirm Password do not match" Display="Dynamic" CssClass="validation-error"></asp:CompareValidator>
        </fieldset>

        <div class="actions">
            <asp:Button ID="btnResetPassword" runat="server" Text="Change Password" CssClass="btn btn-primary" OnClick="btnResetPassword_Click" />
        </div>

    </asp:Panel>

    <asp:Panel ID="pnlResetSuccess" runat="server" Visible="false" CssClass="alert alert-success success">
        <asp:Literal ID="lResetSuccess" runat="server"></asp:Literal>
    </asp:Panel>

    <asp:Panel ID="pnlDelete" runat="server" Visible="false">

        <asp:Literal ID="lDelete" runat="server"></asp:Literal>

        <div class="actions">
            <asp:Button ID="btnDelete" runat="server" Text="Yes, Delete the Account" CssClass="btn btn-primary" OnClick="btnDelete_Click" />
        </div>

    </asp:Panel>

    <asp:Panel ID="pnlDeleted" runat="server" Visible="false">
        <asp:Literal ID="lDeleted" runat="server"></asp:Literal>
    </asp:Panel>

</ContentTemplate>
</asp:UpdatePanel>
