<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FileManager.ascx.cs" Inherits="RockWeb.Blocks.Cms.FileManager" %>

<asp:UpdatePanel runat="server" ID="upnlContent">
    <ContentTemplate>
    <asp:Panel ID="pnlFileManager" CssClass="panel panel-block" runat="server" >
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-folder-open"></i>
                    File Manager
                </h1>
            </div>
            <div class="panel-body padding-h-none">
        <iframe id="iframeFileBrowser" class="js-file-browser file-browser" runat="server"  style="width: 100%; height: 420px;" scrolling="no" frameBorder="0" />
        <script>

            $(document).ready(function () {
                $('.js-file-browser').fadeIn(50);
                Sys.Application.add_load(function () {
                    $('.js-file-browser').fadeIn(50);
                });
            });
        </script>
        </div>
    </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

