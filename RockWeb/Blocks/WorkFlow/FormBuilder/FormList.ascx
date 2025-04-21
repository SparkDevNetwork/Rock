<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FormList.ascx.cs" Inherits="RockWeb.Blocks.WorkFlow.FormBuilder.FormList" %>
<style>
    .block-instance.form-list .panel-body {
        flex: 1 0 0;
        overflow: hidden;
    }
    .block-instance.form-list .filter-options, .block-instance.form-list .form-category-display {
        overflow-y: auto;
    }
    .block-instance.form-list .filter-options {
        min-width: 320px;
        padding: 0 !important;
    }
    .block-instance.form-list .filter-options .category-header {
        padding: 18px 18px 12px;
    }
    @media (max-width: 1199px) {
        .block-instance.form-list .filter-options {
            border-right: 0;
            border-bottom: 1px solid var(--panel-border);
    }
    }
    .block-instance.form-list .form-actions {
        margin-top: 12px;
    }
    .block-instance.form-list .btn-link-spaced {
        padding: 0;
        margin-right: 8px;
    }
    .block-instance.form-list .btn-link-spaced + .btn-link-spaced {
        margin-left: 8px;
    }
    .aspNetDisabled.btn-category-delete {
        pointer-events: auto;
    }
</style>
<div class="panel panel-block panel-analytics styled-scroll">
    <div class="panel-heading">
        <h1 class="panel-title"><i class="fa-solid fa-align-left"></i>Form Builder</h1>
    </div>
    <div class="panel-body">
        <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" />
        <div class="row row-eq-height-md d-flex flex-column h-100 flex-md-row">
            <asp:UpdatePanel ID="upnlCategory" Class="filter-options" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
                <ContentTemplate>
                    <asp:HiddenField ID="hfInitialCategoryParentIds" runat="server" />
                    <div class="category-header d-flex align-items-center justify-content-between">
                        <strong class="d-block text-sm m-0">Form Categories</strong>
                        <div runat="server" id="divTreeviewActions" class="btn-group pull-right">
                            <button type="button" class="btn btn-link text-color btn-xs btn-square dropdown-toggle" data-toggle="dropdown" title='<asp:Literal ID="ltAddCategory" runat="server" Text="Add Category" />'>
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
            <asp:UpdatePanel ID="upnlForms" class="form-category-display flex-fill" runat="server">
                <ContentTemplate>
                    <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                    <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                    <Rock:NotificationBox ID="nbValidationError" runat="server" Title="There is a problem with one or more of the values you entered" NotificationBoxType="Danger" Visible="false" />
                    <asp:HiddenField ID="hfSelectedCategory" runat="server" />
                    <div class="container-in-block">
                        <div class="rock-header">
                            <h3 class="title">
                                <asp:Literal ID="lTitle" runat="server" /></h3>
                            <div class="d-flex justify-content-between">
                                <div class="description pr-2">
                                    <asp:Literal ID="lDescription" runat="server" />
                                </div>
                                <div id="divFormListTopPanel" runat="server" class="ml-auto d-flex gap-1">
                                    <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-xs btn-square btn-security" />
                                    <asp:LinkButton ID="btnEdit" runat="server" CssClass="btn btn-default btn-xs btn-square" OnClick="btnEditCategory_Click"><i class="fa fa-pencil"></i></asp:LinkButton>
                                    <asp:LinkButton ID="btnDeleteCategory" data-toggle="tooltip" data-trigger="hover" data-delay="250" runat="server" CssClass="btn btn-danger btn-xs btn-square btn-category-delete" OnClick="btnDeleteCategory_Click"><i class="fa fa-trash-alt"></i></asp:LinkButton>
                                </div>
                            </div>
                            <hr class="section-header-hr" />
                        </div>
                        <asp:Panel ID="pnlFormList" runat="server" Visible="false">
                            <div class="d-flex flex-wrap align-items-start">
                                <div class="form-inline margin-b-md label-auto">
                                    <Rock:RockDropDownList ID="ddlSortBy" Label="Sort By" runat="server" FormGroupCssClass="form-group-sm" AutoPostBack="true" OnSelectedIndexChanged="ddlSortBy_SelectedIndexChanged" />
                                </div>
                                <asp:LinkButton ID="lbAddForm" runat="server" ToolTip="Add Form" CssClass="btn btn-sm btn-square btn-primary ml-2 ml-auto" OnClick="lbAddForm_Click"><i class="fa fa-plus"></i></asp:LinkButton>
                            </div>
                            <asp:Repeater ID="rForms" runat="server" OnItemDataBound="rForms_ItemDataBound" OnItemCommand="rForms_ItemCommand">
                                <ItemTemplate>
                                    <div class="card card-sm group-hover mb-3">
                                        <div class="card-body d-flex justify-content-between">
                                            <div>
                                                <h3 class="m-0"><%# Eval("Name") %></h3>
                                                <p class="text-muted text-sm group-hover-item pr-3 mt-1 leading-snug"><%# Eval("Description") %></p>
                                            </div>
                                            <asp:Panel ID="pnlSideMenu" class="d-flex align-items-center" runat="server">
                                                <asp:LinkButton ID="lbSubmissions" runat="server" ToolTip="Submissions" CssClass="btn btn-default btn-sm btn-square ml-2" CommandName="Submissions" CommandArgument='<%# Eval( "Id" ) %>' Text="<i class='fa fa-list'></i>" />
                                                <asp:LinkButton ID="lbBuilder" runat="server" ToolTip="Builder" CssClass="btn btn-default btn-sm btn-square ml-1" CommandName="Builder" Text="<i class='fa fa-edit'></i>" CommandArgument='<%# Eval( "Id" ) %>' />
                                                <asp:LinkButton ID="lbAnalytics" runat="server" ToolTip="Analytics" CssClass="btn btn-default btn-sm btn-square ml-1" CommandName="Analytics" CommandArgument='<%# Eval( "Id" ) %>' Text="<i class='fa fa-chart-bar'></i>" />
                                                <asp:LinkButton ID="lbLinkToForm" runat="server" ToolTip="Link To Form" CssClass="btn btn-default btn-sm btn-square ml-1" CommandName="LinkToForm" CommandArgument='<%# Eval( "Id" ) %>' Text="<i class='fa fa-link'></i>" />
                                                <div class="dropdown js-group-actions hide-dragging">
                                                    <button type="button" class="btn btn-default btn-sm btn-square dropdown-toggle ml-1" data-toggle="dropdown">
                                                        <i class="fa fa-ellipsis-v"></i>
                                                    </button>
                                                    <ul class="dropdown-menu">
                                                        <li>
                                                            <asp:LinkButton ID="lbCopy" runat="server" title="Copy" CssClass="btn btn-link" CommandName="Copy" CommandArgument='<%# Eval( "Id" ) %>' Text="Clone" />
                                                            <asp:LinkButton ID="lbDelete" runat="server" CssClass="btn btn-link" title="Delete form and all submissions." OnClientClick="return Rock.dialogs.confirmDelete(event, 'Form');" Text="Delete" CommandName="Delete" CommandArgument='<%# Eval( "Id" ) %>' />
                                                        </li>
                                                    </ul>
                                                </div>
                                                <span class='badge badge-info ml-2'><%# Eval("SubmissionCount") %></span>
                                            </asp:Panel>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </asp:Panel>
                        <asp:Panel ID="pnlAddForm" runat="server" Visible="false">
                            <div class="row">
                                <div class="col-xs-12 col-md-9 col-lg-8">
                                    <Rock:DataTextBox ID="tbFormName" runat="server" SourceTypeName="Rock.Model.WorkflowType, Rock" PropertyName="Name" Help="The internal name of the form you are creating. This name will not be displayed when the form is not being shown." />
                                    <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.WorkflowType, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" Help="An internal description of what the form will be used for." />
                                    <Rock:RockDropDownList ID="ddlTemplate" Label="Template" runat="server" Help="An optional template to use that provides pre-configured settings." />
                                    <Rock:CategoryPicker ID="cpCategory" runat="server" Required="true" Label="Category" EntityTypeName="Rock.Model.WorkflowType" />
                                </div>
                            </div>
                            <div class="form-actions">
                                <asp:LinkButton ID="btnStartBuilding" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Start Building" CssClass="btn btn-primary" OnClick="btnStartBuilding_Click" />
                                <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                            </div>
                        </asp:Panel>
                        <asp:Panel ID="pnlCategory" runat="server" Visible="false">
                            <asp:HiddenField ID="hfCategoryId" runat="server" />
                            <asp:HiddenField ID="hfParentCategory" runat="server" />
                            <asp:CustomValidator ID="cvCategory" runat="server" Display="None" />
                            <fieldset>
                                <Rock:DataTextBox ID="tbCategoryName" runat="server" SourceTypeName="Rock.Model.Category, Rock" PropertyName="Name" />
                                <Rock:DataTextBox ID="tbCategoryDescription" runat="server" SourceTypeName="Rock.Model.Category, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.Category, Rock" PropertyName="IconCssClass" Label="Icon CSS Class" />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:ColorPicker ID="cpHighlightColor" runat="server" Label="Highlight Color" />
                                    </div>
                                </div>
                            </fieldset>

                            <div class="form-actions">
                                <asp:LinkButton ID="btnCategorySave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnCategorySave_Click" />
                                <asp:LinkButton ID="btnCategoryCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                            </div>
                        </asp:Panel>
                    </div>
                    <Rock:ModalDialog ID="mdLinkToFormModal" runat="server" Title="Link To Form" CancelLinkVisible="false" OnSaveClick="mdLinkToFormModal_SaveClick" SaveButtonText="Close" >
                        <Content>
                            <h3>Select a Page for Form Link</h3>
                            <p>Choose a page from the list below to generate a link for the form. Once selected, you can copy and use the link as needed.</p>
                            <Rock:Grid ID="gPages" runat="server" DisplayType="Light" RowItemText="Page">
                                <Columns>
                                    <Rock:RockBoundField DataField="Name" HeaderText="Page Name" />
                                    <Rock:RockBoundField DataField="Site" HeaderText="Site" />
                                    <Rock:RockBoundField DataField="Route" HeaderText="Route" />
                                    <Rock:RockTemplateFieldUnselected>
                                        <ItemTemplate>
                                            <button class="btn btn-default js-copy-clipboard"
                                                    data-toggle="tooltip"
                                                    data-placement="top"
                                                    data-trigger="hover"
                                                    data-delay="250"
                                                    title="Copy to Clipboard"
                                                    data-clipboard-text='<%# Eval( "RouteURL" ) %>'
                                                    onclick="$(this).attr('data-original-title', 'Copied').tooltip('show').attr('data-original-title', 'Copy to Clipboard');return false;">
                                                <i class='fa fa-clipboard'></i> Copy Link
                                            </button>
                                        </ItemTemplate>
                                    </Rock:RockTemplateFieldUnselected>
                                </Columns>
                            </Rock:Grid>
                        </Content>
                    </Rock:ModalDialog>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
</div>
