<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ResetPassword.ascx.cs" Inherits="RockWeb.Blocks.Security.ResetPassword" %>

<asp:UpdatePanel runat="server">
<ContentTemplate>

    <asp:Panel ID="pnlEntry" runat="server">
    
        <fieldset>
            <legend>Change Password</legend>
            <Rock:LabeledTextBox ID="tbUserName" runat="server" LabelText="Username" Required="true" ></Rock:LabeledTextBox>
            <Rock:LabeledTextBox ID="tbOldPassword" runat="server" LabelText="Old Password" Required="true" ></Rock:LabeledTextBox>
            <Rock:LabeledTextBox ID="tbPassword" runat="server" LabelText="New Password" Required="true" ></Rock:LabeledTextBox>
            <Rock:LabeledTextBox ID="tbPasswordConfirm" runat="server" LabelText="Confirmation" Required="true" ></Rock:LabeledTextBox>
        </fieldset>

        <div class="actions">
            <asp:Button ID="btnChange" runat="server" Text="Change Password" CssClass="btn primary" OnClick="btnChange_Click" />
        </div>

    </asp:Panel>

    <asp:Panel ID="pnlDone" runat="server" Visible="false"><asp:Literal ID="lDone" runat="server"></asp:Literal></asp:Panel>

</ContentTemplate>
</asp:UpdatePanel>
