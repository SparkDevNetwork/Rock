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
using System.IO;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Badge.Component
{
    /// <summary>
    /// In Group Of Type Badge
    /// </summary>
    [Description( "Shows badge if the individual is in a group of a specified type." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "In Group Of Type" )]

    [GroupTypeField( "Group Type", "The type of group to use.", true )]
    [ColorField( "Badge Color", "The color of the badge (#ffffff).", true, defaultValue: "#0ab4dd" )]
    [Rock.SystemGuid.EntityTypeGuid( "E7CE9210-FE85-4772-9225-E6C721B816BD")]
    public class InGroupOfType : BadgeComponent
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

        /// <inheritdoc/>
        public override void Render( BadgeCache badge, IEntity entity, TextWriter writer )
        {
            if ( !( entity is Person ) )
            {
                return;
            }

            if ( !String.IsNullOrEmpty( GetAttributeValue( badge, "GroupType" ) ) )
            {
                writer.Write( String.Format( "<div class='rockbadge rockbadge-icon rockbadge-ingroupoftype rockbadge-id-{0}' data-html='true'  data-toggle='tooltip' data-original-title=''>", badge.Id ) );

                writer.Write( "</div>" );
            }

        }

        /// <inheritdoc/>
        protected override string GetJavaScript( BadgeCache badge, IEntity entity )
        {
            var groupTypeGuid = GetAttributeValue( badge, "GroupType" ).AsGuidOrNull();

            if ( !groupTypeGuid.HasValue || !( entity is Person person ) )
            {
                return null;
            }

            var badgeColor = GetAttributeValue( badge, "BadgeColor" );

            if ( badgeColor.IsNullOrWhiteSpace() )
            {
                badgeColor = "#0ab4dd";
            }

            return string.Format( @"
$.ajax({{
    type: 'GET',
    url: Rock.settings.get('baseUrl') + 'api/Badges/InGroupOfType/{0}/{1}' ,
    statusCode: {{
        200: function (data, status, xhr) {{
            var groupIcon = data.GroupTypeIconCss;

            if (groupIcon == '') {{
                groupIcon = 'fa fa-times';
            }}

            var badgeHtml = '<i class=\'badge-icon ' + groupIcon + '\'></i>';
            if (data.PersonInGroup) {{
                var labelText = data.NickName + ' is in a ' + data.GroupTypeName + '.';
                var groupLength = data.GroupList.length;
                if(groupLength>5){{
                    groupLength = 5;
                }}

                for (i = 0; i < groupLength; ++i) {{
                    labelText = labelText + ' <br/> ' +  data.GroupList[i].GroupName;
                }}

                if(data.GroupList.length >5){{
                    var restGroup = data.GroupList.length - 5;
                    labelText = labelText + ' <br/> (...and ' + restGroup.toString() + ' more)';
                }}

            }} else {{
                $('.rockbadge-ingroupoftype.rockbadge-id-{3}').addClass('rockbadge-disabled');
                var labelText = data.NickName + ' is not in a ' + data.GroupTypeName + '.';
            }}
            $('.rockbadge-ingroupoftype.rockbadge-id-{3}').css('color', '{2}').html(badgeHtml).attr('data-original-title', labelText);
        }}
    }},
}});", person.Id.ToString(), groupTypeGuid.ToString(), badgeColor, badge.Id );
        }
    }
}
