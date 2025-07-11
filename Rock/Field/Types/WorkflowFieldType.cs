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
using System.Linq;
#if WEBFORMS
using System.Web.UI;
using System.Web.UI.WebControls;
#endif
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security.SecurityGrantRules;
using Rock.Security;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a single (or null) workflow filtered by a selected workflow type
    /// </summary>
    [FieldTypeUsage( FieldTypeUsage.System )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.WORKFLOW )]
    public class WorkflowFieldType : FieldType, IEntityFieldType, IEntityReferenceFieldType, ISecurityGrantFieldType
    {
        #region Configuration

        private const string WORKFLOW_TYPE_KEY = "workflowtype";
        private const string WORKFLOW_TYPE_OPTIONS_KEY = "workflowtypeoptions";

        /// <inheritdoc />
        public override Dictionary<string, string> GetPublicEditConfigurationProperties( Dictionary<string, string> privateConfigurationValues )
        {
            var configurationProperties = base.GetPublicEditConfigurationProperties( privateConfigurationValues );

            if ( !configurationProperties.ContainsKey( WORKFLOW_TYPE_OPTIONS_KEY ) )
            {
                var workflowTypes = WorkflowTypeCache.All().OrderBy( wt => wt.Name ).ToListItemBagList();
                configurationProperties[WORKFLOW_TYPE_OPTIONS_KEY] = workflowTypes.ToCamelCaseJson( false, true );
            }

            return configurationProperties;
        }

        /// <inheritdoc />
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string value )
        {
            var configurationValues = base.GetPublicConfigurationValues( privateConfigurationValues, usage, value );

            if ( usage != ConfigurationValueUsage.View && configurationValues.TryGetValue( WORKFLOW_TYPE_KEY, out string workflowTypeIdString ) && int.TryParse( workflowTypeIdString, out int workflowTypeId ) )
            {
                var workflowGuid = WorkflowTypeCache.GetGuid( workflowTypeId );
                if ( workflowGuid != null )
                {
                    configurationValues[WORKFLOW_TYPE_KEY] = workflowGuid.ToString();
                }
            }

            return configurationValues;
        }

        /// <inheritdoc />
        public override Dictionary<string, string> GetPrivateConfigurationValues( Dictionary<string, string> publicConfigurationValues )
        {
            var configurationValues = base.GetPrivateConfigurationValues( publicConfigurationValues );

            if ( configurationValues.TryGetValue( WORKFLOW_TYPE_KEY, out string workflowTypeIdString ) && Guid.TryParse( workflowTypeIdString, out Guid workflowTypeGuid ) )
            {
                var workflowId = WorkflowTypeCache.GetId( workflowTypeGuid );
                if ( workflowId != null )
                {
                    configurationValues[WORKFLOW_TYPE_KEY] = workflowId.ToString();
                }
            }

            return configurationValues;
        }

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            string formattedValue = string.Empty;

            Guid guid = Guid.Empty;
            if ( Guid.TryParse( privateValue, out guid ) )
            {
                using ( var rockContext = new RockContext() )
                {
                    var workflow = new WorkflowService( rockContext ).GetNoTracking( guid );
                    if ( workflow != null )
                    {
                        formattedValue = workflow.Name;
                    }
                }
            }

            return formattedValue;
        }

        #endregion

        #region Edit Control

        /// <inheritdoc/>
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetTextValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( privateValue.IsNotNullOrWhiteSpace() )
            {
                var guid = privateValue.AsGuidOrNull();
                ListItemBag workflow = null;

                using ( var rockContext = new RockContext() )
                {
                    if ( guid.HasValue )
                    {
                        workflow = new WorkflowService( rockContext ).GetSelect( guid.Value, w => new ListItemBag()
                        {
                            Text = w.Name,
                            Value = w.Guid.ToString()
                        } );
                    }
                    else
                    {
                        var id = privateValue.AsIntegerOrNull();
                        if ( id.HasValue )
                        {
                            workflow = new WorkflowService( rockContext ).GetSelect( id.Value, w => new ListItemBag()
                            {
                                Text = w.Name,
                                Value = w.Guid.ToString()
                            } );
                        }
                    }

                    if ( workflow != null )
                    {
                        return workflow.ToCamelCaseJson( false, true );
                    }
                }
            }
            return string.Empty;
        }

        /// <inheritdoc/>
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            var jsonValue = publicValue.FromJsonOrNull<ListItemBag>();

            if ( jsonValue != null )
            {
                return jsonValue.Value;
            }

            return string.Empty;
        }

        #endregion

        #region Entity Methods

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public IEntity GetEntity( string value )
        {
            return GetEntity( value, null );
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public IEntity GetEntity( string value, RockContext rockContext )
        {
            Guid? guid = value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                rockContext = rockContext ?? new RockContext();
                return new WorkflowService( rockContext ).Get( guid.Value );
            }

            return null;
        }

        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            Guid? guid = privateValue.AsGuidOrNull();

            if ( !guid.HasValue )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var workflowId = new WorkflowService( rockContext ).GetId( guid.Value );

                if ( !workflowId.HasValue )
                {
                    return null;
                }

                return new List<ReferencedEntity>
                {
                    new ReferencedEntity( EntityTypeCache.GetId<Model.Workflow>().Value, workflowId.Value )
                };
            }
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the Name property of a Workflow and
            // should have its persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<Model.Workflow>().Value, nameof( Model.Workflow.Name ) )
            };
        }

        #endregion

        #region ISecurityGrantFieldType

        /// <inheritdoc/>
        public void AddRulesToSecurityGrant( SecurityGrant grant, Dictionary<string, string> privateConfigurationValues )
        {
            var workflowTypeId = privateConfigurationValues.GetValueOrDefault( WORKFLOW_TYPE_KEY, "" ).AsIntegerOrNull();

            if ( workflowTypeId.HasValue )
            {
                var workflowType = WorkflowTypeCache.Get( workflowTypeId.Value );

                if ( workflowType != null )
                {
                    grant.AddRule( new EntitySecurityGrantRule( workflowType.TypeId, workflowType.Id ) );
                }
            }
            else
            {
                var workflowTypeEntityType = EntityTypeCache.Get<WorkflowType>();

                grant.AddRule( new EntityTypeSecurityGrantRule( workflowTypeEntityType.Id ) );
            }
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
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( WORKFLOW_TYPE_KEY );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            // build a drop down list of defined types (the one that gets selected is
            // used to build a list of defined values) 
            var ddl = new RockDropDownList();
            controls.Add( ddl );
            ddl.AutoPostBack = true;
            ddl.SelectedIndexChanged += OnQualifierUpdated;
            ddl.Label = "Workflow Type";
            ddl.Help = "Workflow Type to select workflows from, if left blank any workflow type's workflows can be selected.";

            ddl.Items.Add( new ListItem() );

            var workflowTypeService = new WorkflowTypeService( new RockContext() );
            var workflowTypes = workflowTypeService.Queryable().OrderBy( a => a.Name ).ToList();
            workflowTypes.ForEach( g =>
                ddl.Items.Add( new ListItem( g.Name, g.Id.ToString().ToUpper() ) )
            );

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
            configurationValues.Add( WORKFLOW_TYPE_KEY, new ConfigurationValue( "Workflow Type", "Workflow Type to select workflows from, if left blank any workflow type's workflows can be selected.", "" ) );

            if ( controls != null && controls.Count == 1 )
            {
                if ( controls[0] != null && controls[0] is DropDownList )
                {
                    configurationValues[WORKFLOW_TYPE_KEY].Value = ( ( DropDownList ) controls[0] ).SelectedValue;
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
            if ( controls != null && controls.Count == 1 && configurationValues != null )
            {
                if ( controls[0] != null && controls[0] is DropDownList && configurationValues.ContainsKey( WORKFLOW_TYPE_KEY ) )
                {
                    ( ( DropDownList ) controls[0] ).SelectedValue = configurationValues[WORKFLOW_TYPE_KEY].Value;
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
        public override System.Web.UI.Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            WorkflowPicker editControl = new WorkflowPicker { ID = id };

            if ( configurationValues != null && configurationValues.ContainsKey( WORKFLOW_TYPE_KEY ) )
            {
                int workflowTypeId = 0;
                if ( Int32.TryParse( configurationValues[WORKFLOW_TYPE_KEY].Value, out workflowTypeId ) && workflowTypeId > 0 )
                {
                    editControl.WorkflowTypeId = workflowTypeId;
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
        public override string GetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            WorkflowPicker workflowPicker = control as WorkflowPicker;
            if ( workflowPicker != null )
            {
                if ( workflowPicker.WorkflowId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var workflowGuid = new WorkflowService( rockContext ).GetGuid( workflowPicker.WorkflowId.Value );
                        if ( workflowGuid != null )
                        {
                            return workflowGuid.ToString();
                        }
                    }
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
        public override void SetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            WorkflowPicker workflowPicker = control as WorkflowPicker;
            if ( workflowPicker != null )
            {
                Guid? guid = value.AsGuidOrNull();
                if ( guid.HasValue )
                {
                    var workflowId = new WorkflowService( new RockContext() ).GetId( guid.Value );
                    if ( workflowId != null )
                    {
                        workflowPicker.WorkflowId = workflowId;
                        return;
                    }
                }
                else
                {
                    int? id = value.AsIntegerOrNull();
                    if ( id.HasValue )
                    {
                        workflowPicker.WorkflowId = id.Value;
                    }
                }

                workflowPicker.WorkflowId = null;
            }
        }

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public int? GetEditValueAsEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            Guid guid = GetEditValue( control, configurationValues ).AsGuid();
            return new WorkflowService( new RockContext() ).GetId( guid );
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            var itemGuid = new WorkflowService( new RockContext() ).GetGuid( id ?? 0 );
            string guidValue = itemGuid?.ToString() ?? string.Empty;
            SetEditValue( control, configurationValues, guidValue );
        }

#endif
        #endregion
    }
}