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
                    </Rock:GridFilter>
                    <Rock:Grid ID="gHistory" runat="server" DisplayType="Full" AllowSorting="true" EmptyDataText="No Attendance Found" OnRowDataBound="gHistory_RowDataBound" >
                        <Columns>
                            <Rock:RockBoundField DataField="LocationName" HeaderText="Location" SortExpression="LocationName" />
                            <Rock:CampusField DataField="CampusId" HeaderText="Campus" SortExpression="CampusName" />
                            <Rock:RockBoundField DataField="ScheduleName" HeaderText="Schedule" SortExpression="ScheduleName" />
                            <Rock:PersonField DataField="Person" HeaderText="Person" SortExpression="Person.LastName, Person.NickName" />
                            <Rock:RockTemplateField HeaderText="Group" SortExpression="GroupName" >
                                <ItemTemplate>
                                    <asp:Literal ID="lGroupName" runat="server" />
                                        
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:DateTimeField DataField="StartDateTime" HeaderText="Start Date Time" SortExpression="StartDateTime" />
                            <Rock:DateTimeField DataField="EndDateTime" HeaderText="End Date Time" SortExpression="EndDateTime" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>              

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
