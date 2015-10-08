<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttendanceHistoryList.ascx.cs" Inherits="RockWeb.Blocks.Checkin.AttendanceHistoryList" %>

<asp:UpdatePanel ID="upAttendanceHistory" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" CssClass="panel panel-block" runat="server">

            <Rock:ModalAlert ID="mdGridWarning" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-check-square"></i> Attendance History</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server">
                        <Rock:DateRangePicker ID="drpDates" runat="server" Label="Date Range" />
                        <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" />
                        <Rock:RockDropDownList ID="ddlAttendanceGroup" runat="server" Label="Group" />
                        <Rock:SchedulePicker ID="spSchedule" runat="server" Label="Schedule" />
                        <Rock:RockDropDownList ID="ddlDidAttend" runat="server" Label="Attended">
                            <asp:ListItem Text="[All]" Value="all"></asp:ListItem>
                            <asp:ListItem Text="Did Attend" Value="didattend"></asp:ListItem>
                            <asp:ListItem Text="Did Not Attend" Value="didnotattend"></asp:ListItem>
                        </Rock:RockDropDownList>
                    </Rock:GridFilter>
                    <Rock:Grid ID="gHistory" runat="server" DisplayType="Full" AllowSorting="true" EmptyDataText="No Attendance Found" OnRowDataBound="gHistory_RowDataBound" >
                        <Columns>
                            <Rock:RockLiteralField HeaderText="Location" ID="lLocationName" SortExpression="LocationName" />
                            <Rock:CampusField DataField="CampusId" HeaderText="Campus" SortExpression="CampusName" />
                            <Rock:RockBoundField DataField="ScheduleName" HeaderText="Schedule" SortExpression="ScheduleName" />
                            <Rock:PersonField DataField="Person" HeaderText="Person" SortExpression="Person.LastName, Person.NickName" />
                            <Rock:RockLiteralField HeaderText="Group" ID="lGroupName" SortExpression="GroupName" />
                            <Rock:DateTimeField DataField="StartDateTime" HeaderText="Start Date Time" SortExpression="StartDateTime" />
                            <Rock:DateTimeField DataField="EndDateTime" HeaderText="End Date Time" SortExpression="EndDateTime" />
                            <Rock:BoolField DataField="DidAttend" HeaderText="Attended" SortExpression="DidAttend" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>              

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
