<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CkEditorFileBrowser.ascx.cs" Inherits="RockWeb.Blocks.Utility.CkEditorFileBrowser" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        

        <div class="js-mergefieldpicker-result">
            <asp:HiddenField ID="hfResultValue" runat="server" />
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
