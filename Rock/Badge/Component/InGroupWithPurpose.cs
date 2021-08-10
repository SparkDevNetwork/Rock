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

            writer.Write( String.Format( "<div class='badge badge-ingroupwithpurpose badge-id-{0}' data-toggle='tooltip' data-original-title=''>", badge.Id ) );

            writer.Write( "</div>" );
        }

        /// <summary>
        /// Gets the java script.
        /// </summary>
        /// <param name="badge"></param>
        /// <returns></returns>
        protected override string GetJavaScript( BadgeCache badge )
        {
            var groupTypePurposeGuid = GetAttributeValue( badge, "GroupTypePurpose" ).AsGuidOrNull();

            if ( Person == null || !groupTypePurposeGuid.HasValue )
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
                var labelText = data.NickName + ' is not in a group with the ' + data.Purpose + ' purpose.';
            }}
            $('.badge-ingroupwithpurpose.badge-id-{3}').html(badgeHtml);
            $('.badge-ingroupwithpurpose.badge-id-{3}').attr('data-original-title', labelText);
        }}
    }},
}});", Person.Id.ToString(), groupTypePurposeGuid.ToString(), badgeColor, badge.Id, GetAttributeValue( badge, "BadgeIconCss" ) );
        }
    }
}
