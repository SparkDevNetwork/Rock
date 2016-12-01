<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ResidentGradeDetail.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Residency.ResidentGradeDetail" %>

<!-- just to help do css intellisense at design time  -->
<link rel="stylesheet" type="text/css" href="~/CSS/bootstrap.min.css" visible="false" />

<!-- just to help do css intellisense at design time  -->
<link rel="stylesheet" type="text/css" href="~/Themes/Rock/Styles/bootstrap.css" visible="false" />
<link rel="stylesheet" type="text/css" href="~/Themes/Rock/Styles/theme.css" visible="false" />

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <asp:HiddenField ID="hfCompetencyPersonProjectId" runat="server" />
            <asp:HiddenField ID="hfCompetencyPersonProjectAssessmentId" runat="server" />
            <asp:HiddenField ID="hfAssessorPersonId" runat="server" />

            <div class="banner">
                <h1>
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>
            </div>

            <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
            <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

            <div>
                <div class="row">
                    <div class="col-md-12">
                        <asp:Literal ID="lblMainDetails" runat="server" />
                    </div>
                </div>
            </div>

            <asp:Repeater ID="rptPointOfAssessment" runat="server" OnItemDataBound="rptPointOfAssessment_ItemDataBound">
                <ItemTemplate>
                    <div class="row panel-body">
                        <asp:HiddenField ID="hfProjectPointOfAssessmentId" runat="server" />
                        <div class="panel">
                            <div class="panel-heading">
                                <asp:Literal ID="lblAssessmentText" runat="server" />
                            </div>
                            <div class="panel-body">
                                <div class="col-md-2">
                                    <Rock:RockDropDownList ID="ddlPointOfAssessmentRating" runat="server" Label="Rating" />
                                    <Rock:RockCheckBox ID="ckPointOfAssessmentPassFail" runat="server" Text="Passed"  />
                                </div>
                                <div class="col-md-10">
                                    <Rock:RockTextBox ID="tbRatingNotesPOA" runat="server" TextMode="MultiLine" Rows="4" Label="Instructor Notes" />
                                </div>
                            </div>
                        </div>
                </ItemTemplate>
            </asp:Repeater>

            <Rock:RockTextBox ID="tbRatingNotesOverall" runat="server" TextMode="MultiLine" Rows="4" Label="Evaluation Notes" />
            

            <Rock:NotificationBox ID="nbSaveMessage" runat="server" NotificationBoxType="Success" Text="Graded Successfully" Visible="false" />

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
