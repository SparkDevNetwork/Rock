<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CompetencyPersonAssessmentReport.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Residency.CompetencyPersonAssessmentReport" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-book"></i> Assessment Report for: <asp:Literal runat="server" ID="lbResidentName" /></h1>
            </div>
            <div class="panel-body">

                <div class="no-print well">
                    <Rock:DateRangePicker runat="server" ID="pDateRange" Label="Select Date/Range" />
                    <Rock:RockTextBox runat="server" ID="tbSemesterName" Label="Semester Name" Required="true" />
                    <asp:LinkButton ID="btnApply" runat="server" Text="Apply" CssClass="btn btn-primary" OnClick="btnApply_Click" />
                </div>

                <div class="row">
                    <div class="col-md-12">
                
                        <Rock:RockLiteral runat="server" ID="lbYear" Label="<b>Year</b>" />
                        <Rock:RockLiteral runat="server" ID="lbSemester" Label="<b>Semester</b>" />
                        <Rock:RockLiteral runat="server" ID="lbCurrentDate" Label="<b>Date</b>" />
                    </div>
                </div>

                <h3>Assessment Summary</h3>

                <div class="grid">
                    <Rock:Grid ID="gAssessmentSummary" runat="server" AllowSorting="true" DataKeyNames="Id" RowItemText="Competency" DisplayType="Light">
                        <Columns>
                            <asp:BoundField DataField="TrackName" HeaderText="Track" SortExpression="TrackDisplayOrder" />
                            <asp:BoundField DataField="CompetencyName" HeaderText="Competency" SortExpression="CompetencyName" />
                            <asp:BoundField DataField="OverallRating" HeaderText="Overall Rating" SortExpression="OverallRating" DataFormatString="{0:F2}" />
                        </Columns>
                    </Rock:Grid>
                </div>

                <h3>Assessment Details</h3>
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

            </div>
        </div>
        
    </ContentTemplate>
</asp:UpdatePanel>
