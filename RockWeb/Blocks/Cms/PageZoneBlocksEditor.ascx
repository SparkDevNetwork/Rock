<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PageZoneBlocksEditor.ascx.cs" Inherits="RockWeb.Blocks.Cms.PageZoneBlocksEditor" %>
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

                <div class="pull-right">
                    <div class="input-group input-group-sm">
                        <Rock:RockDropDownList ID="ddlZones" runat="server" Label="" AutoPostBack="true" OnSelectedIndexChanged="ddlZones_SelectedIndexChanged" />
                    </div>
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
                        <asp:LinkButton ID="btnAddBlock" runat="server" ToolTip="Add Block" Text="<i class='fa fa-plus'></i>" CssClass="btn btn-default btn-sm" OnClick="btnAddBlock_Click" />
                    </div>

                </div>
            </div>

            <%--  This will hold blocks that need to be added to the page so that Custom Admin actions will work --%>
            <%-- Display -9999 offscreen. This will hopefully hide everything except for any modals that get shown with the Custom Action --%>
            <asp:Panel ID="pnlBlocksHolder" runat="server" Style="position: absolute; left: -9999px">
            </asp:Panel>

        </asp:Panel>

        <Rock:ModalDialog ID="mdBlockMove" runat="server" ValidationGroup="vgBlockMove" OnSaveClick="mdBlockMove_SaveClick" Title="Move Block">
            <Content>
                <asp:HiddenField ID="hfBlockMoveBlockId" runat="server" />
                <legend>New Location</legend>
                <Rock:RockDropDownList ID="ddlMoveToZoneList" runat="server" Label="Zone" />
                <Rock:RockRadioButtonList ID="cblBlockMovePageOrLayout" runat="server" Label="Parent" RepeatDirection="Horizontal" />

            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="mdAddBlock" runat="server" ValidationGroup="vgAddBlock" OnSaveClick="mdAddBlock_SaveClick" Title="Add Block">
            <Content>
                <asp:ValidationSummary ID="vsAddBlock" runat="server" ValidationGroup="vgAddBlock" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                <Rock:RockTextBox ID="tbNewBlockName" runat="server" Label="Name" Required="true" ValidationGroup="vgAddBlock" />
                
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlBlockType" runat="server" Label="Type" AutoPostBack="true" OnSelectedIndexChanged="ddlBlockType_SelectedIndexChanged" />
                    </div>
                    <div class="col-md-6 padding-t-md">
                        <label>Common Block Types</label><br />
                        <asp:Repeater ID="rptCommonBlockTypes" runat="server" OnItemDataBound="rptCommonBlockTypes_ItemDataBound">
                            <ItemTemplate>
                                <asp:LinkButton ID="btnNewBlockQuickSetting" runat="server" Text="Todo" CssClass="btn btn-default btn-xs" OnClick="btnNewBlockQuickSetting_Click" />
                            </ItemTemplate>
                        </asp:Repeater>
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
                            __doPostBack('<%=upPages.ClientID %>', 're-order-panel-widget:' + ui.item.attr('id') + ';' + newItemIndex);
                        }
                    }
                });
            });
        </script>

    </ContentTemplate>
</asp:UpdatePanel>

