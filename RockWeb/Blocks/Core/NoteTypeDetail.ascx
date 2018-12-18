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

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbName" runat="server" Label="Name" Required="true" />
                        <Rock:EntityTypePicker ID="epEntityType" runat="server" Required="true" Label="Entity Type" IncludeGlobalOption="false" EnhanceForLongLists="true" />
                        <Rock:RockLiteral ID="lEntityTypeReadOnly" runat="server" Visible="false" Label="Entity Type" />

                        <Rock:RockTextBox ID="tbIconCssClass" runat="server" Label="Icon CSS Class" />
                        <Rock:ColorPicker ID="cpBackgroundColor" runat="server" Label="Background Color" />
                        <Rock:ColorPicker ID="cpFontColor" runat="server" Label="Font Color" />
                        <Rock:ColorPicker ID="cpBorderColor" runat="server" Label="Border Color" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbUserSelectable" runat="server" Label="User Selectable" Text="Yes" />
                        <Rock:RockCheckBox ID="cbRequiresApprovals" runat="server" Label="Requires Approvals" Text="Yes" />
                        <Rock:RockCheckBox ID="cbSendApprovalNotifications" runat="server" Label="Send Approval Notifications" Text="Yes" />
                        <Rock:RockCheckBox ID="cbAllowsWatching" runat="server" Label="Allows Watching" Text="Yes"  Help="If enabled, an option to watch individual notes will appear, and note watch notifications will be sent on watched notes." />
                        <Rock:RockCheckBox ID="cbAutoWatchAuthors" runat="server" Label="Auto Watch Authors" Text="Yes"  Help="If enabled, the author of a note will get notifications for direct replies to the note. In other words, a 'watch' will be automatically enabled on the note."/>

                        <Rock:RockCheckBox ID="cbAllowsReplies" runat="server" Label="Allow Replies" AutoPostBack="true" OnCheckedChanged="cbAllowsReplies_CheckedChanged" Text="Yes" />

                        <Rock:NumberBox ID="nbMaxReplyDepth" runat="server" CssClass="input-width-sm" NumberType="Integer" MinimumValue="0" MaximumValue="9999" Label="Max Reply Depth" />

                        <Rock:CodeEditor ID="ceApprovalUrlTemplate" runat="server" Label="Approval URL Template" EditorHeight="100" EditorMode="Lava" Help="An optional lava template that can be used to general a URL where notes of this type can be approved. If this is left blank, the approval URL will be a URL to the page (including a hash anchor to the note) where the note was originally created." />
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
