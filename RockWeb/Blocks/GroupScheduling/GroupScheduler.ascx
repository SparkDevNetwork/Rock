<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupScheduler.ascx.cs" Inherits="RockWeb.Blocks.GroupScheduling.GroupScheduler" %>

<asp:UpdatePanel ID="upnlContent" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false" class="block-content-main">
    <ContentTemplate>
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block styled-scroll panel-groupscheduler">
            <%-- Panel Header --%>
            <div class="panel-heading panel-follow">
                <h1 class="panel-title">
                    <i class="fa fa-calendar-alt"></i>
                    Group Scheduler
                </h1>

                <div class="panel-labels hidden-xs">
                    <button id="btnHelp" runat="server"
                        class="btn btn-default btn-xs btn-square"
                        onclick="$('#filter-drawer').slideToggle();return false;">
                        <i class='fa fa-question'></i>
                    </button>

                    <button id="btnCopyToClipboard" runat="server" disabled="disabled"
                        data-toggle="tooltip" data-placement="bottom" data-trigger="hover" data-delay="250" title="Copy Report Link to Clipboard"
                        class="btn btn-default btn-xs btn-square btn-copy-to-clipboard"
                        onclick="$(this).attr('data-original-title', 'Copied').tooltip('show').attr('data-original-title', 'Copy Link to Clipboard');return false;">
                        <i class='fa fa-clipboard'></i>
                    </button>

                    <asp:HiddenField ID="hfDisplayedOccurrenceIds" runat="server" />

                    <asp:LinkButton ID="btnSendNowSingleGroupMode" runat="server" CssClass="js-sendnow btn btn-default btn-xs" OnClick="btnSendNowSelectedGroup_Click">
                        <i class="fa fa-envelope"></i>
                        Send Now
                    </asp:LinkButton>

                    <asp:Panel ID="pnlSendNowMultiGroupMode" runat="server" class="btn-group" >
                        <div class="dropdown-toggle btn btn-default btn-xs" data-toggle="dropdown">
                            <i class="fa fa-envelope"></i>
                            Send Now
                        </div>

                        <ul class="dropdown-menu" role="menu">
                            <li>
                                <asp:LinkButton ID="btnSendNowSelectedGroup" runat="server" Text="Send to Selected Group" OnClick="btnSendNowSelectedGroup_Click" />
                            </li>
                            <li>
                                <asp:LinkButton ID="btnSendNowAllGroups" runat="server" Text="Send to All Groups" OnClick="btnSendNowAllGroups_Click"/>
                            </li>
                        </ul>
                    </asp:Panel>
                    <Rock:ModalAlert ID="maSendNowResults" runat="server" />


                    <asp:LinkButton ID="btnAutoScheduleSingleGroupMode" runat="server" CssClass="js-autoschedule btn btn-default btn-xs" OnClick="btnAutoScheduleSelectedGroup_Click">
                        <i class="fa fa-magic"></i>
                        Auto Schedule
                    </asp:LinkButton>

                    <asp:Panel ID="pnlAutoScheduleMultiGroupMode" runat="server" class="btn-group" >
                        <div class="dropdown-toggle btn btn-default btn-xs" data-toggle="dropdown">
                            <i class="fa fa-magic"></i>
                            Auto Schedule
                        </div>

                        <ul class="dropdown-menu dropdown-menu-right" role="menu">
                            <li>
                                <asp:LinkButton ID="btnAutoScheduleSelectedGroup" runat="server" Text="Schedule Selected Group" OnClick="btnAutoScheduleSelectedGroup_Click"/>
                            </li>
                            <li>
                                <asp:LinkButton ID="btnAutoScheduleAllGroups" runat="server" Text="Schedule  All Groups" OnClick="btnAutoScheduleAllGroups_Click"/>
                            </li>
                        </ul>
                    </asp:Panel>
                </div>
                <div class="rock-fullscreen-toggle js-fullscreen-trigger"></div>
            </div>

            <%-- Filter Options (Header) --%>
            <div class="panel-collapsable p-0">
                <div id="filter-drawer" class="panel-drawer" style="display: none;">
                    <div class="p-3">
                        <div>
                        <h5 class="mt-0 mb-4">Group Scheduler Help</h5>
                        <p><strong>Scheduling Basics</strong></p>
                        <p>This screen allows you to schedule individuals into groups. Openings are shown for each group location schedule to meet the configured minimum number of individuals. Additional individuals can be added by dropping them into the ‘Add Individual’ zone.</p>
                        </div>
                        <div class="row mt-4">
                            <div class="col-md-6">
                                <p><strong>Scheduled Individual Legend</strong></p>
                                <p>Scheduled individuals have several states that they can be in. These states are described using an icon to determine how the invite matches their preference. A color describes the status of the invite.</p>
                                <div class="text-center mb-5"><img src="/Assets/Images/group-scheduler/scheduled-legend.svg"></div>
                            </div>

                            <div class="col-md-6">
                                <p><strong>Unscheduled Individuals Legend</strong></p>
                                <p>A person who is not scheduled can also be in various states. Each of these is represented by an icon. Rolling over the individuals will give more details about the state. Yellow indicates a conflict for one or more schedules.</p>
                                <div class="text-left mb-5"><img src="/Assets/Images/group-scheduler/unscheduled-legend.svg"></div>
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <p><strong>Group Location Schedule Status</strong></p>
                                <p>At the top of the group location for each schedule is a status bar. This bar displays quite a bit of information for you.</p>
                                <p>The green bar represents the individuals who have accepted invites while yellow are those still pending. People who have declined will not be represented on this bar.</p>
                                <p>The white line represents the minimum number of individuals you need. The black bar is your desired number.</p>
                            </div>

                            <div class="col-md-6">
                                <p>&nbsp;</p>
                                <div class="text-left mb-3"><img src="/Assets/Images/group-scheduler/progress-example.svg"></div>
                                <p>So in this case enough people have accepted your invite to meet the minimum. If all remaining invites are accepted you would have enough to meet your desired number.</p>
                            </div>
                        </div>
                    </div>

                </div>

                <div class="row row-eq-height no-gutters">
                    <div class="col-lg-3 col-md-4">
                        <%-- Resource List - Filter Options (Header) --%>
                        <div class="panel-toolbar styled-scroll-white h-100 pr-1 resource-filter-options align-items-center">
                            <asp:Panel ID="pnlResourceListFilter" runat="server">
                                <div class="btn-group">
                                    <div class="dropdown-toggle btn btn-xs btn-tool" data-toggle="dropdown">
                                        <i class="fa fa-list-ul"></i>
                                        <%--<asp:HiddenField ID="hfSchedulerResourceListSourceType" runat="server" />--%>
                                        List: <asp:Literal ID="lSelectedResourceTypeDropDownText" runat="server" Text="Group Members" />
                                    </div>

                                    <ul class="dropdown-menu" role="menu">
                                        <asp:Repeater ID="rptSchedulerResourceListSourceType" runat="server" OnItemDataBound="rptSchedulerResourceListSourceType_ItemDataBound">
                                            <ItemTemplate>
                                                <li>
                                                    <asp:LinkButton ID="btnResourceListSourceType" runat="server" Text="-" CommandArgument="-" OnClick="ResourceListSourceType_Change" />
                                                </li>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </ul>
                                </div>
                            </asp:Panel>

                            <asp:Panel ID="pnlAddPerson" runat="server" CssClass="btn btn-xs btn-tool js-add-resource" ToolTip="Add Person">
                                <i class="fa fa-plus"></i>
                            </asp:Panel>
                        </div>
                    </div>
                    <div class="col-lg-9 col-md-8">
                        <%-- AttendanceOccurrences - Filter Options (Header) --%>
                        <!--<div class="group-scheduler-occurrence-filter occurrences-filter-options">-->
                            <asp:HiddenField ID="hfSelectedGroupId" runat="server" />
                            <div class="panel-toolbar">
                                <!-- Filter for Groups/ChildGroups -->
                                <div class="d-flex">
                                    <Rock:GroupPicker ID="gpPickedGroups" runat="server" Label="" AllowMultiSelect="true" OnValueChanged="gpPickedGroups_ValueChanged" CssClass="occurrences-groups-picker" LimitToSchedulingEnabledGroups="true" />
                                    <div>
                                    <asp:LinkButton ID="btnShowChildGroups" runat="server" CssClass="btn btn-xs btn-tool" Text="<i class='fa fa-square'></i> Show Child Groups" AutoPostBack="true" OnClick="btnShowChildGroups_Click" />
                                    </div>
                                </div>

                                <!-- Filter for Week -->
                                <div class="d-block">
                                    <asp:Panel ID="pnlWeekFilter" CssClass="btn-group" runat="server">
                                            <div class="dropdown-toggle btn btn-xs btn-tool" data-toggle="dropdown">
                                                <asp:HiddenField ID="hfWeekSundayDate" runat="server" />
                                                <asp:Literal ID="lWeekFilterText" runat="server" Text="Week: -- " />
                                            </div>

                                            <ul class="dropdown-menu" role="menu">
                                                <asp:Repeater ID="rptWeekSelector" runat="server" OnItemDataBound="rptWeekSelector_ItemDataBound">
                                                    <ItemTemplate>
                                                        <li>
                                                            <asp:LinkButton ID="btnSelectWeek" runat="server" Text="-" CommandArgument="-" OnClick="btnSelectWeek_Click" />
                                                        </li>
                                                    </ItemTemplate>
                                                </asp:Repeater>
                                            </ul>
                                    </asp:Panel>

                                <!-- Filter for Locations -->
                                    <asp:Panel ID="pnlLocationFilter" CssClass="btn-group" runat="server">

                                            <div class="dropdown-toggle btn btn-xs btn-tool" data-toggle="dropdown">
                                                <%--<Rock:HiddenFieldWithClass ID="hfPickedLocationIds" runat="server" CssClass="js-attendance-occurrence-location-ids"/>--%>
                                                <asp:Literal ID="lSelectedLocationFilterText" runat="server" Text="Locations...." />
                                            </div>


                                            <ul class="dropdown-menu" role="menu">
                                                <asp:Repeater ID="rptLocationSelector" runat="server" OnItemDataBound="rptLocationSelector_ItemDataBound">
                                                    <ItemTemplate>
                                                        <li>
                                                            <asp:LinkButton ID="btnSelectLocation" runat="server" Text="-" CommandArgument="-" OnClick="btnSelectLocation_Click" />
                                                        </li>
                                                    </ItemTemplate>
                                                </asp:Repeater>
                                            </ul>
                                    </asp:Panel>

                                <!-- Filter for Schedules -->
                                    <asp:Panel ID="pnlScheduleFilter" CssClass="btn-group" runat="server">
                                            <div class="dropdown-toggle btn btn-xs btn-tool" data-toggle="dropdown">
                                                <asp:HiddenField ID="hfSelectedScheduleId" runat="server" />
                                                <asp:Literal ID="lScheduleFilterText" runat="server" Text="Schedule..." />
                                            </div>

                                            <ul class="dropdown-menu dropdown-menu-right" role="menu">
                                                <asp:Repeater ID="rptScheduleSelector" runat="server" OnItemDataBound="rptScheduleSelector_ItemDataBound">
                                                    <ItemTemplate>
                                                        <li>
                                                            <asp:LinkButton ID="btnSelectSchedule" runat="server" Text="-" CommandArgument="-" OnClick="btnSelectSchedule_Click" />
                                                        </li>
                                                    </ItemTemplate>
                                                </asp:Repeater>
                                            </ul>
                                    </asp:Panel>
                                </div>
                            </div>
                    <!-- </div> -->
                    </div>
                </div>
            </div>

            <%-- Panel Body --%>
            <div class="panel-body-parent">

                <div class="alert alert-warning m-3 visible-xs-block">
                    This block is not supported on mobile.
                </div>



                <Rock:NotificationBox ID="nbFilterMessage" runat="server" CssClass="m-3" />
                <Rock:NotificationBox ID="nbAuthorizedGroupsWarning" runat="server" NotificationBoxType="Warning" Dismissable="true" />
                <asp:Literal ID="lDebug" runat="server" Visible="false" />

                    <asp:Panel ID="pnlSchedulerContainer" runat="server" CssClass="panel-body p-0">

                        <%-- Scheduling: container for the scheduler scheduled containers --%>
                        <asp:Panel ID="pnlScheduler" runat="server" CssClass="row row-eq-height no-gutters">

                                <div class="col-lg-3 col-md-4 hidden-xs">
                                    <div class="d-flex flex-column flex-fill h-100 mh-100 sidebar-border">
                                        <%-- Resource List - Person List --%>

                                        <%-- Resource List - Filter Options (Header Options) --%>
                                        <div class="group-schedule-filter-options clearfix">
                                            <asp:Panel ID="pnlResourceFilterAlternateGroup" runat="server" CssClass="m-2">
                                                <Rock:GroupPicker ID="gpResourceListAlternateGroup" runat="server" Label="Alternate Group" OnValueChanged="gpResourceListAlternateGroup_ValueChanged" />
                                            </asp:Panel>

                                            <asp:Panel ID="pnlResourceFilterDataView" runat="server" CssClass="m-2">
                                                <Rock:DataViewItemPicker ID="dvpResourceListDataView" runat="server" Label="Data View" EntityTypeId="15" OnValueChanged="dvpResourceListDataView_ValueChanged" />
                                            </asp:Panel>
                                        </div>

                                        <asp:Panel ID="pnlSchedulerResourceList" runat="server" CssClass="js-group-scheduler-resourcelist group-scheduler-resourcelist d-flex flex-column flex-fill mh-100">
                                            <div class="hidden">
                                            <Rock:HiddenFieldWithClass ID="hfOccurrenceGroupId" CssClass="js-occurrence-group-id" runat="server" />
                                            <Rock:HiddenFieldWithClass ID="hfOccurrenceSundayDate" CssClass="js-occurrence-sunday-date" runat="server" />
                                            <Rock:HiddenFieldWithClass ID="hfOccurrenceScheduleIds" CssClass="js-occurrence-schedule-ids" runat="server" />
                                            <Rock:HiddenFieldWithClass ID="hfResourceGroupId" CssClass="js-resource-group-id" runat="server" />
                                            <Rock:HiddenFieldWithClass ID="hfResourceGroupMemberFilterType" CssClass="js-resource-groupmemberfiltertype" runat="server" />
                                            <Rock:HiddenFieldWithClass ID="hfSchedulerResourceListSourceType" CssClass="js-resource-scheduler-resource-list-source-type" runat="server" />
                                            <Rock:HiddenFieldWithClass ID="hfResourceDataViewId" CssClass="js-resource-dataview-id" runat="server" />
                                            <Rock:HiddenFieldWithClass ID="hfResourceAdditionalPersonIds" CssClass="js-resource-additional-person-ids" runat="server" />
                                            <Rock:HiddenFieldWithClass ID="hfPickedLocationIds" runat="server" CssClass="js-attendance-occurrence-location-ids"/>
                                            </div>
                                            <div class="js-unscheduled-resource-template" style="display: none">
                                                <%-- template that groupScheduler.js uses to populate unscheduled resources, data-status will always be "unscheduled" when it is in the list of unscheduled resources --%>

                                                <div class="js-resource resource unselectable" data-status="unscheduled" data-has-scheduling-conflict="false" data-has-requirements-conflict="false" data-has-blackout-conflict="false" data-is-scheduled="" data-person-id="" data-placement="bottom">
                                                    <div class="flex">
                                                        <span class="resource-name js-resource-name flex-grow-1"></span>
                                                        <div class="js-resource-name-meta">
                                                            <span class="resource-member-role js-resource-member-role"></span>
                                                        </div>
                                                        <div class="dropdown js-resource-actions hide-dragging">
                                                            <button class="btn btn-link btn-overflow" type="button" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-v"></i></button>
                                                        </div>
                                                    </div>

                                                    <div class="resource-preferences js-resource-preferences hide-dragging small text-muted">
                                                    </div>

                                                    <div class="resource-scheduled js-resource-scheduled hide-dragging small text-muted">
                                                    </div>

                                                    <div class="resource-meta">
                                                        <div class="js-resource-meta text-right"></div>
                                                    </div>
                                                </div>
                                            </div>

                                                <div class="js-add-resource-picker margin-all-sm" style="display: none">
                                                    <Rock:PersonPicker ID="ppAddPerson" runat="server" Label="Select Person" OnSelectPerson="ppAddPerson_SelectPerson" />
                                                </div>

                                                    <Rock:RockTextBox ID="sfResource" runat="server" CssClass="resource-search padding-all-sm js-resource-search" PrependText="<i class='fa fa-search'></i>" Placeholder="Search" spellcheck="false" />


                                            <div class="resource-list d-flex flex-fill">


                                                    <div class="scroll-list">
                                                        <%-- loading indicator --%>
                                                        <i class="fa fa-refresh fa-spin margin-l-md js-loading-notification" style="display: none; opacity: .4;"></i>

                                                        <%-- container for list of resources --%>

                                                        <asp:Panel ID="pnlResourceListContainer" CssClass="js-scheduler-source-container resource-container dropzone" runat="server">
                                                        </asp:Panel>
                                                    </div>

                                            </div>
                                        </asp:Panel>
                                    </div>
                                </div>

                                <div class="col-lg-9 col-md-8">
                                    <div class="board-scroll">
                                    <div class="js-scheduled-resource-template" style="display: none">
                                        <%-- template that groupScheduler.js uses to populate scheduled resources
                                             data-status values: pending || confirmed || declined
                                             data-matching-preference-state values: matches-preference || not-matches-preference || no-preference
                                        --%>

                                        <div class="js-resource resource unselectable" data-status="pending||confirmed||declined" data-has-scheduling-conflict="false" data-matches-preference="matches-preference|not-matches-preference|no-preference" data-has-requirements-conflict="false" data-has-blackout-conflict="false" data-attendance-id="" data-person-id="">

                                            <div class="flex">
                                                <span class="resource-name js-resource-name flex-grow-1"></span>
                                                <span class="resource-member-role js-resource-member-role"></span>

                                                <div class="resource-meta">
                                                    <div class="js-resource-meta text-right"></div>
                                                </div>
                                                <div class="dropdown js-resource-actions hide-dragging">
                                                    <button class="btn btn-link btn-overflow" type="button" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-v"></i></button>
                                                    <ul class="dropdown-menu dropdown-menu-right">
                                                        <li>
                                                            <button type="button" class="dropdown-item btn-link js-markconfirmed">Mark Confirmed</button>
                                                        </li>
                                                        <li>
                                                            <button type="button" class="dropdown-item btn-link js-markpending">Mark Pending</button>
                                                        </li>
                                                        <li>
                                                            <button type="button" class="dropdown-item btn-link js-markdeclined">Mark Declined</button>
                                                        </li>
                                                        <li>
                                                            <button type="button" class="dropdown-item btn-link js-resendconfirmation">Resend Confirmation</button>
                                                        </li>
                                                        <li>
                                                            <button type="button" class="dropdown-item btn-link js-update-preference">Update Preference</button>
                                                        </li>
                                                        <li>
                                                            <button type="button" class="dropdown-item dropdown-item-danger btn-link js-remove">Remove</button>
                                                        </li>
                                                    </ul>
                                                </div>
                                            </div>

                                        </div>
                                    </div>



                                    <%-- containers for AttendanceOccurrence locations that resources can be dragged into
                                        The occurrences are a Repeater within a Repeater
                                            Repeater 1 (rptOccurrenceColumns) - The Columns, which are either Groups or Schedule/Day depending on MultiGroup/SingleGroup mode
                                            Repeater 2 (rptAttendanceOccurrences) - The Occurrences for each column from Repeater 1

                                    --%>
                                    <asp:Panel ID="pnlSchedulerLocations" runat="server" CssClass="d-flex flex-row w-100 h-100 locations js-scheduled-occurrences">
                                        <asp:Repeater ID="rptOccurrenceColumns" runat="server" OnItemDataBound="rptOccurrenceColumns_ItemDataBound">
                                            <ItemTemplate>
                                                <%-- pnlOccurrenceColumn

                                                    can have the following classes:
                                                    - occurrence-column, js-occurrence-column (always)
                                                    - occurrence-column-selected (if in multi-group mode, and this is the selected group)
                                                    - occurrence-column-group (if in multi-group mode)
                                                    - occurrence-column-schedule (if in single-group mode, where each column is a schedule/day)

                                                    and these data attributes:
                                                      - data-is-scheduler-target-column
                                                        - If in Multi-Group mode, only one group can be scheduled at a time, so only one column will have this set to true
                                                        - If in Single-Group mode, all columns will have this set to true
                                                --%>
                                                <asp:Panel ID="pnlOccurrenceColumn" runat="server" CssClass="board-column occurrence-column js-occurrence-column" data-is-scheduler-target-column="false" >
                                                    <%-- Occurrence Column Heading when in Multi-Group mode (show Group name with Checkbox --%>
                                                    <asp:Panel ID="pnlMultiGroupModeColumnHeading" runat="server" CssClass="board-heading mt-3">
                                                        <div class="d-flex justify-content-between">
                                                            <span class="board-column-title flex-fill text-wrap"><asp:Literal ID="lMultiGroupModeColumnGroupNameHtml" runat="server" /></span>
                                                            <asp:LinkButton ID="btnMultiGroupModeColumnSelectedGroup" runat="server"
                                                            CssClass="text-color p-0"
                                                            Text="fa fa-check-square"
                                                            AutoPostBack="true"
                                                            OnClick="btnMultiGroupModeColumnSelectedGroup_Click"/>
                                                        </div>

                                                        <div class="board-heading-pill mt-2 mb-3" style="background:#C8C8C8"></div>
                                                    </asp:Panel>

                                                    <%-- Occurrence Column Heading when in Single-Group mode (show schedule information) --%>
                                                    <asp:Panel ID="pnlSingleGroupModeColumnHeading" runat="server" CssClass="board-heading mt-3">
                                                        <div class="d-flex justify-content-between">
                                                            <span class="board-column-title"><asp:Literal ID="lSingleGroupModeColumnHeadingOccurrenceDate" runat="server" /></span>
                                                            <span class="board-column-schedule-name" data-toggle="tooltip" data-placement="bottom" title="<%# Eval("Schedule.AbbreviatedName") %>"><asp:Literal ID="lSingleGroupModeColumnHeadingOccurrenceScheduleName" runat="server" /></span>
                                                        </div>
                                                        <div class="board-heading-pill mt-2 mb-3" style="background:#C8C8C8"></div>
                                                    </asp:Panel>
                                                    <div class="board-cards">
                                                        <asp:Panel ID="pnlGroupHasSchedulingDisabled" runat="server">
                                                            <span class="group-scheduling-disabled-label d-block small text-center text-muted">Scheduling Not Enabled</span>
                                                        </asp:Panel>
                                                    <asp:Repeater ID="rptAttendanceOccurrences" runat="server" OnItemDataBound="rptAttendanceOccurrences_ItemDataBound">
                                                        <ItemTemplate>

                                                            <asp:Panel ID="pnlScheduledOccurrence" runat="server" CssClass="panel panel-block occurrence location js-scheduled-occurrence" data-hide-if-empty="0">

                                                                    <div class="panel-heading">
                                                                            <%-- Occurrence Panel Heading when in Multi-Group mode --%>
                                                                            <asp:Panel ID="pnlMultiGroupModePanelHeading" runat="server" CssClass="d-flex justify-content-between align-items-center w-100">
                                                                                <div class="d-flex flex-column">
                                                                                    <asp:Literal ID="lMultiGroupModeLocationTitle" runat="server" />
                                                                                    <span class="date small text-nowrap text-muted"><asp:Literal runat="server" ID="lMultiGroupModeOccurrenceScheduledDate" /></span>
                                                                                </div>

                                                                                <div class="d-flex text-nowrap overflow-hidden ml-2">
                                                                                    <span class="board-column-schedule-name text-truncate" data-toggle="tooltip" data-placement="bottom" title="<%# Eval("Schedule.AbbreviatedName") %>"><asp:Literal runat="server" ID="lMultiGroupModeOccurrenceScheduleName" /></span>

                                                                                    <span class="autoscheduler-warning ml-1 js-autoscheduler-warning" data-placement="bottom" data-original-title="Auto Schedule requires a desired capacity for this location.">
                                                                                        <i class="fa fa-exclamation-triangle"></i>
                                                                                    </span>
                                                                                </div>
                                                                            </asp:Panel>

                                                                            <%-- Occurrence Panel Heading when in Single-Group mode --%>
                                                                            <asp:Panel ID="pnlSingleGroupModePanelHeading" runat="server">
                                                                                <asp:Literal ID="lSingleGroupModeLocationTitle" runat="server" />

                                                                                <span class="autoscheduler-warning js-autoscheduler-warning" data-placement="bottom" data-original-title="Auto Schedule requires a desired capacity for this location.">
                                                                                        <i class="fa fa-exclamation-triangle"></i>
                                                                                    </span>
                                                                            </asp:Panel>
                                                                    </div>

                                                                    <asp:Panel ID="pnlStatusLabels" runat="server" CssClass="panel-labels">
                                                                        <div class="scheduling-status js-scheduling-status" data-placement="bottom">
                                                                            <div class="scheduling-status-progress">
                                                                                <div class="progress rounded-0 js-scheduling-progress">
                                                                                    <div class="progress-bar scheduling-progress-confirmed js-scheduling-progress-confirmed" style="width: 0%">
                                                                                        <span class="sr-only"><span class="js-progress-text-percent"></span>% Complete (confirmed)</span>
                                                                                    </div>
                                                                                    <div class="progress-bar scheduling-progress-pending js-scheduling-progress-pending" style="width: 0%">
                                                                                        <span class="sr-only"><span class="js-progress-text-percent"></span>% Complete (pending)</span>
                                                                                    </div>
                                                                                    <div class="minimum-indicator js-minimum-indicator" data-minimum-value="0" style="margin-left: 0%">
                                                                                    </div>
                                                                                    <div class="desired-indicator js-desired-indicator" data-desired-value="0" style="margin-left: 0%">
                                                                                    </div>
                                                                                </div>
                                                                            </div>
                                                                        </div>


                                                                    </asp:Panel>

                                                                    <div class="panel-body p-0">
                                                                        <div class="alert alert-danger js-alert js-scheduler-schedule-person-error margin-all-md" style="display: none">
                                                                            <button type="button" class="close js-hide-alert" aria-hidden="true"><i class="fa fa-times"></i></button>
                                                                            <span class="js-scheduler-schedule-person-error-text"></span>
                                                                        </div>

                                                                        <div class="scheduler-target-container js-scheduler-target-container dropzone"></div>
                                                                    </div>
                                                            </asp:Panel>
                                                        </ItemTemplate>
                                                    </asp:Repeater>
                                                    </div>
                                                </asp:Panel>
                                            </ItemTemplate>

                                        </asp:Repeater>


                                    </asp:Panel>
                                    </div>
                                </div>
                        </asp:Panel>
                    </asp:Panel>
            </div>
        </asp:Panel>

        <%-- Preferences Modal --%>
        <asp:UpdatePanel ID="upGroupScheduleAssignmentPreference" runat="server">
            <ContentTemplate>
                <Rock:ModalDialog ID="mdGroupScheduleAssignmentPreference" runat="server" OnSaveClick="mdGroupScheduleAssignmentPreference_SaveClick" Title="Update Preference" >
                    <Content>
                        <asp:HiddenField ID="hfGroupScheduleAssignmentGroupMemberId" runat="server" />
                        <asp:HiddenField ID="hfGroupScheduleAssignmentScheduleId" runat="server" />
                        <asp:HiddenField ID="hfGroupScheduleAssignmentLocationId" runat="server" />

                        <Rock:NotificationBox ID="nbGroupScheduleAssignmentScheduleWarning" runat="server" NotificationBoxType="Warning" Visible="false" />
                        <Rock:RockDropDownList ID="ddlGroupMemberScheduleTemplate" runat="server" Label="Schedule" OnSelectedIndexChanged="ddlGroupMemberScheduleTemplate_SelectedIndexChanged" AutoPostBack="true" />
                        <asp:Panel ID="pnlGroupPreferenceAssignment" runat="server" >
                            <Rock:DatePicker ID="dpGroupMemberScheduleTemplateStartDate" runat="server" Label="Starting On" />
                            <Rock:RockRadioButtonList ID="rblGroupScheduleAssignmentUpdateOption" runat="server" RepeatDirection="Horizontal" AutoPostBack="true" OnSelectedIndexChanged="rblGroupScheduleAssignmentUpdateOption_SelectedIndexChanged">
                                <asp:ListItem Text="Replace Preference" Value="UpdatePreference" Selected="true" />
                                <asp:ListItem Text="Add to Preference" Value="AppendToPreference" />
                            </Rock:RockRadioButtonList>
                        </asp:Panel>

                        <Rock:NotificationBox ID="nbGroupScheduleAssignmentUpdatePreferenceInformation" runat="server" NotificationBoxType="Info" Visible="false" CssClass="margin-t-md" />
                    </Content>
                </Rock:ModalDialog>
            </ContentTemplate>
        </asp:UpdatePanel>

        <%-- This will get set back to false if the UpdatePanel gets updated, this will help determine if we really need to initialize the group scheduler --%>
        <input type="hidden" class="js-groupscheduler-initialized" value="false" />

        <script>
            Sys.Application.add_load(function () {
                Rock.controls.fullScreen.initialize('body');

                if ($('.js-groupscheduler-initialized').val() == 'true' ) {
                    return
                }

                Rock.controls.fullScreen.initialize();
                // toggle the person picker
                $('.js-add-resource').on('click', function () {
                    $('.js-add-resource-picker').slideToggle();
                });

                // filter the search list when stuff is typed in the search box
                $('.js-resource-search').on('keyup', function () {
                    var value = $(this).find('input').val().toLowerCase().trim();
                    $(".js-scheduler-source-container .js-resource").filter(function () {
                        if (value == '') {
                            // show everybody
                            $(this).toggle(true);
                        }
                        else {

                            var resourceName = $(this).find('.js-resource-name').text();
                            var resourceNameSplit = resourceName.split(' ');
                            var anyMatch = false;

                            // if the first or lastname starts with the searchstring, show the person
                            $.each(resourceNameSplit, function (nindex) {
                                if (resourceNameSplit[nindex].toLowerCase().indexOf(value) == 0) {
                                    anyMatch = true;
                                }
                            })

                            // if first or last didn't match, see if fullname starts with the search value
                            if (!anyMatch) {
                                if (resourceName.toLowerCase().indexOf(value) == 0) {
                                    anyMatch = true;
                                }
                            }

                            $(this).toggle(anyMatch);
                        }
                    });
                });


                $('.js-autoschedule').on('click', function (e) {
                    // make sure the element that triggered this event isn't disabled
                    if (e.currentTarget && e.currentTarget.disabled) {
                        return false;
                    }

                    e.preventDefault();

                    Rock.dialogs.confirm("Are you sure you want to Auto-Schedule for these locations?", function (result) {
                        if (result) {
                            window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                        }
                    })
                });

                var schedulerControlId = '<%=pnlScheduler.ClientID%>';

                $('.js-groupscheduler-initialized').val('true');

                Rock.controls.groupScheduler.initialize({
                    id: schedulerControlId,
                });

                $('#' + schedulerControlId).on('click', '.js-update-preference',  function (a, b, c) {
                    var $resource = $(this).closest('.js-resource');
                    var attendanceId = $resource.attr('data-attendance-id');
                    var groupMemberId = $resource.attr('data-groupmember-id');
                    var postbackArgument = 'update-preference|attendanceId:' + attendanceId + '|groupMemberId:' + groupMemberId;
                    var postbackJs = "javascript:__doPostBack('<%=upnlContent.ClientID %>', '" + postbackArgument + "')";
                    window.location = postbackJs;
                });
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
