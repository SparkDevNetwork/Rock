<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttendanceReportingByTimestamp.ascx.cs" Inherits="RockBlocks.Plugins.org_lakepointe.Checkin.AttendanceReportingByTimestamp" %>
<asp:UpdatePanel ID="upReport" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlView" runat="server">
            <asp:Panel ID="pnlFilter" runat="server">
                <div class="panel panel-block margin-t-md">
                    <div class="panel-heading clearfix">
                        <h1 class="panel-title pull-left">
                            <i class="fa fa-users"></i>Attendance By Time
                        </h1>
                    </div>
                    <div class="panel-body">
                        <Rock:NotificationBox ID="nbFiltersError" runat="server" NotificationBoxType="Danger" Visible="false" />
                        <div class="filter-list">
                            <div class="row">
                                <asp:ValidationSummary runat="server" HeaderText="Please correct the following to run report" CssClass="alert alert-validation" ValidationGroup="ReportFilter" />
                            </div>
                            <div class="row filterfield">
                                <div class="col-md-4">
                                    <Rock:GroupTypePicker ID="gtpAttendanceConfiguration" runat="server" Required="True" ValidationGroup="ReportFilter" RequiredErrorMessage="Attendance Configuration Is Required" Label="Attendance Configuration" AutoPostBack="true" OnSelectedIndexChanged="gtpAttendanceConfiguration_SelectedIndexChanged" />
                                </div>
                            </div>
                            
                            <div class="row">
                                <div class="filterfield">
                                    <div class="col-md-4">
                                        <Rock:GroupPicker ID="gpGroups" runat="server" Label="Groups" AllowMultiSelect="true" />
                                    </div>
                                </div>
                                <asp:Panel ID="pnlCampus" runat="server" CssClass="filterfield">
                                    <div class="col-md-4">
                                        <Rock:CampusPicker ID="cpCampus" runat="server" Label="Campus" IncludeInactive="false" />
                                    </div>
                                </asp:Panel>
                                <asp:Panel ID="pnlSchedule" runat="server" CssClass="filterfield">
                                    <div class="col-md-4">
                                        <Rock:SchedulePicker ID="spSchedule" runat="server" Label="Schedule" AllowMultiSelect="false" />
                                    </div>
                                </asp:Panel>
                            </div>

                            <div class="row  filterfield">
                                <div class="col-md-4">
                                    <Rock:DateTimePicker ID="dtpStart" runat="server" Label="Start Time" Required="true" RequiredErrorMessage="Check in Start Time is required" ValidationGroup="ReportFilter" Help="Start Date/Time to Report on" />
                                </div>
                                <div class="col-md-4">
                                    <Rock:DateTimePicker ID="dtpEnd" runat="server" Label="End Time" Help="End Date/Time to Report on." />
                                </div>
                                <div class="col-md-4">
                                    <Rock:RockControlWrapper ID="rcwHardStop" runat="server" Label="Show Rollovers" Help="Identify people who rollover to another class.">
                                        <div class="controls">
                                            <div class="js-hard-stop">
                                                <Rock:HiddenFieldWithClass ID="hfHardStop" runat="server" CssClass="js-hidden-selected" />
                                                <div class="btn-group">
                                                    <asp:LinkButton ID="btnHardStopFlagYes" runat="server" CssClass="btn  btn-default active" CausesValidation="false" data-val="1">Yes</asp:LinkButton>
                                                    <asp:LinkButton ID="btnHardStopFlagNo" runat="server" CssClass="btn btn-default" CausesValidation="false" data-val="0">No</asp:LinkButton>
                                                </div>
                                            </div>
                                        </div>
                                    </Rock:RockControlWrapper>
                                </div>
                            </div>
                        </div>
                        <div class="actions margin-t-md">
                            <asp:LinkButton ID="btnFilter" runat="server" AccessKey="m" ToolTip="Alt+m" Text="Filter" CssClass="btn btn-primary btn-sm" CausesValidation="true" ValidationGroup="ReportFilter" OnClick="btnFilter_Click" />
                            <asp:LinkButton ID="btnFilterSetDefault" runat="server" Text="Reset Filters" ToolTip="Set the filter to its default values" CssClass="btn btn-link btn-sm pull-right" OnClick="btnFilterSetDefault_Click" />

                        </div>
                        <asp:Panel ID="pnlResults" runat="server" Visible="false">
                            <Rock:NotificationBox ID="nbAttendeesError" runat="server" NotificationBoxType="Danger" Dismissable="true" Visible="false" />
                            <Rock:Grid ID="gAttendees" runat="server" AllowSorting="true" RowItemText="Attendee"  ExportSource="ColumnOutput" ExportFilename="Attendees">
                                <Columns>
                                    <Rock:SelectField />
                                    <asp:HyperLinkField DataNavigateUrlFields="PersonId" DataTextField="AttendedPersonFullName" HeaderText="Name" SortExpression="AttendedPersonLastName, AttendedPersonNickName" />
                                    <Rock:RockTemplateField HeaderText="Rollover" ItemStyle-HorizontalAlign="Center" ItemStyle-VerticalAlign="Middle" SortExpression="IsRollover">
                                        <ItemTemplate>
                                            <asp:Label ID="lblRollover" runat="server" Visible='<%# (bool)Eval("IsRollover") %>'><i class="fa fa-chevron-up"></i></asp:Label>
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:RockBoundField HeaderText="Is Rollover" DataField="IsRollover" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                    <Rock:RockBoundField DataField="AttendedPersonFullName" HeaderText="Person" Visible="false" SortExpression="AttendedPersonLastName, AttendedPersonNickName" ExcelExportBehavior="AlwaysInclude" />
                                    <Rock:RockBoundField DataField="AttendedPersonEmail" HeaderText="Email" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                    <Rock:RockBoundField DataField="AttendedPersonGender" HeaderText="Gender" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                    <Rock:RockBoundField DataField="AttendedPersonBirthdate" HeaderText="Birthdate" Visible="true" DataFormatString="{0:d}" ExcelExportBehavior="AlwaysInclude" />
                                    <Rock:RockBoundField DataField="AttendedPersonAge" HeaderText="Age" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                    <Rock:RockBoundField DataField="AttendanceCodeValue" HeaderText="Code" />
                                    <Rock:RockBoundField DataField="CheckInGroupName" HeaderText="Group" SortExpression="CheckInGroupName" />
                                    <Rock:RockBoundField DataField="CheckInLocationName" HeaderText="Location" SortExpression="CheckInLocationName" />
                                    <Rock:RockBoundField DataField="StartDateTime" HeaderText="Check In Time" DataFormatString="{0:g}" SortExpression="StartDateTime" />
                                    <asp:HyperLinkField  DataNavigateUrlFields="CheckedInByPersonId" DataTextField="CheckedInByPersonFullName" HeaderText="Check In By" />
                                    <Rock:RockBoundField DataField="EndDateTime" HeaderText="Check Out Time" DataFormatString="{0:g}" SortExpression="EndDateTimeSortValue" />
                                    <asp:HyperLinkField  DataNavigateUrlFields="CheckedOutByPersonId" DataTextField="CheckedOutByPersonFullName"  HeaderText="Check Out By" />


                                </Columns>
                            </Rock:Grid>

                        </asp:Panel>
                    </div>
                </div>
            </asp:Panel>

        </asp:Panel>

        <script type="text/javascript">
            function setActiveButtonGroupButton($activeBtn) {
                $activeBtn.addClass('active');
                $activeBtn.siblings('.btn').removeClass('active');
                $activeBtn.closest('.btn-group').siblings('.js-hidden-selected').val($activeBtn.data('val'));
            }

            Sys.Application.add_load(function () {
                debugger;
                $('.js-hard-stop .btn').on('click', function (e) {
                    setActiveButtonGroupButton($(this));
                    return false;
                });
                setActiveButtonGroupButton($('.js-hard-stop').find("[data-val='" + $('.js-hard-stop .js-hidden-selected').val() + "']"));
            });

        </script>
    </ContentTemplate>
</asp:UpdatePanel>
