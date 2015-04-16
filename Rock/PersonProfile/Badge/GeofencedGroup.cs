// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.PersonProfile.Badge
{
    /// <summary>
    /// Campus Badge
    /// </summary>
    [Description( "Displays the group(s) of a particular type that have a geo-fence location around one or more of the the person's map locations." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Geofenced Group" )]

    [GroupTypeField( "Group Type", "The type of group to use.", true )]
    [TextField( "Badge Color", "The color of the badge (#ffffff).", true, "#0ab4dd" )]
    public class GeofencedGroup : BadgeComponent
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

            if ( groupTypeGuid.HasValue &&  !String.IsNullOrEmpty( badgeColor ) )
            {
                writer.Write( String.Format( 
                    "<span title='{0}' data-toggle='tooltip' class='label badge-geofenced-group badge-id-{1}' style='background-color:{2};display:none' ></span>", 
                    badge.Name.EscapeQuotes(), badge.Id, badgeColor.EscapeQuotes() ) );

                writer.Write( String.Format( @"
<script>
Sys.Application.add_load(function () {{
                                                
    $.ajax({{
            type: 'GET',
            url: Rock.settings.get('baseUrl') + 'api/PersonBadges/GeofencedGroups/{0}/{1}' ,
            statusCode: {{
                200: function (result, status, xhr) {{
                    var $badge = $('.badge-geofenced-group.badge-id-{3}');
                    var badgeHtml = '';
                    if (result && result != '') {{
                        badgeHtml = '<span class=\'label label-{2}\'>' + result + '</span>';
                        $badge.show();
                    }} else {{
                        $badge.hide();
                    }}
                    $badge.html(badgeHtml);
                }}
            }},
    }});
}});
</script>
                
", Person.Id.ToString(), groupTypeGuid.ToString(), badgeColor, badge.Id ) );
            }

        }
    }
}