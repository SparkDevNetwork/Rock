<%@ Page ValidateRequest="false" Language="C#" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>
<!DOCTYPE html> 
<html>
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=10" />
    <meta charset="utf-8">
    <title></title>

    <!-- Set the viewport width to device width for mobile -->
    <meta name="viewport" content="width=device-width" />

    <script src="<%# ResolveUrl("~/Scripts/modernizr.js") %>" ></script>
    <script src="<%# ResolveUrl("~/Scripts/jquery-1.10.2.min.js") %>"></script>

    <!-- Included CSS Files -->
    <link rel="stylesheet" href="<%# ResolveUrl("~/Themes/RockChMS/Styles/bootstrap.css") %>"/>
	<link rel="stylesheet" href="<%# ResolveUrl("~/Themes/RockChMS/Styles/theme.css") %>"/>
	<link rel="stylesheet" href="<%# ResolveUrl("~/Styles/developer.css") %>"/>

    <script src="<%# ResolveUrl("~/Scripts/jquery.js") %>" ></script>
    <script src="<%# ResolveUrl("~/Scripts/bootstrap.min.js") %>" ></script>

</head>
<body id="splash">

    <form id="form1" runat="server">

        <div id="content">
            <asp:Image ID="Image1" runat="server" AlternateText="Rock ChMS" ImageUrl="~/Assets/Images/rock-logo.svg" CssClass="pageheader-logo" />
                
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