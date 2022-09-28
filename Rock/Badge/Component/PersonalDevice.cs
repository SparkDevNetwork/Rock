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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Badge.Component
{
    /// <summary>
    /// Badge showing the number of devices configured for the person.
    /// </summary>
    [Description( "Badge showing the number of devices configured for the person." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Personal Device" )]

    [LinkedPage( "Personal Devices Detail", "Page to show the details of the personal devices added.", false, order: 1 )]
    [BooleanField( "Hide If None", "Should the badge be hidden if there are no devices registered to this person?", true, order: 2 )]
    [Rock.SystemGuid.EntityTypeGuid( "C92E1D6C-EE4B-4BD6-B5C6-9E6071243341")]
    public class PersonalDevice : BadgeComponent
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

            writer.Write( $"<div class='rockbadge rockbadge-overlay rockbadge-personaldevice rockbadge-id-{badge.Id}' data-toggle='tooltip' data-original-title=''>" );
            writer.Write( "</div>" );
        }

        /// <inheritdoc/>
        protected override string GetJavaScript( BadgeCache badge, IEntity entity )
        {
            if ( !( entity is Person person ) )
            {
                return null;
            }

            //  create url for link to details
            string detailPageUrl = new PageReference( GetAttributeValue( badge, "PersonalDevicesDetail" ), new Dictionary<string, string> { { "PersonGuid", person.Guid.ToString() } } ).BuildUrl();

            string noneCss = GetAttributeValue( badge, "HideIfNone" ).AsBoolean() ? "none" : "";

            return $@"
                $.ajax({{
                    type: 'GET',
                    url: Rock.settings.get('baseUrl') + 'api/Badges/PersonalDevicesNumber/{person.Id}' ,
                    statusCode: {{
                        200: function (data, status, xhr) {{
                            var devicesNumber = data;
                            var linkUrl = '{detailPageUrl}';
                            var badgeContent = '';
                            var labelContent = '';
                            var badgeClass = 'badge-disabled';

                            if (devicesNumber > 0){{
                                badgeClass=''
                            }}

                            if ( devicesNumber !=1 ) {{
                                labelContent = 'There are ' + devicesNumber + ' devices linked to this individual.';
                            }} else {{
                                labelContent = 'There is 1 device linked to this individual.';
                            }}

                            if (linkUrl != '') {{
                                badgeContent = '<a href=\'' + linkUrl + '\' class=\'badge-content\'><i class=\''+ badgeClass +' fa fa-mobile badge-icon\'></i><span class=\'metric-value\'>' + devicesNumber + '</span></a>';
                            }} else {{
                                badgeContent = '<div class=\'badge-content\'><i class=\'badge-icon '+ badgeClass +' fa  fa-mobile\'></i><span class=\'metric-value\'>' + devicesNumber + '</span></div>';
                            }}
                            $('.rockbadge-personaldevice.rockbadge-id-{badge.Id}').html(badgeContent).attr('data-original-title', labelContent);

                            if (devicesNumber < 1) {{
                                $('.rockbadge-personaldevice.rockbadge-id-{badge.Id}').css('display', '{noneCss}');
                            }}
                        }}
                    }},
                }});";
        }
    }
}