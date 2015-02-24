<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CkEditorMergeFieldPicker.ascx.cs" Inherits="RockWeb.Blocks.Utility.CkEditorMergeFieldPicker" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:MergeFieldPicker ID="mfpCkEditor" runat="server" Label="Pick a Merge Field" OnSelectItem="mfpCkEditor_SelectItem" />
        <!-- add pnlFiller so that dialog is pre-sized nicely when the mergefieldpicker drops down  -->
        <asp:Panel ID="pnlFiller" runat="server" Height="300px"></asp:Panel>
        <div class="js-mergefieldpicker-result">
            <asp:HiddenField ID="hfResultValue" runat="server" />
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
