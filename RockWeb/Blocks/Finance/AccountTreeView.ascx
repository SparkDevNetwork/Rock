﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountTreeView.ascx.cs" Inherits="RockWeb.Blocks.Finance.AccountTreeView" %>

<asp:UpdatePanel ID="upAccountType" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
    <ContentTemplate>
        <asp:HiddenField ID="hfInitialAccountId" runat="server" />
        <asp:HiddenField ID="hfexcludeInactiveGroups" runat="server" />
        <asp:HiddenField ID="hfInitialAccountParentIds" runat="server" />
        <asp:HiddenField ID="hfSelectedAccountId" runat="server" />
        <asp:HiddenField ID="hfPageRouteTemplate" runat="server" />
        <asp:HiddenField ID="hfDetailPageUrl" runat="server" />

        <div class="treeview js-accounttreeview">
            <div class="treeview-actions rollover-container" id="divTreeviewActions" runat="server">

                <div class="btn-group pull-left margin-r-sm">
                    <button type="button" class="btn btn-action btn-xs dropdown-toggle" data-toggle="dropdown">
                        <i class="fa fa-plus-circle"></i>&nbsp;Add Account <span class="fa fa-caret-down"></span>
                    </button>
                    <ul class="dropdown-menu" role="menu">
                        <li>
                            <asp:LinkButton ID="lbAddAccountRoot" OnClick="lbAddAccountRoot_Click" Text="Add Top-Level" runat="server"></asp:LinkButton></li>
                        <li>
                            <asp:LinkButton ID="lbAddAccountChild" OnClick="lbAddAccountChild_Click" Enabled="false" Text="Add Child To Selected" runat="server"></asp:LinkButton></li>
                    </ul>
                </div>

                <div class="rollover-item" id="pnlRolloverConfig" runat="server">
                    <i class="fa fa-gear clickable js-show-config" onclick="$(this).closest('.js-accounttreeview').find('.js-config-panel').slideToggle()"></i>
                </div>
            </div>

            <div class="js-config-panel" style="display: none" id="pnlConfigPanel" runat="server">
                <Rock:Toggle ID="tglHideInactiveAccounts" runat="server" OnText="Active" OffText="All" Checked="true" ButtonSizeCssClass="btn-xs" OnCheckedChanged="tglHideInactiveAccounts_CheckedChanged" Label="Show" />
            </div>

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
            $(function () {
                var $selectedId = $('#<%=hfSelectedAccountId.ClientID%>'),
                    $expandedIds = $('#<%=hfInitialAccountParentIds.ClientID%>');

                var scrollbCategory = $('#<%=pnlTreeviewContent.ClientID%>').closest('.treeview-scroll');
                scrollbCategory.tinyscrollbar({ axis: 'x', sizethumb: 60, size: 200 });

                // resize scrollbar when the window resizes
                $(document).ready(function () {
                    $(window).on('resize', function () {
                        resizeScrollbar(scrollbCategory);
                    });
                });

                $('#<%=pnlTreeviewContent.ClientID%>')
                    .on('rockTree:selected', function (e, id) {
                        var accountSearch = '?AccountId=' + id;
                        var currentItemId = $selectedId.val();

                        if (currentItemId !== id) {

                            // get the data-id values of rock-tree items that are showing visible children (in other words, Expanded Nodes)
                            var expandedDataIds = $(e.currentTarget).find('.rocktree-children').filter(":visible").closest('.rocktree-item').map(function () {
                                return $(this).attr('data-id')
                            }).get().join(',');

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
                    .on('rockTree:rendered', function () {

                        // update viewport height
                        resizeScrollbar(scrollbCategory);

                    })
                    .on('rockTree:collapse', function ()
                    {
                        resizeScrollbar(scrollbCategory);
                    })
                    .on('rockTree:expand', function ()
                    {
                        resizeScrollbar(scrollbCategory);
                    })
                    .rockTree({
                        restUrl: '<%=ResolveUrl( "~/api/FinancialAccounts/GetChildren/" ) %>',
                        restParams: '/'+ ($('#<%=hfexcludeInactiveGroups.ClientID%>').val() || false),
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
