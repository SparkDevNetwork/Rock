<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttendanceAnalytics.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.AttendanceAnalytics" %>

<style>
    .group-checkboxes .rock-check-box-list label,
    .campuses-picker label {
        cursor: pointer;
    }
</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block panel-analytics">
            <div class="panel-heading panel-follow">
                <h1 class="panel-title">
                    <i class="fa fa-check-square-o"></i> Attendance Analytics<asp:Literal ID="lSpecificGroupName" runat="server" />
                </h1>

                <div class="panel-labels">
                    <asp:Button ID="btnCheckinDetails" runat="server" CssClass="btn btn-default btn-xs" OnClick="btnCheckinDetails_Click" Text="Check-in Detail" />
                    <a href="#" onclick="$('.js-slidingdaterange-help').slideToggle()">
                        <i class='fa fa-question-circle'></i>
                    </a>
                    <button id="btnCopyToClipboard" runat="server" disabled="disabled"
                        data-toggle="tooltip" data-placement="top" data-trigger="hover" data-delay="250" title="Copy Report Link to Clipboard"
                        class="btn btn-link padding-all-none btn-copy-to-clipboard"
                        onclick="$(this).attr('data-original-title', 'Copied').tooltip('show').attr('data-original-title', 'Copy Link to Clipboard');return false;">
                        <i class='fa fa-clipboard'></i>
                    </button>
                </div>
                <div class="rock-fullscreen-toggle js-fullscreen-trigger"></div>
            </div>

            <Rock:NotificationBox ID="nbInvalidGroup" runat="server" NotificationBoxType="Warning" Heading="Sorry" Visible="false"
                Text="<p>You've selected an invalid group id, or you do not have authorization to view attendance for the selected group.</p>" />
            <asp:Panel ID="pnlContent" runat="server">

                <div class="panel-info">
                    <div class="alert alert-info js-slidingdaterange-help margin-v-none" style="display: none">
                        <asp:Literal ID="lSlidingDateRangeHelp" runat="server" />
                    </div>
                </div>

                <div class="panel-body">
                    <div class="row row-eq-height">
                        <div class="col-md-3 filter-options">

                            <Rock:GroupTypePicker ID="ddlAttendanceType" runat="server" Label="Attendance Type" AutoPostBack="true" OnSelectedIndexChanged="ddlCheckinType_SelectedIndexChanged" />
                            <Rock:NotificationBox ID="nbGroupTypeWarning" runat="server" NotificationBoxType="Warning" Text="Please select a group type template in the block settings." Dismissable="false" />

                            <div class="actions margin-b-md">
                            </div>

                            <Rock:NotificationBox ID="nbDateRangeWarning" runat="server" NotificationBoxType="Warning" Text="Date Range is required" Visible="false" Dismissable="true" />
                            <Rock:SlidingDateRangePicker ID="drpSlidingDateRange" runat="server" Label="Sunday Date Range"
                                EnabledSlidingDateRangeTypes="Previous, Last, Current, DateRange" EnabledSlidingDateRangeUnits="Week, Month, Year" />

                            <Rock:RockControlWrapper ID="rcwGroupBy" runat="server" Label="Group By">
                                <div class="controls">
                                    <div class="js-group-by">
                                        <Rock:HiddenFieldWithClass ID="hfGroupBy" CssClass="js-hidden-selected" runat="server" />
                                        <div class="btn-group">
                                            <asp:LinkButton ID="btnGroupByWeek" runat="server" CssClass="btn btn-xs btn-default active" Text="Week" data-val="0" OnClick="btnGroupBy_Click" />
                                            <asp:LinkButton ID="btnGroupByMonth" runat="server" CssClass="btn btn-xs btn-default" Text="Month" data-val="1" OnClick="btnGroupBy_Click" />
                                            <asp:LinkButton ID="btnGroupByYear" runat="server" CssClass="btn btn-xs btn-default" Text="Year" data-val="2" OnClick="btnGroupBy_Click" />
                                        </div>
                                    </div>
                                </div>
                            </Rock:RockControlWrapper>

                            <Rock:SchedulePicker ID="spSchedules" runat="server" Label="Schedules" AllowMultiSelect="true"
                                Help="The schedules to display attendance for. Leave blank to not filter by schedule." />

                            <Rock:RockCheckBoxList ID="clbCampuses" runat="server" FormGroupCssClass="campuses-picker js-campuses-picker" CssClass="campuses-picker-vertical" Label="Campuses"
                                Help="The campuses to display attendance for. Leave blank to not filter by campus." />

                            <Rock:GroupPicker ID="gpGroups" runat="server" Label="Select Group(s)" AllowMultiSelect="true" Visible="false" OnSelectItem="gpGroups_SelectItem" />
                            <Rock:RockControlWrapper ID="rcwSelectedGroups" runat="server" Label="Selected Groups" Visible="false">
                                <ul class="list-unstyled" visible="false" >
                                    <asp:Repeater ID="rptSelectedGroups" runat="server">
                                        <ItemTemplate><li><%# Container.DataItem %></li></ItemTemplate>
                                    </asp:Repeater>
                                </ul>
                            </Rock:RockControlWrapper>

                            <asp:Panel ID="pnlGroups" runat="server" Visible="false" class="js-groups-container">
                                <Rock:NotificationBox ID="nbGroupsWarning" runat="server" NotificationBoxType="Warning" Text="Please select at least one group." Visible="false"/>

                                <div class="grouplist-actions rollover-container" id="divGroupListActions" runat="server">
                                    <Rock:Toggle runat="server" ID="cbShowInactive" CssClass="pull-right" ButtonSizeCssClass="btn-xs" OnCssClass="btn-primary" OffCssClass="btn-primary" OnText="All Groups" OffText="Active Groups" AutoPostBack="true" OnCheckedChanged="cbShowInactive_CheckedChanged" />
                                    <span class="h4 js-checkbox-selector cursor-pointer">Groups</span>
                                    <span class="rollover-item" id="pnlRolloverConfig" runat="server">
                                        <i class="fa fa-gear clickable js-show-config" onclick="$(this).closest('.js-groups-container').find('.js-groups-config-panel').slideToggle()"></i>
                                    </span>
                                </div>

                                <div class="js-groups-config-panel" style="display: none" id="pnlConfigPanel" runat="server">
                                    <Rock:RockCheckBox ID="cbIncludeGroupsWithoutSchedule" runat="server" Text="Include groups that don't have a schedule" OnCheckedChanged="cbIncludeGroupsWithoutSchedule_CheckedChanged" AutoPostBack="true" />
                                </div>

                                <hr class="margin-t-sm" />

                                <ul class="list-unstyled group-checkboxes" >
                                    <asp:Repeater ID="rptGroupTypes" runat="server" OnItemDataBound="rptGroupTypes_ItemDataBound">
                                        <ItemTemplate>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </ul>

                            </asp:Panel>

                            <Rock:DataViewItemPicker ID="dvpDataView" runat="server" Label="Limit by Data View" Visible="false" />

                        </div>
                        <div class="col-md-9">

                            <div class="row analysis-types">
                                <div class="col-sm-8">
                                    <div class="controls">
                                        <div class="js-show-by">
                                            <Rock:HiddenFieldWithClass ID="hfShowBy" CssClass="js-hidden-selected" runat="server" />
                                            <div class="btn-group">
                                                <asp:LinkButton ID="btnShowByChart" runat="server" CssClass="btn btn-default active" data-val="0" OnClick="btnShowByChart_Click">
                                                        <i class="fa fa-line-chart"></i> Chart
                                                </asp:LinkButton>
                                                <asp:LinkButton ID="btnShowByAttendees" runat="server" CssClass="btn btn-default" data-val="1" OnClick="btnShowByAttendees_Click">
                                                        <i class="fa fa-users"></i> Attendees
                                                </asp:LinkButton>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-sm-4">
                                    <div class="actions text-right">
                                        <asp:LinkButton ID="btnApply" runat="server" CssClass="btn btn-primary" ToolTip="Update the chart" OnClick="btnApply_Click"><i class="fa fa-refresh"></i> Update</asp:LinkButton>
                                    </div>
                                </div>
                            </div>

                            <asp:Panel ID="pnlUpdateMessage" runat="server" Visible="true" >
                                <Rock:NotificationBox ID="nbUpdateMessage" runat="server" NotificationBoxType="Default" CssClass="text-center padding-all-lg" Heading="Confirm Settings"
                                    text="<p>Confirm your settings and select the 'Update' button to display your results.</p>" />
                            </asp:Panel>

                            <asp:Panel ID="pnlResults" runat="server" Visible="false">

                                <asp:Panel ID="pnlShowByChart" runat="server">
                                    <div class="clearfix">
                                        <div class="pull-right">

                                                <Rock:RockControlWrapper ID="rcwGraphBy" runat="server" Label="Graph By">
                                                    <div class="controls">
                                                        <div class="js-graph-by">
                                                            <Rock:HiddenFieldWithClass ID="hfGraphBy" CssClass="js-hidden-selected" runat="server" />
                                                            <div class="btn-group">
                                                                <asp:LinkButton ID="btnGraphByTotal" runat="server" CssClass="btn btn-xs btn-default active" Text="Total" data-val="0" OnClick="btnGraphBy_Click" />
                                                                <asp:LinkButton ID="btnGraphByGroup" runat="server" CssClass="btn btn-xs btn-default" Text="Group" data-val="1" OnClick="btnGraphBy_Click" />
                                                                <asp:LinkButton ID="btnGraphByCampus" runat="server" CssClass="btn btn-xs btn-default" Text="Campus" data-val="2" OnClick="btnGraphBy_Click" />
                                                                <asp:LinkButton ID="btnGraphByLocation" runat="server" CssClass="btn btn-xs btn-default" Text="Location" data-val="4" OnClick="btnGraphBy_Click" />
                                                                <asp:LinkButton ID="btnGraphByTime" runat="server" CssClass="btn btn-xs btn-default" Text="Schedule" data-val="3" OnClick="btnGraphBy_Click" />
                                                            </div>
                                                        </div>
                                                    </div>
                                                </Rock:RockControlWrapper>

                                        </div>
                                    </div>
                                    <Rock:LineChart ID="lcAttendance" runat="server" DataSourceUrl="" Title="" Subtitle="" ChartHeight="300" />
                                    <Rock:BarChart ID="bcAttendance" runat="server" DataSourceUrl="" Title="" Subtitle="" ChartHeight="300" />
                                    <div class="row margin-t-sm">
                                        <div class="col-md-12">
                                            <div class="pull-right">
                                                <asp:LinkButton ID="lShowChartAttendanceGrid" runat="server" CssClass="btn btn-default btn-xs margin-b-sm" Text="Show Data <i class='fa fa-chevron-down'></i>" ToolTip="Show Data" OnClick="lShowChartAttendanceGrid_Click" />
                                            </div>
                                        </div>
                                    </div>
                                    <asp:Panel ID="pnlChartAttendanceGrid" runat="server" Visible="false">

                                        <div class="grid">
                                            <Rock:Grid ID="gChartAttendance" runat="server" AllowSorting="true" DataKeyNames="DateTimeStamp,SeriesName" RowItemText="Attendance Summary">
                                                <Columns>
                                                    <Rock:DateField DataField="DateTime" HeaderText="Date" SortExpression="DateTimeStamp" />
                                                    <Rock:RockBoundField DataField="SeriesName" HeaderText="Series" SortExpression="SeriesName" />
                                                    <Rock:RockBoundField DataField="YValue" HeaderText="Count" SortExpression="YValue" />
                                                </Columns>
                                            </Rock:Grid>
                                        </div>
                                    </asp:Panel>
                                </asp:Panel>

                                <asp:Panel ID="pnlShowByAttendees" runat="server">
                                    <div class="panel">
                                        <div class="grid-filter">
                                            <div class="controls pull-right margin-t-sm">
                                                <div class="js-view-by">
                                                    <Rock:HiddenFieldWithClass ID="hfViewBy" CssClass="js-hidden-selected" runat="server" />
                                                    <asp:panel ID="pnlViewBy" runat="server" CssClass="btn-group">
                                                        <asp:HyperLink ID="btnViewAttendees" runat="server" CssClass="btn btn-default btn-sm active" data-val="0">
                                                            Attendees
                                                        </asp:HyperLink>
                                                        <asp:HyperLink ID="btnViewParentsOfAttendees" runat="server" CssClass="btn btn-default btn-sm" data-val="1">
                                                            Parents of Attendees
                                                        </asp:HyperLink>
                                                        <asp:HyperLink ID="btnViewChildrenOfAttendees" runat="server" CssClass="btn btn-default btn-sm" data-val="2">
                                                            Children of Attendees
                                                        </asp:HyperLink>
                                                    </asp:panel>
                                                </div>
                                            </div>
                                            <Rock:RockControlWrapper ID="rcwAttendeesFilter" runat="server" Label="Filter">
                                                <p>
                                                    <Rock:RockRadioButton ID="radAllAttendees" runat="server" GroupName="grpFilterBy" Text="All Attendees" CssClass="js-attendees-all" />
                                                </p>
                                                <p>
                                                    <Rock:RockRadioButton ID="radByVisit" runat="server" GroupName="grpFilterBy" Text="By Visit" CssClass="js-attendees-by-visit" />
                                                    <asp:Panel ID="pnlByVisitOptions" runat="server" CssClass="js-attendees-by-visit-options padding-l-lg form-inline">
                                                        <Rock:RockDropDownList ID="ddlNthVisit" CssClass="input-width-md" runat="server">
                                                            <asp:ListItem />
                                                            <asp:ListItem Text="1st" Value="1" />
                                                            <asp:ListItem Text="2nd" Value="2" />
                                                            <asp:ListItem Text="3rd" Value="3" />
                                                            <asp:ListItem Text="4th" Value="4" />
                                                            <asp:ListItem Text="5th" Value="5" />
                                                            <asp:ListItem Text="No" Value="0" />
                                                        </Rock:RockDropDownList>
                                                        <span>visit</span>
                                                    </asp:Panel>
                                                </p>
                                                <p>
                                                    <Rock:RockRadioButton ID="radByPattern" runat="server" GroupName="grpFilterBy" Text="Pattern" CssClass="js-attendees-by-pattern" />

                                                    <asp:Panel ID="pnlByPatternOptions" runat="server" CssClass="js-attendees-by-pattern-options padding-l-lg">
                                                        <div class="form-inline">
                                                            <span>Attended at least </span>
                                                            <Rock:NumberBox ID="tbPatternXTimes" runat="server" CssClass="input-width-xs" /><asp:Literal ID="lPatternXFor" runat="server" Text=" times for the selected date range" />
                                                        </div>
                                                        <div class="padding-l-lg margin-t-sm">
                                                            <div class="form-inline">
                                                                <Rock:RockCheckBox ID="cbPatternAndMissed" runat="server" />and missed at least
                                                                            <Rock:NumberBox ID="tbPatternMissedXTimes" runat="server" CssClass="input-width-xs" />&nbsp;<asp:Literal ID="lPatternAndMissedXBetween" runat="server" Text=" weeks between" />
                                                                <Rock:NotificationBox ID="nbMissedDateRangeRequired" runat="server" NotificationBoxType="Warning" Text="Date Range is required" Visible="false" />
                                                                <div class="margin-t-sm">
                                                                    <Rock:DateRangePicker ID="drpPatternDateRange" runat="server" />
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </asp:Panel>
                                                </p>
                                            </Rock:RockControlWrapper>

                                        </div>
                                    </div>

                                    <Rock:NotificationBox ID="nbAttendeesError" runat="server" NotificationBoxType="Danger" Dismissable="true" Visible="false" />
                                    <Rock:Grid ID="gAttendeesAttendance" runat="server" AllowSorting="true" RowItemText="Attendee" OnRowDataBound="gAttendeesAttendance_RowDataBound" ExportSource="ColumnOutput" ExportFilename="AttendanceAnalytics">
                                        <Columns>
                                            <Rock:SelectField />
                                            <asp:HyperLinkField DataNavigateUrlFields="ParentId" DataTextField="Parent" HeaderText="Parent" SortExpression="Parent.LastName, Parent.NickName"/>
                                            <Rock:RockBoundField DataField="Parent" HeaderText="Parent" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                            <Rock:RockBoundField DataField="Parent.Email" HeaderText="Parent Email" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                            <Rock:RockBoundField DataField="Parent.GivingId" HeaderText="Parent GivingId" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                            <asp:HyperLinkField DataNavigateUrlFields="ChildId" DataTextField="Child" HeaderText="Child" SortExpression="Child.LastName, Child.NickName"/>
                                            <Rock:RockBoundField DataField="Child" HeaderText="Child" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                            <Rock:RockBoundField DataField="Child.Email" HeaderText="Child Email" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                            <Rock:RockBoundField DataField="Child.Age" HeaderText="Child Age" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                            <Rock:RockBoundField DataField="Child.GivingId" HeaderText="Child GivingId" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                            <asp:HyperLinkField DataNavigateUrlFields="PersonId" DataTextField="Person" HeaderText="Name" SortExpression="Person.LastName, Person.NickName"/>
                                            <Rock:RockBoundField DataField="Person" HeaderText="Person" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                            <Rock:RockBoundField DataField="Person.Email" HeaderText="Email" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                            <Rock:RockBoundField DataField="Person.Gender" HeaderText="Gender" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                            <Rock:RockBoundField DataField="Person.Age" HeaderText="Age" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                            <Rock:RockBoundField DataField="Person.GivingId" HeaderText="GivingId" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                            <Rock:RockBoundField DataField="Person.Birthdate" HeaderText="Birthdate" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                            <Rock:DefinedValueField DataField="Person.ConnectionStatusValueId" HeaderText="Connection Status" SortExpression="Person.ConnectionStatusValueId" />
                                            <Rock:RockLiteralField HeaderText="First Visit" ID="lFirstVisitDate" SortExpression="FirstVisit.StartDateTime"/>
                                            <Rock:RockLiteralField HeaderText="Second Visit" ID="lSecondVisitDate" />
                                            <Rock:DateField DataField="LastVisit.StartDateTime" HeaderText="Last Visit" SortExpression="LastVisit.StartDateTime" />
                                            <Rock:CampusField DataField="LastVisit.CampusId" HeaderText="Campus" SortExpression="LastVisit.Campus.Name" />
                                            <Rock:RockLiteralField HeaderText="Service Time" ID="lServiceTime" />
                                            <Rock:RockBoundField DataField="LastVisit.GroupName" HeaderText="Check-in Area" SortExpression="LastVisit.GroupName" />
                                            <Rock:RockBoundField DataField="LastVisit.LocationName" HeaderText="Location" SortExpression="LastVisit.LocationName" />
                                            <Rock:RockBoundField DataField="LastVisit.InGroup" HeaderText="In Group" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                            <Rock:RockBoundField DataField="LastVisit.RoleName" HeaderText="Group Role" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                            <Rock:RockLiteralField HeaderText="Home Address" ID="lHomeAddress" ItemStyle-Wrap="false" />
                                            <Rock:RockLiteralField HeaderText="Phone Numbers" ID="lPhoneNumbers"  ItemStyle-Wrap="false" />
                                            <Rock:RockLiteralField HeaderText="Count" ID="lAttendanceCount" SortExpression="AttendanceSummary.Count" />
                                            <Rock:RockLiteralField HeaderText="Attendance %" ID="lAttendancePercent" SortExpression="AttendanceSummary.Count" />
                                        </Columns>
                                    </Rock:Grid>

                                </asp:Panel>

                            </asp:Panel>

                        </div>
                    </div>
                </div>

            </asp:Panel>

        </div>

        <script>
            function setActiveButtonGroupButton($activeBtn) {
                $activeBtn.addClass('active');
                $activeBtn.siblings('.btn').removeClass('active');
                $activeBtn.closest('.btn-group').siblings('.js-hidden-selected').val($activeBtn.data('val'));
            }

            function showFilterByOptions() {
                if ($('.js-attendees-all').is(':checked')) {
                    $('.js-attendees-by-visit-options').hide();
                    $('.js-attendees-by-pattern-options').hide();
                } else if ($('.js-attendees-by-visit').is(':checked')) {
                    $('.js-attendees-by-visit-options').show();
                    $('.js-attendees-by-pattern-options').hide();
                } else if ($('.js-attendees-by-pattern').is(':checked')) {
                    $('.js-attendees-by-visit-options').hide();
                    $('.js-attendees-by-pattern-options').show();
                }

            }

            Sys.Application.add_load(function () {
                Rock.controls.fullScreen.initialize();

                // Graph-By button group
                $('.js-graph-by .btn').on('click', function (e) {
                    setActiveButtonGroupButton($(this));
                });

                setActiveButtonGroupButton($('.js-graph-by').find("[data-val='" + $('.js-graph-by .js-hidden-selected').val() + "']"));

                // Group-By button group
                $('.js-group-by .btn').on('click', function (e) {
                    setActiveButtonGroupButton($(this));
                });

                setActiveButtonGroupButton($('.js-group-by').find("[data-val='" + $('.js-group-by .js-hidden-selected').val() + "']"));

                // Show-By button group
                $('.js-show-by .btn').on('click', function (e) {
                    setActiveButtonGroupButton($(this));
                });

                setActiveButtonGroupButton($('.js-show-by').find("[data-val='" + $('.js-show-by .js-hidden-selected').val() + "']"));

                // View-By button group
                $('.js-view-by .btn').on('click', function (e) {
                    setActiveButtonGroupButton($(this));
                });

                setActiveButtonGroupButton($('.js-view-by').find("[data-val='" + $('.js-view-by .js-hidden-selected').val() + "']"));

                // Attendees Filter
                $('.js-attendees-all, .js-attendees-by-visit, .js-attendees-by-pattern').on('click', function (e) {
                    showFilterByOptions();
                });

                showFilterByOptions();

                // toggle all group checkboxes
                $('.js-checkbox-selector, .js-groups-container .rock-check-box-list .control-label').on('click', function (e) {

                    var container = $(this).closest('.js-groups-container');

                    var isChecked = true;
                    container.find('input:checkbox').each(function (a) {
                        if (!$(this).prop('checked')) {
                            isChecked = false;
                        }
                    });

                    container.find('input:checkbox').each(function () {
                        $(this).prop('checked', !isChecked);
                    });

                });

                // toggle campus checkboxes
                $('.js-campuses-picker label').on('click', function (e) {

                    var container = $(this).parent().find('.controls');
                    var isChecked = true;
                    container.find('input:checkbox').each(function (a) {
                        if (!$(this).prop('checked')) {
                            isChecked = false;
                        }
                    });

                    container.find('input:checkbox').each(function () {
                        $(this).prop('checked', !isChecked);
                    });

                });
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
