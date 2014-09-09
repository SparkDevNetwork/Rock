<%@ control language="C#" autoeventwireup="true" inherits="RockWeb.Blocks.Utility.CkEditorMergeFieldPicker, RockWeb" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:MergeFieldPicker ID="mfpCkEditor" runat="server" Label="Pick a Merge Field" OnSelectItem="mfpCkEditor_SelectItem" />
        <div class="js-mergefieldpicker-result">
            <asp:HiddenField ID="hfResultValue" runat="server" />
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
