<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AllStaffLive.ascx.cs" Inherits="RockWeb.Plugins.cc_newspring.Blocks.AllStaffLive.AllStaffLive" %>

<asp:HiddenField ID="liveFeedStatus" runat="server" />

<% if ( liveFeedStatus.Value.Equals( "on" ) )
   { %>
<div class="col-md-12">

    <h2>
        <asp:Literal ID="liveHeading" runat="server" />
    </h2>

    <asp:HiddenField ID="localIP" runat="server" />
    <script>var localIP = <%= localIP.Value %></script>

    <div id="live_feed" class="live_feed"></div>

    <script>
        var playerParam = {
            "pcode": "E1dWM6UGncxhent7MRATc3hmkzUD",
            "playerBrandingId": "ZmJmNTVlNDk1NjcwYTVkMzAzODkyMjg0",
            autoplay: false,
            "skin": {
                "config": "//s3.amazonaws.com/ns.assets/newspring/skin.new.json"
            }
        };

        if (window.OO) {
            OO.ready(function() {
                OO.Player.create(
                    'live_feed',
                    'ZjeTJwajryMI8LMaVC3hFYS1xs3z3TA8',
                    playerParam
                );
            });
        } else {
            setTimeout(firePlayer, 80);
        }
    </script>
</div>
<% } %>