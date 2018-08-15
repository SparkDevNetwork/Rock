<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConnectionsReport.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.ConnectionsReport.ConnectionsReport" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="js-group-panel" runat="server">
            <div class="panel panel-block">
                <div class="panel-heading panel-follow clearfix">

                    <h1 class="panel-title pull-left">
                        <asp:Literal ID="lReadOnlyTitle" runat="server" />
                    </h1>

                </div>

                <Rock:GridFilter ID="workflowFilters" runat="server" OnApplyFilterClick="workflowFilters_ApplyFilterClick">
                    <Rock:RockDropDownList ID="rddConnectionOpportunity" runat="server" Label="Connection Opportunity"></Rock:RockDropDownList>
                    <Rock:CampusPicker ID="campusPicker" runat="server" Label="Campus"></Rock:CampusPicker>
                    <Rock:PersonPicker ID="ppAssignedPerson" runat="server" Label="Assigned Worker" />
                    <Rock:RockCheckBoxList ID="cblStatus" runat="server" Label="Status" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" />
                    <Rock:RockCheckBoxList ID="cblState" runat="server" Label="State" RepeatDirection="Horizontal">
                        <asp:ListItem Text="Active" Value="0" />
                        <asp:ListItem Text="Inactive" Value="1" />
                        <asp:ListItem Text="Future Follow Up" Value="2" />
                        <asp:ListItem Text="Future Follow Up (Past Due)" Value="-2" />
                        <asp:ListItem Text="Connected" Value="3" />
                    </Rock:RockCheckBoxList>
                    <Rock:DateRangePicker ID="dateRange" ClientIDMode="Static" runat="server" CssClass="np_customDate" Label="Date" />
                    <%--<Rock:RockDropDownList ID="rddlGroupBy" runat="server" Label="Group By">
                        <asp:ListItem Text="" Value=""></asp:ListItem>
                        <asp:ListItem Text="Workflow" Value="1"></asp:ListItem>
                        <asp:ListItem Text="Campus" Value="2"></asp:ListItem>
                        <asp:ListItem Text="Worker" Value="3"></asp:ListItem>
                        <asp:ListItem Text="Status" Value="4"></asp:ListItem>
                    </Rock:RockDropDownList>--%>
                </Rock:GridFilter>

                <div id="viewToggleDiv" class="clearfix" style="margin-top: 15px; margin-right: 15px;">
                    <div id="viewToggleDiv2" class="pull-right">
                        <Rock:Toggle ID="viewToggle" ClientIDMode="Static" runat="server" CssClass="pull-left" OnText="Workflow Report" OffText="Workflow Metrics" ActiveButtonCssClass="btn-success" ButtonSizeCssClass="btn-xs" Checked="true" />
                    </div>
                </div>

                <div class="panel-body" id="StatsPanel" <%=  viewToggle.Checked ? "style='display: none;'" : "" %>>
                    <div style="width: 100%;">
                        <asp:HiddenField ID="hfChartData" runat="server" ClientIDMode="Static" Value="" EnableViewState="false" />
                        <script type="text/javascript" src="https://www.google.com/jsapi"></script>
                        <script type="text/javascript">
                            // For some reason the onclick event doesn't work, so we use mousedown and mouseup as a workaround
                            viewtogglemousedown = false;
                            $(document).on('mousedown', '#viewToggle', function (e)
                            {
                                if (e.which == 1)
                                {
                                    viewtogglemousedown = true;
                                }
                            });
                            $(document).on('mouseup', '#viewToggle', function (e)
                            {
                                if (viewtogglemousedown)
                                {
                                    viewtogglemousedown = false;
                                    // Mouse clicked
                                    var tglState = ($('#viewToggle_hfChecked').val() == 'false');

                                    if (tglState)
                                    {
                                        $('#StatsPanel').hide();
                                        $('#ReportPanel').show();
                                    }
                                    else
                                    {
                                        $('#ReportPanel').hide();
                                        $('#StatsPanel').show();
                                        drawCharts();
                                    }

                                }
                            });
                            $(document).on('mouseup', function (e)
                            {
                                viewtogglemousedown = false;
                            });

                            // We want to redraw the charts when the window size changes, but only when it's done changing.
                            // (we don't want to be constantly redrawing it while in the middle of a resize)
                            var rtime;
                            var timeout = false;
                            var delta = 1000;
                            $(window).resize(function ()
                            {
                                rtime = new Date();
                                if (timeout === false)
                                {
                                    timeout = true;
                                    setTimeout(resizeend, delta);
                                }
                            });
                            function resizeend()
                            {
                                if (new Date() - rtime < delta)
                                {
                                    setTimeout(resizeend, delta);
                                } else
                                {
                                    timeout = false;
                                    console.log('Done resizing');
                                    drawCharts();
                                }
                            }


                            // Makes sure google's visualization library is loaded, then draws the charts
                            function initCharts()
                            {
                                if (google.visualization)
                                {
                                    drawCharts();
                                }
                                else
                                {
                                    google.load("visualization", "1", { packages: ["corechart"] });
                                    google.setOnLoadCallback(drawCharts);
                                }
                            }


                            function drawCharts()
                            {
                                dateMin = $('#dateRange_lower').val();
                                dateMax = $('#dateRange_upper').val();
                                daysMin = Math.round(Math.abs(new Date() - new Date(dateMax)) / 8.64e7);
                                daysMax = Math.round(Math.abs(new Date(dateMax) - new Date(dateMin)) / 8.64e7);
                                drawChart("workflowChart1", "Histogram", {
                                    title: 'Workflows by Age (Days)',
                                    //legend: { position: 'none' },
                                    //colors: ['#5C3292', '#1A8763', '#871B47', '#999999'],
                                    interpolateNulls: false,
                                    histogram: {
                                        bucketSize: 10,
                                        hideBucketItems: true
                                    },
                                    hAxis: {
                                        gridlines: { color: '#CCC', count: -1 },
                                        minValue: daysMin,
                                        maxValue: daysMax,
                                        maxAlternation: 1,
                                        slantedText: false
                                    },
                                    vAxis: {
                                        minValue: 0,
                                        maxValue: 50
                                    }
                                });
                                drawChart("workflowChart2", "PieChart", {
                                    title: 'Campus Distribution'
                                });
                                drawChart("workflowChart3", "PieChart", {
                                    title: 'Workflow Distribution'
                                });
                                drawChart("workflowChart4", "PieChart", {
                                    title: 'Status Distribution'
                                });
                                drawChart("workflowChart5", "PieChart", {
                                    title: 'Worker Distribution'
                                });

                            }

                            function drawChart(chartDivId, chartType, chartOptions)
                            {
                                var chartData = $("#" + chartDivId).data("chart");
                                if (chartData && $.isArray(chartData) && chartData.length > 1)
                                {
                                    var dataTable = google.visualization.arrayToDataTable(chartData);
                                    var chart = new google.visualization[chartType](document.getElementById(chartDivId));
                                    chart.draw(dataTable, chartOptions);
                                }
                                else
                                {
                                    $("#" + chartDivId).html("<span>No Chart Data</span>");
                                }
                            }

                        </script>
                        <style>
                            .subCharts:after {
                                clear: both;
                            }

                            .subChartsLeft {
                                float: left;
                                width: 50%;
                            }

                            .subChartsRight {
                                float: left;
                                width: 50%;
                            }

                            .workflowChart {
                                width: 100%;
                                height: 500px;
                                max-width: 1100px;
                                margin: auto;
                            }

                            @media (max-width: 992px) {
                                .subChartsLeft {
                                    float: none;
                                    width: 100%;
                                }

                                .subChartsRight {
                                    float: none;
                                    width: 100%;
                                }
                            }
                        </style>


                        <%--<div class="subCharts">
                            <div class="subChartsLeft">
                                <Rock:RockLiteral ID="rlWorkflowStats" runat="server" />
                            </div>
                            <div class="subChartsRight">--%>
                        <div id="workflowChart1" class="workflowChart" data-chart="<%= workflowChartData1 %>"></div>

                        <%--</div>
                        </div>--%>

                        <div class="subCharts">
                            <div class="subChartsLeft">
                                <div id="workflowChart2" class="workflowChart" data-chart="<%= workflowChartData2 %>"></div>
                                <div id="workflowChart3" class="workflowChart" data-chart="<%= workflowChartData3 %>"></div>

                            </div>
                            <div class="subChartsRight">
                                <div id="workflowChart4" class="workflowChart" data-chart="<%= workflowChartData4 %>"></div>
                                <div id="workflowChart5" class="workflowChart" data-chart="<%= workflowChartData5 %>"></div>

                            </div>
                        </div>


                    </div>
                    <asp:Literal ID="lContent" runat="server" />
                    <asp:Literal ID="statsLiquid" runat="server" />

                </div>
                <div class="panel-body" id="ReportPanel" <%=  !viewToggle.Checked ? "style='display: none;'" : "" %>>
                    <Rock:Grid ID="workflowReportTable" runat="server" AllowSorting="true" AllowPaging="true" RowClickEnabled="true" OnRowSelected="workflowReportTable_RowSelected" OnGridRebind="workflowReportTable_GridRebind">
                        <Columns>
                            <Rock:RockBoundField DataField="Connector" HeaderText="Connector" SortExpression="Connector.PersonAlias.Person.LastName,Connector.PersonAlias.Person.NickName" />
                            <Rock:RockBoundField HeaderText="Opportunity" DataField="Opportunity.Name" SortExpression="ConnectionOpportunity.Name" />
                            <Rock:RockBoundField DataField="Campus" HeaderText="Campus" SortExpression="Campus.Name" />
                            <Rock:RockBoundField DataField="Name" HeaderText="Requestor" SortExpression="PersonAlias.Person.NickName,PersonAlias.Person.LastName" />
                            <%--<Rock:RockBoundField DataField="Group" HeaderText="Group" SortExpression="AssignedGroup.Name" />--%>
                            <Rock:RockBoundField DataField="Opened" HeaderText="Opened" HtmlEncode="false" />
                            <Rock:RockBoundField DataField="LastActivity" HeaderText="Last Activity" HtmlEncode="false" />
                            <asp:TemplateField HeaderText="Status" SortExpression="ConnectionStatus.Name">
                                <ItemTemplate>
                                    <span class='label label-<%# Eval("StatusLabel") %>'><%# Eval("Status") %></span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="State" SortExpression="ConnectionState">
                                <ItemTemplate>
                                    <%# Eval("StateLabel") %>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </Rock:Grid>
                    <Rock:Grid ID="workflowGroupedReportTable" runat="server" Visible="false" AllowSorting="false" AllowPaging="true" RowClickEnabled="false" OnGridRebind="workflowReportTable_GridRebind">
                        <Columns>
                            <asp:TemplateField>
                                <HeaderTemplate><%# workflowGroupedReportTableItemName %></HeaderTemplate>
                                <ItemTemplate><%# HttpUtility.HtmlDecode(Eval("GroupedItem").ToString()) %></ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField HeaderText="Count" DataField="Count" SortExpression="Count" />
                            <asp:TemplateField HeaderText="0 - 30 Days">
                                <ItemTemplate><%# HttpUtility.HtmlDecode(Eval("OneMonthOldStats").ToString()) %></ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="30 - 60 Days">
                                <ItemTemplate><%# HttpUtility.HtmlDecode(Eval("TwoMonthsOldStats").ToString()) %></ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="60 - 90 Days">
                                <ItemTemplate><%# HttpUtility.HtmlDecode(Eval("ThreeMonthsOldStats").ToString()) %></ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText=">90 Days">
                                <ItemTemplate><%# HttpUtility.HtmlDecode(Eval("OlderThanThreeMonthsStats").ToString()) %></ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Total">
                                <ItemTemplate><%# HttpUtility.HtmlDecode(Eval("TotalStats").ToString()) %></ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </Rock:Grid>
                </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
