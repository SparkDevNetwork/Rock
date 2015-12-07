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
    [Description( "Displays the campus group(s) of a particular type that have a geo-fence location around one or more of the the person's map locations for their campus." )]
    [Export( typeof( Rock.PersonProfile.BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Geofenced By Campus Group" )]

    [GroupTypeField( "Group Type", "The type of group to use.", true )]
    [TextField( "Badge Color", "The color of the badge (#ffffff).", true, "#0ab4dd" )]
    public class GeofencedByCampusGroup : Rock.PersonProfile.BadgeComponent
    {
        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( PersonBadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            Guid? groupTypeGuid = GetAttributeValue( badge, "GroupType" ).AsGuid();
            string badgeColor = GetAttributeValue( badge, "BadgeColor" );

            if ( groupTypeGuid.HasValue && !string.IsNullOrEmpty( badgeColor ) )
            {
                writer.Write( string.Format(
                    "<span class='label badge-geofencing-group badge-id-{0}' style='background-color:{1};display:none' ></span>",
                    badge.Id,
                    badgeColor.EscapeQuotes() ) );

                writer.Write( string.Format( 
                    @"
                    <script>
                    Sys.Application.add_load(function () {{
                                                
                        $.ajax({{
                                type: 'GET',
                                url: Rock.settings.get('baseUrl') + 'api/CCV/Badges/GeofencingCampusGroups/{0}/{1}' ,
                                statusCode: {{
                                    200: function (data, status, xhr) {{
                                        var $badge = $('.badge-geofencing-group.badge-id-{2}');
                                        var badgeHtml = '';

                                        $.each(data, function() {{
                                            if ( badgeHtml != '' ) {{ 
                                                badgeHtml += ' | ';
                                            }}
                                            badgeHtml += '<span title=""' + this.LeaderNames + '"" data-toggle=""tooltip"">' + this.GroupName + '</span>';
                                        }});

                                        if (badgeHtml != '') {{
                                            $badge.show('fast');
                                        }} else {{
                                            $badge.hide();
                                        }}
                                        $badge.html(badgeHtml);
                                        $badge.find('span').tooltip();
                                    }}
                                }},
                        }});
                    }});
                    </script>
                
                    ",
                     Person.Id.ToString(),
                     groupTypeGuid.ToString(),
                     badge.Id ) );
            }
        }
    }
}