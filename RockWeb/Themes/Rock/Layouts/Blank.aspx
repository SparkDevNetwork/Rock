<%@ Page Language="C#" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<!DOCTYPE html>

<html class="no-js">
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=10" />
    <title></title>

    <script src="<%# ResolveUrl("~/Scripts/modernizr.js") %>"></script>
    <script src="<%# ResolveUrl("~/Scripts/jquery-1.10.2.min.js") %>"></script>

    <link rel="stylesheet" href="<%# ResolveRockUrl("~~/Styles/bootstrap.css") %>" />
    <link rel="stylesheet" href="<%# ResolveRockUrl("~~/Styles/theme.css") %>" />
    <link rel="stylesheet" href="<%# ResolveRockUrl("~/Styles/developer.css") %>" />

    <style>
        html, body {
            height: 100%;
            min-height: 100%;
            width: 100%;
            min-width: 100%;
            background-color: #ffffff;
            margin: 0 0 0 0;
            padding: 0 0 0 0;
            vertical-align: top;
        }
    </style>

</head>

<body class="rock-blank">
    <form id="form1" runat="server">
        <ajaxToolkit:ToolkitScriptManager ID="sManager" runat="server" />

        <asp:UpdateProgress ID="updateProgress" runat="server">
            <ProgressTemplate>
                <div class="updateprogress-status">
                    <div class="spinner">
                        <div class="rect1"></div>
                        <div class="rect2"></div>
                        <div class="rect3"></div>
                        <div class="rect4"></div>
                        <div class="rect5"></div>
                    </div>
                </div>
                <div class="updateprogress-bg modal-backdrop">
                </div>
            </ProgressTemplate>
        </asp:UpdateProgress>

        <main class="container">
        
            <!-- Start Content Area -->
            <Rock:Zone Name="Main" runat="server" />

        </main>
    </form>
</body>
</html>
