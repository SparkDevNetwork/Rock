<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CompetencyPersonProjectList.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Residency.CompetencyPersonProjectList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlList" CssClass="panel panel-block" runat="server">

            <asp:HiddenField ID="hfCompetencyPersonId" runat="server" />
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-folder-open-o"></i> Projects</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">

                    <Rock:Grid ID="gList" runat="server" AllowSorting="true" OnRowSelected="gList_Edit" DataKeyNames="Id" RowItemText="Project">
                        <Columns>
                            <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                            <asp:BoundField DataField="CurrentCompleted" HeaderText="Assessments Completed" SortExpression="CurrentCompleted" />
                            <asp:BoundField DataField="MinAssessmentCount" HeaderText="Assessments Required" SortExpression="MinAssessmentCount" />
                            <Rock:DeleteField OnClick="gList_Delete" />
                        </Columns>
                    </Rock:Grid>

                </div>

            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
