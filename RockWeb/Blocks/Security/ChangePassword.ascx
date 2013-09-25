<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ChangePassword.ascx.cs" Inherits="RockWeb.Blocks.Security.ChangePassword" %>

<asp:UpdatePanel runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlEntry" runat="server">

            <asp:Panel ID="pnlInvalid" runat="server" CssClass="alert alert-error error" Visible="false">
                <asp:Literal ID="lInvalid" runat="server"></asp:Literal>
            </asp:Panel>

            <fieldset>
                <legend>Change Password</legend>
                <Rock:RockTextBox ID="tbUserName" runat="server" Label="Username" Required="true"></Rock:RockTextBox>
                <Rock:RockTextBox ID="tbOldPassword" runat="server" Label="Old Password" Required="true" TextMode="Password"></Rock:RockTextBox>
                <Rock:RockTextBox ID="tbPassword" runat="server" Label="New Password" Required="true" TextMode="Password"></Rock:RockTextBox>
                <Rock:RockTextBox ID="tbPasswordConfirm" runat="server" Label="Confirm Password" Required="true" TextMode="Password"></Rock:RockTextBox>
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
