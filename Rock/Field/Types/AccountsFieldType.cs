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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Stored as a delimited list of FinancialAccount Guids
    /// </summary>
    public class AccountsFieldType : FieldType
    {
        #region Configuration

        private const string DISPLAY_PUBLIC_NAME = "displaypublicname";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( DISPLAY_PUBLIC_NAME );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            // Add checkbox for deciding if the textbox is used for storing a password
            var cb = new RockCheckBox();
            controls.Add( cb );
            cb.AutoPostBack = true;
            cb.Checked = true;
            cb.CheckedChanged += OnQualifierUpdated;
            cb.Label = "Display Public Name";
            cb.Text = "Yes";
            cb.Help = "When set, public name will be displayed.";
            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            var configurationValues = base.ConfigurationValues( controls );
            configurationValues.Add( DISPLAY_PUBLIC_NAME, new ConfigurationValue( "Display Public Name", "When set, public name will be displayed.", "True" ) );

            if ( controls != null && controls.Count > 0 && controls[0] != null && controls[0] is CheckBox )
            {
                configurationValues[DISPLAY_PUBLIC_NAME].Value = ( ( CheckBox ) controls[0] ).Checked.ToString();
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
            if ( controls != null && controls.Count > 0 && configurationValues != null )
            {
                if ( controls[0] != null && controls[0] is CheckBox && configurationValues.ContainsKey( DISPLAY_PUBLIC_NAME ) )
                {
                    ( ( CheckBox ) controls[0] ).Checked = configurationValues[DISPLAY_PUBLIC_NAME].Value.AsBoolean();
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
            string formattedValue = string.Empty;

            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                bool displayPublicName = true;

                if ( configurationValues != null &&
                     configurationValues.ContainsKey( DISPLAY_PUBLIC_NAME ) )
                {
                    displayPublicName = configurationValues[DISPLAY_PUBLIC_NAME].Value.AsBoolean();
                }

                var guids = value.SplitDelimitedValues();

                using ( var rockContext = new RockContext() )
                {
                    var accounts = new FinancialAccountService( rockContext ).Queryable().AsNoTracking().Where( a => guids.Contains( a.Guid.ToString() ) );
                    if ( accounts.Any() )
                    {
                        formattedValue = string.Join( ", ", ( from account in accounts select displayPublicName && account.PublicName != null && account.PublicName != string.Empty ? account.PublicName : account.Name ).ToArray() );
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
            bool displayPublicName = true;

            if ( configurationValues != null &&
                 configurationValues.ContainsKey( DISPLAY_PUBLIC_NAME ) )
            {
                displayPublicName = configurationValues[DISPLAY_PUBLIC_NAME].Value.AsBoolean();
            }
            return new AccountPicker { ID = id, AllowMultiSelect = true, DisplayPublicName = displayPublicName };
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as AccountPicker;

            if ( picker != null )
            {
                var guids = new List<Guid>();
                var ids = picker.SelectedValuesAsInt();
                using ( var rockContext = new RockContext() )
                {
                    var accounts = new FinancialAccountService( rockContext ).Queryable().AsNoTracking().Where( a => ids.Contains( a.Id ) );

                    if ( accounts.Any() )
                    {
                        guids = accounts.Select( a => a.Guid ).ToList();
                    }
                }

                return string.Join( ",", guids );
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
            if ( value != null )
            {
                var picker = control as AccountPicker;
                var guids = new List<Guid>();

                if ( picker != null )
                {
                    var ids = value.Split( new[] { ',' } );

                    foreach ( var id in ids )
                    {
                        Guid guid;

                        if ( Guid.TryParse( id, out guid ) )
                        {
                            guids.Add( guid );
                        }
                    }

                    var accounts = new FinancialAccountService( new RockContext() ).Queryable().Where( a => guids.Contains( a.Guid ) );
                    picker.SetValues( accounts );
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
            var control = base.FilterValueControl( configurationValues, id, required, filterMode );
            if ( control is AccountPicker )
            {
                var accountPicker = (AccountPicker)control;
                accountPicker.AllowMultiSelect = false;
                accountPicker.Required = required;
            }
            return control;
        }

        #endregion

    }
}
