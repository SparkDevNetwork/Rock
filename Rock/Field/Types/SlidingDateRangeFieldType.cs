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
using Rock.Data;
using Rock.Enums.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save a sliding date range. Last X (Hours, Days, etc)
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.SLIDING_DATE_RANGE )]
    public class SlidingDateRangeFieldType : FieldType
    {
        #region Configuration

        /// <summary>
        /// Enabled SlidingDateRangeTypes
        /// </summary>
        protected const string ENABLED_SLIDING_DATE_RANGE_TYPES = "enabledSlidingDateRangeTypes";

        /// <summary>
        /// Enabled SlidingDateRangeUnits
        /// </summary>
        protected const string ENABLED_SLIDING_DATE_RANGE_UNITS = "enabledSlidingDateRangeUnits";

        private const string TIME_UNIT_TYPES_PROPERTY_KEY = "timeUnitTypes";
        private const string SLIDING_DATE_RANGE_TYPES_PROPERTY_KEY = "slidingDateRangeTypes";

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicEditConfigurationProperties( Dictionary<string, string> privateConfigurationValues )
        {
            using ( var rockContext = new RockContext() )
            {
                var configurationProperties = new Dictionary<string, string>();

                var typesList = Enum.GetValues( typeof( SlidingDateRangePicker.SlidingDateRangeType ) )
                    .Cast<SlidingDateRangePicker.SlidingDateRangeType>()
                    .Where( a => a != SlidingDateRangePicker.SlidingDateRangeType.All )
                    .Select( a => new ListItemBag
                    {
                        Text = a.ConvertToString(),
                        Value = a.ConvertToInt().ToString()
                    } ).ToList();

                var unitList = Enum.GetValues( typeof( SlidingDateRangePicker.TimeUnitType ) )
                    .Cast<SlidingDateRangePicker.TimeUnitType>()
                    .Select( a => new ListItemBag
                    {
                        Text = a.ConvertToString(),
                        Value = a.ConvertToInt().ToString()
                    } ).ToList();

                configurationProperties[SLIDING_DATE_RANGE_TYPES_PROPERTY_KEY] = typesList.ToCamelCaseJson( false, true );
                configurationProperties[TIME_UNIT_TYPES_PROPERTY_KEY] = unitList.ToCamelCaseJson( false, true );

                return configurationProperties;
            }
        }

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return SlidingDateRangePicker.FormatDelimitedValues( privateValue );
        }

        #endregion

        #region Edit Control

        #endregion

        #region Filter Control

        /// <summary>
        /// Determines whether this filter has a filter control
        /// </summary>
        /// <returns></returns>
        public override bool HasFilterControl()
        {
            return false;
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            List<string> configKeys = new List<string>();
            configKeys.Add( ENABLED_SLIDING_DATE_RANGE_TYPES );
            configKeys.Add( ENABLED_SLIDING_DATE_RANGE_UNITS );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            List<Control> controls = new List<Control>();

            var clbSlidingDateRangeTypes = new RockCheckBoxList();
            clbSlidingDateRangeTypes.Label = "Enabled Sliding Date Range Types";
            clbSlidingDateRangeTypes.Help = "Select specific types or leave all blank to use all of them";
            controls.Add( clbSlidingDateRangeTypes );
            var typesList = Enum.GetValues( typeof( SlidingDateRangePicker.SlidingDateRangeType ) )
                .Cast<SlidingDateRangePicker.SlidingDateRangeType>()
                .Where( a => a != SlidingDateRangePicker.SlidingDateRangeType.All );

            foreach ( var type in typesList )
            {
                clbSlidingDateRangeTypes.Items.Add( new ListItem( type.ConvertToString(), type.ConvertToInt().ToString() ) );
            }

            var clbSlidingDateRangeUnits = new RockCheckBoxList();
            clbSlidingDateRangeUnits.Label = "Enabled Sliding Date Range Units";
            clbSlidingDateRangeUnits.Help = "Select specific units or leave all blank to use all of them";
            controls.Add( clbSlidingDateRangeUnits );
            var unitsList = Enum.GetValues( typeof( SlidingDateRangePicker.TimeUnitType ) ).Cast<SlidingDateRangePicker.TimeUnitType>();
            foreach ( var type in unitsList )
            {
                clbSlidingDateRangeUnits.Items.Add( new ListItem( type.ConvertToString(), type.ConvertToInt().ToString() ) );
            }

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>();
            configurationValues.Add( ENABLED_SLIDING_DATE_RANGE_TYPES, new ConfigurationValue( "Enabled SlidingDateRange Types", "The enabled SlidingDateRange types", string.Empty ) );
            configurationValues.Add( ENABLED_SLIDING_DATE_RANGE_UNITS, new ConfigurationValue( "Enabled SlidingDateRange Units", "The enabled SlidingDateRange units", string.Empty ) );

            if ( controls != null && controls.Count >= 1 )
            {
                var clbSlidingDateRangeTypes = controls[0] as RockCheckBoxList;
                if ( clbSlidingDateRangeTypes != null )
                {
                    configurationValues[ENABLED_SLIDING_DATE_RANGE_TYPES].Value = clbSlidingDateRangeTypes.SelectedValues.AsDelimited( "," );
                }
            }

            if ( controls != null && controls.Count >= 2 )
            {
                var clbSlidingDateUnitTypes = controls[1] as RockCheckBoxList;
                if ( clbSlidingDateUnitTypes != null )
                {
                    configurationValues[ENABLED_SLIDING_DATE_RANGE_UNITS].Value = clbSlidingDateUnitTypes.SelectedValues.AsDelimited( "," );
                }
            }

            return configurationValues;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="configurationValues"></param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( controls != null && configurationValues != null )
            {
                if ( controls.Count >= 1 )
                {
                    var clbSlidingDateRangeTypes = controls[0] as RockCheckBoxList;
                    if ( clbSlidingDateRangeTypes != null && configurationValues.ContainsKey( ENABLED_SLIDING_DATE_RANGE_TYPES ) )
                    {
                        var selectedDateRangeTypes = configurationValues[ENABLED_SLIDING_DATE_RANGE_TYPES].Value.SplitDelimitedValues().AsIntegerList().Select( a => ( SlidingDateRangePicker.SlidingDateRangeType ) a );
                        foreach ( var item in clbSlidingDateRangeTypes.Items.OfType<ListItem>() )
                        {
                            item.Selected = selectedDateRangeTypes.Contains( item.Value.ConvertToEnum<SlidingDateRangePicker.SlidingDateRangeType>() );
                        }
                    }

                }
                if ( controls.Count >= 2 )
                {
                    var clbSlidingDateRangeUnits = controls[1] as RockCheckBoxList;
                    if ( clbSlidingDateRangeUnits != null && configurationValues.ContainsKey( ENABLED_SLIDING_DATE_RANGE_UNITS ) )
                    {
                        var selectedDateRangeUnits = configurationValues[ENABLED_SLIDING_DATE_RANGE_UNITS].Value.SplitDelimitedValues().AsIntegerList().Select( a => ( SlidingDateRangePicker.TimeUnitType ) a );
                        foreach ( var item in clbSlidingDateRangeUnits.Items.OfType<ListItem>() )
                        {
                            item.Selected = selectedDateRangeUnits.Contains( item.Value.ConvertToEnum<SlidingDateRangePicker.TimeUnitType>() );
                        }
                    }
                }
            }
        }

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
            return GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var picker = new SlidingDateRangePicker { ID = id };
            if ( configurationValues != null && configurationValues.ContainsKey( ENABLED_SLIDING_DATE_RANGE_TYPES ) )
            {
                var selectedDateRangeTypes = configurationValues[ENABLED_SLIDING_DATE_RANGE_TYPES]
                    .Value
                    .SplitDelimitedValues()
                    .Select( a => a.ConvertToEnum<SlidingDateRangePicker.SlidingDateRangeType>() )
                    .Where( a => a != SlidingDateRangePicker.SlidingDateRangeType.All );
                picker.EnabledSlidingDateRangeTypes = selectedDateRangeTypes.ToArray();
            }

            if ( configurationValues != null && configurationValues.ContainsKey( ENABLED_SLIDING_DATE_RANGE_UNITS ) )
            {
                var selectedDateRangeUnits = configurationValues[ENABLED_SLIDING_DATE_RANGE_UNITS].Value.SplitDelimitedValues().Select( a => a.ConvertToEnum<SlidingDateRangePicker.TimeUnitType>() );
                picker.EnabledSlidingDateRangeUnits = selectedDateRangeUnits.ToArray();
            }

            return picker;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( System.Web.UI.Control control, System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues )
        {
            SlidingDateRangePicker editor = control as SlidingDateRangePicker;
            if ( editor != null && editor.SlidingDateRangeMode != SlidingDateRangePicker.SlidingDateRangeType.All )
            {
                return editor.DelimitedValues;
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( System.Web.UI.Control control, System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            SlidingDateRangePicker editor = control as SlidingDateRangePicker;
            if ( editor != null )
            {
                editor.DelimitedValues = value;
            }
        }

        /// <summary>
        /// Creates the control needed to filter (query) values using this field type.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override System.Web.UI.Control FilterControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, Rock.Reporting.FilterMode filterMode )
        {
            // This field type does not support filtering
            return null;
        }

#endif
        #endregion
    }
}