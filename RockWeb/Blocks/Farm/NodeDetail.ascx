<%@ Control Language="C#" AutoEventWireup="true" CodeFile="NodeDetail.ascx.cs" Inherits="RockWeb.Blocks.Farm.NodeDetail" %>

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
</script>

<asp:UpdatePanel ID="upUpdatePanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-network-wired"></i>
                    Web Farm Node:
                    <asp:Literal runat="server" ID="lNodeName" />
                </h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlActive" runat="server" />
                </div>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbMessage" runat="server" NotificationBoxType="Info" />
                <asp:ValidationSummary ID="valValidation" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <div id="pnlViewDetails" runat="server">

                    <div class="row">
                        <div class="col-md-5">
                            <asp:Literal ID="lDescription" runat="server" />
                        </div>
                        <div class="col-md-7">
                            <span class="control-label">CPU Utilization</span>
                            <div style="height:250px;">
                                <asp:Literal ID="lChart" runat="server" />
                            </div>
                        </div>
                    </div>

                    <!-- div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" data-shortcut-key="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                    </div -->
                </div>

                <div id="pnlEditDetails" runat="server">
                    <div class="alert alert-info">
                        In order to respect any new setting changes made here, please restart this node after saving.
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
