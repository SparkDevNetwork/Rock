<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PageProperties.ascx.cs" Inherits="RockWeb.Blocks.Administration.PageProperties" %>

<script type="text/javascript">
    
    // If this control is in a modal window, hide this form's save button and bind the modal popup
    // Save button to this form's save click event

    Sys.Application.add_load(function () {

        if ($('#modal-popup', window.parent.document)) {
            $('#modal-popup a.btn.primary', window.parent.document).click(function () {
                $('#<%= btnSave.ClientID %>').click();
            });
            $('div.admin-dialog .actions').hide();
        }

    });

</script>

<div class="admin-dialog">

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert-message block-massage error"/>

    <asp:PlaceHolder ID="phContent" runat="server">

        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert-message block-message error"/>

        <fieldset>
            <legend>Basic Settings</legend>
            <Rock:DataDropDownList ID="ddlParentPage" runat="server" LabelText="Parent Page" SourceTypeName="Rock.CMS.Page, Rock" PropertyName="ParentPageId"/>
            <Rock:DataTextBox ID="tbPageName" runat="server" SourceTypeName="Rock.CMS.Page, Rock" PropertyName="Name"/>
            <Rock:DataTextBox ID="tbPageTitle" runat="server" SourceTypeName="Rock.CMS.Page, Rock" PropertyName="Title"/>
            <Rock:DataDropDownList ID="ddlLayout" runat="server" SourceTypeName="Rock.CMS.Page, Rock" PropertyName="Layout"/>
            <Rock:DataTextBox ID="tbDescription" runat="server" TextMode="MultiLine" Rows="3" SourceTypeName="Rock.CMS.Page, Rock" PropertyName="Description" />
        </fieldset>

        <fieldset>
            <legend>Menu Display Options</legend>
            <Rock:DataDropDownList ID="ddlMenuWhen" runat="server" LabelText="Display When" SourceTypeName="Rock.CMS.Page, Rock" PropertyName="DisplayInNavWhen"/>
            <Rock:LabeledCheckBox ID="cbMenuDescription" runat="server" LabelText="Show Description"/>
            <Rock:LabeledCheckBox ID="cbMenuIcon" runat="server" LabelText="Show Idon"/>
            <Rock:LabeledCheckBox ID="cbMenuChildPages" runat="server" LabelText="Show Child Pages"/>
        </fieldset>

        <fieldset id="fsAttributes" runat="server" visible="false">
            <legend>Attributes</legend>
            <placeholder id="phAttributes" runat="server"></placeholder>
        </fieldset>

        <fieldset>
            <legend>Advanced Settings</legend>
            <Rock:LabeledCheckBox ID="cbRequiresEncryption" runat="server" LabelText="Force SSL"/>
            <Rock:LabeledCheckBox ID="cbEnableViewState" runat="server" LabelText="Enable ViewState"/>
            <Rock:LabeledCheckBox ID="cbIncludeAdminFooter" runat="server" LabelText="Allow Configuration"/>
            <Rock:DataTextBox ID="tbCacheDuration" runat="server" LabelText="Cache Duration" SourceTypeName="Rock.CMS.Page, Rock" PropertyName="OutputCacheDuration"/>

        </fieldset>

        <asp:ValidationSummary ID="valSummaryBottom" runat="server" HeaderText="Please Correct the Following" CssClass="alert-message block-message error"/>

        <div class="actions">
            <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="btn primary" OnClick="btnSave_Click " />
        </div>

    </asp:PlaceHolder>

</div>

