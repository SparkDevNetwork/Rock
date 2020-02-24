<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Ooyala.ascx.cs" Inherits="RockWeb.Plugins.cc_newspring.Blocks.Video.Ooyala" %>

<div class="embed-responsive embed-responsive-16by9">
    <script src='//player.ooyala.com/v3/bb2b5b1739de4b4e829882d478883a3f'></script>

    <asp:HiddenField ID="ooyalaId" runat="server" />

    <div id='ooyalaplayer' class='embed-responsive-item' style='width: 600px; height: 400px'></div>

    <script>
        var ooyalaID = document.getElementById('<%= ooyalaId.ClientID %>');
        OO.ready(function () { OO.Player.create('ooyalaplayer', ooyalaID.value); });
    </script>

    <noscript>
        <div>Please enable Javascript to watch this video</div>
    </noscript>

    <script type="text/javascript" src="../plugins/cc_newspring/Blocks/Video/scripts.js"></script>
</div>