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
    /// In Group With Purpose Badge
    /// </summary>
    [Description( "Shows badge if the individual is in a group where it's group type has a specified purpose (e.g. Serving)." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "In Group With Purpose" )]

    [DefinedValueField( SystemGuid.DefinedType.GROUPTYPE_PURPOSE, "Group Type Purpose", "The purpose to filter on." )]
    [TextField( "Badge Icon CSS", "The CSS icon to use for the badge.", true, "fa fa-users", key: "BadgeIconCss" )]
    [TextField( "Badge Color", "The color of the badge (#ffffff).", true, "#0ab4dd" )]
    [Rock.SystemGuid.EntityTypeGuid( "1844AC11-7117-4C91-8D82-A6340D50323E")]
    public class InGroupWithPurpose : BadgeComponent
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

            writer.Write( String.Format( "<div class='rockbadge rockbadge-icon rockbadge-ingroupwithpurpose rockbadge-id-{0}' data-toggle='tooltip' data-original-title=''>", badge.Id ) );

            writer.Write( "</div>" );
        }

        /// <inheritdoc/>
        protected override string GetJavaScript( BadgeCache badge, IEntity entity )
        {
            var groupTypePurposeGuid = GetAttributeValue( badge, "GroupTypePurpose" ).AsGuidOrNull();

            if ( !( entity is Person person ) || !groupTypePurposeGuid.HasValue )
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
    url: Rock.settings.get('baseUrl') + 'api/Badges/InGroupWithPurpose/{0}/{1}' ,
    statusCode: {{
        200: function (data, status, xhr) {{
            var badgeHtml = '';
            var groupIcon = '{4}';

            if (data.PersonInGroup) {{
                badgeHtml = '<i class=\'badge-icon ' + groupIcon + '\' style=\'color: {2}\'></i>';
                var labelText = data.NickName + ' is in group with the ' + data.Purpose + ' purpose.';
            }} else {{
                badgeHtml = '<i class=\'badge-icon badge-disabled ' + groupIcon + '\'></i>';
                $('.rockbadge-ingroupwithpurpose.rockbadge-id-{3}').addClass('rockbadge-disabled');
                var labelText = data.NickName + ' is not in a group with the ' + data.Purpose + ' purpose.';
            }}
            $('.rockbadge-ingroupwithpurpose.rockbadge-id-{3}').html(badgeHtml);
            $('.rockbadge-ingroupwithpurpose.rockbadge-id-{3}').attr('data-original-title', labelText);
        }}
    }},
}});", person.Id.ToString(), groupTypePurposeGuid.ToString(), badgeColor, badge.Id, GetAttributeValue( badge, "BadgeIconCss" ) );
        }
    }
}
