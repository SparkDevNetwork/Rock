using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Web.UI;

namespace church.ccv.Badges.Info
{
    [Description( "Displays the Membership Status for the specific person." )]
    [Export( typeof( Rock.PersonProfile.BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Member" )]
    
    [TextField( "Badge Color", "The color of the badge (#ffffff).", true, "#00ff00" )]
    class Member : Rock.PersonProfile.BadgeComponent
    {
        public override void Render( PersonBadgeCache badge, HtmlTextWriter writer )
        {            
            string badgeColor = GetAttributeValue( badge, "BadgeColor" );

            if ( !string.IsNullOrEmpty( badgeColor ) )
            {   
                writer.Write( string.Format(
                    @"<div class='badge badge-id-{0}' style='color: {1}' data-toggle='tooltip' data-container='body'> 
                        <i class='badge-icon badge-icon-id-{0} fa fa-check-square badge-disabled'></i>
                    </div>",
                    badge.Id,
                    badgeColor.EscapeQuotes() ) );

                writer.Write( string.Format(
                    @"
                    <script>
                    Sys.Application.add_load(function () {{
                                                
                        $.ajax({{
                                type: 'GET',
                                url: Rock.settings.get('baseUrl') + 'api/CCV/Badges/IsMember/{0}' ,
                                statusCode: {{
                                    200: function (data, status, xhr) {{
                                        var $badge = $('.badge.badge-id-{2}');
                                        
                                        if( data.MembershipDate == null )
                                        {{
                                            $badge.attr( 'data-original-title', '{1} is not a member' );
                                        }}
                                        else
                                        {{
                                            var membershipDate = new Date(data.MembershipDate);
                                            var membershipDateFormatted = (membershipDate.getMonth() + 1) + '/' + membershipDate.getDate() + '/' + membershipDate.getFullYear();
                                            $badge.attr( 'data-original-title', '{1} became a member on ' + membershipDateFormatted + '' );

                                            $badge.find( '.badge-icon-id-{2}' ).removeClass( 'badge-disabled' );
                                        }}

                                        $badge.show();
                                    }}
                                }},
                        }});
                    }});
                    </script>
                
                    ",
                     Person.Id.ToString(),
                     Person.NickName,
                     badge.Id ) );
            }
        }
    }
}
