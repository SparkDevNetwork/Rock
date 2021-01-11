// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Badge.Component
{
    /// <summary>
    /// Geofenced By Group Badge
    /// </summary>
    [Description( "Displays the group(s) of a particular type that have a geo-fence location around one or more of the person's map locations." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Geofenced By Group" )]

    [GroupTypeField( "Group Type", "The type of group to use.", true )]
    [TextField( "Badge Color", "The color of the badge (#ffffff).", true, "#0ab4dd" )]
    public class GeofencedByGroup : BadgeComponent
    {
        /// <summary>
        /// Determines of this badge component applies to the given type
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public override bool DoesApplyToEntityType( string type )
        {
            return type.IsNullOrWhiteSpace() || typeof( Person ).FullName == type;
        }

        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( BadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            if ( Person == null )
            {
                return;
            }

            string badgeColor = GetAttributeValue( badge, "BadgeColor" );

            if ( !badgeColor.IsNullOrWhiteSpace() )
            {
                writer.Write( string.Format(
                    "<span class='label badge-geofencing-group badge-id-{0}' style='background-color:{1};display:none' ></span>",
                    badge.Id, badgeColor.EscapeQuotes() ) );
            }
        }

        /// <summary>
        /// Gets the java script.
        /// </summary>
        /// <param name="badge"></param>
        /// <returns></returns>
        protected override string GetJavaScript( BadgeCache badge )
        {
            var groupTypeGuid = GetAttributeValue( badge, "GroupType" ).AsGuidOrNull();

            if ( Person == null || !groupTypeGuid.HasValue )
            {
                return null;
            }

            return string.Format( @"
$.ajax({{
    type: 'GET',
    url: Rock.settings.get('baseUrl') + 'api/Badges/GeofencingGroups/{0}/{1}' ,
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
}});", Person.Id.ToString(), groupTypeGuid.ToString(), badge.Id );
        }
    }
}