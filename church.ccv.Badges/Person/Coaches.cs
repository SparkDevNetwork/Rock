using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;


namespace church.ccv.Badges.Person
{
    /// <summary>
    /// Coaches Badge
    /// </summary>
    [Description( "Displays the coaches for the specific person." )]
    [Export( typeof( Rock.PersonProfile.BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Coaches" )]

    [TextField( "Group Types", "List of group types to display." )]
    [TextField( "Label", "Label to display badge.", true, "Coach" )]
    [TextField( "Badge Color", "The color of the badge (#ffffff).", true, "#ee7624" )]
    class Coaches : Rock.PersonProfile.BadgeComponent
    {
        public override void Render( PersonBadgeCache badge, HtmlTextWriter writer )
        {
            var groupTypeIds = GetAttributeValue( badge, "GroupTypes" );
            string label = GetAttributeValue( badge, "Label" );
            string badgeColor = GetAttributeValue( badge, "BadgeColor" );

            if ( groupTypeIds != String.Empty && !string.IsNullOrEmpty( badgeColor ) && !string.IsNullOrEmpty( label ) )
            {
                writer.Write( string.Format(
                    "<span class='label badge-coaches badge-id-{0}' style='background-color:{1};display:none' ></span>",
                    badge.Id,
                    badgeColor.EscapeQuotes() ) );

                writer.Write( string.Format(
                    @"
                    <script>
                    Sys.Application.add_load(function () {{
                                                
                        $.ajax({{
                                type: 'GET',
                                url: Rock.settings.get('baseUrl') + 'api/CCV/Badges/Coaches/{0}/{1}' ,
                                statusCode: {{
                                    200: function (data, status, xhr) {{
                                        var $badge = $('.badge-coaches.badge-id-{2}');
                                        var badgeHtml = '';

                                        $.each(data, function() {{
                                            if ( badgeHtml != '' ) {{ 
                                                badgeHtml += ' | ';
                                            }}
                                            badgeHtml += '<span title=""' + this.LeaderNames + '"" data-toggle=""tooltip"">{3}</span>';
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
                     groupTypeIds.ToString(),
                     badge.Id,
                     label ) );
            }
        }
    }
}
