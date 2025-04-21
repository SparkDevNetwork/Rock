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
using System.Web.UI.WebControls;
#endif

using Rock.Attribute;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a Day of the Week
    /// stored as int value that can be cast to System.DayOfWeek (where Sunday = 0)
    /// </summary>
    [FieldTypeUsage( FieldTypeUsage.Advanced )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [IconSvg( @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 16 16""><g><path d=""M12.81,2.75H11.5V1.44A.44.44,0,0,0,11.06,1h-.87a.44.44,0,0,0-.44.44V2.75H6.25V1.44A.44.44,0,0,0,5.81,1H4.94a.44.44,0,0,0-.44.44V2.75H3.19A1.31,1.31,0,0,0,1.88,4.06v9.63A1.31,1.31,0,0,0,3.19,15h9.62a1.31,1.31,0,0,0,1.31-1.31V4.06A1.31,1.31,0,0,0,12.81,2.75Zm0,10.77a.17.17,0,0,1-.16.17H3.35a.17.17,0,0,1-.16-.17V5.38h9.62Z""/><rect x=""4.04"" y=""5.85"" width=""1.87"" height=""7.33"" rx=""0.3""/></g></svg>" )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.DAY_OF_WEEK )]
    public class DayOfWeekFieldType : FieldType
    {
        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string value, Dictionary<string, string> configurationValues )
        {
            var intValue = value.AsIntegerOrNull();

            if ( !intValue.HasValue )
            {
                return string.Empty;
            }

            System.DayOfWeek dayOfWeek = ( System.DayOfWeek ) intValue.Value;
            return dayOfWeek.ConvertToString();
        }

        #endregion

        #region Edit Control

        #endregion

        #region Filter Control

        /// <summary>
        /// Converts the type of the value to property.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <param name="isNullableType">if set to <c>true</c> [is nullable type].</param>
        /// <returns></returns>
        public override object ConvertValueToPropertyType( string value, Type propertyType, bool isNullableType )
        {
            int? intValue = value.AsIntegerOrNull();
            if ( intValue.HasValue )
            {
                return ( System.DayOfWeek ) intValue.Value;
            }
            return null;
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
        public override string FormatValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            return !condensed
                ? GetTextValue( value, configurationValues.ToDictionary( k => k.Key, k => k.Value.Value ) )
                : GetCondensedTextValue( value, configurationValues.ToDictionary( k => k.Key, k => k.Value.Value ) );
        }

        /// <summary>
        /// Returns the value that should be used for sorting, using the most appropriate datatype
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override object SortValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues )
        {
            int? intValue = value.AsIntegerOrNull();
            if ( intValue.HasValue )
            {
                System.DayOfWeek dayOfWeek = ( System.DayOfWeek ) intValue.Value;
                return dayOfWeek;
            }

            return ( System.DayOfWeek ) 0;
        }

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override System.Web.UI.Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            return new DayOfWeekPicker { ID = id };
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            List<string> values = new List<string>();

            DayOfWeekPicker dayOfWeekPicker = control as DayOfWeekPicker;

            if ( dayOfWeekPicker != null )
            {
                var selectedDay = dayOfWeekPicker.SelectedDayOfWeek;
                if ( selectedDay != null )
                {
                    return selectedDay.ConvertToInt().ToString();
                }
                else
                {
                    return string.Empty;
                }
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            DayOfWeek? dayOfWeek = null;

            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                dayOfWeek = ( DayOfWeek ) ( value.AsInteger() );
            }

            DayOfWeekPicker dayOfWeekPicker = control as DayOfWeekPicker;
            dayOfWeekPicker.SelectedDayOfWeek = dayOfWeek;
        }

        /// <summary>
        /// Gets the filter compare control.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override Control FilterCompareControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            var lbl = new Label();
            lbl.ID = string.Format( "{0}_lIs", id );
            lbl.AddCssClass( "data-view-filter-label" );
            lbl.Text = "Is";

            // hide the compare control when in SimpleFilter mode
            lbl.Visible = filterMode != FilterMode.SimpleFilter;

            return lbl;
        }


        /// <summary>
        /// Gets the filter compare value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override string GetFilterCompareValue( Control control, FilterMode filterMode )
        {
            return GetEqualToCompareValue();
        }

#endif
        #endregion

    }
}