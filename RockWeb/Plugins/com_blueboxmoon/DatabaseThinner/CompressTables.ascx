<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CompressTables.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.DatabaseThinner.CompressTables" %>

<script src="/SignalR/hubs"></script>
<script type="text/javascript">
    $(function () {
        var proxy = $.connection.rockMessageHub;

        proxy.client.scanProgress = function (completed, total) {
            var $bar = $('#<%= upPanel.ClientID %> .js-scan-progress-bar');

            $bar.prop('aria-valuenow', completed);
            $bar.prop('aria-valuemax', total);
            $bar.css('width', (completed / total * 100) + '%');
            $bar.text(completed + '/' + total);

            $bar.removeClass('progress-bar-striped').removeClass('active');

            $('#<%= pnlProgress.ClientID %>').show();
        };

        proxy.client.scanStatus = function (status) {
            $('#<%= hfTableState.ClientID %>').val(status);
            $('#<%= lbScanCompleted.ClientID %>').get(0).click();
        };

        proxy.client.compressActionProgress = function (completed, total) {
            var $bar = $('#<%= upPanel.ClientID %> .js-compress-progress-bar');

            $bar.prop('aria-valuenow', completed);
            $bar.prop('aria-valuemax', total);
            $bar.css('width', (completed / total * 100) + '%');
            $bar.text(completed + '/' + total);

            $bar.removeClass('progress-bar-striped').removeClass('active');
        }

        proxy.client.compressActionStatus = function (error) {
            $('#<%= hfCompressStatus.ClientID %>').val(error);

            if (error !== '' && error !== 'FixPartialTables') {
                $('#<%= pnlCompressStatus.ClientID %> .js-compress-error .alert').text(error);
                $('#<%= pnlCompressStatus.ClientID %> .progress').hide();
                $('#<%= pnlCompressStatus.ClientID %> .js-compress-status').hide();
                $('#<%= pnlCompressStatus.ClientID %> .js-compress-error').show();
            }
            else {
                window.location = $('#<%= lbCompressDone.ClientID %>').attr('href');
            }
        };

        $.connection.hub.start().done(function () {
            $('#<%= hfConnectionId.ClientID %>').val($.connection.hub.id);
        });
    });

    Sys.Application.add_load(function () {
        /* Enable the toggle buttons for turning on/off compression */
        $('#<%= upPanel.ClientID %> .js-toggle-button').bootstrapToggle({
            on: 'Yes',
            off: 'No',
            size: 'small'
        });

        /* Override the default click handler for the toggle buttons so we can confirm the action. */
        $('#<%= upPanel.ClientID %> .js-toggle-button').closest('.toggle').on('click', function (e) {
            var $cb = $(this).find('input');
            var onlineMode = '<%= GetAttributeValue( "UseOnlineMode" ) %>' === 'True';
            var warningMessage = 'a table' + (onlineMode ? '' : ' will make it unavailable and ') + ' could take a few minutes, are you sure?';

            e.preventDefault();
            e.stopPropagation();

            var tableName = $cb.closest('tr').children('td:first-child').text();

            if ($cb.is(':checked')) {
                Rock.dialogs.confirm('Decompressing ' + warningMessage, function (result) {
                    if (result === true) {
                        $('#<%= hfModifyTable.ClientID %>').val(tableName + '|False');
                        window.location = $('#<%= lbModifyTable.ClientID %>').attr('href');
                        $cb.bootstrapToggle('off');
                    }
                });
            }
            else {
                Rock.dialogs.confirm('Compressing ' + warningMessage, function (result) {
                    if (result === true) {
                        $('#<%= hfModifyTable.ClientID %>').val(tableName + '|True');
                        window.location = $('#<%= lbModifyTable.ClientID %>').prop('href');
                        $cb.bootstrapToggle('on');
                    }
                });
            }
        });

        /* If the scan button is disabled, auto-show the scan progress bar. */
        if ($('#<%= lbScanTables.ClientID %>').hasClass('aspNetDisabled')) {
            var $bar = $('#<%= upPanel.ClientID %> .js-scan-progress-bar');

            $bar.prop('aria-valuenow', 1);
            $bar.prop('aria-valuemax', 1);
            $bar.css('width', '100%');
            $bar.text('');
            $bar.addClass('progress-bar-striped').addClass('active');

            $('#<%= pnlProgress.ClientID %>').show();
        }
    });
