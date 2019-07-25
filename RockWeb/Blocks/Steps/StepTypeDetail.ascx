<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StepTypeDetail.ascx.cs" Inherits="RockWeb.Blocks.Steps.StepTypeDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upStepType" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbBlockStatus" runat="server" NotificationBoxType="Info" />

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="false">
            <asp:HiddenField ID="hfStepTypeId" runat="server" />

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
                <asp:ValidationSummary ID="valStepTypeDetail" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <div id="pnlViewDetails" runat="server">
                    <div class="row">
                        <div class="col-md-12">
                            <asp:Literal ID="lStepTypeDescription" runat="server"></asp:Literal>
                        </div>
                    </div>
                    <%-- Steps Activity Summary --%>
                    <div id="pnlActivitySummary" runat="server">
                        <div class="row">
                            <div class="col-sm-6">
                                <h5>Steps Activity Summary</h5>
                            </div>
                            <div class="col-sm-6">

                                <asp:LinkButton ID="btnRefreshChart" runat="server" CssClass="btn btn-default pull-right" ToolTip="Refresh Chart"
                                    OnClick="btnRefreshChart_Click"><i class="fa fa-refresh"></i></asp:LinkButton>

                                <Rock:SlidingDateRangePicker ID="drpSlidingDateRange"
                                            runat="server"
                                            EnabledSlidingDateRangeTypes="Previous, Last, Current, DateRange"
                                            EnabledSlidingDateRangeUnits="Week, Month, Year"
                                            SlidingDateRangeMode="Current"
                                            TimeUnit="Year"
                                            Label=""
                                            CssClass="pull-right" />

                            </div>
                        </div>
                        <%-- Steps Activity Chart --%>
                        <Rock:NotificationBox ID="nbActivityChartMessage" runat="server" NotificationBoxType="Info" />
                        <div id="pnlActivityChart" runat="server">
                            <div class="chart-container" align="center">
                                <canvas id="chartCanvas" runat="server" height="350" width="700" />
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lblMainDetails" runat="server" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
                        <span class="pull-right">
                            <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-security" />
                        </span>
                    </div>
                </div>

                <div id="pnlEditDetails" runat="server">

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.StepType, Rock" PropertyName="Name" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbIsActive" runat="server" SourceTypeName="Rock.Model.StepType, Rock" PropertyName="IsActive" Label="Active" Checked="true" Text="Yes" />
                        </div>
                    </div>

                    <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.StepType, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbIconCssClass"
                                runat="server"
                                SourceTypeName="Rock.Model.StepType, Rock"
                                PropertyName="IconCssClass"
                                Label="Icon CSS Class"
                                Help="The Font Awesome icon class to use when displaying steps of this type."
                                ValidateRequestMode="Disabled" />
                            <Rock:RockCheckBox ID="cbAllowMultiple"
                                runat="server"
                                SourceTypeName="Rock.Model.StepType, Rock"
                                PropertyName="AllowMultiple"
                                Label="Allow Multiple"
                                Text="Yes"
                                Help="Can this step be achieved more than once by the same person?" />
                            <Rock:RockCheckBox ID="cbHasDuration"
                                runat="server"
                                SourceTypeName="Rock.Model.StepType, Rock"
                                PropertyName="HasEndDate"
                                Label="Spans Time"
                                Text="Yes"
                                Help="Does this step occur immediately or does it take some period of time to complete?" />
                            <Rock:RockCheckBox ID="cbShowBadgeCount"
                                runat="server"
                                SourceTypeName="Rock.Model.StepType, Rock"
                                PropertyName="ShowCountOnBadge"
                                Label="Show Count on Badge"
                                Help="Should the number of achievements for this step type be shown on the step badge?"
                                Checked="false"
                                Text="Yes" />
                        </div>
                        <div class="col-md-6">
                            <Rock:ColorPicker ID="cpHighlight"
                                runat="server"
                                Label="Highlight Color"
                                Help="The color to use when displaying steps of this type." />
                            <Rock:RockCheckBoxList ID="cblPrerequsities"
                                runat="server"
                                Label="Prerequisite Steps"
                                Help="The steps that must be completed prior to this step."
                                RepeatDirection="Vertical"
                                DataValueField="Id"
                                DataTextField="Name" />
                        </div>
                    </div>

                    <Rock:PanelWidget ID="wpAttributes" runat="server" Title="Step Participant Attributes">
                        <div class="grid">
                            <Rock:Grid ID="gAttributes" runat="server" RowItemText="Step Participant Attribute" >
                                <Columns>
                                    <Rock:ReorderField />
                                    <Rock:RockBoundField DataField="Name" HeaderText="Attribute" />
                                    <Rock:RockBoundField DataField="FieldType" HeaderText="Field Type" />
                                    <Rock:BoolField DataField="AllowSearch" HeaderText="Allow Search" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                                    <Rock:EditField OnClick="gAttributes_Edit" />
                                    <Rock:DeleteField OnClick="gAttributes_Delete" />
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

                    <Rock:PanelWidget ID="wpAdvanced" runat="server" Title="Advanced Settings">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataViewItemPicker ID="dvpAutocomplete"
                                    runat="server"
                                    DataField="WorkflowType"
                                    Label="Auto-Complete Data View"
                                    Help="A Data View that returns a list of people who should be regarded as having completed this step." />
                                <Rock:DataViewItemPicker ID="dvpAudience"
                                    runat="server"
                                    DataField="WorkflowType"
                                    Label="Audience Data View"
                                    Help="A Data View that returns a list of people who are eligible to take this step." />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbAllowEdit"
                                    runat="server"
                                    SourceTypeName="Rock.Model.StepType, Rock"
                                    PropertyName="AllowManualEdit"
                                    Label="Allow Manual Edit"
                                    Help="Can the details of this step achievement be modified by the participant?"
                                    Checked="false"
                                    Text="Yes" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12">
                                <Rock:CodeEditor ID="ceCardTemplate" runat="server" EditorHeight="200" EditorMode="Lava" EditorTheme="Rock" Label="Card Content Lava Template"
                                    Help="The template to use when formatting the summary card for this step." />
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

        <Rock:ModalAlert ID="mdCopy" runat="server" />

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgAttribute" runat="server" Title="Step Participant Attributes" OnSaveClick="dlgAttribute_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Attributes">
            <Content>
                <Rock:AttributeEditor ID="edtAttributes" runat="server" ShowActions="false" ValidationGroup="Attributes" />
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
