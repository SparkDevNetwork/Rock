<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupScheduleToolbox.ascx.cs" Inherits="RockWeb.Blocks.GroupScheduling.GroupScheduleToolbox" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfSelectedPersonId" runat="server" />
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-calendar"></i>
                    <asp:Literal ID="lTitle" runat="server" Text="Schedule Toolbox" />
                </h1>

                <div class="panel-labels">
                </div>
            </div>

            <asp:Panel ID="pnlToolbox" CssClass="panel-body" runat="server">

                <div class="margin-b-md">
                    <%--<Rock:ButtonGroup ID="bgTabs" runat="server" SelectedItemClass="btn btn-primary active" UnselectedItemClass="btn btn-default" AutoPostBack="true" OnSelectedIndexChanged="bgTabs_SelectedIndexChanged" />--%>
                    <ul class="nav nav-pills margin-b-md">
                        <asp:Repeater ID="rptTabs" runat="server">
                            <ItemTemplate>
                                <li class='<%# GetTabClass(Container.DataItem) %>'>
                                    <asp:LinkButton ID="lbTab" runat="server" Text='<%# GetTabName(Container.DataItem) %>' CommandArgument="<%# Container.DataItem %>" OnClick="lbTab_Click" CausesValidation="false">
                                    </asp:LinkButton>
                                </li>
                            </ItemTemplate>
                        </asp:Repeater>
                    </ul>
                </div>

                <%-- My Schedule --%>
                <asp:Panel ID="pnlMySchedule" runat="server">
                    <div class="row">
                        <div class="col-xs-12 col-sm-7">

                            <Rock:NotificationBox ID="nbNoUpcomingSchedules" runat="server" Visible="false" CssClass="hidden" Text="No upcoming schedules" NotificationBoxType="Info" />

                            <%-- Pending Confirmations Grid --%>
                            <asp:Panel ID="pnlPendingConfirmations" Style="margin-bottom: 30px;" runat="server" CssClass="pending-confirmations form-group">
                                <span class="control-label">
                                    <h2 id="lPendingConfirmations">Pending Requests</h2>
                                </span>

                                <div class="table-responsive table-no-border">

                                    <table class="grid-table table table-condensed table-borderless">
                                        <tbody>
                                            <asp:Repeater ID="rptPendingConfirmations" runat="server" OnItemDataBound="rptPendingConfirmations_ItemDataBound">
                                                <ItemTemplate>
                                                    <tr>
                                                        <td>
                                                            <asp:Literal ID="lPendingOccurrenceDetails" runat="server" />
                                                        </td>
                                                        <td class="hidden">
                                                            <asp:Literal ID="lPendingOccurrenceTime" runat="server" />

                                                        </td>
                                                        <td>
                                                            <div class="actions">
                                                                <asp:LinkButton ID="btnConfirmAttending" runat="server" CssClass="text-primary" OnClick="btnConfirmAttending_Click"><span class="fa fa-check-circle fa-2x"></span></asp:LinkButton>
                                                            </div>
                                                        </td>
                                                        <td>
                                                            <div class="actions">
                                                                <asp:LinkButton ID="btnDeclineAttending" runat="server" CssClass="text-gray hover-decline" Text="Decline" OnClick="btnDeclineAttending_Click"><span class="fa fa-times-circle fa-2x"></span></asp:LinkButton>
                                                            </div>
                                                        </td>
                                                    </tr>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                        </tbody>
                                    </table>

                                </div>


                                </tbody>
                                </table>
                            </asp:Panel>

                            <%-- Upcoming Schedules Grid --%>
                            <asp:Panel ID="pnlUpcomingSchedules" runat="server" Style="margin-bottom: 30px;" CssClass="confirmed margin-t-md">
                                <div class="row">
                                    <div class="col-xs-12">
                                        <h2 runat="server" id="lUpcomingSchedules">Confirmed Schedule
                                        </h2>
                                    </div>
                                </div>

                                        <div class="table-responsive table-no-border">

                                            <table class="grid-table table table-condensed table-borderless">
                                                <tbody>
                                                    <asp:Repeater ID="rptUpcomingSchedules" runat="server" OnItemDataBound="rptUpcomingSchedules_ItemDataBound">
                                                        <ItemTemplate>
                                                            <tr>
                                                                <td>
                                                                    <asp:Literal ID="lConfirmedOccurrenceDetails" runat="server" />
                                                                </td>
                                                                <td class="hidden">
                                                                    <asp:Literal ID="lConfirmedOccurrenceTime" runat="server" />
                                                                </td>
                                                                <td>
                                                                    <div class="pull-right" style="padding-right: 3px;">
                                                                        <asp:LinkButton ID="btnCancelConfirmAttending" runat="server" CssClass="text-gray hover-decline" OnClick="btnCancelConfirmAttending_Click"><span class="fa fa-times-circle fa-2x"></asp:LinkButton>
                                                                    </div>
                                                                </td>
                                                            </tr>
                                                        </ItemTemplate>
                                                    </asp:Repeater>
                                                </tbody>
                                            </table>
                                        </div>

                            </asp:Panel>

                            <asp:Panel ID="pnlDeclinedSchedules" Style="margin-bottom: 30px;" runat="server" CssClass="form-group">
                                <span class="control-label">
                                    <h2 id="lDeclinedRequests">Declined Requests</h2>
                                </span>

                                <div class="table-responsive table-no-border">

                                    <table class="grid-table table table-condensed table-borderless">
                                        <tbody>
                                            <asp:Repeater ID="rptDeclined" runat="server" OnItemDataBound="rptDeclined_ItemDataBound">
                                                <ItemTemplate>
                                                    <tr>
                                                        <td>
                                                            <asp:Literal ID="lDeclinedOccurrenceDetails" runat="server" />
                                                        </td>
                                                        <td class="hidden">
                                                            <asp:Literal ID="lDeclinedOccurrenceTime" runat="server" />

                                                        </td>
                                                        <td>
                                                            <div class="actions">
                                                                <asp:LinkButton ID="btnConfirmAttending" runat="server" CssClass="text-primary" OnClick="btnConfirmAttending_Click"><span class="fa fa-check-circle fa-2x"></span></asp:LinkButton>
                                                            </div>
                                                        </td>
                                                        <td class="hidden">
                                                            <div class="actions">
                                                                <asp:LinkButton ID="btnDeclineAttending" runat="server" CssClass="text-gray hover-decline" Text="Decline" OnClick="btnDeclineAttending_Click"><span class="fa fa-times-circle fa-2x"></span></asp:LinkButton>
                                                            </div>
                                                        </td>
                                                    </tr>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                        </tbody>
                                    </table>

                                </div>


                                </tbody>
                                </table>
                            </asp:Panel>
                        </div>

                        <%-- <div class="row">
                                <div class="col-xs-12"><hr /></div>
                            </div>--%>

                        <div class="col-xs-12 col-sm-5 text-center">


                                <button id="btnCopyToClipboard" runat="server" disabled="disabled"
                                        data-toggle="tooltip" data-placement="top" data-trigger="hover" data-delay="250" title="Copies the link to synchronize your schedule with a calendar such as Microsoft Outlook or Google Calendar"
                                        class="btn btn-info btn-xs btn-copy-to-clipboard margin-l-md margin-b-sm text-center"
                                        onclick="$(this).attr('data-original-title', 'Copied').tooltip('show').attr('data-original-title', 'Copy Link to Clipboard');return false;">
                                        <i class="fa fa-calendar-alt"></i>Link to Calendar
                                    </button>
                             
                        

                            <div class="well">
                                
                                <h3 class="clearfix">
                                    <asp:Literal runat="server" ID="lBlackoutDates" Text="Blockout Dates" />
                                </h3>
                                <hr class="margin-t-sm margin-b-sm" />
                                <p>
                                    Please provide any dates <%= ( CurrentPersonId == null || CurrentPersonId != SelectedPersonId ? "they" : "you") %> will not be able to attend.
                                </p>

                                <Rock:Grid ID="gBlackoutDates" runat="server" EmptyDataText="No block out dates have been set." DataKeyNames="ExclusionId" ShowHeader="false" DisplayType="Light">
                                    <Columns>
                                        <Rock:RockBoundField DataField="ExclusionId" Visible="false"></Rock:RockBoundField>
                                        <Rock:RockBoundField DataField="PersonAliasId" Visible="false"></Rock:RockBoundField>
                                        <Rock:RockTemplateField>
                                            <ItemTemplate>
                                                <asp:Literal ID="litExclusionDateRange" runat="server" Text='<%# Eval("DateRange")%>'></asp:Literal><span> - </span>
                                                <asp:Literal ID="litExclusionFullName" runat="server" Text='<%# Eval("FullName") %>'></asp:Literal><span> - </span>
                                                <asp:Literal ID="litExclusionGroupName" runat="server" Text='<%# Eval("GroupName") %>'></asp:Literal>
                                            </ItemTemplate>
                                        </Rock:RockTemplateField>
                                        <Rock:DeleteField ID="gBlackoutDatesDelete" runat="server" OnClick="gBlackoutDatesDelete_Click"></Rock:DeleteField>
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </div>
                    </div>
                </asp:Panel>

                <%-- Preferences --%>
                <asp:Panel ID="pnlPreferences" Visible="false" runat="server">
                    <div class="row">
                        <div class="col-xs-12">

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
                                            <Rock:DatePicker ID="dpGroupMemberScheduleTemplateStartDate" runat="server" Label="Starting On" OnValueChanged="dpGroupMemberScheduleTemplateStartDate_ValueChanged" AutoPostBack="true" />
                                        </div>
                                    </div>

                                    <asp:Panel ID="pnlGroupPreferenceAssignment" runat="server" Visible="false">

                                        <span class="control-label">
                                            <asp:Literal runat="server" ID="lGroupPreferenceAssignmentLabel" Text="Assignment" />
                                        </span>
                                        <p>
                                            <asp:Literal runat="server" ID="lGroupPreferenceAssignmentHelp" Text="Please select a time and optional location that you would like to be scheduled for." />
                                        </p>

                                        <asp:Repeater ID="rptGroupPreferenceAssignments" runat="server" OnItemDataBound="rptGroupPreferenceAssignments_ItemDataBound">
                                            <ItemTemplate>
                                                <div class="row js-person-schedule-preferences-row margin-b-sm">
                                                    <asp:HiddenField ID="hfGroupMemberId" runat="server" />
                                                    <asp:HiddenField ID="hfScheduleId" runat="server" />
                                                    <div class="col-md-4">
                                                        <Rock:RockCheckBox ID="cbGroupPreferenceAssignmentScheduleTime" runat="server" AutoPostBack="true" OnCheckedChanged="cbGroupPreferenceAssignmentScheduleTime_CheckedChanged" />
                                                    </div>
                                                    <div class="col-md-8">
                                                        <Rock:RockDropDownList ID="ddlGroupPreferenceAssignmentLocation" runat="server" OnSelectedIndexChanged="ddlGroupPreferenceAssignmentLocation_SelectedIndexChanged" AutoPostBack="true" />
                                                    </div>
                                                </div>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                        <br />
                                    </asp:Panel>
                                </ItemTemplate>
                            </asp:Repeater>


                            <%-- Blackout Dates --%>
                        </div>
                </asp:Panel>

                <%-- Sign-up --%>
                <asp:Panel ID="pnlSignup" CssClass="row" runat="server">
                    <div class="col-md-6">
                        <asp:Literal ID="lSignupMsg" runat="server" />
                        <Rock:DynamicPlaceholder ID="phSignUpSchedules" runat="server" />
                    </div>
                </asp:Panel>

            </asp:Panel>

        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <%-- BlackoutDates Modal  --%>
        <Rock:ModalDialog ID="mdAddBlackoutDates" runat="server" Title="Add Blockout Dates" OnSaveClick="mdAddBlackoutDates_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="AddBlackOutDates">
            <Content>
                <p>
                    <label>Choose the dates, group, and people who will be unavailable</label>
                </p>
                <asp:ValidationSummary ID="valSummaryAddBlackoutDates" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="AddBlackOutDates" />

                <Rock:DateRangePicker ID="drpBlackoutDateRange" runat="server" Label="Date Range" ValidationGroup="AddBlackOutDates" Required="true" RequiredErrorMessage="Date Range is required" />
                <Rock:RockTextBox ID="tbBlackoutDateDescription" runat="server" Label="Description" MaxLength="100" Help="A short description of why you'll be unavailable" />

                <Rock:RockDropDownList ID="ddlBlackoutGroups" Visible="false" runat="server" Label="Group" />
                <Rock:RockCheckBoxList ID="cblBlackoutPersons" runat="server" RepeatDirection="Vertical" RepeatColumns="1" Label="Individual" ValidationGroup="AddBlackOutDates" Required="true" RequiredErrorMessage="At least one person must be selected" />

            </Content>
        </Rock:ModalDialog>

        <script type="text/javascript">
            function clearActiveDialog() {
                $('#<%=hfActiveDialog.ClientID %>').val('');
            }
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
