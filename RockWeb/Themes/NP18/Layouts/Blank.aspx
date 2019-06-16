<%@ Page Language="C#" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<!DOCTYPE html>

<html class="no-js">
<head runat="server">
    <title></title>

    <!-- Modernizr (Used by Rock RMS & Themes for feature detection) -->
    <script src="<%# ResolveRockUrl("~/Scripts/modernizr.js" ) %>"></script>

    <!-- jQuery (Used by Rock RMS & Themes) -->
    <script src="<%# System.Web.Optimization.Scripts.Url("~/Scripts/Bundles/RockJQueryLatest") %>"></script>

    <!-- NewPointe 2018 Theme CSS Files -->
    <link rel="stylesheet" href="<%# ResolveRockUrl("~~/Styles/bootstrap.min.css", true) %>" />
    <link rel="stylesheet" href="<%# ResolveRockUrl("~~/Styles/theme.min.css", true) %>" />
    <link rel="stylesheet" href="<%# ResolveRockUrl("~/Styles/developer.css", true) %>" />

    <style>
        html, body {
            height: 100%;
            width: 100%;
            margin: 0;
            padding: 0;
        }
    </style>

</head>

<body class="rock-blank">
    <form id="form1" runat="server">

        <!-- Main Content -->
        <main class="container-fluid">
            <Rock:Zone Name="Main" runat="server" />
        </main>

        <%-- controls for scriptmanager and update panel --%>
        <asp:ScriptManager ID="sManager" runat="server" />
        <asp:UpdateProgress ID="updateProgress" runat="server" DisplayAfter="800">
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
                <div class="updateprogress-bg modal-backdrop"></div>
            </ProgressTemplate>
        </asp:UpdateProgress>

    </form>
</body>
</html>
