<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttendanceReporting.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.AttendanceReporting" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:LineChart ID="lcAttendance" runat="server" DataSourceUrl="" Title="" Subtitle="" ChartHeight="300" />
        <div class="row">
            <div class="col-md-12">
                <div class="pull-right">
                    <asp:LinkButton ID="lShowGrid" runat="server" CssClass="btn btn-default btn-xs" Text="Show Data <i class='fa fa-chevron-down'></i>" ToolTip="Show Data" OnClick="lShowGrid_Click" />
                </div>
            </div>
        </div>
        <asp:Panel ID="pnlGrid" runat="server" Visible="false">

            <div class="grid">
                <Rock:Grid ID="gAttendance" runat="server" AllowSorting="true" DataKeyNames="DateTimeStamp,SeriesId" RowItemText="Attendance Summary">
                    <Columns>
                        <Rock:DateField DataField="DateTime" HeaderText="Date" SortExpression="DateTimeStamp" />
                        <asp:BoundField DataField="SeriesId" HeaderText="Series" SortExpression="SeriesId" />
                        <asp:BoundField DataField="YValue" HeaderText="Count" SortExpression="YValue" />
                    </Columns>
                </Rock:Grid>
            </div>
        </asp:Panel>

        <div class="actions">
            <asp:LinkButton ID="btnApply" runat="server" CssClass="btn btn-primary" Text="Apply" ToolTip="Update the chart" OnClick="btnApply_Click" />
        </div>

        <div class="row">
            <div class="col-md-6">
                <Rock:SlidingDateRangePicker ID="drpSlidingDateRange" runat="server" Label="Date Range" />

                <Rock:RockControlWrapper ID="rcwGraphBy" runat="server" Label="Graph By">
                    <div class="controls">
                        <div class="btn-group js-graph-by">
                            <Rock:HiddenFieldWithClass ID="hfGraphBy" CssClass="js-hidden-selected" runat="server" />
                            <asp:HyperLink ID="btnGraphByTotal" runat="server" CssClass="btn btn-primary" Text="Total" data-val="0" />
                            <asp:HyperLink ID="btnGraphByType" runat="server" CssClass="btn btn-default" Text="Type" data-val="1" />
                            <asp:HyperLink ID="btnGraphByCampus" runat="server" CssClass="btn btn-default" Text="Campus" data-val="2" />
                            <asp:HyperLink ID="btnGraphByTime" runat="server" CssClass="btn btn-default" Text="Schedule" data-val="3" />
                        </div>
                    </div>
                </Rock:RockControlWrapper>

                <Rock:RockControlWrapper ID="rcwGroupBy" runat="server" Label="Group By">
                    <div class="controls">
                        <div class="btn-group js-group-by">
                            <Rock:HiddenFieldWithClass ID="hfGroupBy" CssClass="js-hidden-selected" runat="server" />
                            <asp:HyperLink ID="btnGroupByWeek" runat="server" CssClass="btn btn-primary" Text="Week" data-val="0" />
                            <asp:HyperLink ID="btnGroupByMonth" runat="server" CssClass="btn btn-default" Text="Month" data-val="1" />
                            <asp:HyperLink ID="btnGroupByYear" runat="server" CssClass="btn btn-default" Text="Year" data-val="2" />
                        </div>
                    </div>
                </Rock:RockControlWrapper>

                <Rock:CampusesPicker ID="cpCampuses" runat="server" Label="Campuses" />
            </div>
            <div class="col-md-6">

                <Rock:NotificationBox ID="nbGroupTypeWarning" runat="server" NotificationBoxType="Warning" Text="Please select a group type template in the block settings." Dismissable="true" />
                <h4>Type</h4>
                <ul class="rocktree">

                    <asp:Repeater ID="rptGroupTypes" runat="server" OnItemDataBound="rptGroupTypes_ItemDataBound">
                        <ItemTemplate>
                        </ItemTemplate>
                    </asp:Repeater>

                </ul>
            </div>
        </div>

        <script>
            function setActiveButtonGroupButton($activeBtn) {
                $activeBtn.removeClass('btn-default').addClass('btn-primary');
                $activeBtn.siblings('.btn').addClass('btn-default').removeClass('btn-primary');
                $activeBtn.siblings('.js-hidden-selected').val($activeBtn.data('val'));
            }

            Sys.Application.add_load(function () {
                $('.js-graph-by .btn').on('click', function (e) {
                    setActiveButtonGroupButton($(this));
                });

                setActiveButtonGroupButton($('.js-graph-by').find("[data-val='" + $('.js-graph-by .js-hidden-selected').val() + "']"));

                $('.js-group-by .btn').on('click', function (e) {
                    setActiveButtonGroupButton($(this));
                });

                setActiveButtonGroupButton($('.js-group-by').find("[data-val='" + $('.js-group-by .js-hidden-selected').val() + "']"));
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
