<%@ Control Language="C#" AutoEventWireup="true" CodeFile="NoteTypeList.ascx.cs" Inherits="RockWeb.Blocks.Core.NoteTypeList" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" runat="server">

            <Rock:NotificationBox ID="nbOrdering" runat="server" NotificationBoxType="Info" Text="Note: Select a specific entity type filter in order to reorder note types." Dismissable="true" Visible="false" />
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-edit"></i> Note Types</h1>
                </div>
                <div class="panel-body">

                    <div class="grid grid-panel">
                        <Rock:GridFilter ID="gfNoteTypes" runat="server" OnDisplayFilterValue="gfNoteTypes_DisplayFilterValue">
                            <Rock:EntityTypePicker ID="entityTypeFilter" runat="server" Required="false" Label="Entity Type" IncludeGlobalOption="false" EnhanceForLongLists="true" />
                        </Rock:GridFilter>
                        <Rock:Grid ID="gNoteTypes" runat="server" RowItemText="Note Type" OnRowSelected="gNoteTypes_Edit" >
                            <Columns>
                                <Rock:ReorderField />
                                <asp:BoundField DataField="EntityType.Name" HeaderText="Entity Type" />
                                <Rock:RockBoundField DataField="Name" HeaderText="Note Type" />
                                <Rock:RockBoundField DataField="CssClass" HeaderText="CSS Class" />
                                <Rock:RockBoundField DataField="IconCssClass" HeaderText="Icon CSS Class" />
                                <Rock:BoolField DataField="UserSelectable" HeaderText="User Selectable" />
                                <Rock:BoolField DataField="RequiresApprovals" HeaderText="Requires Approvals" />
                                <Rock:BoolField DataField="AllowsWatching" HeaderText="Allows Watching" />
                                <Rock:BoolField DataField="AllowsReplies" HeaderText="Allows Replies" />
                                <Rock:BoolField DataField="IsSystem" HeaderText="System" />
                                <Rock:SecurityField />
                                <Rock:DeleteField OnClick="gNoteTypes_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>

                </div>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
