<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PageMap.ascx.cs" Inherits="RockWeb.Blocks.Cms.PageMap" %>

<asp:UpdatePanel ID="upPanel" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
    <ContentTemplate>
        <asp:HiddenField ID="hfRootPageId" runat="server" />
        <asp:HiddenField ID="hfInitialPageId" runat="server" />
        <asp:HiddenField ID="hfInitialPageParentIds" runat="server" />
        <asp:HiddenField ID="hfSelectedPageId" runat="server" />
        <asp:HiddenField ID="hfPageRouteTemplate" runat="server" />
        <asp:HiddenField ID="hfDetailPageUrl" runat="server" />
        <asp:HiddenField ID="hfSiteTypeList" runat="server" />
        <div class="treeview js-pagestreeview">
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title">Pages</h1>
                    <asp:Panel ID="divTreeviewActions" CssClass="panel-labels treeview-actions" runat="server">
                        <div id="divAddPage" runat="server" class="btn-group">
                            <button type="button" class="btn btn-link btn-xs dropdown-toggle" data-toggle="dropdown" title="<asp:Literal ID="ltAddPage" runat="server" Text=" Add Page" />">
                                <i class="fa fa-plus"></i>
                            </button>
                            <ul class="dropdown-menu dropdown-menu-right" role="menu">
                                <li>
                                    <asp:LinkButton ID="lbAddPageRoot" OnClick="lbAddPageRoot_Click" Text="Add Top-Level" runat="server"></asp:LinkButton></li>
                                <li>
                                    <asp:LinkButton ID="lbAddPageChild" OnClick="lbAddPageChild_Click" Text="Add Child To Selected" runat="server"></asp:LinkButton></li>
                            </ul>
                        </div>
                    </asp:Panel>
                </div>
                <div class="panel-body">                    
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
            </div>
        </div>

        <script type="text/javascript">
            var <%=pnlTreeviewContent.ClientID%>IScroll = null;

            function loadTreeView()
            {
                var $selectedId = $( '#<%=hfSelectedPageId.ClientID%>' ),
                    $expandedIds = $( '#<%=hfInitialPageParentIds.ClientID%>' );

                var scrollbPage = $( '#<%=pnlTreeviewContent.ClientID%>' ).closest( '.treeview-scroll' );
                var scrollContainer = scrollbPage.find( '.viewport' );
                var scrollIndicator = scrollbPage.find( '.track' );
                    <%=pnlTreeviewContent.ClientID%>IScroll = new IScroll( scrollContainer[ 0 ], {
                    mouseWheel: false,
                    eventPassthrough: true,
                    preventDefault: false,
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
                    preventDefaultException: { tagName: /.*/ }
                } );

                // resize scrollbar when the window resizes
                $( document ).ready( function ()
                {
                    $( window ).on( 'resize', function ()
                    {
                        resizeScrollbar( scrollbPage );
                    } );
                } );

                $( '#<%=pnlTreeviewContent.ClientID%>' )
                    .on( 'rockTree:selected', function ( e, id )
                    {
                        var itemSearch = '?Page=' + id;
                        var currentItemId = $selectedId.val();
                        var $li = $( this ).find( '[data-id="' + id + '"]' )
                        var rockTree = $( this ).data( 'rockTree' );

                        if ( currentItemId !== id )
                        {
                            // get the data-id values of rock-tree items that are showing visible children (in other words, Expanded Nodes)
                            var expandedDataIds = $( e.currentTarget ).find( '.rocktree-children' ).filter( ":visible" ).closest( '.rocktree-item' ).map( function ()
                            {
                                return $( this ).attr( 'data-id' )
                            } ).get().join( ',' );

                            $( '#<%=hfInitialPageParentIds.ClientID%>' ).val( expandedDataIds );


                            if ( $li.length > 1 )
                            {
                                for ( i = 0; i < $li.length; i++ )
                                {
                                    if ( !rockTree.selectedNodes[ 0 ].name === $li.find( 'span' ).text() )
                                    {
                                        $li = $li[ i ];
                                        break;
                                    }
                                }
                            }

                            if ( itemSearch )
                            {
                                var locationUrl = window.location.href.split( '?' )[ 0 ] + itemSearch;
                                locationUrl += "&ExpandedIds=" + encodeURIComponent( expandedDataIds ).toLowerCase();
                                locationUrl += "&Redirect=false"
                                if ( window.location != locationUrl )
                                {
                                    window.location = locationUrl;
                                }
                            }
                        }
                    } )
                    .on( 'rockTree:rendered rockTree:expand rockTree:collapse', function ()
                    {
                        // update viewport height
                        resizeScrollbar( scrollbPage );
                    } )
                    .rockTree( {
                        restUrl: '<%=ResolveUrl( "~/api/pages/getchildren/" ) %>',
                        restParams: '?RootGroupId=' + ( $( '#<%=hfRootPageId.ClientID%>' ).val() || 0 )
                            + '&siteType=' + ( $( '#<%=hfSiteTypeList.ClientID%>' ).val() || 0 )
                            + '&$orderby=Order',
                        multiSelect: false,
                        selectedIds: $selectedId.val() ? $selectedId.val().split( ',' ) : null,
                        expandedIds: $expandedIds.val() ? $expandedIds.val().split( ',' ) : null
                    } );
            }

            function resizeScrollbar(scrollControl) {
                var overviewHeight = $(scrollControl).find('.overview').height();

                $(scrollControl).find('.viewport').height(overviewHeight);

                if (<%=pnlTreeviewContent.ClientID%>IScroll) {
                        <%=pnlTreeviewContent.ClientID%>IScroll.refresh();
                }
            }

            /*
             * 2-DEC-2021: DMV
             *
             * JavaScript is not executed by AJAX on partial postbacks.
             * The end request handler will ensure that the tree is reloaded when that happens.
             * 
             */

            // This will run on initial page load
            loadTreeView();

            Sys.WebForms.PageRequestManager.getInstance().add_endRequest( endRequestHandler );
            function endRequestHandler ( sender, args )
            {
                // This will run on any partial postback
                loadTreeView();
            }
        </script>
    </ContentTemplate>
</asp:UpdatePanel>

