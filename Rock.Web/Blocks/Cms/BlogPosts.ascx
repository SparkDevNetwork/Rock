<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BlogPosts.ascx.cs" Inherits="Rock.Web.Blocks.Cms.BlogPosts" %>

<asp:UpdatePanel ID="upPosts" runat="server" UpdateMode="Always">
<ContentTemplate>
    <asp:BulletedList ID="blPosts" runat="server">
    </asp:BulletedList>
</ContentTemplate>
</asp:UpdatePanel>

