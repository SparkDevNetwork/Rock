<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupTreeView.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupTreeView" %>

<asp:UpdatePanel ID="upGroupType" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
    <ContentTemplate>
        <asp:HiddenField ID="hfRootGroupId" runat="server" />
        <asp:HiddenField ID="hfInitialGroupId" runat="server" />
        <asp:HiddenField ID="hfGroupTypesInclude" runat="server" />
        <asp:HiddenField ID="hfGroupTypesExclude" runat="server" />
        <asp:HiddenField ID="hfIncludeInactiveGroups" runat="server" />
        <asp:HiddenField ID="hfLimitPublicGroups" runat="server" />
        <asp:HiddenField ID="hfCountsType" runat="server" />
        <asp:HiddenField ID="hfCampusFilter" runat="server" />
        <asp:HiddenField ID="hfIncludeNoCampus" runat="server" />
        <asp:HiddenField ID="hfInitialGroupParentIds" runat="server" />
        <asp:HiddenField ID="hfLimitToSecurityRoleGroups" runat="server" />
        <asp:HiddenField ID="hfSelectedGroupId" runat="server" />
        <asp:HiddenField ID="hfPageRouteTemplate" runat="server" />
        <asp:HiddenField ID="hfDetailPageUrl" runat="server" />

        <div class="treeview js-grouptreeview">
            <asp:Panel ID="pnlTreeviewActions" CssClass="treeview-actions rollover-container" runat="server">

                <div id="divAddGroup" runat="server" class="btn-group pull-left margin-r-sm">
                    <button type="button" class="btn btn-action btn-xs dropdown-toggle" data-toggle="dropdown">
                        <i class="fa fa-plus-circle"></i>&nbsp;Add Group <span class="fa fa-caret-down"></span>
                    </button>
                    <ul class="dropdown-menu" role="menu">
                        <li>
                            <asp:LinkButton ID="lbAddGroupRoot" OnClick="lbAddGroupRoot_Click" Text="Add Top-Level" runat="server"></asp:LinkButton></li>
                        <li>
                            <asp:LinkButton ID="lbAddGroupChild" OnClick="lbAddGroupChild_Click" Enabled="false" Text="Add Child To Selected" runat="server"></asp:LinkButton></li>
                    </ul>
                </div>

                <div class="rollover-item" id="pnlRolloverConfig" runat="server">
                    <i class="fa fa-gear clickable js-show-config" onclick="$(this).closest('.js-grouptreeview').find('.js-config-panel').slideToggle()"></i>
                </div>
            </asp:Panel>

            <div class="js-config-panel" style="display: none" id="pnlConfigPanel" runat="server">
                <div class="row">
                    <div class="col-md-6">
                        <Rock:Toggle ID="tglHideInactiveGroups" runat="server" OnText="Active" OffText="All" Checked="true" ButtonSizeCssClass="btn-xs" OnCheckedChanged="tglHideInactiveGroups_CheckedChanged" Label="Show" />
                    </div>
                    <div class="col-md-6">
                        <Rock:Toggle ID="tglLimitPublicGroups" runat="server" OnText="Is Public" OffText="All" ButtonSizeCssClass="btn-xs" OnCheckedChanged="tglLimitPublicGroups_CheckedChanged" Label="Public" />
                    </div>
                </div>
                <Rock:RockDropDownList ID="ddlCountsType" runat="server" Label="Show Count For" OnSelectedIndexChanged="ddlCountsType_SelectedIndexChanged" CssClass="input-sm" AutoPostBack="true" />
                <Rock:CampusPicker ID="ddlCampuses" runat="server" Label="Filter by Campus" OnSelectedIndexChanged="ddlCampuses_SelectedIndexChanged" CssClass="input-sm" AutoPostBack="true" />
                <Rock:Toggle ID="tglIncludeNoCampus" runat="server" OnText="Yes" OffText="No" ButtonSizeCssClass="btn-xs" OnCheckedChanged="tglIncludeNoCampus_CheckedChanged" Label="Include groups with no campus" />
                <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="tbSearch" Text="Search" CssClass="control-label" />
                    <asp:Panel ID="pnlSearch" runat="server" DefaultButton="btnSearch" CssClass="input-group">
                        <asp:TextBox ID="tbSearch" runat="server" CssClass="form-control input-sm" />
                        <span class="input-group-btn">
                            <asp:LinkButton ID="btnSearch" runat="server" CssClass="btn btn-default btn-sm" OnClick="btnSearch_OnClick"><i class="fa fa-search"></i></asp:LinkButton>
                        </span>
                    </asp:Panel>
                </div>
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
            var <%=hfSelectedGroupId.ClientID%>IScroll = null;

            $(function () {
                var $selectedId = $('#<%=hfSelectedGroupId.ClientID%>'),
                    $expandedIds = $('#<%=hfInitialGroupParentIds.ClientID%>');

                var scrollbCategory = $('#<%=pnlTreeviewContent.ClientID%>').closest('.treeview-scroll');
                var scrollContainer = scrollbCategory.find('.viewport');
                var scrollIndicator = scrollbCategory.find('.track');
                <%=hfSelectedGroupId.ClientID%>IScroll = new IScroll(scrollContainer[0], {
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
                    .on('rockTree:selected', function (e, id) {
                        var groupSearch = '?GroupId=' + id;
                        var currentItemId = $selectedId.val();

                        if (currentItemId !== id) {

                            // get the data-id values of rock-tree items that are showing visible children (in other words, Expanded Nodes)
                            var expandedDataIds = $(e.currentTarget).find('.rocktree-children').filter(":visible").closest('.rocktree-item').map(function () {
                                return $(this).attr('data-id')
                            }).get().join(',');

                            var pageRouteTemplate = $('#<%=hfPageRouteTemplate.ClientID%>').val();
                            var locationUrl = "";
                            if (pageRouteTemplate.match(/{groupId}/i)) {
                                locationUrl = Rock.settings.get('baseUrl') + pageRouteTemplate.replace(/{groupId}/i, id);
                                locationUrl += "?ExpandedIds=" + encodeURIComponent(expandedDataIds);
                            }
                            else {
                                var detailPageUrl = $('#<%=hfDetailPageUrl.ClientID%>').val();
                                if (detailPageUrl) {
                                    locationUrl = detailPageUrl + groupSearch;
                                }
                                else {
                                    locationUrl = window.location.href.split('?')[0] + groupSearch;
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
                        restUrl: '<%=ResolveUrl( "~/api/groups/getchildren/" ) %>',
                        restParams: '?RootGroupId=' + ($('#<%=hfRootGroupId.ClientID%>').val() || 0)
                            + '&LimitToSecurityRoleGroups=' + ($('#<%=hfLimitToSecurityRoleGroups.ClientID%>').val() || false)
                            + '&IncludedGroupTypeIds=' + ($('#<%=hfGroupTypesInclude.ClientID%>').val() || '0')
                            + '&ExcludedGroupTypeIds=' + ($('#<%=hfGroupTypesExclude.ClientID%>').val() || '0')
                            + '&IncludeInactiveGroups=' + ($('#<%=hfIncludeInactiveGroups.ClientID%>').val() || false)
                            + '&CountsType=' + ($('#<%=hfCountsType.ClientID%>').val() || false)
                            + '&IncludeNoCampus=' + ($('#<%=hfIncludeNoCampus.ClientID%>').val() || false)
                        + '&CampusId=' + ($('#<%=hfCampusFilter.ClientID%>').val() || 0)
                        + '&LimitToPublic=' + ($('#<%=hfLimitPublicGroups.ClientID%>').val() || false),
                        multiSelect: false,
                        selectedIds: $selectedId.val() ? $selectedId.val().split(',') : null,
                        expandedIds: $expandedIds.val() ? $expandedIds.val().split(',') : null
                    });
            });

            function resizeScrollbar(scrollControl) {
                    var overviewHeight = $(scrollControl).find('.overview').height();

                    $(scrollControl).find('.viewport').height(overviewHeight);

                    if (<%=hfSelectedGroupId.ClientID%>IScroll) {
                        <%=hfSelectedGroupId.ClientID%>IScroll.refresh();
                    }
                }
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
