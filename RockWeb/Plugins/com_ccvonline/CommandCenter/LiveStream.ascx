<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LiveStream.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.CommandCenter.LiveStream" %>

<br />

<asp:Repeater ID="rptvideostreams" runat="server" >
    <ItemTemplate>
        <div class="col-md-4">
            <div class="panel panel-default">
                <h3 class="panel-heading"><%# Eval("[0]") %></h3>
                <div class="panel-body">
                    <div class="videocontent">
                        <video id='<%# Eval("[0]") %>' class="video-js vjs-default-skin vjs-live vjs-big-play-centered" controls
                                preload="none" width="auto" height="330" poster="<%# ResolveRockUrl("~~/Assets/images/" + Eval("[0]") + "poster.jpg") %>" data-setup='{ "techOrder": ["flash"] }'>
                            <source src='<%# Eval("[1]") %>' type='rtmp/mp4'>
                            <p class="vjs-no-js">To view this video please enable JavaScript, and consider upgrading to a web browser that <a href="http://videojs.com/html5-video-support/" target="_blank">supports HTML5 video</a></p>
                        </video>
                        <br />
                        <div style="text-align:center;">
                            <button type="button" class="btn btn-lg btn-primary"><i class="fa fa-microphone"></i> Mute</button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </ItemTemplate>
</asp:Repeater>


