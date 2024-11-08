<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AssessmentTypeDetail.ascx.cs" Inherits="RockWeb.Blocks.Assessments.AssessmentTypeDetail" %>

<asp:UpdatePanel ID="upMain" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbBlockStatus" runat="server" NotificationBoxType="Info" />

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="false">
            <asp:HiddenField ID="hfEntityId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lIcon" runat="server" />
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                </div>

            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <asp:ValidationSummary ID="valAssessmentTypeDetail" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <div id="pnlViewDetails" runat="server">
                    <div class="row">
                        <div class="col-md-12">
                            <asp:Literal ID="lAssessmentTypeDescription" runat="server"></asp:Literal>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lblMainDetails" runat="server" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" data-shortcut-key="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
                        <span class="pull-right">
                            <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-square btn-security" />
                        </span>
                    </div>
                </div>

                <div id="pnlEditDetails" runat="server">

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbTitle" runat="server" SourceTypeName="Rock.Model.AssessmentType, Rock" PropertyName="Title" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbIsActive" runat="server" SourceTypeName="Rock.Model.AssessmentType, Rock" PropertyName="IsActive" Label="Active" Checked="true" />
                        </div>
                    </div>

                    <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.AssessmentType, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                    <Rock:DataTextBox ID="tbAssessmentPath" runat="server" SourceTypeName="Rock.Model.AssessmentType, Rock" PropertyName="AssessmentPath" />
                    <Rock:DataTextBox ID="tbResultsPath" runat="server" SourceTypeName="Rock.Model.AssessmentType, Rock" PropertyName="AssessmentResultsPath" />

                    <div class="row">
                        <div class="col-md-3">
                            <Rock:NumberBox ID="nbDaysBeforeRetake"
                                runat="server"
                                SourceTypeName="Rock.Model.AssessmentType, Rock"
                                PropertyName="MinimumDaysToRetake"
                                Label="Minimum Days to Re-take"
                                AppendText="days"
                                MinimumValue="0"
                                CssClass="input-width-md"
                                Help="The minimum number of days after the test has been taken before it can be taken again." />
                        </div>
                        <div class="col-md-3">
                            <%-- Spacer --%>
                        </div>
                        <div class="col-md-3">
                            <Rock:RockCheckBox ID="cbRequiresRequest"
                                runat="server"
                                SourceTypeName="Rock.Model.AssessmentType, Rock"
                                PropertyName="RequiresRequest"
                                Label="Requires Request"
                                Help="Is a person required to receive a request before this test can be taken?" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
