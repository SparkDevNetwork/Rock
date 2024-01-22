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
#if WEBFORMS
using System.Web.UI;
#endif
using Rock.Attribute;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// stored as comma delimited list of int value that can each be cast to System.DayOfWeek (where Sunday = 0)
    /// </summary>
    [FieldTypeUsage( FieldTypeUsage.Advanced )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [IconSvg( @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 16 16""><path d=""M12.81,2.75H11.5V1.44A.44.44,0,0,0,11.06,1h-.87a.44.44,0,0,0-.44.44V2.75H6.25V1.44A.44.44,0,0,0,5.81,1H4.94a.44.44,0,0,0-.44.44V2.75H3.19A1.31,1.31,0,0,0,1.88,4.06v9.63A1.31,1.31,0,0,0,3.19,15h9.62a1.31,1.31,0,0,0,1.31-1.31V4.06A1.31,1.31,0,0,0,12.81,2.75Zm0,10.77a.17.17,0,0,1-.16.17H3.35a.17.17,0,0,1-.16-.17V5.38h9.62Z""/><rect x=""4.03"" y=""5.85"" width=""1.87"" height=""7.33"" rx=""0.3""/><rect x=""6.79"" y=""5.85"" width=""1.87"" height=""7.33"" rx=""0.3""/></svg>" )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.DAYS_OF_WEEK )]
    public class DaysOfWeekFieldType : FieldType
    {
        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string value, Dictionary<string, string> configurationValues )
        {
            if ( string.IsNullOrWhiteSpace( value ) )
            {
                return string.Empty;
            }

            var daysOfWeek = value.Split( ',' ).Select( a => ( DayOfWeek ) a.AsInteger() ).ToList();
            var dayNames = daysOfWeek.Select( a => a.ConvertToString() ).ToList();

            return dayNames.AsDelimited( ", " );
        }

        #endregion

        #region EditControl

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

        /// <summary>
        /// Formats the filter value value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override string FormatFilterValueValue( Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            string formattedValue = string.Empty;

            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                List<DayOfWeek> daysOfWeek = value.Split( ',' ).Select( a => ( DayOfWeek ) ( a.AsInteger() ) ).ToList();
                List<string> dayNames = daysOfWeek.Select( a => a.ConvertToString() ).ToList();
                return AddQuotes( dayNames.AsDelimited( "' AND '" ) );
            }

            return string.Empty;
        }

        #endregion

        #region WebForms
#if WEBFORMS

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
            return !condensed
                ? GetTextValue( value, configurationValues.ToDictionary( k => k.Key, k => k.Value.Value ) )
                : GetCondensedTextValue( value, configurationValues.ToDictionary( k => k.Key, k => k.Value.Value ) );
        }

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
            if ( daysOfWeekPicker != null )
            {
                return daysOfWeekPicker.SelectedDaysOfWeek.Select( a => a.ConvertToInt().ToString() ).ToList().AsDelimited( "," );
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
            if ( daysOfWeekPicker != null )
            {
                var selectedDaysOfWeek = ( value ?? string.Empty ).SplitDelimitedValues().AsIntegerList().Select( a => ( DayOfWeek ) a ).ToList();
                daysOfWeekPicker.SelectedDaysOfWeek = selectedDaysOfWeek;
            }
        }


#endif
        #endregion
    }
}
