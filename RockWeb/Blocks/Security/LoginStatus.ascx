<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LoginStatus.ascx.cs" Inherits="RockWeb.Blocks.Security.LoginStatus" %>
<ul class="nav loginstatus">
    <li class="dropdown">
        <a class="dropdown-toggle navbar-link" href="#" data-toggle="dropdown">
            <i class="icon-user"></i>
            <asp:PlaceHolder ID="phHello" runat="server"><asp:Literal ID="lHello" runat="server" /></asp:PlaceHolder>
            <b class="caret"></b>
        </a>
        <ul class="dropdown-menu">
            <li>
                <asp:PlaceHolder ID="phMyAccount" runat="server">
                    <a href='<%= Page.ResolveUrl("~") + "MyAccount" %>'>My Account</a>
                </asp:PlaceHolder>
            </li>
            <li class="divider"></li>
            <li><asp:LinkButton ID="lbLoginLogout" runat="server" OnClick="lbLoginLogout_Click" CausesValidation="false"></asp:LinkButton></li>
        </ul>
    </li>
</ul>
<asp:HiddenField ID="hfTest" runat="server" />

