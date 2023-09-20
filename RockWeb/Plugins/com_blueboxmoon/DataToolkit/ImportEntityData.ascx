<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ImportEntityData.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.DataToolkit.ImportEntityData" %>

<style>
    th.minimum-width,
    td.minimum-width {
        width: 1%;
        white-space: nowrap;
        padding-right: 30px !important;
    }
</style>

<script src="/SignalR/hubs"></script>
<script type="text/javascript">
    $(function () {
        var proxy = $.connection.rockMessageHub;

        proxy.client.importProgress = function (completed, total) {
            var $bar = $('#<%= upPanel.ClientID %> .js-import-progress-bar');

            $bar.prop('aria-valuenow', completed);
            $bar.prop('aria-valuemax', total);
            $bar.css('width', (completed.replace(',','') / total.replace(',','') * 100) + '%');
            $bar.text(completed + '/' + total);

            $('#<%= pnlProgress.ClientID %>').show();
            $('#<%= upPanel.ClientID %> .js-hide-details').fadeIn();
        };

        proxy.client.importStatus = function (status, success) {
            var $pnlStatus = $('#<%= pnlStatus.ClientID %>');

            $pnlStatus.find('.js-status-text').text(status);

            if (success != true) {
                $pnlStatus.addClass('alert-danger').removeClass('alert-success');
            }
            else {
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
                <h1 class="panel-title"><i class="fa fa-download"></i> Import Entity Data</h1>
            </div>

            <div class="panel-body">
                <asp:Panel ID="pnlDetails" runat="server" style="position: relative;">
                    <div class="js-hide-details" style="position: absolute; top: -15px; left: -15px; right: -15px; bottom: -15px; background-color: rgba(0, 0, 0, 0.7); z-index: 10; display: none;"></div>
                    <Rock:NotificationBox ID="nbValidation" runat="server" NotificationBoxType="Danger" />

                    <div class="row">
                        <div class="col-md-4">
                            <Rock:FileUploader ID="fupData" runat="server" Label="Import File" Help="The JSON or CSV file that contains the data to be imported." OnFileUploaded="fupData_FileUploaded" OnFileRemoved="fupData_FileRemoved" />
                        </div>
                    </div>

                    <asp:Panel ID="pnlImportOptions" runat="server" Visible="false">
                        <ul class="nav nav-pills margin-b-md">
                            <li id="liSettings" runat="server" role="presentation">
                                <asp:LinkButton ID="lbSettings" runat="server" Text="Settings" OnClick="lbTab_Click" CausesValidation="false" />
                            </li>
                            <li id="liSample" runat="server" role="presentation">
                                <asp:LinkButton ID="lbSample" runat="server" Text="Sample" OnClick="lbTab_Click" CausesValidation="false" />
                            </li>
                        </ul>

                        <asp:Panel ID="pnlSettings" runat="server" Visible="false">
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
                                    <Rock:EntityTypePicker ID="etEntityType" runat="server" Required="true" Label="Entity Type" Help="The type of entity data to be imported." OnSelectedIndexChanged="etEntityType_SelectedIndexChanged" AutoPostBack="true" />
                                </div>
                                <div class="col-md-2"></div>
                            </div>

                            <Rock:Grid ID="gProperties" runat="server" AllowSorting="false" AllowPaging="false" OnRowDataBound="gProperties_RowDataBound">
                                <Columns>
                                    <Rock:RockBoundField HeaderText="Property Name" DataField="Name" HeaderStyle-CssClass="minimum-width" ItemStyle-CssClass="minimum-width" />
                                    <Rock:RockTemplateField HeaderText="Lava Template">
                                        <ItemTemplate>
                                            <Rock:CodeEditor ID="ceLavaTemplate" runat="server" EditorHeight="100" EditorMode="Lava" />
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
                                </Columns>
                            </Rock:Grid>

                            <asp:Panel ID="pnlActions" runat="server" Visible="false" CssClass="actions margin-t-md">
                                <asp:LinkButton ID="lbImport" runat="server" Text="Import" CssClass="btn btn-primary" OnClick="lbImport_Click" />
                                <asp:LinkButton ID="lbTestImport" runat="server" Text="Test Import" CssClass="btn btn-default margin-l-sm" OnClick="lbTestImport_Click" />
                                <asp:LinkButton ID="lbPreview" runat="server" Text="Preview" CssClass="btn btn-default margin-l-sm" OnClick="lbPreview_Click" />
                                <asp:LinkButton ID="lbSaveAsPreset" runat="server" Text="Save as Preset" CssClass="btn btn-default margin-l-lg" OnClick="lbSaveAsPreset_Click" />
                            </asp:Panel>
                        </asp:Panel>

                        <asp:Panel ID="pnlSample" runat="server" Visible="false">
                            <Rock:Grid ID="gSample" runat="server" AutoGenerateColumns="false" AllowSorting="false" AllowPaging="false" OnGridRebind="gSample_GridRebind" />
                        </asp:Panel>
                    </asp:Panel>
                </asp:Panel>

                <asp:Panel ID="pnlProgress" runat="server" CssClass="margin-t-lg" style="display: none;">
                    <strong>Progress</strong><br />
                    <div class="progress">
                        <div class="progress-bar js-import-progress-bar" role="progressbar" style="width: 0%" aria-valuenow="0" aria-valuemax="0">0/0</div>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlPreview" runat="server" Visible="false" CssClass="margin-t-lg">
                    <Rock:Grid ID="gPreview" runat="server" AllowSorting="false" AllowPaging="false" />
                </asp:Panel>

                <asp:Panel ID="pnlStatus" runat="server" CssClass="alert alert-success margin-t-md margin-b-sm" style="display: none;">
                    <pre class="js-status-text"></pre>
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
