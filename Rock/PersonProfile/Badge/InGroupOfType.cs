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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;

using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Data;
using System.Collections.Generic;
using System.Data;
using System;
using System.Diagnostics;
using Rock.Web.Cache;

namespace Rock.PersonProfile.Badge
{
    /// <summary>
    /// In Group Of Type Badge
    /// </summary>
    [Description( "Shows badge if the individual is in a group of a specified type." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "In Group Of Type" )]
    
    [GroupTypeField("Group Type", "The type of group to use.", true)]
    [TextField("Badge Color", "The color of the badge (#ffffff).", true, "#0ab4dd")]
    public class InGroupOfType : BadgeComponent
    {        
        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( PersonBadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            if (!String.IsNullOrEmpty(GetAttributeValue(badge, "GroupType")))
            {
                string badgeColor = "#0ab4dd";

                if (!String.IsNullOrEmpty(GetAttributeValue(badge, "BadgeColor")))
                {
                    badgeColor = GetAttributeValue(badge, "BadgeColor");
                }

                Guid groupTypeGuid = GetAttributeValue( badge, "GroupType" ).AsGuid();
                if ( groupTypeGuid != Guid.Empty )
                {
                    writer.Write( String.Format( "<div class='badge badge-ingroupoftype badge-id-{0}' data-toggle='tooltip' data-original-title=''>", badge.Id ) );

                    writer.Write( "</div>" );

                    writer.Write( String.Format( @"
<script>
    Sys.Application.add_load(function () {{
                                                
        $.ajax({{
                type: 'GET',
                url: Rock.settings.get('baseUrl') + 'api/PersonBadges/InGroupOfType/{0}/{1}' ,
                statusCode: {{
                    200: function (data, status, xhr) {{
                        var badgeHtml = '';
                        var groupIcon = data.GroupTypeIconCss;
                                            
                        if (groupIcon == '') {{
                            groupIcon = 'fa fa-times';
                        }}

                        if (data.PersonInGroup) {{
                            badgeHtml = '<i class=\'badge-icon ' + groupIcon + '\' style=\'color: {2}\'></i>';
                            var labelText = data.NickName + ' is in a ' + data.GroupTypeName + '.';
                        }} else {{
                            badgeHtml = '<i class=\'badge-icon badge-disabled ' + groupIcon + '\'></i>';
                            var labelText = data.NickName + ' is not in a ' + data.GroupTypeName + '.';
                        }}
                        $('.badge-ingroupoftype.badge-id-{3}').html(badgeHtml);
                        $('.badge-ingroupoftype.badge-id-{3}').attr('data-original-title', labelText);
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
}
