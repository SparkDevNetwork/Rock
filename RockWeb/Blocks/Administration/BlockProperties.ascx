<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BlockProperties.ascx.cs" Inherits="RockWeb.Blocks.Administration.BlockProperties" %>

<script type="text/javascript">

    // If this control is in a modal window, hide this form's save button and bind the modal popup
    // Save button to this form's save click event

    Sys.Application.add_load(function () {

        if ($('#modal-popup_panel', window.parent.document)) {
            $('#modal-popup_panel a.btn.primary', window.parent.document).click(function () {
                $('#<%= btnSave.ClientID %>').click();
            });
            $('#non-modal-actions').hide();
        }

    });

</script>

<div class="admin-dialog">

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert-message block-massage error"/>

    <asp:PlaceHolder ID="phContent" runat="server">

        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert-message block-message error"/>

        <fieldset>
            <legend>Settings</legend>
            <Rock:DataTextBox ID="tbBlockName" runat="server" SourceTypeName="Rock.CMS.BlockInstance, Rock" PropertyName="Name" />
            <Rock:DataTextBox ID="tbCacheDuration" runat="server" SourceTypeName="Rock.CMS.BlockInstance, Rock" PropertyName="OutputCacheDuration" LabelText="Cache Duration" />
        </fieldset>

        <asp:placeholder id="phAttributes" runat="server"></asp:placeholder>

        <asp:ValidationSummary ID="valSummaryBottom" runat="server" HeaderText="Please Correct the Following" CssClass="alert-message block-message error"/>

        <div id="non-modal-actions" class="actions">
            <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="btn primary" OnClick="btnSave_Click " />
        </div>

    </asp:PlaceHolder>

</div>

