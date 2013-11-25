<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PrayerCommentDetail.ascx.cs" Inherits="RockWeb.Blocks.Prayer.PrayerCommentDetail" %>

<asp:UpdatePanel ID="upPrayerComments" runat="server">
    <ContentTemplate>
            <section class="widget persontimeline">
                <header class="clearfix">
                    <h4>
                        <i class="fa fa-comment"></i>
                        <asp:Literal ID="lTitle" runat="server"></asp:Literal>
                    </h4>
                    <a class="btn btn-default btn-sm add-note"><i class="fa fa-plus"></i></a>
                </header>

                <div class="widget-content">
                    <div class="note-entry clearfix" style="display: none;">
                        <div class="note">
                            <label>Note</label>
                            <asp:TextBox ID="tbNewNote" runat="server" TextMode="MultiLine"></asp:TextBox>
                        </div>

                        <div class="actions">
                            <asp:LinkButton ID="btnAddNote" runat="server" CssClass="btn btn-primary btn-sm" Text="Add Note" />
                            <a class="add-note-cancel btn btn-default btn-sm">Cancel</a>
                        </div>

                    </div>

                    <asp:HiddenField ID="hfNoteId" runat="server" />
                    <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <!-- Edit -->
                    <div class="note-container overview">

                        <asp:PlaceHolder ID="phNotesBefore" runat="server"></asp:PlaceHolder>

                        <div class="well" id="fieldsetEditDetails" runat="server">
                                <div class="row-fluid">
                                    <div class="span6">
                                        <Rock:DataTextBox ID="tbText" runat="server" SourceTypeName="Rock.Model.Note, Rock" PropertyName="Text" TextMode="MultiLine" Rows="3" CssClass="input-xxlarge" />
                                    </div>
                                </div>
                                <div class="row-fluid">
                                    <div class="span6">
                                        <Rock:DataTextBox ID="tbCaption" runat="server" SourceTypeName="Rock.Model.Note, Rock" PropertyName="Caption" Label="by"></Rock:DataTextBox>
                                    </div>
                                </div>
                                <div class="actions">
                                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary btn-sm" OnClick="btnSave_Click" />
                                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-default btn-sm" CausesValidation="false" OnClick="btnCancel_Click" />
                                </div>
                        </div>

                        <asp:PlaceHolder ID="phNotesAfter" runat="server"></asp:PlaceHolder>
                    </div>

                </div>

            </section>
    </ContentTemplate>
</asp:UpdatePanel>
