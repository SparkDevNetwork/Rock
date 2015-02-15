<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ResidentProjectAssessmentDetail.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.ResidentProjectAssessmentDetail" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
            <asp:HiddenField ID="hfCompetencyPersonProjectAssessmentId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-check"></i> <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <div class="row">
                    <div class="col-md-6">
                        <asp:Literal ID="lblMainDetailsCol1" runat="server" />
                    </div>
                    <div class="col-md-6">
                        <asp:Literal ID="lblMainDetailsCol2" runat="server" />
                    </div>
                </div>

                <asp:Panel ID="pnlViewComments" runat="server">
                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lblResidentComments" runat="server" />
                        </div>
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlEditComments" runat="server">
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:RockTextBox ID="tbResidentComments" runat="server" TextMode="MultiLine" Rows="5" Label="Resident Comments" />
                        </div>
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </asp:Panel>

                <h4>Project Assessment
                </h4>
                <Rock:Grid ID="gList" runat="server" AllowSorting="false" DataKeyNames="ProjectPointOfAssessmentId,CompetencyPersonProjectAssessmentId" DisplayType="Light" RowClickEnabled="false">
                    <Columns>
                        <Rock:ColorField DataField="ProjectPointOfAssessmentColor" ToolTipDataField="ProjectPointOfAssessment.PointOfAssessmentTypeValue.Value" />
                        <asp:BoundField DataField="ProjectPointOfAssessment.AssessmentOrder" HeaderText="#" />
                        <asp:BoundField DataField="ProjectPointOfAssessment.AssessmentText" HeaderText="Text" />
                        <asp:BoundField DataField="CompetencyPersonProjectAssessmentPointOfAssessment.Rating" HeaderText="Rating" />
                        <asp:BoundField DataField="CompetencyPersonProjectAssessmentPointOfAssessment.RatingNotes" HeaderText="Rating Notes" />
                    </Columns>
                </Rock:Grid>

            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
