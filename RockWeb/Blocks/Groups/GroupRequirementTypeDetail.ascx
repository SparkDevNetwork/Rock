<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupRequirementTypeDetail.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupRequirementTypeDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server" CssClass="panel panel-block">

            <asp:HiddenField ID="hfGroupRequirementTypeId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-check-square-o"></i>
                    <asp:Literal ID="lActionTitle" runat="server" /></h1>

                <div class="panel-labels">
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

                <fieldset>

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.GroupRequirementType, Rock" PropertyName="Name" />
                        </div>
                        <div class="col-md-6">
                        </div>
                    </div>

                    <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.GroupRequirementType, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.GroupRequirementType, Rock" PropertyName="IconCssClass" Label="Icon CSS Class" />
                        </div>
                        <div class="col-md-6">
                            <Rock:CategoryPicker ID="cpCategory" runat="server" AllowMultiSelect="false" Required="false" Label="Category" EntityTypeName="Rock.Model.GroupRequirementType" />
                        </div>
                        <div class="col-md-6">
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbSummary" runat="server" SourceTypeName="Rock.Model.GroupRequirementType, Rock" PropertyName="Summary" TextMode="MultiLine" Rows="4" Help="A short description of the requirement to display with the name." />
                        </div>
                    </div>

                    <h4 class="margin-t-md">Requirement Criteria</h4>
                    <span class="text-muted">The configuration below determines the how we'll know if an individual meets the requirement or not.</span>
                    <hr class="margin-t-sm" >
                    <Rock:RockControlWrapper ID="rcwRequirementCheckType" runat="server" Label="Check Type">
                        <div class="controls">
                            <div class="js-requirement-check-type">
                                <Rock:HiddenFieldWithClass ID="hfRequirementCheckType" CssClass="js-hidden-selected" runat="server" />
                                <div class="btn-group">
                                    <asp:HyperLink ID="btnRequirementCheckTypeSQL" runat="server" CssClass="btn btn-default active" Text="SQL" data-val="0" />
                                    <asp:HyperLink ID="btnRequirementCheckTypeDataview" runat="server" CssClass="btn btn-default" Text="Data View" data-val="1" />
                                    <asp:HyperLink ID="btnRequirementCheckTypeManual" runat="server" CssClass="btn btn-default" Text="Manual" data-val="2" />
                                </div>
                            </div>
                        </div>
                    </Rock:RockControlWrapper>
                    <div class="row">
                        <div class="col-md-12">
                            <div class="js-dataview-mode-div">
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:DataViewItemPicker ID="dpDataView" runat="server" Label="Meets Criteria Data View" Help="Although the field is optional, if it is not set then this 'Requirement Type' will not prevent a person from being added to the group." />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:DataViewItemPicker ID="dpWarningDataView" runat="server" Label="Warning Criteria Data View" Help="Optional data view that will return a list of people that should be marked as in a warning status." />
                                    </div>
                                </div>
                            </div>
                            <div class="js-sql-mode-div">
                                <div class="col-md-12">
                                    <label>SQL Syntax</label><a class="help" href="javascript: $('.js-sourcesql-help').toggle;"><i class="fa fa-question-circle"></i></a>
                                    <div class="alert alert-info js-sourcesql-help" id="nbSQLHelp" runat="server" style="display: none;"></div>
                                </div>
                                <div class="col-md-6">
                                    <Rock:CodeEditor ID="ceSqlExpression" runat="server" Label="Meets SQL Expression" Help="A SQL expression that returns a list of Person Ids that meet the criteria." EditorMode="Sql" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:CodeEditor ID="ceWarningSqlExpression" runat="server" Label="Warning SQL Expression" Help="Optional SQL expression that returns a list of Person Ids that should be marked as in a warning status." EditorMode="Sql" />
                                </div>
                            </div>
                            <div class="js-manual-mode-div">
                                <Rock:DataTextBox ID="tbCheckboxLabel" runat="server" SourceTypeName="Rock.Model.GroupRequirementType, Rock" PropertyName="CheckboxLabel" Help="The label that is used for the checkbox when the requirement is manually set." />
                            </div>
                        </div>
                    </div>
                    <h4 class="margin-t-md">Descriptive Labels</h4>
                    <span class="text-muted">The options below help better to describe the state of a requirement for an individual.</span>
                    <hr class="margin-t-sm" >
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbPositiveLabel" runat="server" SourceTypeName="Rock.Model.GroupRequirementType, Rock" PropertyName="PositiveLabel" Label="Meets Requirement Label" Help="The text that is displayed when the requirement is met." />
                        </div>
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbNegativeLabel" runat="server" SourceTypeName="Rock.Model.GroupRequirementType, Rock" PropertyName="NegativeLabel" Label="Does Not Meet Requirement Label" Help="The text that is displayed when the requirement is not met." />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbWarningLabel" runat="server" SourceTypeName="Rock.Model.GroupRequirementType, Rock" PropertyName="WarningLabel" Label="Warning Label" Help="The text that is displayed when the requirement in a warning state." />
                        </div>
                    </div>

                    <h4 class="margin-t-md">Workflows</h4>
                    <span class="text-muted">Workflows can be set up to help an individual complete a requirement. These workflows can be configured to be started manually, or when the requirement is triggered.</span>
                    <hr class="margin-t-sm">
                    <div class="row">
                        <div class="col-md-4">
                            <Rock:WorkflowTypePicker ID="wtpDoesNotMeetWorkflowType" runat="server" Label="Does Not Meet Requirement Workflow" Help="The workflow type to configure for requirements that are not met. These workflows can help an individual complete requirements." />
                        </div>
                        <div class="col-md-2">
                            <Rock:RockCheckBox ID="cbAutoInitiateDoesNotMeetRequirementWorkflow" runat="server" Label="Auto initiate" Help="Determines if the workflow should be automatically launched at the time of not being met, or if the workflow should be manually launched by the individual." />
                        </div>
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbDoesNotMeetRequirementWorkflowTypeLinkText" runat="server" SourceTypeName="Rock.Model.GroupRequirementType, Rock" PropertyName="DoesNotMeetWorkflowLinkText" Label="Link Text" Help="The text to use for the link to initiate the 'Does Not Meet Requirement' Workflow." />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-4">
                            <Rock:WorkflowTypePicker ID="wtpWarningWorkflowType" runat="server" Label="Warning Requirement Workflow" Help="The workflow type to configure for requirements that are in a warning state. These workflows can help an individual complete requirements." />
                        </div>
                        <div class="col-md-2">
                            <Rock:RockCheckBox ID="cbAutoInitiateWarningRequirementWorkflow" runat="server" Label="Auto initiate" Help="Determines if the workflow should be automatically launched at the time of warning, or if the workflow should be manually launched by the individual." />
                        </div>
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbWarningRequirementWorkflowTypeLinkText" runat="server" SourceTypeName="Rock.Model.GroupRequirementType, Rock" PropertyName="WarningWorkflowLinkText" Label="Link Text" Help="The text to use for the link to initiate the 'Warning' Workflow." />
                        </div>
                    </div>
                    <h4 class="margin-t-md">Additional Settings</h4>
                    <span class="text-muted">The settings below allow additional controls to be configured for requirements.</span>
                    <hr class="margin-t-sm">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbCanExpire" runat="server" Label="Can Expire" CssClass="js-can-expire-checkbox" Help="Determines if a requirement should expire after a configured period of time." />
                            <div class="js-can-expire-days">
                                <Rock:NumberBox ID="nbExpireInDays" runat="server" Label="Expire Duration" AppendText="days" CssClass="input-width-lg" Help="The number of days after the requirement is met before it expires (If CanExpire is true). Leave blank if it never expires." />
                            </div>
                        </div>
                        <div class="col-md-6">
                            <Rock:RockRadioButtonList ID="rblDueDate" runat="server" Label="Due Date" AutoPostBack="true" OnSelectedIndexChanged="rblDueDate_SelectedIndexChanged"  Help="Determines if a requirement has a period of time to be met."></Rock:RockRadioButtonList>

                            <Rock:NumberBox ID="nbDueDateOffset" runat="server" Label="Due Date Offset" AppendText="days" CssClass="input-width-lg" Help="The number of days before/after the configured date setting to determine the due date period." />

                        </div>
                    </div>
                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+s" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

        </asp:Panel>

        <script>
            function setActiveButtonGroupButton($activeBtn) {
                $activeBtn.addClass('active');
                $activeBtn.siblings('.btn').removeClass('active');

                var requirementCheckType = $activeBtn.data('val');

                // set hidden field of what requirementchecktype mode we are in
                $activeBtn.closest('.btn-group').siblings('.js-hidden-selected').val(requirementCheckType);

                // Sql
                if (requirementCheckType == '0') {
                    $('.js-sql-mode-div').show();
                }
                else {
                    $('.js-sql-mode-div').hide();
                }

                // Dataview
                if (requirementCheckType == '1') {
                    $('.js-dataview-mode-div').show();
                }
                else {
                    $('.js-dataview-mode-div').hide();
                }

                // Manual
                if (requirementCheckType == '2') {
                    $('.js-manual-mode-div').show();
                    $('.js-can-expire-checkbox').attr('disabled', 'disabled');
                    $('.js-can-expire-days').find('.form-control').attr('disabled', 'disabled').addClass('aspNetDisabled');
                }
                else {
                    $('.js-manual-mode-div').hide();
                    $('.js-can-expire-checkbox').removeAttr('disabled');
                    $('.js-can-expire-days').find('.form-control').removeAttr('disabled').removeClass('aspNetDisabled');
                }
            }

            function setExpireDaysVisibility() {
                var checked = $('.js-can-expire-checkbox').is(':checked');
                if (checked) {
                    $('.js-can-expire-days').show();
                } else {
                    $('.js-can-expire-days').hide();
                }
            }

            Sys.Application.add_load(function () {
                $('.js-requirement-check-type .btn').on('click', function (e) {
                    setActiveButtonGroupButton($(this));
                });

                setTimeout(function () {
                    setActiveButtonGroupButton($('.js-requirement-check-type').find("[data-val='" + $('.js-requirement-check-type .js-hidden-selected').val() + "']"));
                })

                $('.js-can-expire-checkbox').change(setExpireDaysVisibility);

                setExpireDaysVisibility();
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
