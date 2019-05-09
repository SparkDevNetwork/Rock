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
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display value filter
    /// </summary>
    [Serializable]
    public class ValueFilterFieldType : FieldType
    {
        #region Configuration

        /// <summary>
        /// The hide filter mode
        /// </summary>
        public const string HIDE_FILTER_MODE = "hidefiltermode";

        /// <summary>
        /// The comparison types
        /// </summary>
        public const string COMPARISON_TYPES = "comparisontypes";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();

            configKeys.Add( HIDE_FILTER_MODE );
            configKeys.Add( COMPARISON_TYPES );

            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            // Add checkbox for deciding if the filter mode should be hidden
            var cbHideFilterMode = new RockCheckBox
            {
                AutoPostBack = true,
                Label = "Hide Filter mode",
                Text = "Yes",
                Help = "When set, filter mode will be hidden."
            };
            cbHideFilterMode.CheckedChanged += OnQualifierUpdated;
            controls.Add( cbHideFilterMode );

            var defaultComparisonType = Reporting.ComparisonHelper.StringFilterComparisonTypes | Model.ComparisonType.RegularExpression;
            var cbComparisonTypes = new RockCheckBoxList()
            {
                AutoPostBack = true,
                Label = "Comparison Types",
                Required = true,
                RepeatColumns = 2,
                Help = "The comparison types the user can select from."
            };
            cbComparisonTypes.BindToEnum<Model.ComparisonType>();
            cbComparisonTypes.SetValues( defaultComparisonType.GetFlags<Model.ComparisonType>().Cast<int>() );
            cbComparisonTypes.SelectedIndexChanged += OnQualifierUpdated;
            controls.Add( cbComparisonTypes );

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>
            {
                { HIDE_FILTER_MODE, new ConfigurationValue( "Hide Filter Mode", "When set, filter mode will be hidden.", "" ) },
                { COMPARISON_TYPES, new ConfigurationValue( "Comparison Types", "The comparison types the user can select from.", "" ) }
            };

            if ( controls != null )
            {
                if ( controls.Count > 1 )
                {
                    if ( controls[0] is CheckBox cbHideFilterMode )
                    {
                        configurationValues[HIDE_FILTER_MODE].Value = cbHideFilterMode.Checked.ToString();
                    }

                    if ( controls[1] is RockCheckBoxList cbComparisonTypes )
                    {
                        Model.ComparisonType comparisonType = 0;

                        foreach ( int value in cbComparisonTypes.SelectedValuesAsInt )
                        {
                            comparisonType |= ( Model.ComparisonType ) value;
                        }

                        configurationValues[COMPARISON_TYPES].Value = ( ( int ) comparisonType ).ToString();
                    }
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
            if ( controls != null && controls.Count > 1 && configurationValues != null )
            {
                if ( configurationValues.ContainsKey( HIDE_FILTER_MODE ) )
                {
                    if ( controls[0] is CheckBox cbHidefilterMode )
                    {
                        cbHidefilterMode.Checked = configurationValues[HIDE_FILTER_MODE].Value.AsBoolean();
                    }
                }

                if ( configurationValues.ContainsKey( COMPARISON_TYPES ) )
                {
                    if ( controls[1] is RockCheckBoxList cbComparisonTypes )
                    {
                        Model.ComparisonType comparisonType = ( Model.ComparisonType ) configurationValues[COMPARISON_TYPES].Value.AsInteger();

                        cbComparisonTypes.SetValues( comparisonType.GetFlags<Model.ComparisonType>().Cast<int>() );
                    }
                }
            }
        }

        #endregion

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
            var filter = GetFilterExpression( configurationValues, value );

            try
            {
                return filter.ToString();
            }
            catch
            {
                return string.Empty;
            }
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
            // use un-condensed formatted value as the sort value
            return this.FormatValue( parentControl, value, configurationValues, false );
        }

        /// <summary>
        /// Formats the value as HTML.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        public override string FormatValueAsHtml( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed = false )
        {
            // NOTE: this really should not be encoding the value. FormatValueAsHtml method is really designed to wrap a value with appropriate html (i.e. convert an email into a mailto anchor tag)
            // but keeping it here for backward compatibility.
            return System.Web.HttpUtility.HtmlEncode( FormatValue( parentControl, value, configurationValues, condensed ) );
        }

        /// <summary>
        /// Formats the value as HTML.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        public override string FormatValueAsHtml( Control parentControl, int? entityTypeId, int? entityId, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed = false )
        {
            // NOTE: this really should not be encoding the value. FormatValueAsHtml method is really designed to wrap a value with appropriate html (i.e. convert an email into a mailto anchor tag)
            // but keeping it here for backward compatibility.
            return System.Web.HttpUtility.HtmlEncode( FormatValue( parentControl, entityTypeId, entityId, value, configurationValues, condensed ) );
        }

        #endregion

        #region Edit Control

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The id.</param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            ValueFilter ft = new ValueFilter
            {
                ID = id
            };

            if ( configurationValues != null )
            {
                if ( configurationValues.ContainsKey( HIDE_FILTER_MODE ) &&
                    configurationValues[HIDE_FILTER_MODE].Value.AsBoolean() )
                {
                    ft.HideFilterMode = true;
                }

                if ( configurationValues.ContainsKey( COMPARISON_TYPES ) )
                {
                    Model.ComparisonType comparisonType = ( Model.ComparisonType ) configurationValues[COMPARISON_TYPES].Value.AsInteger();

                    if ( comparisonType == 0 )
                    {
                        comparisonType = Reporting.ComparisonHelper.StringFilterComparisonTypes | Model.ComparisonType.RegularExpression;
                    }

                    ft.ComparisonTypes = comparisonType;
                }
            }

            return ft;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var tvf = control as ValueFilter;

            if ( control != null )
            {
                tvf.Filter = ( CompoundFilterExpression ) FilterExpression.FromJsonOrNull( value );
            }
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var tvf = control as ValueFilter;

            if ( control == null || tvf.Filter.Filters.Count == 0 )
            {
                return string.Empty;
            }

            return tvf.Filter.ToJson();
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Gets the filter object that can be used to evaluate an object against the filter.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The attribute value.</param>
        /// <returns>A CompoundFilter object that can be used to evaluate the truth of the filter.</returns>
        public static FilterExpression GetFilterExpression( Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            return FilterExpression.FromJsonOrNull( value );
        }

        #endregion
    }
}