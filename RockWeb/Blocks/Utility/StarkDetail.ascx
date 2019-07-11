<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StarkDetail.ascx.cs" Inherits="RockWeb.Blocks.Utility.StarkDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block panel-next">

            <div class="panel-heading">
                <div class="panel-meta">
                    <h1 class="panel-title">
                        <i class="fa fa-star"></i><!--
                        --><span>Blank Detail Block</span>
                    </h1>



                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="hlblTest" runat="server" LabelType="Info" Text="Label" />
                        <span class="label label-type" title="" data-toggle="tooltip" data-original-title="A group of people who share an interest and meet together with regular frequency."><a href="/page/117?groupTypeId=25">Small Group</a></span>
                        <span class="label label-campus">Main Campus</span>
                    </div>
                </div>
                    <div class="panel-toolbar" role="menu">
                        <div class="panel-follow-status js-follow-status" title="Click to Follow"></div>

                        <div class="rock-fullscreen-toggle js-fullscreen-trigger"></div>

                        <a href="#" class="btn btn-toolbar-master" data-toggle="dropdown" aria-expanded="false"><i class="fa fa-ellipsis-v"></i></a>
                        <ul id="menu1" class="dropdown-menu" aria-labelledby="drop4">
                        <li><a href="#"><i class="js-selectionicon fa fa-fw"></i> Settings</a></li>
                        <li><a href="#" class="js-fullscreen-trigger"><i class="js-selectionicon fa fa-fw"></i> Fullscreen</a></li>
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

<!-- DEMO DATA START -->
<fieldset id="ctl00_main_ctl33_ctl01_ctl06_fieldsetViewDetails">

                        <div class="taglist">
                            <div class="taglist clearfix">
					<div class="tag-wrap">

					</div>
				</div>
                        </div>




    <p class="description">Small group with Ted as the group leader.</p>

<div class="row">
   <div class="col-md-6">
        <dl>

            <dt> Parent Group <!-- dt-->
               </dt><dd>Section A</dd>





        </dl>
        <dl>

        <dt>Topic:</dt>

<dd>Book of Genesis </dd>

        </dl>
    </div>

    <div class="col-md-6 location-maps">














	    	<div class="group-location-map">

	    	    <h4> Meeting Location </h4>

	    	    <a href="/group/111/map">
	    	    <img class='img-thumbnail' src='//maps.googleapis.com/maps/api/staticmap?style=feature:all|element:all|saturation:-100|gamma:1|&style=feature:all|element:labels.text.stroke|visibility:off|&style=feature:poi|element:all|visibility:off|&style=feature:road|element:geometry|visibility:simplified|&style=feature:water|element:all|visibility:on|color:0xc6dfec|&style=feature:administrative.neighborhood|element:labels.text.fill|visibility:off|&style=feature:road.local|element:labels.text|weight:0.5|color:0x333333|&style=feature:transit.station|element:labels.icon|visibility:off|&markers=color:0xee7624|33.593043,-112.126518&visual_refresh=true&path=fillcolor:0xe71e2255|color:0xe71e2255|&sensor=false&size=450x250&zoom=13&format=png&scale=2&key=AIzaSyB5e4KyzZ8m-Jhr7jIXhC6DLYnF44V1hho'/>
                </a>

	    	    11624 N 31st Dr
Phoenix, AZ 85029

	    	 </div>










	</div>
</div>

                        <div class="actions">
                            <a id="ctl00_main_ctl33_ctl01_ctl06_btnEdit" accesskey="m" title="Alt+m" class="btn btn-primary" href="javascript:__doPostBack('ctl00$main$ctl33$ctl01$ctl06$btnEdit','')">Edit</a>


                            <a id="ctl00_main_ctl33_ctl01_ctl06_btnArchive" class="btn btn-destructive js-archive-group" href="javascript:__doPostBack('ctl00$main$ctl33$ctl01$ctl06$btnArchive','')">Archive</a>
                            <span class="pull-right">

                                <a id="ctl00_main_ctl33_ctl01_ctl06_hlGroupHistory" title="Group History" class="btn btn-sm btn-square btn-default" href="/group/111/history"><i class="fa fa-history"></i></a>

                                <a id="ctl00_main_ctl33_ctl01_ctl06_hlAttendance" title="Attendance" class="btn btn-sm btn-square btn-default" href="/group/111/attendance"><i class="fa fa-check-square-o"></i></a>
                                <a id="ctl00_main_ctl33_ctl01_ctl06_hlMap" title="Interactive Map" class="btn btn-sm btn-square btn-default" href="/group/111/map"><i class="fa fa-map-marker"></i></a>

                                <a href="javascript: Rock.controls.modal.show($(this), '/Secure/16/111?t=Secure+Group&amp;pb=&amp;sb=Done')" id="ctl00_main_ctl33_ctl01_ctl06_btnSecurity" class="btn btn-sm btn-square btn-security" title="Secure Group"><i class="fa fa-lock"></i></a>
                            </span>
                        </div>

                    </fieldset>
<!-- DEMO DATA END -->

            </div>

        </asp:Panel>


<script>
Sys.Application.add_load(function () {
    Rock.controls.fullScreen.initialize();

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