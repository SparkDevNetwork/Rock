<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ForgotUserName.ascx.cs" Inherits="RockWeb.Blocks.Security.ForgotUserName" %>

<asp:UpdatePanel id="upnlContent" runat="server">
<ContentTemplate>

    <asp:Panel ID="pnlEntry" runat="server">

        <asp:Literal ID="lCaption" runat="server" ></asp:Literal>

        <asp:Panel ID="pnlWarning" runat="server" Visible="false" CssClass="alert alert-validation">
            <asp:Literal ID="lWarning" runat="server"></asp:Literal>
        </asp:Panel>

        <fieldset>
            <Rock:RockTextBox ID="tbEmail" runat="server" Label="Enter your email address" Required="true" DisplayRequiredIndicator="false"></Rock:RockTextBox>
        </fieldset>

        <div class="actions">
            <asp:Button ID="btnSend" runat="server" Text="Email me reset instructions" CssClass="btn btn-primary" OnClick="btnSend_Click" Visible="false" />
            <div id="pnlCaptcha" runat="server" class="form-group">
                <Rock:Captcha ID="cpCaptcha" runat="server" OnTokenReceived="cpCaptcha_TokenReceived" />
            </div>
        </div>

    </asp:Panel>

    <asp:Panel ID="pnlSuccess" runat="server" Visible="false" CssClass="alert alert-success">
        <asp:Literal ID="lSuccess" runat="server"></asp:Literal>
    </asp:Panel>

</ContentTemplate>
</asp:UpdatePanel>
