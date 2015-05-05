<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DVRRecordingDetail.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.CommandCenter.DVRRecordingDetail" %>

<asp:UpdatePanel ID="upRecordings" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfBaseUrl" runat="server" />
        <asp:HiddenField ID="hfRecording" runat="server" />
        <asp:HiddenField ID="hfClipStart" runat="server" />
        <asp:HiddenField ID="hfClipDuration" runat="server" />

        <Rock:NotificationBox ID="mdWarning" runat="server" Text="Recording not available" Visible="false" NotificationBoxType="Danger" />
            
        <asp:Panel ID="pnlVideo" runat="server">
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-play-circle-o"></i> <asp:Literal ID="lblTitle" runat="server" /></h1>
                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="lblCampus" LabelType="Campus" runat="server" />
                        <Rock:HighlightLabel ID="lblVenue" LabelType="Warning" runat="server" />
                    </div>
                </div>
                <div class="panel-body">
                    <div class="row">
                        <div class="col-sm-12">
                            <div class="videocontent">
                                <a id="player"></a>
                            </div>
                        </div>
                    </div>
                    <div class="row">
                        <asp:Panel ID="pnlControls" runat="server">
                            <div class="col-sm-12">
                                <div class="servicebuttons">
                                    <div class="btn-group">
                                        <asp:PlaceHolder id="plcServiceTimeButtons" runat="server" />
                                    </div>
                                </div>
                            </div>
                        </asp:Panel> 
                    </div>

                    <asp:Panel ID="pnlShare" runat="server">
                        <div class="sharebutton clearfix">
                            <a ID="sharebutton" class="btn btn-success btn-xs pull-right" data-toggle="collapse" data-target="#sharepanel"><i class="fa fa-share-alt"></i> Share <i id="chevron-toggle" class="fa fa-chevron-down"></i> </a>
                        </div>
                        <div id="sharepanel" class="collapse">
                            <asp:ValidationSummary ID="valSummary" runat="server" DisplayMode="BulletList" CssClass="alert alert-danger" 
                                ValidationGroup="sharegroup" HeaderText="Please correct the following..." />

                            <div class="row">
                                <div class="col-sm-4">
                                    <div class="input-group margin-b-md">
                                        <input type="text" ID="starttime" class="form-control" />
                                        <span class="input-group-btn">
                                            <a ID="btnStartTime" class="btn btn-default" onclick="javascript: GetVideoStartTime()" >Start</a>
                                        </span>
                                    </div>
                                </div>

                                <div class="col-sm-4">
                                    <div class="input-group margin-b-md">
                                        <input ID="endtime" class="form-control" />
                                        <span class="input-group-btn">
                                            <a ID="btnEndTime" class="btn btn-default" onclick="javascript: GetVideoEndTime()">End</a>
                                        </span>
                                    </div>
                                </div>

                                <div class="col-sm-4">
                                    <input id="totaltime" class="form-control  margin-b-md" placeholder="Total" disabled />
                                </div>
                            </div>

                            <br />

                            <Rock:RockTextBox ID="tbEmailTo" runat="server" Label="To" 
                                TextMode="Email" ValidationGroup="sharegroup" Required="true" />

                            <Rock:RockTextBox ID="tbEmailMessage" runat="server" Label="Message" Help="The URL link will automatically be appended to this message." 
                                CssClass="form-control" Rows="3" TextMode="MultiLine" />

                            <Rock:RockTextBox ID="tbLink" runat="server" Label="Link" Help="This URL link will be included automatically in the email message." 
                                TextMode="Url" CssClass="js-urltextbox" ValidationGroup="sharegroup" Required="true" RequiredErrorMessage="A link hasn't been created yet.  Please set a start and end point." />

                            <Rock:BootstrapButton ID="btnSendEmail" runat="server" Text="Send" OnClick="btnSendEmail_Click" CssClass="btn btn-default pull-right" 
                                CausesValidation="true" ValidationGroup="sharegroup" /> 

                        </div>

                    </asp:Panel>
                </div>
            </div>
        </asp:Panel>
            
        
    </ContentTemplate>
</asp:UpdatePanel>


<script type="text/javascript">

    var recordingurl = '<%=hfRecording.Value%>';
    var baseUrl = '<%=hfBaseUrl.Value%>';
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
        flowplayer('player', '/Plugins/church_ccv/CommandCenter/Assets/flowplayer.commercial-3.2.18.swf',
		{
		    key: '#$392ba7eb81984ddb47a',
			plugins: {
			    f4m: { url: '/Plugins/church_ccv/CommandCenter/Assets/flowplayer.f4m-3.2.9.swf' },
			    httpstreaming: { url: '/Plugins/church_ccv/CommandCenter/Assets/flowplayer.httpstreaming-3.2.9.swf' },
			},
			clip: {
			    url: recordingurl + '/manifest.f4m?DVR&wowzadvrplayliststart=' + clipStartEndUrl,
			    urlResolvers: ['f4m'],
			    provider: 'httpstreaming',
			    baseUrl: 'http://ccvwowza-out.ccvonline.com:1935/commandcenter/',
			    autoPlay: true
			}
		});

        // Setup recording buttons
        $('.servicebutton').first().addClass('active');

        // Toggle share button
        $('#sharebutton').click(function () {
            $('#chevron-toggle').toggleClass('fa-chevron-down fa-chevron-up');
        })
    }

    

    // Change recording playback
    function ChangeRecording(recording) {
        recordingurl = recording;

        // Reset button states
        $('.servicebutton').each(function () {
            $(this).removeClass('active');
        });

        // Set button state to active
        $("[recordingname=" + recordingurl + "]").addClass('active');

        // Play recording
        $f("player").play(recordingurl + '/manifest.f4m?DVR&wowzadvrplayliststart=0');
   
    }

    // Get start time
    function GetVideoStartTime() {
        recordingstarttime = $f("player").getTime().toString();

        var time = FormatSecondsToTime(recordingstarttime);
        $('#starttime').val(time.toString());

        CalculateTotalAndUrl();
    }

    // Get end time
    function GetVideoEndTime() {
        recordingduration = $f("player").getTime().toString();

        var time = FormatSecondsToTime(recordingduration);

        $('#endtime').val(time.toString());

        CalculateTotalAndUrl();
    }

    // Calculate total time and url
    function CalculateTotalAndUrl() {

        if ( recordingstarttime != null && recordingduration != null)
        {
            // calculate total time
            recordingtotaltime = recordingduration - recordingstarttime;

            var total = FormatSecondsToTime(recordingtotaltime);

            $("#totaltime").val(total);

            // generate url
            var url = baseUrl + '?ClipUrl=' + recordingurl + '&ClipStart=' + Math.floor(recordingstarttime*1000) + '&ClipDuration=' + Math.floor((recordingduration*1000) - (recordingstarttime*1000));
            $(".js-urltextbox").val(url);
        }
    }

    // Takes the amount of seconds and formats it into a proper time format
    function FormatSecondsToTime(inputseconds) {
        var sec_num = parseInt(inputseconds, 10); // don't forget the second param
        var hours = Math.floor(sec_num / 3600);
        var minutes = Math.floor((sec_num - (hours * 3600)) / 60);
        var seconds = sec_num - (hours * 3600) - (minutes * 60);

        if (hours < 10) { hours = "0" + hours; }
        if (minutes < 10) { minutes = "0" + minutes; }
        if (seconds < 10) { seconds = "0" + seconds; }
        var time = hours + ':' + minutes + ':' + seconds;
        return time;
    }

</script>
