<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PrayerCommentDetail.ascx.cs" Inherits="RockWeb.Blocks.Prayer.PrayerCommentDetail" %>

<asp:UpdatePanel ID="upPrayerComments" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">
            <asp:HiddenField ID="hfNoteId" runat="server" />
            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-error" />

            <!-- Edit -->
                <div class="note-container overview" >
                    <asp:PlaceHolder ID="phNotesBefore" runat="server"></asp:PlaceHolder>

                    <fieldset id="fieldsetEditDetails" runat="server">
                        <legend>
                            <asp:Literal ID="lActionTitle" runat="server" />
                        </legend>
                        <div class="row-fluid">
                            <div class="span6"><Rock:DataTextBox ID="tbText" runat="server" SourceTypeName="Rock.Model.Note, Rock" PropertyName="Text" TextMode="MultiLine" Rows="3" CssClass="input-xxlarge" />
                            </div>
                        </div>
                        <div class="row-fluid">
                            <div class="span6"><Rock:DataTextBox ID="tbCaption" runat="server" SourceTypeName="Rock.Model.Note, Rock" PropertyName="Caption" LabelText="by"></Rock:DataTextBox>
                            </div>
                        </div>
                        <div class="actions">
                            <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary btn-small" OnClick="btnSave_Click" />
                            <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-small" CausesValidation="false" OnClick="btnCancel_Click" />
                        </div>
                    </fieldset>

                    <asp:PlaceHolder ID="phNotesAfter" runat="server"></asp:PlaceHolder>
                </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
