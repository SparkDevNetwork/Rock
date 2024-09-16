<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StepProgramDetail.ascx.cs" Inherits="RockWeb.Blocks.Steps.StepProgramDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<style>
.chart-banner { width: 100%; }
.chart-banner canvas { height: 350px; }
</style>

<asp:UpdatePanel ID="upStepProgram" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbBlockStatus" runat="server" NotificationBoxType="Info" Visible="false" />
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="false">
            <asp:HiddenField ID="hfStepProgramId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lIcon" runat="server" />
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlCategory" runat="server" LabelType="Info" />
                    <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                </div>

            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <asp:ValidationSummary ID="valStepProgramDetail" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <div id="pnlViewDetails" runat="server">
                    <h3 class="mt-1">
                        <asp:Literal ID="lStepProgramName" runat="server" />
                        Step Program
                    </h3>
                    <div class="row">
                        <div class="col-sm-12 col-md-6 col-lg-7">
                            <div class="description"><asp:Literal ID="lStepProgramDescription" runat="server" /></div>
                        </div>
                        <div class="col-sm-12 col-md-6 col-lg-5">
                            <div class="flex-grow-1 d-flex justify-content-end">
                                <Rock:SlidingDateRangePicker ID="drpSlidingDateRange"
                                    runat="server"
                                    EnabledSlidingDateRangeTypes="Previous, Last, Current, DateRange"
                                    EnabledSlidingDateRangeUnits="Week, Month, Year, Day, Hour"
                                    SlidingDateRangeMode="Current"
                                    TimeUnit="Year"
                                    Label=""
                                    FormGroupCssClass="input-group-sm d-flex flex-wrap justify-content-end" />

                                <asp:LinkButton ID="btnRefreshChart" runat="server" CssClass="btn btn-default btn-sm btn-square" ToolTip="Refresh Chart"
                                    OnClick="btnRefreshChart_Click"><i class="fa fa-refresh"></i></asp:LinkButton>
                            </div>
                        </div>
                    </div>
                    <asp:Literal ID="lKpi" runat="server" />

                    <%-- Steps Activity Summary --%>
                    <div id="pnlActivitySummary" runat="server">
                        <%-- Steps Activity Chart --%>
                        <Rock:NotificationBox ID="nbActivityChartMessage" runat="server" NotificationBoxType="Info" />
                        <div id="pnlActivityChart" runat="server" class="chart-banner">
                            <canvas id="chartCanvas" runat="server" />
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
                            <asp:HyperLink ID="hlStepFlow" runat="server" CssClass="btn btn-sm btn-default btn-step-flow" CausesValidation="false"><i class="fas fa-project-diagram"></i> Step Flow</asp:HyperLink>
                            <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-security btn-square" />
                        </span>
                    </div>
                </div>

                <div id="pnlEditDetails" runat="server">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.StepProgram, Rock" PropertyName="Name" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbActive" runat="server" SourceTypeName="Rock.Model.StepProgram, Rock" PropertyName="IsActive" Label="Active" Checked="true" />
                        </div>
                    </div>

                    <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.StepProgram, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.StepProgram, Rock" Label="Icon CSS Class" PropertyName="IconCssClass" ValidateRequestMode="Disabled" />
                            <Rock:CategoryPicker ID="cpCategory" runat="server" EntityTypeName="Rock.Model.StepProgram" Label="Category" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockRadioButtonList ID="rblDefaultListView" runat="server" Label="Default List View" RepeatDirection="Horizontal" />
                        </div>
                    </div>

                    <Rock:PanelWidget ID="wpAttributes" runat="server" Title="Step Type Attributes">
                        <div class="grid">
                            <Rock:Grid ID="gAttributes" runat="server" RowItemText="Step Type Attribute">
                                <Columns>
                                    <Rock:ReorderField />
                                    <Rock:RockBoundField DataField="Name" HeaderText="Attribute" />
                                    <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                    <Rock:BoolField DataField="IsRequired" HeaderText="Required" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                                    <Rock:EditField OnClick="gAttributes_Edit" />
                                    <Rock:DeleteField OnClick="gAttributes_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:ModalDialog ID="dlgAttribute" runat="server" Title="Step Participant Attributes" OnSaveClick="dlgAttribute_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Attributes">
                        <Content>
                            <Rock:AttributeEditor ID="edtAttributes" runat="server" ShowActions="false" ValidationGroup="Attributes" />
                        </Content>
                    </Rock:ModalDialog>

                    <Rock:PanelWidget ID="wpStatuses" runat="server" Title="Statuses">
                        <div class="grid">
                            <Rock:Grid ID="gStatuses" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Status" ShowConfirmDeleteDialog="false">
                                <Columns>
                                    <Rock:ReorderField />
                                    <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                                    <Rock:BoolField DataField="IsCompleteStatus" HeaderText="Completion" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                                    <Rock:EditField OnClick="gStatuses_Edit" />
                                    <Rock:DeleteField OnClick="gStatuses_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpWorkflow" runat="server" Title="Workflows">
                        <div class="grid">
                            <Rock:Grid ID="gWorkflows" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Workflow" ShowConfirmDeleteDialog="false">
                                <Columns>
                                    <Rock:RockBoundField DataField="WorkflowTypeName" HeaderText="Workflow Type" />
                                    <Rock:RockBoundField DataField="TriggerDescription" HeaderText="Trigger" />
                                    <Rock:EditField OnClick="gWorkflows_Edit" />
                                    <Rock:DeleteField OnClick="gWorkflows_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </Rock:PanelWidget>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>

        <Rock:ModalAlert ID="mdCopy" runat="server" />

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgStepStatuses" runat="server" ScrollbarEnabled="false" SaveButtonText="Add" OnSaveClick="btnAddStepStatus_Click" Title="Create Status" ValidationGroup="StepStatus">
            <Content>
                <asp:HiddenField ID="hfStepProgramAddStepStatusGuid" runat="server" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbStepStatusName" SourceTypeName="Rock.Model.StepStatus, Rock" PropertyName="Name" Label="Name" runat="server" ValidationGroup="StepStatus" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" ValidationGroup="StepStatus" />
                        <Rock:RockCheckBox ID="cbIsCompleted"
                            runat="server"
                            Label="Is Complete"
                            Help="Does this status indicate that the step has been completed?"
                            ValidationGroup="StepStatus" />
                        <Rock:ColorPicker ID="cpStatus"
                            runat="server"
                            Label="Display Color"
                            Help="The color used to display a step having this status." />
                    </div>

                </div>
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgStepWorkflow" runat="server" Title="Select Workflow" OnSaveClick="dlgStepWorkflow_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="StepWorkflow">
            <Content>
                <asp:HiddenField ID="hfAddStepWorkflowGuid" runat="server" />
                <asp:ValidationSummary ID="valStepWorkflowSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="StepWorkflow" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlTriggerType" runat="server" Label="Launch Workflow When"
                            OnSelectedIndexChanged="ddlTriggerType_SelectedIndexChanged" AutoPostBack="true" Required="true" ValidationGroup="StepWorkflow" />
                    </div>
                    <div class="col-md-6">
                        <Rock:WorkflowTypePicker ID="wpWorkflowType" runat="server" Label="Workflow Type" Required="true" ValidationGroup="StepWorkflow" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlPrimaryQualifier" runat="server" Visible="false" ValidationGroup="StepWorkflow" />
                        <Rock:RockDropDownList ID="ddlSecondaryQualifier" runat="server" Visible="false" ValidationGroup="StepWorkflow" />
                    </div>
                    <div class="col-md-6">
                    </div>
                </div>

            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
