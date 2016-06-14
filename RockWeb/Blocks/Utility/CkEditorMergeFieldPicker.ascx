<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CkEditorMergeFieldPicker.ascx.cs" Inherits="RockWeb.Blocks.Utility.CkEditorMergeFieldPicker" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <!-- set a min height so that dialog is pre-sized nicely when the mergefieldpicker drops down  -->
        <div style="min-height: 350px">
            <Rock:MergeFieldPicker ID="mfpCkEditor" runat="server" Label="Pick a Merge Field" OnSelectItem="mfpCkEditor_SelectItem" />

            <div class="js-mergefieldpicker-result">
                <asp:HiddenField ID="hfResultValue" runat="server" />
            </div>
        </div>
        <div class="actions">
            <a class="btn btn-primary js-select-mergefield-button">OK</a>
            <a class="btn btn-link js-cancel-mergefield-button">Cancel</a>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
