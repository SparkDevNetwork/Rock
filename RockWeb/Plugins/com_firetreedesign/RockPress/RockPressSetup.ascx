<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RockPressSetup.ascx.cs" Inherits="RockWeb.Plugins.com_firetreedesign.RockPress.RockPressSetup" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-wordpress"></i> RockPress Setup</h1>
            </div>

            <div class="panel-body">
                <asp:Literal ID="lDetails" runat="server" />

                <div class="actions">
                    <asp:LinkButton ID="lbDelete" runat="server" Text="Delete API Key" CssClass="btn btn-danger" OnClick="lbDelete_Click" />
                    <asp:LinkButton ID="lbGenerate" runat="server" Text="Generate API Key" CssClass="btn btn-primary" OnClick="lbGenerate_Click" />
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>