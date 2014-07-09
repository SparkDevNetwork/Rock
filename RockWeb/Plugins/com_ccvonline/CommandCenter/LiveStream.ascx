<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LiveStream.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.CommandCenter.LiveStream" %>

<h1>Command Center</h1>

<asp:Repeater ID="rptvideostreams" runat="server" >
    <ItemTemplate>
        <div class="col-md-4">
            <div class="panel panel-default">
                
                <div class="panel-body">   
                    <h3 style="text-align: center;"><%# Eval("Name") %></h3>                 
                    <video id='<%# Eval("Id") %>' class="video-js vjs-default-skin vjs-live vjs-big-play-centered videocontent" controls autoplay
                            preload="none" width="auto" height="330" poster="<%# ResolveRockUrl("~~/Assets/images/" + Eval("Name") + "poster.jpg") %>" data-setup='{ "techOrder": ["flash"] }'>
                        <source src='<%# Eval("Stream") %>' type='rtmp/mp4'>
                        <p class="vjs-no-js">To view this video please enable JavaScript, and consider upgrading to a web browser that <a href="http://videojs.com/html5-video-support/" target="_blank">supports HTML5 video</a></p>
                    </video>
                    <br />
                    <div style="text-align: center;">
                        <asp:Button id='<%# Eval("Id") %>' class="btn audio-toggle muted" >
                            <i class="fa fa-volume-off"></i>
                            <span></span>
                        </asp:Button>   
                    </div>                 
                </div>
            </div>
        </div>
    </ItemTemplate>
</asp:Repeater>


<script>
    // mute all videos on load except the first one
    $('.video-js').not($('.video-js').first()).each(function () {
        videojs(this.id).muted(true);
    });

    // untoggle first button
    $('.audio-toggle').first().each(function () {
        $(this).addClass('enabled');
        $(this).removeClass('muted');
    });

    $('.audio-toggle').click(function (event) {
        // set flag if user clicked on current item this will note that they wish to mute current channel
        var currentItem = false;
        if ($(this).is('.enabled')) {
            currentItem = true;
        }

        // toggle button status
        $('.audio-toggle').each(function (index) {
            $(this).removeClass('enabled');
            $(this).addClass('muted');
            $(this).removeClass('btn-primary');
        });

        // mute all videos
        $('.video-js').each(function () {
            videojs(this.id).muted(true);
        });

        // get id of video player
        var playerId = $(this).siblings('.video-js').attr('id');

        // enabled selected video unless it is the active one, then mute
        if (currentItem) {
            videojs(playerId).muted(true);
        } else {
            $(this).addClass('enabled');
            $(this).addClass('btn-primary');
            videojs(playerId).muted(false);
        }
    });
</script>