<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LiveStream.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.CommandCenter.LiveStream" %>

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

            // Intentionally not using the latest version of flowplayer.  There are some issues with using version
            // 3.2.18. The issues are not being able to toggle mute when a live stream in unavailable.  Also, 
            // the newest version of the httpstreaming plugin is unreliable when passing clip duration parameters.
            flowplayer('<%# Eval("[0]") %>', "/Plugins/church_ccv/CommandCenter/Assets/flowplayer.commercial-3.2.9.swf",
                {
                    key: '#$392ba7eb81984ddb47a',
                    plugins: {
                        controls: {
                            time: false,
                            scrubber: false
                        },
                        rtmp: {
                            url: '/Plugins/church_ccv/CommandCenter/Assets/flowplayer.rtmp-3.2.9.swf',
                        }
                    },
                    clip: {
                        url: '<%# Eval("[2]") %>',
                        live: true,
                        provider: 'rtmp',
                        scaling: 'scale',
                        onStart: function () { SetupPlayers(); }
                    },
                    showErrors: false
			    });
        </script>

    </ItemTemplate>
</asp:Repeater>


<script type="text/javascript">   

    function pageLoad() {

        // untoggle first button
        $('.audio-toggle').first().each(function () {
            $(this).addClass('enabled');
            $(this).removeClass('muted');
            $(this).addClass('btn-primary');
            $(this).removeClass('btn-default');
        });

        //MutePlayers();    
    }

    // mute all videos on load except the first one.  This logic
    // is used instead of .First() because flow player does not 
    // support it.
    function SetupPlayers() {
        var counter = 0;
        $f('*').each(function () {
            counter++;
            if (counter === 1) {
                this.unmute();
            } else {
                this.mute();
            }
        });
    }

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