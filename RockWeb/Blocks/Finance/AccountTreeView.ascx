<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountTreeView.ascx.cs" Inherits="RockWeb.Blocks.Finance.AccountTreeView" %>

<asp:UpdatePanel ID="upAccountType" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
    <ContentTemplate>
        <asp:HiddenField ID="hfInitialAccountId" runat="server" />
        <asp:HiddenField ID="hfexcludeInactiveGroups" runat="server" />
        <asp:HiddenField ID="hfInitialAccountParentIds" runat="server" />
        <asp:HiddenField ID="hfSelectedAccountId" runat="server" />
        <asp:HiddenField ID="hfPageRouteTemplate" runat="server" />
        <asp:HiddenField ID="hfDetailPageUrl" runat="server" />
        <asp:HiddenField ID="hfUsePublicName" runat="server" />
        <div class="treeview js-accounttreeview">
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title">Accounts</h1>
                    <div class="panel-labels treeview-actions" id="divTreeviewActions" runat="server">
                        <div class="btn-group">
                            <button type="button" class="btn btn-link btn-xs dropdown-toggle" data-toggle="dropdown">
                                <i class="fa fa-plus"></i>
                            </button>
                            <ul class="dropdown-menu" role="menu">
                                <li>
                                    <asp:LinkButton ID="lbAddAccountRoot" OnClick="lbAddAccountRoot_Click" Text="Add Top-Level" runat="server"></asp:LinkButton></li>
                                <li>
                                    <asp:LinkButton ID="lbAddAccountChild" OnClick="lbAddAccountChild_Click" Enabled="false" Text="Add Child To Selected" runat="server"></asp:LinkButton></li>
                            </ul>
                        </div>
                        <button type="button" id="pnlSearchButton" class="btn btn-link btn-xs dropdown-toggle">
                            <i class="fa fa-search"></i>
                        </button>
                        <button type="button" id="pnlRolloverConfig" class="btn btn-link btn-xs clickable js-show-config" onclick="$(this).closest('.js-accounttreeview').find('.js-config-panel').slideToggle()" runat="server">
                            <i class="fa fa-ellipsis-v"></i>
                        </button>
                    </div>
                </div>
                <div class="panel-body">
                    <div class="rocktree-drawer js-config-panel" style="display: none" id="pnlConfigPanel" runat="server">
                        <div class="d-flex flex-wrap justify-content-between align-items-end">
                            <div class="mb-1">
                                <Rock:Toggle ID="tglHideInactiveAccounts" runat="server" OnText="Active" OffText="All" Checked="true" FormGroupCssClass="m-0" ButtonSizeCssClass="btn-xs" OnCheckedChanged="tglHideInactiveAccounts_CheckedChanged" Label="Show" />
                            </div>
                            <div class="mb-1">
                                <asp:LinkButton ID="lbOrderTopLevelAccounts" runat="server" CssClass="btn btn-xs btn-default text-wrap" Text="Order Top-Level Accounts" OnClick="lbOrderTopLevelAccounts_Click" />
                            </div>
                        </div>
                    </div>

                    <div class="rocktree-drawer form-group js-group-search" style="display: none">
                        <asp:Label runat="server" AssociatedControlID="tbSearch" Text="Search" CssClass="control-label d-none" />
                        <asp:Panel ID="pnlSearch" runat="server" DefaultButton="btnSearch" CssClass="input-group">
                            <asp:TextBox ID="tbSearch" runat="server" placeholder="Quick Find" CssClass="form-control input-sm" />
                            <span class="input-group-btn">
                                <asp:LinkButton ID="btnSearch" runat="server" CssClass="btn btn-default btn-sm"><i class="fa fa-search"></i></asp:LinkButton>
                            </span>
                        </asp:Panel>
                    </div>

                    <div class="treeview-search-parent styled-scroll">
                        <div id="divTreeView" class="treeview-scroll scroll-container scroll-container-horizontal" runat="server">
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

                        <div id="divSearchResults" class="search-results" runat="server" style="display: none;">
                        </div>
                    </div>
                </div>
            </div>
        </div>


        <script type="text/javascript">
            var <%=hfSelectedAccountId.ClientID%>IScroll = null;

            $(function () {
                var $selectedId = $('#<%=hfSelectedAccountId.ClientID%>'),
                    $expandedIds = $('#<%=hfInitialAccountParentIds.ClientID%>');

                var scrollbCategory = $('#<%=pnlTreeviewContent.ClientID%>').closest('.treeview-scroll');
                var scrollContainer = scrollbCategory.find('.viewport');
                var scrollIndicator = scrollbCategory.find('.track');
                <%=hfSelectedAccountId.ClientID%>IScroll = new IScroll(scrollContainer[0], {
                    mouseWheel: false,
                    eventPassthrough: true,
                    preventDefault: false,
                    scrollX: true,
                    scrollY: false,
                    indicators: {
                        el: scrollIndicator[0],
                        interactive: true,
                        resize: false,
                        listenX: true,
                        listenY: false,
                    },
                    click: false,
                    preventDefaultException: { tagName: /.*/ }
                });

                // resize scrollbar when the window resizes
                $(document).ready(function () {
                    $(window).on('resize', function () {
                        resizeScrollbar(scrollbCategory);
                    });
                });

                $('#<%=pnlTreeviewContent.ClientID%>')
                    .on('rockTree:rendered', function (e, data) {
                        createSearch();
                    })
                    .on('rockTree:selected', function (e, id, expandedIds, reload) {

                        var accountSearch = '?AccountId=' + id;
                        var currentItemId = $selectedId.val();

                        if (currentItemId !== id || reload === true) {

                            // get the data-id values of rock-tree items that are showing visible children (in other words, Expanded Nodes)
                            var expandedDataIds = !expandedIds ? $(e.currentTarget).find('.rocktree-children').filter(":visible").closest('.rocktree-item').map(function () {
                                return $(this).attr('data-id')
                            }).get().join(',') : expandedIds;

                            var pageRouteTemplate = $('#<%=hfPageRouteTemplate.ClientID%>').val();
                            var locationUrl = "";
                            if (pageRouteTemplate.match(/{accountId}/i)) {
                                locationUrl = Rock.settings.get('baseUrl') + pageRouteTemplate.replace(/{accountId}/i, id);
                                locationUrl += "?ExpandedIds=" + encodeURIComponent(expandedDataIds);
                            }
                            else {
                                var detailPageUrl = $('#<%=hfDetailPageUrl.ClientID%>').val();
                                if (detailPageUrl) {
                                    locationUrl = detailPageUrl + accountSearch;
                                }
                                else {
                                    locationUrl = window.location.href.split('?')[0] + accountSearch;
                                }

                                locationUrl += "&ExpandedIds=" + encodeURIComponent(expandedDataIds);
                            }

                            window.location = locationUrl;
                        }
                    })
                    .on('rockTree:rendered rockTree:expand rockTree:collapse rockTree:itemClicked', function () {
                        // update viewport height
                        resizeScrollbar(scrollbCategory);
                    })
                    .rockTree({
                        restUrl: '<%=ResolveUrl( "~/api/FinancialAccounts/GetChildren/" ) %>',
                        restParams: '?activeOnly=' + ($('#<%=hfexcludeInactiveGroups.ClientID%>').val() || false) + "&displayPublicName=" + ($('#<%=hfUsePublicName.ClientID%>').val() || false) + "&lazyLoad=true",
                        multiSelect: false,
                        selectedIds: $selectedId.val() ? $selectedId.val().split(',') : null,
                        expandedIds: $expandedIds.val() ? $expandedIds.val().split(',') : null
                    });

                function createSearch() {
                    // Handle the input searching
                    var $searchInputControl = $('#<%=tbSearch.ClientID%>');
                var $btnSearch = $('#<%=btnSearch.ClientID%>')
                var currentTree = $('#<%=pnlTreeviewContent.ClientID%>').html();
                var expandedDataIds = $('#<%=pnlTreeviewContent.ClientID%>').find('[node-open=true]').map(function () {
                    return $(this).attr('data-id');
                }).get();

                var $searchPanel = $('#pnlSearchButton').closest('.js-accounttreeview').find('.js-group-search');

                $('#pnlSearchButton').off('click').on('click', function (e) {

                    $searchPanel.slideToggle(function () {
                        if ($searchPanel.is(':hidden')) {
                            $('#<%=divSearchResults.ClientID%>').hide().html('');
                        $('#<%=divTreeView.ClientID%>').css('visibility', 'visible');
                        $('#<%=pnlTreeviewContent.ClientID%>').html(currentTree);
                    }
                });

             });

                // Default to showing the search panel.
                $searchPanel.show();

                // If no input then revert the control
                $searchInputControl.off('keyup').on('keyup', function () {
                    if ($searchInputControl.val().length === 0) {
                        $('#<%=divSearchResults.ClientID%>').hide().html('');
                     $('#<%=divTreeView.ClientID%>').css('visibility', 'visible');
                     $('#<%=pnlTreeviewContent.ClientID%>').html(currentTree);
                 }
             });

                //execute ajax search
                $btnSearch.off('click').on('click', function (e) {

                    $('#<%=divTreeView.ClientID%>').css('visibility', 'hidden');
                 $('#<%=divSearchResults.ClientID%>').show();

                 var searchKeyword = $searchInputControl.val();
                 if (searchKeyword && searchKeyword.length > 0) {
                     var restUrl = '<%=ResolveUrl( "~/api/FinancialAccounts/GetChildrenBySearchTerm")%>'
                     var restUrlParams = ("?activeOnly=" + $('#<%=hfexcludeInactiveGroups.ClientID%>').val() || false) + "&displayPublicName=" + ($('#<%=hfUsePublicName.ClientID%>').val() || false) + '&searchTerm=' + searchKeyword;

                    restUrl = restUrl + restUrlParams;

                    $.getJSON(restUrl, function (data, status) {

                        if (data && status === 'success') {
                            $('#<%=pnlTreeviewContent.ClientID%>').html('');
                     }
                     else {
                         $('#<%=divSearchResults.ClientID%>').hide();
                         $('#<%=divTreeView.ClientID%>').css('visibility', 'visible');
                         return;
                     }

                     var nodes = [];
                     for (var i = 0; i < data.length; i++) {
                         var obj = data[i];
                         var node = {
                             nodeId: obj.Id,
                             parentId: obj.ParentId,
                             glcode: obj.GlCode,
                             title: obj.Name + (obj.GlCode ? ' (' + obj.GlCode + ')' : ''),
                             name: obj.Name,
                             hasChildren: obj.HasChildren,
                             isActive: obj.IsActive,
                             path: obj.Path
                         };

                         nodes.push(node);
                     }

                     if (nodes) {
                         var listHtml = '';
                         nodes.forEach(function (v, idx) {
                             if (v.path === '') {
                                 v.path = 'Top-Level';
                             };
                             listHtml +=
                                 '<li id="divSearchItem_' + idx + '" class="preview-item">' +
                                 '   <a class="search-result-link" data-id="' + v.nodeId + '" href="javascript:void(0);">' +
                                 '              <span class="title">' + v.title + '</span>' +
                                 '              <span class="subtitle">' + v.path.replaceAll('^', '<i class="fa fa-chevron-right px-1" aria-hidden="true"></i>') +
                                 '   </span></a>' +
                                 '</li>';
                         });

                         $('#<%=divSearchResults.ClientID%>').html('<ul class="list-unstyled">' +
                             listHtml +
                             '</ul>');
                     }

                     $('.search-result-link').off('click').on('click', function () {
                         var nodeId = $(this).attr('data-id');

                         // Set the selected id
                         $selectedId.val(nodeId);

                         restUrl = '<%=ResolveUrl( "~/api/FinancialAccounts/GetParentIds/")%>'
                         restUrlParams = nodeId;
                         restUrl = restUrl + restUrlParams;

                         // Get the ancestor ids so the tree will expand
                         $.getJSON(restUrl, function (data, status) {
                             if (data && status === 'success') {

                                 data.forEach(function (expId, idx) {
                                     if (!expandedDataIds.find(function (v) {
                                         return v === expId;
                                     })) {
                                         expandedDataIds.push(expId);
                                     }
                                 });

                                 triggerSelect(nodeId, expandedDataIds.join(','), true);
                             }
                         });

                     });
                 }); //getJson
                 }
             });
                } //createSearch


            }); //function()

            // jquery function to ensure HTML is state remains the same each time it is executed
            $.fn.outerHTML = function (s) {
                return (s)
                    ? this.before(s).remove()
                    : $("<p>").append(this.eq(0).clone()).html();
            }

            function triggerSelect(nodeId, expandedIds, reload) {
                $('#<%=pnlTreeviewContent.ClientID%>').trigger('rockTree:selected', [nodeId, expandedIds, reload])
            }

            function resizeScrollbar(scrollControl) {
                var overviewHeight = $(scrollControl).find('.overview').height();

                $(scrollControl).find('.viewport').height(overviewHeight);

                if (<%=hfSelectedAccountId.ClientID%>IScroll) {
                <%=hfSelectedAccountId.ClientID%>IScroll.refresh();
                }
            }
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
