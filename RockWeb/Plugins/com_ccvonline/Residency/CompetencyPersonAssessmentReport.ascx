<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CompetencyPersonAssessmentReport.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.CompetencyPersonAssessmentReport" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <div class="no-print well">
            <Rock:DateRangePicker runat="server" ID="pDateRange" Label="Select Date/Range" />
            <Rock:RockTextBox runat="server" ID="tbSemesterName" CssClass="js-semester-name-edit" Label="Semester Name" Required="true" />
            <asp:LinkButton ID="btnApply" runat="server" Text="Apply" CssClass="btn btn-primary" OnClick="btnApply_Click" />
        </div>

        <div class="row-fluid">
            <Rock:RockLiteral runat="server" ID="lbResidentName" Label="<b>Resident</b>" />
            <Rock:RockLiteral runat="server" ID="lbYear" Label="<b>Year</b>" />
            <Rock:RockLiteral runat="server" ID="lbSemester" Label="<b>Semester</b>" CssClass ="js-semester-name-label" />
            <Rock:RockLiteral runat="server" ID="lbCurrentDate" Label="<b>Date</b>" />
        </div>

        <h3>Assessment Summary</h3>
        <Rock:Grid ID="gAssessmentSummary" runat="server" AllowSorting="true" DataKeyNames="Id" RowItemText="Competency" DisplayType="Light">
            <Columns>
                <asp:BoundField DataField="TrackName" HeaderText="Track" SortExpression="TrackDisplayOrder" />
                <asp:BoundField DataField="CompetencyName" HeaderText="Competency" SortExpression="CompetencyName" />
                <asp:BoundField DataField="OverallRating" HeaderText="Overall Rating" SortExpression="OverallRating" DataFormatString="{0:F2}" />
            </Columns>
        </Rock:Grid>

        <h3>Assessment Details
        </h3>
        <Rock:Grid ID="gAssessmentDetails" runat="server" AllowSorting="false" DataKeyNames="Id" DisplayType="Light">
            <Columns>
                <asp:BoundField DataField="Competency" HeaderText="Competency" />
                <asp:BoundField DataField="ProjectName" HeaderText="Project Name" />
                <asp:BoundField DataField="ProjectDescription" HeaderText="Project Description" />
                <asp:BoundField DataField="Evaluator" HeaderText="Evaluator" />
                <Rock:DateField DataField="AssessmentDateTime" HeaderText="Date" />
                <asp:BoundField DataField="OverallRating" HeaderText="Overall Rating" />
                <asp:BoundField DataField="RatingNotes" HeaderText="Assessor Notes" />
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>
