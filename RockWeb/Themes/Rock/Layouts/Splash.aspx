<%@ Page Language="C#" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>
<!DOCTYPE html> 
<html>
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=10" />
    <meta charset="utf-8">
    <title></title>

    <!-- Set the viewport width to device width for mobile -->
    <meta name="viewport" content="width=device-width" />

    <script src="<%# ResolveRockUrl("~/Scripts/modernizr.js", true) %>" ></script>
    <script src="<%# ResolveRockUrl("~/Scripts/jquery-1.10.2.min.js", true) %>"></script>

    <!-- Included CSS Files -->
    <link rel="stylesheet" href="<%# ResolveRockUrl("~/Themes/Rock/Styles/bootstrap.css", true) %>"/>
	<link rel="stylesheet" href="<%# ResolveRockUrl("~/Themes/Rock/Styles/theme.css", true) %>"/>
	<link rel="stylesheet" href="<%# ResolveRockUrl("~/Styles/developer.css", true) %>"/>

    <script src="<%# ResolveRockUrl("~/Scripts/bootstrap.min.js", true) %>" ></script>

</head>
<body id="splash">

    <form id="form1" runat="server">

        <div id="content">
            <asp:Image ID="Image1" runat="server" AlternateText="Rock" ImageUrl="<%$ Fingerprint:~/Assets/Images/rock-logo.svg %>" CssClass="pageheader-logo" />
                
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