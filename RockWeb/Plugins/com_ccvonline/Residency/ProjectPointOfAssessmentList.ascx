<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ProjectPointOfAssessmentList.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.ProjectPointOfAssessmentList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" runat="server">
            <h4>Competencies</h4>
            <asp:HiddenField ID="hfProjectId" runat="server" />
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />

            <Rock:Grid ID="gList" runat="server" OnRowSelected="gList_Edit" DataKeyNames="Id" AllowSorting="false">
                <Columns>
                    <Rock:ColorField DataField="ProjectPointOfAssessmentColor" ToolTipDataField="PointOfAssessmentTypeValue.Name" />
                    <Rock:ReorderField />
                    <asp:BoundField DataField="AssessmentOrder" HeaderText="#" />
                    <asp:BoundField DataField="AssessmentText" HeaderText="Text" />
                    <Rock:DeleteField OnClick="gList_Delete" />
                </Columns>
            </Rock:Grid>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
