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

namespace Rock.PersonProfile.Badge
{
    /// <summary>
    /// In Group With Purpose Badge
    /// </summary>
    [Description( "Shows badge if the individual is in a group where it's group type has a specified purpose (e.g. Serving)." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "In Group With Purpose" )]
    
    [DefinedValueField( SystemGuid.DefinedType.GROUPTYPE_PURPOSE, "Group Type Purpose", "The purpose to filter on.")]
    [TextField( "Badge Icon CSS", "The CSS icon to use for the badge.", true, "fa fa-users", key:"BadgeIconCss")]
    [TextField( "Badge Color", "The color of the badge (#ffffff).", true, "#0ab4dd")]
    public class InGroupWithPurpose : BadgeComponent
    {        
        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( PersonBadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            if (!String.IsNullOrEmpty(GetAttributeValue(badge, "GroupTypePurpose")))
            {
                string badgeColor = "#0ab4dd";

                if (!String.IsNullOrEmpty(GetAttributeValue(badge, "BadgeColor")))
                {
                    badgeColor = GetAttributeValue(badge, "BadgeColor");
                }

                Guid groupTypePurposeGuid = GetAttributeValue( badge, "GroupTypePurpose" ).AsGuid();
                if ( groupTypePurposeGuid != Guid.Empty )
                {
                    writer.Write( String.Format( "<div class='badge badge-ingroupwithpurpose badge-id-{0}' data-toggle='tooltip' data-original-title=''>", badge.Id ) );

                    writer.Write( "</div>" );

                    writer.Write( String.Format( @"
<script>
    Sys.Application.add_load(function () {{
                                                
        $.ajax({{
                type: 'GET',
                url: Rock.settings.get('baseUrl') + 'api/PersonBadges/InGroupWithPurpose/{0}/{1}' ,
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
        }});
    }});
</script>          
", Person.Id.ToString(), groupTypePurposeGuid.ToString(), badgeColor, badge.Id, GetAttributeValue(badge, "BadgeIconCss" ) ) );
                }
            }

        }


    }
}
