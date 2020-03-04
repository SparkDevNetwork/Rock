﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AchievementTypeDetail.ascx.cs" Inherits="RockWeb.Blocks.Streaks.AchievementTypeDetail" %>

<style>
.chart-banner
{
    width: 100%;
}
.chart-banner canvas
{
    height: 350px;
}
</style>

<asp:UpdatePanel ID="upAchievementTypeDetail" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfIsEditMode" runat="server" />

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lIcon" runat="server" />
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>
                <div class="panel-labels">
                    <asp:Literal ID="lCategory" runat="server" />
                    <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                </div>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <asp:ValidationSummary ID="valAchievementTypeDetail" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <asp:CustomValidator ID="cvCustomValidator" runat="server" Display="None" />

                <div id="pnlViewDetails" runat="server">
                    <div class="row margin-b-lg">
                        <div class="col-md-12">
                            <asp:Literal ID="lDescription" runat="server" />
                        </div>
                    </div>

                    <div id="pnlActivitySummary" runat="server" class="row">
                        <div class="col-sm-6">
                            <h5>Successful Attempts</h5>
                        </div>
                        <div class="col-sm-6">
                            <asp:LinkButton ID="btnRefreshChart" runat="server" CssClass="btn btn-default pull-right" ToolTip="Refresh Chart" OnClick="btnRefreshChart_Click">
                                <i class="fa fa-refresh"></i>
                            </asp:LinkButton>
                            <Rock:SlidingDateRangePicker ID="drpSlidingDateRange" CssClass="pull-right" runat="server" EnabledSlidingDateRangeTypes="Previous, Last, Current, DateRange"
                                EnabledSlidingDateRangeUnits="Week, Month, Year" SlidingDateRangeMode="Current" TimeUnit="Year" Label="" />
                        </div>
                        <div class="col-sm-12">
                            <Rock:NotificationBox ID="nbActivityChartMessage" runat="server" NotificationBoxType="Info" />
                            <div id="pnlActivityChart" runat="server" class="chart-banner">
                                <canvas id="chartCanvas" runat="server" />
                            </div>
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" CausesValidation="false" OnClick="btnDelete_Click" />
                        <asp:LinkButton ID="btnRebuild" runat="server" Text="Rebuild" CssClass="btn btn-danger pull-right" CausesValidation="false" OnClick="btnRebuild_Click" />
                    </div>
                </div>

                <div id="pnlEditDetails" runat="server">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.StreakTypeAchievementType, Rock" PropertyName="Name" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbActive" runat="server" SourceTypeName="Rock.Model.StreakTypeAchievementType, Rock" PropertyName="IsActive" Label="Active" Checked="true" Text="Yes" />
                        </div>
                    </div>

                    <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.StreakTypeAchievementType, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />

                    <div class="row">
                        <div class="col-sm-12">
                            <Rock:StreakTypePicker ID="stpStreakType" runat="server" Label="Streak Type" />
                        </div>
                        <div class="col-md-6">
                            <Rock:CategoryPicker ID="cpCategory" runat="server" Label="Category" EntityTypeName="Rock.Model.StreakTypeAchievementType" />
                            <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.StreakTypeAchievementType, Rock" Label="Icon CSS Class" PropertyName="AchievementIconCssClass" ValidateRequestMode="Disabled" Help="The font awesome icon class to use for this achievement." />                            
                        </div>
                        <div class="col-md-3 col-sm-6">
                            <Rock:RockCheckBox ID="cbAllowOverachievement" runat="server" SourceTypeName="Rock.Model.StreakTypeAchievementType, Rock" PropertyName="AllowOverAchievement" Label="Allow Overachievement" Checked="false" Text="Yes" AutoPostBack="true" OnCheckedChanged="cbAllowOverachievement_CheckedChanged" Help="When enabled, achievement beyond the defined goal will be tracked so it is possible for progress to be greater than 100%. Only one achievement is allowed when this is enabled." />
                        </div>
                        <div class="col-md-3 col-sm-6">
                            <Rock:NumberBox ID="nbMaxAccomplishments" runat="server" SourceTypeName="Rock.Model.StreakTypeAchievementType, Rock" PropertyName="MaxAccomplishmentsAllowed" Label="Max Accomplishments Allowed" MinimumValue="1" Help="How many times are people allowed to earn this achievement. This must be 1 in order to track overachievement." />
                        </div>
                    </div>

                    <hr />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:ComponentPicker ID="cpAchievementComponent" runat="server" Label="Achievement Component" Required="true" ContainerType="Rock.Achievement.AchievementContainer" AutoPostBack="true" OnSelectedIndexChanged="cpAchievementComponent_SelectedIndexChanged" Help="The achievement type allows different methods for calculating a successful achievement." />
                        </div>
                        <div class="col-md-6">
                            <p class="panel-body">
                                <asp:Literal ID="lComponentDescription" runat="server" />
                            </p>
                        </div>
                    </div>

                    <Rock:AttributeValuesContainer ID="avcComponentAttributes" runat="server" NumberOfColumns="2" />

                    <hr />

                    <Rock:PanelWidget ID="pwStep" runat="server" Title="Step Configuration">
                        <div class="row">
                            <div class="col-sm-12">
                                <Rock:RockCheckBox Label="Add Step on Success" ID="cbAddStep" runat="server" OnCheckedChanged="cbAddStep_CheckedChanged" AutoPostBack="true" />
                            </div>
                        </div>
                        <div class="row" id="divStepControls" runat="server" visible="false">
                            <div class="col-md-6 col-sm-12">
                                <Rock:StepProgramStepTypePicker ID="spstStepType" runat="server" Help="When this achievement is earned, a step can be added for the person if you choose that step program, step type, and then a status." AutoPostBack="true" OnSelectedIndexChanged="spstStepType_SelectedIndexChanged" Required="true" />
                            </div>
                            <div class="col-md-6 col-sm-12">
                                <Rock:StepStatusPicker ID="sspStepStatus" runat="server" Label="Step Status" Help="When this achievement is earned, a step can be added for the person if you choose that step program, step type, and then a status." Required="true" />
                            </div>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpAdvanced" runat="server" Title="Advanced Settings">
                        <div class="row">
                            <div class="col-sm-12">
                                <Rock:RockCheckBoxList ID="cblPrerequsities"
                                    runat="server"
                                    Label="Prerequisite Achievements"
                                    Help="The achievements that must be earned prior to this achievement."
                                    RepeatDirection="Vertical"
                                    DataValueField="Id"
                                    DataTextField="Name" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-4">
                                <Rock:WorkflowTypePicker ID="wtpStartWorkflowType" runat="server" Label="Start Workflow Type" Help="The type of workflow to trigger when a person begins an attempt at this achievement" />
                            </div>
                            <div class="col-md-4">
                                <Rock:WorkflowTypePicker ID="wtpSuccessWorkflowType" runat="server" Label="Success Workflow Type" Help="The type of workflow to trigger when a person successfully finishes an attempt at this achievement" />
                            </div>
                            <div class="col-md-4">
                                <Rock:WorkflowTypePicker ID="wtpFailureWorkflowType" runat="server" Label="Failure Workflow Type" Help="The type of workflow to trigger when a person unsuccessfully finishes an attempt at this achievement" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-sm-12">
                                <Rock:CodeEditor ID="ceBadgeLava" runat="server" EditorHeight="200" EditorMode="Lava" EditorTheme="Rock" Label="Badge Lava Template" Help="The template to use when displaying a badge for this achievement." />
                            </div>
                            <div class="col-sm-12">
                                <Rock:CodeEditor ID="ceResultsLava" runat="server" EditorHeight="200" EditorMode="Lava" EditorTheme="Rock" Label="Results Lava Template" Help="The template to use when displaying the results of this achievement." />
                            </div>
                        </div>
                    </Rock:PanelWidget>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
