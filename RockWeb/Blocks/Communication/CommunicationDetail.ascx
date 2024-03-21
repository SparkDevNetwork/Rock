<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CommunicationDetail.ascx.cs" Inherits="RockWeb.Blocks.Communication.CommunicationDetail" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Warning" />
        <asp:Panel ID="pnlCommunicationView" runat="server" CssClass="panel panel-block styled-scroll">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-comment-o"></i>
                    <asp:Literal ID="lTitle" runat="server" /></h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlBulk" runat="server" Text="Bulk" LabelType="Default" Visible="false" />
                    <Rock:HighlightLabel ID="hlStatus" runat="server" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>

            <asp:Panel ID="pnlTabs" class="panel-body d-flex flex-column" runat="server">
                <ul class="nav nav-tabs margin-b-md">
                    <li id="tabAnalytics" runat="server" class="active">
                        <asp:LinkButton ID="lnkTabAnalytics" runat="server" Text="Analytics" OnClick="lbTab_Click" class="js-chart-tab" />
                    </li>
                    <li id="tabMessageDetails" runat="server">
                        <asp:LinkButton ID="lnkTabMessageDetails" runat="server" Text="Message Details" OnClick="lbTab_Click" />
                    </li>
                    <li id="tabActivity" runat="server">
                        <asp:LinkButton ID="lnkTabActivity" runat="server" Text="Activity" OnClick="lbTab_Click" />
                    </li>
                    <li id="tabRecipientDetails" runat="server">
                        <asp:LinkButton ID="lnkTabRecipientDetails" runat="server" Text="Recipient Details" OnClick="lbTab_Click" />
                    </li>
                </ul>

                <%-- Tab Pane: Analytics --%>
                <asp:Panel ID="pnlAnalyticsTab" runat="server" CssClass="tab-panel">
                    <asp:Panel ID="pnlAnalyticsDeliveryStatusSummary" runat="server" CssClass="margin-t-md">

                        <%-- Actions Summary --%>
                        <div class="recipient-status row">
                            <div class="col-sm-3">
                                <div id="pnlPendingSummary" runat="server" class="metric-tile metric-pending clickable js-actions-statistic" title="The number of recipients that have not yet received the communication">
                                    <div class="metric-icon"><i class="fa fa-clock"></i></div>
                                    <div class="value">
                                        <asp:Literal ID="lPending" runat="server"></asp:Literal>
                                        <small>Pending</small>
                                    </div>
                                </div>
                            </div>
                            <div class="col-sm-3">
                                <div id="pnlDeliveredSummary" runat="server" class="metric-tile metric-delivered clickable js-actions-statistic" title="The number of recipients that the communication was successfully delivered to">
                                    <div class="metric-icon"><i class="fa fa-inbox"></i></div>
                                    <div class="value">
                                        <asp:Literal ID="lDelivered" runat="server"></asp:Literal>
                                        <small>Delivered</small>
                                    </div>
                                </div>
                            </div>
                            <div class="col-sm-3">
                                <div id="pnlFailedSummary" runat="server" class="metric-tile metric-failed clickable js-actions-statistic" title="The number of recipients to whom the communication could not be sent">
                                    <div class="metric-icon"><i class="fa fa-comment-slash"></i></div>
                                    <div class="value">
                                        <asp:Literal ID="lFailed" runat="server"></asp:Literal>
                                        <small>Failed</small>
                                    </div>
                                </div>
                            </div>
                            <div class="col-sm-3">
                                <div id="pnlCancelledSummary" runat="server" class="metric-tile metric-cancelled clickable js-actions-statistic" title="The number of recipients for whom the communication was cancelled">
                                    <div class="metric-icon"><i class="fa fa-ban"></i></div>
                                    <div class="value">
                                        <asp:Literal ID="lCancelled" runat="server"></asp:Literal>
                                        <small>Cancelled</small>
                                    </div>
                                </div>
                            </div>
                        </div>

                    </asp:Panel>

                    <%-- Analytics --%>
                    <Rock:NotificationBox ID="nbAnalyticsNotAvailable" runat="server" NotificationBoxType="Info" Text="Analytics not available for this communication." />

                    <asp:UpdatePanel ID="upAnalytics" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <asp:HiddenField ID="hfCommunicationId" runat="server" />
                            <div class="panel-analytics">
                                <div class="row">
                                    <div class="col-md-12">
                                        <Rock:NotificationBox ID="nbCommunicationorCommunicationListFound" runat="server" NotificationBoxType="Warning" Text="Invalid Communication or CommunicationList Specified" Visible="false" />
                                        <%-- Main Opens/Clicks Line Chart --%>
                                        <Rock:NotificationBox ID="nbOpenClicksLineChartMessage" runat="server" NotificationBoxType="Info" Text="No Communication Activity" Visible="false" />
                                        <div class="chart-container chart-banner w-100">
                                            <canvas id="openClicksLineChartCanvas" runat="server" class="js-chart-canvas-main" style="height:350px;" />
                                        </div>
                                    </div>
                                </div>

                                <hr />
                                <h1 class="text-center">Actions</h1>
                                <div class="row">
                                    <div class="col-md-4">
                                        <%-- Opens/Clicks PieChart --%>
                                        <Rock:NotificationBox ID="nbOpenClicksPieChartMessage" runat="server" NotificationBoxType="Info" Text="No Communication Activity" Visible="false" />
                                        <div class="chart-container">
                                            <canvas id="opensClicksPieChartCanvas" runat="server" class="js-chart-canvas-opens" />
                                        </div>
                                    </div>

                                    <%-- Actions Summary --%>
                                    <div class="col-md-8">
                                        <div class="row">
                                            <div class="col-xs-6 col-sm-4">
                                                <div class="metric-tile metric-sm metric-opened">
                                                    <div class="value">
                                                        <asp:Literal ID="lUniqueOpens" runat="server" />
                                                        <small>Unique Opens</small>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="col-xs-6 col-sm-4">
                                                <div class="metric-tile metric-sm metric-opened">
                                                    <div class="value">
                                                        <asp:Literal ID="lTotalOpens" runat="server" />
                                                        <small>Total Opens</small>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="col-xs-6 col-sm-4">
                                                <div class="metric-tile metric-sm metric-opened">
                                                    <div class="value">
                                                        <asp:Literal ID="lPercentOpened" runat="server" />
                                                        <small>Percent Opened</small>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="col-xs-6 col-sm-4">
                                                <div class="metric-tile metric-sm metric-clicked">
                                                    <div class="value">
                                                        <asp:Literal ID="lUniqueClicks" runat="server" />
                                                        <small>Unique Clicks</small>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="col-xs-6 col-sm-4">
                                                <div class="metric-tile metric-sm metric-clicked">
                                                    <div class="value">
                                                        <asp:Literal ID="lTotalClicks" runat="server" />
                                                        <small>Total Clicks</small>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="col-xs-6 col-sm-4">
                                                <div class="metric-tile metric-sm metric-clicked">
                                                    <div class="value">
                                                        <asp:Literal ID="lClickThroughRate" runat="server" />
                                                        <small>Click-Through Rate</small>
                                                    </div>
                                                </div>

                                            </div>
                                        </div>
                                    </div>
                                </div>


                                <hr />
                                <h1 class="text-center">Clients</h1>

                                <div class="row">
                                    <div class="col-md-4">
                                        <%-- Clients Doughnut Chart --%>
                                        <Rock:NotificationBox ID="nbClientsDoughnutChartMessage" runat="server" NotificationBoxType="Info" Text="No Client Communication Activity" Visible="false" />
                                        <div class="chart-container">
                                            <canvas id="clientsDoughnutChartCanvas" runat="server" class="js-chart-canvas-clients" />
                                        </div>
                                    </div>
                                    <div class="col-md-8">
                                        <asp:Panel ID="pnlClientApplicationUsage" runat="server">
                                            <h4>Clients In Use</h4>
                                            <asp:Repeater ID="rptClientApplicationUsage" runat="server" OnItemDataBound="rptClientApplicationUsage_ItemDataBound">
                                                <ItemTemplate>
                                                    <div class="row">
                                                        <div class="col-md-6">
                                                            <asp:Literal ID="lApplicationName" runat="server" />
                                                        </div>
                                                        <div class="col-md-6">
                                                            <asp:Literal ID="lUsagePercent" runat="server" />
                                                        </div>
                                                    </div>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                        </asp:Panel>
                                    </div>
                                </div>

                                <asp:Panel ID="pnlMostPopularLinks" runat="server">
                                    <hr />
                                    <h1 class="text-center">Popular Links</h1>

                                    <div class="row hidden-xs">
                                        <div class="col-sm-10"><strong>URL</strong></div>
                                        <div class="col-sm-1"><strong>Uniques</strong></div>
                                        <div id="pnlCTRHeader" runat="server" class="col-sm-1"><strong>CTR</strong></div>
                                    </div>
                                    <asp:Repeater ID="rptMostPopularLinks" runat="server" OnItemDataBound="rptMostPopularLinks_ItemDataBound">
                                        <ItemTemplate>
                                            <div class="row margin-b-lg">
                                                <div class="col-sm-10 col-xs-12">
                                                    <p>
                                                        <asp:Literal ID="lUrl" runat="server" />
                                                    </p>
                                                    <asp:Literal ID="lUrlProgressHTML" runat="server" />
                                                </div>
                                                <div class="col-sm-1 col-xs-6">
                                                    <label class="visible-xs margin-r-sm pull-left">Uniques:</label><asp:Literal ID="lUniquesCount" runat="server" />
                                                </div>
                                                <div id="pnlCTRData" runat="server" class="col-sm-1 col-xs-6">
                                                    <label class="visible-xs margin-r-sm pull-left">CTR:</label><asp:Literal ID="lCTRPercent" runat="server" />
                                                </div>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </asp:Panel>
                            </div>

                        </ContentTemplate>
                    </asp:UpdatePanel>
                </asp:Panel>

                <%-- Tab Pane: Message Details --%>
                <asp:Panel ID="pnlMessage" runat="server" CssClass="tab-panel h-100">
                    <asp:UpdatePanel runat="server" UpdateMode="Conditional" class="d-flex flex-column h-100">
                        <ContentTemplate>
                            <div class="row d-flex flex-wrap flex-fill">
                                <div class="col-md-4">
                                    <dl>
                                        <asp:Literal ID="lFutureSend" runat="server"></asp:Literal>
                                        <asp:Literal ID="lCreatedBy" runat="server"></asp:Literal>
                                        <asp:Literal ID="lApprovedBy" runat="server"></asp:Literal>
                                    </dl>
                                </div>
                                <%-- Message Content --%>
                                <div class="col-md-8 d-flex flex-column">
                                    <asp:Literal ID="lDetails" runat="server" />
                                </div>
                            </div>

                            <%-- Message Actions --%>
                            <div class="actions">
                                <asp:LinkButton ID="btnApprove" runat="server" Text="Approve" CssClass="btn btn-success" OnClick="btnApprove_Click" />
                                <asp:LinkButton ID="btnDeny" runat="server" Text="Deny" CssClass="btn btn-danger" OnClick="btnDeny_Click" />
                                <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-default" OnClick="btnEdit_Click" />
                                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel Send" CssClass="btn btn-link" OnClick="btnCancel_Click" />
                                <asp:LinkButton ID="btnCopy" runat="server" Text="Copy Communication" CssClass="btn btn-link" OnClick="btnCopy_Click" />
                                <asp:LinkButton ID="btnTemplate" runat="server" Text="Create Personal Template" CssClass="btn btn-link" OnClick="btnTemplate_Click" Visible="False" />

                                <Rock:NotificationBox ID="nbTemplateCreated" runat="server" NotificationBoxType="Success" Visible="False" Text="A new personal communication template was created." Dismissable="True"></Rock:NotificationBox>

                            </div>

                            <asp:Panel ID="pnlResult" runat="server" Visible="false">
                                <Rock:NotificationBox ID="nbResult" runat="server" NotificationBoxType="Success" />
                                <br />
                                <asp:HyperLink ID="hlViewCommunication" runat="server" Text="View Communication" />
                            </asp:Panel>

                        </ContentTemplate>
                    </asp:UpdatePanel>

                </asp:Panel>

                <%-- Tab Pane: Activity --%>
                <asp:Panel ID="pnlActivity" runat="server" CssClass="tab-panel padding-t-md">
                    <asp:UpdatePanel ID="upnlActivity" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                        <ContentTemplate>
                            <asp:HiddenField ID="hfActiveRecipient" runat="server" />

                            <div class="grid grid-panel">
                                <Rock:Grid ID="gInteractions" runat="server" AllowSorting="true" RowItemText="Activity" OnRowDataBound="gInteractions_RowDataBound">
                                    <Columns>
                                        <Rock:DateTimeField HeaderText="Date" DataField="InteractionDateTime" ItemStyle-CssClass="text-nowrap" SortExpression="InteractionDateTime" />
                                        <Rock:PersonField HeaderText="Person" DataField="PersonAlias.Person"
                                            SortExpression="PersonAlias.Person.LastName,PersonAlias.Person.NickName" />
                                        <Rock:RockBoundField HeaderText="Activity" DataField="Operation" SortExpression="Operation" />
                                        <Rock:RockLiteralField HeaderText="Details" ItemStyle-CssClass="wrap-contents" ID="lActivityDetails" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </asp:Panel>

                <%-- Tab Pane: Recipient Details  --%>
                <asp:Panel ID="pnlRecipients" runat="server" CssClass="tab-panel">
                    <asp:UpdatePanel ID="upnlRecipients" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <%-- Column Selection --%>
                            <Rock:RockCheckBoxList ID="cblProperties" runat="server" RepeatDirection="Horizontal" Label="Person Properties" />
                            <Rock:RockListBox ID="lbAttributes" runat="server" Label="Person Attributes" />

                            <%-- Message Actions --%>
                            <div class="actions">
                                <asp:LinkButton ID="btnUpdateRecipientsList" runat="server" Text="Update" CssClass="btn btn-primary btn-xs" OnClick="btnUpdateRecipientsList_Click" />
                            </div>

                            <div class="grid grid-panel margin-t-lg">
                                <Rock:GridFilter ID="rFilter" runat="server">
                                    <%-- Block-specific Filter Fields --%>
                                    <Rock:RockTextBox ID="txbFirstNameFilter" runat="server" Label="First Name" />
                                    <Rock:RockTextBox ID="txbLastNameFilter" runat="server" Label="Last Name" />
                                    <Rock:RockCheckBoxList ID="cblMedium" runat="server" Label="Communication Medium">
                                        <asp:ListItem Text="Email" />
                                        <asp:ListItem Text="SMS" />
                                    </Rock:RockCheckBoxList>
                                    <Rock:RockCheckBoxList ID="cblDeliveryStatus" runat="server" Label="Delivery Status">
                                        <asp:ListItem Text="Pending" />
                                        <asp:ListItem Text="Delivered" />
                                        <asp:ListItem Text="Failed" />
                                        <asp:ListItem Text="Cancelled" />
                                    </Rock:RockCheckBoxList>
                                    <Rock:RockCheckBoxList ID="cblOpenedStatus" runat="server" Label="Opened Status">
                                        <asp:ListItem Text="Opened" />
                                        <asp:ListItem Text="Unopened" />
                                    </Rock:RockCheckBoxList>
                                    <Rock:RockCheckBoxList ID="cblClickedStatus" runat="server" Label="Clicked Status">
                                        <asp:ListItem Text="Clicked" />
                                        <asp:ListItem Text="Not Clicked" />
                                    </Rock:RockCheckBoxList>
                                    <Rock:RockTextBox ID="txbDeliveryStatusNote" runat="server" Label="Delivery Note" />
                                    <Rock:CampusesPicker ID="cpCampuses" runat="server" Label="Campus" IncludeInactive="true" ForceVisible="true" />
                                </Rock:GridFilter>
                                <Rock:Grid ID="gRecipients" runat="server" EmptyDataText="No Recipients Found" AllowSorting="true">
                                    <Columns>
                                        <%-- Columns are dynamically added based on user selection --%>
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </asp:Panel>

            </asp:Panel>
        </asp:Panel>

        <asp:UpdatePanel ID="upDialog" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:HiddenField ID="hfActiveDialog" runat="server" />

                <Rock:ModalDialog ID="mdCreateTemplate" runat="server" Title="New Personal Template" OnCancelScript="clearActiveDialog();">
                    <Content>
                        <asp:ValidationSummary ID="valCreateTemplate" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                        <Rock:NotificationBox ID="nbTemplate" runat="server" NotificationBoxType="Info" Text="This will create a new personal communication template based off the current communication." Dismissable="True"></Rock:NotificationBox>
                        <div class="row">
                            <div class="col-sm-6">
                                <Rock:RockTextBox ID="tbTemplateName" runat="server" Label="Template Name" />
                            </div>
                            <div class="col-sm-6">
                                <Rock:CategoryPicker ID="cpTemplateCategory" runat="server" AllowMultiSelect="false" Label="Category" EntityTypeName="Rock.Model.CommunicationTemplate" />
                            </div>
                        </div>
                        <Rock:RockTextBox ID="tbTemplateDescription" runat="server" Label="Description" TextMode="MultiLine" Rows="3" />
                    </Content>
                </Rock:ModalDialog>
            </ContentTemplate>
        </asp:UpdatePanel>

        <script>
            Sys.Application.add_load(function () {
                loadCharts();
                refreshMessageContent();
                $('.js-date-rollover').tooltip();

                <%-- Hook the bootstrap tab change event to force the charts on a previously inactive tab to reload.
                     If we don't do this, the charts will not be visible following a postback from a different tab. --%>
                $('.js-chart-tab').on('shown.bs.tab', function (e) {
                    loadCharts();
                });
            });

            <%-- Clear modal when canceled --%>
            function clearActiveDialog() {
                $('#<%=hfActiveDialog.ClientID %>').val('');
            }

            <%-- Load the Message Content --%>
            function refreshMessageContent() {
                var scriptText = $('#load-email-body').html();
                eval(scriptText);
            }

            <%-- Set the iframe height for styled scrolling on load --%>
            function resizeIframe(el) {
                el.style.height = el.contentWindow.document.documentElement.scrollHeight + 'px';
            }

            <%-- Load the Analytics Charts --%>
            function loadCharts() {
                var chartSeriesColors = <%=this.SeriesColorsJSON%>;

                var getSeriesColors = function (numberOfColors) {

                    var result = chartSeriesColors;
                    while (result.length < numberOfColors) {
                        result = result.concat(chartSeriesColors);
                    }

                    return result;
                };

                <%-- Main Line Chart --%>
                var lineChartDataLabels = <%=this.LineChartDataLabelsJSON%>;
                var lineChartDataOpens = <%=this.LineChartDataOpensJSON%>;
                var lineChartDataClicks = <%=this.LineChartDataClicksJSON%>;
                var lineChartDataUnopened = <%=this.LineChartDataUnOpenedJSON%>;

                resetCanvas('js-chart-canvas-main');

                var linecharts = $('#<%=openClicksLineChartCanvas.ClientID%>');

                if (linecharts.length == 0) {
                    return null;
                }

                var linechartCtx = $('#<%=openClicksLineChartCanvas.ClientID%>')[0].getContext('2d');

                var clicksLineChart = new Chart(linechartCtx, {
                    type: 'line',
                    data: {
                        labels: lineChartDataLabels,
                        datasets: [{
                            type: 'line',
                            label: 'Opens',
                            backgroundColor: '#5DA5DA',
                            borderColor: '#5DA5DA',
                            data: lineChartDataOpens,
                            spanGaps: true,
                            fill: false
                        },
                        {
                            type: 'line',
                            label: 'Clicks',
                            backgroundColor: '#60BD68',
                            borderColor: '#60BD68',
                            data: lineChartDataClicks,
                            spanGaps: true,
                            fill: false
                        },
                        {
                            type: 'line',
                            label: 'Unopened',
                            backgroundColor: '#FFBF2F',
                            borderColor: '#FFBF2F',
                            data: lineChartDataUnopened,
                            hidden: lineChartDataUnopened == null,
                            spanGaps: true,
                            fill: false
                        }],
                    },
                    options: {
                        maintainAspectRatio: false,
                        responsive: true,
                        legend: {
                            position: 'bottom',
                            labels: {
                                filter: function (item, data) {
                                    // don't include the label if the dataset is hidden
                                    if (data.datasets[item.datasetIndex].hidden) {
                                        return false;
                                    }

                                    return true;
                                }
                            }
                        },
                        scales: {
                            xAxes: [{
                                type: 'time',
                                time: {
                                    unit: false,
                                    tooltipFormat: '<%=this.LineChartTimeFormat%>',
                                }
                            }]
                        }
                    }
                });

                <%-- Clicks/Opens Pie Chart --%>
                var pieChartDataOpenClicks = <%=this.PieChartDataOpenClicksJSON%>;

                resetCanvas('js-chart-canvas-opens');

                var opensClicksPieChartCanvasCtx = $('#<%=opensClicksPieChartCanvas.ClientID%>')[0].getContext('2d');
                var opensClicksPieChart = new Chart(opensClicksPieChartCanvasCtx, {
                    type: 'pie',
                    options: {
                        maintainAspectRatio: false,
                        responsive: true,
                        legend: {
                            position: 'right',
                            labels: {
                                filter: function (item, data) {
                                    // don't include the label if the dataset isn't defined
                                    if (data.datasets[0].data[item.index] == null) {
                                        return false;
                                    }

                                    return true;
                                }
                            }
                        }
                    },
                    data: {
                        labels: [
                            'Opens',
                            'Clicked',
                            'Unopened'
                        ],
                        datasets: [{
                            type: 'pie',
                            data: pieChartDataOpenClicks,
                            backgroundColor: ['#5DA5DA', '#60BD68', '#FFBF2F'],
                        }],
                    }
                });

                <%-- Clients Doughnut Chart --%>
                var pieChartDataClientCounts = <%=this.PieChartDataClientCountsJSON%>;
                var pieChartDataClientLabels = <%=this.PieChartDataClientLabelsJSON%>;

                resetCanvas('js-chart-canvas-clients');

                var clientsDoughnutChartCanvasCtx = $('#<%=clientsDoughnutChartCanvas.ClientID%>')[0].getContext('2d');
                var clientsDoughnutChart = new Chart(clientsDoughnutChartCanvasCtx, {
                    type: 'doughnut',
                    options: {
                        maintainAspectRatio: false,
                        responsive: true,
                        legend: {
                            position: 'right'
                        },
                        cutoutPercentage: 50,
                        tooltips: {
                            callbacks: {
                                label: function (tooltipItem, data) {
                                    var dataValue = data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index];
                                    var labelText = data.labels[tooltipItem.index];
                                    return labelText + ": " + dataValue + "%";
                                }
                            }
                        }
                    },
                    data: {
                        labels: pieChartDataClientLabels,
                        datasets: [{
                            type: 'doughnut',
                            data: pieChartDataClientCounts,
                            backgroundColor: getSeriesColors(pieChartDataClientCounts.length)
                        }],
                    }
                });

                <%-- Re-create the canvas element identified by the specified css class.
                     This action allows a new chart to be inserted into the document. --%>
                function resetCanvas(canvasClass) {
                    var canvas = $('.' + canvasClass);
                    var canvasParent = canvas.parent();
                    var canvasId = canvas.attr('id');
                    var canvasHtml = '<canvas id="' + canvasId + '" class="' + canvasClass + '" />';
                    canvas.remove();
                    canvasParent.append(canvasHtml);
                }

                <%-- Enable Tooltips for Statistics --%>
                $('.js-actions-statistic').tooltip();
            }
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
