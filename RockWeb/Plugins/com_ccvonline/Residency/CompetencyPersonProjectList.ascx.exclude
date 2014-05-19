<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CompetencyPersonProjectList.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.CompetencyPersonProjectList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        <asp:HiddenField ID="hfCompetencyPersonId" runat="server" />
        <Rock:Grid ID="gList" runat="server" AllowSorting="true" OnRowSelected="gList_Edit" DataKeyNames="Id" RowItemText="Project" >
            <Columns>
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                <asp:BoundField DataField="CurrentCompleted" HeaderText="Assessments Completed" SortExpression="CurrentCompleted" />
                <asp:BoundField DataField="MinAssessmentCount" HeaderText="Assessments Required" SortExpression="MinAssessmentCount" />
                <Rock:DeleteField OnClick="gList_Delete" />
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>