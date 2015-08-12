<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AutoAssignNumber.ascx.cs" Inherits="RockWeb.Plugins.com_CentralAZ.Utility.AutoAssignNumber" %>

<asp:UpdatePanel ID="upnlContent" runat="server" Visible="false">
    <ContentTemplate>
        <asp:LinkButton ID="lbAutoAssign" runat="server" Enabled="true" CssClass="btn btn-default btn-xs" OnClick="lbAutoAssign_Click"><i class="fa fa-plus-circle"></i> Assign </asp:LinkButton>
        <Rock:NotificationBox runat="server" id="nbMessage" NotificationBoxType="Success"></Rock:NotificationBox>
    </ContentTemplate>
</asp:UpdatePanel>
