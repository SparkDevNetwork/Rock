<%@ Page ValidateRequest="false" Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">
    <link href="/Themes/GrayFabric/Css/flexslider.css" rel="stylesheet">

    <!-- band to provide gray bar at the top of the page -->
    <div id="band" class="homepage">
	</div>

    <div class="container">
	  	<header>
		  	<div class="row identity">
			  	<div class="col-md-6">
                    <asp:HyperLink ID="hlLogo" runat="server" NavigateUrl="~" >
                        <asp:Image ID="imgLogo" runat="server" ImageUrl="~/Themes/GrayFabric/Assets/Images/rocksolidchurchlogo.svg" CssClass="logo" />
                    </asp:HyperLink>
			  	</div>
                <Rock:Zone ID="Heading" Name="Header" runat="server" />
			  	<div class="col-md-3 service-times">
			  		<i class="icon-time"></i> <span class="bold">Service Times</span>
				  	<br><span class="light">Sunday 9am, 10:30am and Noon</span>
			  	</div>
			  	<div class="col-md-3 my-account">My Account</div>
			</div>
	  		
           <Rock:Zone ID="Navigation" runat="server" />

	  	</header>

        <Rock:Zone ID="PromotionRotator" runat="server" />

    
	  </div>
  	  
  	  <div class="separator"></div>	  
  	  
    <Rock:Zone ID="PromotionList" runat="server" />
  	 
  	  
  	<section class="contact container">
  		<div class="row">
  			<section class="col-md-6 social">
	  			<span class="bold">Be Social, Share!</span> Like, comment, tweet, pin and share!
	  			<div class="icons">	
	  				<a href=""><i class="icon-twitter-sign"></i></a>
	  				<a href=""><i class="icon-facebook-sign"></i></a>
	  				<a href=""><i class="icon-google-plus-sign"></i></a>
	  				<a href=""><i class="icon-pinterest-sign"></i></a>
	  				<a href=""><i class="icon-linkedin-sign"></i></a>
	  			</div>
  			</section>
  			<section class="traditional col-md-6">
	  			<div class="map">
	  				<img src="http://maps.google.com/maps/api/staticmap?center=33.590795,-112.126459&zoom=13&markers=33.590795,-112.126459&size=225x225&sensor=false" />
	  			</div>
	  			<div class="info">
	  				<span class="bold">Rock Solid Church</span><br />
	  				<span class="light">
	  					<p>3120 W Cholla St<br />
	  					Phoenix, AZ 85029</p>
	  					
	  					<p>(623) 555-1234</p>
	  					<p><a href="mailto:sample@rockchms.com">info@rockchms.com</a></p>
	  				</span>
	  			</div>
  			</section>
  		</div>
  	</section>  



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
   
	<!-- Scripts -->
    <script type="text/javascript" src="../Themes/GrayFabric/Scripts/jquery.flexslider-min.js"></script>
    
     <script>

         $(document).ready(function () {
             $('.flexslider').flexslider({
                 animation: "slide",
                 nextText: '<i class="icon-chevron-right"></i>',
                 prevText: '<i class="icon-chevron-left"></i>',

             });
         });

    </script>

    


    
    <Rock:Zone ID="Menu" runat="server" />	
    

    <Rock:Zone ID="Zone1" runat="server" />
    
    <Rock:Zone ID="Footer" runat="server" />


    
                    <!-- display any ajax error messages here (use with ajax-client-error-handler.js) -->
                    <div class="alert alert-error ajax-error" style="display:none">
                        <strong>Ooops!</strong>
                        <span class="ajax-error-message"></span>
                    </div>

     
		
		
        
</asp:Content>

