<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupMembers.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.GroupMembers" %>

<Rock:RockUpdatePanel ID="upGroupMembers" runat="server">
    <ContentTemplate>
        <div class="persondetails-grouplist js-grouplist-sort-container">

            <asp:Repeater ID="rptrGroups" runat="server" OnItemDataBound="rptrGroups_ItemDataBound" >
                <ItemTemplate>

                    <asp:Panel ID="pnlGroup" runat="server" CssClass="card card-profile card-family-member panel-widget">
                        <asp:HiddenField ID="hfGroupId" runat="server" Value='<%# Eval("Id") %>' />
                            <div class="card-header group-hover">
                                <span class="card-title"><%# FormatAsHtmlTitle(Eval("Name").ToString()) %></span>

                                <div class="panel-labels">
                                    <a id="lReorderIcon" runat="server" class="btn btn-link btn-xs js-stop-immediate-propagation panel-widget-reorder"><i class="fa fa-bars"></i></a>
                                    <asp:HyperLink ID="hlEditGroup" runat="server" AccessKey="O" ToolTip="Alt+O" CssClass="btn btn-default btn-xs btn-square"><i class="fa fa-pencil"></i></asp:HyperLink>
                                </div>
                            </div>

                        <asp:Literal ID="lGroupHeader" runat="server" />

                        <asp:Panel class="card-section pb-0" ID="pnlMembersDiv" runat="server">
                            <div class="family-grid">
                                <asp:Repeater ID="rptrMembers" runat="server" OnItemDataBound="rptrMembers_ItemDataBound">
                                    <ItemTemplate>
                                        <asp:Literal ID="litGroupMemberInfo" runat="server"></asp:Literal>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </asp:Panel>

                        <asp:panel ID="pnlGroupAttributes" runat="server" CssClass="card-section js-group-attribute-section">
                            <asp:Label ID="lblShowGroupAttributeTitle" runat="server" Visible="false" CssClass="pull-left"></asp:Label>
                            <asp:Panel ID="pnlShowExpandChevorn" runat="server" CssClass="pull-right">
                                <a class="js-show-more-family-attributes"><i class="fa fa-chevron-down"></i></a>
                            </asp:Panel>
                            <dl class="m-0">
                                <asp:Literal ID="litGroupAttributes" runat="server"></asp:Literal>
                            </dl>
                            <div class="js-more-group-attributes" style="display:none">
                                <br />
                                <dl class="m-0">
                                    <asp:Literal ID="litMoreGroupAttributes" runat="server"></asp:Literal>
                                </dl>
                            </div>
                        </asp:panel>

                        <asp:Repeater ID="rptrAddresses" runat="server" OnItemDataBound="rptrAddresses_ItemDataBound" OnItemCommand="rptrAddresses_ItemCommand">
                            <HeaderTemplate>
                                <div class="card-section">
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:Literal ID="litAddress" runat="server"></asp:Literal>

                                <div class="group-hover-item group-hover-show ml-auto">
                                    <asp:LinkButton ID="lbVerify" runat="server" CssClass="btn btn-xs btn-square btn-default" CommandName="verify" ToolTip="Verify Address">
                                        <i class="fa fa-globe"></i>
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="lbLocationSettings" runat="server" CssClass="btn btn-xs btn-square btn-default" CommandName="settings" ToolTip="Configure Location">
                                        <i class="fa fa-gear"></i>
                                    </asp:LinkButton>
                                </div>
                                </div><%--This is the ending of the div that begins in litAddress--%>
                            </ItemTemplate>
                            <FooterTemplate>
                                </div>
                            </FooterTemplate>
                        </asp:Repeater>

                        <asp:Literal ID="lGroupFooter" runat="server" />

                    </asp:Panel>

                </ItemTemplate>
            </asp:Repeater>
        </div>

        <script>
            Sys.Application.add_load(function () {
                <%-- This script should only be loaded once per page.
                     Set a global flag during initialization to prevent repeat executions for multiple block instances --%>
                if (typeof window.isGroupMembersBlockInitialized === 'undefined') {
                    window.isGroupMembersBlockInitialized = true;

                    var fixHelper = function (e, ui) {
                        ui.children().each(function () {
                            $(this).width($(this).width());
                        });
                        return ui;
                    };

                    // javascript to make the Reorder buttons work on the panel-widget controls
                    $('.js-grouplist-sort-container').sortable({
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
                                window.location = "javascript:__doPostBack('<%=upGroupMembers.ClientID %>', '" +  postbackArg + "')";
                            }
                        }
                    });

                    $('.js-show-more-family-attributes').on('click', function (e) {
                        var $pnl = $(this).closest('.js-group-attribute-section');
                        var $moreAttributes = $pnl.find('.js-more-group-attributes').first();
                        if ($moreAttributes.is(':visible')) {
                            $moreAttributes.slideUp();
                            $(this).html('<i class="fa fa-chevron-down"></i>');
                        } else {
                            $moreAttributes.slideDown();
                            $(this).html('<i class="fa fa-chevron-up"></i>');
                        }
                    });
                }
            });
        </script>

    </ContentTemplate>
</Rock:RockUpdatePanel>