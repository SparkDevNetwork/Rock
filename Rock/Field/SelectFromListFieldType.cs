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

using Rock.Model;
using Rock.Reporting;
using Rock.ViewModels.Utility;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// abstract field that lets you pick multiple items from a CheckListBox
    /// </summary>
    public abstract class SelectFromListFieldType : FieldType
    {
        #region Configuration

        private const string CLIENT_VALUES = "values";
        private const string REPEAT_COLUMNS = "repeatColumns";
        private const string ENHANCED_SELECTION_KEY = "enhancedselection";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            List<string> configKeys = base.ConfigurationKeys();
            configKeys.Add( REPEAT_COLUMNS );
            configKeys.Add( ENHANCED_SELECTION_KEY );
            return configKeys;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string privateValue )
        {
            var clientValues = base.GetPublicConfigurationValues( privateConfigurationValues, usage, privateValue );

            var repeatColumns = privateConfigurationValues.GetValueOrNull( REPEAT_COLUMNS )?.AsIntegerOrNull() ?? 0;

            if ( repeatColumns == 0 )
            {
                clientValues[REPEAT_COLUMNS] = "4";
            }

            var values = GetListSource( privateConfigurationValues.ToDictionary( k => k.Key, k => new ConfigurationValue( k.Value ) ) )
                    .Select( kvp => new ListItemBag
                    {
                        Value = kvp.Key,
                        Text = kvp.Value
                    } )
                    .ToList()
                    .ToCamelCaseJson( false, true );

            clientValues.AddOrReplace( CLIENT_VALUES, values );

            return clientValues;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            List<Control> controls = base.ConfigurationControls();

            // option for Displaying an enhanced 'chosen' value picker
            var cbEnhanced = new RockCheckBox();
            cbEnhanced.AutoPostBack = true;
            cbEnhanced.CheckedChanged += OnQualifierUpdated;
            cbEnhanced.Label = "Enhance For Long Lists";
            cbEnhanced.Help = "When set, will render a searchable selection of options.";
            controls.Add( cbEnhanced );

            var tbRepeatColumns = new NumberBox();
            tbRepeatColumns.Label = "Number of Columns";
            tbRepeatColumns.Help = "Select how many columns the list should use before going to the next row. If blank or 0 then 4 columns will be displayed. There is no upper limit enforced here however the block this is used in might add contraints due to available space.";
            tbRepeatColumns.MinimumValue = "0";
            tbRepeatColumns.AutoPostBack = true;
            tbRepeatColumns.TextChanged += OnQualifierUpdated;
            controls.Add( tbRepeatColumns );

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = base.ConfigurationValues( controls );

            string description = "Select how many columns the list should use before going to the next row. If blank 4 is used.";
            configurationValues.Add( REPEAT_COLUMNS, new ConfigurationValue( "Repeat Columns", description, string.Empty ) );

            description = "When set, will render a searchable selection of options.";
            configurationValues.Add( ENHANCED_SELECTION_KEY, new ConfigurationValue( "Enhance For Long Lists", description, string.Empty ) );

            if ( controls != null && controls.Count >= 2 )
            {
                var cbEnhanced = controls[0] as RockCheckBox;
                var tbRepeatColumns = controls[1] as NumberBox;

                tbRepeatColumns.Visible = !cbEnhanced.Checked;

                configurationValues[ENHANCED_SELECTION_KEY].Value = cbEnhanced.Checked.ToString();
                configurationValues[REPEAT_COLUMNS].Value = tbRepeatColumns.Visible ? tbRepeatColumns.Text : string.Empty;
            }

            return configurationValues;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="configurationValues">The configuration values.</param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            base.SetConfigurationValues( controls, configurationValues );

            if ( controls != null && controls.Count >= 2 && configurationValues != null )
            {
                var cbEnhanced = controls[0] as RockCheckBox;
                var tbRepeatColumns = controls[1] as NumberBox;

                cbEnhanced.Checked = configurationValues.ContainsKey( ENHANCED_SELECTION_KEY ) ? configurationValues[ENHANCED_SELECTION_KEY].Value.AsBoolean() : cbEnhanced.Checked;
                tbRepeatColumns.Text = configurationValues.ContainsKey( REPEAT_COLUMNS ) ? configurationValues[REPEAT_COLUMNS].Value : string.Empty;
                tbRepeatColumns.Visible = !cbEnhanced.Checked;
            }
        }

        #endregion Configuration

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( privateValue == null )
            {
                return string.Empty;
            }

            var valueGuidList = privateValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList();

            return GetListSource( privateConfigurationValues.ToDictionary( k => k.Key, k => new ConfigurationValue( k.Value ) ) )
                .Where( a => valueGuidList.Contains( a.Key.AsGuid() ) )
                .Select( s => s.Value )
                .ToList()
                .AsDelimited( ", " );
        }

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
            return GetTextValue( value, configurationValues.ToDictionary( k => k.Key, k => k.Value.Value ) );
        }

        #endregion

        #region Edit Control 

        /// <summary>
        /// Gets the list source.
        /// </summary>
        /// <value>
        /// The list source.
        /// </value>
        internal abstract Dictionary<string, string> GetListSource( Dictionary<string, ConfigurationValue> configurationValues );

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
            if ( configurationValues == null )
            {
                return null;
            }

            ListControl editControl = null;

            if ( configurationValues.ContainsKey( ENHANCED_SELECTION_KEY ) && configurationValues[ENHANCED_SELECTION_KEY].Value.AsBoolean() )
            {
                editControl = new RockListBox { ID = id };
                ( ( RockListBox ) editControl ).DisplayDropAsAbsolute = true;
            }
            else
            {
                editControl = new RockCheckBoxList { ID = id };

                var rockCheckBoxList = ( RockCheckBoxList ) editControl;
                rockCheckBoxList.RepeatDirection = RepeatDirection.Horizontal;

                // Fixed bug preventing what was is stated in the 'Columns' help text: "If blank or 0 then 4 columns..."
                if ( configurationValues.ContainsKey( REPEAT_COLUMNS ) && configurationValues[REPEAT_COLUMNS].Value.AsInteger() != 0 )
                {
                    rockCheckBoxList.RepeatColumns = configurationValues[REPEAT_COLUMNS].Value.AsInteger();
                }
            }

            var listSource = GetListSource( configurationValues );

            if ( listSource.Any() )
            {
                foreach ( var item in listSource )
                {
                    ListItem listItem = new ListItem( item.Value, item.Key );
                    editControl.Items.Add( listItem );
                }

                return editControl;
            }
            else
            {
                return null;
            }
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

            if ( control != null && control is ListControl editControl )
            {
                foreach ( ListItem li in editControl.Items )
                {
                    if ( li.Selected )
                    {
                        values.Add( li.Value );
                    }
                }

                return values.AsDelimited<string>( "," );
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
            if ( value != null )
            {
                List<string> values = new List<string>();
                values.AddRange( value.SplitDelimitedValues() );

                if ( control != null && control is ListControl editControl )
                {
                    foreach ( ListItem li in editControl.Items )
                    {
                        li.Selected = values.Contains( li.Value, StringComparer.OrdinalIgnoreCase );
                    }
                }
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

        /// <summary>
        /// Gets the filter value control.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override Control FilterValueControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            var ddlList = new RockDropDownList();
            ddlList.ID = string.Format( "{0}_ddlList", id );
            ddlList.AddCssClass( "js-filter-control" );

            if ( !required )
            {
                ddlList.Items.Add( new ListItem() );
            }

            var listSource = GetListSource( configurationValues );

            if ( listSource.Any() )
            {
                foreach ( var item in listSource )
                {
                    ListItem listItem = new ListItem( item.Value, item.Key );
                    ddlList.Items.Add( listItem );
                }

                return ddlList;
            }

            return null;
        }

        /// <summary>
        /// Gets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control != null && control is RockDropDownList )
            {
                return ( ( RockDropDownList ) control ).SelectedValue;
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( control != null && control is RockDropDownList )
            {
                ( ( RockDropDownList ) control ).SetValue( value );
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
            var values = new List<string>();
            var listSource = GetListSource( configurationValues );

            foreach ( string key in value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
            {
                if ( listSource.ContainsKey( key ) )
                {
                    values.Add( listSource[key] );
                }
            }

            return AddQuotes( values.ToList().AsDelimited( "' OR '" ) );
        }

        #endregion

    }
}
