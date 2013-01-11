<%@ Page Title="" ValidateRequest="false" Language="C#" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>
<!DOCTYPE html> 
<html>
<head id="Head1" runat="server">
    <meta charset="utf-8">
    <title></title>
    
    <!--[if lt IE 9]>
        <script src="<%# ResolveUrl("~/Themes/RockCms/Scripts/html5.js") %>" ></script>
    <![endif]-->

    <!-- Set the viewport width to device width for mobile -->
    <meta name="viewport" content="width=device-width" />

    <!-- Included CSS Files -->
    <link rel="stylesheet" href="<%# ResolveUrl("~/Themes/RockChMS/Css/rock-boot.less") %>">
    <link rel="stylesheet" href="<%# ResolveUrl("~/Themes/RockChMS/Css/rock-boot-responsive.less") %>">
    <link rel="stylesheet" href="<%# ResolveUrl("~/Css/rock-core.less") %>">
    <link rel="stylesheet" href="<%# ResolveUrl("~/Themes/RockChMS/Css/rock-theme.less") %>">
    <link rel="stylesheet" href="<%# ResolveUrl("~/CSS/developer.less") %>">

<<<<<<< HEAD
    <script src="<%# ResolveUrl("~/Scripts/jquery-1.8.0.min.js") %>" ></script>
=======
    <script src="<%# ResolveUrl("~/Scripts/jquery-1.8.3.min.js") %>" ></script>
    <script src="<%# ResolveUrl("~/Scripts/jquery-ui-1.8.23.custom.min.js") %>" ></script>
>>>>>>> f9b70accf4d5e42eb66054b1b8b75a3f61885d27
    <script src="<%# ResolveUrl("~/Scripts/bootstrap.min.js") %>" ></script>

</head>
<body id="splash">

    <form id="form1" runat="server">

            <div id="content">
                <img alt="Rock ChMS" src="/RockWeb/Assets/Images/rock-logo.svg" class="pageheader-logo" />
                
                <div id="content-box" class="group">
                    <Rock:Zone ID="Content" runat="server" />
                </div>
            </div>


    </form>
</body>
</html>