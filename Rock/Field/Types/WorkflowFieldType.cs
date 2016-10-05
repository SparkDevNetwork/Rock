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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a single (or null) workflow filtered by a selected workflow type
    /// </summary>
    public class WorkflowFieldType : FieldType, IEntityFieldType
    {

        #region Configuration

        private const string WORKFLOW_TYPE_KEY = "workflowtype";

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
                    configurationValues[WORKFLOW_TYPE_KEY].Value = ( (DropDownList)controls[0] ).SelectedValue;
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
                    ( (DropDownList)controls[0] ).SelectedValue = configurationValues[WORKFLOW_TYPE_KEY].Value;
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

            Guid guid = Guid.Empty;
            if ( Guid.TryParse( value, out guid ) )
            {
                var workflow = new WorkflowService( new RockContext() ).Get( guid );
                if ( workflow != null )
                {
                    formattedValue = workflow.Name;
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
                    var workflow = new WorkflowService( new RockContext() ).Get( workflowPicker.WorkflowId.Value );
                    if ( workflow != null )
                    {
                        return workflow.Guid.ToString();
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
                    var workflow = new WorkflowService( new RockContext() ).Get( guid.Value );
                    if ( workflow != null )
                    {
                        workflowPicker.WorkflowId = workflow.Id;
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

        #endregion

        #region Entity Methods

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public int? GetEditValueAsEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            Guid guid = GetEditValue( control, configurationValues ).AsGuid();
            var item = new WorkflowService( new RockContext() ).Get( guid );
            return item != null ? item.Id : (int?)null;
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            var item = new WorkflowService( new RockContext() ).Get( id ?? 0 );
            string guidValue = item != null ? item.Guid.ToString() : string.Empty;
            SetEditValue( control, configurationValues, guidValue );
        }

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

    }
}