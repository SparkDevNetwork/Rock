<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupScheduler.ascx.cs" Inherits="RockWeb.Blocks.GroupScheduling.GroupScheduler" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block panel-groupscheduler">
            <%-- Panel Header --%>
            <div class="panel-heading panel-follow">
                <h1 class="panel-title">
                    <i class="fa fa-calendar-alt"></i>
                    Group Scheduler
                </h1>

                <div class="panel-labels hidden-xs">
                    <button id="btnCopyToClipboard" runat="server" disabled="disabled"
                        data-toggle="tooltip" data-placement="top" data-trigger="hover" data-delay="250" title="Copy Report Link to Clipboard"
                        class="btn btn-link padding-all-none btn-copy-to-clipboard"
                        onclick="$(this).attr('data-original-title', 'Copied').tooltip('show').attr('data-original-title', 'Copy Link to Clipboard');return false;">
                        <i class='fa fa-clipboard'></i>
                    </button>

                    <asp:HiddenField ID="hfDisplayedOccurrenceIds" runat="server" />
                    <asp:LinkButton ID="btnSendNow" runat="server" CssClass="js-sendnow btn btn-default btn-xs" OnClick="btnSendNow_Click">
                        <i class="fa fa-envelope"></i>
                        Send Now
                    </asp:LinkButton>
                    <Rock:ModalAlert ID="maSendNowResults" runat="server" />
                    <asp:LinkButton ID="btnAutoSchedule" runat="server" CssClass="js-autoschedule btn btn-default btn-xs" OnClick="btnAutoSchedule_Click">
                        <i class="fa fa-magic"></i>
                        Auto Schedule
                    </asp:LinkButton>
                </div>
                <div class="rock-fullscreen-toggle js-fullscreen-trigger"></div>
            </div>

            <%-- Panel Body --%>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbNotice" runat="server" />
                <div class="visible-xs-block">
                    <div class="alert alert-warning">
                        This block is not supported on mobile.
                    </div>
                </div>
                <div class="row row-eq-height hidden-xs">
                    <%-- Filter Options --%>
                    <div class="col-lg-2 col-md-3 col-sm-3 col-xs-3 filter-options">
                        <asp:HiddenField ID="hfGroupId" runat="server" />
                        <Rock:GroupPicker ID="gpGroup" runat="server" Label="Group" LimitToSchedulingEnabledGroups="true" OnValueChanged="gpGroup_ValueChanged" />
                        <Rock:RockDropDownList ID="ddlWeek" runat="server" Label="Week" AutoPostBack="true" OnSelectedIndexChanged="ddlWeek_SelectedIndexChanged" />

                        <Rock:NotificationBox ID="nbGroupWarning" runat="server" NotificationBoxType="Warning" />
                        <asp:Panel ID="pnlGroupScheduleLocations" runat="server">
                            <Rock:RockRadioButtonList ID="rblSchedule" runat="server" Label="Schedule" AutoPostBack="true" OnSelectedIndexChanged="rblSchedule_SelectedIndexChanged" />
                            <label class="control-label js-location-label">Locations</label>
                            <Rock:RockCheckBoxList ID="cblGroupLocations" runat="server" AutoPostBack="true" OnSelectedIndexChanged="cblGroupLocations_SelectedIndexChanged" />
                        </asp:Panel>

                        <Rock:RockControlWrapper ID="rcwResourceListSource" runat="server" Label="Source of People">

                            <Rock:ButtonGroup ID="bgResourceListSource" runat="server" CssClass="margin-b-md" SelectedItemClass="btn btn-xs btn-primary" UnselectedItemClass="btn btn-xs btn-default" AutoPostBack="true" OnSelectedIndexChanged="bgResourceListSource_SelectedIndexChanged" />

                            <asp:Panel ID="pnlResourceFilterGroup" runat="server">
                                <Rock:RockRadioButtonList ID="rblGroupMemberFilter" runat="server" AutoPostBack="true" OnSelectedIndexChanged="rblGroupMemberFilter_SelectedIndexChanged" />
                            </asp:Panel>

                            <asp:Panel ID="pnlResourceFilterAlternateGroup" runat="server">
                                <Rock:GroupPicker ID="gpResourceListAlternateGroup" runat="server" Label="Alternate Group" OnValueChanged="gpResourceListAlternateGroup_ValueChanged" />
                            </asp:Panel>

                            <asp:Panel ID="pnlResourceFilterDataView" runat="server">
                                <Rock:DataViewItemPicker ID="dvpResourceListDataView" runat="server" Label="Data View" EntityTypeId="15" OnValueChanged="dvpResourceListDataView_ValueChanged" />
                            </asp:Panel>
                        </Rock:RockControlWrapper>
                    </div>


                    <asp:Panel ID="pnlSchedulerContainer" runat="server" CssClass="col-lg-10 col-md-9">
                        <Rock:NotificationBox ID="nbFilterInstructions" runat="server" CssClass="margin-all-md" NotificationBoxType="Info" Visible="true" Text="Select a group, schedule and at least one location to start scheduling." />

                        <%-- Scheduling: container for the scheduler scheduled containers --%>
                        <asp:Panel ID="pnlScheduler" runat="server" CssClass="resource-area">
                            <div class="row row-eq-height">
                                <div class="col-md-4 hidden-xs">

                                    <div class="group-scheduler-resourcelist">

                                        <Rock:HiddenFieldWithClass ID="hfOccurrenceGroupId" CssClass="js-occurrence-group-id" runat="server" />
                                        <Rock:HiddenFieldWithClass ID="hfOccurrenceSundayDate" CssClass="js-occurrence-sunday-date" runat="server" />
                                        <Rock:HiddenFieldWithClass ID="hfOccurrenceScheduleId" CssClass="js-occurrence-schedule-id" runat="server" />
                                        <Rock:HiddenFieldWithClass ID="hfResourceGroupId" CssClass="js-resource-group-id" runat="server" />
                                        <Rock:HiddenFieldWithClass ID="hfResourceGroupMemberFilterType" CssClass="js-resource-groupmemberfiltertype" runat="server" />
                                        <Rock:HiddenFieldWithClass ID="hfResourceDataViewId" CssClass="js-resource-dataview-id" runat="server" />
                                        <Rock:HiddenFieldWithClass ID="hfResourceAdditionalPersonIds" CssClass="js-resource-additional-person-ids" runat="server" />

                                        <div class="js-unscheduled-resource-template" style="display: none">
                                            <%-- template that groupScheduler.js uses to populate unscheduled resources, data-status will always be "unscheduled" when it is in the list of unscheduled resources --%>

                                            <div class="js-resource resource unselectable" data-status="unscheduled" data-has-scheduling-conflict="false" data-has-requirements-conflict="false" data-has-blackout-conflict="false" data-is-scheduled="" data-person-id="">
                                                <div class="flex">
                                                    <span class="resource-name js-resource-name"></span>
                                                    <div class="resource-meta">

                                                        <div class="js-resource-meta text-right"></div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>

                                        <div class="panel panel-block resource-list">

                                            <div class="panel-heading">
                                                <h1 class="panel-title">
                                                    <i class="fa fa-user"></i>
                                                    People
                                                </h1>

                                                <div class="panel-labels">
                                                    <asp:Panel ID="pnlAddPerson" runat="server" CssClass="btn btn-xs btn-default btn-square js-add-resource" ToolTip="Add Person">
                                                        <i class="fa fa-plus"></i>
                                                    </asp:Panel>
                                                </div>
                                            </div>

                                            <div class="panel-body padding-all-none">

                                                <div class="js-add-resource-picker margin-all-sm" style="display: none">
                                                    <Rock:PersonPicker ID="ppAddPerson" runat="server" Label="Select Person" OnSelectPerson="ppAddPerson_SelectPerson" />
                                                </div>

                                                <Rock:RockTextBox ID="sfResource" runat="server" CssClass="resource-search padding-all-sm js-resource-search" PrependText="<i class='fa fa-search'></i>" Placeholder="Search" spellcheck="false" />

                                                <div class="viewport">
                                                    <%-- loading indicator --%>
                                                    <i class="fa fa-refresh fa-spin margin-l-md js-loading-notification" style="display: none; opacity: .4;"></i>

                                                    <%-- container for list of resources --%>

                                                    <asp:Panel ID="pnlResourceListContainer" CssClass="js-scheduler-source-container resource-container dropzone" runat="server">
                                                    </asp:Panel>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <div class="col-md-8">

                                    <div class="js-scheduled-resource-template" style="display: none">
                                        <%-- template that groupScheduler.js uses to populate scheduled resources, possible data-status values: pending, confirmed, declined --%>

                                        <div class="js-resource resource unselectable" data-status="pending" data-has-scheduling-conflict="false" data-has-requirements-conflict="false" data-has-blackout-conflict="false" data-attendance-id="" data-person-id="">

                                                <div class="flex">
                                                    <span class="resource-name js-resource-name"></span>
                                                    <div class="resource-meta">

                                                        <div class="js-resource-meta text-right"></div>
                                                    </div>
                                                    <div class="dropdown js-resource-actions hide-transit">
                                                        <button class="btn btn-link btn-overflow" type="button" data-toggle="dropdown"><i class="fas fa-ellipsis-h"></i></button>
                                                        <ul class="dropdown-menu">
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
                                                        </ul>
                                                    </div>
                                                </div>

                                        </div>
                                    </div>

                                    <%-- containers for AttendanceOccurrence locations that resources can be dragged into --%>
                                    <div class="locations js-scheduled-occurrences">
                                        <asp:Repeater ID="rptAttendanceOccurrences" runat="server" OnItemDataBound="rptAttendanceOccurrences_ItemDataBound">
                                            <ItemTemplate>

                                                <asp:Panel ID="pnlScheduledOccurrence" runat="server" CssClass="location js-scheduled-occurrence" data-hide-if-empty="0">
                                                    <Rock:HiddenFieldWithClass ID="hfAttendanceOccurrenceId" runat="server" CssClass="js-attendanceoccurrence-id" />
                                                    <Rock:HiddenFieldWithClass ID="hfLocationScheduleMinimumCapacity" runat="server" CssClass="js-minimum-capacity" />
                                                    <Rock:HiddenFieldWithClass ID="hfLocationScheduleDesiredCapacity" runat="server" CssClass="js-desired-capacity" />
                                                    <Rock:HiddenFieldWithClass ID="hfLocationScheduleMaximumCapacity" runat="server" CssClass="js-maximum-capacity" />
                                                    <div class="panel panel-block">
                                                        <div class="panel-heading">
                                                            <h1 class="panel-title">
                                                                <asp:Literal ID="lLocationTitle" runat="server" />
                                                            </h1>
                                                            <asp:Panel ID="pnlStatusLabels" runat="server" CssClass="panel-labels">
                                                                <div class="scheduling-status js-scheduling-status pull-right">
                                                                    <div class="scheduling-status-progress">
                                                                        <div class="progress js-scheduling-progress">
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
                                                                    <div class="js-scheduling-status-light scheduling-status-light" data-status="none">
                                                                    </div>
                                                                </div>

                                                                <div class="autoscheduler-warning js-autoscheduler-warning pull-right margin-r-md" data-original-title="Auto Schedule requires a desired capacity for this location.">
                                                                    <i class="fa fa-exclamation-triangle"></i>
                                                                </div>
                                                            </asp:Panel>
                                                        </div>
                                                        <div class="panel-body">
                                                            <div class="scheduler-target-container js-scheduler-target-container dropzone"></div>
                                                        </div>
                                                    </div>
                                                </asp:Panel>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </div>
                                </div>
                            </div>
                        </asp:Panel>
                    </asp:Panel>
                </div>
            </div>
        </asp:Panel>

        <script>
            Sys.Application.add_load(function () {
                Rock.controls.fullScreen.initialize();
                // toggle the person picker
                $('.js-add-resource').on('click', function () {
                    $('.js-add-resource-picker').slideToggle();
                });

                $('.js-location-label').on('click', function () {
                    // NOTE: this is doing a postback because it gets applied automatically (there isn't a Apply Filter button)
                    window.location = "javascript:__doPostBack('<%=upnlContent.ClientID %>', 'select-all-locations')";
                })

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

                Rock.controls.groupScheduler.initialize({
                    id: schedulerControlId,
                });
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
