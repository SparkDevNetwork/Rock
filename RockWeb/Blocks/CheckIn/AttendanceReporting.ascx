<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttendanceReporting.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.AttendanceReporting" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-check-square-o"></i>Attendance Analysis</h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlblDateRange" runat="server" LabelType="Info" Text="Date Range" />
                </div>
            </div>
            <div class="panel-body">
                <div class="row row-eq-height-md">
                    <div class="col-md-4 filter-options">

                        <Rock:GroupTypePicker ID="ddlCheckinType" runat="server" Label="Check-in Type" AutoPostBack="true" OnSelectedIndexChanged="ddlCheckinType_SelectedIndexChanged" />
                        <Rock:NotificationBox ID="nbGroupTypeWarning" runat="server" NotificationBoxType="Warning" Text="Please select a group type template in the block settings." Dismissable="false" />

                        <div class="actions margin-b-md">
                            
                        </div>

                        <Rock:SlidingDateRangePicker ID="drpSlidingDateRange" runat="server" Label="Date Range" />

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

                        <Rock:CampusesPicker ID="cpCampuses" runat="server" Label="Campuses" />

                        <h4>Group</h4>
                        <ul class="rocktree">

                            <asp:Repeater ID="rptGroupTypes" runat="server" OnItemDataBound="rptGroupTypes_ItemDataBound">
                                <ItemTemplate>
                                </ItemTemplate>
                            </asp:Repeater>

                        </ul>

                    </div>
                    <div class="col-md-8">

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
                                                        <asp:LinkButton ID="btnGraphByTime" runat="server" CssClass="btn btn-xs btn-default" Text="Schedule" data-val="3" OnClick="btnGraphBy_Click" />
                                                    </div>
                                                </div>
                                            </div>
                                        </Rock:RockControlWrapper>
                                
                                </div>
                            </div>
                            <Rock:LineChart ID="lcAttendance" runat="server" DataSourceUrl="" Title="" Subtitle="" ChartHeight="300" />
                            <div class="row margin-t-sm">
                                <div class="col-md-12">
                                    <div class="pull-right">
                                        <asp:LinkButton ID="lShowChartAttendanceGrid" runat="server" CssClass="btn btn-default btn-xs margin-b-sm" Text="Show Data <i class='fa fa-chevron-down'></i>" ToolTip="Show Data" OnClick="lShowChartAttendanceGrid_Click" />
                                    </div>
                                </div>
                            </div>
                            <asp:Panel ID="pnlChartAttendanceGrid" runat="server" Visible="false">

                                <div class="grid">
                                    <Rock:Grid ID="gChartAttendance" runat="server" AllowSorting="true" DataKeyNames="DateTimeStamp,SeriesId" RowItemText="Attendance Summary">
                                        <Columns>
                                            <Rock:DateField DataField="DateTime" HeaderText="Date" SortExpression="DateTimeStamp" />
                                            <Rock:RockBoundField DataField="SeriesId" HeaderText="Series" SortExpression="SeriesId" />
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
                                            <div class="btn-group">
                                                <asp:HyperLink ID="btnViewAttendees" runat="server" CssClass="btn btn-default active" data-val="0">
                                                    Attendees
                                                </asp:HyperLink>
                                                <asp:HyperLink ID="btnViewParentsOfAttendees" runat="server" CssClass="btn btn-default" data-val="1">
                                                    Parents of Attendees
                                                </asp:HyperLink>
                                            </div>
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
                                                </Rock:RockDropDownList>
                                                <span>visit</span>
                                            </asp:Panel>
                                        </p>
                                        <p>
                                            <Rock:RockRadioButton ID="radByPattern" runat="server" GroupName="grpFilterBy" Text="Pattern" CssClass="js-attendees-by-pattern" />

                                            <asp:Panel ID="pnlByPatternOptions" runat="server" CssClass="js-attendees-by-pattern-options padding-l-lg">
                                                <div class="form-inline">
                                                    <span>Attended at least </span>
                                                    <Rock:NumberBox ID="tbPatternXTimes" runat="server" CssClass="input-width-xs" /><span> times for the selected date range </span>
                                                </div>
                                                <div class="padding-l-lg">
                                                    <div class="form-inline">
                                                        <Rock:RockCheckBox ID="cbPatternAndMissed" runat="server" />and missed at least                                                           
                                                                    <Rock:NumberBox ID="tbPatternMissedXTimes" runat="server" CssClass="input-width-xs" />&nbsp;weeks between
                                                        <Rock:NotificationBox ID="nbMissedDateRangeRequired" runat="server" NotificationBoxType="Warning" Text="Date Range is required" Visible="false" />
                                                        <Rock:DateRangePicker ID="drpPatternDateRange" runat="server" />
                                                    </div>
                                                </div>
                                            </asp:Panel>
                                        </p>
                                    </Rock:RockControlWrapper>

                                    <div class="actions margin-b-md">
                                        <asp:LinkButton ID="btnApplyAttendeesFilter" runat="server" Visible="false" CssClass="btn btn-primary" Text="Apply" ToolTip="Update the Attendees grid" OnClick="btnApplyAttendeesFilter_Click" />
                                    </div>

                                </div>
                            </div>

                            <Rock:NotificationBox ID="nbAttendeesError" runat="server" NotificationBoxType="Danger" Dismissable="true" Visible="false" />
                            <Rock:Grid ID="gAttendeesAttendance" runat="server" AllowSorting="true" RowItemText="Attendee" OnRowDataBound="gAttendeesAttendance_RowDataBound" ExportGridAsWYSIWYG="true">
                                <Columns>
                                    <Rock:SelectField />
                                    <Rock:PersonField DataField="Parent" HeaderText="Parent" />
                                    <Rock:PersonField DataField="Person" HeaderText="Name" SortExpression="PersonAlias.Person.NickName, PersonAlias.Person.LastName" />
                                    <Rock:RockLiteralField HeaderText="First Visit" ID="lFirstVisitDate" />
                                    <Rock:RockLiteralField HeaderText="Second Visit" ID="lSecondVisitDate" />
                                    <Rock:DateField DataField="LastVisit.StartDateTime" HeaderText="Last Visit" SortExpression="LastVisit.StartDateTime" />
                                    <Rock:CampusField DataField="LastVisit.CampusId" HeaderText="Campus" />
                                    <Rock:RockLiteralField HeaderText="Service Time" ID="lServiceTime" />
                                    <Rock:RockBoundField DataField="LastVisit.Group.Name" HeaderText="Check-in Area" SortExpression="LastVisit.Group.Name" />
                                    <Rock:RockLiteralField HeaderText="Home Address" ID="lHomeAddress" ItemStyle-Wrap="false" />
                                    <Rock:PhoneNumbersField HeaderText="Phone Numbers" DataField="PhoneNumbers" ItemStyle-Wrap="false" DisplayCountryCode="false" />
                                    <Rock:RockLiteralField HeaderText="Count" ID="lAttendanceCount" SortExpression="AttendanceSummary.Count" />
                                    <Rock:RockLiteralField HeaderText="Attendance %" ID="lAttendancePercent" />
                                </Columns>
                            </Rock:Grid>

                        </asp:Panel>
                    </div>
                </div>
            </div>
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
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
