<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EmployeeCoachingHRPortal.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Forms.EmployeeCoachingHRPortal" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlGrid" runat="server" CssClass="panel panel-block js-grid-header" Visible="true">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-plug"></i>HR Portal for Employee Coaching</h1>
            </div>
            <Rock:NotificationBox ID="nbNoOpportunities" runat="server" NotificationBoxType="Info" Visible="false" Text="There are no current reports." />
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue" Visible="true">
                        <Rock:PersonPicker ID="ppSupervisor" runat="server" Label="Supervisor" Visible="true" />
                        <Rock:PersonPicker ID="ppEmployee" runat="server" Label="Employee" Visible="true" />
                        <Rock:NumberRangeEditor ID="nreMonth" runat="server" Label="Month(s)" LowerValue="1" UpperValue="12" Visible="true" />
                        <Rock:DateRangePicker ID="drpSupervisor" runat="server" Label="Supervisor Submit Date" Visible="true" />
                        <Rock:DateRangePicker ID="drpEmployee" runat="server" Label="Employee Submit Date" Visible="true" />
                        <Rock:RockDropDownList ID="ddlStatusFilter" runat="server" Visible="true" Label="Report Status">
                            <asp:ListItem Text="[All]" Value="all"></asp:ListItem>
                            <asp:ListItem Text="Employee" Value="Employee"></asp:ListItem>
                            <asp:ListItem Text="Supervisor" Value="Supervisor"></asp:ListItem>
                            <asp:ListItem Text="HR" Value="HR"></asp:ListItem>
                        </Rock:RockDropDownList>
                    </Rock:GridFilter>
                    <Rock:Grid ID="gReports" runat="server" OnRowSelected="gReport_View" CssClass="js-grid-requests" Visible="true" AllowSorting="true" OnRowDataBound="gReport_RowDataBound">
                        <Columns>
                            <Rock:SelectField />
                            <Rock:RockBoundField DataField="SupervisorName" HeaderText="Supervisor Name" SortExpression="SupervisorName" />
                            <Rock:RockBoundField DataField="EmployeeName" HeaderText="Employee Name" SortExpression="EmployeeName" />
                            <Rock:RockBoundField DataField="PositionTitle" HeaderText="Position Title" SortExpression="PositionTitle" />
                            <Rock:RockBoundField DataField="Month" HeaderText="Month" DataFormatString="{0:d}" SortExpression="Month" />
                            <Rock:RockBoundField DataField="Status" HeaderText="Status" SortExpression="Status" />
                            <Rock:RockBoundField DataField="SupervisorSubmitDate" HeaderText="Supervisor Submit Date" DataFormatString="{0:g}" SortExpression="SupervisorSubmitDate" />
                            <Rock:RockBoundField DataField="EmployeeSubmitDate" HeaderText="Employee Submit Date" DataFormatString="{0:g}" SortExpression="EmployeeSubmitDate" />
                            <Rock:DeleteField OnClick="DeleteReportClicked" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
