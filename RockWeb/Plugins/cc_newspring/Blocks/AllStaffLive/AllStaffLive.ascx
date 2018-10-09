<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AllStaffLive.ascx.cs" Inherits="RockWeb.Plugins.cc_newspring.Blocks.AllStaffLive.AllStaffLive" %>

<asp:HiddenField ID="liveFeedStatus" runat="server" />

<% if ( liveFeedStatus.Value.Equals( "on" ) )
   { %>

    <asp:HiddenField ID="localIP" runat="server" />
    <script>var localIP = <%= localIP.Value %></script>
    
    <div id="la1-video-player" data-embed-id="4259abb2-409f-4864-8296-986982c5fd32"></div>
    <script type="application/javascript" data-main="//control.livingasone.com/webplayer/loader.js" src="//control.livingasone.com/webplayer/require.js"></script>

<% } %>