<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PageLiquid.ascx.cs" Inherits="RockWeb.Blocks.Cms.PageLiquid" %>
<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>
    <asp:PlaceHolder ID="phContent" runat="server"></asp:PlaceHolder>
</ContentTemplate>
</asp:UpdatePanel>
