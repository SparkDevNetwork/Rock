<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AllStaffLive.ascx.cs" Inherits="RockWeb.Plugins.cc_newspring.Blocks.AllStaffLive.AllStaffLive" %>

<!-- 16:9 aspect ratio -->

<asp:HiddenField ID="liveFeedStatus" runat="server" />
<asp:HiddenField ID="ooyalaId" runat="server" />
<asp:HiddenField ID="scheduleText" runat="server" />

<% if ( liveFeedStatus.Value.Equals( "on" ) )
   { %>
<div class="embed-responsive embed-responsive-16by9">

    <h1>All Staff is LIVE</h1>

    <script src='//player.ooyala.com/v3/bb2b5b1739de4b4e829882d478883a3f'></script>

    <div id='ooyalaplayer' style='width: 600px; height: 400px'></div>

    <script>OO.ready(function() { OO.Player.create('ooyalaplayer', <%= ooyalaId.Value %>); });</script>
    <noscript>
        <div>Please enable Javascript to watch this video</div>
    </noscript>

    <script type="text/javascript" src="../plugins/cc_newspring/Blocks/AllStaffLive/scripts.js"></script>
</div>
<% }
   else if ( liveFeedStatus.Value.Equals( "off" ) )
   { %>
<h1>Video All Staff <%= scheduleText.Value %></h1>
<% } %>