<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ChangePassword.ascx.cs" Inherits="RockWeb.Blocks.Security.ChangePassword" %>

<asp:UpdatePanel runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlEntry" runat="server">

            <asp:Panel ID="pnlInvalid" runat="server" CssClass="alert alert-error error" Visible="false">
                <asp:Literal ID="lInvalid" runat="server"></asp:Literal>
            </asp:Panel>

            <fieldset>
                <legend>Change Password</legend>
                <Rock:LabeledTextBox ID="tbUserName" runat="server" LabelText="Username" Required="true"></Rock:LabeledTextBox>
                <Rock:LabeledTextBox ID="tbOldPassword" runat="server" LabelText="Old Password" Required="true" TextMode="Password"></Rock:LabeledTextBox>
                <Rock:LabeledTextBox ID="tbPassword" runat="server" LabelText="New Password" Required="true" TextMode="Password"></Rock:LabeledTextBox>
                <Rock:LabeledTextBox ID="tbPasswordConfirm" runat="server" LabelText="Confirm Password" Required="true" TextMode="Password"></Rock:LabeledTextBox>
                <asp:CompareValidator ID="cvPassword" runat="server" ControlToCompare="tbPassword" ControlToValidate="tbPasswordConfirm" ErrorMessage="New Password and Confirm Password do not match" Display="Dynamic" CssClass="validation-error"></asp:CompareValidator>
            </fieldset>

            <div class="actions">
                <asp:Button ID="btnChange" runat="server" Text="Change Password" CssClass="btn btn-primary" OnClick="btnChange_Click" />
            </div>

        </asp:Panel>

        <asp:Panel ID="pnlSuccess" runat="server" Visible="false" CssClass="alert alert-success success">
            <asp:Literal ID="lSuccess" runat="server"></asp:Literal>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
