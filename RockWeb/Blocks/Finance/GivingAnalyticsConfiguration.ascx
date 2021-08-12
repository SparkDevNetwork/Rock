<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GivingAnalyticsConfiguration.ascx.cs" Inherits="RockWeb.Blocks.Finance.GivingAnalyticsConfiguration" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" Visible="false" />
        <asp:Panel ID="pnlDetail" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fas fa-funnel-dollar"></i>Giving Analytics Configuration</h1>
            </div>
            <div class="panel-body">
                <asp:HiddenField ID="hfCampaignConnectionGuid" runat="server" />
                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <div class="well">
                    <h4>General Setting</h4>
                    <span class="text-muted">The settings below help to configure the giving analytics features within Rock.</span>
                    <hr class="margin-t-sm">
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:RockCheckBox ID="cbEnableGivingAnalytics" runat="server" Label="Enable Giving Analytics" Checked="true" />
                        </div>
                        <div class="col-md-12">
                            <Rock:DaysOfWeekPicker ID="dwpDaysToUpdateAnalytics" runat="server" Label="Days to Update Giving Group Classifications" RepeatDirection="Horizontal" />
                        </div>
                        <div class="col-md-12">
                            <Rock:RockCheckBoxList ID="cblTransactionTypes" runat="server" Label="TransactionTypes" RepeatDirection="Horizontal" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockRadioButtonList ID="rblAccountTypes" runat="server" Label="Accounts" RepeatDirection="Horizontal" AutoPostBack="true" OnSelectedIndexChanged="rblAccountTypes_SelectedIndexChanged" />
                        </div>
                        <div id="divAccounts" runat="server" class="col-md-6">
                            <Rock:AccountPicker ID="apAccounts" runat="server" Label="Selected Accounts" AllowMultiSelect="true" />
                            <Rock:RockCheckBox ID="cbIncludeChildAccounts" runat="server" DisplayInline="true" Label="Include children of selected accounts" />
                        </div>
                    </div>
                </div>

                <div class="well">
                    <h4>Alerts</h4>
                    <span class="text-muted">The configuration below will be used to generate alerts. An alert will be triggered the first matching rule unless that rule is configured to continue matching other rules.</span>
                    <hr class="margin-t-sm">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:NumberBox ID="nbGlobalRepeatPreventionDuration" Label="Global Repeat Prevention Duration" runat="server" AppendText="days" CssClass="input-width-md" Help="This will prevent any alert from being triggered within the provided number of days from a previous alter." />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:NumberBox ID="nbGratitudeRepeatPreventionDuration" Label="Gratitude Repeat Prevention Duration" runat="server" AppendText="days" CssClass="input-width-md" Help="This will prevent a gratitude alert from being triggered within the provided number of days from a previous alter." />
                        </div>
                        <div class="col-md-6">
                            <Rock:NumberBox ID="nbFollowupRepeatPreventionDuration" Label="Follow-up Repeat Prevention Duration" runat="server" AppendText="days" CssClass="input-width-md" Help="This will prevent a follow-up alert from being triggered within the provided number of days from a previous alter." />
                        </div>
                    </div>
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

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>
            </div>
        </asp:Panel>
        <Rock:ModalDialog ID="mdDetails" runat="server" Title="Alert Details" ValidationGroup="vgAlertDetails">
            <Content>
                <asp:HiddenField ID="hfIdValue" runat="server" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.FinancialTransactionAlertType, Rock" PropertyName="Name" Required="true" ValidationGroup="vgAlertDetails" />
                    </div>
                    <div class="col-md-6">
                        <Rock:CampusPicker ID="cpCampus" runat="server" ValidationGroup="vgAlertDetails" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockRadioButtonList ID="rblAlertType" runat="server" Label="Alert Type" RepeatDirection="Horizontal" ValidationGroup="vgAlertDetails" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbContinueIfMatched" runat="server" Label="Continue If Matched" ValidationGroup="vgAlertDetails" Help="Determines If additional rules should be considered if this rule is matched." />
                    </div>
                    <div class="col-md-6">
                        <Rock:DaysOfWeekPicker ID="dwpDaysToRunAlertType" runat="server" Label="Days to Run" RepeatDirection="Horizontal" />
                    </div>
                    <div class="col-md-6">
                        <Rock:NumberBox ID="nbRepeatPreventionDuration" runat="server" Label="Repeat Prevention Duration" AppendText="days" ValidationGroup="vgAlertDetails" Help="The number of days between triggering the same alert. Blank means that the same trigger can occur on each occrrence if not global settings exist." />
                    </div>
                </div>
                <div class="well">
                    <h4>Match Criteria</h4>
                    <span class="text-muted">The following criteria will be considered to determine if this alert should be fired.</span>
                    <hr class="margin-t-sm">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:NumberBox ID="nbAmountSensitivityScale" CssClass="input-width-xl" runat="server" NumberType="Double" Label="Amount Sensitivity Scale" ValidationGroup="vgAlertDetails" Help="The number of interquartile ranges below or above the median amount the gift must be to trigger the alert. A recommended value for classification of outliers is 1.5." />
                        </div>
                        <div class="col-md-6">
                            <p>
                                The amount sensitivity scale determines how many gifts will trigger the alert based on the amount of the gift.
                                This will most often be used to alert for situations when a gift is larger than expected.
                                Positive numbers will trigger alerts for gifts larger than  normal.
                                Negative Values would trigger for gifts smaller than expected (use caution).
                            </p>
                            <p>
                                Typical Values are shown below.
                                <ul>
                                    <li>2 (Aggressive) - This would alert when a gift was within 2 times the interquatile range (IQR) from their median gift amount. For a bi-weekly giver with a median gift of $400 and an IQR of $65, this alert would be generated if a gift of $530 was recieved.</li>
                                    <li>3 (Normal) - This would alert when a gift was within 3 times the interquartile range (IQR) from their median gift amount. For a bi-weekly giver with a median gift of $400 and an IQR of $65, this alert would be generated if a gift of $595 was recieved.</li>
                                </ul>
                            </p>
                            <p>
                                In the event that there is a very consistent giver—every gift is the exact same amount—we use a fallback value.  The fallback amount sensitivity is calculated as 15% of the median gift amount.
                            </p>
                        </div>
                    </div>
                    <hr class="margin-t-sm">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:NumberBox ID="nbFrequencySensitivityScale" CssClass="input-width-xl" runat="server" NumberType="Double" Label="Frequency Sensitivity Scale" ValidationGroup="vgAlertDetails" Help="The number of standard devivations below or above the mean the gift must be to trigger the alert. A value of 2 would classify the extreme 5% as outliers." />
                        </div>
                        <div class="col-md-6">
                            <p>
                                The frequency sensitivity scale determines how many gifts will trigger the alert based on the frequency. This will most often be used to alert for situations when a gift would have been expected but not given.
                                Positive numbers would trigger alerts for gifts that are late.
                                Negative Values would trigger for gifts that are early.
                            </p>
                            <p>
                                Typical Values are shown below.
                                <ul>
                                    <li>2 (Aggressive) - This would alert when a gift was within 2 standard deviations from their mean. For a bi-weekly giver with a mean of 14 days and a standard deviation of 3.8, this alert would be generated if no gift was recieved within 22 days since their last gift.</li>
                                    <li>3 (Normal) - This would alert when a gift was within 3 standard deviations from their mean. For a bi-weekly giver with a mean of 14 days and a standard deviations of 3.8, this alert would be generated if no gift was recieved within 26 days since their last gift.</li>
                                </ul>
                            </p>
                            <p>
                                In the event that there is a very consistent giver—every gift is the same number of days apart—we use a fallback value.  The fallback frequency sensitivity is calculated as 15% of the average days between gifts.   If that value is less than a day, then we again fallback to 3 days.
                            </p>
                        </div>
                    </div>
                    <hr class="margin-t-sm">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:CurrencyBox ID="cbMinimumGiftAmount" runat="server" Label="Minimum Gift Amount" ValidationGroup="vgAlertDetails" Help="The minimum amount the specific gift must be to be considered a match." />
                            <Rock:CurrencyBox ID="cbMinimumMedianGiftAmount" runat="server" Label="Minimum Median Gift Amount" ValidationGroup="vgAlertDetails" Help="The minimum median gift amount for the giver to be considered a match." />
                            <Rock:NumberBox ID="nbMaxDaysSinceLastGift" runat="server" AppendText="days" Label="Maximum Days Since Last Gift" ValidationGroup="vgAlertDetails" Help="The maximum number of days since the last gift." />
                            <Rock:DataViewItemPicker ID="dvpPersonDataView" runat="server" Label="Person Data View" ValidationGroup="vgAlertDetails" Help="Data view to filter if any individual with the giving id of the gift is in the data is in." />
                        </div>
                        <div class="col-md-6">
                            <Rock:CurrencyBox ID="cbMaximumGiftAmount" runat="server" Label="Maximum Gift Amount" ValidationGroup="vgAlertDetails" Help="The maximum amount the specific gift must be to be considered a match." />
                            <Rock:CurrencyBox ID="cbMaximumMedianGiftAmount" runat="server" Label="Maximum Median Gift Amount" ValidationGroup="vgAlertDetails" Help="The maximum median gift amount for the giver to be considered a match." />
                        </div>
                    </div>
                </div>

                <div class="well">
                    <h4>Alert Actions</h4>
                    <span class="text-muted">If the criteria above is matched the following actions will be taken.</span>
                    <hr class="margin-t-sm">
                    <Rock:WorkflowTypePicker ID="wtpLaunchWorkflow" runat="server" Label="Launch Workflow of Type" ValidationGroup="vgAlertDetails" Help="If matched a workflow of the provided type will be launched setting the authorized person as the initiator and setting the attribute with the key of 'FinancialTransactionId' is it exists" />
                    <Rock:RockDropDownList ID="ddlConnectionType" runat="server" Label="Connection Type" AutoPostBack="true" ValidationGroup="vgAlertDetails" OnSelectedIndexChanged="ddlConnectionType_SelectedIndexChanged" CssClass="input-width-xxl" />
                    <Rock:RockDropDownList ID="ddlConnectionOpportunity" runat="server" Label="Connection Opportunity" ValidationGroup="vgAlertDetails" CssClass="input-width-xxl" Help="If matched will create a new connection request with the authorized person as the requestor and setting the attribute with the key 'FinancialTransactionId' if it exists." />
                    <Rock:RockDropDownList ID="ddlSystemCommunication" runat="server" Label="Send Communication From Template" Help="If matched a new communication will be sent to the person authorizing the gift using the provided communication template." ValidationGroup="vgAlertDetails" />
                    <Rock:RockCheckBox ID="cbSendBusEvent" runat="server" Label="Send Bus Event" Help="If matched will send an event via the Event Bus to notify external systems." ValidationGroup="vgAlertDetails" />
                    <Rock:GroupPicker ID="gpNotificationGroup" runat="server" Label="Alert Summary Notification Group" Help="This group will receive a summary email when an alert of this type is created." ValidationGroup="vgAlertDetails" />
                </div>
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
