<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PageMenu.ascx.cs" Inherits="RockWeb.Blocks.Cms.PageMenu" %>
<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>
    <asp:PlaceHolder ID="phContent" runat="server"></asp:PlaceHolder>
</ContentTemplate>
</asp:UpdatePanel>
