<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StarkDetail.ascx.cs" Inherits="RockWeb.Blocks.Utility.StarkDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block panel-next">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-star"></i>
                    <span>Blank Detail Block</span>
                </h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlblTest" runat="server" LabelType="Info" Text="Label" />
                </div>

                <div class="panel-toolbar" role="menu">
                    <div id="ctl00_main_ctl33_ctl01_ctl06_pnlFollowing" class="panel-follow-status js-follow-status" data-toggle="tooltip" data-placement="top" title="" data-original-title="Click to Follow">
                    </div>
                </div>

                <div class="panel-toolbar" role="menu">
                    <a href="#" class="btn btn-toolbar-master" data-toggle="dropdown" aria-expanded="false"><i class="fa fa-ellipsis-v"></i></a>
                    <ul id="menu1" class="dropdown-menu" aria-labelledby="drop4">
                    <li><a href="#"><i class="js-selectionicon fa fa-fw"></i> Settings</a></li>
                    <li><a href="#"><i class="js-selectionicon fa fa-fw"></i> Fullscreen</a></li>
                    <li role="separator" class="divider"></li>
                    <li><a href="#" class="js-drawershow"><i class="js-selectionicon fa fa-fw"></i> <span class="text-truncate">Show Details</span></a></li>
                    </ul>
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-subheading">
                <div class="pull-left">
                <Rock:TagList ID="taglPersonTags" runat="server" CssClass="clearfix" />
                </div>

                <div class="badges pull-right">
                    <i class="fa fa-check-circle" style="color:#83758F"></i>
                    <i class="fa fa-flag-checkered" style="color:#16C98D"></i>
                </div>
            </div>
            <div class="panel-body">

                <span class="label label-default">Default</span>
                <span class="label label-primary">Primary</span>
                <span class="label label-success">Success</span>
                <span class="label label-info">Info</span>
                <span class="label label-warning">Warning</span>
                <span class="label label-danger">Danger</span>

            </div>

        </asp:Panel>


<script>
Sys.Application.add_load(function () {

    $('.js-drawershow').on('click', function () {
        var link = $(this);
        $( this ).closest( '.panel' ).find('.panel-drawer').toggleClass('open').find( '.drawer-content' ).slideToggle(function(){
            if ($(this).is(':visible')) {
                link.find('span').text('Hide Details').prop('title', 'Hide additional addresses');
                link.find('.js-selectionicon').toggleClass('fa-check');
            } else {
                link.find('span').text('Show Details').prop('title', 'Show additional addresses');
                link.find('.js-selectionicon').toggleClass('fa-check');
            }
        });

        $expanded = $(this).children('input.filter-expanded');
        $expanded.val($expanded.val() == 'True' ? 'False' : 'True');


    });

});

</script>

    </ContentTemplate>
</asp:UpdatePanel>

        <!-- var icon = $( this ).find( 'i' );
        var iconOpenClass = icon.attr( 'data-icon-open' ) || 'fa fa-chevron-up';
        var iconCloseClass = icon.attr( 'data-icon-closed' ) || 'fa fa-chevron-down';

        if ($( this ).closest( '.panel-drawer' ).hasClass( 'open' )) {
            icon.attr( 'class', iconOpenClass );
        }
        else {
            icon.attr( 'class', iconCloseClass );
        }

                    $('.address-extended').slideToggle(function() {
                        if ($(this).is(':visible')) {
                            link.text('Show Less').prop('title', 'Hide additional addresses');
                        } else {
                            link.text('Show More').prop('title', 'Show additional addresses');
                        }
                        $("#account_entry").height($("#individual_details").height());
                    }); -->