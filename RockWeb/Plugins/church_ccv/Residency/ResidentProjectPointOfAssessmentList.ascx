<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ResidentProjectPointOfAssessmentList.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.ResidentProjectPointOfAssessmentList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" CssClass="panel panel-block" runat="server">
            <asp:HiddenField ID="hfCompetencyPersonProjectId" runat="server" />


            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-list-ul"></i> Points of Assessment</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" DataKeyNames="Id" AllowSorting="false" DisplayType="Light">
                        <Columns>
                            <Rock:ColorField DataField="ProjectPointOfAssessmentColor" ToolTipDataField="PointOfAssessmentTypeValue.Name" />
                            <asp:BoundField DataField="AssessmentOrder" HeaderText="#" />
                            <asp:BoundField DataField="AssessmentText" HeaderText="Text" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>


        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
