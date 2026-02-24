<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LiveMetrics.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Manager.LiveMetrics" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        loadCharts();
        $('.js-threshold-btn-edit').off('click').on('click', function (e) {
            var $parentDiv = $(this).closest('div.js-threshold');
            $parentDiv.find('.js-threshold-nb').val($parentDiv.find('.js-threshold-hf').val());
            $parentDiv.find('.js-threshold-view').hide();
            $parentDiv.find('.js-threshold-edit').show();
        });

        $('a.js-threshold-edit').off('click').on('click', function (e) {
            var $parentDiv = $(this).closest('div.js-threshold');
            $parentDiv.find('.js-threshold-edit').hide();
            $parentDiv.find('.js-threshold-view').show();
            return true;
        });

        $('.js-threshold').on('click', function (e) {
            e.stopPropagation();
        });
    });

     <%-- Load the Analytics Charts --%>
    function loadCharts() {

        var chartData = eval($('#<%=hfChartData.ClientID%>').val());
        var chartLabel = eval($('#<%=hfChartLabel.ClientID%>').val());

        var options = {
            maintainAspectRatio: false,
            legend: {
                position: 'bottom',
                display: false
            },
            tooltips: {
                enabled: true,
                backgroundColor: '#000',
                bodyFontColor: '#fff',
                titleFontColor: '#fff'
            },
            scales: {
                yAxes: [{
                    ticks: {
                        beginAtZero: true,
                        maxTicksLimit: 6,
                        precision: 0
                    },
                }]
            },
            layout: {
                padding: {
                    left: 8,
                    right: 16,
                    top: 16,
                    bottom: 0
                }
            }
        };

        var data = {
            labels: chartLabel,
            datasets: [{
                fill: true,
                backgroundColor: '#059BFF',
                borderColor: '#059BFF',
                borderWidth: 0,
                pointRadius: 10,
                pointBackgroundColor: 'rgba(5,155,255,0.0)',
                pointBorderColor: 'rgba(5,155,255,0.0)',
                pointBorderWidth: 0,
                pointHoverBackgroundColor: 'rgba(5,155,255,.6)',
                pointHoverBorderColor: 'rgba(5,155,255,.6)',
                pointHoverRadius: '9',
                lineTension: 0,
                data: chartData
            }
            ],
            borderWidth: 0
        };

        Chart.defaults.global.defaultFontColor = '#777';
        Chart.defaults.global.defaultFontFamily = 'sans-serif';

        var ctx = document.getElementById( '<%=chartCanvas.ClientID%>').getContext('2d');
        var chart = new Chart(ctx, {
            type: 'line',
            data: data,
            options: options
        });

    }
</script>

