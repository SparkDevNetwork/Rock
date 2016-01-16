<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ThemeStyler.ascx.cs" Inherits="RockWeb.Blocks.Cms.ThemeStyler" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-star"></i> Blank Detail Block</h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlblTest" runat="server" LabelType="Info" Text="Label" />
                </div>
            </div>
            <div class="panel-body">


                <Rock:RockDropDownList ID="ddlTheme" runat="server" Label="Theme" OnSelectedIndexChanged="ddlTheme_SelectedIndexChanged" AutoPostBack="true" />

                <Rock:NotificationBox ID="nbMessages" runat="server" />

                <asp:PlaceHolder ID="phThemeControls" runat="server" />
                <asp:Literal ID="lTest" runat="server" />

                <Rock:ColorPicker ID="cpColor" runat="server" Value="#000" Label="My Color" />

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
