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
using System.Collections.Generic;
using System.Web.UI;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// 
    /// </summary>
    public class EntityFieldType : FieldType
    {
        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            string formattedValue = string.Empty;
            string[] values = ( value ?? string.Empty ).Split( '|' );
            if ( values.Length == 2 )
            {
                var entityType = EntityTypeCache.Read( values[0].AsGuid() );
                if ( entityType != null )
                {
                    formattedValue = entityType.FriendlyName + "|EntityId:" + values[1].AsIntegerOrNull();
                }
            }

            return base.FormatValue( parentControl, formattedValue, null, condensed );
        }

        /// <summary>
        /// Creates the control(s) neccessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var entityPicker = new EntityPicker { ID = id };
            return entityPicker;
        }

        /// <summary>
        /// Reads new values entered by the user for the field ( as Guid) 
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            EntityPicker entityPicker = control as EntityPicker;
            if ( entityPicker != null && entityPicker.EntityTypeId.HasValue )
            {
                var entityType = EntityTypeCache.Read( entityPicker.EntityTypeId.Value );
                if ( entityType != null )
                {
                    return entityType.Guid.ToString() + "|" + entityPicker.EntityId;
                }
            }

            return null;
        }

        /// <summary>
        /// Sets the value ( as Guid )
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            string[] values = (value ?? string.Empty).Split( '|' );
            if ( values.Length == 2 )
            {
                EntityPicker entityPicker = control as EntityPicker;
                if ( entityPicker != null )
                {
                    var entityType = EntityTypeCache.Read( values[0].AsGuid() );
                    if ( entityType != null )
                    {
                        entityPicker.EntityTypeId = entityType.Id;
                        entityPicker.EntityId = values[1].AsIntegerOrNull();
                    }
                }
            }
        }
    }
}