<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ProjectPointOfAssessmentList.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.ProjectPointOfAssessmentList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-list-ul"></i> Points of Assessment</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" OnRowSelected="gList_Edit" DataKeyNames="Id" AllowSorting="false">
                        <Columns>
                            <Rock:ColorField DataField="ProjectPointOfAssessmentColor" ToolTipDataField="PointOfAssessmentTypeValue.Name" />
                            <Rock:ReorderField />
                            <asp:BoundField DataField="AssessmentOrder" HeaderText="#" />
                            <asp:BoundField DataField="AssessmentText" HeaderText="Text" />
                            <Rock:DeleteField OnClick="gList_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

            <asp:HiddenField ID="hfProjectId" runat="server" />
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
