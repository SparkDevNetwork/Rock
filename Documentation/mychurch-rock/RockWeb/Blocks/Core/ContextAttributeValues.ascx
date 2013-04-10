<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContextAttributeValues.ascx.cs" Inherits="RockWeb.Blocks.Core.ContextAttributeValues" %>

<asp:UpdatePanel ID="upAttributeValues" runat="server" class="context-attribute-values">
<ContentTemplate>
    <h3><%=_category%></h3>
    <asp:PlaceHolder ID="phAttributes" runat="server"></asp:PlaceHolder>
</ContentTemplate>
</asp:UpdatePanel>

