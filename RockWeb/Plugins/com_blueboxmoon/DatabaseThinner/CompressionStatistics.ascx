<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CompressionStatistics.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.DatabaseThinner.CompressionStatistics" %>

<script src="/SignalR/hubs"></script>
<script type="text/javascript">
    $(function () {
        var proxy = $.connection.rockMessageHub;

        proxy.client.statisticScanProgress = function (completed, total, text) {
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

        proxy.client.statisticScanStatus = function (status) {
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
        <div class="panel panel-block">
            <div class="panel-heading">
                <h3 class="panel-title">Compression Statistics</h3>
            </div>

            <div class="panel-body">
                <asp:HiddenField ID="hfConnectionId" runat="server" />
                <asp:HiddenField ID="hfScanState" runat="server" />
                <asp:LinkButton ID="lbScanCompleted" runat="server" CssClass="hidden" OnClick="lbScanCompleted_Click" />

                <asp:Panel ID="pnlScan" runat="server">
                    <div class="alert alert-warning">
                        <p>
                            Scanning compression information can take some time and spike CPU usage.
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
                    <fieldset>
                    <div class="row">
                            <div class="col-md-6">
                                <dl>
                                    <dt>Total Space Saved</dt>
                                    <dd><asp:Literal ID="ltSpaceSaved" runat="server" /></dd>
                                </dl>
                            </div>

                            <div class="col-md-6">
                                <dl>
                                    <dt>Estimated Total Annual Savings</dt>
                                    <dd><asp:Literal ID="ltAnnualSavings" runat="server" /></dd>
                                </dl>
                            </div>
                        </div>
                    </fieldset>

                    <asp:Panel ID="pnlTables" runat="server">
                        <h4>Tables</h4>
                        <div class="well">
                            <fieldset>
                                <div class="row">
                                    <div class="col-md-6">
                                        <dl>
                                            <dt>Compressed Tables</dt>
                                            <dd><asp:Literal ID="ltCompressedTableCount" runat="server" /></dd>
                                            <dt>Space Saved</dt>
                                            <dd><asp:Literal ID="ltTableSpaceSaved" runat="server" /></dd>
                                            <dt>Estimated Annual Savings</dt>
                                            <dd><asp:Literal ID="ltTableAnnualSavings" runat="server" /></dd>
                                        </dl>
                                    </div>

                                    <div class="col-md-6">
                                        <dl>
                                            <dt>Uncompressed Size</dt>
                                            <dd><asp:Literal ID="ltTableUncompressedSize" runat="server" /></dd>
                                            <dt>Compressed Size</dt>
                                            <dd><asp:Literal ID="ltTableCompressedSize" runat="server" /></dd>
                                        </dl>
                                    </div>
                                </div>
                            </fieldset>
                        </div>
                    </asp:Panel>

                    <h4>Communications</h4>
                    <div class="well">
                        <fieldset>
                            <div class="row">
                                <div class="col-md-6">
                                    <dl>
                                        <dt>Compressed Communications</dt>
                                        <dd><asp:Literal ID="ltCompressedCommunicationsCount" runat="server" /></dd>
                                        <dt>Space Saved</dt>
                                        <dd><asp:Literal ID="ltCompressedCommunicationsSpaceSaved" runat="server" /></dd>
                                        <dt>Estimated Annual Savings</dt>
                                        <dd><asp:Literal ID="ltCompressedCommunicationsAnnualSavings" runat="server" /></dd>
                                    </dl>
                                </div>

                                <div class="col-md-6">
                                    <dl>
                                        <dt>Uncompressed Size</dt>
                                        <dd><asp:Literal ID="ltCompressedCommunicationsUncompressedSize" runat="server" /></dd>
                                        <dt>Compressed Size</dt>
                                        <dd><asp:Literal ID="ltCompressedCommunicationsCompressedSize" runat="server" /></dd>
                                    </dl>
                                </div>
                            </div>
                        </fieldset>
                    </div>
                </asp:Panel>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
