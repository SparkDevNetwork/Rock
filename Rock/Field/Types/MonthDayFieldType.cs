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
using System.Collections.Generic;
using System.Web.UI;

using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to for specifying a Month and Day portion of a Date
    /// Stored as "M/d" (regardless of culture), but formatted value will be culture specific
    /// </summary>
    [Rock.Attribute.RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    public class MonthDayFieldType : FieldType
    {
        #region Edit Control

        /// <summary>
        /// Renders the controls necessary for prompting user for a new value and adds them to the parentControl
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var mdpMonthDatePicker = new MonthDayPicker { ID = id };

            return mdpMonthDatePicker;
        }

        /// <summary>
        /// Returns the field's current value(s) formatted as a culture specific Month Day string
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            var valueAsDateTime = value.MonthDayStringAsDateTime();
            if ( valueAsDateTime.HasValue )
            {
                return valueAsDateTime.Value.ToMonthDayString();
            }

            return base.FormatValue( parentControl, value, configurationValues, condensed );
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var mdpMonthDatePicker = control as MonthDayPicker;
            return mdpMonthDatePicker?.SelectedDate?.ToString( "M/d" ) ?? string.Empty;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues"></param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var mdpMonthDatePicker = control as MonthDayPicker;
            if ( mdpMonthDatePicker != null )
            {
                mdpMonthDatePicker.SelectedDate = value.MonthDayStringAsDateTime();
            }
        }

        #endregion
    }
}