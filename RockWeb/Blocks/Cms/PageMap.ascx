<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PageMap.ascx.cs" Inherits="RockWeb.Blocks.Cms.PageMap" %>

<asp:UpdatePanel ID="upPanel" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
    <ContentTemplate>

        <asp:HiddenField ID="hfExpandedIds" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfSelectedItemId" runat="server" />

        <div class="treeview js-pagestreeview">

            <div class="treeview-actions rollover-container" id="divTreeviewActions" runat="server">

                <div class="pull-left margin-r-sm">
                    <div class="btn-group ">
                        <button type="button" class="btn btn-action btn-xs dropdown-toggle" data-toggle="dropdown">
                            <i class="fa fa-plus-circle"></i>
                            <asp:Literal ID="ltAddPage" runat="server" Text=" Add Page" />
                            <span class="fa fa-caret-down"></span>
                        </button>
                        <ul class="dropdown-menu" role="menu">
                            <li>
                                <asp:LinkButton ID="lbAddPageRoot" OnClick="lbAddPageRoot_Click" Text="Add Top-Level" runat="server"></asp:LinkButton></li>
                            <li>
                                <asp:LinkButton ID="lbAddPageChild" OnClick="lbAddPageChild_Click" Text="Add Child To Selected" runat="server"></asp:LinkButton></li>
                        </ul>
                    </div>
                </div>

            </div>

            <!-- Keep the treeview hidden to avoid flashing while the RockTree is being rendered -->
            <div class="treeview-scroll scroll-container scroll-container-horizontal js-pages-scroll-container" style="visibility: hidden">

                <div class="viewport">
                    <div class="overview">
                        <div class="panel-body treeview-frame">
                            <asp:Panel ID="pnlTreeviewContent" runat="server">
                                <asp:Literal ID="lPages" runat="server" ViewStateMode="Disabled"></asp:Literal>
                            </asp:Panel>
                        </div>
                    </div>
                </div>

                <div class="scrollbar">
                    <div class="track">
                        <div class="thumb">
                            <div class="end"></div>
                        </div>
                    </div>
                </div>
            </div>

        </div>

        <script type="text/javascript">

            var scrollbPage = $('#<%=pnlTreeviewContent.ClientID%>').closest('.treeview-scroll');
            scrollbPage.tinyscrollbar({ axis: 'x', sizethumb: 60, size: 200 });

            // resize scrollbar when the window resizes
            $(document).ready(function () {
                $(window).on('resize', function () {
                    resizeScrollbar(scrollbPage);
                });
            });

            $(function () {

                var $selectedId = $('#<%=hfSelectedItemId.ClientID%>');
                var $expandedIds = $('#<%=hfExpandedIds.ClientID%>')
                var $pageTree = $('#<%=pnlTreeviewContent.ClientID%>');

                $pageTree.on('rockTree:selected', function (e, id) {

                    var $li = $(this).find('[data-id="' + id + '"]')
                    var rockTree = $(this).data('rockTree');

                    // get the data-id values of rock-tree items that are showing visible children (in other words, Expanded Nodes)
                    var expandedDataIds = $(e.currentTarget).find('.rocktree-children').filter(":visible").closest('.rocktree-item').map(function () {
                        var dataId = $(this).attr('data-id');
                        if (dataId != id) {
                            return dataId;
                        }
                    }).get().join(',');

                    $('#hfExpandedIds').val(expandedDataIds);


                    if ($li.length > 1) {
                        for (i = 0; i < $li.length; i++) {
                            if (!rockTree.selectedNodes[0].name === $li.find('span').text()) {
                                $li = $li[i];
                                break;
                            }
                        }
                    }

                    var itemSearch = '?Page=' + id;

                    if (itemSearch) {
                        var locationUrl = window.location.href.split('?')[0] + itemSearch;
                        locationUrl += "&ExpandedIds=" + encodeURIComponent(expandedDataIds).toLowerCase();
                        if (window.location != locationUrl) {
                            window.location = locationUrl;
                        }
                    }
                });

                $pageTree.on('rockTree:rendered rockTree:expand rockTree:collapse', function () {

                    // update viewport height
                    resizeScrollbar(scrollbPage);
                })

                $pageTree.rockTree({
                    selectedIds: $selectedId.val() ? $selectedId.val().split(',') : null,
                    expandedIds: $expandedIds.val() ? $expandedIds.val().split(',') : null
                });

                $('.js-pages-scroll-container').css("visibility", "visible");
            });

            function resizeScrollbar(scrollControl) {
                var overviewHeight = $(scrollControl).find('.overview').height();

                $(scrollControl).find('.viewport').height(overviewHeight);

                scrollControl.tinyscrollbar_update('relative');
            }
        </script>
    </ContentTemplate>
</asp:UpdatePanel>

