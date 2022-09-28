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
using System.Linq;
using System.Web.UI;

using Rock.Attribute;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to for specifying a Month and Day portion of a Date
    /// Stored as "M/d" (regardless of culture), but formatted value will be culture specific
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [IconSvg( @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 16 16""><defs><style>.cls-1,.cls-2{fill:#343a40;}.cls-2{stroke:#343a40;}</style></defs><path d=""M12.89,2.75H11.56V1.33A.34.34,0,0,0,11.22,1H10.11a.33.33,0,0,0-.33.33V2.75H6.22V1.33A.33.33,0,0,0,5.89,1H4.78a.34.34,0,0,0-.34.33V2.75H3.11A1.33,1.33,0,0,0,1.78,4.06v9.63A1.33,1.33,0,0,0,3.11,15h9.78a1.33,1.33,0,0,0,1.33-1.31V4.06A1.33,1.33,0,0,0,12.89,2.75Zm-.17,10.94H3.28a.18.18,0,0,1-.17-.17V5.37h9.78v8.15A.18.18,0,0,1,12.72,13.69Z""/><path class=""cls-2"" d=""M9.75,12a.43.43,0,0,1-.44.44H6.69a.44.44,0,1,1,0-.87h.87v-4L6.93,8a.44.44,0,1,1-.49-.73l1.32-.88a.48.48,0,0,1,.45,0,.45.45,0,0,1,.23.39V11.6h.87A.43.43,0,0,1,9.75,12Z""/></svg>" )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.MONTH_DAY )]
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
            return !condensed
                ? GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
                : GetCondensedTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

        /// <inheritdoc/>
        public override string GetTextValue( string value, Dictionary<string, string> configurationValues )
        {
            var valueAsDateTime = value.MonthDayStringAsDateTime();
            if ( valueAsDateTime.HasValue )
            {
                return valueAsDateTime.Value.ToMonthDayString();
            }

            return value;
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