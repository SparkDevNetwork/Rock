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
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a time value
    /// </summary>
    [Serializable]
    [FieldTypeUsage( FieldTypeUsage.Common )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [IconSvg( @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 16 16""><g><path d=""M8,1a7,7,0,1,0,7,7A7,7,0,0,0,8,1ZM8,13.69A5.69,5.69,0,1,1,13.69,8,5.69,5.69,0,0,1,8,13.69Zm2.79-4.84L8.66,7.62V4.28a.66.66,0,0,0-1.32,0V8a.68.68,0,0,0,.33.57L10.13,10a.9.9,0,0,0,.33.09A.65.65,0,0,0,11,9.75.67.67,0,0,0,10.79,8.85Z""/></g></svg>" )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.TIME )]
    public class TimeFieldType : FieldType
    {
        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string value, Dictionary<string, string> configurationValues )
        {
            if ( !TimeSpan.TryParse( value, out var timeValue ) )
            {
                return string.Empty;
            }

            return timeValue.ToTimeString();
        }

        #endregion

        #region Edit Control

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
            get { return ComparisonHelper.DateFilterComparisonTypes; }
        }

        /// <summary>
        /// Converts the type of the value to property.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <param name="isNullableType">if set to <c>true</c> [is nullable type].</param>
        /// <returns></returns>
        public override object ConvertValueToPropertyType( string value, Type propertyType, bool isNullableType )
        {
            var timeValue = TimeSpan.MinValue;
            if ( TimeSpan.TryParse( value, out timeValue ) )
            {
                return timeValue;
            }
            return null;
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Formats time display
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
        /// Returns the value using the most appropriate datatype
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override object ValueAsFieldType( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues )
        {
            return value.AsTimeSpan();
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
            // return ValueAsFieldType which returns the value as a TimeSpan
            return this.ValueAsFieldType( parentControl, value, configurationValues );
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
            var tp = new TimePicker { ID = id };
            return tp;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as TimePicker;
            if ( picker != null )
            {
                if ( picker.SelectedTime.HasValue )
                {
                    // serialize the time using culture-insensitive "constant" format
                    return picker.SelectedTime.Value.ToString( "c" );
                }

                return string.Empty;
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
            var picker = control as TimePicker;
            if ( picker != null )
            {
                picker.SelectedTime = value.AsTimeSpan();
            }
        }

        /// <summary>
        /// Determines whether this FieldType supports doing PostBack for the editControl
        /// </summary>
        /// <param name="editControl">The edit control.</param>
        /// <returns>
        ///   <c>true</c> if [has change handler] [the specified control]; otherwise, <c>false</c>.
        /// </returns>
        public override bool HasChangeHandler( Control editControl )
        {
            // the TimePicker can cause a postback loop if OnChange and AutoPostback is enabled, so disable the HasChangeHandler
            return false;
        }

        /// <summary>
        /// Specifies an action to perform when the EditControl's Value is changed. See also <seealso cref="HasChangeHandler(Control)" />
        /// </summary>
        /// <param name="editControl">The edit control.</param>
        /// <param name="action">The action.</param>
        public override void AddChangeHandler( Control editControl, Action action )
        {
            // the TimePicker can cause a postback loop if OnChange and AutoPostback is enabled, so disable the HasChangeHandler
        }

#endif
        #endregion
    }
}