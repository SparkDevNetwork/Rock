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

using Rock.Rest.Filters;
using Rock.ViewModel.Controls;
using Rock.ViewModel.NonEntities;
using Rock.Web.Cache;

namespace Rock.Rest.v2.Controls
{
    /// <summary>
    /// Provides API endpoints for the AttributeEditor control.
    /// </summary>
    /// <seealso cref="Rock.Rest.v2.Controls.ControlsControllerBase" />
    [RoutePrefix( "api/v2/Controls/FieldTypeEditor" )]
    public class FieldTypeEditorController : ControlsControllerBase
    {
        /// <summary>
        /// Gets the available field types for the current person.
        /// </summary>
        /// <returns>A collection <see cref="ListItemViewModel"/> that represents the field types that are available.</returns>
        [HttpGet]
        [Route( "availableFieldTypes" )]
        [Authenticate]
        public IHttpActionResult GetAvailableFieldTypes()
        {
            var fieldTypes = FieldTypeCache.All()
                .Where( f => f.Platform.HasFlag( Rock.Utility.RockPlatform.Obsidian ) )
                .ToList();

            var fieldTypeItems = fieldTypes
                .Select( f => new ListItemViewModel
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
        /// <returns>An instance of <see cref="FieldTypeConfigurationPropertiesViewModel"/> that represents the state of the attribute configuration.</returns>
        [HttpPost]
        [Route( "fieldTypeConfiguration" )]
        [Authenticate]
        public IHttpActionResult UpdateAttributeConfiguration( [FromBody] FieldTypeConfigurationViewModel updateViewModel )
        {
            var fieldType = Rock.Web.Cache.FieldTypeCache.Get( updateViewModel.FieldTypeGuid )?.Field;

            if ( fieldType == null )
            {
                return BadRequest( "Unknown field type." );
            }

            // Convert the public configuration options into our private
            // configuration options (values).
            var configurationValues = fieldType.GetPrivateConfigurationOptions( updateViewModel.ConfigurationOptions );

            // Convert the default value from the public value into our
            // private internal value.
            var privateDefaultValue = fieldType.GetPrivateEditValue( updateViewModel.DefaultValue, configurationValues );

            // Get the new configuration properties from the currently selected
            // options.
            var configurationProperties = fieldType.GetPublicEditConfigurationProperties( configurationValues );

            // Get the public configuration options from the internal options (values).
            var publicConfigurationOptions = fieldType.GetPublicConfigurationOptions( configurationValues );

            // Get the editable attribute value so they can render a default value
            // control.
            var clientEditableValue = new ClientEditableAttributeValueViewModel
            {
                FieldTypeGuid = updateViewModel.FieldTypeGuid,
                AttributeGuid = Guid.Empty,
                Name = "Default Value",
                Categories = new List<ClientAttributeValueCategoryViewModel>(),
                Order = 0,
                TextValue = fieldType.GetTextValue( privateDefaultValue, configurationValues ),
                Value = fieldType.GetPublicEditValue( privateDefaultValue, configurationValues ),
                Key = "DefaultValue",
                IsRequired = false,
                Description = string.Empty,
                ConfigurationValues = fieldType.GetPublicConfigurationValues( configurationValues )
            };

            return Ok( new FieldTypeConfigurationPropertiesViewModel
            {
                ConfigurationProperties = configurationProperties,
                ConfigurationOptions = publicConfigurationOptions,
                DefaultValue = clientEditableValue
            } );
        }
    }
}
