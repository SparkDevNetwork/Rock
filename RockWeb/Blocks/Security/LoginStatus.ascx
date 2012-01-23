<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LoginStatus.ascx.cs" Inherits="RockWeb.Blocks.Security.LoginStatus" %>
<div class="account-name">
    <asp:PlaceHolder ID="phHello" runat="server"><asp:Literal ID="lHello" runat="server" /></asp:PlaceHolder>
    <div class="account-actions">
        <asp:PlaceHolder ID="phMyAccount" runat="server">
            <a href='<%= Page.ResolveUrl("~") + "MyAccount" %>'>My Account</a>
        </asp:PlaceHolder>
        <asp:LinkButton ID="lbLoginLogout" runat="server" OnClick="lbLoginLogout_Click" CausesValidation="false"></asp:LinkButton>
    </div>
</div>
<asp:HiddenField ID="hfTest" runat="server" />

