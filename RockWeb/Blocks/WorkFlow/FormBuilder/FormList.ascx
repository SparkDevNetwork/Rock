<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FormList.ascx.cs" Inherits="RockWeb.Blocks.WorkFlow.FormBuilder.FormList" %>
<%@ Import Namespace="Rock" %>
<div class="panel panel-block">
    <div class="panel-heading">
        <h1 class="panel-title"><i class="fa-solid fa-align-left"></i>Form Builder</h1>
    </div>
    <div class="panel-body">
        <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" />
        <div class="picker-wrapper clearfix">
            <asp:UpdatePanel ID="upnlCategory" Class="picker-folders" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
                <ContentTemplate>
                    <asp:HiddenField ID="hfInitialCategoryParentIds" runat="server" />
                    <div class="actions">
                        <h4 class="pull-left">Form Categories</h4>
                        <div runat="server" id="divTreeviewActions" class="btn-group pull-right">
                            <button type="button" class="btn btn-link dropdown-toggle" data-toggle="dropdown" title='<asp:Literal ID="ltAddCategory" runat="server" Text="Add Category" />'>
                                <i class="fa fa-folder-plus"></i>
                            </button>
                            <ul class="dropdown-menu dropdown-menu-right" role="menu">
                                <li>
                                    <asp:LinkButton ID="lbAddCategoryRoot" OnClick="lbAddCategoryRoot_Click" Text="Add Top-Level" runat="server"></asp:LinkButton></li>
                                <li>
                                    <asp:LinkButton ID="lbAddCategoryChild" OnClick="lbAddCategoryChild_Click" Text="Add Child To Selected" runat="server"></asp:LinkButton></li>
                            </ul>
                        </div>
                    </div>
                    <div class="treeview js-categorytreeview">
                        <div class="treeview-scroll scroll-container scroll-container-horizontal">
                            <div class="viewport">
                                <div class="overview">
                                    <div class="treeview-frame">
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
                        var <%=pnlTreeviewContent.ClientID%>IScroll = null;

                        var scrollbCategory = $('#<%=pnlTreeviewContent.ClientID%>').closest('.treeview-scroll');
                        var scrollContainer = scrollbCategory.find('.viewport');
                        var scrollIndicator = scrollbCategory.find('.track');
                                <%=pnlTreeviewContent.ClientID%>IScroll = new IScroll(scrollContainer[ 0 ], {
                                    mouseWheel: false,
                                    scrollX: true,
                                    scrollY: false,
                                    indicators: {
                                        el: scrollIndicator[ 0 ],
                                        interactive: true,
                                        resize: false,
                                        listenX: true,
                                        listenY: false,
                                    },
                                    click: false,
                                });

                        // resize scrollbar when the window resizes
                        $(document).ready(function () {
                            $(window).on('resize', function () {
                                resizeScrollbar(scrollbCategory);
                            });
                        });

                        // scrollbar hide/show
                        var timerScrollHide;
                        $("#<%=upnlCategory.ClientID%>").on({
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
                            var $selectedId = $('#<%=hfSelectedCategory.ClientID%>'),
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

                                        if (item.Children && typeof item.Children.length === 'number') {
                                            node.children = _mapCategories(item.Children);
                                        }
                                        return node;
                                    });
                                };

                            $('#<%=pnlTreeviewContent.ClientID%>')
                                .on('rockTree:selected', function (e, id) {
                                    $('#<%=hfSelectedCategory.ClientID%>').val(id);
                                    // use setTimeout so that the doPostBack happens later (to avoid javascript exception that occurs due to timing)
                                    setTimeout(function () {
                                        var postbackArg = 'category-selected:' + id;
                                        window.location = "javascript:__doPostBack('<%=upnlForms.ClientID %>', '" + postbackArg + "')";
                                    });
                                })
                                // update viewport height
                                .on('rockTree:rendered rockTree:expand rockTree:collapse rockTree:itemClicked', function () {
                                    resizeScrollbar(scrollbCategory);
                                })
                                .rockTree({
                                    restUrl: '<%= ResolveUrl( "~/api/categories/getchildren/" ) %>',
                                    restParams: '<%= RestParms %>',
                                    mapping: {
                                        include: [ 'isCategory', 'entityId' ],
                                        mapData: _mapCategories
                                    },
                                    selectedIds: $selectedId.val() ? $selectedId.val().split(',') : null,
                                    expandedIds: $expandedIds.val() ? $expandedIds.val().split(',') : null
                                });
                        });

                        function resizeScrollbar (scrollControl) {
                            var overviewHeight = $(scrollControl).find('.overview').height();

                            $(scrollControl).find('.viewport').height(overviewHeight);

                            if (<%=pnlTreeviewContent.ClientID%>IScroll) {
                                        <%=pnlTreeviewContent.ClientID%>IScroll.refresh();
                            }
                        }
                    </script>
                </ContentTemplate>
            </asp:UpdatePanel>
            <asp:UpdatePanel ID="upnlForms" class="picker-files" runat="server">
                <ContentTemplate>
                    <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                    <Rock:NotificationBox ID="nbValidationError" runat="server" Title="There is a problem with one or more of the values you entered" NotificationBoxType="Danger" Visible="false" />
                    <asp:HiddenField ID="hfSelectedCategory" runat="server" />
                    <div class="js-files">
                        <h3>
                            <asp:Literal ID="lTitle" runat="server" /></h3>
                        <div>
                            <asp:Literal ID="lDescription" runat="server" />
                            <div id="divFormListTopPanel" runat="server" class="pull-right">
                                <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-xs btn-square btn-security" />
                                <asp:LinkButton ID="btnEdit" runat="server" CssClass="btn btn-default btn-xs btn-square" OnClick="btnEditCategory_Click"><i class="fa fa-pencil"></i></asp:LinkButton>
                                <asp:LinkButton ID="btnDelete" runat="server" CssClass="btn btn-danger btn-xs btn-square"><i class="fa fa-trash-alt"></i></asp:LinkButton>
                            </div>
                        </div>
                        <hr class="mt-2" />
                        <asp:Panel ID="pnlFormList" runat="server" Visible="false">
                            <div class="row">
                                <div class="col-xs-12">
                                    <div class="form-horizontal label-auto pull-right">
                                        <Rock:RockDropDownList ID="ddlSortBy" Label="Sort By" runat="server" FormGroupCssClass="input-width-xl" AutoPostBack="true" OnSelectedIndexChanged="ddlSortBy_SelectedIndexChanged" />
                                    </div>
                                    <asp:LinkButton ID="lbAddForm" runat="server" ToolTip="Add Form" CssClass="btn btn-xs btn-default" OnClick="lbAddForm_Click"><i class="fa fa-plus"></i></asp:LinkButton>
                                </div>
                            </div>
                            <asp:Repeater ID="rForms" runat="server">
                                <ItemTemplate>
                                    <div class="card card-sm card-schedule">
                                        <div class="card-body d-flex">
                                            <div class="flex-fill">
                                                <h3><%# Eval("Name") %></h3>
                                                <p>Created by <%# Eval("CreatedByPersonAlias.Person") %> <%# ((DateTime?)Eval("CreatedDateTime")).ToElapsedString() %></p>
                                            </div>
                                            <asp:Panel ID="pnlSideMenu" class="d-flex align-items-center flex-nowrap justify-content-end" runat="server">
                                                <span class='badge badge-info'>23</span>
                                            </asp:Panel>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </asp:Panel>
                        <asp:Panel ID="pnlAddForm" runat="server" Visible="false">
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:DataTextBox ID="tbFormName" runat="server" SourceTypeName="Rock.Model.WorkflowType, Rock" PropertyName="Name" Help="The internal name of the foam you are creating. This name will not be displayed when the form is not being shown." />
                                    <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.WorkflowType, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" Help="An internal description of what the foam will be used for." />
                                    <Rock:RockDropDownList ID="ddlTemplate" Label="Template" runat="server" Help="An optional template to use that provides pre-configured settings." />
                                    <Rock:CategoryPicker ID="cpCategory" runat="server" Required="true" Label="Category" EntityTypeName="Rock.Model.WorkflowType" />
                                </div>
                            </div>
                            <div class="actions">
                                <asp:LinkButton ID="btnStartBuilding" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Start Building" CssClass="btn btn-primary" OnClick="btnStartBuilding_Click" />
                                <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                            </div>
                        </asp:Panel>
                        <asp:Panel ID="pnlCategory" runat="server" Visible="false">
                            <fieldset>
                                <Rock:DataTextBox ID="tbCategoryName" runat="server" SourceTypeName="Rock.Model.Category, Rock" PropertyName="Name" />
                                <Rock:DataTextBox ID="tbCategoryDescription" runat="server" SourceTypeName="Rock.Model.Category, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.Category, Rock" PropertyName="IconCssClass" />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:DataTextBox ID="tbHighlightColor" runat="server" SourceTypeName="Rock.Model.Category, Rock" PropertyName="HighlightColor" />
                                    </div>
                                </div>
                            </fieldset>

                            <div class="actions">
                                <asp:LinkButton ID="btnCategorySave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnCategorySave_Click" />
                                <asp:LinkButton ID="btnCategoryCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                            </div>
                        </asp:Panel>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
</div>
