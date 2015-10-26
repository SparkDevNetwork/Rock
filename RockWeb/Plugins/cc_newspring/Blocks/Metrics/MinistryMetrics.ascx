<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MinistryMetrics.ascx.cs" Inherits="RockWeb.Plugins.cc_newspring.Blocks.Metrics.MinistryMetrics" %>

<div class="col-md-<%= metricWidth.Value %>">

    <asp:UpdatePanel ID="pnlContent" runat="server">
        <ContentTemplate>

            <% if ( churchMetricWarning.Visible )
               { %>

            <Rock:NotificationBox ID="churchMetricWarning" runat="server" NotificationBoxType="Warning" Visible="false"
                Text="Please select a metric source or key in the block settings." />

            <% }
               else
               { %>

            <asp:HiddenField ID="metricBlockNumber" runat="server" />
            <asp:HiddenField ID="metricWidth" runat="server" />
            <asp:HiddenField ID="metricClass" runat="server" />
            <asp:HiddenField ID="metricDisplay" runat="server" />
            <asp:HiddenField ID="metricComparisonDisplay" runat="server" />

            <asp:HiddenField ID="metricTitle" runat="server" />
            <asp:HiddenField ID="metricBlockId" runat="server" />

            <asp:HiddenField ID="currentMetricValue" runat="server" />
            <asp:HiddenField ID="previousMetricValue" runat="server" />

            <% if ( metricDisplay.Value.Equals( "Text" ) )
               { %>
            <div class="panel panel-block">
                <div class="panel-heading clearfix">
                    <h1 class="panel-title pull-left"><%= metricTitle.Value %></h1>
                </div>
                <div class="panel-body">
                    <h1><%= currentMetricValue.Value %><% if ( metricComparisonDisplay.Value != "" )
                                                          { %>%<% } %><% if ( metricClass.Value != "" )
                                                                         { %> <i class="fa fa-fw <%= metricClass.Value %> pull-right"></i><% } %></h1>

                    <% if ( previousMetricValue.Value != "" )
                       { %>
                    <h4>Last Year</h4>
                    <h3><%= previousMetricValue.Value %></h3>
                    <% } %>
                </div>
            </div>
            <% }
               else if ( metricDisplay.Value.Equals( "Line" ) )
               { %>

            <asp:HiddenField ID="currentYear" runat="server" />
            <asp:HiddenField ID="previousYear" runat="server" />
            <asp:HiddenField ID="metricLabels" runat="server" />
            <asp:HiddenField ID="metricDataPointsCurrent" runat="server" />
            <asp:HiddenField ID="metricDataPointsPrevious" runat="server" />

            <script>

                var lineOptions = {
                    responsive: true,
                    scaleFontSize: 16,
                    tooltipFontSize: 16,
                    bezierCurve: false,
                    datasetStrokeWidth: 3,
                    pointDotRadius: 6,
                    pointDotStrokeWidth: 3,
                }

                var <%= metricBlockId.Value %>Data = {
                    labels: [<%= metricLabels.Value %>],
                    datasets: [
                        {
                            label: "<%= currentYear.Value %>",
                                fillColor: "rgba(89,161,46,0)",
                                strokeColor: "rgba(89,161,46,1)",
                                pointColor: "rgba(89,161,46,1)",
                                pointStrokeColor: "#fff",
                                data: [<%= metricDataPointsCurrent.Value %>]
                            }
                        ]
                }

                        $( document ).ready(function() {
                            var <%= metricBlockId.Value %> = document.getElementById("<%= metricBlockId.Value %>Chart").getContext("2d");
                            window.<%= metricBlockId.Value %>Chart = new Chart(<%= metricBlockId.Value %>).Line(<%= metricBlockId.Value %>Data, lineOptions);
                        });
            </script>

            <div class="panel panel-block panel-chart">
                <div class="panel-heading clearfix">
                    <h1 class="panel-title pull-left"><%= metricTitle.Value %></h1>
                </div>
                <div class="panel-body">
                    <canvas id="<%= metricBlockId.Value %>Chart"></canvas>
                </div>
            </div>

            <% }
               else if ( metricDisplay.Value.Equals( "Donut" ) )
               { %>

            <script>

                var pieOptions = {
                    animation: false,
                    responsive: true,
                    scaleFontSize: 16,
                    tooltipFontSize: 16,
                    bezierCurve: false,
                    datasetStrokeWidth: 3,
                    pointDotRadius: 6,
                    pointDotStrokeWidth: 3,
                }

                $('body').on('ajaxSuccess', 'body', function() {
                    console.log('loaded');
                });

                $( document ).ready( function() {
                    var <%= metricBlockId.Value %> = document.getElementById("<%= metricBlockId.Value %>Chart").getContext("2d");
                    window.<%= metricBlockId.Value %>Chart = new Chart(<%= metricBlockId.Value %>).Doughnut(<%= MetricBlockValues %>, pieOptions);
                });
            </script>

            <div class="panel panel-block panel-chart">
                <div class="panel-heading clearfix">
                    <h1 class="panel-title pull-left"><%= metricTitle.Value %></h1>
                </div>
                <div class="panel-body">
                    <canvas id="<%= metricBlockId.Value %>Chart" class="HotAndNow"></canvas>
                </div>
            </div>

            <% } %>

            <% } %>
        </ContentTemplate>
    </asp:UpdatePanel>
</div>