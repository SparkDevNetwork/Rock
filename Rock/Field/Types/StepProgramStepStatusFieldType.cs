﻿// <copyright>
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
#if WEBFORMS
using System.Web.UI;
#endif
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a single (or null) step status filtered by a selected step program
    /// Stored as "StepProgram.Guid|StepStatus.Guid"
    /// </summary>
    [FieldTypeUsage( FieldTypeUsage.System )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( "F8E85355-2780-4772-9B21-30B84741E6D1" )]
    public class StepProgramStepStatusFieldType : FieldType, IEntityReferenceFieldType
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

        #endregion Configuration

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var formattedValue = string.Empty;
            GetModelsFromAttributeValue( privateValue, out var stepProgram, out var stepStatus );

            if ( stepStatus != null )
            {
                formattedValue = "Step Status: " + stepStatus.Name;
            }

            if ( stepProgram != null )
            {
                formattedValue = "Step Program: " + stepProgram.Name;
            }

            return formattedValue;
        }

        /// <inheritdoc/>
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetTextValue( privateValue, privateConfigurationValues );
        }

        #endregion Formatting

        #region Edit Control

        /// <inheritdoc/>
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            GetModelsFromAttributeValue( privateValue, out var stepProgram, out var stepStatus );
            if ( stepProgram == null || stepStatus == null )
            {
                return base.GetPublicEditValue( privateValue, privateConfigurationValues );
            }

            return new StepProgramStepStatus
            {
                StepProgram = stepProgram.ToListItemBag(),
                StepStatus = stepStatus.ToListItemBag()
            }
            .ToCamelCaseJson( false, true );
        }

        /// <inheritdoc/>
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            var stepProgramStepStatus = publicValue.FromJsonOrNull<StepProgramStepStatus>();
            var stepProgramGUID = stepProgramStepStatus?.StepProgram?.Value;
            var stepStatusGUID = stepProgramStepStatus?.StepStatus?.Value;
            if( stepProgramGUID == null && stepStatusGUID  == null) // If no value is provided, return null.
            {
                return null;
            }
            return $"{stepProgramGUID}|{stepStatusGUID}";
        }

        #endregion Edit Control

        #region Parse Helpers

        /// <summary>
        /// Gets the models from the delimited values.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="stepProgramGuid">The step program unique identifier.</param>
        /// <param name="stepStatusGuid">The step status unique identifier.</param>
        public static void ParseDelimitedGuids( string value, out Guid? stepProgramGuid, out Guid? stepStatusGuid )
        {
            var parts = ( value ?? string.Empty ).Split( '|' );

            if ( parts.Length == 1 )
            {
                // If there is only one guid, assume it is the status
                stepProgramGuid = null;
                stepStatusGuid = parts[0].AsGuidOrNull();
                return;
            }

            stepProgramGuid = parts.Length > 0 ? parts[0].AsGuidOrNull() : null;
            stepStatusGuid = parts.Length > 1 ? parts[1].AsGuidOrNull() : null;
        }

        /// <summary>
        /// Gets the models from the delimited values.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="stepProgram">The step program.</param>
        /// <param name="stepStatus">The step status.</param>
        private void GetModelsFromAttributeValue( string value, out StepProgram stepProgram, out StepStatus stepStatus )
        {
            stepProgram = null;
            stepStatus = null;

            ParseDelimitedGuids( value, out var stepProgramGuid, out var stepStatusGuid );

            if ( stepProgramGuid.HasValue || stepStatusGuid.HasValue )
            {
                var rockContext = new RockContext();

                if ( stepProgramGuid.HasValue )
                {
                    var stepProgramService = new StepProgramService( rockContext );
                    stepProgram = stepProgramService.Queryable().AsNoTracking().FirstOrDefault( sp => sp.Guid == stepProgramGuid.Value );
                }

                if ( stepStatusGuid.HasValue )
                {
                    var stepStatusService = new StepStatusService( rockContext );
                    stepStatus = stepStatusService.Queryable().AsNoTracking().FirstOrDefault( sp => sp.Guid == stepStatusGuid.Value );
                }
            }
        }

        #endregion Parse Helpers

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            ParseDelimitedGuids( privateValue, out var stepProgramGuid, out var stepStatusGuid );

            if ( !stepProgramGuid.HasValue && !stepStatusGuid.HasValue )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                // These are intentionally out of order to maintain backward compatibility
                // with the old FormatValue returning the wrong value. When that is fixed
                // this should also be fixed.
                if ( stepProgramGuid.HasValue )
                {
                    var stepProgramId = new StepProgramService( rockContext ).GetId( stepProgramGuid.Value );

                    if ( stepProgramId.HasValue )
                    {
                        return new List<ReferencedEntity>
                        {
                            new ReferencedEntity( EntityTypeCache.GetId<StepProgram>().Value, stepProgramId.Value )
                        };
                    }
                }

                if ( stepStatusGuid.HasValue )
                {
                    var stepStatusId = new StepStatusService( rockContext ).GetId( stepStatusGuid.Value );

                    if ( stepStatusId.HasValue )
                    {
                        return new List<ReferencedEntity>
                        {
                            new ReferencedEntity( EntityTypeCache.GetId<StepStatus>().Value, stepStatusId.Value )
                        };
                    }
                }

                return null;
            }
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the Name property of StepStatus and
            // StepProgram and should have its persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<StepStatus>().Value, nameof( StepStatus.Name ) ),
                new ReferencedProperty( EntityTypeCache.GetId<StepProgram>().Value, nameof( StepProgram.Name ) )
            };
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
                ? GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
                : GetCondensedTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
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
            var editControl = new StepProgramStepStatusPicker { ID = id };

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
            var stepProgramStepStatusPicker = control as StepProgramStepStatusPicker;

            if ( stepProgramStepStatusPicker != null )
            {
                var rockContext = new RockContext();
                Guid? stepProgramGuid = null;
                Guid? stepStatusGuid = null;

                if ( stepProgramStepStatusPicker.StepProgramId.HasValue )
                {
                    var stepProgram = new StepProgramService( rockContext ).GetNoTracking( stepProgramStepStatusPicker.StepProgramId.Value );

                    if ( stepProgram != null )
                    {
                        stepProgramGuid = stepProgram.Guid;
                    }
                }

                if ( stepProgramStepStatusPicker.StepStatusId.HasValue )
                {
                    var stepStatus = new StepStatusService( rockContext ).GetNoTracking( stepProgramStepStatusPicker.StepStatusId.Value );

                    if ( stepStatus != null )
                    {
                        stepStatusGuid = stepStatus.Guid;
                    }
                }

                if ( stepProgramGuid.HasValue || stepStatusGuid.HasValue )
                {
                    return string.Format( "{0}|{1}", stepProgramGuid, stepStatusGuid );
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
            var stepProgramStepStatusPicker = control as StepProgramStepStatusPicker;

            if ( stepProgramStepStatusPicker != null )
            {
                GetModelsFromAttributeValue( value, out var stepProgram, out var stepStatus );
                stepProgramStepStatusPicker.StepProgramId = stepProgram?.Id;
                stepProgramStepStatusPicker.StepStatusId = stepStatus?.Id;
            }
        }

#endif
        #endregion

        /// <summary>
        /// A POCO to store the Step Program Step Status as a ListItemBag
        /// </summary>
        private class StepProgramStepStatus
        {
            public ListItemBag StepProgram { get; set; }
            public ListItemBag StepStatus { get; set; }
        }
    }
}