<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CompetencyPersonProjectAssessmentPointOfAssessmentDetail.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.CompetencyPersonProjectAssessmentPointOfAssessmentDetail" %>

<asp:UpdatePanel ID="upCompetencyPersonProjectAssessmentPointOfAssessmentDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <asp:HiddenField ID="hfProjectPointOfAssessmentId" runat="server" />
            <asp:HiddenField ID="hfCompetencyPersonProjectAssessmentId" runat="server" />

            <div class="banner">
                <h1>
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>
            </div>

            <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
            <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

            <div id="pnlEditDetails" runat="server">

                <div class="row">
                    <div class="col-md-6">
                        <asp:Literal ID="lblMainDetailsCol1" runat="server" />
                        <Rock:NumberBox ID="tbRating" runat="server" Label="Rating" MaximumValue="5" MinimumValue="1" />
                        <Rock:DataTextBox ID="tbRatingNotes" runat="server" SourceTypeName="com.ccvonline.Residency.Model.CompetencyPersonProjectAssessmentPointOfAssessment, com.ccvonline.Residency" PropertyName="RatingNotes" TextMode="MultiLine" Rows="3" />
                    </div>
                    <div class="col-md-6">
                        <asp:Literal ID="lblMainDetailsCol2" runat="server" />
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
