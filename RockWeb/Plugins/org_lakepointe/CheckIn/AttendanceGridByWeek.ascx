<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttendanceGridByWeek.ascx.cs" Inherits="RockBlocks.Plugins.org_lakepointe.Checkin.AttendanceGridByWeek" %>
<asp:UpdatePanel ID="upReport" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlView" runat="server">
            <asp:Panel ID="pnlFilter" runat="server">
                <div class="panel panel-block margin-t-md">
                    <div class="panel-heading clearfix">
                        <h1 class="panel-title pull-left">
                            <i class="fa fa-users"></i>Attendance By Week
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
                                    <Rock:DatePicker ID="dpStart" runat="server" Label="Start Date" Required="true" RequiredErrorMessage="Check in Start Date is required" ValidationGroup="ReportFilter" Help="Start Date/Time to Report on" />
                                </div>
                                <div class="col-md-4">
                                    <Rock:DatePicker ID="dpEnd" runat="server" Label="End Date" Help="End Date to report on ." />
                                </div>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12 actions margin-t-md">
                                <asp:LinkButton ID="btnFilter" runat="server" AccessKey="m" ToolTip="Alt+m" Text="Filter" CssClass="btn btn-primary btn-sm" CausesValidation="true" ValidationGroup="ReportFilter" OnClick="btnFilter_Click" />
                                <asp:LinkButton ID="btnFilterSetDefault" runat="server" Text="Reset Filters" ToolTip="Set the filter to its default values" CssClass="btn btn-link btn-sm pull-right" OnClick="btnFilterSetDefault_Click" />

                            </div>
                        </div>
                        <asp:Panel ID="pnlResults" runat="server" Visible="false">
                            <Rock:NotificationBox ID="nbAttendeesError" runat="server" NotificationBoxType="Danger" Dismissable="true" Visible="false" />
                            <Rock:Grid ID="gAttendees" runat="server" AllowSorting="true" RowItemText="Group Member" ExportSource="DataSource" ExportFilename="GroupMemberAttendance">
                                <Columns>
                                    <Rock:SelectField />
                                    <Rock:RockBoundField DataField="GroupName" HeaderText="Group" SortExpression="GroupName" />
                                    <asp:HyperLinkField DataNavigateUrlFields="PersonId" DataTextField="PersonName" HeaderText="Name" SortExpression="PersonLastName, PersonFirstName" />
                                    <Rock:RockBoundField DataField="PersonName" HeaderText="Name" SortExpression="PersonLastName, PersonFirstName" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                    <Rock:RockBoundField DataField="Birthdate" HeaderText="Birthdate" DataFormatString="{0:d}" />
                                    <Rock:RockBoundField DataField="Age" HeaderText="Age" Visible="false" />
                                    <Rock:RockBoundField DataField="Gender" HeaderText="Gender" />
                                    <Rock:RockBoundField DataField="ParentsName" HeaderText="Parents" />
                                    <Rock:RockBoundField DataField="MainPhone" HeaderText="Home Phone" />
                                    <Rock:RockBoundField DataField="AttendanceCount" HeaderText="Attended" SortExpression="AttendanceCount" />
                                </Columns>
                            </Rock:Grid>
                        </asp:Panel>
                    </div>
                </div>
            </asp:Panel>

        </asp:Panel>

        <%--        <script type="text/javascript">
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

        </script>--%>
    </ContentTemplate>
</asp:UpdatePanel>
