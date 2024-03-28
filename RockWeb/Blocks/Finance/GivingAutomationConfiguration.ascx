<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GivingAutomationConfiguration.ascx.cs" Inherits="RockWeb.Blocks.Finance.GivingAutomationConfiguration" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" Visible="false" />
        <asp:Panel ID="pnlDetail" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fas fa-funnel-dollar"></i>
                    Giving Automation Configuration</h1>
            </div>

            <%-- Main Details --%>
            <div class="panel-body">
                <asp:HiddenField ID="hfCampaignConnectionGuid" runat="server" />
                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <%-- General Settings --%>
                <div class="panel panel-section">
                    <div class="panel-heading">
                        <div>
                            <h4 class="panel-title">General Settings</h4>
                            <span class="description">The settings below help to configure the giving automation features within Rock.</span>
                        </div>
                    </div>


                    <div class="panel-body">
                        <div class="row">
                            <div class="col-md-12">
                                <Rock:RockCheckBox ID="cbEnableGivingAutomation" runat="server" Label="Enable Giving Automation" Checked="true" />
                            </div>
                            <div class="col-md-12">
                                <Rock:DaysOfWeekPicker ID="dwpDaysToUpdateClassifications" runat="server" Label="Days to Update Giving Group Classifications" RepeatDirection="Horizontal" />
                            </div>
                            <div class="col-md-12">
                                <Rock:RockCheckBoxList ID="cblTransactionTypes" runat="server" Label="TransactionTypes" RepeatDirection="Horizontal" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockRadioButtonList ID="rblAccountTypes" runat="server" Label="Accounts" RepeatDirection="Horizontal" AutoPostBack="true" OnSelectedIndexChanged="rblAccountTypes_SelectedIndexChanged" />
                            </div>
                            <div id="divAccounts" runat="server" class="col-md-6">
                                <Rock:AccountPicker ID="apGivingAutomationAccounts" runat="server" Label="Selected Accounts" AllowMultiSelect="true" />
                                <Rock:RockCheckBox ID="cbGivingAutomationIncludeChildAccounts" runat="server" DisplayInline="true" Label="Include children of selected accounts" />
                            </div>
                        </div>
                    </div>
                </div>

                <%-- Giving Journey Settings --%>
                <div class="panel panel-section">
                    <div class="panel-heading">
                        <div>
                            <h4 class="panel-title">Giving Journey Settings</h4>
                            <span class="description">Settings to define the journey stage for an individual. The classification process works by looking at the criteria for each stage and selecting the first one that matches.</span>
                        </div>
                    </div>

                    <div class="panel-body">

                        <div class="row">
                            <div class="col-md-12">
                                <Rock:DaysOfWeekPicker ID="dwpDaysToUpdateGivingJourneys" runat="server" Label="Days to Update Giving Journeys" RepeatDirection="Horizontal" />
                            </div>
                        </div>
                        <div class="d-block">
                            <hr />
                            <div class="row d-flex flex-wrap align-items-end align-items-md-center justify-content-center">
                                <div class="col-xs-12 col-md-12 col-lg-2 font-weight-semibold mb-2 mb-md-0">
                                    Former Giver
                                </div>
                                <div class="col-xs-12 flex-sm-eq col-md-3 mb-3 mb-md-0">
                                    <Rock:NumberBox ID="nbFormerGiverNoContributionInTheLastDays" runat="server" Label="No Contribution in the Last" AppendText="days" Required="true" FormGroupCssClass="m-0" />
                                </div>
                                <div class="px-2 mb-3 mb-sm-4 mb-md-0 mt-md-4 text-center">
                                    and
                                </div>
                                <div class="col-xs-12 flex-sm-eq col-md-3 mb-3 mb-md-0">
                                    <Rock:NumberBox ID="nbFormerGiverMedianFrequencyLessThanDays" runat="server" Label="Median Frequency Less Than" AppendText="days" FormGroupCssClass="m-0" />
                                </div>
                                <div class="col-xs-12 col-md-4 text-sm text-muted">
                                    Former Givers are defined as not having a contribution since the number of days provided and having a median frequency less than the number of days provided. Providing no value for Median Frequency would have the effect of not having it be considered.
                                </div>
                            </div>
                            <hr />
                            <div class="row d-flex flex-wrap align-items-end align-items-md-center justify-content-center">
                                <div class="col-xs-12 col-md-12 col-lg-2 font-weight-semibold mb-2 mb-md-0">
                                    Lapsed Giver
                                </div>
                                <div class="col-xs-12 flex-sm-eq col-md-3 mb-3 mb-md-0">
                                    <Rock:NumberBox ID="nbLapsedGiverNoContributionInTheLastDays" runat="server" Label="No Contribution in the Last" AppendText="days" Required="true" FormGroupCssClass="m-0" />
                                </div>
                                <div class="px-2 mb-3 mb-sm-4 mb-md-0 mt-md-4 text-center">
                                    and
                                </div>
                                <div class="col-xs-12 flex-sm-eq col-md-3 mb-3 mb-md-0">
                                    <Rock:NumberBox ID="nbLapsedGiverMedianFrequencyLessThanDays" runat="server" Label="Median Frequency Less Than" AppendText="days" FormGroupCssClass="m-0" />
                                </div>
                                <div class="col-xs-12 col-md-4 text-sm text-muted">
                                    Lapsed Givers are defined as not having contributed since the number of days provided and having a median frequency less than the number of days provided. Providing no value for Median Frequency would have the effect of not having it be considered.
                                </div>
                            </div>
                            <hr />
                            <div class="row d-flex flex-wrap align-items-end align-items-md-center justify-content-center">
                                <div class="col-xs-12 col-md-12 col-lg-2 font-weight-semibold mb-2 mb-md-0">
                                    New Giver
                                </div>
                                <div class="col-xs-12 flex-sm-eq col-shrink-0 col-md-3 mb-3 mb-md-0">
                                    <Rock:NumberRangeEditor ID="nreNewGiverContributionCountBetween" runat="server" CssClass="input-width-sm" Label="Contribution Count Between" Required="true" FormGroupCssClass="mb-0 mt-sm-1" />
                                </div>
                                <div class="px-2 mb-3 mb-sm-4 mb-md-0 mt-md-4 text-center">
                                    and
                                </div>
                                <div class="col-xs-12 flex-sm-eq col-md-3 mb-4 mb-md-0">
                                    <Rock:NumberBox ID="nbNewGiverFirstGiftInLastDays" runat="server" Label="First Gift in the Last" AppendText="days" FormGroupCssClass="mb-0 my-sm-1" />
                                </div>
                                <div class="col-xs-12 col-md-4 text-sm text-muted">
                                    New Givers are defined as having a total contribution count between the values provided. Their first contribution must also be within the number of days configured.
                                </div>
                            </div>
                            <hr />
                            <div class="row d-flex flex-wrap align-items-end align-items-md-center">
                                <div class="col-xs-12 col-md-12 col-lg-2 font-weight-semibold mb-2 mb-md-0">
                                    Occasional Giver
                                </div>
                                <div class="col-md-8 col-lg-6">
                                    <Rock:NumberRangeEditor ID="nreOccasionalGiverMedianFrequencyDays" runat="server" CssClass="input-width-sm" Label="Median Frequency Days" Required="true" FormGroupCssClass="m-0" />
                                </div>
                                <div class="col-xs-12 col-md-4 text-sm text-muted">
                                    Occasional Givers are defined as having a median frequency between the days provided. They must also have at least one gift in that time frame.
                                </div>
                            </div>
                            <hr />
                            <div class="row d-flex flex-wrap align-items-end align-items-md-center">
                                <div class="col-xs-12 col-md-12 col-lg-2 font-weight-semibold mb-2 mb-md-0">
                                    Consistent Giver
                                </div>
                                <div class="col-xs-12 col-sm-7 col-md-3 mb-3 mb-md-0">
                                    <Rock:NumberBox ID="nbConsistentGiverMedianLessThanDays" runat="server" Label="Median Less Than" AppendText="days" Required="true" FormGroupCssClass="m-0" />
                                </div>
                                <div class="col-sm-6 col-md-5 col-lg-3"></div>
                                <div class="col-xs-12 col-md-4 text-sm text-muted">
                                    Consistent Givers are defined as having a median frequency less than the days provided. They must also have at least one gift in that time frame.
                                </div>
                            </div>
                            <hr />
                            <div class="row d-flex flex-wrap align-items-end align-items-md-center justify-content-center">
                                <div class="col-xs-12 col-md-12 col-lg-2 font-weight-semibold mb-2 mb-md-0">
                                    Non-Giver
                                </div>
                                <div class="col-xs-12 col-md-8 col-lg-6">
                                </div>
                                <div class="col-xs-12 col-md-4 text-sm text-muted">
                                    Non-Givers are defined as having never given.
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <%-- Alerts Settings --%>
                <div class="panel panel-section">
                    <div class="panel-heading">
                        <div>
                            <h4 class="panel-title">Giving Alerts</h4>
                            <span class="description">The configuration below will be used to generate alerts. An alert will be triggered for the first matching rule unless that rule is configured to continue matching other rules.</span>
                        </div>
                    </div>
                    <div class="panel-body">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:NumberBox ID="nbGlobalRepeatPreventionDuration" Label="Global Repeat Prevention Duration" runat="server" AppendText="days" CssClass="input-width-md" Help="This will prevent any alert from being triggered within the provided number of days from a previous alert." />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:NumberBox ID="nbGratitudeRepeatPreventionDuration" Label="Gratitude Repeat Prevention Duration" runat="server" AppendText="days" CssClass="input-width-md" Help="This will prevent a gratitude alert from being triggered within the provided number of days from a previous alert." />
                            </div>
                            <div class="col-md-6">
                                <Rock:NumberBox ID="nbFollowupRepeatPreventionDuration" Label="Follow-up Repeat Prevention Duration" runat="server" AppendText="days" CssClass="input-width-md" Help="This will prevent a follow-up alert from being triggered within the provided number of days from a previous alert." />
                            </div>
                        </div>
                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                        <Rock:Grid ID="gAlerts" runat="server" OnRowDataBound="gAlerts_RowDataBound" OnRowSelected="gAlerts_Edit" DisplayType="Light">
                            <Columns>
                                <Rock:ReorderField />
                                <Rock:RockLiteralField ID="lStatusIcons" HeaderText="" HeaderStyle-CssClass="w-1" ItemStyle-CssClass="w-1 badge-legend" />
                                <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                                <Rock:RockBoundField DataField="Campus" HeaderText="Campus" NullDisplayText="All Campus" />
                                <Rock:CurrencyField DataField="MinimumGiftAmount" HeaderText="Min Amount" />
                                <Rock:CurrencyField DataField="MaximumGiftAmount" HeaderText="Max Gift Amount" />
                                <Rock:RockLiteralField ID="lActionsTaken" HeaderText="Action" />
                                <Rock:BoolField DataField="ContinueIfMatched" HeaderText="Continue" />
                                <Rock:DeleteField OnClick="gAlerts_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>
            </div>
        </asp:Panel>

        <%-- Alert Details Modal --%>
        <Rock:ModalDialog ID="mdAlertDetails" runat="server" Title="Alert Details" ValidationGroup="vgAlertDetails">
            <Content>
                <asp:HiddenField ID="hfFinancialTransactionAlertTypeId" runat="server" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.FinancialTransactionAlertType, Rock" PropertyName="Name" Required="true" ValidationGroup="vgAlertDetails" />
                    </div>
                    <div class="col-md-6">
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:CampusPicker ID="cpCampus" runat="server" Label="Person Campus" Help="Optional Campus to filter people by." ValidationGroup="vgAlertDetails" />
                    </div>
                    <div class="col-md-3">
                        <Rock:AccountPicker ID="apAlertAccount" runat="server" Label="Account" Help="Optional account to filter gifts by." />
                    </div>
                    <div class="col-md-3">
                        <div class="margin-t-lg">
                            <Rock:RockCheckBox ID="cbAlertIncludeChildAccounts" runat="server" Text="Include Child Accounts" Help="Checking this option will include all child accounts under all the selected account." />
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockRadioButtonList ID="rblAlertType" runat="server" Label="Alert Type" RepeatDirection="Horizontal" ValidationGroup="vgAlertDetails" AutoPostBack="true" OnSelectedIndexChanged="rblAlertType_SelectedIndexChanged" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbContinueIfMatched" runat="server" Label="Continue If Matched" ValidationGroup="vgAlertDetails" Help="Determines If additional rules should be considered if this rule is matched." />
                    </div>
                    <div class="col-md-6">
                        <Rock:DaysOfWeekPicker ID="dwpDaysToRunAlertType" runat="server" Label="Days to Run" RepeatDirection="Horizontal" />
                    </div>
                    <div class="col-md-6">
                        <Rock:NumberBox ID="nbRepeatPreventionDuration" runat="server" Label="Repeat Prevention Duration" AppendText="days" ValidationGroup="vgAlertDetails" Help="The number of days between triggering the same alert. Blank means that the same trigger can occur on each occurrence if no global settings exist." />
                    </div>
                </div>
                <div class="panel panel-section">
                    <div class="panel-heading">
                        <div>
                            <h4 class="panel-title">Match Criteria</h4>
                            <span class="description">The following criteria will be considered to determine if this alert should be fired.</span>
                        </div>
                    </div>

                    <div class="panel-body">
                        <div class="row">
                            <div class="col-md-5">
                                <Rock:NumberBox ID="nbAmountSensitivityScale" CssClass="input-width-xl" runat="server" NumberType="Double" Label="Amount Sensitivity Scale" ValidationGroup="vgAlertDetails" Help="The number of interquartile ranges below or above the median amount the gift must be to trigger the alert. A recommended value for classification of outliers is 1.5." />
                            </div>
                            <div class="col-md-7 text-sm">
                                <asp:Literal ID="lAmountSensitivityScaleHelp" runat="server" />
                            </div>
                        </div>
                        <hr>
                        <div class="row">
                            <div class="col-md-5">
                                <Rock:NumberBox ID="nbFrequencySensitivityScale" CssClass="input-width-xl" runat="server" NumberType="Double" Label="Frequency Sensitivity Scale" ValidationGroup="vgAlertDetails" Help="The number of standard deviations below or above the mean the gift must be to trigger the alert. A value of 2 would classify the extreme 5% as outliers." />
                            </div>
                            <div class="col-md-7 text-sm">
                                <asp:Literal ID="lFrequencySensitivityScaleHelp" runat="server" />
                            </div>
                        </div>
                        <hr>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:CurrencyBox ID="cbMinimumGiftAmount" runat="server" Label="Minimum Gift Amount" ValidationGroup="vgAlertDetails" Help="The minimum amount the specific gift must be to be considered a match." />
                                <Rock:CurrencyBox ID="cbMinimumMedianGiftAmount" runat="server" Label="Minimum Median Gift Amount" ValidationGroup="vgAlertDetails" Help="The minimum median gift amount for the giver to be considered a match." />
                                <Rock:NumberBox ID="nbMaxDaysSinceLastGift" runat="server" AppendText="days" Label="Maximum Days Since Last Gift" ValidationGroup="vgAlertDetails" Help="The maximum number of days since the last gift." />
                                <Rock:DataViewItemPicker ID="dvpPersonDataView" runat="server" Label="Person Data View" ValidationGroup="vgAlertDetails" Help="This data view will optionally filter the people who will trigger the alerts. The person's Giving Id will be used to filter out the gifts by." />
                            </div>
                            <div class="col-md-6">
                                <Rock:CurrencyBox ID="cbMaximumGiftAmount" runat="server" Label="Maximum Gift Amount" ValidationGroup="vgAlertDetails" Help="The maximum amount the specific gift must be to be considered a match." />
                                <Rock:CurrencyBox ID="cbMaximumMedianGiftAmount" runat="server" Label="Maximum Median Gift Amount" ValidationGroup="vgAlertDetails" Help="The maximum median gift amount for the giver to be considered a match." />
                            </div>
                        </div>
                    </div>
                </div>

                <div class="panel panel-section">
                    <div class="panel-heading">
                        <div>
                            <h4 class="panel-title">Alert Actions</h4>
                            <span class="description">If the criteria above is matched the following actions will be taken.</span>
                        </div>
                    </div>

                    <div class="panel-body">
                        <Rock:WorkflowTypePicker ID="wtpLaunchWorkflow" runat="server" Label="Launch Workflow of Type" ValidationGroup="vgAlertDetails" Help="If matched, the selected workflow will be launched setting the workflow 'Initiator' as the financial transaction's 'authorized' person, and setting the attribute value with key  'FinancialTransactionId' (if it exists) to the financial transaction's Id." />
                        <Rock:RockDropDownList ID="ddlConnectionType" runat="server" Label="Connection Type" AutoPostBack="true" ValidationGroup="vgAlertDetails" OnSelectedIndexChanged="ddlConnectionType_SelectedIndexChanged" CssClass="input-width-xxl" />
                        <Rock:RockDropDownList ID="ddlConnectionOpportunity" runat="server" Label="Connection Opportunity" ValidationGroup="vgAlertDetails" CssClass="input-width-xxl" Help="If matched, will create a new connection request with the giver as the requestor and setting the attribute with the key 'FinancialTransactionId' if it exists. The Connection Request's Campus would be set from the Campus Filter defined above if it is provided. Otherwise the primary campus of the giver would be used in the Connection Request." />
                        <Rock:RockDropDownList ID="ddlDonorSystemCommunication" runat="server" Label="Send Donor Communication From Template" Help="If matched, a new communication will be sent to the person authorizing the gift using the provided communication template." ValidationGroup="vgAlertDetails" />
                        <Rock:RockDropDownList ID="ddlAccountParticipantSystemCommunication" runat="server" Label="Send Account Participant Communication From Template" Help="If matched, a new communication will be sent to those following the Financial Account with the Purpose Key of 'Giving Alerts'. This action does require that an Account Filter be configured above for the alert." ValidationGroup="vgAlertDetails" />
                        <Rock:RockCheckBox ID="cbSendBusEvent" runat="server" Label="Send Bus Event" Help="If matched, will send an event via the Event Bus to notify external systems." ValidationGroup="vgAlertDetails" />
                        <Rock:GroupPicker ID="gpNotificationGroup" runat="server" Label="Alert Summary Notification Group" Help="This group will receive a summary email when an alert of this type is created." ValidationGroup="vgAlertDetails" />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
