using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Web.UI;

namespace church.ccv.Badges.Info
{
    [Description( "Displays the Starting Point Status for the specific person." )]
    [Export( typeof( Rock.PersonProfile.BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Starting Point" )]
    
    [TextField( "Badge Color", "The color of the badge (#ffffff).", true, "#00ff00" )]
    class StartingPoint : Rock.PersonProfile.BadgeComponent
    {
        public override void Render( PersonBadgeCache badge, HtmlTextWriter writer )
        {            
            string badgeColor = GetAttributeValue( badge, "BadgeColor" );

            if ( !string.IsNullOrEmpty( badgeColor ) )
            {   
                writer.Write( string.Format(
                    @"<a class='badge badge-id-{0}' style='color: {1}' data-toggle='tooltip' data-container='body'> 
                        <i class='badge-icon badge-icon-id-{0} fa fa-map-marker badge-disabled'></i>
                    </a>",
                    badge.Id,
                    badgeColor.EscapeQuotes() ) );

                writer.Write( string.Format(
                    @"
                    <script>
                    Sys.Application.add_load(function () {{
                                                
                        $.ajax({{
                                type: 'GET',
                                url: Rock.settings.get('baseUrl') + 'api/CCV/Badges/TakenStartingPoint/{0}' ,
                                statusCode: {{
                                    200: function (data, status, xhr) {{
                                        var $badge = $('.badge.badge-id-{2}');
                                        
                                        if (data.Status == 2) {{
                                            $badge.find( '.badge-icon-id-{2}' ).removeClass( 'badge-disabled' );

                                            $badge.attr('data-original-title', '{1} is registered for Starting Point ' + data.DateFormatted );
                                            $badge.attr('href', '/page/113?GroupId=' + data.RegistrationGroupId );
                                            $badge.find( '.badge-icon-id-{2}' ).attr('style', 'opacity: .5' );

                                            $badge.append( '<sup style=\'margin-left: -4px;\'>R</sup>' );
                                            
                                        }} else if (data.Status == 1) {{
                                            $badge.attr('data-original-title', '{1} took Starting Point on ' + data.DateFormatted );
                                            $badge.find( '.badge-icon-id-{2}' ).removeClass( 'badge-disabled' );
                                        }}
                                        else {{
                                            $badge.attr('data-original-title', '{1} has not taken Starting Point' );
                                            $badge.attr('href', '/page/551?PersonGuid={3}&EventItemId=1' );
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
                     badge.Id,
                     Person.Guid.ToString( ) ) );
            }
        }
    }
}
