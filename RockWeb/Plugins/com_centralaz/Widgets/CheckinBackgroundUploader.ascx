<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckinBackgroundUploader.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Widgets.CheckinBackgroundUploader" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlView" runat="server">
            <div id="divAdmin" runat="server" class="well" visible="false">
                <h2>Admin Panel</h2>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockCheckBoxList ID="cblAllowedThemes" runat="server" RepeatDirection="Horizontal" Label="Allowed Themes" />
                        <Rock:BootstrapButton ID="btnUpdateSettings" runat="server" Text="Save Changes" CssClass="btn btn-primary" OnClick="btnUpdateSettings_Click" />
                    </div>
                </div>
            </div>
            <div class="well">
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlTheme" runat="server" OnSelectedIndexChanged="ddlTheme_SelectedIndexChanged" Label="Theme" AutoPostBack="true" />
                    </div>
                    <div class="col-md-6">
                        <Rock:FileUploader ID="fuPhoto" runat="server" DisplayMode="Button" OnFileUploaded="fuPhoto_FileUploaded" IsBinaryFile="false" Label="Change Background Photo" />
                    </div>
                </div>
            </div>
            <asp:Image ID="imbGalleryItem" runat="server" Style="<%# GetImageSize() %>" />
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
