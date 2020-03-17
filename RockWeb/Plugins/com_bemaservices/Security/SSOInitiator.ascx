<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SSOInitiator.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.Security.SSOInitiator" %>

<asp:UpdatePanel ID="upnlInfo" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbAlert" runat="server" />
    </ContentTemplate>
 </asp:UpdatePanel>