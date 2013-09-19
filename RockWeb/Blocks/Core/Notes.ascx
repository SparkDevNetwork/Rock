<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Notes.ascx.cs" Inherits="RockWeb.Blocks.Core.Notes" %>
<%@ Import Namespace="Rock" %>

<asp:UpdatePanel ID="upNotes" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlNotes" runat="server">
            <section class="panel panel-default panel-notes">

                <div class="panel-heading clearfix">
                    <h3 class="panel-title">
                        <i class="icon-calendar"></i>
                        <asp:Literal ID="lTitle" runat="server"></asp:Literal>
                    </h3>
                    <a class="btn btn-sm btn-default add-note"><i class="icon-plus"></i></a>
                </div>

                <div class="panel-body">

                    <div class="note-entry clearfix" style="display: none;">
                        <div class="note">
                            <label>Note</label>
                            <asp:TextBox ID="tbNewNote" runat="server" TextMode="MultiLine"></asp:TextBox>
                        </div>
                        <div class="settings clearfix">
                            <div class="options">
                                <asp:CheckBox ID="cbAlert" runat="server" Text="Alert" />
                                <asp:CheckBox ID="cbPrivate" runat="server" Text="Private" />
                            </div>
                            <button class="btn btn-xs security" type="button" id="btnSecurity" runat="server"><i class="icon-lock"></i> Security</button>
                        </div>
                        <div class="actions">
                            <asp:LinkButton ID="btnAddNote" runat="server" CssClass="btn btn-primary btn-small" Text="Add Note" />
                            <a class="add-note-cancel btn btn-xs">Cancel</a>
                        </div>

                    </div>

                    <asp:Repeater ID="rptNotes" runat="server">
                        <ItemTemplate>
                            <Rock:NoteEditor ID="noteEditor" runat="server" Note='<%# Container.DataItem as Rock.Model.Note %>'
                                OnSaveButtonClick="noteEditor_SaveButtonClick" OnDeleteButtonClick="noteEditor_DeleteButtonClick"></Rock:NoteEditor>
                        </ItemTemplate>
                    </asp:Repeater>

                    <asp:LinkButton ID="lbShowMore" runat="server" OnClick="lbShowMore_Click">
                    <i class="icon-angle-down"></i>
                    <span>Load More</span>
                    <i class="icon-angle-down"></i>
                    </asp:LinkButton>

                    <asp:HiddenField ID="hfDisplayCount" runat="server" Value="10" />

                </div>

            </section>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
