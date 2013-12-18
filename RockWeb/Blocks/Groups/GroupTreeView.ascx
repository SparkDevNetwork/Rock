<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupTreeView.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupTreeView" %>

<asp:UpdatePanel ID="upGroupType" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
    <ContentTemplate>
        <asp:HiddenField ID="hfRootGroupId" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfInitialGroupId" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfGroupTypes" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfInitialGroupParentIds" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfLimitToSecurityRoleGroups" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="hfSelectedGroupId" runat="server" ClientIDMode="Static" />

        <div class="treeview">
            <div class="treeview-actions">
                <asp:LinkButton ID="lbAddGroup" runat="server" CssClass="add btn btn-mini btn-action" ToolTip="Add Group" CausesValidation="false" OnClick="lbAddGroup_Click">
                        <i class="fa fa-plus"></i> Group
                </asp:LinkButton>
            </div>

            <div class="treeview-scroll scroll-container scroll-container-horizontal">
                
                <div class="viewport">
                    <div class="overview">
                        <div class="panel-body treeview-frame">
                            <div id="treeview-content">
                            </div>
                        </div>
                    
                    </div>
                </div>
                <div class="scrollbar"><div class="track"><div class="thumb"><div class="end"></div></div></div></div>
            </div>
        </div>


        <script type="text/javascript">
            $(function () {
                var $selectedId = $('#hfSelectedGroupId'),
                    $expandedIds = $('#hfInitialGroupParentIds');

                var scrollbCategory = $('.treeview-scroll');
                scrollbCategory.tinyscrollbar({ axis: 'x', sizethumb: 60, size: 200 });

                // resize scrollbar when the window resizes
                $(document).ready(function () {
                    $(window).on('resize', function () {
                        resizeScrollbar(scrollbCategory);
                    });
                });

                $('#treeview-content')
                    .on('rockTree:selected', function (e, id) {
                        var groupSearch = '?groupId=' + id;
                        if (window.location.search.indexOf(groupSearch) === -1) {
                            var postUrl = window.location.href.split('?')[0] + groupSearch;

                            // get the data-id values of rock-tree items that are showing children (in other words, Expanded Nodes)
                            var expandedDataIds = $(e.currentTarget).find('.rocktree-children').closest('.rocktree-item').map(function () { return $(this).attr('data-id') }).get().join(',');

                            // to a form post to the newUrl
                            var form = $('<form action="' + postUrl + '" method="post">' +
                                            '<input type="hidden" name="expandedIds" value="' + expandedDataIds + '" />' +
                                         '</form>');
                            $('body').append(form);
                            $(form).submit();
                        }
                    })
                    .on('rockTree:rendered', function () {

                        // update viewport height
                        resizeScrollbar(scrollbCategory);

                    })
                    .rockTree({
                        restUrl: '<%=ResolveUrl( "~/api/groups/getchildren/" ) %>',
                        restParams: '/' + ($('#hfRootGroupId').val() || 0) + '/' + ($('#hfLimitToSecurityRoleGroups').val() || false) + '/' + ($('#hfGroupTypes').val() || 0),
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
