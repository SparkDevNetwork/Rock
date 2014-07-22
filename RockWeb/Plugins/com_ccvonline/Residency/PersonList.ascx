<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonList.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.PersonList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-user"></i> Residents</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" AllowSorting="true" OnRowSelected="gList_Edit" DataKeyNames="Id" IsPersonList="true" RowItemText="Resident">
                        <Columns>
                            <asp:BoundField DataField="FullName" HeaderText="Name" SortExpression="FullName" />
                            <asp:BoundField DataField="CompetencyCount" HeaderText="Competency Count" SortExpression="CompetencyCount" />
                            <asp:BoundField DataField="CompletedProjectAssessmentsTotal" HeaderText="Project Assessments Completed" SortExpression="CompletedProjectAssessmentsTotal" />
                            <asp:BoundField DataField="MinAssessmentCount" HeaderText="Project Assessments Required" SortExpression="MinAssessmentCount" />
                            <Rock:DeleteField OnClick="gList_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
            <asp:HiddenField ID="hfGroupId" runat="server" />
            

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
