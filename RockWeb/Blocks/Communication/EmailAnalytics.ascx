<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EmailAnalytics.ascx.cs" Inherits="RockWeb.Blocks.Communication.EmailAnalytics" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfCommunicationId" runat="server" />
        <asp:HiddenField ID="hfCommunicationListGroupId" runat="server" />
        <div class="panel panel-block panel-analytics">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-line-chart"></i>&nbsp;<asp:Literal ID="lTitle" runat="server" Text="Email Analytics" /></h1>

                <div class="panel-labels">
                </div>
            </div>

            <div class="panel-body">
                <canvas id="clicksLineChartCanvas" runat="server" />
            </div>
        </div>



        <script>
            Sys.Application.add_load(function () {
                // Workaround for Chart.js not working in IE11 (supposed to be fix in chart.js 2.7)
                // see https://github.com/chartjs/Chart.js/issues/4633
                Number.MAX_SAFE_INTEGER = Number.MAX_SAFE_INTEGER || 9007199254740991;
                Number.MIN_SAFE_INTEGER = Number.MIN_SAFE_INTEGER || -9007199254740991;

                var linechartCtx = $('#<%=clicksLineChartCanvas.ClientID%>')[0].getContext('2d');
                var clicksLineChart = new Chart(linechartCtx, {
                    type: 'line',
                    data: {
                        labels: [
                            new Date('2017-07-30T13:16:18.9843825-07:00'),
                            new Date('2017-08-01T13:16:18.9843825-07:00'),
                            new Date('2017-08-02T13:16:18.9843825-07:00'),
                            new Date('2017-08-03T13:16:18.9843825-07:00'),
                            new Date('2017-08-04T13:16:18.9843825-07:00'),
                            new Date('2017-08-05T13:16:18.9843825-07:00'),
                        ],
                        datasets: [{
                            type: 'line',
                            label: 'dataset 1',
                            //backgroundColor: 'blue',
                            //borderColor: 'green',
                            data: [
                                100,
                                200,
                                300,
                                100,
                                50,
                                400
                            ]
                        }],
                    },
                    options: {
                        scales: {
                            xAxes: [{
                                type: 'time',
                                //display: true,
                                time: {
                                    unit: 'day',
                                    //format: 'MM/DD/YYYY HH:mm'
                                }
                            }]
                        }
                    }
                    

                });
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
