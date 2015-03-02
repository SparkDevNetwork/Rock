<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LoginStatus.ascx.cs" Inherits="RockWeb.Blocks.Security.LoginStatus" %>

<ul class="nav navbar-nav loginstatus">    
    <li class="dropdown" ID="liDropdown" runat="server">

        <a class="dropdown-toggle navbar-link" href="#" data-toggle="dropdown">
            <div id="divProfilePhoto" runat="server" class="profile-photo"></div>

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
            <li class="divider"></li>
            <li><asp:LinkButton ID="lbLoginLogout" runat="server" OnClick="lbLoginLogout_Click" CausesValidation="false"></asp:LinkButton></li>
        </ul>

    </li>
    <li ID="liLogin" runat="server" Visible="false"><asp:LinkButton ID="lbLogin" runat="server" OnClick="lbLoginLogout_Click" CausesValidation="false" Text="Login"></asp:LinkButton></li>
</ul>
<asp:HiddenField ID="hfActionType" runat="server" />

