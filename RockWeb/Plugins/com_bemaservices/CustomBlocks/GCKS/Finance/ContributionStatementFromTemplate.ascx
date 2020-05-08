<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContributionStatementFromTemplate.ascx.cs" Inherits="RockWeb.Plugins.com_visitgracechurch.Finance.ContributionStatementFromTemplate" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Literal ID="lResults" runat="server" />

        <asp:Literal ID="lDebug" runat="server" />

    </ContentTemplate>
</asp:UpdatePanel>