</script>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfConnectionId" runat="server" />
        <asp:HiddenField ID="hfTableState" runat="server" />
        <asp:LinkButton ID="lbScanCompleted" runat="server" CssClass="hidden" OnClick="lbScanCompleted_Click" />

        <Rock:NotificationBox ID="nbUnsupported" runat="server" NotificationBoxType="Warning" Visible="false">
            Table compression is not supported on your version of SQL server.
            It is currently only supported on Azure SQL and SQL Server 2016 SP1 or later.
        </Rock:NotificationBox>

        <asp:Panel ID="pnlDetails" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h3 class="panel-title">Table Compression Settings</h3>
            </div>

            <div class="panel-body">
                <asp:Panel ID="pnlPartialTables" runat="server" Visible="false" CssClass="alert alert-warning">
                    <p>
                        The following tables are only partially compressed. This will cause the estimated uncompressed
                        size to be incorrect. Fixing these is recommended, but could take quite some time and cause your
                        Rock server to be unavailable until the process is finished.
                    </p>

                    <ul><li><asp:Literal ID="ltPartialTables" runat="server" /></li></ul>

                    <asp:LinkButton ID="lbFixPartialTables" runat="server" CssClass="btn btn-default margin-t-md" Text="Fix Tables" OnClick="lbFixPartialTables_Click" />
                </asp:Panel>

                <asp:Panel ID="pnlScan" runat="server" CssClass="margin-b-md">
                    <Rock:NotificationBox ID="nbScanWarning" runat="server" NotificationBoxType="Warning">
                        <p>
                            Doing a scan on all tables can take many minutes depending on the size of your database.
                        </p>
                        <p>
                            It also puts a heavy load on the SQL server so we suggest only running a scan after normal usage hours.
                        </p>
                    </Rock:NotificationBox>

                    <Rock:RockDropDownList ID="ddlScanTable" runat="server" Label="Table" EnhanceForLongLists="true" />

                    <asp:LinkButton ID="lbScanTables" runat="server" CssClass="btn btn-primary" Text="Scan" OnClick="lbScanTables_Click" />

                    <asp:Panel ID="pnlProgress" runat="server" CssClass="margin-t-md" style="display: none;">
                        <div class="progress">
                            <div class="progress-bar progress-bar-info js-scan-progress-bar" role="progressbar" style="width: 0%" aria-valuenow="0" aria-valuemax="0">0/0</div>
                        </div>
                    </asp:Panel>
                </asp:Panel>

                <asp:Panel ID="pnlScanResults" runat="server" CssClass="margin-b-md" Visible="false">
                    <asp:HiddenField ID="hfModifyTable" runat="server" />
                    <asp:LinkButton ID="lbModifyTable" runat="server" CssClass="hidden" OnClick="lbModifyTable_Click" />

                    <Rock:NotificationBox ID="nbRecommendation" runat="server" NotificationBoxType="Info" />
                    <div class="grid">
                        <Rock:Grid ID="gTables" runat="server" RowItemText="Table" AllowSorting="true" OnGridRebind="gTables_GridRebind">
                            <Columns>
                                <asp:BoundField DataField="Name" HeaderText="Table" SortExpression="Name" />
                                <asp:BoundField DataField="RowCount" HeaderText="Row Count" SortExpression="RowCount" DataFormatString="{0:N0}" />
                                <asp:BoundField DataField="UncompressedSizeMB" HeaderText="Uncompressed Size" SortExpression="UncompressedSize" DataFormatString="{0:N} MB" />
                                <asp:BoundField DataField="CompressedSizeMB" HeaderText="Compressed Size" SortExpression="CompressedSize" DataFormatString="{0:N} MB" />
                                <asp:BoundField DataField="SpaceSavedMB" HeaderText="Savings" SortExpression="SpaceSaved" DataFormatString="{0:N} MB" />
                                <asp:BoundField DataField="EstimatedSavingsPerYearMB" HeaderText="Savings per Year" SortExpression="EstimatedSavingsPerYear" DataFormatString="{0:N} MB" />
                                <Rock:BoolField DataField="CompressionRecommended" HeaderText="Recommended" SortExpression="CompressionRecommended" />
                                <Rock:RockTemplateField HeaderText="Compressed" SortExpression="Compressed" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <input type="checkbox" class="js-toggle-button" <%# (bool)Eval("Compressed") ? "checked" : "" %> />
                                    </ItemTemplate>
                                </Rock:RockTemplateField>
                            </Columns>
                        </Rock:Grid>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlCompressStatus" runat="server" Visible="false">
                    <asp:HiddenField ID="hfCompressStatus" runat="server" />
                    <Rock:NotificationBox ID="nbCompressStatus" runat="server" CssClass="js-compress-status margin-b-md" NotificationBoxType="Info" />

                    <div class="progress">
                        <div class="progress-bar progress-bar-info progress-bar-striped active js-compress-progress-bar" role="progressbar" style="width: 100%" aria-valuenow="0" aria-valuemax="0"></div>
                    </div>

                    <div class="js-compress-error" style="display: none;">
                        <div class="alert alert-danger margin-b-md"></div>

                        <asp:LinkButton ID="lbCompressDone" runat="server" CssClass="btn btn-primary" Text="Continue" OnClick="lbCompressDone_Click" />
                    </div>
                </asp:Panel>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
