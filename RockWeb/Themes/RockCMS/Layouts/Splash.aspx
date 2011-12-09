<%@ Page Title="" ValidateRequest="false" Language="C#" 
    AutoEventWireup="true" CodeFile="Splash.aspx.cs" Inherits="RockWeb.Themes.RockCMS.Layouts.Splash" %>

<html>
<head id="Head1" runat="server">
    <meta charset="utf-8">
    <title></title>
    
    <!--[if lt IE 9]>
        <script src="<%# ResolveUrl("~/Themes/RockCMS/Scripts/html5.js") %>" ></script>
    <![endif]-->

    <!-- Set the viewport width to device width for mobile -->
	<meta name="viewport" content="width=device-width" />

	<!-- Included CSS Files -->
	<link rel="stylesheet" href="<%# ResolveUrl("~/CSS/base.css") %>">
    <link rel="stylesheet" href="<%# ResolveUrl("~/CSS/cms-core.css") %>">
    <link rel="stylesheet" href="<%# ResolveUrl("~/CSS/grid.css") %>">
    <link rel="stylesheet" href="<%# ResolveUrl("~/Themes/RockCMS/CSS/rock.css") %>">

    <script src="<%# ResolveUrl("~/Scripts/bootstrap-modal.js") %>" ></script>
    <script src="<%# ResolveUrl("~/Scripts/bootstrap-tabs.js") %>" ></script>

</head>
<body id="splash">

    <form id="form1" runat="server">

            <div id="content">
                <h1>Rock ChMS</h1>
                
                <div id="content-box" class="group">
                    <asp:PlaceHolder ID="Content" runat="server">
                        
                    </asp:PlaceHolder>
                </div>
            </div>


    </form>
</body>
</html>