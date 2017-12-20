<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ChOPForgotUsername.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.ChurchOnlinePlatform.ChOPForgotUsername" %>

<asp:UpdatePanel id="upnlContent" runat="server">
<ContentTemplate>
    <div class="sso-createaccount-panel-wrapper">
        <img src="/themes/church_ccv_external_v8/assets/ccv_logo-hi-res.png" style="width: 125px; margin-bottom: 25px;"/>

        <asp:Panel ID="pnlEntry" runat="server">
            <asp:Literal ID="lCaption" runat="server" ></asp:Literal> 

            <fieldset>
                <Rock:RockTextBox ID="tbEmail" runat="server" Label="Email" Required="true" ></Rock:RockTextBox>
            </fieldset>

            <asp:Panel ID="pnlWarning" runat="server" Visible="false" CssClass="alert alert-warning">
                <asp:Literal ID="lWarning" runat="server"></asp:Literal>
            </asp:Panel>

            <div class="actions">
                <asp:Button ID="btnSend" runat="server" Text="Send Username" CssClass="btn btn-primary" OnClick="btnSend_Click" />
            </div>

        </asp:Panel>

        <asp:Panel ID="pnlSuccess" runat="server" Visible="false" CssClass="alert alert-success success">
            <asp:Literal ID="lSuccess" runat="server"></asp:Literal>
            <br />
            <div>
                <asp:Button ID="btnSentLogin" runat="server" Text="Login" CssClass="btn btn-primary" OnClick="btnSendLogin_Click" />
            </div>
        </asp:Panel>
    </div>
</ContentTemplate>
</asp:UpdatePanel>
