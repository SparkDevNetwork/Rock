<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttendanceSpreadSheetTool.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Reporting.AttendanceSpreadSheetTool" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <Triggers>
        <asp:PostBackTrigger ControlID="btnCreateSpreadsheet" />
    </Triggers>
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">Attendance Spreadsheet Tool</h1>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbAttendanceMetricWarning" runat="server" NotificationBoxType="Warning" Text="Please select the attendance metric in the block settings." Dismissable="false" />
                <Rock:NotificationBox ID="nbHeadcountsMetricWarning" runat="server" NotificationBoxType="Warning" Text="Please select the headcount metric in the block settings." Dismissable="false" />
                <Rock:NotificationBox ID="nbGeneralFundsMetricWarning" runat="server" NotificationBoxType="Warning" Text="Please select the general funds metric in the block settings." Dismissable="false" />
                <Rock:NotificationBox ID="nbGroupTypeWarning" runat="server" NotificationBoxType="Warning" Text="Please select a group type template in the block settings." Dismissable="false" />
                <Rock:NotificationBox ID="nbGroupsWarning" runat="server" NotificationBoxType="Warning" Text="Please select at least one group." Visible="false" />

                <div class="grid ">
                    <div class="hidden-print">
                        <Rock:RockDropDownList ID="ddlSundayDate" runat="server" Label="Weekend" AutoPostBack="true" OnSelectedIndexChanged="btnUpdate_Click" />
                    </div>

                    <h2>
                        <asp:Literal ID="lSundayDate" runat="server" /></h2>

                    <asp:Button ID="btnCreateSpreadsheet" runat="server" CssClass="btn btn-primary" Text="Create Spreadsheet" OnClick="btnCreateSpreadsheet_Click" />

                    <h3>Attendance (Checkin)</h3>
                    <Rock:NotificationBox runat="server" ID="nbReorderInstructions" NotificationBoxType="Info" Text="Drag the Attendance Areas/Groups below into the order that they should be shown in the Export. Use block settings to add/remove additional areas/groups." />

                    <Rock:Grid ID="gCheckinAttendanceExport" runat="server" AllowSorting="false" AllowPaging="false" OnGridReorder="gCheckinAttendanceExport_GridReorder" ExportFilename="CheckInExport" >
                        <Columns>
                        </Columns>
                    </Rock:Grid>

                    <h3>Headcounts</h3>
                    <Rock:Grid ID="gHeadcountsExport" runat="server" AllowSorting="false" AllowPaging="false" ExportFilename="HeadcountsExport">
                        <Columns>
                        </Columns>
                    </Rock:Grid>
                    
                    <h3>General Funds by Campus</h3>
                    <Rock:Grid ID="gGeneralFundsExport" runat="server" AllowSorting="false" AllowPaging="false" ExportFilename="GeneralFundsExport">
                        <Columns>
                            <Rock:RockBoundField DataField="EntityID" HeaderText="Campus" />
                            <Rock:RockBoundField DataField="MetricValue" HeaderText="Offering" />
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
                    <Rock:MetricCategoryPicker ID="mpGeneralFundsMetric" runat="server" Label="Metric for General Funds"/>

                    <Rock:RockCheckBoxList ID="cblCampuses" runat="server" FormGroupCssClass="campuses-picker js-campuses-picker" CssClass="campuses-picker-vertical" Label="Campuses"
                        Help="The campuses to display attendance for. Leave blank to not filter by campus." />

                    <Rock:RockCheckBoxList ID="cblAttendanceTypes" runat="server" Label="Attendance Types" Help="Select the Attendance Types to determine which Groups will be shown." Required="false" ValidationGroup="vgConfigure" />

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

                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
