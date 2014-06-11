<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CompetencyPersonProjectAssessmentList.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.CompetencyPersonProjectAssessmentList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlList" runat="server">
            <h4>Assessments</h4>

            <asp:HiddenField ID="hfCompetencyPersonProjectId" runat="server" />
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />

            <Rock:Grid ID="gList" runat="server" AllowSorting="true" OnRowSelected="gList_Edit" DataKeyNames="Id" RowItemText="Assessment">
                <Columns>
                    <Rock:DateTimeField DataField="AssessmentDateTime" HeaderText="Assessment Date/Time" SortExpression="AssessmentDateTime" />
                    <asp:BoundField DataField="OverallRating" HeaderText="Rating" SortExpression="OverallRating" />
                    <Rock:DeleteField OnClick="gList_Delete" />
                </Columns>
            </Rock:Grid>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
