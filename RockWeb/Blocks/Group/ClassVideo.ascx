<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ClassVideo.ascx.cs" Inherits="RockWeb.Blocks.Group.ClassVideo" %>

<h3><asp:Literal ID="lParentGroup" runat="server"></asp:Literal></h3>

<asp:UpdatePanel ID="pnlVideos" runat="server" class="class-videos">
<ContentTemplate>

    <div class="class-toc">
        <asp:PlaceHolder ID="phTreeView" runat="server"></asp:PlaceHolder>
    </div>
    <div class="class-media">
        <video id='video_<%=CurrentBlock.Id %>' class='video-js vjs-default-skin' 
            controls width='640' height='264' preload='auto' style='display:none'>
        </video>
    </div>

    <asp:HiddenField ID="hfVideoUrl" runat="server" />

</ContentTemplate>
</asp:UpdatePanel>






