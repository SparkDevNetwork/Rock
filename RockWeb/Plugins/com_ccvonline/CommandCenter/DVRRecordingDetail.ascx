<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DVRRecordingDetail.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.CommandCenter.DVRRecordingDetail" %>

<asp:UpdatePanel ID="upRecordings" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfRecording" runat="server" />

        <asp:Panel ID="pnlDetails" runat="server">

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
                            <a ID="sharebutton" class=" btn btn-success" data-toggle="collapse" data-target="#sharepanel">Share</a>
                        </div>
                
                        <div id="sharepanel" class="panel panel-default collapse">
                            <div class="panel-body">

                                <div class="col-md-6">
                                    <div class="input-group">
                                        <asp:TextBox ID="starttime" runat="server" CssClass="form-control" ReadOnly="true" />
                                        <span class="input-group-btn">
                                            <a ID="btnStartTime" class="btn btn-default">Start</a>
                                        </span>
                                    </div>
                                </div>

                                <div class="col-md-6">
                                    <div class="input-group">
                                        <asp:TextBox ID="endtime" runat="server" CssClass="form-control" ReadOnly="true" />
                                        <span class="input-group-btn">
                                            <a ID="btnEndTime" class="btn btn-default">End</a>
                                        </span>
                                    </div>
                                </div>

                                <Rock:DataTextBox ID="dtbLink" runat="server" Label="Link" TextMode="Url" Enabled="false" />

                                <Rock:DataTextBox ID="dtbEmailTo" runat="server" Label="To" />

                                <Rock:DataTextBox ID="dtbEmailFrom" runat="server" Label="From" />

                                <Rock:DataTextBox ID="dtbEmailMessage" runat="server" Label="Message" CssClass="form-control" Rows="3" />

                                <Rock:BootstrapButton ID="btnSendEmail" runat="server" Text="Send" CssClass="btn btn-default pull-right" /> 

                            </div>
                        </div> 


        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

<%-- Setup video player --%>
<script type="text/javascript">

    var recordingName = '<%=hfRecording.Value%>';

    function pageLoad() {
        // setup player
        flowplayer("player", "/Plugins/com_ccvonline/CommandCenter/Assets/flowplayer-3.2.18.swf",
			{
			    plugins: {
			        f4m: { url: '/Plugins/com_ccvonline/CommandCenter/Assets/flowplayer.f4m-3.2.10.swf' },
			        httpstreaming: { url: '/Plugins/com_ccvonline/CommandCenter/Assets/flowplayer.httpstreaming-3.2.11.swf' },
			    },
			    clip: {
			        url: recordingName + '/manifest.f4m?DVR&wowzadvrplayliststart=0',
			        urlResolvers: ['f4m'],
			        provider: 'httpstreaming',
			        baseUrl: 'http://ccvwowza:1935/commandcenter/',
			        autoplay: true,
                    scaling: 'fit'
			    }
			});
    }

    <%-- Change video --%>
    function ChangeRecording( recording ) {
        $f("player").play(recording + '/manifest.f4m?DVR&wowzadvrplayliststart=0');
    }

</script>