<Rock:RockUpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" Dismissable="true" />

        <asp:Panel ID="pnlContent" runat="server" CssClass="checkin-manager">

            <div class="panel-header hidden">
                <h1 class="panel-title"><i class="ti ti-sitemap"></i>&nbsp;<asp:Literal ID="lGroupTypeName" runat="server" /></h1>
            </div>

            <div class="panel checkin-chart">
                <asp:HiddenField ID="hfChartData" runat="server" />
                <asp:HiddenField ID="hfChartLabel" runat="server" />
                <asp:Literal ID="lChartCanvas" runat="server" />
                <div id="pnlChart" runat="server" class="chart-banner">
                    <canvas id="chartCanvas" runat="server" />
                </div>
            </div>

            <div class="row">
                <asp:Panel ID="pnlCheckedInCount" runat="server" CssClass="col-sm-4">
                    <div class="panel checkin-metric">
                        <div class="panel-body">
                            <span class="h3 font-weight-semibold"><asp:Literal ID="lCheckedInPeopleCount" runat="server" /></span>
                            <span class="d-block small text-muted font-weight-semibold">Checked-in</span>
                        </div>
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnlPresentCount" runat="server" CssClass="col-sm-4">
                    <div class="panel checkin-metric">
                        <div class="panel-body">
                            <span class="h3 font-weight-semibold"><asp:Literal ID="lPresentPeopleCount" runat="server" /></span>
                            <span class="d-block small text-muted font-weight-semibold">Present</span>
                        </div>
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnlTotalCount" runat="server" CssClass="col-sm-4">
                    <div class="panel checkin-metric">
                        <div class="panel-body">
                            <span class="h3 font-weight-semibold"><asp:Literal ID="lTotalPeopleCount" runat="server" /></span>
                            <span class="d-block small text-muted font-weight-semibold">Total</span>
                        </div>
                    </div>
                </asp:Panel>
            </div>

            <div class="panel panel-default">

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <asp:Panel ID="pnlNavHeading" runat="server" CssClass="panel-header cursor-pointer">
                    <asp:PlaceHolder runat="server">
                        <div class="pull-left">
                            <i class="ti ti-chevron-left"></i>
                            <asp:Literal ID="lNavHeading" runat="server" />
                        </div>
                        <div class="pull-right">
                            <Rock:Toggle ID="tglHeadingRoom" runat="server" OnText="Open" OffText="Close" ButtonSizeCssClass="btn-xs" OnCssClass="btn-success" OffCssClass="btn-danger" OnCheckedChanged="tglRoom_CheckedChanged" />
                        </div>
                        <asp:Panel ID="pnlThreshold" runat="server" CssClass="location-threshold pull-right d-flex mr-spacing-tiny js-threshold">
                            <span class="small align-self-center mr-spacing-tiny">Threshold: </span>
                            <Rock:HiddenFieldWithClass ID="hfThreshold" runat="server" CssClass="js-threshold-hf" />
                            <asp:Label ID="lThreshold" runat="server" CssClass="js-threshold-view js-threshold-l small align-self-center mr-spacing-xs" />
                            <a class="btn btn-default btn-xs btn-square js-threshold-view js-threshold-btn-edit mr-spacing-tiny"><i class="ti ti-edit"></i></a>
                            <Rock:NumberBox ID="nbThreshold" runat="server" CssClass="input-xs input-width-xs js-threshold-edit js-threshold-nb mr-spacing-tiny" NumberType="Integer" Style="display: none"></Rock:NumberBox>
                            <asp:LinkButton ID="lbUpdateThreshold" runat="server" CssClass="btn btn-primary btn-xs btn-square js-threshold-edit js-threshold-btn-save paneleditor-button mr-spacing-tiny" OnClick="lbUpdateThreshold_Click" Style="display: none"><i class="ti ti-check"></i></asp:LinkButton>
                            <a class="btn btn-default btn-xs btn-square js-threshold-edit js-threshold-btn-cancel paneleditor-button mr-spacing-tiny" style="display: none"><i class="ti ti-ban"></i></a>
                        </asp:Panel>
                    </asp:PlaceHolder>
                </asp:Panel>

                <ul class="list-group">
                    <asp:Repeater ID="rptNavItems" runat="server">
                        <ItemTemplate>
                            <li id="liNavItem" runat="server" class="list-group-item cursor-pointer">
                                <div class="content"><%# Eval("Name") %></div>
                                <div class="pull-right d-flex align-items-center">
                                    <asp:Label ID="lblCurrentCount" runat="server" CssClass="badge" />
                                    <Rock:Toggle ID="tglRoom" runat="server" CssClass="ml-spacing-md" OnText="Open" OffText="Close" ButtonSizeCssClass="btn-xs" OnCssClass="btn-success" OffCssClass="btn-danger" OnCheckedChanged="tglRoom_CheckedChanged"  />
                                    <i class='ti ti-fw ti-chevron-right ml-spacing-md'></i>
                                </div>
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>

                    <asp:Repeater ID="rptPeople" runat="server">
                        <ItemTemplate>
                            <li id="liNavItem" runat="server" class="list-group-item d-flex align-items-center cursor-pointer clearfix">
                                <div class="d-flex align-items-center">
                                    <asp:Literal ID="imgPerson" runat="server" />
                                    <div>
                                        <span class="checkin-person-name js-checkin-person-name"><%# Eval("Name") %></span><asp:Literal ID="lAge" runat="server" />
                                        <%# Eval("ScheduleGroupNames") %>
                                    </div>
                                </div>
                                <div class="ml-auto">
                                    <asp:Literal ID="lStatus" runat="server" />
                                    <asp:Literal ID="lMobileStatus" runat="server" />
                                </div>
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>

                </ul>

            </div>

        </asp:Panel>

    </ContentTemplate>
</Rock:RockUpdatePanel>
