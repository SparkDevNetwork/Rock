<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DVRRecordingDetail.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.CommandCenter.DVRRecordingDetail" %>

<asp:UpdatePanel ID="upRecordings" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfRecording" runat="server" />
        <asp:HiddenField ID="hfClipStart" runat="server" />
        <asp:HiddenField ID="hfClipDuration" runat="server" />

        <Rock:NotificationBox ID="mdWarning" runat="server" Text="Recording not available" Visible="false" NotificationBoxType="Danger" />

        <asp:Panel ID="pnlVideo" runat="server">

            <div class="row">
                <div class="col-sm-6">
                    <h3><asp:Literal ID="lblTitle" runat="server" /></h3>
                </div> 
                <div class="col-sm-6">
                    <div class="pull-right">
                        <Rock:HighlightLabel ID="lblCampus" LabelType="Campus" runat="server" />
                        <Rock:HighlightLabel ID="lblVenueType" LabelType="Warning" runat="server" />
                    </div>
                </div>
            </div>
                        
            <div class="row">
                <div class="col-sm-12">
                    <div class="videocontent">                                          
                        <a id="player" data-flashfit="true"></a>
                    </div>
                </div>
            </div>

        </asp:Panel>


        <asp:Panel ID="pnlControls" runat="server">

            <div class="row">
                <div class="col-sm-12">
                    <div class="servicebuttons">
                        <div class="btn-group">
                            <asp:PlaceHolder id="plcServiceTimeButtons" runat="server" />
                        </div>
                    </div>
                </div>
            </div>

            <div class="sharebutton">
                <a ID="sharebutton" class=" btn btn-success btn-xs" data-toggle="collapse" data-target="#sharepanel">Share</a>
            </div>
                
            <div id="sharepanel" class="panel panel-default collapse">
                <div class="panel-body">

                    <div class="row">
                        <div class="col-sm-4">
                            <div class="input-group">
                                <input type="text" ID="starttime" class="form-control" />
                                <span class="input-group-btn">
                                    <a ID="btnStartTime" class="btn btn-default" onclick="javascript: GetVideoStartTime()" >Start</a>
                                </span>
                            </div>
                        </div>

                        <div class="col-sm-4">
                            <div class="input-group">
                                <input ID="endtime" class="form-control" />
                                <span class="input-group-btn">
                                    <a ID="btnEndTime" class="btn btn-default" onclick="javascript: GetVideoEndTime()">End</a>
                                </span>
                            </div>
                        </div>

                        <div class="col-sm-4">
                            <input id="totaltime" class="form-control" placeholder="Total" />
                        </div>
                    </div>

                    <br />

                    <Rock:RockTextBox ID="tbLink" runat="server" Label="Link" TextMode="Url" CssClass="js-urltextbox" />

                    <Rock:RockTextBox ID="tbEmailTo" runat="server" Label="To" TextMode="Email" />

                    <Rock:RockTextBox ID="tbEmailFrom" runat="server" Label="From" TextMode="Email" />

                    <Rock:RockTextBox ID="tbEmailMessage" runat="server" Label="Message" CssClass="form-control" Rows="3" TextMode="MultiLine" />

                    <Rock:BootstrapButton ID="btnSendEmail" runat="server" Text="Send" OnClick="btnSendEmail_Click" CssClass="btn btn-default pull-right" /> 

                </div>
            </div> 

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>


<script type="text/javascript">

    var recordingurl = '<%=hfRecording.Value%>';
    var recordingstarttime;
    var recordingduration;
    var recordingtotaltime;

    var clipStart = '<%=hfClipStart.Value%>';
    var clipDuration = '<%=hfClipDuration.Value%>';

    function pageLoad() {
        
        var clipStartEndUrl;

        if (clipStart != '' && clipDuration != '')
        {
            clipStartEndUrl = clipStart + '&wowzadvrplaylistduration=' + clipDuration;
        }
        else
        {
            clipStartEndUrl = '0';
        }

        // setup player
        flowplayer("player", "/Plugins/com_ccvonline/CommandCenter/Assets/flowplayer-3.2.18.swf",
			{
			    plugins: {
			        f4m: { url: '/Plugins/com_ccvonline/CommandCenter/Assets/flowplayer.f4m-3.2.10.swf' },
			        httpstreaming: { url: '/Plugins/com_ccvonline/CommandCenter/Assets/flowplayer.httpstreaming-3.2.11.swf' },
			    },
			    clip: {
			        url: recordingurl + '/manifest.f4m?DVR&wowzadvrplayliststart=' + clipStartEndUrl,
			        urlResolvers: ['f4m'],
			        provider: 'httpstreaming',
			        baseUrl: 'http://ccvwowza:1935/commandcenter/',
			        autoplay: true,
			        scaling: 'fit'
			    }
			});
    }

    // Change recording playback
    function ChangeRecording(recording) {
        recordingurl = recording;

        $f("player").play(recordingurl + '/manifest.f4m?DVR&wowzadvrplayliststart=0');
    }

    // Get start time
    function GetVideoStartTime() {
        recordingstarttime = $f("player").getTime().toString();

        var time = recordingstarttime.split('.', 1);
        time = (time / 60).toFixed(0) + ":" + (time % 60);

        $('#starttime').val(time.toString());

        CalculateTotalAndUrl();
    }

    // Get start time
    function GetVideoEndTime() {
        recordingduration = $f("player").getTime().toString();

        var time = recordingduration.split('.', 1);
        time = (time / 60).toFixed(0) + ":" + (time % 60);

        $('#endtime').val(time.toString());

        CalculateTotalAndUrl();
    }

    // Calculate total time and url
    function CalculateTotalAndUrl() {

        if ( recordingstarttime != null && recordingduration != null)
        {
            // calculate total time
            recordingtotaltime = recordingduration - recordingstarttime;

            var total = recordingtotaltime.toString().split('.', 1);
            total = (total / 60).toFixed(0) + ":" + (total % 60);

            $("#totaltime").val(total);

            // generate url
            var url = 'http://localhost:50345/page/361?ClipUrl=' + recordingurl + '&ClipStart=' + (recordingstarttime*1000) + '&ClipDuration=' + (recordingduration*1000);
            $(".js-urltextbox").val(url);
        }
    }

</script>
