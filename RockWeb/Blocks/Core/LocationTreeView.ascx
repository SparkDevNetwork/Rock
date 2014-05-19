<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LocationTreeView.ascx.cs" Inherits="RockWeb.Blocks.Core.LocationTreeView" %>

<asp:UpdatePanel ID="upLocationType" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
    <ContentTemplate>
        <asp:HiddenField ID="hfRootLocationId" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfInitialLocationId" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfInitialLocationParentIds" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfSelectedLocationId" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfPageRouteTemplate" runat="server" ClientIDMode="Static" />

        <div class="treeview">
            <div class="treeview-actions" id="divTreeviewActions" runat="server">
                
                <div class="btn-group">
                    <button type="button" class="btn btn-action btn-xs dropdown-toggle" data-toggle="dropdown">
                        <i class="fa fa-plus-circle"></i> Add Location <span class="fa fa-caret-down"></span>
                    </button>
                    <ul class="dropdown-menu" role="menu">
                        <li><asp:LinkButton ID="lbAddLocationRoot" OnClick="lbAddLocationRoot_Click" Text="Add Top-Level" runat="server"></asp:LinkButton></li>
                        <li><asp:LinkButton ID="lbAddLocationChild" OnClick="lbAddLocationChild_Click" Enabled="false" Text="Add Child To Selected" runat="server"></asp:LinkButton></li>
                    </ul>
                </div>

            </div>

            <div class="treeview-scroll scroll-container scroll-container-horizontal">
                
                <div class="viewport">
                    <div class="overview">
                        <div class="panel-body treeview-frame">
                            <div id="treeview-content">
                            </div>
                        </div>
                    
                    </div>
                </div>
                <div class="scrollbar"><div class="track"><div class="thumb"><div class="end"></div></div></div></div>
            </div>
        </div>


        <script type="text/javascript">
            $(function () {
                var $selectedId = $('#hfSelectedLocationId'),
                    $expandedIds = $('#hfInitialLocationParentIds');

                var scrollbCategory = $('.treeview-scroll');
                scrollbCategory.tinyscrollbar({ axis: 'x', sizethumb: 60, size: 200 });

                // resize scrollbar when the window resizes
                $(document).ready(function () {
                    $(window).on('resize', function () {
                        resizeScrollbar(scrollbCategory);
                    });
                });

                $('#treeview-content')
                    .on('rockTree:selected', function (e, id) {
                        var locationSearch = '?LocationId=' + id;
                        var currentItemId = $selectedId.val();

                        if (currentItemId !== id) {

                            // get the data-id values of rock-tree items that are showing visible children (in other words, Expanded Nodes)
                            var expandedDataIds = $(e.currentTarget).find('.rocktree-children').filter(":visible").closest('.rocktree-item').map(function () {
                                return $(this).attr('data-id')
                            }).get().join(',');

                            var pageRouteTemplate = $('#hfPageRouteTemplate').val();
                            var locationUrl = "";
                            if (pageRouteTemplate.match(/{locationId}/i)) {
                                locationUrl = Rock.settings.get('baseUrl') + pageRouteTemplate.replace(/{locationId}/i, id);
                                locationUrl += "?ExpandedIds=" + encodeURIComponent(expandedDataIds);
                            }
                            else {
                                locationUrl = window.location.href.split('?')[0] + locationSearch;
                                locationUrl += "&ExpandedIds=" + encodeURIComponent(expandedDataIds);
                            }

                            window.location = locationUrl;
                        }
                    })
                    .on('rockTree:rendered', function () {

                        // update viewport height
                        resizeScrollbar(scrollbCategory);

                    })
                    .rockTree({
                        restUrl: '<%=ResolveUrl( "~/api/Locations/getchildren/" ) %>',
                        restParams: '/' + ($('#hfRootLocationId').val() || 0),
                        multiSelect: false,
                        selectedIds: $selectedId.val() ? $selectedId.val().split(',') : null,
                        expandedIds: $expandedIds.val() ? $expandedIds.val().split(',') : null
                    });
            });

            function resizeScrollbar(scrollControl) {
                var overviewHeight = $(scrollControl).find('.overview').height();

                $(scrollControl).find('.viewport').height(overviewHeight);

                scrollControl.tinyscrollbar_update('relative');
            }
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
