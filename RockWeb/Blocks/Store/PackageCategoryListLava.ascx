<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PackageCategoryListLava.ascx.cs" Inherits="RockWeb.Blocks.Store.PackageCategoryListLava" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlCategories" runat="server">
            <asp:Literal ID="lOutput" runat="server"></asp:Literal>

            <asp:Literal ID="lDebug" Visible="false" runat="server"></asp:Literal>
        </asp:Panel>

        <asp:Panel ID="pnlError" runat="server" Visible="false">
            <div class="alert alert-warning">
                <h4>Store Currently Not Available</h4>
                <p>We're sorry, the Rock Store is currently not available. Check back soon!</p>
                <small><em><asp:Literal ID="lErrorMessage" runat="server" /></em></small>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
