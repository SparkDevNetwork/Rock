<%@ Page Language="C#" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <title></title>

    <!-- Set the viewport width to device width for mobile -->
    <meta name="viewport" content="width=device-width, initial-scale=1.0, user-scalable=no">

    <script src="<%# System.Web.Optimization.Scripts.Url("~/Scripts/Bundles/RockJQueryLatest") %>"></script>

    <link rel="stylesheet" href="<%# ResolveRockUrl("~~/Styles/theme.css", true) %>" />

    <style>
        html, body, #form1 {
            height: 100%;
            margin: 0;
            padding: 0;
        }

        .page-fullscreen,
        .page-fullscreen .zone-instance,
        .page-fullscreen .zone-content,
        .page-fullscreen .block-instance,
        .page-fullscreen .block-content,
        .page-fullscreen .block-content > div,
        .page-fullscreen .obsidian-block-wrapper,
        .page-fullscreen .obsidian-block {
            height: 100%;
        }
    </style>

</head>

<body class="rock-blank">
    <form id="form1" runat="server">
        <asp:ScriptManager ID="sManager" runat="server" />

        <asp:UpdateProgress ID="updateProgress" runat="server" DisplayAfter="800">
            <ProgressTemplate>
                <div class="updateprogress-status">
                    <div class="updateprogress-loader">
                        <svg viewBox="0 0 135 140" xmlns="http://www.w3.org/2000/svg" fill="CurrentColor"><rect y="10" width="15" height="120" rx="6"><animate attributeName="height" begin="0.5s" dur="1s" values="120;110;100;90;80;70;60;50;40;140;120" calcMode="linear" repeatCount="indefinite"/><animate attributeName="y" begin="0.5s" dur="1s" values="10;15;20;25;30;35;40;45;50;0;10" calcMode="linear" repeatCount="indefinite"/></rect><rect x="30" y="10" width="15" height="120" rx="6"><animate attributeName="height" begin="0.25s" dur="1s" values="120;110;100;90;80;70;60;50;40;140;120" calcMode="linear" repeatCount="indefinite"/><animate attributeName="y" begin="0.25s" dur="1s" values="10;15;20;25;30;35;40;45;50;0;10" calcMode="linear" repeatCount="indefinite"/></rect><rect x="60" width="15" height="140" rx="6"><animate attributeName="height" begin="0s" dur="1s" values="120;110;100;90;80;70;60;50;40;140;120" calcMode="linear" repeatCount="indefinite"/><animate attributeName="y" begin="0s" dur="1s" values="10;15;20;25;30;35;40;45;50;0;10" calcMode="linear" repeatCount="indefinite"/></rect><rect x="90" y="10" width="15" height="120" rx="6"><animate attributeName="height" begin="0.25s" dur="1s" values="120;110;100;90;80;70;60;50;40;140;120" calcMode="linear" repeatCount="indefinite"/><animate attributeName="y" begin="0.25s" dur="1s" values="10;15;20;25;30;35;40;45;50;0;10" calcMode="linear" repeatCount="indefinite"/></rect><rect x="120" y="10" width="15" height="120" rx="6"><animate attributeName="height" begin="0.5s" dur="1s" values="120;110;100;90;80;70;60;50;40;140;120" calcMode="linear" repeatCount="indefinite"/><animate attributeName="y" begin="0.5s" dur="1s" values="10;15;20;25;30;35;40;45;50;0;10" calcMode="linear" repeatCount="indefinite"/></rect></svg>        
                    </div>
                </div>
                <div class="updateprogress-bg modal-backdrop">
                </div>
            </ProgressTemplate>
        </asp:UpdateProgress>

        <main class="page-fullscreen">
            <!-- Start Content Area -->
            <Rock:Zone Name="Main" runat="server" />
        </main>
    </form>
</body>
</html>