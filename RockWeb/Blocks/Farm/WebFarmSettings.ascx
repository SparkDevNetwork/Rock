<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WebFarmSettings.ascx.cs" Inherits="RockWeb.Blocks.Farm.WebFarmSettings" %>

<style>
    .card .indicator {
        height: 4px;
    }

    .server-meta {
        font-size: 20px;
    }

    .bg-disabled {
        background: #F5F5F5;
        color: #AEAEAE;
    }

    .bg-disabled .indicator {
        background: #A3A3A3;
    }
</style>
<script>
    (function () {
        var options = {
            responsive: true,
            maintainAspectRatio: false,
            legend: {
                display: false
            },
            scales: {
                yAxes: [{
                    display: false,
                    ticks: {
                        min: 0,
                        max: 100
                    }
                }],
                xAxes: [{
                    display: false
                }]
            },
            hover: {
                mode: 'nearest',
                intersect: false
            },
            tooltips: {
                hasIndicator: true,
                intersect: false,
                callbacks: {
                    label: function (tooltipItem, data) {
                        var label = data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index] || '';
                        if (label) {
                            label = 'CPU: ' + label + '%';
                        }

                        return label;
                    }
                }
            }
        };

        $(document).ready(function () {
            $('.js-chart').each(function () {
                var el = $(this);
                var data = el.attr('data-chart') ? JSON.parse(el.attr('data-chart')) : {};
                var chart = new Chart(el, { type: 'line', data: data, options: options });
            });
        });
    })();
</script>

<asp:UpdatePanel ID="upUpdatePanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-network-wired"></i>
                    Web Farm Settings
                </h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlActive" runat="server" />
                </div>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <asp:ValidationSummary ID="valValidation" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <div id="pnlViewDetails" runat="server">
                    <div class="row">
                        <div class="col-md-4">
                            <asp:Literal ID="lDescription" runat="server" />
                        </div>
                        <div class="col-md-8">
                            <h5>Nodes</h5>
                                <div class="row">
                                    <asp:Repeater ID="rNodes" runat="server" OnItemCommand="rNodes_ItemCommand" OnItemDataBound="rNodes_ItemDataBound">
                                        <ItemTemplate>
                                            <div class="col-sm-6 col-md-6 col-lg-4">
                                                <asp:LinkButton runat="server" style="color: inherit;" CommandArgument='<%# Eval("Id") %>'>
                                                    <div class="card border-top-0 mb-4 <%# !(bool)Eval("IsActive") ? "bg-disabled" : "" %>">
                                                        <div class="indicator <%# (bool)Eval("IsActive") ? "bg-success" : "" %> <%# (bool)Eval("IsUnresponsive") ? "bg-danger" : "" %>"></div>
                                                        <div class="card-header bg-transparent d-flex justify-content-between py-2 px-2">
                                                            <span class="server-meta flex-fill d-flex flex-nowrap align-items-center leading-snug overflow-hidden" title='Polling Interval: <%# Eval("PollingIntervalSeconds") %>'>
                                                                <i class="fa fa-<%# (bool)Eval("IsActive") ? "server" : "exclamation-triangle" %>"></i>
                                                                <p class="ml-1 font-weight-bold text-truncate text-black mb-0">
                                                                    <%# Eval("NodeName") %>
                                                                </p>
                                                            </span>
                                                            <%# (bool)Eval("IsLeader") ? "<span class='ml-2 flex-shrink-0' title='Leader'><i class='fa fa-user-tie'></i></span>" :"" %>
                                                            <%# (bool)Eval("IsJobRunner") ? "<span class='ml-2 flex-shrink-0' title='Job Runner'><i class='fa fa-cog'></i></span>" :"" %>
                                                        </div>
                                                        <div class="card-body p-0" style="height:88px;">
                                                            <span id="spanLastSeen" runat="server" class="label label-danger rounded-pill position-absolute m-2" style="bottom:0;right:0;">
                                                                <asp:Literal ID="lLastSeen" runat="server" />
                                                            </span>
                                                            <asp:Literal ID="lChart" runat="server" />
                                                        </div>
                                                    </div>
                                                </asp:LinkButton>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>
                            </div>
                        </div>

                        <div class="actions">
                            <asp:LinkButton ID="btnEdit" runat="server" AccessKey="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                        </div>
                    </div>

                <div id="pnlEditDetails" runat="server">
                    <div class="alert alert-info">
                        In order to respect any new setting changes made here, please restart Rock on each node after saving.
                    </div>

                    <div class="row">
                        <div class="col-md-6 col-sm-9">
                            <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                            <Rock:RockTextBox ID="tbWebFarmKey" runat="server" Label="Key" Help="This feature is intended for enterprise size churches that would benefit from a distributed environment. Most Rock churches should not use the Web Farm because of the low level of benefit and a high complexity cost. A special key is required to activate this feature." />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6 col-sm-9">
                            <Rock:NumberBox ID="nbPollingMin" runat="server" Label="Polling Minimum" Help="The number of seconds that is the minimum wait time before a node attempts to execute leadership. If this value is left blank, then a default will be used." />
                            <Rock:NumberBox ID="nbPollingMax" runat="server" Label="Polling Maximum" Help="The number of seconds that is the maximum wait time before a node attempts to execute leadership. If this value is left blank, then a default will be used." />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>

            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
