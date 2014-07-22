<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CompetencyPersonProjectDetail.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.CompetencyPersonProjectDetail" %>

<asp:UpdatePanel ID="upCompetencyPersonProjectDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <asp:HiddenField ID="hfCompetencyPersonProjectId" runat="server" />
            <asp:HiddenField ID="hfCompetencyPersonId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-folder-open-o"></i> <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <div id="pnlEditDetails" runat="server">

                    <fieldset>

                        <Rock:RockLiteral ID="lblPersonName" runat="server" Label="Resident" />
                        <Rock:RockLiteral ID="lblCompetency" runat="server" Label="Competency" />
                        <Rock:RockLiteral ID="lblProject" runat="server" Label="Project" />
                        <asp:Panel ID="pnlEditProject" runat="server">
                            <Rock:RockDropDownList ID="ddlProject" runat="server" DataTextField="Name" DataValueField="Id" Required="true" OnSelectedIndexChanged="ddlProject_SelectedIndexChanged" AutoPostBack="true" />

                            <Rock:RockLiteral ID="lblMinAssessmentCountDefault" runat="server" Label="Assessments Required - Default" />
                            <Rock:DataTextBox ID="tbMinAssessmentCountOverride" runat="server" SourceTypeName="com.ccvonline.Residency.Model.CompetencyPersonProject, com.ccvonline.Residency" PropertyName="MinAssessmentCount"
                                Label="Assessments Required - Override" Help="Set this to specify the number of assessments of this project that a person must complete if it should be something other than the default." CssClass="input-mini" />
                        </asp:Panel>
                        <Rock:NotificationBox ID="nbAllProjectsAlreadyAdded" runat="server" NotificationBoxType="Info" Text="All projects for this competency have already been assigned to this resident." />

                    </fieldset>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </div>

                <fieldset id="fieldsetViewDetails" runat="server">

                    <div class="row">
                        <Rock:NotificationBox ID="NotificationBox1" runat="server" NotificationBoxType="Info" />
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <asp:Literal ID="lblMainDetails" runat="server" />
                        </div>
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                    </div>

                </fieldset>

            </div>

            
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
