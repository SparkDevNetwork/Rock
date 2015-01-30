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
using System.Linq;
using System.Web.UI;

using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// stored as comma delimited list of int value that can each be cast to System.DayOfWeek (where Sunday = 0)
    /// </summary>
    public class DaysOfWeekFieldType : FieldType
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
                List<DayOfWeek> daysOfWeek = value.Split( ',' ).Select( a => (DayOfWeek)( a.AsInteger() ) ).ToList();
                List<string> dayNames = daysOfWeek.Select( a => a.ConvertToString() ).ToList();
                return dayNames.AsDelimited( "," );
            }

            return base.FormatValue( parentControl, formattedValue, null, condensed );
        }

        #endregion

        #region EditControl

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
            return new DaysOfWeekPicker { ID = id };
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var daysOfWeekPicker = control as DaysOfWeekPicker;
            if (daysOfWeekPicker != null)
            {
                return daysOfWeekPicker.SelectedDaysOfWeek.Select( a => a.ConvertToInt().ToString()).ToList().AsDelimited( "," );
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var daysOfWeekPicker = control as DaysOfWeekPicker;
            if (daysOfWeekPicker != null)
            {
                daysOfWeekPicker.SelectedDaysOfWeek = (value ?? string.Empty).Split( ',' ).Select( a => (DayOfWeek)( a.AsInteger() ) ).ToList();
            }
        }

        #endregion 

        #region Filter Control

        /// <summary>
        /// Gets the type of the filter comparison.
        /// </summary>
        /// <value>
        /// The type of the filter comparison.
        /// </value>
        public override ComparisonType FilterComparisonType
        {
            get
            {
                return ComparisonHelper.ContainsFilterComparisonTypes;
            }
        }

        #endregion

    }
}
