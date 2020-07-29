<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ExceptionList.ascx.cs" Inherits="RockWeb.Blocks.Administration.ExceptionList" %>

<script type="text/javascript">
    function confirmExceptionListClear() {
        return confirm("Are you sure that you want to clear all exceptions?");
    }
</script>

<asp:UpdatePanel ID="upExceptionList" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdAlert" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <%-- Block-specific Header --%>
                <h1 class="panel-title"><i class="fa fa-bug"></i>Exception Log Summary</h1>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbBlockStatus" runat="server" />
                <asp:Panel ID="pnlList" runat="server">
                    <h4>Recent Activity</h4>
                    <p>
                        Shows the frequency and variety of recent exception activity.
                    </p>

                    <%-- Exceptions Chart --%>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:LineChart ID="lcExceptions" runat="server" DataSourceUrl="~/api/ExceptionLogs/GetChartData" ChartHeight="280px" />
                            <Rock:BarChart ID="bcExceptions" runat="server" DataSourceUrl="~/api/ExceptionLogs/GetChartData" ChartHeight="280px" />
                        </div>
                    </div>

                    <%-- Exceptions List --%>
                    <h4>Log Summary</h4>
                    <div class="row">
                        <div class="col-md-9">
                            <p>
                                A summary of the system exception log, grouped by type and description with most recent events shown first.
                            </p>
                        </div>

                        <div class="col-md-3">
                            <p class="clearfix">
                                <asp:LinkButton ID="btnClearExceptions" runat="server" CssClass="btn btn-action btn-sm pull-right" OnClientClick="return confirmExceptionListClear();" OnClick="btnClearExceptions_Click" CausesValidation="false">
                            <i class="fa fa-repeat"></i> Clear All Exceptions
                                </asp:LinkButton>
                            </p>
                        </div>
                    </div>

                    <div class="grid margin-t-md">
                        <Rock:GridFilter ID="fExceptionList" runat="server">
                            <Rock:RockDropDownList ID="ddlSite" runat="server" Label="Site" />
                            <Rock:PagePicker ID="ppPage" runat="server" Label="Page" />
                            <Rock:PersonPicker ID="ppUser" runat="server" Label="User" />
                            <Rock:RockTextBox ID="txtType" runat="server" Label="Type" />
                            <Rock:SlidingDateRangePicker ID="sdpDateRange" runat="server" Label="Date Range" EnabledSlidingDateRangeTypes="Previous, Last, Current, DateRange" />
                        </Rock:GridFilter>
                        <Rock:Grid ID="gExceptionList" runat="server" AllowSorting="true" EmptyDataText="No Exceptions Found" OnRowDataBound="gExceptionList_RowDataBound">
                            <Columns>
                                <Rock:DateTimeField DataField="LastExceptionDate" HeaderText="Last Date" ItemStyle-Wrap="false" SortExpression="LastExceptionDate" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" />
                                <Rock:RockBoundField DataField="ExceptionTypeName" HeaderText="Type" SortExpression="ExceptionTypeName" TruncateLength="255" HtmlEncode="false" />
                                <Rock:RockLiteralField ID="lDescription" HeaderText="Description" SortExpression="Description" ItemStyle-CssClass="wrap-contents" />
                                <Rock:RockBoundField DataField="TotalCount" HeaderText="Total Count" SortExpression="TotalCount" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Center" />
                                <Rock:RockBoundField DataField="SubsetCount" SortExpression="SubsetCount" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Center" />
                                <Rock:LinkButtonField ID="lbShowDetail" Text="<i class='fa fa-file-alt'></i>" CssClass="btn btn-default btn-sm btn-square" OnClick="gExceptionList_ShowDetail" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </asp:Panel>
            </div>
    </ContentTemplate>
</asp:UpdatePanel>
