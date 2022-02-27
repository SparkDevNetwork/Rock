<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EventList.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor.EventList" %>
<%@ Register Namespace="com.blueboxmoon.WatchdogMonitor.Web.UI.Controls" Assembly="com.blueboxmoon.WatchdogMonitor" TagPrefix="WM" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><asp:Literal ID="lTitle" runat="server" /></h1>
            </div>

            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfEvents" runat="server" OnApplyFilterClick="gfEvents_ApplyFilterClick" OnClearFilterClick="gfEvents_ClearFilterClick" OnDisplayFilterValue="gfEvents_DisplayFilterValue">
                        <Rock:RockDropDownList ID="ddlDevice" runat="server" Label="Device" />
                        <Rock:RockDropDownList ID="ddlState" runat="server" Label="State" />
                        <Rock:DateRangePicker ID="drpDateRange" runat="server" Label="Date Range" />
                        <Rock:RockTextBox ID="tbLastMessage" runat="server" Label="Last Message" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gEvents" runat="server" AllowSorting="true" RowItemText="Event" OnGridRebind="gEvents_GridRebind" OnRowDataBound="gEvents_RowDataBound">
                        <Columns>
                            <Rock:RockBoundField DataField="DeviceName" HeaderText="Device" SortExpression="DeviceName" />
                            <Rock:RockBoundField DataField="ServiceCheckName" HeaderText="Service Check" SortExpression="ServiceCheckName" />
                            <Rock:RockBoundField DataField="HtmlState" HeaderText="State" HtmlEncode="false" SortExpression="State" />
                            <Rock:DateTimeField DataField="StartDateTime" HeaderText="Start Date Time" SortExpression="StartDateTime" />
                            <Rock:DateTimeField DataField="EndDateTime" HeaderText="End Date Time" SortExpression="EndDateTime" />
                            <Rock:RockBoundField DataField="LastSummary" HeaderText="Last Message" SortExpression="LastSummary" />
                            <Rock:EditField ButtonCssClass="js-toggle-silence btn btn-default" IconCssClass="fa fa-bell" OnClick="gEvents_ToggleSilenceClick" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
