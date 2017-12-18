<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MetricDetail.ascx.cs" Inherits="RockWeb.Blocks.Reporting.MetricDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
            <asp:HiddenField ID="hfMetricId" runat="server" />
            <asp:HiddenField ID="hfMetricCategoryId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-signal"></i>
                    <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlType" runat="server" LabelType="Type" />
                    <Rock:HighlightLabel ID="hlScheduleFriendlyText" runat="server" />
                    <Rock:HighlightLabel ID="ltLastRunDateTime" runat="server" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" CssClass="margin-t-md" />
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <div id="pnlEditDetails" runat="server">

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbTitle" runat="server" SourceTypeName="Rock.Model.Metric, Rock" PropertyName="Title" />
                            <Rock:DataTextBox ID="tbSubtitle" runat="server" SourceTypeName="Rock.Model.Metric, Rock" PropertyName="Subtitle" />
                        </div>
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.Metric, Rock" PropertyName="IconCssClass" Label="Icon CSS Class" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.Metric, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:PersonPicker ID="ppMetricChampionPerson" runat="server" Label="Metric Champion" Help="Person responsible for overseeing the metric and meeting the goals established." />
                            <Rock:CategoryPicker ID="cpMetricCategories" runat="server" AllowMultiSelect="true" Label="Categories" />
                        </div>
                        <div class="col-md-6">
                            <Rock:PersonPicker ID="ppAdminPerson" runat="server" Label="Administrator" Help="Person responsible for entering the metric values." />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbYAxisLabel" runat="server" Label="Units Label" Help="The label that will be used for the Y Axis when displayed as a chart" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbIsCumulative" runat="server" Label="Cumulative" Help="Helps to calculate year to date metrics." />
                            <Rock:RockCheckBox ID="cbEnableAnalytics" runat="server" Label="Enable Analytics" Help="If this is enabled, a SQL View named 'AnalyticsFactMetric{{Metric.Name}}' will be made available that can be used by Analytic tools, such as Power BI" />
                        </div>
                    </div>

                    <div class="well">
                        <div class="row">
                            <div class="col-md-12">
                                <Rock:RockDropDownList ID="ddlSourceType" runat="server" Label="Source Type" AutoPostBack="true" OnSelectedIndexChanged="ddlSourceType_SelectedIndexChanged" />

                                    <asp:Panel ID="pnlSQLSourceType" runat="server">
                                        <label>Source SQL</label><a class="help" href="javascript: $('.js-sourcesql-help').toggle;"><i class="fa fa-question-circle"></i></a>
                                        <div class="alert alert-info js-sourcesql-help" id="nbSQLHelp" runat="server" style="display: none;"></div>
                                        <Rock:CodeEditor ID="ceSourceSql" runat="server" EditorMode="Sql" />
                                    </asp:Panel>

                                    <asp:Panel ID="pnlLavaSourceType" runat="server">
                                        <label>Source Lava</label><a class="help" href="javascript: $('.js-sourcelava-help').toggle;"><i class="fa fa-question-circle"></i></a>
                                        <div class="alert alert-info js-sourcelava-help" id="nbLavaHelp" runat="server" style="display: none;"></div>
                                        <Rock:CodeEditor ID="ceSourceLava" runat="server" EditorMode="Lava" />
                                    </asp:Panel>
                            
                                    <asp:Panel ID="pnlDataviewSourceType" runat="server">
                                        <Rock:RockDropDownList ID="ddlDataView" runat="server" Label="Source DataView" EnhanceForLongLists="true" />
                                        <Rock:NotificationBox ID="nbDataViewHelp" runat="server" Visible="false" />
                                    </asp:Panel>
                            
                                    <Rock:RockControlWrapper ID="rcwSchedule" runat="server" Label="Schedule" Help="Select the schedule of when the metric values should be calculated.">
                                        <Rock:RockRadioButtonList ID="rblScheduleSelect" runat="server" CssClass="margin-b-sm" OnSelectedIndexChanged="rblScheduleSelect_SelectedIndexChanged" AutoPostBack="true" RepeatDirection="Horizontal" />

                                        <Rock:RockDropDownList ID="ddlSchedule" runat="server" />

                                        <asp:HiddenField ID="hfUniqueScheduleId" runat="server" />
                                        <Rock:ScheduleBuilder ID="sbSchedule" runat="server" ShowDuration="false" ShowScheduleFriendlyTextAsToolTip="true" />
                                    </Rock:RockControlWrapper>
                                </div>
                        </div>
                    </div>

                    <Rock:ModalAlert ID="mdMetricPartitionsGridWarning" runat="server" />

                    <Rock:NotificationBox ID="mdMetricPartitionsEntityTypeWarning" runat="server" NotificationBoxType="Danger" Visible="false" />
                    <Rock:PanelWidget ID="pwMetricPartitions" runat="server" Title="Series Partitions" TitleIconCssClass="fa fa-pause">
                        <Rock:NotificationBox ID="nbMetricValuesWarning" runat="server" NotificationBoxType="Info" CssClass="margin-t-md" />
                        <Rock:Grid ID="gMetricPartitions" runat="server" AllowSorting="false" AllowPaging="false" DisplayType="Light" DataKeyNames="Guid" OnRowSelected="gMetricPartitions_RowSelected" RowItemText="Series Partition">
                            <Columns>
                                <Rock:ReorderField />
                                <Rock:RockBoundField DataField="Label" HeaderText="Label" SortExpression="Label" />
                                <Rock:RockBoundField DataField="EntityTypeName" HeaderText="Type" SortExpression="EntityTypeName" />
                                <Rock:RockBoundField DataField="EntityTypeQualifier" HeaderText="" SortExpression="EntityTypeQualifier" />
                                <Rock:BoolField DataField="IsRequired" HeaderText="Is Required" SortExpression="IsRequired" />
                                <Rock:DeleteField OnClick="gMetricPartitions_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </Rock:PanelWidget>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </div>

                <fieldset id="fieldsetViewDetails" runat="server">
                    <Rock:HighlightLabel ID="lMetricChartSummary" runat="server" Text="Summary of Values" />
                    <Rock:LineChart ID="lcMetricsChart" runat="server" />

                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lblMainDetails" runat="server" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
                        <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-security pull-right" />
                    </div>

                </fieldset>

            </div>

            <!-- Series Partition Detail Modal  -->
            <Rock:ModalDialog ID="mdMetricPartitionDetail" runat="server" Title="Series Partition" ValidationGroup="vg-series-partition" OnSaveClick="mdMetricPartitionDetail_SaveClick">
                <Content>
                    <asp:HiddenField ID="hfMetricPartitionGuid" runat="server" />
                    
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbMetricPartitionLabel" runat="server" Label="Label" Required="true" ValidationGroup="vg-series-partition" />
                            <Rock:EntityTypePicker ID="etpMetricPartitionEntityType" runat="server" Label="Entity Type" AutoPostBack="true" OnSelectedIndexChanged="etpMetricPartitionEntityType_SelectedIndexChanged" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbMetricPartitionIsRequired" runat="server" Label="Is Required" />
                            <Rock:RockControlWrapper ID="rcwAdvanced" runat="server" Label=" ">
                                <Rock:PanelWidget ID="pwMetricPartitionAdvanced" Title="Advanced" runat="server">
                                    <Rock:RockTextBox ID="tbMetricPartitionEntityTypeQualifierColumn" runat="server" Label="Entity Type Qualifier Column" />
                                    <Rock:RockTextBox ID="tbMetricPartitionEntityTypeQualifierValue" runat="server" Label="Entity Type Qualifier Value" />
                                    <Rock:RockDropDownList ID="ddlMetricPartitionDefinedTypePicker" runat="server" Visible="false" Label="Entity Type Qualifier Value" />
                                </Rock:PanelWidget>
                            </Rock:RockControlWrapper>
                        </div>
                    </div>

                </Content>
            </Rock:ModalDialog>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
