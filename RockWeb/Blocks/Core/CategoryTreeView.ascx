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

            




            <div id="scrollbar1">
                
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
                $(document).ready(function () {
                    
                });
                
            </script>

            <style>
                #scrollbar1 { width: 250px; clear: both; margin: 0; }
                #scrollbar1 .viewport { width: 230px; height: 200px; overflow: hidden; position: relative; }
                #scrollbar1 .overview { list-style: none; position: absolute; left: 0; top: 0; width: 500px; }
                #scrollbar1 .thumb .end,
                #scrollbar1 .thumb { background-color: #003D5D; }
                #scrollbar1 .scrollbar { position: relative; height: 15px; clear: both; }
                #scrollbar1 .track { background-color: #D8EEFD; height: 100%; width:13px; position: relative; padding: 0 1px; }
                #scrollbar1 .thumb { height: 20px; width: 13px; cursor: pointer; overflow: hidden; position: absolute; top: 0; }
                #scrollbar1 .thumb .end { overflow: hidden; height: 5px; width: 13px; }
                #scrollbar1 .disable{ display: none; }
                .noSelect { user-select: none; -o-user-select: none; -moz-user-select: none; -khtml-user-select: none; -webkit-user-select: none; }
            </style>

            <script type="text/javascript">

                var scrollbCategory = $('#scrollbar1');
                scrollbCategory.tinyscrollbar({ axis: 'x' });


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


                            scrollbCategory.tinyscrollbar_update();
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
