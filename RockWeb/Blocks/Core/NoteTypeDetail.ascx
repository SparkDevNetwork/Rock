<%@ Control Language="C#" AutoEventWireup="true" CodeFile="NoteTypeDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.NoteTypeDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-edit"></i>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </h1>
            </div>

            <Rock:PanelDrawer ID="pdAuditDetails" runat="server" />

            <asp:HiddenField ID="hfNoteTypeId" runat="server" />

            <div class="panel-body">
                <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbName" runat="server" Label="Name" Required="true" />
                        <Rock:EntityTypePicker ID="epEntityType" runat="server" Required="true" Label="Entity Type" IncludeGlobalOption="false" EnhanceForLongLists="true" />
                        <Rock:RockLiteral ID="lEntityTypeReadOnly" runat="server" Visible="false" Label="Entity Type" />

                        <Rock:RockTextBox ID="tbIconCssClass" runat="server" Label="Icon CSS Class" />
                        <Rock:ColorPicker ID="cpColor" runat="server" Label="Color" Help="The base color to use for notes of this type. The background and foreground colors will be automatically calculated from this color." />

                        <Rock:RockDropDownList ID="ddlFormatType" runat="server" Label="Content Format" Help="Structured format provides additional features and is the default for all new note types. Unstructured is a legacy format that is not checked for correctness and will be removed in the future." AutoPostBack="true" OnSelectedIndexChanged="ddlFormatType_SelectedIndexChanged" />

                        <Rock:NotificationBox ID="nbStructuredWarning" runat="server" NotificationBoxType="Warning">
                            Once you change a note type to the Structured format, it cannot be changed back. Be sure this is what you want to do.
                        </Rock:NotificationBox>
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbUserSelectable" runat="server" Label="User Selectable" Text="Yes" Help="This flag enables notes of this note type to be added from the note entry block" />
                        <Rock:RockCheckBox ID="cbAllowsWatching" runat="server" Label="Allows Watching" Text="Yes"  Help="If enabled, an option to watch individual notes will appear, and note watch notifications will be sent on watched notes." />
                        <Rock:RockCheckBox ID="cbAutoWatchAuthors" runat="server" Label="Auto Watch Authors" Text="Yes"  Help="If enabled, the author of a note will get notifications for direct replies to the note. In other words, a 'watch' will be automatically enabled on the note."/>

                        <Rock:RockCheckBox ID="cbAllowsReplies" runat="server" Label="Allow Replies" AutoPostBack="true" OnCheckedChanged="cbAllowsReplies_CheckedChanged" Text="Yes" />
                        <Rock:NumberBox ID="nbMaxReplyDepth" runat="server" CssClass="input-width-sm" NumberType="Integer" MinimumValue="0" MaximumValue="9999" Label="Max Reply Depth" />

                        <Rock:RockCheckBox ID="cbAllowsAttachments" runat="server" Label="Allows Attachments" Text="Yes" Help="If enabled, then this note type will allow attachments. However, not all UI components will currently allow file uploads." AutoPostBack="true" OnCheckedChanged="cbAllowsAttachments_CheckedChanged" />
                        <Rock:BinaryFileTypePicker ID="bftpAttachmentType" runat="server" Label="Attachment File Type" Required="true" Help="When a file is attached to a note, it will be stored using this file type." />

                        <asp:Panel ID="pnlStructuredFeatures" runat="server">
                            <Rock:RockCheckBox ID="cbIsMentionEnabled" runat="server" Visible="false" Label="Enable Mentions" Help="Mentions allow a person to be mentioned in the text of a note. Once saved the mentioned person will be notified." />
                        </asp:Panel>
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
