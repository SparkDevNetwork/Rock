<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CompetencyPersonAssessmentReport.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.CompetencyPersonAssessmentReport" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <div class="no-print well">
            <Rock:DateRangePicker runat="server" ID="pDateRange" LabelText="Select Date/Range" />
            <Rock:LabeledTextBox runat="server" ID="tbSemesterName" CssClass="js-semester-name-edit" LabelText="Semester Name" Required="true" />
            <asp:LinkButton ID="btnApply" runat="server" Text="Apply" CssClass="btn btn-primary" OnClick="btnApply_Click" />
        </div>

        <div class="row-fluid">
            <Rock:LabeledText runat="server" ID="lbResidentName" LabelText="<b>Resident</b>" />
            <Rock:LabeledText runat="server" ID="lbYear" LabelText="<b>Year</b>" />
            <Rock:LabeledText runat="server" ID="lbSemester" LabelText="<b>Semester</b>" TextCssClass="js-semester-name-label" />
            <Rock:LabeledText runat="server" ID="lbCurrentDate" LabelText="<b>Date</b>" />
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
