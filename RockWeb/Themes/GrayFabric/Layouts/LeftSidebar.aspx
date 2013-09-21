<%@ Page ValidateRequest="false" Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

    <!-- band to provide gray bar at the top of the page -->
    <div id="band" class="insidepage">
	</div>

    <div class="container">
	  	<header>
		  	<div class="row-fluid identity">
			  	<div class="span6">
			  		<img class="logo" src="./assets/img/rocksolidchurchlogo.svg">
                    <asp:HyperLink ID="hlLogo" runat="server" NavigateUrl="~" >
                        <asp:Image ID="imgLogo" runat="server" ImageUrl="~/Themes/GrayFabric/Assets/Images/rocksolidchurchlogo.svg" CssClass="logo" />
                    </asp:HyperLink>
			  	</div>
                <Rock:Zone ID="Heading" Name="Header" runat="server" />
			  	<div class="span3 service-times">
			  		<i class="icon-time"></i> <span class="bold">Service Times</span>
				  	<br><span class="light">Sunday 9am, 10:30am and Noon</span>
			  	</div>
			  	<div class="span3 my-account">My Account</div>
			</div>
	  		
           <Rock:Zone ID="Navigation" runat="server" />

	  	</header>

    </div>
  	  
  	  
  	<div id="content" class="container"> 
        <h1 class="page-title"><Rock:PageTitle ID="PageTitle" runat="server" /></h1>

  	    <div class="row-fluid">
            <div class="span3">
                <Rock:Zone ID="Sidebar" runat="server" />
            </div>
            <div class="span9">
                <Rock:Zone ID="Main" runat="server" />
            </div>
        </div>
    </div>
  	



    <footer>
  		<div class="container">
	  		<section class="author light">
	  			<span>Design by <a href="http://www.voracitysolutions.com">Voracity Solutions</a></span> 
	  			<span>Powered by <a href="http://www.rockchms.com">Rock ChMS</a></span>
	  		</section>
	  		<section class="actions light right-align">
	  			<a>Login</a>
	  		</section>
  		</div>
  	</footer>   


    
    
    <Rock:Zone ID="Footer" runat="server" />


    
                    <!-- display any ajax error messages here (use with ajax-client-error-handler.js) -->
                    <div class="alert alert-error ajax-error" style="display:none">
                        <strong>Ooops!</strong>
                        <span class="ajax-error-message"></span>
                    </div>

     
		
		
        
</asp:Content>

