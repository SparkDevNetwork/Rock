<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FileManager.ascx.cs" Inherits="RockWeb.Blocks.Cms.FileManager" %>

<asp:UpdatePanel runat="server" ID="upPanel">
    <ContentTemplate>
        <asp:LinkButton ID="btnFileBrowser" runat="server" CssClass="btn btn-default" OnClick="btnFileBrowser_Click" >
            Browse <i class="fa fa-files-o"></i>
        </asp:LinkButton>
        <Rock:ModalDialog ID="mdFileBrowser" runat="server" Visible="false" Title="Browse Files" >
            <Content>
                <iframe id="iframeFileBrowser" class="js-file-browser" runat="server"  style="width: 100%; height:420px;" scrolling="no" frameBorder="0" />
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>

