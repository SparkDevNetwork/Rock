<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupVideo.ascx.cs" Inherits="RockWeb.Blocks.Group.GroupVideo" %>

<h3><asp:Literal ID="lParentGroup" runat="server"></asp:Literal></h3>

<table>
<tr>
    <td>
        <div class="group-tree" video-id="video_<%=BlockInstance.Id %>">
            <asp:PlaceHolder ID="phTreeView" runat="server"></asp:PlaceHolder>
        </div>
    </td>
    <td>
        <video id='video_<%=BlockInstance.Id %>' class='video-js vjs-default-skin'
            controls width='640' height='264' preload='auto' style='display:none'>
        </video>
    </td>
</tr>
</table>






