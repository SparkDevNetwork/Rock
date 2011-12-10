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

    <fieldset>
        <legend>Basic Settings</legend>

        <dl><Rock:DataDropDownList ID="ddlParentPage" runat="server" LabelText="Parent Page"
            SourceTypeName="Rock.CMS.Page, Rock.Framework" PropertyName="ParentPageId"></Rock:DataDropDownList></dl>

        <dl><Rock:DataTextBox ID="tbPageName" runat="server"
            SourceTypeName="Rock.CMS.Page, Rock.Framework" PropertyName="Name"></Rock:DataTextBox></dl>

        <dl><Rock:DataTextBox ID="tbPageTitle" runat="server"
            SourceTypeName="Rock.CMS.Page, Rock.Framework" PropertyName="Title"></Rock:DataTextBox></dl>

        <dl><Rock:DataDropDownList ID="ddlLayout" runat="server" 
            SourceTypeName="Rock.CMS.Page, Rock.Framework" PropertyName="Layout"></Rock:DataDropDownList></dl>
    
        <dl><Rock:DataTextBox ID="tbDescription" runat="server" TextMode="MultiLine" Rows="3"
            SourceTypeName="Rock.CMS.Page, Rock.Framework" PropertyName="Description"></Rock:DataTextBox></dl>

    </fieldset>

    <fieldset>
        <legend>Menu Display Options</legend>

        <dl><Rock:DataDropDownList ID="ddlMenuWhen" runat="server" LabelText="Display When"
            SourceTypeName="Rock.CMS.Page, Rock.Framework" PropertyName="DisplayInNavWhen"></Rock:DataDropDownList></dl>

        <dl><Rock:DataCheckBox ID="cbMenuDescription" runat="server" LabelText="Show Description"
            SourceTypeName="Rock.CMS.Page, Rock.Framework" PropertyName="MenuDisplayDescription"></Rock:DataCheckBox></dl>

        <dl><Rock:DataCheckBox ID="cbMenuIcon" runat="server" LabelText="Show Idon"
            SourceTypeName="Rock.CMS.Page, Rock.Framework" PropertyName="MenuDisplayIcon"></Rock:DataCheckBox></dl>

        <dl><Rock:DataCheckBox ID="cbMenuChildPages" runat="server" LabelText="Show Child Pages"
            SourceTypeName="Rock.CMS.Page, Rock.Framework" PropertyName="MenuDisplayChildPages"></Rock:DataCheckBox></dl>

    </fieldset>

    <fieldset id="fsAttributes" runat="server" visible="false">
        <legend>Page Attributes</legend>
    </fieldset>

    <fieldset>
        <legend>Advanced Settings</legend>

        <dl><Rock:DataCheckBox ID="cbRequiresEncryption" runat="server" LabelText="Force SSL"
            SourceTypeName="Rock.CMS.Page, Rock.Framework" PropertyName="RequiresEncryption"></Rock:DataCheckBox></dl>

        <dl><Rock:DataCheckBox ID="cbEnableViewState" runat="server" LabelText="Enable ViewState"
            SourceTypeName="Rock.CMS.Page, Rock.Framework" PropertyName="EnableViewState"></Rock:DataCheckBox></dl>

        <dl><Rock:DataCheckBox ID="cbIncludeAdminFooter" runat="server" LabelText="Allow Configuration" 
            SourceTypeName="Rock.CMS.Page, Rock.Framework" PropertyName="IncludeAdminFooter"></Rock:DataCheckBox></dl>

        <dl><Rock:DataTextBox ID="tbCacheDuration" runat="server" LabelText="Cache Duration"
            SourceTypeName="Rock.CMS.Page, Rock.Framework" PropertyName="OutputCacheDuration"></Rock:DataTextBox></dl>

    </fieldset>

    <div class="actions">
        <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="btn primary" OnClick="btnSave_Click " />
    </div>

</div>

