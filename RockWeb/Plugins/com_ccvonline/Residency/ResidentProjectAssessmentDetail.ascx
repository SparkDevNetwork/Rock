<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ResidentProjectAssessmentDetail.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.ResidentProjectAssessmentDetail" %>

<!-- just to help do css intellisense at design time  -->
<link rel="stylesheet" type="text/css" href="~/CSS/bootstrap.min.css" visible="false" />

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">
            <asp:HiddenField ID="hfCompetencyPersonProjectAssessmentId" runat="server" />
            <div class="well">
                <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lblMainDetailsCol1" runat="server" />
                        </div>
                        <div class="col-md-6">
                            <asp:Literal ID="lblMainDetailsCol2" runat="server" />
                        </div>
                    </div>
                <asp:Panel ID="pnlViewComments" runat="server">
                    <div class="row-fluid">
                        <asp:Literal ID="lblResidentComments" runat="server" />
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary btn-mini" OnClick="btnEdit_Click" />
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnlEditComments" runat="server">
                    <Rock:RockTextBox ID="tbResidentComments" runat="server" TextMode="MultiLine" Rows="5" CssClass="input-xlarge" Label="Resident Comments" />
                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </asp:Panel>
            </div>
            <h4>Project Assessment
            </h4>
            <Rock:Grid ID="gList" runat="server" AllowSorting="false" OnRowSelected="gList_Edit" DataKeyNames="ProjectPointOfAssessmentId,CompetencyPersonProjectAssessmentId" DisplayType="Light">
                <Columns>
                    <Rock:ColorField DataField="ProjectPointOfAssessmentColor" ToolTipDataField="ProjectPointOfAssessment.PointOfAssessmentTypeValue.Name" />
                    <asp:BoundField DataField="ProjectPointOfAssessment.AssessmentOrder" HeaderText="#" />
                    <asp:BoundField DataField="ProjectPointOfAssessment.AssessmentText" HeaderText="Text" />
                    <asp:BoundField DataField="CompetencyPersonProjectAssessmentPointOfAssessment.Rating" HeaderText="Rating" />
                    <asp:BoundField DataField="CompetencyPersonProjectAssessmentPointOfAssessment.RatingNotes" HeaderText="Rating Notes" />
                </Columns>
            </Rock:Grid>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
