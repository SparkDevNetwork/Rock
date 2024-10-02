<%@ Page Language="C#" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <title></title>

    <script src="<%# System.Web.Optimization.Scripts.Url("~/Scripts/Bundles/RockJQueryLatest") %>"></script>

    <link rel="stylesheet" href="<%# ResolveRockUrl("~~/Styles/bootstrap.css", true) %>" />
    <link rel="stylesheet" href="<%# ResolveRockUrl("~~/Styles/theme.css", true) %>" />

    <style>
        html, body {
            height: auto;
            width: 100%;
            min-width: 100%;
            margin: 0;
            padding: 0;
            vertical-align: top;
        }
    </style>

</head>

<body class="rock-modal-page">
    <form id="form1" runat="server">
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
                <div class="updateprogress-bg modal-backdrop">
                </div>
            </ProgressTemplate>
        </asp:UpdateProgress>

        <main>
            <!-- Start Content Area -->
            <Rock:Zone Name="Main" runat="server" />
        </main>
    </form>
</body>
</html>