<%@ Control Language="C#" AutoEventWireup="true" CodeFile="UpdateBemaCode.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.Support.UpdateBemaCode" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server">
            <div class="alert alert-info">
                <p>A newer version of BEMA Custom code is available for installation.</p>
                <asp:LinkButton ID="lbInstall" runat="server" OnClick="lbInstall_Click" CssClass="btn btn-primary btn-install">Install</asp:LinkButton>
                <asp:Literal ID="lMessages" runat="server" />

            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
