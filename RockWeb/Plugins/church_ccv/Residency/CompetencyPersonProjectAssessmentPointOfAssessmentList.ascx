<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CompetencyPersonProjectAssessmentPointOfAssessmentList.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.CompetencyPersonProjectAssessmentPointOfAssessmentList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" CssClass="panel panel-block" runat="server">
            

            <asp:HiddenField ID="hfCompetencyPersonProjectAssessmentId" runat="server" />
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-list-ul"></i> Points of Assessment</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" AllowSorting="false" OnRowSelected="gList_Edit" DataKeyNames="ProjectPointOfAssessmentId,CompetencyPersonProjectAssessmentId">
                        <Columns>
                            <Rock:ColorField DataField="ProjectPointOfAssessmentColor" ToolTipDataField="ProjectPointOfAssessment.PointOfAssessmentTypeValue.Name" />
                            <asp:BoundField DataField="ProjectPointOfAssessment.AssessmentOrder" HeaderText="#" SortExpression="ProjectPointOfAssessment.AssessmentOrder" />
                            <asp:BoundField DataField="ProjectPointOfAssessment.AssessmentText" HeaderText="Text" SortExpression="ProjectPointOfAssessment.AssessmentText" />
                            <asp:BoundField DataField="CompetencyPersonProjectAssessmentPointOfAssessment.Rating" HeaderText="Rating" SortExpression="CompetencyPersonProjectAssessmentPointOfAssessment.Rating" />
                            <asp:BoundField DataField="CompetencyPersonProjectAssessmentPointOfAssessment.RatingNotes" HeaderText="Rating Notes" SortExpression="CompetencyPersonProjectAssessmentPointOfAssessment.RatingText" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
