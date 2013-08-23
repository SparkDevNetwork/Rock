<%@ Page ValidateRequest="false" Language="C#" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>
<!DOCTYPE html> 
<html>
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta charset="utf-8">
    <title></title>
    
    <!--[if lt IE 9]>
        <script src="<%# ResolveUrl("~/Themes/RockChMS/Scripts/html5.js") %>" ></script>
    <![endif]-->

    <!-- Set the viewport width to device width for mobile -->
    <meta name="viewport" content="width=device-width" />

    <!-- Included CSS Files -->
    <link rel="stylesheet" href="<%# ResolveUrl("~/Themes/RockChMS/Css/bootstrap.less") %>">
    <link rel="stylesheet" href="<%# ResolveUrl("~/Themes/RockChMS/Css/bootstrap-responsive.less") %>">
    <link rel="stylesheet" href="<%# ResolveUrl("~/Themes/RockChMS/Css/site-theme.less") %>">
    <link rel="stylesheet" href="<%# ResolveUrl("~/CSS/developer.css") %>">

    <script src="<%# ResolveUrl("~/Scripts/jquery.js") %>" ></script>
    <script src="<%# ResolveUrl("~/Scripts/bootstrap.min.js") %>" ></script>

</head>
<body id="splash">

    <form id="form1" runat="server">

            <div id="content">
                <asp:Image ID="Image1" runat="server" AlternateText="Rock ChMS" ImageUrl="~/Assets/Images/rock-logo.svg" CssClass="pageheader-logo" />
                
                <div id="content-box" class="clearfix">
                    <Rock:Zone ID="Content" runat="server" />
                </div>
            </div>


    </form>
</body>
</html>