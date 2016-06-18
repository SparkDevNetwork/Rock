<%@ Control Language="C#" AutoEventWireup="true" CodeFile="HtmlEditorMergeFieldPicker.ascx.cs" Inherits="RockWeb.Blocks.Utility.HtmlEditorMergeFieldPicker" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <h1>
            <asp:Literal ID="lTitle" runat="server">Select Merge Field</asp:Literal>
        </h1>

        <!-- set a min height so that dialog is pre-sized nicely when the mergefieldpicker drops down  -->
        <div style="min-height: 350px">
            <Rock:MergeFieldPicker ID="mfpHtmlEditor" runat="server" Label="" OnSelectItem="mfpHtmlEditor_SelectItem" />

            <div class="js-mergefieldpicker-result">
                <asp:HiddenField ID="hfResultValue" runat="server" />
            </div>
        </div>

        <div class="row">
            <div class="margin-all-md actions pull-right">
                <a class="btn btn-primary js-select-mergefield-button">OK</a>
                <a class="btn btn-link js-cancel-mergefield-button">Cancel</a>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
