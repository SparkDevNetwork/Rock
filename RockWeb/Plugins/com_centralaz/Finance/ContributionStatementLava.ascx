<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContributionStatementLava.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Finance.ContributionStatementLava" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        
        <asp:Literal ID="lContent" runat="server"></asp:Literal>

        <asp:Literal ID="lDebug" runat="server"></asp:Literal>

    </ContentTemplate>
</asp:UpdatePanel>
