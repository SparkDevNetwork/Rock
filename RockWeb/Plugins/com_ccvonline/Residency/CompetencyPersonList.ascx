<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CompetencyPersonList.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.CompetencyPersonList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlList" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-lightbulb-o"></i> Resident Competencies</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" AllowSorting="true" OnRowSelected="gList_Edit" DataKeyNames="Id" RowItemText="Competency">
                        <Columns>
                            <asp:BoundField DataField="TrackName" HeaderText="Track" SortExpression="TrackDisplayOrder" />
                            <asp:BoundField DataField="CompetencyName" HeaderText="Competency" SortExpression="CompetencyName" />
                            <asp:BoundField DataField="CompletedProjectAssessmentsTotal" HeaderText="Project Assessments Completed" SortExpression="CompletedProjectAssessmentsTotal" />
                            <asp:BoundField DataField="MinAssessmentCount" HeaderText="Project Assessments Required" SortExpression="MinAssessmentCount" />
                            <Rock:DeleteField OnClick="gList_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>

            <asp:HiddenField ID="hfPersonId" runat="server" />
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
