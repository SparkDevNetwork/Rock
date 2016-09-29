<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttendanceSpreadSheetTool.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Reporting.AttendanceSpreadSheetTool" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">Attendance Spreadshoot Tool</h1>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbAttendanceMetricWarning" runat="server" NotificationBoxType="Warning" Text="Please select the attendance metric in the block settings." Dismissable="false" />
                <Rock:NotificationBox ID="nbHeadcountsMetricWarning" runat="server" NotificationBoxType="Warning" Text="Please select the headcount metric in the block settings." Dismissable="false" />
                <Rock:NotificationBox ID="nbGroupTypeWarning" runat="server" NotificationBoxType="Warning" Text="Please select a group type template in the block settings." Dismissable="false" />
                <Rock:NotificationBox ID="nbGroupsWarning" runat="server" NotificationBoxType="Warning" Text="Please select at least one group." Visible="false" />

                <div class="grid ">
                    <Rock:GridFilter ID="gfList" runat="server" OnApplyFilterClick="btnUpdate_Click">
                        <asp:Panel ID="pnlCustomLayout" runat="server">
                            <Rock:RockDropDownList ID="ddlSundayDate" runat="server" Label="Sunday Date" />
                            <Rock:SchedulePicker ID="spSchedules" runat="server" Label="Schedules" AllowMultiSelect="true" />

                            <asp:Panel ID="pnlGroups" runat="server">
                                <h4 class="js-checkbox-selector cursor-pointer">Groups</h4>
                                <hr class="margin-t-none" />
                                <ul class="list-unstyled js-group-checkboxes group-checkboxes">

                                    <asp:Repeater ID="rptGroupTypes" runat="server" OnItemDataBound="rptGroupTypes_ItemDataBound">
                                        <ItemTemplate>
                                        </ItemTemplate>
                                    </asp:Repeater>

                                </ul>
                            </asp:Panel>

                        </asp:Panel>

                    </Rock:GridFilter>


                    <Rock:RockCheckBox ID="cbShowServiceTimeColumns" runat="server" Text="Show Service Time Columns" Checked="true" />
                    <Rock:RockCheckBox ID="cbShowTotalColumns" runat="server" Text="Show Totals Columns" Checked="true" />
                    <Rock:RockControlWrapper ID="rcwCheckinAreaOptions" runat="server" Label="Checkin Area Options">
                        <Rock:RockRadioButton ID="rbAll" runat="server" GroupName="checkinareaoptions" Text="Show All" Checked="true" />
                        <Rock:RockRadioButton ID="rbShowOnlyVolunteerAttendance" GroupName="checkinareaoptions" runat="server" Text="Show only 'Volunteer -' groups" Checked="false" />
                        <Rock:RockRadioButton ID="rbHideVolunteerAttendance" GroupName="checkinareaoptions" runat="server" Text="Hide 'Volunteer -' groups" Checked="false" />
                    </Rock:RockControlWrapper>
                    <Rock:RockCheckBox ID="cbShowSortKey" runat="server" Text="Show Sort Key" Checked="false" />
                    <asp:LinkButton ID="btnUpdate" runat="server" CssClass="btn btn-sm btn-primary" Text="Update" OnClick="btnUpdate_Click" />

                    <h2>
                        <asp:Literal ID="lSundayDate" runat="server" /></h2>
                    <h3>Headcounts Export</h3>
                    <Rock:Grid ID="gHeadcountsExport" runat="server" AllowSorting="false" AllowPaging="false" ExportFilename="HeadcountsExport">
                        <Columns>
                        </Columns>
                    </Rock:Grid>


                    <h3>Attendance Export (Checkin)</h3>

                    <Rock:Grid ID="gCheckinAttendanceExport" runat="server" AllowSorting="false" AllowPaging="false" ExportFilename="CheckInExport">
                        <Columns>
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>

        <%-- Configuration Panel --%>
        <asp:Panel ID="pnlConfigure" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdConfigure" runat="server" ValidationGroup="vgConfigure" OnSaveClick="mdConfigure_SaveClick">
                <Content>
                    <Rock:MetricCategoryPicker ID="mpAttendanceMetric" runat="server" Label="Metric for Attendance (Checkin)" />
                    <Rock:MetricCategoryPicker ID="mpHeadcountsMetric" runat="server" Label="Metric for Headcounts" />

                    <Rock:RockCheckBoxList ID="cblCampuses" runat="server" FormGroupCssClass="campuses-picker js-campuses-picker" CssClass="campuses-picker-vertical" Label="Campuses"
                        Help="The campuses to display attendance for. Leave blank to not filter by campus." />

                    <Rock:RockCheckBoxList ID="cblAttendanceTypes" runat="server" Label="Attendance Types" Help="Select the Attendance Types to determine which Groups will be shown." Required="false" ValidationGroup="vgConfigure" />
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
