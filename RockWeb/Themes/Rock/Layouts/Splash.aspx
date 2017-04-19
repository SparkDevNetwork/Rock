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

<html>
<head runat="server">
    <meta charset="utf-8">
    <title></title>

    <!-- Set the viewport width to device width for mobile disabling zooming -->
    <meta name="viewport" content="width=device-width, initial-scale=1.0, user-scalable=no">

    <script src="<%# ResolveRockUrl("~/Scripts/modernizr.js", true) %>" ></script>
    <script src="<%# ResolveRockUrl("~/Scripts/jquery-1.12.4.min.js", true) %>"></script>

    <!-- Included CSS Files -->
    <link rel="stylesheet" href="<%# ResolveRockUrl("~/Themes/Rock/Styles/bootstrap.css", true) %>"/>
	<link rel="stylesheet" href="<%# ResolveRockUrl("~/Themes/Rock/Styles/theme.css", true) %>"/>
	<link rel="stylesheet" href="<%# ResolveRockUrl("~/Styles/developer.css", true) %>"/>

    <script src="<%# ResolveRockUrl("~/Scripts/bootstrap.min.js", true) %>" ></script>

    <!-- Icons -->
    <link rel="shortcut icon" href="<%# ResolveRockUrl("~/Assets/Icons/favicon.ico", true) %>"/>
    <link rel="apple-touch-icon-precomposed" sizes="144x144" href="<%# ResolveRockUrl("~/Assets/Icons/touch-icon-ipad-retina.png", true) %>"/>
    <link rel="apple-touch-icon-precomposed" sizes="114x114" href="<%# ResolveRockUrl("~/Assets/Icons/touch-icon-iphone-retina.png", true) %>"/>
    <link rel="apple-touch-icon-precomposed" sizes="72x72" href="<%# ResolveRockUrl("~/Assets/Icons/touch-icon-ipad.png", true) %>"/>
    <link rel="apple-touch-icon-precomposed" href="<%# ResolveRockUrl("~/Assets/Icons/touch-icon-iphone.png", true) %>"/>

</head>
<body id="splash">

    <form id="form1" runat="server">

        <div id="content">
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
</body>
</html>