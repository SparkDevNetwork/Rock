<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PageZoneBlocksEditor.ascx.cs" Inherits="RockWeb.Blocks.Cms.PageZoneBlocksEditor" %>
<asp:UpdatePanel ID="upPages" runat="server">
    <ContentTemplate>
        <style>
            .block-config-buttons a i.fa {
                float: left;
                margin-top: 3px;
                margin-right: 10px;
                margin-left: 5px;
                font-size: 18px;
                color: #6a6a6a;
            }
        </style>
        <asp:HiddenField ID="hfPageId" runat="server" />

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lZoneIcon" runat="server" />
                    <asp:Literal ID="lZoneTitle" runat="server" /></h1>

                <div class="pull-right">
                    <Rock:RockDropDownList ID="ddlZones" runat="server" Label="" AutoPostBack="true" OnSelectedIndexChanged="ddlZones_SelectedIndexChanged" />
                </div>
            </div>
            <div class="panel-body">
                <legend>Blocks From Layout</legend>
                <asp:Repeater ID="rptLayoutBlocks" runat="server" OnItemDataBound="rptPageOrLayoutBlocks_ItemDataBound">
                    <ItemTemplate>
                        <asp:Panel ID="pnlBlockEditWidget" runat="server" CssClass="panel panel-widget">
                        </asp:Panel>
                    </ItemTemplate>
                </asp:Repeater>

                <legend>Blocks From Page</legend>
                <asp:Repeater ID="rptPageBlocks" runat="server" OnItemDataBound="rptPageOrLayoutBlocks_ItemDataBound">
                    <ItemTemplate>
                        <asp:Panel ID="pnlBlockEditWidget" runat="server" CssClass="panel panel-widget">
                        </asp:Panel>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </asp:Panel>

        <script>

            $(document).ready(function () {

                $(function () {
                    debugger
                    // Bind the block instance delete anchor (ensure it is only bound once)
                    $('.block-config-buttons a.block-delete').off('click').on('click', function (a, b, c) {
                        var blockId = $(this).attr('href');

                        Rock.dialogs.confirm('Are you sorta sure you want to delete this block?', function (result) {

                            if (result) {

                                // delete the block instance
                                $.ajax({
                                    type: 'DELETE',
                                    contentType: 'application/json',
                                    dataType: 'json',
                                    url: Rock.settings.get('baseUrl') + 'api/blocks/' + blockId,
                                    success: function (data, status, xhr) {

                                        // Remove the block instance's container div
                                        $('#bid_' + blockId).remove();

                                    },
                                    error: function (xhr, status, error) {
                                        alert(status + ' [' + error + ']: ' + xhr.responseText);
                                    }
                                });
                            }

                        });

                        // Cancel the default action of the delete anchor tag
                        return false;

                    });

                    // Bind the click event of the block move anchor tag
                    $('.block-config-buttons a.block-move').off('click').on('click', function () {

                        // Get a reference to the anchor tag for use in the dialog success function
                        $moveLink = $(this);

                        // Set the dialog's zone selection select box value to the block's current zone 
                        $('#block-move-zone').val($(this).attr('data-zone'));

                        // Set the dialog's parent option to the current zone's parent (either the page or the layout)
                        var pageBlock = $(this).attr('data-zone-location') == 'Page';
                        $('#block-move-Location_0').prop('checked', pageBlock);
                        $('#block-move-Location_1').prop('checked', !pageBlock);

                        // Show the popup block move dialog
                        $('.js-modal-block-move .modal').modal('show');

                        return false;

                    });
                }
                )
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>

