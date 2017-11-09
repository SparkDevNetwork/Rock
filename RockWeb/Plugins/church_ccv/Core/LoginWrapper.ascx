<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LoginWrapper.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Core.LoginWrapper" %>

<div id="divProfilePhoto" runat="server" class="profile-photo"></div>   
<ul class="loginstatus"> 
    
    <li class="dropdown" ID="liDropdown" runat="server">
        
        <a class="masthead-navitem dropdown-toggle navbar-link loginstatus" href="#" data-toggle="dropdown">
        
            <asp:PlaceHolder ID="phHello" runat="server"><asp:Literal ID="lHello" runat="server" /></asp:PlaceHolder>
            <b class="fa fa-caret-down"></b>
        </a>

        <ul class="dropdown-menu">
            <asp:PlaceHolder ID="phMyAccount" runat="server">
                <li>
                    <asp:HyperLink ID="hlMyAccount" runat="server" Text="My Account" />
                </li>
            </asp:PlaceHolder>
            <asp:PlaceHolder ID="phMySettings" runat="server">
                <li>
                    <asp:HyperLink ID="hlMySettings" runat="server" Text="My Settings" />
                </li>
            </asp:PlaceHolder>
            <asp:PlaceHolder ID="phMyProfile" runat="server">
                <li>
                    <asp:HyperLink ID="hlMyProfile" runat="server" Text="My Profile" />
                </li>
            </asp:PlaceHolder>
            <asp:Literal ID="lDropdownItems" runat="server" />

            <li><asp:LinkButton ID="lbLogout" runat="server" OnClick="LoginStatus_lbLogout_Click" CausesValidation="false"></asp:LinkButton></li>
        </ul>

    </li>
    <li ID="liLogin" runat="server" Visible="false"><asp:LinkButton CssClass="masthead-navitem" ID="lbLogin" runat="server" OnClientClick="displayLoginModal(); return false;" CausesValidation="false" Text="Login">
    <asp:PlaceHolder ID="phNewAccount" runat="server" Visible="false" ><asp:HyperLink ID="hlNewAccount" CssClass="masthead-navitem" runat="server" Text="Create Account" /></asp:PlaceHolder></asp:LinkButton></li>
</ul>
