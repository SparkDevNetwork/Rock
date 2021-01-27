<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StoreHeader.ascx.cs" Inherits="RockWeb.Blocks.Store.StoreHeader" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Literal ID="lOutput" runat="server"></asp:Literal>

        <asp:Literal ID="lDebug" Visible="false" runat="server"></asp:Literal>

        <asp:Panel ID="pnlConfigureOrganization" runat="server" CssClass="alert alert-warning">
            <strong>Rock Shop Configuration Needed </strong>
            <div class="row">
                <div class="col-sm-10">
                    The Rock Shop is not yet configured for your server. Please complete the configuration to enable showing pricing and the installation of plugins.
                </div>
                <div class="col-sm-2">
                    <Rock:BootstrapButton ID="btnConfigureRockShop" runat="server" Text="Configure Rock Shop" CssClass="btn btn-warning" OnClick="btnConfigureRockShop_Click" />
                </div>
            </div>
        </asp:Panel>

        <asp:Literal id="litOrganizationLava" runat="server" />
    </ContentTemplate>
</asp:UpdatePanel>
