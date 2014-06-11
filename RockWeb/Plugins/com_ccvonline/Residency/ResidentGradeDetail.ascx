<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ResidentGradeDetail.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.ResidentGradeDetail" %>

<!-- just to help do css intellisense at design time  -->
<link rel="stylesheet" type="text/css" href="~/CSS/bootstrap.min.css" visible="false" />

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <asp:HiddenField ID="hfCompetencyPersonProjectId" runat="server" />
            <asp:HiddenField ID="hfCompetencyPersonProjectAssessmentId" runat="server" />
            <asp:HiddenField ID="hfAssessorPersonId" runat="server" />

            <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
            <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

            <fieldset>
                <legend>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </legend>

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div >
                    <div class="row">
                        <asp:Literal ID="lblMainDetails" runat="server" />
                    </div>
                </div>

                <asp:Repeater ID="rptPointOfAssessment" runat="server" OnItemDataBound="rptPointOfAssessment_ItemDataBound">
                    <ItemTemplate>
                        <div class="row">
                            <div class="span1">
                                <Rock:RockDropDownList ID="ddlPointOfAssessmentRating" runat="server" Label="Rating" CssClass="input-mini" />
                            </div>
                            <div class="span11">
                                <div class="row">
                                    <asp:HiddenField ID="hfProjectPointOfAssessmentId" runat="server" />
                                    <p>
                                        <asp:Literal ID="lblAssessmentText" runat="server" />
                                    </p>
                                    <Rock:RockTextBox ID="tbRatingNotesPOA" runat="server" CssClass="input-xxlarge" TextMode="MultiLine" Rows="4" Label="Instructor Notes" />
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>

                <Rock:RockTextBox ID="tbRatingNotesOverall" runat="server" CssClass="input-xxlarge" TextMode="MultiLine" Rows="4" Label="Evaluation Notes" />

            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
