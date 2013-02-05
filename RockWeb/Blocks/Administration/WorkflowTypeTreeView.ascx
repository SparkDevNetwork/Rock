<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowTypeTreeView.ascx.cs" Inherits="RockWeb.Blocks.Administration.WorkflowTypeTreeView" %>

<asp:UpdatePanel ID="upWorkflowType" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
    <ContentTemplate>
        <div class="treeview-back">
            <h3>
                <asp:Literal ID="ltlTreeViewTitle" runat="server" /></h3>
            <div id="treeviewWorkflowType" class="workflowTypeTreeview">
            </div>
        </div>
        <script>
            function onSelect(e) {
                var dataItem = this.dataItem(e.node);
                showItemDetails(dataItem);
            }

            function showItemDetails(dataItem) {
                __doPostBack('<%= upWorkflowType.ClientID %>', dataItem.EntityTypeName + 'Id=' + dataItem.Id);
            }

            function onDataBound(e) {
                // automatically select the first item in the treeview if there isn't one currently selected
                var treeViewData = $('.workflowTypeTreeview').data("kendoTreeView");
                var selectedNode = treeViewData.select();
                var nodeData = this.dataItem(selectedNode);
                if (!nodeData) {
                    var firstItem = treeViewData.root[0].firstChild;
                    var firstDataItem = this.dataItem(firstItem);
                    if (firstDataItem) {
                        treeViewData.select(firstItem);
                        showItemDetails(firstDataItem);
                    }
                }
            }

            var restUrl = "<%=ResolveUrl( "~/api/categories/getchildren/" ) %>";

            var dataList = new kendo.data.HierarchicalDataSource({
                transport: {
                    read: {
                        url: function (options) {
                            var requestUrl = restUrl + (options.id || 0) + '/' + 'WorkflowType'
                            return requestUrl;
                        },
                        error: function (xhr, status, error) {
                            {
                                alert(status + ' [' + error + ']: ' + xhr.responseText);
                            }
                        }
                    }
                },
                schema: {
                    model: {
                        id: 'Id',
                        hasChildren: 'HasChildren',
                        entityTypeName: 'EntityTypeName'
                    }
                }
            });

            $('.workflowTypeTreeview').kendoTreeView({
                template: "<i class='#= item.IconCssClass #'></i> #= item.Name #",
                dataSource: dataList,
                dataTextField: 'Name',
                dataImageUrlField: 'IconSmallUrl',
                select: onSelect,
                dataBound: onDataBound
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
