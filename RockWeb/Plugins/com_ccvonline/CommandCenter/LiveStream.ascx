<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LiveStream.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.CommandCenter.LiveStream" %>

<h1>Command Center</h1>
<div style="max-width: 500px;" class="container">
    <Rock:NotificationBox id="ntbAlert" runat="server" NotificationBoxType="Danger" Visible="false" Text="There are currently no streams available." />
</div>

<asp:Repeater ID="rptvideostreams" runat="server" >
    <ItemTemplate>
        <div class="col-md-4">
            <div class="panel panel-default">                
                <div class="panel-body">   
                    <h3><%# Eval("[1]") %></h3>
                    <div class="videocontent">
                        <a id='<%# Eval("[0]") %>' data-flashfit="true" class="live-player"></a>
                    </div>                                                         
                    <br />
                    <div style="text-align: center;">
                        <a id='<%# Eval("[0]") %>' class="btn btn-default audio-toggle muted" >
                            <i class="fa fa-volume-off"></i>
                            <span></span>
                        </a>   
                    </div>                 
                </div>
            </div>
        </div>

        <script type="text/javascript">
                // setup player
                flowplayer('<%# Eval("[0]") %>', "/Plugins/com_ccvonline/CommandCenter/Assets/flowplayer.commercial-3.2.18.swf",
                    {
                        key: '#$392ba7eb81984ddb47a',
                        controls: {
                            time: false,
                            scrubber: false
                        },
                        plugins: {
                            rtmp: {
                                url: '/Plugins/com_ccvonline/CommandCenter/Assets/flowplayer.rtmp-3.2.13.swf',
                            }
                        },
                        clip: {
                            url: '<%# Eval("[2]") %>',
			                live: true,
			                provider: 'rtmp',
			                scaling: 'scale',
			                onStart: function () { MutePlayers(); }
			    }
			});
        </script>

    </ItemTemplate>
</asp:Repeater>


<script type="text/javascript">   

    // mute all videos on load except the first one
    function MutePlayers() {
        $f('*').each(function () {
            this.mute();
        });

        $f('*').first().unmute();
    }

    // untoggle first button
    $('.audio-toggle').first().each(function () {
        $(this).addClass('enabled');
        $(this).removeClass('muted');
        $(this).addClass('btn-primary');
        $(this).removeClass('btn-default');
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
            $(this).addClass('btn-default');
        });

        // mute all videos
        $f('*').each(function () {
            this.mute();
        });

        // get id of video player from button id
        var playerId = $(this).attr('id');

        // enabled selected video unless it is the active one, then mute
        if (currentItem) {
            $f(playerId).mute();
        } else {
            $(this).addClass('enabled');
            $(this).addClass('btn-primary');
            $(this).removeClass('btn-default');
            $f(playerId).unmute();
        }
    });
</script>