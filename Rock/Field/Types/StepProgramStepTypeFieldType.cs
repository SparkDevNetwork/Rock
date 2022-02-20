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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a single (or null) step type filtered by a selected step program
    /// Stored as "StepProgram.Guid|StepType.Guid"
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    public class StepProgramStepTypeFieldType : FieldType
    {
        #region Keys

        /// <summary>
        /// Keys for the config values
        /// </summary>
        public static class ConfigKey
        {
            /// <summary>
            /// The default step program unique identifier
            /// </summary>
            public const string DefaultStepProgramGuid = "DefaultStepProgramGuid";
        }

        #endregion Keys

        #region Configuration

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            return new List<string>
            {
                ConfigKey.DefaultStepProgramGuid
            };
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var stepProgramPicker = new StepProgramPicker
            {
                Label = "Default Step Program",
                Help = "The default step program selection"
            };

            StepProgramPicker.LoadDropDownItems( stepProgramPicker, true );
            return new List<Control> { stepProgramPicker };
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            var configurationValues = new Dictionary<string, ConfigurationValue>
            {
                { ConfigKey.DefaultStepProgramGuid, new ConfigurationValue( "Default Step Program Guid", "The default step program.", string.Empty ) }
            };

            if ( controls != null && controls.Count == 1 )
            {
                var stepProgramPicker = controls[0] as StepProgramPicker;

                if ( stepProgramPicker != null )
                {
                    configurationValues[ConfigKey.DefaultStepProgramGuid].Value = stepProgramPicker.SelectedValue;
                }
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
            if ( controls != null && controls.Count == 1 && configurationValues != null && configurationValues.ContainsKey( ConfigKey.DefaultStepProgramGuid ) )
            {
                var stepProgramPicker = controls[0] as StepProgramPicker;

                if ( stepProgramPicker != null )
                {
                    stepProgramPicker.SelectedValue = configurationValues[ConfigKey.DefaultStepProgramGuid].Value;
                }
            }
        }

        #endregion Configuration

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
            var formattedValue = string.Empty;
            GetModelsFromAttributeValue( value, out var stepProgram, out var stepType );

            if ( stepType != null )
            {
                formattedValue = "Step Type: " + stepType.Name;
            }
            else if ( stepProgram != null )
            {
                formattedValue = "Step Program: " + stepProgram.Name;
            }

            return base.FormatValue( parentControl, formattedValue, null, condensed );
        }

        #endregion Formatting

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
            var editControl = new StepProgramStepTypePicker { ID = id };

            if ( configurationValues != null && configurationValues.ContainsKey( ConfigKey.DefaultStepProgramGuid ) )
            {
                var stepProgramGuid = configurationValues[ConfigKey.DefaultStepProgramGuid].Value.AsGuidOrNull();

                if ( stepProgramGuid.HasValue )
                {
                    var stepProgram = new StepProgramService( new RockContext() ).GetNoTracking( stepProgramGuid.Value );
                    editControl.DefaultStepProgramId = stepProgram?.Id;
                }
            }

            return editControl;
        }

        /// <summary>
        /// Reads new values entered by the user for the field ( as Guid )
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var stepProgramStepTypePicker = control as StepProgramStepTypePicker;

            if ( stepProgramStepTypePicker != null )
            {
                var rockContext = new RockContext();
                Guid? stepProgramGuid = null;
                Guid? stepTypeGuid = null;

                if ( stepProgramStepTypePicker.StepProgramId.HasValue )
                {
                    var stepProgram = new StepProgramService( rockContext ).GetNoTracking( stepProgramStepTypePicker.StepProgramId.Value );

                    if ( stepProgram != null )
                    {
                        stepProgramGuid = stepProgram.Guid;
                    }
                }

                if ( stepProgramStepTypePicker.StepTypeId.HasValue )
                {
                    var stepType = new StepTypeService( rockContext ).GetNoTracking( stepProgramStepTypePicker.StepTypeId.Value );

                    if ( stepType != null )
                    {
                        stepTypeGuid = stepType.Guid;
                    }
                }

                if ( stepProgramGuid.HasValue || stepTypeGuid.HasValue )
                {
                    return string.Format( "{0}|{1}", stepProgramGuid, stepTypeGuid );
                }
            }

            return null;
        }

        /// <summary>
        /// Sets the value. ( as Guid )
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var stepProgramStepTypePicker = control as StepProgramStepTypePicker;

            if ( stepProgramStepTypePicker != null )
            {
                GetModelsFromAttributeValue( value, out var stepProgram, out var stepType );
                stepProgramStepTypePicker.StepProgramId = stepProgram?.Id;
                stepProgramStepTypePicker.StepTypeId = stepType?.Id;
            }
        }

        #endregion Edit Control

        #region Parse Helpers

        /// <summary>
        /// Gets the models from the delimited values.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="stepProgramGuid">The step program unique identifier.</param>
        /// <param name="stepTypeGuid">The step type unique identifier.</param>
        public static void ParseDelimitedGuids( string value, out Guid? stepProgramGuid, out Guid? stepTypeGuid )
        {
            var parts = ( value ?? string.Empty ).Split( '|' );

            if ( parts.Length == 1 )
            {
                // If there is only one guid, assume it is the type
                stepProgramGuid = null;
                stepTypeGuid = parts[0].AsGuidOrNull();
                return;
            }

            stepProgramGuid = parts.Length > 0 ? parts[0].AsGuidOrNull() : null;
            stepTypeGuid = parts.Length > 1 ? parts[1].AsGuidOrNull() : null;
        }

        /// <summary>
        /// Gets the models from the delimited values.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="stepProgram">The step program</param>
        /// <param name="stepType">The step type</param>
        private void GetModelsFromAttributeValue( string value, out StepProgram stepProgram, out StepType stepType )
        {
            stepProgram = null;
            stepType = null;

            ParseDelimitedGuids( value, out var stepProgramGuid, out var stepTypeGuid );

            if ( stepProgramGuid.HasValue || stepTypeGuid.HasValue )
            {
                var rockContext = new RockContext();

                if ( stepProgramGuid.HasValue )
                {
                    var stepProgramService = new StepProgramService( rockContext );
                    stepProgram = stepProgramService.Queryable().AsNoTracking().FirstOrDefault( sp => sp.Guid == stepProgramGuid.Value );
                }

                if ( stepTypeGuid.HasValue )
                {
                    var stepTypeService = new StepTypeService( rockContext );
                    stepType = stepTypeService.Queryable().AsNoTracking().FirstOrDefault( sp => sp.Guid == stepTypeGuid.Value );
                }
            }
        }

        #endregion Parse Helpers
    }
}