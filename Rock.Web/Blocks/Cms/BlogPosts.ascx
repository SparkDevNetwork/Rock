<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BlogPosts.ascx.cs" Inherits="Rock.Web.Blocks.Cms.BlogPosts" %>

<asp:UpdatePanel ID="upPosts" runat="server" UpdateMode="Always">
<ContentTemplate>
    <asp:Literal ID="lPosts" runat="server"></asp:Literal>

    <a ID="aOlder" class="button older-posts" runat="server">Older Posts</a>
    <a ID="aNewer" class="button newer-posts" runat="server">Newer Posts</a>

</ContentTemplate>
</asp:UpdatePanel>

