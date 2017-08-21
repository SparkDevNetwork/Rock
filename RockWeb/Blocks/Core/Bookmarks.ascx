<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Bookmarks.ascx.cs" Inherits="RockWeb.Blocks.Core.Bookmarks" %>



<div class="nav navbar-nav bookmark-block">
    <div id="liDropdown" runat="server">

        <asp:LinkButton ID="lIcon" runat="server" OnClick="btnCancel_Click" OnClientClick="return iconClick();" data-toggle="popover" data-placement="bottom" class="bookmark-popover">
            <i class="fa fa-bookmark-o fa-2x" aria-hidden="true"></i>
        </asp:LinkButton>
        <div class="row js-categorytreeview popovercontent dropdown-menu">
            <asp:UpdatePanel ID="upCategoryTree" runat="server">
                <ContentTemplate>
                    <div class="panel-body">
                        <div class="col-md-12">
                            <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" />
                            <asp:Panel ID="pnlBookmarkList" runat="server">
                                <h4><strong>Bookmarks</strong></h4>
                                <div class="treeview-scroll scroll-container scroll-container-vertical treeview-bookmark">
                                    <div class="scrollbar">
                                        <div class="track">
                                            <div class="thumb">
                                                <div class="end"></div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="viewport">
                                        <div class="overview">
                                            <div class="treeview-frame">
                                                <asp:Panel class="pnlTreeviewContent" runat="server">
                                                </asp:Panel>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </asp:Panel>
                            <asp:Panel ID="pnlBookmarkDetail" runat="server">
                                <div class="row">
                                    <div class="col-md-12">
                                        <Rock:DataTextBox ID="tbName" runat="server"
                                            SourceTypeName="Rock.Model.PersonBookmark, Rock" PropertyName="Name" />
                                    </div>
                                    <div class="col-md-12">
                                        <Rock:RockTextBox ID="tbUrl" runat="server" Label="URL" Required="true" RequiredErrorMessage="A URL is Required" />
                                    </div>
                                    <div class="col-md-12">
                                        <Rock:CategoryPicker ID="cpCategory" runat="server" Label="Category" EntityTypeName="Rock.Model.PersonBookmark" />
                                    </div>
                                </div>
                            </asp:Panel>
                            <div class="actions">
                                <asp:LinkButton ID="btnAdd" runat="server" Text="Add" CssClass="btn btn-xs btn-default	" OnClick="btnAdd_Click" />
                                <asp:LinkButton ID="btnManage" runat="server" Text="Manage" CssClass="btn btn-xs btn-default" OnClick="btnManage_Click" />
                                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-xs btn-default" OnClick="btnSave_Click" />
                                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-xs btn-link popover-cancel" OnClick="btnCancel_Click" CausesValidation="false" />
                            </div>

                        </div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
</div>
<script type="text/javascript">

    var scrollbCategory = $('.pnlTreeviewContent').closest('.treeview-scroll');
    scrollbCategory.tinyscrollbar({ sizethumb: 20, size: 120 });

    // resize scrollbar when the window resizes
    $(document).ready(function () {
        $(window).on('resize', function () {
            resizeScrollbar(scrollbCategory);
        });
    });

    // scrollbar hide/show
    var timerScrollHide;
    $("#<%=upCategoryTree.ClientID%>").on({

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


    function bookmarkTree() {
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

                // If this Tree Node represents a Category, add a prefix to its identifier to prevent collisions with other Entity identifiers.
                if (item.IsCategory) {
                    node.id = '<%= CategoryNodePrefix %>' + node.id;
                }
                if (item.Children && typeof item.Children.length === 'number') {
                    node.children = _mapCategories(item.Children);
                }

                return node;
            });

        };
        scrollbCategory.tinyscrollbar();

        var key = sessionStorage.getItem("com.rockrms.bookmark");

        $('.pnlTreeviewContent')
            .on('rockTree:selected', function (e, id) {

            })
            .on('rockTree:rendered', function () {
                var nodes = $(this).data();
                sessionStorage.setItem("com.rockrms.bookmark", JSON.stringify(nodes.rockTree));
                resizeScrollbar(scrollbCategory);

            });

        if (key && key != '') {
            var rockTreeobj = JSON.parse(key);

            $('.pnlTreeviewContent')
                .rockTree({
                    mapping: {
                        include: ['isCategory', 'entityId'],
                        mapData: _mapCategories,
                    }
                });

            var rockData = $('.pnlTreeviewContent').data('rockTree');
            rockData.nodes = rockTreeobj.nodes;
            rockData.render();
            rockData.options.restUrl = '<%= ResolveUrl( "~/api/categories/getchildren/" ) %>',
                rockData.options.restParams = '<%= RestParms %>'

        }
        else {
            $('.pnlTreeviewContent')
                .rockTree({
                    restUrl: '<%= ResolveUrl( "~/api/categories/getchildren/" ) %>',
                    restParams: '<%= RestParms %>',
                    mapping: {
                        include: ['isCategory', 'entityId'],
                        mapData: _mapCategories
                    }
                });
        }
    }

    function resizeScrollbar(scrollControl) {
        scrollControl.tinyscrollbar_update('relative');
    }

    function clearBookmarkCache() {
        sessionStorage.clear();
    }

    function iconClick() {

        var popup_content = $('.popovercontent').first();
        if (popup_content.is(":hidden")) {
            popup_content.slideToggle(function () {
                bookmarkTree();
            });
            
        }
        else {
            var rockData = $('.pnlTreeviewContent').data('rockTree');
            if (!rockData)
            {
                return true;
            }
                popup_content.slideUp(function () {
                    scrollbCategory.tinyscrollbar();
                });
        }
        return false;
    }

</script>
<asp:HiddenField ID="hfActionType" runat="server" />
<style>
    .bookmark-block {
        float: right;
        position: relative;
        padding-top: 12px;
        padding-right: 40px;
    }

        .bookmark-block .dropdown-menu {
            max-width: 320px;
            left: 50%;
            transform: translate(-100%, 0px);
        }

    .treeview-bookmark {
        width: 280px;
    }

        .treeview-bookmark .viewport {
            width: 265px;
            margin-bottom: 10px;
            max-height: 200px;
        }

    @media screen and (min-width: 992px) {
        .bookmark-block .dropdown-menu {
            transform: translate(-50%, 0px);
        }
    }
</style>
