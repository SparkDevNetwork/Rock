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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a dropdown list of activity types for a specific workflow type
    /// </summary>
    [Serializable]
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    public class WorkflowActivityFieldType : FieldType, IEntityFieldType
    {

        #region Configuration

        private const string WORKFLOW_TYPE_KEY = "WorkflowType";

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

            // build a drop down list of workflow types (the one that gets selected is
            // used to build a list of workflow activity types) 
            var ddl = new RockDropDownList();
            controls.Add( ddl );
            ddl.AutoPostBack = true;
            ddl.SelectedIndexChanged += OnQualifierUpdated;
            ddl.Label = "Workflow Type";
            ddl.Help = "The Workflow Type to select activities from.";
            var originalValue = ddl.SelectedValue;

            // Add empty field because the default value dropdown list will only be populated after the workflow type index have been changed.
            ddl.Items.Add( new ListItem( string.Empty, string.Empty ) );

            Rock.Model.WorkflowTypeService workflowTypeService = new Model.WorkflowTypeService( new RockContext() );
            foreach ( var workflowType in workflowTypeService.Queryable().OrderBy( w => w.Name ) )
            {
                ddl.Items.Add( new ListItem( workflowType.Name, workflowType.Guid.ToString() ) );
            }

            var httpContext = System.Web.HttpContext.Current;
            if ( string.IsNullOrEmpty( originalValue ) && httpContext != null && httpContext.Request != null && httpContext.Request.Params["WorkflowTypeId"] != null && httpContext.Request.Params["WorkflowTypeId"].AsIntegerOrNull() == 0 )
            {

                var workflowType = GetContextWorkflowType();
                ddl.Items.Add( new ListItem( ( string.IsNullOrWhiteSpace( workflowType.Name ) ? "Current Workflow" : workflowType.Name ), "" ) );
                ddl.SelectedIndex = ddl.Items.Count - 1;
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
            configurationValues.Add( WORKFLOW_TYPE_KEY, new ConfigurationValue( "Workflow Type", "The Workflow Type to select activities from", string.Empty ) );

            if ( controls != null && controls.Count > 0 && controls[0] != null && controls[0] is DropDownList )
            {
                configurationValues[WORKFLOW_TYPE_KEY].Value = ( ( DropDownList ) controls[0] ).SelectedValue;
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
            if ( controls != null && configurationValues != null && controls.Count > 0 && controls[0] != null && controls[0] is DropDownList && configurationValues.ContainsKey( WORKFLOW_TYPE_KEY ) )
            {
                ( ( DropDownList ) controls[0] ).SelectedValue = configurationValues[WORKFLOW_TYPE_KEY].Value;
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

            Guid guid = value.AsGuid();
            if ( !guid.IsEmpty() )
            {
                var workflowType = GetContextWorkflowType();
                if ( workflowType != null )
                {
                    formattedValue = workflowType.ActivityTypes
                        .Where( a => a.Guid.Equals( guid ) )
                        .Select( a => a.Name )
                        .FirstOrDefault();
                }

                if ( string.IsNullOrWhiteSpace( formattedValue ) )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        formattedValue = new WorkflowActivityTypeService( rockContext )
                            .Queryable()
                            .AsNoTracking()
                            .Where( a => a.Guid.Equals( guid ) )
                            .Select( a => a.Name )
                            .FirstOrDefault();
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
            ListControl editControl;

            editControl = new Rock.Web.UI.Controls.RockDropDownList { ID = id };
            editControl.Items.Add( new ListItem() );

            IEnumerable<WorkflowActivityType> activityTypes = null;

            Guid? workflowTypeGuid = configurationValues != null && configurationValues.ContainsKey( WORKFLOW_TYPE_KEY ) ? configurationValues[WORKFLOW_TYPE_KEY].Value.AsGuidOrNull() : null;

            WorkflowType workflowType = null;

            if ( workflowTypeGuid.HasValue )
            {
                var workflowTypeService = new WorkflowTypeService( new RockContext() );
                workflowType = workflowTypeService.Get( workflowTypeGuid.Value );
            }

            if ( workflowType == null )
            {
                workflowType = GetContextWorkflowType();
            }

            if ( workflowType != null )
            {
                activityTypes = workflowType.ActivityTypes;

                if ( activityTypes != null && activityTypes.Any() )
                {
                    foreach ( var activityType in activityTypes.OrderBy( a => a.Order ) )
                    {
                        editControl.Items.Add( new ListItem( activityType.Name ?? "[New Activity]", activityType.Guid.ToString().ToUpper() ) );
                    }
                }
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
            var picker = control as RockDropDownList;
            if ( picker != null )
            {
                return picker.SelectedValue;
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
            var picker = control as RockDropDownList;
            if ( picker != null )
            {
                picker.SetValue( value.AsGuidOrNull() );
            }
        }

        private WorkflowType GetContextWorkflowType()
        {
            var httpContext = System.Web.HttpContext.Current;
            if ( httpContext != null && httpContext.Items != null )
            {
                return httpContext.Items[WORKFLOW_TYPE_KEY] as WorkflowType;
            }

            return null;
        }

        #endregion

        #region Filter Control

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

        /// <summary>
        /// Determines whether this filter has a filter control
        /// </summary>
        /// <returns></returns>
        public override bool HasFilterControl()
        {
            return false;
        }

        #endregion

        #region IEntityFieldType
        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public int? GetEditValueAsEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var guid = GetEditValue( control, configurationValues ).AsGuid();
            var item = new WorkflowActivityTypeService( new RockContext() ).Get( guid );
            return item != null ? item.Id : ( int? ) null;
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            var item = new WorkflowActivityTypeService( new RockContext() ).Get( id ?? 0 );
            var guidValue = item != null ? item.Guid.ToString() : string.Empty;
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
            var guid = value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                rockContext = rockContext ?? new RockContext();
                return new WorkflowActivityTypeService( rockContext ).Get( guid.Value );
            }

            return null;
        }
        #endregion
    }
}