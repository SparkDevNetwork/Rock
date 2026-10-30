<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContributionStatementListLava.ascx.cs" Inherits="RockWeb.Blocks.Finance.ContributionStatementListLava" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">
            <Rock:NotificationBox ID="nbNotAuthorized" runat="server" NotificationBoxType="Warning" Text="You are not authorized to view this person's contribution statements." Visible="false" />
            <asp:Literal ID="lResults" runat="server" />

            <asp:Literal ID="lDebug" runat="server" />
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
