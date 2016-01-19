<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ThemeStyler.ascx.cs" Inherits="RockWeb.Blocks.Cms.ThemeStyler" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-pencil"></i> Theme Editor</h1>
            </div>
            <div class="panel-body">

                <div class="well clearfix">
                    <Rock:RockDropDownList ID="ddlTheme" runat="server" Label="Theme" OnSelectedIndexChanged="ddlTheme_SelectedIndexChanged" AutoPostBack="true" />
                    <div class="pull-right">
                        <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-primary" Text="Save" Visible="false" OnClick="btnSave_Click" />
                    </div>
                </div>

                <Rock:NotificationBox ID="nbMessages" runat="server" />

                <asp:PlaceHolder ID="phThemeControls" runat="server" EnableViewState="true" />

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
