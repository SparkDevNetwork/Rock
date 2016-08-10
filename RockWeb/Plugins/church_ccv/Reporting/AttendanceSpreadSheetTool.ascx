<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttendanceSpreadSheetTool.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Reporting.AttendanceSpreadSheetTool" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">Attendance Spreadshoot Tool</h1>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-md-3">
                        <Rock:NotificationBox ID="nbMetricWarning" runat="server" NotificationBoxType="Warning" Text="Please select a metric in the block settings." Dismissable="false" />
                        <Rock:NotificationBox ID="nbGroupTypeWarning" runat="server" NotificationBoxType="Warning" Text="Please select a group type template in the block settings." Dismissable="false" />

                        <Rock:RockDropDownList ID="ddlSundayDate" runat="server" Label="Sunday Date" />

                        <Rock:SchedulePicker ID="spSchedules" runat="server" Label="Schedules" AllowMultiSelect="true" />

                        <Rock:NotificationBox ID="nbGroupsWarning" runat="server" NotificationBoxType="Warning" Text="Please select at least one group." Visible="false" />
                        <Rock:PanelWidget ID="pwGroups" runat="server" Title="Groups">
                            <h4 class="js-checkbox-selector cursor-pointer">Groups</h4>
                            <hr class="margin-t-none" />
                            <ul class="list-unstyled js-group-checkboxes group-checkboxes">

                                <asp:Repeater ID="rptGroupTypes" runat="server" OnItemDataBound="rptGroupTypes_ItemDataBound">
                                    <ItemTemplate>
                                    </ItemTemplate>
                                </asp:Repeater>

                            </ul>
                        </Rock:PanelWidget>
                    </div>
                    <div class="col-md-9">
                    </div>


                </div>

                <asp:LinkButton ID="btnUpdate" runat="server" CssClass="btn btn-primary" Text="Update" OnClick="btnUpdate_Click" />

                <div class="grid ">
                    <Rock:Grid ID="gList" runat="server" AllowSorting="false" AllowPaging="false">
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
                    <Rock:MetricCategoryPicker ID="mpMetric" runat="server" Label="Metric" />

                    <Rock:RockCheckBoxList ID="cblCampuses" runat="server" FormGroupCssClass="campuses-picker js-campuses-picker" CssClass="campuses-picker-vertical" Label="Campuses"
                        Help="The campuses to display attendance for. Leave blank to not filter by campus." />

                    <Rock:RockCheckBoxList ID="cblAttendanceTypes" runat="server" Label="Attendance Types" Help="Select the Attendance Types to determine with Groups will be shown." Required="false" ValidationGroup="vgConfigure" />
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
