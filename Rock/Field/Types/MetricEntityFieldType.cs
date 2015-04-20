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
using System;
using System.Collections.Generic;
using System.Web.UI;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// 
    /// </summary>
    public class MetricEntityFieldType : FieldType
    {

        #region Formatting

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

            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                var valueParts = value.Split( '|' );
                if ( valueParts.Length > 0 )
                {
                    var service = new MetricService( new RockContext() );
                    var metric = service.Get( new Guid( valueParts[0] ) );

                    if ( metric != null )
                    {
                        formattedValue = metric.Title;
                        var entityType = EntityTypeCache.Read( metric.EntityTypeId ?? 0 );
                        if ( entityType != null && entityType.SingleValueFieldType != null )
                        {
                            if ( valueParts.Length > 1 )
                            {
                                formattedValue = string.Format( "{0} - EntityId:{1}", metric.Title, valueParts[1].AsIntegerOrNull() );
                            }
                        }
                    }
                }
            }

            return base.FormatValue( parentControl, formattedValue, null, condensed );
        }

        #endregion

        #region Edit Control

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            return new MetricEntityPicker { ID = id };
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// returns pipe delimited: Metric (as Guid) | EntityId | GetEntityFromContext | CombineValues | Metric's Category (as Guid)
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as MetricEntityPicker;
            string result = null;

            if ( picker != null )
            {
                result = picker.DelimitedValues;
            }

            return result;
        }

        /// <summary>
        /// Sets the value.
        /// value is pipe delimited: Metric (as Guid) | EntityId | GetEntityFromContext | CombineValues | Metric's Category (as Guid)
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( value != null )
            {
                var picker = control as MetricEntityPicker;

                if ( picker != null )
                {
                    picker.DelimitedValues = value;
                }
            }
        }

        #endregion

    }
}
