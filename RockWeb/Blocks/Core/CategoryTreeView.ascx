﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CategoryTreeView.ascx.cs" Inherits="RockWeb.Blocks.Core.CategoryTreeView" %>

<asp:UpdatePanel ID="upCategoryTree" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
    <ContentTemplate>

        <asp:HiddenField ID="hfInitialCategoryParentIds" runat="server" />
        <asp:HiddenField ID="hfSelectedItemId" runat="server" />
        <asp:HiddenField ID="hfPageRouteTemplate" runat="server" />
        <asp:HiddenField ID="hfDetailPageUrl" runat="server" />

        <div class="treeview js-categorytreeview">

            <div class="treeview-actions rollover-container" id="divTreeviewActions" runat="server">

                <div class="pull-left margin-r-sm">
                    <div class="btn-group ">
                        <button type="button" class="btn btn-action btn-xs dropdown-toggle" data-toggle="dropdown">
                            <i class="fa fa-plus-circle"></i>
                            <asp:Literal ID="ltAddCategory" runat="server" Text=" Add Category" />
                            <span class="fa fa-caret-down"></span>
                        </button>
                        <ul class="dropdown-menu" role="menu">
                            <li>
                                <asp:LinkButton ID="lbAddCategoryRoot" OnClick="lbAddCategoryRoot_Click" Text="Add Top-Level" runat="server"></asp:LinkButton></li>
                            <li>
                                <asp:LinkButton ID="lbAddCategoryChild" OnClick="lbAddCategoryChild_Click" Text="Add Child To Selected" runat="server"></asp:LinkButton></li>
                        </ul>
                    </div>

                    <asp:LinkButton ID="lbAddItem" runat="server" CssClass="add btn btn-xs btn-action" ToolTip="Add Item" CausesValidation="false" OnClick="lbAddItem_Click">
                        <i class="fa fa-plus-circle"></i>
                        <asp:Literal ID="lAddItem" runat="server" Text="Add Item" />
                    </asp:LinkButton>
                </div>

                <div class="rollover-item" id="pnlRolloverConfig" runat="server">
                    <i class="fa fa-gear clickable js-show-config" onclick="$(this).closest('.js-categorytreeview').find('.js-config-panel').slideToggle()"></i>
                </div>

            </div>

            <div class="js-config-panel" style="display: none" id="pnlConfigPanel" runat="server">
                <Rock:Toggle ID="tglHideInactiveItems" runat="server" OnText="Active" OffText="All" Checked="true" ButtonSizeCssClass="btn-xs" OnCheckedChanged="tglHideInactiveItems_CheckedChanged" Label="Show" />
            </div>

            <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" />
            
            <div class="treeview-scroll scroll-container scroll-container-horizontal">

                <div class="viewport">
                    <div class="overview">
                        <div class="panel-body treeview-frame">
                            <asp:Panel ID="pnlTreeviewContent" runat="server" />
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

            var scrollbCategory = $('#<%=pnlTreeviewContent.ClientID%>').closest('.treeview-scroll');
            scrollbCategory.tinyscrollbar({ axis: 'x', sizethumb: 60, size: 200 });

            // resize scrollbar when the window resizes
            $(document).ready(function () {
                $(window).on('resize', function () {
                    resizeScrollbar(scrollbCategory);
                });
            });

            // scrollbar hide/show
            var timerScrollHide;
            $("#<%=upCategoryTree.ClientID%>").on({
                mouseenter: function () {
                    clearTimeout(timerScrollHide);
                    $("[id$='upCategoryTree'] div[class~='scrollbar'] div[class='track'").fadeIn('fast');
                },
                mouseleave: function () {
                    timerScrollHide = setTimeout(function () {
                        $("[id$='upCategoryTree'] div[class~='scrollbar'] div[class='track'").fadeOut('slow');
                    }, 1000);
                }
            });

            if ('<%= RestParms %>' == '') {
                // EntityType not set
                $('#<%=pnlTreeviewContent.ClientID%>').hide();
            }

            $(function () {
                var $selectedId = $('#<%=hfSelectedItemId.ClientID%>'),
                    $expandedIds = $('#<%=hfInitialCategoryParentIds.ClientID%>'),
                    _mapCategories = function (arr) {
                        return $.map(arr, function (item) {
                            var node = {
                                id: item.Guid || item.Id,
                                name: item.Name || item.Title,
                                iconCssClass: item.IconCssClass,
                                parentId: item.ParentId,
                                hasChildren: item.HasChildren,
                                isCategory: item.IsCategory,
                                isActive: item.IsActive,
                                entityId: item.Id
                            };

                            // If this Tree Node represents a Category, add a prefix to its identifier to prevent collisions with other Entity identifiers.
                            if (item.IsCategory) {
                                node.id = '<%= CategoryNodePrefix %>' + node.id;
                            }
                            if (item.Children && typeof item.Children.length === 'number') {
                                node.children = _mapCategories(item.Children);
                            }

                            return node;
                        });
                    };

                $('#<%=pnlTreeviewContent.ClientID%>')
                    .on('rockTree:selected', function (e, id) {

                        var $node = $('[data-id="' + id + '"]'),
                            isCategory = $node.attr('data-iscategory') === 'true',
                            urlParameter = (isCategory ? 'CategoryId' : '<%= PageParameterName %>');

                        // Get the id of the Entity represented by this Tree Node if it has been specified as an attribute.
                        // If not, assume the id of the Entity is the same as the Node.
                        var entityId = $node.attr('data-entityid') || id;

                        var itemSearch = '?' + urlParameter + '=' + entityId;

                        var currentItemId = $selectedId.val();

                        if (currentItemId !== id) {
                            // get the data-id values of rock-tree items that are showing visible children (in other words, Expanded Nodes)
                            var expandedDataIds = $(e.currentTarget).find('.rocktree-children').filter(":visible").closest('.rocktree-item').map(function () {
                                return $(this).attr('data-id')
                            }).get().join(',');

                            var pageRouteTemplate = $('#<%=hfPageRouteTemplate.ClientID%>').val();
                            var locationUrl = "";
                            var regex = new RegExp("{" + urlParameter + "}", "i");

                            if (pageRouteTemplate.match(regex)) {
                                locationUrl = Rock.settings.get('baseUrl') + pageRouteTemplate.replace(regex, entityId);
                                locationUrl += "?ExpandedIds=" + encodeURIComponent(expandedDataIds);
                            }
                            else {
                                var detailPageUrl = $('#<%=hfDetailPageUrl.ClientID%>').val();
                                if (detailPageUrl) {
                                    locationUrl = detailPageUrl + itemSearch;
                                }
                                else {
                                    locationUrl = window.location.href.split('?')[0] + itemSearch;
                                }

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
                            restUrl: '<%= ResolveUrl( "~/api/categories/getchildren/" ) %>',
                            restParams: '<%= RestParms %>',
                            mapping: {
                                include: ['isCategory', 'entityId'],
                                mapData: _mapCategories
                            },
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

<asp:UpdatePanel ID="upCategoryTreeConfig" runat="server">
    <ContentTemplate>
        <Rock:ModalDialog ID="mdCategoryTreeConfig" runat="server" OnSaveClick="mdCategoryTreeConfig_SaveClick">
            <Content>
                <Rock:NotificationBox ID="nbRootCategoryEntityTypeWarning" runat="server" Text="Entity Type must be set in Block Settings before setting Root Category." NotificationBoxType="Warning" />
                <Rock:CategoryPicker ID="cpRootCategory" runat="server" Label="Root Category" />

                <Rock:CategoryPicker ID="cpExcludeCategories" runat="server" Label="Exclude Categories" AllowMultiSelect="true" />
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
