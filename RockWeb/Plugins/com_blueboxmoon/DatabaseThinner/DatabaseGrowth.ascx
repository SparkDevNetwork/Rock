<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DatabaseGrowth.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.DatabaseThinner.DatabaseGrowth" %>

<script src="/SignalR/hubs"></script>
<script type="text/javascript">
    $(function () {
        var proxy = $.connection.rockMessageHub;

        proxy.client.growthScanProgress = function (completed, total, text) {
            var $bar = $('#<%= upPanel.ClientID %> .js-scan-progress-bar');

            $bar.prop('aria-valuenow', completed);
            $bar.prop('aria-valuemax', total);
            if (total !== 0) {
                $bar.css('width', (completed / total * 100) + '%');
                $bar.text(completed + '/' + total);
            }
            else {
                $bar.css('width', '0%');
                $bar.text('');
            }

            $('#<%= upPanel.ClientID %> .js-progress-text').text(text);

            $('#<%= pnlProgress.ClientID %>').show();
        };

        proxy.client.growthScanStatus = function (status) {
            $('#<%= hfScanState.ClientID %>').val(status);
            $('#<%= lbScanCompleted.ClientID %>').get(0).click();
        };

        $.connection.hub.start().done(function () {
            $('#<%= hfConnectionId.ClientID %>').val($.connection.hub.id);
        });
    });
</script>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfConnectionId" runat="server" />
        <asp:HiddenField ID="hfScanState" runat="server" />
        <asp:LinkButton ID="lbScanCompleted" runat="server" CssClass="hidden" OnClick="lbScanCompleted_Click" />

        <asp:Panel ID="pnlScan" runat="server">
            <div class="alert alert-warning">
                <p>
                    Scanning growth statistics can take some time and spike IO usage.
                    Please use caution when gathering these statistics during normal usage hours.
                </p>
            </div>

            <asp:LinkButton ID="lbScan" runat="server" CssClass="btn btn-primary" Text="Show Statistics" OnClick="lbScan_Click" />

            <asp:Panel ID="pnlProgress" runat="server" CssClass="margin-t-md" style="display: none;">
                <p class="js-progress-text"></p>
                <div class="progress">
                    <div class="progress-bar progress-bar-info js-scan-progress-bar" role="progressbar" style="width: 0%" aria-valuenow="0" aria-valuemax="0"></div>
                </div>
            </asp:Panel>
        </asp:Panel>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">
            <Rock:RockLiteral ID="ltEstimatedGrowth" runat="server" Label="Estimated Annual Growth" />

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h3 class="panel-title">Tables</h3>
                </div>

                <div class="panel-body">
                    <div class="grid grid-panel">
                        <Rock:Grid ID="gTables" runat="server" RowItemText="Table" AllowSorting="true" OnGridRebind="gTables_GridRebind">
                            <Columns>
                                <asp:BoundField DataField="Name" SortExpression="Name" HeaderText="Table Name" />
                                <asp:BoundField DataField="Rows" SortExpression="Rows" HeaderText="Total Rows" DataFormatString="{0:N0}" />
                                <asp:BoundField DataField="BytesMB" SortExpression="Bytes" HeaderText="Total Used MB" DataFormatString="{0:N2} MB" />
                                <asp:BoundField DataField="RowsPerYear" SortExpression="RowsPerYear" HeaderText="Annual Rows" DataFormatString="{0:N0}" />
                                <asp:BoundField DataField="BytesPerYearMB" SortExpression="BytesPerYear" HeaderText="Annual Growth MB" DataFormatString="{0:N2} MB" />
                                <asp:BoundField DataField="YearOverYearGrowth" SortExpression="YearOverYearGrowth" HeaderText="YoY Growth" DataFormatString="{0:N2}%" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
