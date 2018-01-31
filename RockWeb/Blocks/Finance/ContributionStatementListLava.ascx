<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContributionStatementListLava.ascx.cs" Inherits="RockWeb.Blocks.Finance.ContributionStatementListLava" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">
            <asp:Literal ID="lResults" runat="server" />

            <asp:Literal ID="lDebug" runat="server" />
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
