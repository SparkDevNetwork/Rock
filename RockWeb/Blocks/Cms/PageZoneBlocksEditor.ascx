<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PageZoneBlocksEditor.ascx.cs" Inherits="RockWeb.Blocks.Cms.PageZoneBlocksEditor" %>
<script type="text/javascript">
    function clearDialog() {
        $('#rock-config-cancel-trigger').trigger('click');
    }
</script>
<asp:UpdatePanel ID="upPages" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfPageId" runat="server" />

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <div class="pull-left">
                    <h1 class="panel-title">
                    <asp:Literal ID="lZoneIcon" runat="server" />
                    <asp:Literal ID="lZoneTitle" runat="server" /></h1>
                </div>

                <Rock:HighlightLabel ID="hlInvalidZoneWarning" runat="server" LabelType="Danger" CssClass="margin-l-md" Text="Invalid Zone" ToolTip="This zone is no longer part of the zones for this layout." Visible="false" />

                <div class="panel-labels">
                    <Rock:RockDropDownList ID="ddlZones" runat="server" Label="" AutoPostBack="true" OnSelectedIndexChanged="ddlZones_SelectedIndexChanged" CssClass="input-xs" />
                </div>
            </div>
            <div class="panel-body">
                <legend>Blocks From Site</legend>
                <div class="site-blocks-sort-container">
                    <asp:Repeater ID="rptSiteBlocks" runat="server" OnItemDataBound="rptBlocks_ItemDataBound">
                        <ItemTemplate>

                            <asp:Panel ID="pnlBlockEditWidget" runat="server" CssClass="panel panel-widget">
                                <asp:HiddenField ID="hfSiteBlockId" runat="server" Value='<%# Eval("Id") %>' />
                            </asp:Panel>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

                <hr />
                <legend>Blocks From Layout</legend>
                <div class="layout-blocks-sort-container">
                    <asp:Repeater ID="rptLayoutBlocks" runat="server" OnItemDataBound="rptBlocks_ItemDataBound">
                        <ItemTemplate>

                            <asp:Panel ID="pnlBlockEditWidget" runat="server" CssClass="panel panel-widget">
                                <asp:HiddenField ID="hfLayoutBlockId" runat="server" Value='<%# Eval("Id") %>' />
                            </asp:Panel>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

                <hr />

                <legend>Blocks From Page</legend>
                <div class="page-blocks-sort-container">
                    <asp:Repeater ID="rptPageBlocks" runat="server" OnItemDataBound="rptBlocks_ItemDataBound">
                        <ItemTemplate>

                            <asp:Panel ID="pnlBlockEditWidget" runat="server" CssClass="panel panel-widget">
                                <asp:HiddenField ID="hfPageBlockId" runat="server" Value='<%# Eval("Id") %>' />
                            </asp:Panel>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

                <div class="actions ">
                    <div class="pull-right">
                        <asp:LinkButton ID="btnAddBlock" runat="server" Text="<i class='fa fa-plus-circle'></i> Add Block to Zone" CssClass="btn btn-default btn-sm" OnClick="btnAddBlock_Click" />
                    </div>

                </div>
            </div>

            <%--  This will hold blocks that need to be added to the page so that Custom Admin actions will work --%>
            <%-- Display -9999 offscreen. This will hopefully hide everything except for any modals that get shown with the Custom Action --%>
            <asp:Panel ID="pnlBlocksHolderDiv" runat="server" Style="position: absolute; left: -9999px">
                <Rock:DynamicPlaceholder ID="phBlockHolder" runat="server" />
            </asp:Panel>

        </asp:Panel>

        <Rock:ModalDialog ID="mdBlockMove" runat="server" ValidationGroup="vgBlockMove" OnSaveClick="mdBlockMove_SaveClick" Title="Move Block">
            <Content>
                <asp:HiddenField ID="hfBlockMoveBlockId" runat="server" />
                <legend>New Location</legend>
                <Rock:RockDropDownList ID="ddlMoveToZoneList" runat="server" Label="Zone" />
                <Rock:RockRadioButtonList ID="cblBlockMovePageLayoutOrSite" runat="server" Label="Parent" RepeatDirection="Horizontal" />

            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="mdAddBlock" runat="server" ValidationGroup="vgAddBlock" OnSaveClick="mdAddBlock_SaveClick" Title="Add Block">
            <Content>
                <asp:ValidationSummary ID="vsAddBlock" runat="server" ValidationGroup="vgAddBlock" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <Rock:RockTextBox ID="tbNewBlockName" runat="server" Label="Name" Required="true" ValidationGroup="vgAddBlock" />

                <div class="row">
                    <div class="col-sm-6">
                        <Rock:RockDropDownList ID="ddlBlockType" runat="server" Label="Type" AutoPostBack="true" OnSelectedIndexChanged="ddlBlockType_SelectedIndexChanged" EnhanceForLongLists="true" />
                    </div>
                    <div class="col-sm-6">
                        <Rock:RockControlWrapper ID="rcwCommonBlockTypes" runat="server" Label="Common Block Types">
                            <asp:Panel ID="pnlCommonBlockTypes" runat="server">
                                <asp:Repeater ID="rptCommonBlockTypes" runat="server" OnItemDataBound="rptCommonBlockTypes_ItemDataBound">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="btnNewBlockQuickSetting" runat="server" Text="Todo" CssClass="btn btn-default btn-xs" OnClick="btnNewBlockQuickSetting_Click" />
                                    </ItemTemplate>
                                </asp:Repeater>
                            </asp:Panel>
                        </Rock:RockControlWrapper>
                    </div>
                </div>

                <Rock:RockRadioButtonList ID="rblAddBlockLocation" runat="server" Label="Add To" RepeatDirection="Horizontal" FormGroupCssClass="margin-t-md" />

            </Content>
        </Rock:ModalDialog>

        <script>
            Sys.Application.add_load(function () {

                var fixHelper = function (e, ui) {
                    ui.children().each(function () {
                        $(this).width($(this).width());
                    });
                    return ui;
                };

                // javascript to make the Reorder buttons work on the panel-widget controls
                $('.site-blocks-sort-container, .layout-blocks-sort-container, .page-blocks-sort-container').sortable({
                    helper: fixHelper,
                    handle: '.panel-widget-reorder',
                    containment: 'parent',
                    tolerance: 'pointer',
                    start: function (event, ui) {
                        {
                            var start_pos = ui.item.index();
                            ui.item.data('start_pos', start_pos);
                        }
                    },
                    update: function (event, ui) {
                        {
                            var newItemIndex = $(ui.item).prevAll('.panel-widget').length;
                            var postbackArg = 're-order-panel-widget:' + ui.item.attr('id') + ';' + newItemIndex;
                            window.location = "javascript:__doPostBack('<%=upPages.ClientID %>', '" +  postbackArg + "')";
                        }
                    }
                });
            });
        </script>

    </ContentTemplate>
</asp:UpdatePanel>

