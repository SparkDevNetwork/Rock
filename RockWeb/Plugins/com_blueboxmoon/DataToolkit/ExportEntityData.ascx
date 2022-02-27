<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ExportEntityData.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.DataToolkit.ExportEntityData" %>

<style>
    pre.auto-hide:empty {
        display: none;
    }
</style>

<script src="/SignalR/hubs"></script>
<script type="text/javascript">
    $(function () {
        var proxy = $.connection.rockMessageHub;

        proxy.client.exportProgress = function (completed, total) {
            var $bar = $('#<%= upPanel.ClientID %> .js-export-progress-bar');

            $bar.prop('aria-valuenow', completed);
            $bar.prop('aria-valuemax', total);
            $bar.css('width', (completed.replace(',','') / total.replace(',','') * 100) + '%');
            $bar.text(completed + '/' + total);

            $('#<%= pnlProgress.ClientID %>').show();
            $('#<%= upPanel.ClientID %> .js-hide-details').fadeIn();
        };

        proxy.client.exportStatus = function (status, url) {
            var $pnlStatus = $('#<%= pnlStatus.ClientID %>');

            $pnlStatus.find('.js-status-text').text(status);

            if (url == null) {
                $pnlStatus.find('.js-download-button').hide();
                $pnlStatus.addClass('alert-danger').removeClass('alert-success');
            }
            else {
                $pnlStatus.find('.js-download-button').show();
                $pnlStatus.find('.js-download-button a').attr('href', url);
                $pnlStatus.addClass('alert-success').removeClass('alert-danger');
            }

            $('#<%= pnlProgress.ClientID %>').hide();
            $pnlStatus.show();
            $('#<%= upPanel.ClientID %> .js-hide-details').fadeOut();
        };

        $.connection.hub.start().done(function () {
            $('#<%= hfConnectionId.ClientID %>').val($.connection.hub.id);
        });
    });
</script>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfConnectionId" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-upload"></i> Export Entity Data</h1>
            </div>

            <div class="panel-body">
                <asp:Panel ID="pnlDetails" runat="server" style="position: relative;">
                    <div class="js-hide-details" style="position: absolute; top: -15px; left: -15px; right: -15px; bottom: -15px; background-color: rgba(0, 0, 0, 0.7); z-index: 10; display: none;"></div>
                    <Rock:NotificationBox ID="nbValidation" runat="server" NotificationBoxType="Danger" />

                    <Rock:RockControlWrapper ID="rcwPreset" runat="server" Label="Preset">
                        <div class="row">
                            <div class="col-lg-4 col-md-6">
                                <Rock:RockDropDownList ID="ddlPreset" runat="server" />
                            </div>

                            <div class="col-lg-6 col-md-6">
                                <asp:LinkButton ID="lbLoadPreset" runat="server" CssClass="btn btn-primary" Text="Load" OnClick="lbLoadPreset_Click" CausesValidation="false" />
                                <asp:LinkButton ID="lbDeletePreset" runat="server" CssClass="btn btn-danger margin-l-sm" Text="Delete" OnClick="lbDeletePreset_Click" CausesValidation="false" OnClientClick="Rock.dialogs.confirmDelete(event, 'preset');" />
                            </div>
                        </div>
                    </Rock:RockControlWrapper>

                    <div class="row">
                        <div class="col-lg-4 col-md-6">
                            <Rock:EntityTypePicker ID="etEntityType" runat="server" Label="Entity Type" Help="The type of entity data to be exported." OnSelectedIndexChanged="etEntityType_SelectedIndexChanged" AutoPostBack="true" />
                        </div>
                    </div>

                    <asp:Panel ID="pnlExportOptions" runat="server" Visible="false">
                        <div class="row">
                            <div class="col-lg-4 col-md-6">
                                <Rock:DataViewItemPicker ID="dvFilter" runat="server" Label="DataView Filter" Help="If you want to filter the data that is exported you may select a DataView to filter those results." />
                            </div>

                            <div class="col-lg-4 col-md-6">
                                <Rock:Toggle ID="tglExportCsv" runat="server" Label="Export As" OnText="CSV" OffText="JSON" Checked="true" OnCssClass="btn-info" OffCssClass="btn-info" OnCheckedChanged="tglExportCsv_CheckedChanged" />
                            </div>
                        </div>
                    </asp:Panel>

                    <Rock:Grid ID="gColumns" runat="server" AllowSorting="false" AllowPaging="false" OnRowDataBound="gColumns_RowDataBound" OnGridReorder="gColumns_GridReorder">
                        <Columns>
                            <Rock:ReorderField HeaderStyle-Width="60" ItemStyle-Width="60" />
                            <Rock:RockTemplateField HeaderText="Column Name" HeaderStyle-Width="250" ItemStyle-Width="250">
                                <ItemTemplate>
                                    <Rock:RockTextBox ID="tbColumnName" runat="server" />
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockTemplateField HeaderText="Lava Template">
                                <ItemTemplate>
                                    <Rock:CodeEditor ID="ceLavaTemplate" runat="server" EditorMode="Lava" EditorHeight="100" />
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:DeleteField OnClick="gColumns_DeleteClick" HeaderStyle-Width="60" ItemStyle-Width="60" />
                        </Columns>
                    </Rock:Grid>

                    <asp:Panel ID="pnlActions" runat="server" Visible="false" CssClass="actions margin-t-md">
                        <asp:LinkButton ID="lbExport" runat="server" Text="Export" CssClass="btn btn-primary" OnClick="lbExport_Click" />
                        <asp:LinkButton ID="lbPreview" runat="server" Text="Preview" CssClass="btn btn-default margin-l-sm" OnClick="lbPreview_Click" />
                        <asp:LinkButton ID="lbSaveAsPreset" runat="server" Text="Save as Preset" CssClass="btn btn-default margin-l-sm" OnClick="lbSaveAsPreset_Click" />
                    </asp:Panel>
                </asp:Panel>

                <asp:Panel ID="pnlProgress" runat="server" CssClass="margin-t-lg" style="display: none;">
                    <strong>Progress</strong><br />
                    <div class="progress">
                        <div class="progress-bar js-export-progress-bar" role="progressbar" style="width: 0%" aria-valuenow="0" aria-valuemax="0">0/0</div>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlPreview" runat="server" Visible="false" CssClass="margin-t-lg">
                    <Rock:Grid ID="gPreview" runat="server" AllowSorting="false" AllowPaging="false" />
                    <pre class="auto-hide"><asp:Literal ID="ltPreviewJson" runat="server" /></pre>
                </asp:Panel>

                <asp:Panel ID="pnlStatus" runat="server" CssClass="alert alert-success margin-t-md margin-b-sm" style="display: none;">
                    <p class="js-status-text"></p>
                    <p class="js-download-button" style="display: none;">
                        <a href="#" class="btn btn-success">Download</a>
                    </p>
                </asp:Panel>
            </div>
        </div>

        <Rock:ModalDialog ID="mdSaveAsPreset" runat="server" OnSaveClick="mdSaveAsPreset_SaveClick" ValidationGroup="SaveAsPreset">
            <Content>
                <Rock:RockTextBox ID="tbSaveAsPresetName" runat="server" Label="Preset Name" Required="true" ValidationGroup="SaveAsPreset" />
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
