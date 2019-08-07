<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowTypeDetail.ascx.cs" Inherits="RockWeb.Blocks.WorkFlow.WorkflowTypeDetail" %>


<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" >
            <asp:HiddenField ID="hfWorkflowTypeId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-cogs"></i>
                    <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlTypeId" runat="server" LabelType="Default" />
                    <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                    <Rock:HighlightLabel ID="hlType" runat="server" LabelType="Type" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <Rock:NotificationBox ID="nbValidationError" runat="server" NotificationBoxType="Danger" EnableViewState="false" />

                <div id="pnlEditDetails" runat="server">

                    <Rock:PanelWidget ID="pwDetails" runat="server" Title="Details" Expanded="true">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.WorkflowType, Rock" PropertyName="Name" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbIsActive" runat="server" Text="Active" />
                                <Rock:RockCheckBox ID="cbIsPersisted" runat="server" Text="Automatically Persisted" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12">
                                <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.WorkflowType, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbWorkTerm" runat="server" SourceTypeName="Rock.Model.WorkflowType, Rock" PropertyName="WorkTerm" Label="Work Term" />
                                <Rock:CategoryPicker ID="cpCategory" runat="server" Required="true" Label="Category" EntityTypeName="Rock.Model.WorkflowType" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="tbNumberPrefix" runat="server" Label="Workflow Number Prefix" Help="The number prefix to use for workflows of this type. For example, to have workflows of this type numbered like 'WF0001, WF0002' use a prefix of 'WF'. Prefixes do need to be unique between workflow types." />
                                <Rock:RockTextBox ID="tbIconCssClass" runat="server" Label="Icon CSS Class" Help="The Icon to use when displaying this type of workflow." />
                            </div>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpAdvancedDetails" runat="server" Title="Advanced Settings">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="tbProcessingInterval" runat="server" Label="Processing Interval (minutes)"
                                    Help="The minimum length of time, in minutes, that must pass before the same persisted workflow instance of this type can be processed again.  If blank, active workflows will be processed each time that the workflow job is run." />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockDropDownList ID="ddlLoggingLevel" Help="The level you would like to audit.  Start and stop times can be logged for each workflow, workflow activity, or activity action." runat="server" Label="Logging Level" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="tbCompletedRetention" runat="server" Label="Completed Workflow Retention Period (days)"
                                    Help="The minimum length of time, in days, that completed workflows should be retained for this workflow.  If blank, completed workflows will never be removed." />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="tbLogRetention" runat="server" Label="Log Retention Period (days)"
                                    Help="The minimum length of time, in days, that the logs will be retained for this workflow. If blank, logs will never be removed." />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12">
                                <Rock:CodeEditor ID="ceNoActionMessage" runat="server" EditorMode="Lava" EditorTheme="Rock" EditorHeight="100" Label="No Action Message"
                                    Help="The text to be displayed when a workflow of this type is active, but does not have an active user entry form. <span class='tip tip-lava'></span> <span class='tip tip-html'>" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12">
                                <Rock:CodeEditor ID="ceSummaryViewText" runat="server" EditorMode="Lava" EditorTheme="Rock" EditorHeight="500" Label="Summary View"
                                    Help="The summary view text to be displayed when a workflow of this type has no user entry form or the workflow has been completed. <span class='tip tip-lava'></span> <span class='tip tip-html'>" />
                            </div>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpAttributes" runat="server" Title="Attributes" CssClass="attribute-panel">
                        <div class="grid">
                            <Rock:Grid ID="gAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Attribute">
                                <Columns>
                                    <Rock:ReorderField />
                                    <Rock:RockBoundField DataField="Name" HeaderText="Attribute" />
                                    <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                    <Rock:RockBoundField DataField="Key" HeaderText="Key" />
                                    <Rock:RockBoundField DataField="FieldType" HeaderText="Field Type" />
                                    <Rock:BoolField DataField="IsRequired" HeaderText="Required" />
                                    <Rock:EditField OnClick="gAttributes_Edit" />
                                    <Rock:DeleteField OnClick="gAttributes_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </Rock:PanelWidget>

                    <div class="workflow-section-activities">

                        <fieldset>
                            <legend>Activities
                                <span class="pull-right">
                                    <asp:LinkButton ID="lbAddActivityType" runat="server" CssClass="btn btn-action btn-xs" OnClick="lbAddActivityType_Click" CausesValidation="false"><i class="fa fa-plus"></i> Add Activity</asp:LinkButton>
                                </span>
                            </legend>
                            <div class="workflow-activity-list">
                                <asp:PlaceHolder ID="phActivities" runat="server" />
                            </div>
                        </fieldset>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </div>

                <fieldset id="fieldsetViewDetails" runat="server">

                    <div class="description">
                        <asp:Literal ID="lWorkflowTypeDescription" runat="server" EnableViewState="false"></asp:Literal>
                    </div>

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" EnableViewState="false" />

                    <div class="row">
                        <div class="col-md-12">
                            <a class="workflow-activities-readonly-header" href="#" onclick="javascript: toggleReadOnlyActivitiesList();">
                                <asp:Label ID="lblActivitiesReadonlyHeaderLabel" runat="server" Text="Activities" EnableViewState="false" />
                                <b class="fa fa-caret-down"></b>
                            </a>

                            <div class="workflow-activities-readonly-list" style="display: none">
                                <asp:Literal ID="lblWorkflowActivitiesReadonly" runat="server" EnableViewState="false" />
                            </div>
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="m" ToolTip="Alt+m" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
                        <span class="pull-right">
                            <asp:LinkButton ID="btnCopy" runat="server" CssClass="btn btn-default btn-sm btn-square fa fa-clone" OnClick="btnCopy_Click" ToolTip="Copy Workflow" />
                            <asp:LinkButton ID="lbLaunchWorkflow" runat="server" CssClass="btn btn-sm btn-square btn-default" OnClick="btnLaunch_Click" ToolTip="Launch Workflow"><i class="fa fa-play"></i></asp:LinkButton>
                            <asp:LinkButton ID="lbManage" runat="server" CssClass="btn btn-sm btn-square btn-default" OnClick="btnManage_Click" ToolTip="Manage Workflows"><i class="fa fa-list"></i></asp:LinkButton>
                            <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-square btn-security" />
                            <asp:LinkButton ID="btnExport" runat="server" class="btn btn-sm btn-square btn-default" ToolTip="Export Workflows"  OnClick="btnExport_Click"><i class="fa fa-file-export"></i></asp:LinkButton>
                        </span>

                    </div>

                </fieldset>

            </div>

        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgAttribute" runat="server" Title="Workflow Attributes" OnSaveClick="dlgAttribute_SaveClick"  OnSaveThenAddClick="dlgAttribute_SaveThenAddClick"  OnCancelScript="clearActiveDialog();" ValidationGroup="Attributes">
            <Content>
                <Rock:AttributeEditor ID="edtAttributes" runat="server" ShowActions="false" ValidationGroup="Attributes" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgActivityAttribute" runat="server" Title="Activity Attributes" OnSaveClick="dlgActivityAttribute_SaveClick" OnSaveThenAddClick="dlgActivityAttribute_SaveThenAddClick" OnCancelScript="clearActiveDialog();" ValidationGroup="ActivityAttributes">
            <Content>
                <asp:HiddenField ID="hfActivityTypeGuid" runat="server" />
                <Rock:AttributeEditor ID="edtActivityAttributes" runat="server" ShowActions="false" ValidationGroup="ActivityAttributes" />
            </Content>
        </Rock:ModalDialog>

        <script>

            function toggleReadOnlyActivitiesList() {
                $('.workflow-activities-readonly-list').toggle(500);
            }

            Sys.Application.add_load(function () {
                var fixHelper = function (e, ui) {
                    ui.children().each(function () {
                        $(this).width($(this).width());
                    });
                    return ui;
                };

                $('.workflow-activity-list').sortable({
                    helper: fixHelper,
                    handle: '.workflow-activity-reorder',
                    containment: 'parent',
                    tolerance: 'pointer',
                    start: function (event, ui) {
                        {
                            var start_pos = ui.item.index();
                            ui.item.data('start_pos', start_pos);
                        }
                    },
                    update: function (event, ui) {
                        {
                            var postbackArg = 're-order-activity:' + ui.item.attr('data-key') + ';' + ui.item.index();
                            window.location = "javascript:__doPostBack('<%=upDetail.ClientID %>', '" +  postbackArg + "')";
                        }
                    }
                });

                $('.workflow-action-list').sortable({
                    helper: fixHelper,
                    handle: '.workflow-action-reorder',
                    containment: 'parent',
                    tolerance: 'pointer',
                    start: function (event, ui) {
                        {
                            var start_pos = ui.item.index();
                            ui.item.data('start_pos', start_pos);
                        }
                    },
                    update: function (event, ui) {
                        {
                            var postbackArg = 're-order-action:' + ui.item.attr('data-key') + ';' + ui.item.index();
                            window.location = "javascript:__doPostBack('<%=upDetail.ClientID %>', '" +  postbackArg + "')";
                        }
                    }
                });

                $('.workflow-formfield-list').sortable({
                    helper: fixHelper,
                    handle: '.workflow-formfield-reorder',
                    containment: 'parent',
                    tolerance: 'pointer',
                    start: function (event, ui) {
                        {
                            var start_pos = ui.item.index();
                            ui.item.data('start_pos', start_pos);
                        }
                    },
                    update: function (event, ui) {
                        {
                            var postbackArg = 're-order-formfield:' + ui.item.attr('data-key') + ';' + ui.item.index();
                            window.location = "javascript:__doPostBack('<%=upDetail.ClientID %>', '" +  postbackArg + "')";
                        }
                    }
                });

            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
