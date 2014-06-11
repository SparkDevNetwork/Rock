<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CompetencyPersonProjectAssessmentDetail.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.CompetencyPersonProjectAssessmentDetail" %>

<asp:UpdatePanel ID="upCompetencyPersonProjectAssessmentDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <asp:HiddenField ID="hfCompetencyPersonProjectAssessmentId" runat="server" />
            <asp:HiddenField ID="hfCompetencyPersonProjectId" runat="server" />

            <div class="banner">
                <h1>
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>
            </div>

            <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
            <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

            <div id="pnlEditDetails" runat="server">

                <div class="row">
                    <div class="col-md-12">
                        <asp:Literal runat="server" ID="lblEditDetails" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-12">
                        <Rock:PersonPicker ID="ppAssessor" runat="server" Label="Assessor" />
                        <Rock:DateTimePicker ID="dtpAssessmentDateTime" runat="server" Label="Assessment Date/Time" Required="true" />
                        <Rock:RockLiteral ID="lblOverallRating" runat="server" Label="Overall Rating" />
                        <Rock:DataTextBox ID="tbRatingNotes" runat="server" SourceTypeName="com.ccvonline.Residency.Model.CompetencyPersonProjectAssessment, com.ccvonline.Residency" PropertyName="RatingNotes" TextMode="MultiLine" Rows="3" />
                        <Rock:DataTextBox ID="tbResidentComments" runat="server" SourceTypeName="com.ccvonline.Residency.Model.CompetencyPersonProjectAssessment, com.ccvonline.Residency" PropertyName="ResidentComments" TextMode="MultiLine" Rows="3" />
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

            <fieldset id="fieldsetViewDetails" runat="server">

                <div class="row">
                    <div class="col-md-6">
                        <asp:Literal ID="lblMainDetailsCol1" runat="server" />
                    </div>
                    <div class="col-md-6">
                        <asp:Literal ID="lblMainDetailsCol2" runat="server" />
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                </div>

            </fieldset>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
