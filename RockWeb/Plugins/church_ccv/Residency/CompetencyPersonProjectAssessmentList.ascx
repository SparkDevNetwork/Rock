<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CompetencyPersonProjectAssessmentList.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Residency.CompetencyPersonProjectAssessmentList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlList" CssClass="panel panel-block" runat="server">

            <asp:HiddenField ID="hfCompetencyPersonProjectId" runat="server" />
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-list-ul"></i> Completed Assessments</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" AllowSorting="true" OnRowSelected="gList_Edit" DataKeyNames="Id" RowItemText="Assessment">
                        <Columns>
                            <Rock:DateTimeField DataField="AssessmentDateTime" ItemStyle-HorizontalAlign="Left" HeaderText="Assessment Date/Time" SortExpression="AssessmentDateTime" />
                            <asp:BoundField DataField="AssessorPerson.FullName" HeaderText="Assessor" SortExpression="AssessorPerson.FirstName, AssessorPerson.LastName" />
                            <asp:BoundField DataField="OverallRating" HeaderText="Rating" SortExpression="OverallRating" />
                            <Rock:DeleteField OnClick="gList_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
