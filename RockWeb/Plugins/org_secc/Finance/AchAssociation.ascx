<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AchAssociation.ascx.cs" Inherits="RockWeb.Plugins.org_secc.Finance.AchAssociation" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title pull-left"><i class="fa  fa-file-text-o"></i> Bank Account and Routing Number Info</h1>
            </div>
            <div class="panel-body">
                <asp:Label ID="lblExportStatus" runat="server" Text="Label"></asp:Label>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>