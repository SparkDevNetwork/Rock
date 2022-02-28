<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StoreHeader.ascx.cs" Inherits="RockWeb.Blocks.Store.StoreHeader" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Literal ID="lOutput" runat="server"></asp:Literal>

        <asp:Literal ID="lDebug" Visible="false" runat="server"></asp:Literal>

        <asp:Panel ID="pnlConfigureOrganization" runat="server" CssClass="alert alert-warning">

            <div class="d-flex flex-wrap flex-sm-nowrap align-items-center">
                <div class="flex-fill">
                    <strong class="d-block">Rock Shop Configuration Needed</strong>
                    The Rock Shop is not yet configured for your server. Please complete the configuration to enable showing pricing and the installation of plugins.
                </div>
                <div class="flex-shrink-0 mt-2 mt-sm-0 ml-sm-3">
                    <Rock:BootstrapButton ID="btnConfigureRockShop" runat="server" Text="Configure Rock Shop" CssClass="btn btn-warning btn-xs" OnClick="btnConfigureRockShop_Click" />
                </div>
            </div>
        </asp:Panel>

        <asp:Literal id="litOrganizationLava" runat="server" />
    </ContentTemplate>
</asp:UpdatePanel>
