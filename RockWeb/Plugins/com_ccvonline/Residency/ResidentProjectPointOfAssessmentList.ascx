<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ResidentProjectPointOfAssessmentList.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.ResidentProjectPointOfAssessmentList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" runat="server">
            <asp:HiddenField ID="hfCompetencyPersonProjectId" runat="server" />

            <h4>Points of Assessment</h4>

            <Rock:Grid ID="gList" runat="server" DataKeyNames="Id" AllowSorting="false" DisplayType="Light">
                <Columns>
                    <Rock:ColorField DataField="ProjectPointOfAssessmentColor" ToolTipDataField="PointOfAssessmentTypeValue.Name" />
                    <asp:BoundField DataField="AssessmentOrder" HeaderText="#" />
                    <asp:BoundField DataField="AssessmentText" HeaderText="Text" />
                </Columns>
            </Rock:Grid>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
