<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupScheduleToolboxV2.ascx.cs" Inherits="RockWeb.Blocks.GroupScheduling.GroupScheduleToolboxV2" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfSelectedPersonId" runat="server" />
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-calendar"></i>
                    <asp:Literal ID="lTitle" runat="server" Text="Schedule Toolbox" />
                </h1>

                <div class="panel-labels">
                </div>
            </div>

            <div class="panel-body">
                <asp:Panel ID="pnlToolbox" CssClass="schedule-toolbox js-navigation-panel" runat="server">
                    <button id="btnCopyToClipboard" runat="server" disabled="disabled"
                            data-toggle="tooltip" data-placement="top" data-trigger="hover" data-delay="250" title="Copies the link to synchronize your schedule with a calendar such as Microsoft Outlook or Google Calendar"
                            class="btn btn-info btn-xs btn-copy-to-clipboard mb-4"
                            onclick="$(this).attr('data-original-title', 'Copied').tooltip('show').attr('data-original-title', 'Copy Link to Clipboard');return false;">
                            <i class="fa fa-calendar-alt"></i> Copy Calendar Link
                    </button>
                    <div class="schedule-toolbox-cards">
                        <asp:Repeater ID="rScheduleCards" runat="server" OnItemDataBound="rScheduleCards_ItemDataBound">
                            <ItemTemplate>
                                <div class="card card-sm card-schedule <%# Eval( "CardCssClass" ) %>">
                                    <div class="card-body d-flex">
                                        <div class="flex-fill">
                                            <span class="card-title">
                                                <asp:Literal ID="lScheduleDate" runat="server" />
                                            </span>
                                            <asp:Literal ID="lScheduleCardDetails" runat="server" />
                                        </div>
                                            <asp:Panel ID="pnlPending" class="schedule-confirm" runat="server">
                                                <asp:LinkButton ID="btnDeclineAttend" runat="server" CssClass="btn btn btn-pill btn-danger" CommandName="AttendanceId" Text="Decline" OnClick="btnDeclineAttend_Click" />
                                                <asp:LinkButton ID="btnConfirmAttend" runat="server" CssClass="btn btn btn-pill btn-primary" CommandName="AttendanceId" Text="Accept" OnClick="btnConfirmAttend_Click" />
                                            </asp:Panel>
                                            <asp:Panel ID="pnlSideMenu" class="d-flex align-items-center flex-nowrap justify-content-end" runat="server">
                                                <Rock:HighlightLabel ID="hlScheduleType" runat="server" />
                                                <div class="btn-group dropdown-right ml-1">
                                                    <button type="button" class="btn btn-link btn-overflow dropdown-toggle" data-toggle="dropdown">
                                                        <i class="fa fa-ellipsis-v"></i>
                                                    </button>
                                                    <ul class="dropdown-menu">
                                                        <li>
                                                            <asp:LinkButton ID="btnCancelConfirmAttend" runat="server" CommandName="AttendanceId" CssClass="text-danger" Text="Cancel Confirmation" OnClick="btnCancelConfirmAttend_Click" />
                                                            <asp:LinkButton ID="btnDeleteScheduleExclusion" runat="server" CommandName="ScheduleExclusionId" CssClass="text-danger" Text="Delete" OnClick="btnDeleteScheduleExclusion_Click" />
                                                        </li>
                                                    </ul>
                                                </div>
                                            </asp:Panel>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                    <div class="schedule-actions">
                        <asp:Literal ID="lActionHeader" runat="server" />
                        <asp:LinkButton ID="btnScheduleUnavailability" runat="server" CssClass="btn btn-lg btn-default btn-block" Text="Schedule Unavailability" OnClick="btnScheduleUnavailability_Click" />
                        <asp:LinkButton ID="btnUpdateSchedulePreferences" runat="server" CssClass="btn btn-lg btn-default btn-block" Text="Update Schedule Preferences" OnClick="btnUpdateSchedulePreferences_Click" />
                        <asp:LinkButton ID="btnSignUp" runat="server" CssClass="btn btn-lg btn-default btn-block" Text="Sign-Up for Additional Times" OnClick="btnSignUp_Click" />
                    </div>
                </asp:Panel>

                <%-- Sign-up --%>
                <asp:Panel ID="pnlSignup" CssClass="schedule-toolbox-signup js-navigation-panel" runat="server">
                    <asp:Literal ID="lSignupMsg" runat="server" />
                    <Rock:DynamicPlaceholder ID="phSignUpSchedules" runat="server" />
                </asp:Panel>
                <%-- Preferences --%>
                <asp:Panel ID="pnlPreferences" CssClass="schedule-toolbox-preferences js-navigation-panel" runat="server">
                    <Rock:NotificationBox ID="nbNoScheduledGroups" runat="server" Visible="false" Text="You are currently not in any scheduled groups." NotificationBoxType="Info" />

                    <%-- Per Group Preferences --%>
                    <asp:Repeater ID="rptGroupPreferences" runat="server" OnItemDataBound="rptGroupPreferences_ItemDataBound">
                        <ItemTemplate>
                            <asp:HiddenField ID="hfPreferencesGroupId" runat="server" />

                            <h3>
                                <asp:Literal runat="server" ID="lGroupPreferencesGroupNameHtml" /></h3>
                            <hr class="margin-t-sm margin-b-sm" />

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockDropDownList ID="ddlSendRemindersDaysOffset" runat="server" Label="Send Reminders" OnSelectedIndexChanged="ddlSendRemindersDaysOffset_SelectedIndexChanged" AutoPostBack="true">
                                        <asp:ListItem Value="" Text="Do not send a reminder"></asp:ListItem>
                                        <asp:ListItem Value="1" Text="1 day before"></asp:ListItem>
                                        <asp:ListItem Value="2" Text="2 days before"></asp:ListItem>
                                        <asp:ListItem Value="3" Text="3 days before"></asp:ListItem>
                                        <asp:ListItem Value="4" Text="4 days before"></asp:ListItem>
                                        <asp:ListItem Value="5" Text="5 days before"></asp:ListItem>
                                        <asp:ListItem Value="6" Text="6 days before"></asp:ListItem>
                                        <asp:ListItem Value="7" Text="7 days before"></asp:ListItem>
                                        <asp:ListItem Value="8" Text="8 days before"></asp:ListItem>
                                        <asp:ListItem Value="9" Text="9 days before"></asp:ListItem>
                                        <asp:ListItem Value="10" Text="10 days before"></asp:ListItem>
                                        <asp:ListItem Value="11" Text="11 days before"></asp:ListItem>
                                        <asp:ListItem Value="12" Text="12 days before"></asp:ListItem>
                                        <asp:ListItem Value="13" Text="13 days before"></asp:ListItem>
                                        <asp:ListItem Value="14" Text="14 days before"></asp:ListItem>
                                    </Rock:RockDropDownList>

                                </div>
                                <div class="col-md-6">
                                </div>

                            </div>

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockDropDownList ID="ddlGroupMemberScheduleTemplate" runat="server" Label="Current Schedule" OnSelectedIndexChanged="ddlGroupMemberScheduleTemplate_SelectedIndexChanged" AutoPostBack="true" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:DatePicker ID="dpGroupMemberScheduleTemplateStartDate" runat="server" Label="Starting On" OnValueChanged="dpGroupMemberScheduleTemplateStartDate_ValueChanged" />
                                </div>
                            </div>

                            <asp:Panel ID="pnlGroupPreferenceAssignment" runat="server" Visible="false">

                                <span class="control-label">
                                    <asp:Literal runat="server" ID="lGroupPreferenceAssignmentLabel" Text="Assignment" />
                                </span>
                                <p>
                                    <asp:Literal runat="server" ID="lGroupPreferenceAssignmentHelp" Text="Please select a time and optional location that you would like to be scheduled for." />
                                </p>

                                <%-- NOTE: This gGroupPreferenceAssignments (and these other controls in the ItemTemplate) is in a repeater and is configured in rptGroupPreferences_ItemDataBound--%>
                                <Rock:Grid ID="gGroupPreferenceAssignments" runat="server" DisplayType="Light" OnRowDataBound="gGroupPreferenceAssignments_RowDataBound" RowItemText="Group Preference Assignment" AllowPaging="false">
                                    <Columns>
                                         <Rock:RockLiteralField ID="lScheduleName" HeaderText="Schedule" />
                                        <Rock:RockLiteralField ID="lLocationName" HeaderText="Location" />
                                        <Rock:LinkButtonField ID="btnEditGroupPreferenceAssignment" CssClass="btn btn-default btn-sm" Text="<i class='fa fa-pencil'></i>" OnClick="btnEditGroupPreferenceAssignment_Click" />
                                        <Rock:DeleteField OnClick="btnDeleteGroupPreferenceAssignment_Click" />
                                    </Columns>
                                </Rock:Grid>
                                <br />
                            </asp:Panel>
                        </ItemTemplate>
                    </asp:Repeater>
                </asp:Panel>
                <%-- Unavailability Schedule --%>
                <asp:Panel ID="pnlUnavailabilitySchedule" CssClass="schedule-toolbox-unavailability js-navigation-panel" runat="server">
                    <asp:ValidationSummary ID="valSummaryAddBlackoutDates" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="UnavailabilitySchedule" />
                    <Rock:DateRangePicker ID="drpUnavailabilityDateRange" runat="server" Label="Date Range" ValidationGroup="UnavailabilitySchedule" Required="true" RequiredErrorMessage="Date Range is required" />
                    <Rock:RockTextBox ID="tbUnavailabilityDateDescription" runat="server" Label="Description" MaxLength="100" Help="A short description of why you'll be unavailable" ValidationGroup="UnavailabilitySchedule" />
                    <Rock:RockDropDownList ID="ddlUnavailabilityGroups" runat="server" Label="Group" ValidationGroup="UnavailabilitySchedule" />
                    <Rock:RockCheckBoxList ID="cblUnavailabilityPersons" runat="server" RepeatDirection="Vertical" RepeatColumns="1" Label="Individual" ValidationGroup="UnavailabilitySchedule" Required="true" RequiredErrorMessage="At least one person must be selected" />
                    <div class="actions">
                        <asp:LinkButton ID="btnUnavailabilityScheduleSave" runat="server" AccessKey="s" ToolTip="Alt+s"  Text="Save" CssClass="btn btn-primary" ValidationGroup="UnavailabilitySchedule" OnClick="btnUnavailabilityScheduleSave_Click" />
                        <asp:LinkButton ID="btnUnavailabilityScheduleCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnUnavailabilityScheduleCancel_Click" />
                    </div>
                </asp:Panel>
                <%-- Preferences Add/Edit GroupScheduleAssignment modal --%>
                <Rock:ModalDialog ID="mdGroupScheduleAssignment" runat="server" OnSaveClick="mdGroupScheduleAssignment_SaveClick" Title="Add/Edit Assignment" >
                    <Content>
                        <asp:HiddenField ID="hfGroupScheduleAssignmentGroupId" runat="server" />
                        <asp:HiddenField ID="hfGroupScheduleAssignmentId" runat="server" />
                        <Rock:RockDropDownList ID="ddlGroupScheduleAssignmentSchedule" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlGroupScheduleAssignmentSchedule_SelectedIndexChanged" Label="Schedule" Required="true" />
                        <Rock:RockDropDownList ID="ddlGroupScheduleAssignmentLocation" runat="server" Label="Location" />
                    </Content>
                </Rock:ModalDialog>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>