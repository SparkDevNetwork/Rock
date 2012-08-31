<%@ Page Title="" ValidateRequest="false" Language="C#" AutoEventWireup="true" Inherits="Rock.Web.UI.Page" %>

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
	<link rel="stylesheet" href="<%# ResolveUrl("~/CSS/base.css") %>">
    <link rel="stylesheet" href="<%# ResolveUrl("~/CSS/cms-core.css") %>">
    <link rel="stylesheet" href="<%# ResolveUrl("~/CSS/grid.css") %>">
    <link rel="stylesheet" href="<%# ResolveUrl("~/Themes/RockCms/CSS/rock.css") %>">

<!--    <script src="<%# ResolveUrl("~/Scripts/jquery-1.5.min.js") %>" ></script> -->
    <script src="<%# ResolveUrl("~/Scripts/jquery-1.8.0.min.js") %>" ></script>
    <script src="<%# ResolveUrl("~/Scripts/jquery-ui-1.8.23.custom.min.js") %>" ></script>
<!--    <script src="<%# ResolveUrl("~/Scripts/bootstrap-modal.js") %>" ></script> -->
<!--    <script src="<%# ResolveUrl("~/Scripts/bootstrap-tabs.js") %>" ></script> -->
    <script src="<%# ResolveUrl("~/Scripts/bootstrap.min.js") %>" ></script>

</head>
<body id="splash">

    <form id="form1" runat="server">

            <div id="content">
                <h1>Rock ChMS</h1>
                
                <div id="content-box" class="group">
                    <Rock:Zone ID="Content" runat="server" />
                </div>
            </div>


    </form>
</body>
</html>