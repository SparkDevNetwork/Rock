<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LoginStatus.ascx.cs" Inherits="RockWeb.Blocks.Security.LoginStatus" %>
<div class="account-name">
    <asp:PlaceHolder ID="phHello" runat="server"></asp:PlaceHolder>
    <div class="account-actions">
        <asp:PlaceHolder ID="phMyAccount" runat="server">
            <a href="myaccount">My Account</a>
        </asp:PlaceHolder>
        <asp:LinkButton ID="lbLoginLogout" runat="server" OnClick="lbLoginLogout_Click"></asp:LinkButton>
    </div>
</div>
