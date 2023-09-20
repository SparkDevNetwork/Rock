<%@ Control Language="C#" AutoEventWireup="true" CodeFile="UnusedFiles.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.DatabaseThinner.UnusedFiles" %>

<script src="/SignalR/hubs"></script>
<script type="text/javascript">
    $(function () {
        var proxy = $.connection.rockMessageHub;

        proxy.client.unusedFileScanProgress = function (completed, total, text) {
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

        proxy.client.unusedFileScanStatus = function (status, error) {
            if (error) {
                $('#<%= upPanel.ClientID %> .js-progress-text').text(error);
            }
            else {
                $('#<%= hfScanState.ClientID %>').val(status);
                $('#<%= lbScanCompleted.ClientID %>').get(0).click();
            }
        };

        $.connection.hub.start().done(function () {
            $('#<%= hfConnectionId.ClientID %>').val($.connection.hub.id);
        });
    });
</script>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfDialogMessage" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h3 class="panel-title">Unused Files</h3>
            </div>

            <div class="panel-body">
                <asp:HiddenField ID="hfConnectionId" runat="server" />
                <asp:HiddenField ID="hfScanState" runat="server" />
                <asp:LinkButton ID="lbScanCompleted" runat="server" CssClass="hidden" OnClick="lbScanCompleted_Click" />

                <asp:Panel ID="pnlScan" runat="server">
                    <div class="alert alert-warning">
                        <p>
                            Scanning for unused files can take some time.
                            Please use caution when running during normal usage hours.
                        </p>
                    </div>

                    <asp:LinkButton ID="lbScan" runat="server" CssClass="btn btn-primary" Text="Scan Files" OnClick="lbScan_Click" />
        
                    <asp:Panel ID="pnlProgress" runat="server" CssClass="margin-t-md" style="display: none;">
                        <p class="js-progress-text"></p>
                        <div class="progress">
                            <div class="progress-bar progress-bar-info js-scan-progress-bar" role="progressbar" style="width: 0%" aria-valuenow="0" aria-valuemax="0"></div>
                        </div>
                    </asp:Panel>
                </asp:Panel>

                <asp:Panel ID="pnlDetails" runat="server" CssClass="grid grid-panel" Visible="false">
                    <Rock:GridFilter ID="gfFiles" runat="server" OnApplyFilterClick="gfFiles_ApplyFilterClick" OnDisplayFilterValue="gfFiles_DisplayFilterValue" OnClearFilterClick="gfFiles_ClearFilterClick">
                        <Rock:RockTextBox ID="tbFileNameFilter" runat="server" Label="File Name" />
                        <Rock:BinaryFileTypePicker ID="ftpFileTypeFilter" runat="server" Label="File Type" />
                    </Rock:GridFilter>

                    <Rock:Grid ID="gFiles" runat="server" AllowSorting="true" OnGridRebind="gFiles_GridRebind">
                        <Columns>
                            <Rock:SelectField />
                            <asp:BoundField DataField="FileName" SortExpression="FileName" HeaderText="File Name" />
                            <asp:BoundField DataField="Id" SortExpression="Id" HeaderText="Id" />
                            <asp:BoundField DataField="FileType" SortExpression="FileType" HeaderText="File Type" />
                            <asp:BoundField DataField="FileSize" SortExpression="FileSize" HeaderText="File Size" DataFormatString="{0:N0}" />
                            <Rock:DateTimeField DataField="CreatedDateTime" SortExpression="CreatedDateTime" HeaderText="Created Date Time" />
                            <asp:TemplateField ItemStyle-CssClass="grid-columncommand" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <a href="<%# Eval( "Url" ) %>" title="View" class="btn btn-default btn-sm" target="_blank">
                                        <i class="fa fa-eye"></i>
                                    </a>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <Rock:LinkButtonField CssClass="btn btn-default btn-sm" Text="<i class='fa fa-save'></i>" ToolTip="Save" OnClick="gFilesSave_Click" />
                            <Rock:DeleteField OnClick="gFilesDelete_Click" IconCssClass="fa fa-trash" />
                        </Columns>
                    </Rock:Grid>
                </asp:Panel>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>

<script>
    Sys.Application.add_load(function () {
        $('.js-unused-bulk-save').on('click', function (e) {
            Rock.dialogs.confirmPreventOnCancel(e, 'Are you sure you wish to save all of the selected files so they are no longer seen as unused?');
        });

        $('.js-unused-bulk-delete').on('click', function (e) {
            Rock.dialogs.confirmPreventOnCancel(e, 'Are you sure you wish to remove all of the selected files from the database?');
        });
    });
</script>