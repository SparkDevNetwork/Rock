<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BemaPackageInstaller.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.ClientTools.BemaPackageInstaller" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbInfo" runat="server" Visible="false" />
        <asp:Panel ID="pnlView" runat="server">
            <div class="alert alert-info">
                <p>A new BEMA Package is available for installation.</p>
                <asp:LinkButton ID="lbInstall" runat="server" OnClick="lbInstall_Click" CssClass="btn btn-primary btn-install">Install</asp:LinkButton>
                <asp:Literal ID="lMessages" runat="server" />

            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
