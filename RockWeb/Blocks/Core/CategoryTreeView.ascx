<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CategoryTreeView.ascx.cs" Inherits="RockWeb.Blocks.Core.CategoryTreeView" %>

<asp:UpdatePanel ID="upCategoryTree" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
    <ContentTemplate>
        <asp:HiddenField ID="hfInitialEntityIsCategory" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfInitialItemId" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfInitialCategoryParentIds" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfSelectedCategoryId" runat="server" ClientIDMode="Static" />
        <div class="treeview treeview-group">
            <div class="treeview-actions pull-right">
                <asp:LinkButton ID="lbAddCategory" runat="server" CssClass="add btn btn-mini" ToolTip="Add Category" CausesValidation="false" OnClick="lbAddCategory_Click">
                        <i class="icon-plus-sign"></i> Add Category
                </asp:LinkButton>
                <asp:LinkButton ID="lbAddItem" runat="server" CssClass="add btn btn-mini" ToolTip="Add Group" CausesValidation="false" OnClick="lbAddItem_Click">
                        <i class="icon-plus-sign"></i> <asp:Literal ID="lAddItem" runat="server" Text="Add Group" />
                </asp:LinkButton>
            </div>
            <div id="treeview-content">
            </div>
            <script type="text/javascript">
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
            </script>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
