<%@ Page Language="C#" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>
<!DOCTYPE html> 

<script runat="server">
    
    // keep code below to call base class init method

    /// <summary>
    /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
    protected override void OnInit( EventArgs e )
    {
        base.OnInit( e );

        lLogoSvg.Text = System.IO.File.ReadAllText( HttpContext.Current.Request.MapPath("~/Assets/Images/rock-logo-sm.svg") );
    }    
    
</script>

<html style="overflow: hidden;">
<head runat="server">
    <meta charset="utf-8">
    <title></title>

    <!-- Set the viewport width to device width for mobile disabling zooming -->
    <meta name="viewport" content="width=device-width, initial-scale=1.0, user-scalable=no">

    <script src="<%# ResolveRockUrl("~/Scripts/modernizr.js", true) %>" ></script>
    <script src="<%# ResolveRockUrl("~/Scripts/jquery-1.12.4.min.js", true) %>"></script>

    <!-- Included CSS Files -->
    <link rel="stylesheet" href="<%# ResolveRockUrl("~/Themes/Rock_NewSpring/Styles/bootstrap.css", true) %>"/>
	<link rel="stylesheet" href="<%# ResolveRockUrl("~/Themes/Rock_NewSpring/Styles/theme.css", true) %>"/>
	<link rel="stylesheet" href="<%# ResolveRockUrl("~/Styles/developer.css", true) %>"/>

    <script src="<%# ResolveRockUrl("~/Scripts/bootstrap.min.js", true) %>" ></script>

</head>
<body id="splash" class="absolute top-zero right-zero bottom-zero left-zero floating">

    <form id="form1" runat="server" class="floating-item">

        <div id="content" class="text-left">
            <div id="logo">
                <asp:Literal ID="lLogoSvg" runat="server" />
            </div>
                
            <div id="content-box" class="clearfix">
                <Rock:Zone Name="Main" runat="server" />
            </div>
        </div>
        
        <script>

            // add quick fade-in effect to the page
            $(document).ready(function () {
                $("#content").rockFadeIn();
            });
        </script>

    </form>

    <!-- TypeKit -->
    <script>
        (function(d) {
        var config = {
          kitId: 'oby6bqi',
          scriptTimeout: 3000,
          async: true
        },
        h=d.documentElement,t=setTimeout(function(){h.className=h.className.replace(/\bwf-loading\b/g,"")+" wf-inactive";},config.scriptTimeout),tk=d.createElement("script"),f=false,s=d.getElementsByTagName("script")[0],a;h.className+=" wf-loading";tk.src='https://use.typekit.net/'+config.kitId+'.js';tk.async=true;tk.onload=tk.onreadystatechange=function(){a=this.readyState;if(f||a&&a!="complete"&&a!="loaded")return;f=true;clearTimeout(t);try{Typekit.load(config)}catch(e){}};s.parentNode.insertBefore(tk,s)
        })(document);
    </script>
    
</body>
</html>