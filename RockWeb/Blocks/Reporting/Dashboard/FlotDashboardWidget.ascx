<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FlotDashboardWidget.ascx.cs" Inherits="RockWeb.Blocks.Reporting.Dashboard.FlotDashboardWidget" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfRestUrlParams" runat="server" />
        <asp:HiddenField ID="hfXAxisLabel" runat="server" />

        <script type="text/javascript">

            var restUrl = '<%= ResolveUrl( "~/api/MetricValues/GetByMetricId/" ) %>';
            restUrl = restUrl + $('#<%= hfRestUrlParams.ClientID%>').val();

            $.ajax({
                url: restUrl,
                dataType: 'json',
                contentType: 'application/json'
            })
            .done(function (chartData) {

                // setup chartPoints object
                var chartPoints = {
                    color: '#8498ab',
                    clickable: true,
                    hoverable: true,
                    shadowSize: 0,
                    label: $('#<%=hfXAxisLabel.ClientID%>').val(),
                    metricValues: chartData,
                    data: []
                }

                // populate the chartPoints data array with data from the REST result
                for (var i = 0; i < chartData.length; i++) {
                    chartPoints.data.push([chartData[i].MetricValueJavascriptTimeStamp, chartData[i].YValue]);
                }

                var chartOptions = {
                    xaxis: {
                        mode: "time",
                        timeformat: "%b %Y",
                        minTickSize: [3, "month"]
                    },
                    yaxis: {
                        minTickSize: null
                    },
                    series: {
                        bars: {
                            show: false,
                            lineWidth: 5,
                            fill: true,
                            fillColor: '#d9edf7'
                        },
                        lines: {
                            show: true
                        },
                        points: {
                            show: false
                        }
                    },
                    grid: {
                        hoverable: true,
                        clickable: true,
                        margin: {
                            top: 0,
                            right: 10,
                            bottom: 0,
                            left: 50
                        },
                        borderWidth: {
                            top: 0,
                            right: 0,
                            bottom: 1,
                            left: 1
                        }
                    }
                }

                // plot the chart
                //$.plot('#<%=pnlPlaceholder.ClientID %>', [chartPoints], chartOptions);

                // setup of bootstrap tooltip which we'll show on the plothover event
                $("<div id='tooltip' class='tooltip right'><div class='tooltip-inner'></div><div class='tooltip-arrow'></div></div>").css({
                    position: "absolute",
                    display: "none",
                }).appendTo("body");

                $('#<%=pnlPlaceholder.ClientID %>').bind('plothover', function (event, pos, item) {
                    if (item) {
                        var tooltipText = item.series.metricValues[item.dataIndex].Note;
                        // if there isn't a note, just show the y value;
                        tooltipText = tooltipText || item.datapoint[1];
                        $("#tooltip").find('.tooltip-inner').html(tooltipText);
                        $("#tooltip").css({ top: item.pageY + 5, left: item.pageX + 5, opacity: 1 });
                        $("#tooltip").show();
                    }
                    else {
                        $('#tooltip').hide();
                    }
                });
            })
            .fail(function (a, b, d) {
                debugger
            });;


        </script>

        <Rock:NotificationBox ID="nbMetricWarning" runat="server" NotificationBoxType="Warning" Text="Please select a metric in the block settings." />

        <!-- chart holder -->
        <div id="chartholder">
            <asp:Label runat="server" ID="lblMetricTitle" />
            <asp:Panel ID="pnlPlaceholder" runat="server" Width="100%" Height="170px" />
        </div>


    </ContentTemplate>
</asp:UpdatePanel>
