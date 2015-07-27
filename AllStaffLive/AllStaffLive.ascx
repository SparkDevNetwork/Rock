<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AllStaffLive.ascx.cs" Inherits="RockWeb.Plugins.cc_newspring.Blocks.AllStaffLive.AllStaffLive" %>

<asp:HiddenField ID="liveFeedStatus" runat="server" />

<% if ( liveFeedStatus.Value.Equals( "on" ) )
   { %>
<div class="col-md-12">

    <h1>
        <asp:Literal ID="liveHeading" runat="server" /></h1>

    <asp:HiddenField ID="localIP" runat="server" />
    <script>var localIP = <%= localIP.Value %></script>

    <div id="main-content">
        <div class="live_player">
            <div class="live_player_wrapper">
                <div id="live_feed" class="live_feed"></div>
            </div>
            <div id="live_jw_player"></div>
            <script src='//player.ooyala.com/v3/MWNjYzYxMWIxNjNkMzRmYThlN2Q1MWZl?namespace=live_player'></script>
        </div>
    </div>

    <script type="text/javascript" src="../plugins/cc_newspring/Blocks/AllStaffLive/Scripts/scripts.js"></script>
    <script type="text/javascript" src="../Plugins/cc_newspring/Blocks/AllStaffLive/Scripts/jwplayer.js"></script>
    <script type="text/javascript" src="../Plugins/cc_newspring/Blocks/AllStaffLive/Scripts/jwplayer.html5.js"></script>
</div>
<% } %>