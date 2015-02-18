<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ProjectDetail.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Residency.ProjectDetail" %>

<!-- just to help do css intellisense at design time  -->
<link rel="stylesheet" type="text/css" href="~/Themes/Rock/Styles/bootstrap.css" visible="false" />
<link rel="stylesheet" type="text/css" href="~/Themes/Rock/Styles/theme.css" visible="false" />

<asp:UpdatePanel ID="upProjectDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <asp:HiddenField ID="hfProjectId" runat="server" />
            <asp:HiddenField ID="hfCompetencyId" runat="server" />
            <asp:HiddenField ID="hfCloneFromProjectId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-folder-open-o"></i> <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <div id="pnlEditDetails" runat="server">

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:NotificationBox ID="nbCloneMessage" runat="server" />
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="church.ccv.Residency.Model.Project, church.ccv.Residency" PropertyName="Name" />
                            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="church.ccv.Residency.Model.Project, church.ccv.Residency" PropertyName="Description" TextMode="MultiLine" Rows="3" CssClass="input-xxlarge" />
                            <Rock:RockLiteral ID="lblPeriod" runat="server" Label="Period" />
                            <Rock:RockLiteral ID="lblTrack" runat="server" Label="Track" />
                            <Rock:RockLiteral ID="lblCompetency" runat="server" Label="Competency" />
                            <Rock:DataTextBox ID="tbMinAssessmentCountDefault" runat="server" SourceTypeName="church.ccv.Residency.Model.Project, church.ccv.Residency" PropertyName="MinAssessmentCountDefault"
                                Label="Default # of Assessments" Help="Set this to specify the default minimum number of assessments of this project that a person must complete." CssClass="input-mini" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </div>

                <fieldset id="fieldsetViewDetails" runat="server">
                
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
