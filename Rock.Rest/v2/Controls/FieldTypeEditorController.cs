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
using System.Linq;
using System.Web.Http;

using Rock.Data;
using Rock.Rest.Filters;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Rest.v2.Controls
{
    /// <summary>
    /// Provides API endpoints for the AttributeEditor control.
    /// </summary>
    /// <seealso cref="Rock.Rest.v2.Controls.ControlsControllerBase" />
    [RoutePrefix( "api/v2/Controls/FieldTypeEditor" )]
    [RockGuid( "fe748698-53d3-4ca8-ba59-1ba36f2350e3" )]
    public class FieldTypeEditorController : ControlsControllerBase
    {
        /// <summary>
        /// Gets the available field types for the current person.
        /// </summary>
        /// <returns>A collection <see cref="ListItemBag"/> that represents the field types that are available.</returns>
        [HttpGet]
        [System.Web.Http.Route( "availableFieldTypes" )]
        [Authenticate]
        [RockGuid( "5300346b-1569-4120-bc85-bd39078c0032" )]
        public IHttpActionResult GetAvailableFieldTypes()
        {
            var fieldTypes = FieldTypeCache.All()
                .Where( f => f.Platform.HasFlag( Rock.Utility.RockPlatform.Obsidian ) )
                .ToList();

            var fieldTypeItems = fieldTypes
                .Select( f => new ListItemBag
                {
                    Text = f.Name,
                    Value = f.Guid.ToString()
                } )
                .ToList();

            return Ok( fieldTypeItems );
        }

        /// <summary>
        /// Gets the attribute configuration information provided and returns a new
        /// set of configuration data. This is used by the attribute editor control
        /// when a field type makes a change that requires new data to be retrieved
        /// in order for it to continue editing the attribute.
        /// </summary>
        /// <param name="updateViewModel">The view model that contains the update request.</param>
        /// <returns>An instance of <see cref="FieldTypeConfigurationPropertiesBag"/> that represents the state of the attribute configuration.</returns>
        [HttpPost]
        [System.Web.Http.Route( "fieldTypeConfiguration" )]
        [Authenticate]
        [RockGuid( "3a544b0f-7ba9-472c-bb08-f1537a484fad" )]
        public IHttpActionResult UpdateAttributeConfiguration( [FromBody] FieldTypeConfigurationBag updateViewModel )
        {
            var fieldType = Rock.Web.Cache.FieldTypeCache.Get( updateViewModel.FieldTypeGuid )?.Field;

            if ( fieldType == null )
            {
                return BadRequest( "Unknown field type." );
            }

            // Convert the public configuration options into our private
            // configuration options (values).
            var configurationValues = fieldType.GetPrivateConfigurationValues( updateViewModel.ConfigurationValues );

            // Convert the default value from the public value into our
            // private internal value.
            var privateDefaultValue = fieldType.GetPrivateEditValue( updateViewModel.DefaultValue, configurationValues );

            // Get the new configuration properties from the currently selected
            // options.
            var configurationProperties = fieldType.GetPublicEditConfigurationProperties( configurationValues );

            // Get the public configuration options from the internal options (values).
            var publicConfigurationValues = fieldType.GetPublicConfigurationValues( configurationValues, Field.ConfigurationValueUsage.Configure, null );

            return Ok( new FieldTypeConfigurationPropertiesBag
            {
                ConfigurationProperties = configurationProperties,
                ConfigurationValues = publicConfigurationValues,
                DefaultValue = fieldType.GetPublicEditValue( privateDefaultValue, configurationValues )
            } );
        }
    }
}
