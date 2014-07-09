<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LiveStream.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.CommandCenter.LiveStream" %>

<h1>Command Center</h1>

<asp:Repeater ID="rptvideostreams" runat="server" >
    <ItemTemplate>
        <div class="col-md-4">
            <div class="panel panel-default">
                <h3 style="text-align: center;"><%# Eval("Name") %></h3>
                <div class="panel-body">                    
                    <video id='<%# Eval("Id") %>' class="video-js vjs-default-skin vjs-live vjs-big-play-centered videocontent" controls autoplay
                            preload="none" width="auto" height="330" poster="<%# ResolveRockUrl("~~/Assets/images/" + Eval("Name") + "poster.jpg") %>" data-setup='{ "techOrder": ["flash"] }'>
                        <source src='<%# Eval("Stream") %>' type='rtmp/mp4'>
                        <p class="vjs-no-js">To view this video please enable JavaScript, and consider upgrading to a web browser that <a href="http://videojs.com/html5-video-support/" target="_blank">supports HTML5 video</a></p>
                    </video>
                    <br />
                    <div class="cc-button audio-toggle muted" style="text-align:center;">
                        <i class="fa fa-volume-off"></i>
                        <span></span>
                    </div>                    
                </div>
            </div>
        </div>
    </ItemTemplate>
</asp:Repeater>


<script>

    videojs("campus_5").ready(function () {
        var myPlayer = this;

        myPlayer.play();
        myPlayer.muted(true);
    });

    $('div.audio-toggle').click(function (event) {
        // set flag if user clicked on current item this will note that they wish to mute current channel
        var currentItem = false;
        if ($(this).is('.enabled')) {
            currentItem = true;
        }

        // mute all videos
        $('.audio-toggle').each(function (index, value) {
            $(this).removeClass('enabled');
            $(this).addClass('muted');
        });

        //$("*").each(function () {
        //    this.muted(true);
        //});

        // get id of video player
        var playerId = $(this).siblings('div.video-js').attr('id');

        // enabled selected video unless it is the active one, then mute
        if (currentItem) {
            $videojs(playerId).muted(true);
        } else {
            $(this).addClass('enabled');
            $videojs(playerId).muted(false);
        }
    });
</script>

<style>
    .cc-button {
	padding: 6px;
	background: #7c7c7c;
	background: -moz-linear-gradient(top,  #7c7c7c 0%, #6b6b6b 26%, #353535 100%);
	background: -webkit-gradient(linear, left top, left bottom, color-stop(0%,#7c7c7c), color-stop(26%,#6b6b6b), color-stop(100%,#353535));
	background: -webkit-linear-gradient(top,  #7c7c7c 0%,#6b6b6b 26%,#353535 100%);
	background: -o-linear-gradient(top,  #7c7c7c 0%,#6b6b6b 26%,#353535 100%);
	background: -ms-linear-gradient(top,  #7c7c7c 0%,#6b6b6b 26%,#353535 100%);
	background: linear-gradient(to bottom,  #7c7c7c 0%,#6b6b6b 26%,#353535 100%);
	filter: progid:DXImageTransform.Microsoft.gradient( startColorstr='#7c7c7c', endColorstr='#353535',GradientType=0 );
	border: 1px solid #222;
	margin: 12px auto 0 auto;
	border-radius: 6px;
	width: 100px;
	text-transform: uppercase;
	font-family: DINWebCondensedLight, Helvetica, Arial, sans-serif;
	cursor: pointer; 
	display: inline-block;
	margin-right: 24px;
}

.ccbutton:hover {
	background: #7c7c7c;
	background: -moz-linear-gradient(top,  #7c7c7c 0%, #898989 26%, #5b5b5b 100%);
	background: -webkit-gradient(linear, left top, left bottom, color-stop(0%,#7c7c7c), color-stop(26%,#898989), color-stop(100%,#5b5b5b));
	background: -webkit-linear-gradient(top,  #7c7c7c 0%,#898989 26%,#5b5b5b 100%);
	background: -o-linear-gradient(top,  #7c7c7c 0%,#898989 26%,#5b5b5b 100%);
	background: -ms-linear-gradient(top,  #7c7c7c 0%,#898989 26%,#5b5b5b 100%);
	background: linear-gradient(to bottom,  #7c7c7c 0%,#898989 26%,#5b5b5b 100%);
	filter: progid:DXImageTransform.Microsoft.gradient( startColorstr='#7c7c7c', endColorstr='#5b5b5b',GradientType=0 );
}

.cc-button.enabled {
	background: #a7cfdf;
	background: -moz-linear-gradient(top,  #a7cfdf 0%, #23538a 100%);
	background: -webkit-gradient(linear, left top, left bottom, color-stop(0%,#a7cfdf), color-stop(100%,#23538a));
	background: -webkit-linear-gradient(top,  #a7cfdf 0%,#23538a 100%);
	background: -o-linear-gradient(top,  #a7cfdf 0%,#23538a 100%);
	background: -ms-linear-gradient(top,  #a7cfdf 0%,#23538a 100%);
	background: linear-gradient(to bottom,  #a7cfdf 0%,#23538a 100%);
	filter: progid:DXImageTransform.Microsoft.gradient( startColorstr='#a7cfdf', endColorstr='#23538a',GradientType=0 );
	border: 1px solid #999;
}

.cc-button i {
	font-size: 16px;
	margin-right: 14px;
}

.audio-toggle i:before {
	content: "\f026"; 
}

.audio-toggle.enabled i:before {
	content: "\f028";
}

.audio-toggle span:before, .audio-toggle.muted span:before {
	content: "Listen";
}

.audio-toggle.enabled span:before {
	content: "Mute";
}

.fullscreen-toggle i:before {
	content: "\f065"; 
}
</style>
