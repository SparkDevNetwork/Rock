<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PageProperties.ascx.cs" Inherits="RockWeb.Blocks.Administration.PageProperties" %>

<asp:PlaceHolder id="phClose" runat="server"></asp:PlaceHolder>

<script type="text/javascript">
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

    <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert-message block-message error"/>

    <fieldset>
        <legend>Basic Settings</legend>

        <dl><Rock:DataDropDownList ID="ddlParentPage" runat="server" LabelText="Parent Page"
            SourceTypeName="Rock.CMS.Page, Rock" PropertyName="ParentPageId"></Rock:DataDropDownList></dl>

        <dl><Rock:DataTextBox ID="tbPageName" runat="server"
            SourceTypeName="Rock.CMS.Page, Rock" PropertyName="Name"></Rock:DataTextBox></dl>

        <dl><Rock:DataTextBox ID="tbPageTitle" runat="server"
            SourceTypeName="Rock.CMS.Page, Rock" PropertyName="Title"></Rock:DataTextBox></dl>

        <dl><Rock:DataDropDownList ID="ddlLayout" runat="server" 
            SourceTypeName="Rock.CMS.Page, Rock" PropertyName="Layout"></Rock:DataDropDownList></dl>
    
        <dl><Rock:DataTextBox ID="tbDescription" runat="server" TextMode="MultiLine" Rows="3"
            SourceTypeName="Rock.CMS.Page, Rock" PropertyName="Description"></Rock:DataTextBox></dl>

    </fieldset>

    <fieldset>
        <legend>Menu Display Options</legend>

        <dl><Rock:DataDropDownList ID="ddlMenuWhen" runat="server" LabelText="Display When"
            SourceTypeName="Rock.CMS.Page, Rock" PropertyName="DisplayInNavWhen"></Rock:DataDropDownList></dl>

        <dl><Rock:LabeledCheckBox ID="cbMenuDescription" runat="server" LabelText="Show Description"></Rock:LabeledCheckBox></dl>

        <dl><Rock:LabeledCheckBox ID="cbMenuIcon" runat="server" LabelText="Show Idon"></Rock:LabeledCheckBox></dl>

        <dl><Rock:LabeledCheckBox ID="cbMenuChildPages" runat="server" LabelText="Show Child Pages"></Rock:LabeledCheckBox></dl>

    </fieldset>

    <fieldset id="fsAttributes" runat="server" visible="false">
        <legend>Page Attributes</legend>
    </fieldset>

    <fieldset>
        <legend>Advanced Settings</legend>

        <dl><Rock:LabeledCheckBox ID="cbRequiresEncryption" runat="server" LabelText="Force SSL"></Rock:LabeledCheckBox></dl>

        <dl><Rock:LabeledCheckBox ID="cbEnableViewState" runat="server" LabelText="Enable ViewState"></Rock:LabeledCheckBox></dl>

        <dl><Rock:LabeledCheckBox ID="cbIncludeAdminFooter" runat="server" LabelText="Allow Configuration"></Rock:LabeledCheckBox></dl>

        <dl><Rock:DataTextBox ID="tbCacheDuration" runat="server" LabelText="Cache Duration"
            SourceTypeName="Rock.CMS.Page, Rock" PropertyName="OutputCacheDuration"></Rock:DataTextBox></dl>

    </fieldset>

    <div class="actions">
        <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="btn primary" OnClick="btnSave_Click " />
    </div>

</div>

