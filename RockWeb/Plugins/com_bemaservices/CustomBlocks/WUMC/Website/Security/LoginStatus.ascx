<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LoginStatus.ascx.cs" Inherits="RockWeb.Plugins.com_bemadev.Security.LoginStatus" %>

   <div class="" ID="liDropdown" runat="server">

        
            <div class="hidden">
             <asp:PlaceHolder ID="phHello" runat="server" Visible="false" ><asp:Literal ID="lHello" runat="server" /></asp:PlaceHolder>
			  <div id="divProfilePhoto" runat="server" class="profile-photo hidden"></div>
            </div>


        <ul class="nav nav-pills" id="ulClass" runat="server" style="list-style: none;">		
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

            <li><asp:LinkButton ID="lbLoginLogout" runat="server" OnClick="lbLoginLogout_Click" CausesValidation="false"></asp:LinkButton></li>
        </ul>

    </div>
	
	<ul ID="liLogin" runat="server" Visible="false" style="list-style: none;">
		<li >
			<asp:LinkButton ID="lbLogin" runat="server" OnClick="lbLoginLogout_Click" CausesValidation="false" Text="Login">
			</asp:LinkButton>
		</li>
	</ul>

<asp:HiddenField ID="hfActionType" runat="server" />


