using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Web.UI;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.PersonProfile.Badge
{
    /// <summary>
    /// Campus with Leader Badge
    /// </summary>
    [Description( "Campus with Leader Badge" )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Campus with Leader" )]
    public class CampusWithLeader : Rock.PersonProfile.BadgeComponent
    {

        /// <summary>
        /// Gets the badge label
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public override void Render( PersonBadgeCache badge, HtmlTextWriter writer )
        {
            writer.Write( string.Format(
                "<span class='label label-campus badge-campuswithleaders badge-id-{0}' style='display:none' ></span>",
                badge.Id ) );

            writer.Write( string.Format(
                @"
                <script>
                Sys.Application.add_load(function () {{
                                                
                    $.ajax({{
                            type: 'GET',
                            url: Rock.settings.get('baseUrl') + 'api/CCV/Badges/CampusesWithLeader/{0}' ,
                            statusCode: {{
                                200: function (data, status, xhr) {{
                                    var $badge = $('.badge-campuswithleaders.badge-id-{1}');
                                    var badgeHtml = '';

                                    $.each(data, function() {{
                                        if ( badgeHtml != '' ) {{ 
                                            badgeHtml += ' | ';
                                        }}
                                        badgeHtml += '<span title=""' + this.LeaderNames + '"" data-toggle=""tooltip"">' + this.CampusNames + '</span>';
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
                    badge.Id ) );
        }
    }
}
