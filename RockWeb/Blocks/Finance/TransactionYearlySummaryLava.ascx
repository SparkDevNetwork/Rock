<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionYearlySummaryLava.ascx.cs" Inherits="RockWeb.Blocks.Finance.TransactionYearlySummaryLava" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">
            <Rock:NotificationBox ID="nbNotAuthorized" runat="server" NotificationBoxType="Warning" Text="You are not authorized to view this person's giving." Visible="false" />
            <asp:Literal ID="lLavaOutput" runat="server" />
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>





