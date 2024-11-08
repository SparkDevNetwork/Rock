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
using Rock.ViewModels.Utility;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a checkboxlist of LavaCommands
    /// Stored as a comma-delimited list of LavaCommand names
    /// </summary>
    [Serializable]
    [FieldTypeUsage( FieldTypeUsage.System )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.LAVA_COMMANDS )]
    public class LavaCommandsFieldType : FieldType
    {
        #region Configuration

        private const string REPEAT_COLUMNS = "repeatColumns";

        #endregion Configuration

        #region Edit Control

        /// <inheritdoc />
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var splitValues = privateValue.SplitDelimitedValues( ",", StringSplitOptions.RemoveEmptyEntries );

            if ( splitValues.Length > 0 )
            {
                var values = splitValues.Select( lc => new ListItemBag() { Text = lc.SplitCase(), Value = lc } );
                return values.ToCamelCaseJson( false, true );
            }

            return base.GetPublicEditValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc />
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            var jsonValues = publicValue.FromJsonOrNull<List<ListItemBag>>();

            if ( jsonValues != null )
            {
                var values = jsonValues.Select( li => li.Value );
                return values.JoinStrings( "," );
            }

            return base.GetPrivateEditValue( publicValue, privateConfigurationValues );
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
            configKeys.Add( REPEAT_COLUMNS );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            List<Control> controls = base.ConfigurationControls();

            var tbRepeatColumns = new NumberBox();
            tbRepeatColumns.Label = "Columns";
            tbRepeatColumns.Help = $"Select how many columns the list should use before going to the next row. If blank or 0 then {LavaCommandsPicker.DefaultRepeatColumns} columns will be displayed. There is no upper limit enforced here however the block this is used in might add contraints due to available space.";
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

            string description = $"Select how many columns the list should use before going to the next row. If blank {LavaCommandsPicker.DefaultRepeatColumns} is used.";
            configurationValues.Add( REPEAT_COLUMNS, new ConfigurationValue( "Repeat Columns", description, LavaCommandsPicker.DefaultRepeatColumns.ToString() ) );

            if ( controls != null && controls.Count > 0 )
            {
                var tbRepeatColumns = controls[0] as NumberBox;
                configurationValues[REPEAT_COLUMNS].Value = tbRepeatColumns.Visible ? tbRepeatColumns.Text : LavaCommandsPicker.DefaultRepeatColumns.ToString();
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

            if ( controls != null && controls.Count > 0 && configurationValues != null )
            {
                var tbRepeatColumns = controls[0] as NumberBox;
                tbRepeatColumns.Text = configurationValues.ContainsKey( REPEAT_COLUMNS ) ? configurationValues[REPEAT_COLUMNS].Value : LavaCommandsPicker.DefaultRepeatColumns.ToString();
            }
        }

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
            var editControl = new LavaCommandsPicker { ID = id };

            if ( configurationValues.ContainsKey( REPEAT_COLUMNS ) )
            {
                editControl.RepeatColumns = configurationValues[REPEAT_COLUMNS].Value.AsInteger();
            }

            return editControl;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as LavaCommandsPicker;
            if ( picker != null )
            {
                return picker.SelectedLavaCommands?.AsDelimited( "," );
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues"></param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var picker = control as LavaCommandsPicker;
            if ( picker != null )
            {
                picker.SelectedLavaCommands = value?.SplitDelimitedValues().ToList() ?? new List<string>();
            }
        }

#endif
        #endregion
    }
}