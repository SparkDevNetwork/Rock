<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CategoryTreeView.ascx.cs" Inherits="RockWeb.Blocks.Core.CategoryTreeView" %>

<asp:UpdatePanel ID="upCategoryTree" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
    <ContentTemplate>
        <asp:HiddenField ID="hfInitialEntityIsCategory" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfInitialItemId" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfInitialCategoryParentIds" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfSelectedCategoryId" runat="server" ClientIDMode="Static" />
        <div class="treeview treeview-group panel panel-treeview">
            
            <div class="panel-heading">
                <h3 class="panel-title"><asp:Literal ID="lTitle" runat="server"></asp:Literal></h3>

            
            
            <div class="treeview-actions row">
                <div class="col-xs-6 col">
                    <asp:LinkButton ID="lbAddCategory" runat="server" CssClass="add btn btn-mini" ToolTip="Add Category" CausesValidation="false" OnClick="lbAddCategory_Click">
                            <i class="fa fa-plus"></i> Category
                    </asp:LinkButton>
                </div>
                <div class="col-xs-6 col">
                    <asp:LinkButton ID="lbAddItem" runat="server" CssClass="add btn btn-mini" ToolTip="Add Group" CausesValidation="false" OnClick="lbAddItem_Click">
                            <i class="fa fa-plus"></i> <asp:Literal ID="lAddItem" runat="server" Text="Add Group" />
                    </asp:LinkButton>
                </div>
            </div>
                </div>

                    <div class="treeview-scroll scroll-container scroll-container-horizontal">
                
                        <div class="viewport">
                            <div class="overview">
                                <div class="panel-body treeview-frame">
                                    <div id="treeview-content"></div>
                                </div>
                            </div>
                    
                        </div>
                        <div class="scrollbar"><div class="track"><div class="thumb"><div class="end"></div></div></div></div>
                    </div>


            <script type="text/javascript">

                var scrollbCategory = $('.treeview-scroll');
                scrollbCategory.tinyscrollbar({ axis: 'x', sizethumb: '60', size: '200' });

                // update viewport height
                resizeScrollbar(scrollbCategory);

                $(function () {
                    var $selectedId = $('#hfSelectedCategoryId'),
                        $expandedIds = $('#hfInitialCategoryParentIds'),
                        _mapCategories = function (arr) {
                            return $.map(arr, function (item) {
                                var node = {
                                    id: item.Guid || item.Id,
                                    name: item.Name || item.Title,
                                    iconCssClass: item.IconCssClass,
                                    parentId: item.ParentId,
                                    hasChildren: item.HasChildren,
                                    isCategory: item.IsCategory
                                };

                                if (item.Children && typeof item.Children.length === 'number') {
                                    node.children = _mapCategories(item.Children);
                                }

                                return node;
                            });
                        };
                    
                    $('#treeview-content')
                        .on('rockTree:selected', function (e, id) {
                            var $node = $('[data-id="' + id + '"]'),
                                isCategory = $node.attr('data-iscategory') === 'true',
                                itemSearch = '?' + (isCategory ? 'CategoryId' : '<%= PageParameterName %>') + '=' + id;

                            $selectedId.val(id);
                            
                            if (window.location.search !== itemSearch) {
                                window.location.search = itemSearch;
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
                                include: ['isCategory'],
                                mapData: _mapCategories
                            },
                            selectedIds: $selectedId.val() ? $selectedId.val().split(',') : null,
                            expandedIds: $expandedIds.val() ? $expandedIds.val().split(',') : null
                        });
                });

                function resizeScrollbar(scrollControl) {
                    var overviewHeight = $(scrollControl).find('.overview').height();

                    $(scrollControl).find('.viewport').height(overviewHeight);

                    scrollbCategory.tinyscrollbar_update('relative');
                }
            </script>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
