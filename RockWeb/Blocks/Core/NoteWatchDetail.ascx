<%@ Control Language="C#" AutoEventWireup="true" CodeFile="NoteWatchDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.NoteWatchDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfNoteWatchId" runat="server" />
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-binoculars"></i>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </h1>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbWatcherMustBeSelectWarning" runat="server" NotificationBoxType="Validation" Text="A Person or Group must be specified as the watcher" Dismissable="true" Visible="false" />
                <Rock:NotificationBox ID="nbUnableToOverride" runat="server" NotificationBoxType="Danger" Text="Unable to set Watching to false. This would override another note watch that doesn't allow overrides." Dismissable="true" Visible="false" />

                <h4 class="mt-0">Watched by</h4>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:PersonPicker ID="ppWatcherPerson" runat="server" Label="Watcher Person" Help="The person that will receive notifications for this note watch" EnableSelfSelection="true" />
                        <Rock:GroupPicker ID="gpWatcherGroup" runat="server" Label="Watcher Group" Help="Member of this group will receive notifications for this note watch" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbIsWatching" runat="server" Label="Watching" AutoPostBack="true" OnCheckedChanged="cbIsWatching_CheckedChanged" Help="Set this to false to block notifications." />
                        <Rock:RockCheckBox ID="cbAllowOverride" runat="server" Label="Allow Override" Help="Set this to false to prevent other note watches from blocking this note watch." />
                    </div>
                </div>

                <h4>Watch Filter</h4>

                <Rock:NotificationBox ID="nbWatchFilterMustBeSeletedWarning" runat="server" NotificationBoxType="Danger" Text="A Watch Filter must be specified." Dismissable="true" Visible="false" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:EntityTypePicker ID="etpEntityType" runat="server" Label="Entity Type" Help="Set EntityType to enable watching a specific note type or specific entity." IncludeGlobalOption="false" AutoPostBack="true" OnSelectedIndexChanged="etpEntityType_SelectedIndexChanged" />
                        <asp:Panel ID="pnlWatchedEntity" runat="server">
                            <Rock:PersonPicker ID="ppWatchedPerson" runat="server" Visible="false" Label="Watching Person" OnSelectPerson="ppWatchedPerson_SelectPerson" Help="Select a Person to watch notes added to this person." />
                            <Rock:GroupPicker ID="gpWatchedGroup" runat="server" Visible="false" Label="Watching Group" OnSelectItem="gpWatchedGroup_SelectItem" Help="Select a Group to watch notes added to this group." />
                            <asp:Panel ID="pnlWatchedEntityGeneric" runat="server" Visible="false">
                                <Rock:NumberBox ID="nbWatchedEntityId" runat="server" Label="EntityId" AutoPostBack="true" OnTextChanged="nbWatchedEntityId_TextChanged" Help="Specify the entity id to watch notes added to the specified entity." />
                                <Rock:RockLiteral ID="lWatchedEntityName" Label="Name" runat="server" Visible="false" />
                            </asp:Panel>
                        </asp:Panel>
                    </div>
                    <div class="col-md-6">
                        <Rock:NotificationBox ID="nbNoteTypeWarning" runat="server" NotificationBoxType="Danger" Text="This note type doesn't allow note watches." Dismissable="true" Visible="false" />
                        <Rock:RockDropDownList ID="ddlNoteType" runat="server" Label="Note Type" Help="Select a Note Type to watch all notes of this note" />

                        <Rock:RockLiteral ID="lWatchedNote" Label="Watching Note" runat="server" />
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
