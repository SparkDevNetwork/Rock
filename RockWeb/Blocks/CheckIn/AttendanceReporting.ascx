<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttendanceReporting.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.AttendanceReporting" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:LineChart ID="lcAttendance" runat="server" DataSourceUrl="" Title="" Subtitle="" ChartHeight="300" />
        <div class="row">
            <div class="col-md-12">
                <div class="pull-right">
                    <asp:LinkButton ID="lShowGrid" runat="server" CssClass="btn btn-default btn-sm" Text="<i class='fa fa-table'></i>" ToolTip="Show Grid" OnClick="lShowGrid_Click" />
                </div>
            </div>
        </div>
        <asp:Panel ID="pnlGrid" runat="server" Visible="false">
            <Rock:Grid ID="gAttendance" runat="server" AllowSorting="true" DataKeyNames="DateTimeStamp,SeriesId">
                <Columns>
                    <Rock:DateField DataField="DateTime" HeaderText="Date" SortExpression="DateTimeStamp" />
                    <asp:BoundField DataField="SeriesId" HeaderText="Series" SortExpression="SeriesId" />
                    <asp:BoundField DataField="YValue" HeaderText="Count" SortExpression="YValue" />
                </Columns>
            </Rock:Grid>
        </asp:Panel>

        <div class="row">
            <div class="col-md-6">
                <Rock:RockDropDownList ID="ddlGraphBy" runat="server" Label="Graph By" />
                <Rock:RockDropDownList ID="ddlGroupBy" runat="server" Label="Group By" />
                <Rock:CampusesPicker ID="cpCampuses" runat="server" Label="Campuses" />
            </div>
            <div class="col-md-6">
                <Rock:SlidingDateRangePicker ID="drpSlidingDateRange" runat="server" Label="Date Range" />
                <Rock:NotificationBox ID="nbGroupTypeWarning" runat="server" NotificationBoxType="Warning" Text="Please select a group type template in the block settings." Dismissable="true" />
                <h4>Area</h4>
                <asp:Repeater ID="rptGroupTypes" runat="server" OnItemDataBound="rptGroupTypes_ItemDataBound" >
                    <ItemTemplate>
                        <Rock:RockCheckBoxList ID="cblGroups" runat="server" Label='<%# Eval("Name") %>' />
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>

        <div class="actions">
            <asp:LinkButton ID="btnApply" runat="server" CssClass="btn btn-primary" Text="Apply" OnClick="btnApply_Click" />
        </div>


    </ContentTemplate>
</asp:UpdatePanel>
