<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupDetailLava.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupDetailLava" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

       <asp:Literal ID="lContent" runat="server"></asp:Literal>

        <asp:Literal ID="lDebug" runat="server"></asp:Literal>

    </ContentTemplate>
</asp:UpdatePanel>
