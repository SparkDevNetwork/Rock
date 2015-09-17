<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttendanceHistoryList.ascx.cs" Inherits="com.reallifeministries.Attendance.AttendanceHistoryList" %>

<asp:UpdatePanel ID="upAttendanceHistory" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" CssClass="panel panel-block" runat="server">

            <Rock:ModalAlert ID="mdGridWarning" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-check-square"></i>Recent Attendance History</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server">
                        <Rock:DateRangePicker ID="drpDates" runat="server" Label="Date Range" />
                        <Rock:RockDropDownList ID="ddlGroups" runat="server" Label="Group" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gHistory" runat="server" DisplayType="Full" AllowSorting="true" EmptyDataText="No Attendance Found" AllowPaging="false" >
                        <Columns>
                            <Rock:RockBoundField DataField="GroupTypeName" HeaderText="Group Type" SortExpression="GroupTypeName" />
                            <Rock:RockBoundField DataField="GroupName" HeaderText="Group" SortExpression="GroupName" />
                            <Rock:RockBoundField DataField="Location" HeaderText="Location" SortExpression="Location" />
                            
                            <Rock:RockTemplateField HeaderText="When" SortExpression="StartDateTime">
                                <ItemTemplate>
                                    <%# ((DateTime)Eval( "StartDateTime" )).ToShortDateString() %>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>              

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
