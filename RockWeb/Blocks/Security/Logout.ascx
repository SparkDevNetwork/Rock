<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Logout.ascx.cs" Inherits="RockWeb.Blocks.Security.Logout" %>



<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Literal ID="lOutput" runat="server" />

        <asp:LinkButton ID="lbAdminLogout" runat="server" CssClass="btn btn-warning" Text="Logout" Visible="false" OnClick="lbAdminLogout_Click" />

    </ContentTemplate>
</asp:UpdatePanel>