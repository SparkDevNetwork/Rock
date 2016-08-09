<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttendanceSpreadSheetTool.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Reporting.AttendanceSpreadSheetTool" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">Attendance Spreadshoot Tool</h1>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-md-6">
                        <Rock:NotificationBox ID="nbGroupTypeWarning" runat="server" NotificationBoxType="Warning" Text="Please select a group type template in the block settings." Dismissable="false" />

                        <Rock:SlidingDateRangePicker ID="drpDateRange" runat="server" Label="Sunday Date" EnabledSlidingDateRangeTypes="Previous, Last, Current" EnabledSlidingDateRangeUnits="Week"
                            Help="The schedules to display attendance for. Leave blank to not filter by schedule." />

                        <Rock:RockCheckBoxList ID="clbCampuses" runat="server" FormGroupCssClass="campuses-picker js-campuses-picker" CssClass="campuses-picker-vertical" Label="Campuses"
                            Help="The campuses to display attendance for. Leave blank to not filter by campus." />

                        <Rock:NotificationBox ID="nbGroupsWarning" runat="server" NotificationBoxType="Warning" Text="Please select at least one group." Visible="false" />
                        <h4 class="js-checkbox-selector cursor-pointer">Groups</h4>
                        <hr class="margin-t-none" />
                        <ul class="list-unstyled js-group-checkboxes group-checkboxes">

                            <asp:Repeater ID="rptGroupTypes" runat="server" OnItemDataBound="rptGroupTypes_ItemDataBound">
                                <ItemTemplate>
                                </ItemTemplate>
                            </asp:Repeater>

                        </ul>
                    </div>
                </div>

                <asp:LinkButton ID="btnUpdate" runat="server" CssClass="btn btn-primary" Text="Update" OnClick="btnUpdate_Click" />

                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" AllowSorting="false" OnRowDataBound="gList_RowDataBound" AllowPaging="false">
                        <Columns>
                            <Rock:RockBoundField DataField="GroupName" HeaderText="Worship" />
                            <Rock:RockBoundField DataField="GrandTotal" HeaderText="Grand Total" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
