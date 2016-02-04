using System;
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock;
using Rock.Attribute;
using Rock.Web.Cache;

namespace church.ccv.Badges.Person
{
    /// <summary>
    /// Campus Badge
    /// </summary>
    [Description( "Displays the Steps Bar for the specific person." )]
    [Export( typeof( Rock.PersonProfile.BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Steps Bar" )]

    [LinkedPage("Baptism Registration Page", "The page to link to for registering for baptism.")]
    [IntegerField("Baptism Event Id", "The event id to use for pulling upcoming baptisms.")]
    [LinkedPage( "Connection Group Registration Page", "The page to link to for registering for connection group." )]
    [IntegerField( "Connection GroupType Id", "The id of the group type to be used for determine the connection badge." )]
    [LinkedPage("Group List Page", "The page to list all of the groups of a certain type")]
    [LinkedPage("Group Details Page", "The group details page.")]
    [IntegerField( "Serving GroupType Id", "The id of the group type to be used for determine the serve badge." )]
    [LinkedPage("Serving Connection Page", "The page to use for creating new serving connections.")]
    public class StepsBar : Rock.PersonProfile.BadgeComponent
    {
        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( PersonBadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            int baptismRegistrationPageId = GetPageIdFromLinkedPageAttribute( "BaptismRegistrationPage", badge);
            int connectionGroupRegistrationPageId = GetPageIdFromLinkedPageAttribute( "ConnectionGroupRegistrationPage", badge );
            int servingConnectionPageId = GetPageIdFromLinkedPageAttribute( "ServingConnectionPage", badge );
            int groupDetailsPageId = GetPageIdFromLinkedPageAttribute( "GroupDetailsPage", badge );
            int groupListPageId = GetPageIdFromLinkedPageAttribute( "GroupListPage", badge );

            writer.Write( string.Format( @"<div class='badge-group badge-group-steps js-badge-group-steps badge-id-{0}'>
                <a class='badge badge-baptism badge-icon step-nottaken' data-toggle='tooltip' data-original-title='{1} is not baptised' data-container='body' href='/page/{4}?PersonGuid={2}&EventItemId={3}'>
                    <i class='icon ccv-baptism'></i>
                </a>

                <div class='badge badge-membership badge-icon step-nottaken' data-toggle='tooltip' data-original-title='{1} is not a member' data-container='body'>
                    <i class='icon ccv-membership'></i>
                </div>
                <div class='badge badge-worship badge-icon step-nottaken' data-toggle='tooltip' data-original-title='{1} is not an eRA' data-container='body'>
                    <i class='icon ccv-worship'></i>
                </div>
                <a class='badge badge-connect badge-icon step-nottaken' data-toggle='tooltip' data-original-title='{1} is not connected to a neighborhood group' data-container='body' href='/page/{5}?PersonGuid={2}'>
                    <i class='icon ccv-connect'></i>
                </a>
                <div class='badge badge-tithe badge-icon step-nottaken' data-toggle='tooltip' data-original-title='{1} is not tithing' data-container='body'>
                    <i class='icon ccv-tithe'></i>
                </div>
                <div class='badge badge-serve badge-icon step-nottaken' data-toggle='tooltip' data-original-title='{1} is not serving (Click for more options)' data-container='body'>
                    <i class='icon ccv-serve'></i>
                </div>
                <div class='badge badge-share badge-icon step-nottaken' data-toggle='tooltip' data-original-title='Coming Soon...' data-container='body'>
                    <i class='icon ccv-share'></i>
                </div>
                <div class='badge badge-coach badge-icon step-nottaken' data-toggle='tooltip' data-original-title='{1} is not coaching' data-container='body'>
                    <i class='icon ccv-coach'></i>
                </div>
            </div>", 
                badge.Id // 0
                , Person.NickName // 1
                , Person.Guid // 2
                , GetAttributeValue(badge, "BaptismEventId") // 3
                , baptismRegistrationPageId // 4
                , connectionGroupRegistrationPageId // 5
            ) );
            
            //writer.Write(string.Format("<script src='{0}' type='text/javascript'></script>", System.Web.VirtualPathUtility.ToAbsolute( "~/Plugins/church_ccv/Badges/Scripts/steps-badge.js" ) ) );

            writer.Write( string.Format(
                @"
                    <script>
                    Sys.Application.add_load(function () {{

    var firstName = '{0}';

    var servingConnectionPageId = {4};
    var servingTeamGroupTypeId = {5};

    var connectGroupTypeId = {6};
    var connectionGroupRegistrationPage = '{7}';

    var groupDetailPageId = '{8}';
    var groupListPageId = '{9}';

    $.ajax({{
        type: 'GET',
        url: Rock.settings.get('baseUrl') + 'api/CCV/Badges/StepsBar/{1}',
        statusCode: {{
            200: function (data, status, xhr) {{

                var $badge = $('.js-badge-group-steps.badge-id-{3}');

                // baptism
                if (data.BaptismResult.BaptismStatus == 2) {{
                    var baptismRegistrationDate = new Date(data.BaptismResult.BaptismRegistrationDate);
                    var baptismRegistrationDateFormatted = (baptismRegistrationDate.getMonth() + 1) + '/' + baptismRegistrationDate.getDate() + '/' + baptismRegistrationDate.getFullYear();

                    $badge.find('.badge-baptism').attr('data-original-title', firstName + ' is registered to be baptised on ' + baptismRegistrationDateFormatted );
                    $badge.find('.badge-baptism').removeClass('step-nottaken');
                    $badge.find('.badge-baptism').addClass('step-partial');
                }} else if (data.BaptismResult.BaptismStatus == 1) {{
                    var baptismDate = new Date(data.BaptismResult.BaptismDate);
                    var baptismDateFormatted = (baptismDate.getMonth() + 1) + '/' + baptismDate.getDate() + '/' + baptismDate.getFullYear();

                    $badge.find('.badge-baptism').removeClass('step-nottaken');
                    $badge.find('.badge-baptism').attr('data-original-title', firstName + ' was baptised on ' + baptismDateFormatted + '');
                }}

                // member
                if (data.MembershipResult.IsMember) {{
                    var membershipDate = new Date(data.MembershipResult.MembershipDate);
                    var membershipDateFormatted = (membershipDate.getMonth() + 1) + '/' + membershipDate.getDate() + '/' + membershipDate.getFullYear();

                    $badge.find('.badge-membership').removeClass('step-nottaken');
                    $badge.find('.badge-membership').attr('data-original-title', firstName + ' became a member on ' + membershipDateFormatted + '');
                }}

                // worship
                if (data.IsWorshipper) {{
                    $badge.find('.badge-worship').removeClass('step-nottaken');
                    $badge.find('.badge-worship').attr('data-toggle', 'tooltip');
                    $badge.find('.badge-worship').attr('data-original-title', firstName + ' is an eRA');
                    $badge.find('.badge-worship').attr('data-container', 'body');
                    $badge.find('.badge-worship').tooltip();
                }}

                // connect
                if (data.ConnectionResult.ConnectionStatus != 1) {{

                    // create content for popover
                    var popoverContent = firstName + "" is in the following connection groups: <ul styling='padding-left: 20px;' > "";

                    $.each( data.ConnectionResult.Groups, function( index, group ) {{

                popoverContent = popoverContent + ""<li><a href='/page/"" + groupDetailPageId + ""?GroupId="" + group.GroupId + ""'>"" + group.GroupName + ""</a></li>"";

                // only display 2
                if ( index == 1 )
                {{
                    return false;
                }}
            }});

            var popoverContent = popoverContent + ""</ul>""

                    // check for more than two groups
            if ( data.ConnectionResult.Groups.length > 2 )
            {{
                var moreCount = data.ConnectionResult.Groups.length - 2;

                if ( moreCount == 1 )
                {{
                    popoverContent = popoverContent + ""<p>and <a href='/page/"" + groupListPageId + ""?PersonId="" + '{2}' + ""&GroupTypeId="" + connectGroupTypeId + ""'> "" + moreCount + "" other</a></p>"";
                }}
                else {{
                    popoverContent = popoverContent + ""<p>and <a href='/page/"" + groupListPageId + ""?PersonId="" + '{2}' + ""&GroupTypeId="" + connectGroupTypeId + ""'> "" + moreCount + "" others</a></p>"";
                }}
            }}

            var popoverContent = popoverContent + ""<p class='margin-b-none'><a href='/page/"" + connectionGroupRegistrationPage + ""?PersonGuid={1}' class='btn btn-primary btn-block btn-xs'>Find Group</a></p>"";

                    $badge.find( '.badge-connect' ).removeClass( 'step-nottaken' );
                    $badge.find( '.badge-connect' ).attr( 'data-toggle', 'popover' );
                    $badge.find( '.badge-connect' ).attr( 'data-container', 'body' );
                    $badge.find( '.badge-connect' ).attr( 'data-content', popoverContent );
                    $badge.find( '.badge-connect' ).attr( 'data-original-title', firstName + ' is in a connection group &nbsp;&nbsp;<i class=""fa fa-mouse-pointer""></i>' );

            var connectPopoverIsOpen = false;

                    $badge.find( '.badge-connect' ).popover({{
                html: true,
                        placement: 'top',
                        trigger: 'manual'
                      }});

                    // disable the anchor tag
                    $badge.find( '.badge-connect' ).on( ""click"", function( e ) {{
                e.preventDefault();
            }});


                    // fancy pants to allow the tooltip and popover to work on the same control
                    $badge.find( '.badge-connect' ).on( 'click', function() {{
                if ( connectPopoverIsOpen )
                {{
                            $badge.find( '.badge-connect' ).popover( 'hide' );
                            connectPopoverIsOpen = false;
                            $badge.find( '.badge-connect' ).attr( 'data-original-title', firstName + ' is in a connection group &nbsp;&nbsp;<i class=""fa fa-mouse-pointer""></i>' );
                }}
                else {{
                            $badge.find( '.badge-connect' ).attr( 'data-original-title', '' );
                            $badge.find( '.badge-connect' ).popover( 'show' );
                            connectPopoverIsOpen = true;
                            $badge.find( '.badge-connect' ).tooltip( 'hide' );
                }}
            }});
        }}

                // tithing
                if (data.IsTithing) {{
                    $badge.find('.badge-tithe').removeClass('step-nottaken');
    }}

                // serving
                if (data.ServingResult.IsServing) {{

                    // create content for popover
                    var popoverContent = firstName + "" is on the following serving teams: <ul styling='padding-left: 20px;'>"";

                    $.each( data.ServingResult.Groups, function (index, group)
    {{

        popoverContent = popoverContent + ""<li><a href='/page/"" + groupDetailPageId + ""?GroupId="" + group.GroupId + ""'>"" + group.GroupName + ""</a></li>"";

        // only display 2
        if ( index == 1 )
        {{
            return false;
        }}
    }});

                    var popoverContent = popoverContent + ""</ul>""

                    // check for more than two groups
                    if (data.ServingResult.Groups.length > 2) {{
                        var moreCount = data.ServingResult.Groups.length - 2;

                        if (moreCount == 1) {{
                            popoverContent = popoverContent + ""<p>and <a href='/page/"" + groupListPageId + ""?PersonId="" + '{2}' + ""&GroupTypeId="" + servingTeamGroupTypeId + ""'> "" + moreCount + "" other</a></p>"";
                        }}
                        else {{
                            popoverContent = popoverContent + ""<p>and <a href='/page/"" + groupListPageId + ""?PersonId="" + '{2}' + ""&GroupTypeId="" + servingTeamGroupTypeId + ""'> "" + moreCount + "" others</a></p>"";
                        }}
                    }}

                    var popoverContent = popoverContent + ""<p class='margin-b-none'><a href='/page/"" + servingConnectionPageId + ""?PersonGuid={1}' class='btn btn-primary btn-block btn-xs'>Connect</a></p>"";

                    $badge.find('.badge-serve').removeClass('step-nottaken');
                    $badge.find('.badge-serve').attr('data-toggle', 'popover');
                    $badge.find('.badge-serve').attr('data-container', 'body');
                    $badge.find('.badge-serve').attr('data-content', popoverContent);
                    $badge.find('.badge-serve').attr('data-original-title', firstName + ' is serving &nbsp;&nbsp;<i class=""fa fa-mouse-pointer""></i>');

var servingPopoverIsOpen = false;

                    $badge.find('.badge-serve').popover({{
    html: true,
                        placement: 'top',
                        trigger: 'manual'
                    }});
                    
                    // fancy pants to allow the tooltip and popover to work on the same control
                    $badge.find('.badge-serve').on('click', function ()
{{
    if ( servingPopoverIsOpen )
    {{
                            $badge.find( '.badge-serve' ).popover( 'hide' );
        servingPopoverIsOpen = false;
                            $badge.find( '.badge-serve' ).attr( 'data-original-title', firstName + ' is serving &nbsp;&nbsp;<i class=""fa fa-mouse-pointer""></i>' );
    }}
    else {{
                            $badge.find( '.badge-serve' ).attr( 'data-original-title', '' );
                            $badge.find( '.badge-serve' ).popover( 'show' );
        servingPopoverIsOpen = true;
                            $badge.find( '.badge-serve' ).tooltip( 'hide' );
    }}
}});
                }}

                

                // sharing
                // not implemented yet

                // coaching
                if (data.CoachingResult.IsCoaching) {{

                    // create content for popover
                    var popoverContent = firstName + "" is in the following coaching groups: <ul styling='padding-left: 20px;'>"";

                    $.each( data.CoachingResult.Groups, function (index, group)
{{

    popoverContent = popoverContent + ""<li><a href='/page/"" + groupDetailPageId + ""?GroupId="" + group.GroupId + ""'>"" + group.GroupName + ""</a></li>"";

    // only display 2
    if ( index == 1 )
    {{
        return false;
    }}
}});

                    var popoverContent = popoverContent + ""</ul>"";

                    // check for more than two groups
                    if (data.CoachingResult.Groups.length > 2) {{
                        var moreCount = data.CoachingResult.Groups.length - 2;

                        if (moreCount == 1) {{
                            popoverContent = popoverContent + ""<p>and "" + moreCount + "" other (see groups tab for details)</p>"";
                        }}
                        else {{
                            popoverContent = popoverContent + ""<p>and "" + moreCount + "" others (see groups tab for details)</p>"";
                        }}
                    }}

                    $badge.find('.badge-coach').removeClass('step-nottaken');
                    $badge.find('.badge-coach').attr('data-toggle', 'popover');
                    $badge.find('.badge-coach').attr('data-container', 'body');
                    $badge.find('.badge-coach').attr('data-content', popoverContent);
                    $badge.find('.badge-coach').attr('data-original-title', firstName + ' is in a coaching group &nbsp;&nbsp;<i class=""fa fa-mouse-pointer""></i>');

var coachPopoverIsOpen = false;

                    $badge.find('.badge-coach').popover({{
    html: true,
                        placement: 'top',
                        trigger: 'manual'
                    }});

                    // fancy pants to allow the tooltip and popover to work on the same control
                    $badge.find('.badge-coach').on('click', function ()
{{
    if ( coachPopoverIsOpen )
    {{
                            $badge.find( '.badge-coach' ).popover( 'hide' );
                            coachPopoverIsOpen = false;
                            $badge.find( '.badge-coach' ).attr( 'data-original-title', firstName + ' is in a coaching group &nbsp;&nbsp;<i class=""fa fa-mouse-pointer""></i>' );
    }}
    else {{
                            $badge.find( '.badge-coach' ).attr( 'data-original-title', '' );
                            $badge.find( '.badge-coach' ).popover( 'show' );
                            coachPopoverIsOpen = true;
                            $badge.find( '.badge-coach' ).tooltip( 'hide' );
    }}
}});
                }}
            }}
        }},
    }});
}});
                    </script>",
                 Person.NickName, // 0
                 Person.Guid.ToString(), // 1
                 Person.Id, // 2
                 badge.Id, // 3  
                 servingConnectionPageId, // 4
                 GetAttributeValue( badge, "ServingGroupTypeId" ).AsInteger(), // 5
                 GetAttributeValue( badge, "ConnectionGroupTypeId").AsInteger(), // 6
                 connectionGroupRegistrationPageId, // 7
                 groupDetailsPageId, // 8
                 groupListPageId // 9
            ));
        }

        private int GetPageIdFromLinkedPageAttribute(string attributeKey, PersonBadgeCache badge )
        {
            int pageId = -1;

            Guid pageGuid = Guid.Empty;
            if ( Guid.TryParse( GetAttributeValue( badge, attributeKey ), out pageGuid ) )
            {
                pageId = Rock.Web.Cache.PageCache.Read( pageGuid ).Id;
            }

            return pageId;
        }
    }
}