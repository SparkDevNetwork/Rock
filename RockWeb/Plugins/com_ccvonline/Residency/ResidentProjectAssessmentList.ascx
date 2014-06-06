<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ResidentProjectAssessmentList.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.ResidentProjectAssessmentList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfCompetencyPersonProjectId" runat="server" />
        <h4><asp:Literal ID="lblTitle" runat="server" Text="Project Assessments" /></h4>
        <Rock:Grid ID="gList" runat="server" AllowSorting="true" OnRowSelected="gList_Edit" DataKeyNames="Id" RowItemText="Project Assessments" DisplayType="Light">
            <Columns>
                <Rock:DateField DataField="AssessmentDateTime" HeaderText="Date" SortExpression="AssessmentDateTime" />
                <asp:BoundField DataField="CompetencyPersonProject.CompetencyPerson.Competency.Track.Name" HeaderText="Track" SortExpression="CompetencyPersonProject.CompetencyPerson.Competency.Track.Name" />
                <asp:BoundField DataField="CompetencyPersonProject.CompetencyPerson.Competency.Name" HeaderText="Competency" SortExpression="CompetencyPersonProject.CompetencyPerson.Competency.Name" />
                <asp:BoundField DataField="CompetencyPersonProject.Project.Name" HeaderText="Project" SortExpression="CompetencyPersonProject.Project.Name" />
                <asp:BoundField DataField="AssessorPerson.FullName" HeaderText="Evaluator" SortExpression="AssessorPerson.FullName" />
                <asp:BoundField DataField="OverallRating" HeaderText="Rating" SortExpression="OverallRating" />
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>