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
    /// Field Type used to display a dropdown list of connection opportunities
    /// Stored as ConnectionOpportunity.Guid
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    public class ConnectionOpportunityFieldType : FieldType, IEntityFieldType
    {
        #region Configuration

        private const string INCLUDE_INACTIVE_KEY = "includeInactive";
        private const string CONNECTION_TYPE_FILTER = "connectionTypeFilter";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( INCLUDE_INACTIVE_KEY );
            configKeys.Add( CONNECTION_TYPE_FILTER );

            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            // Add checkbox for deciding if the list should include inactive items
            var cbIncludeInactive = new RockCheckBox();
            controls.Add( cbIncludeInactive );
            cbIncludeInactive.AutoPostBack = true;
            cbIncludeInactive.CheckedChanged += OnQualifierUpdated;
            cbIncludeInactive.Label = "Include Inactive";
            cbIncludeInactive.Text = "Yes";
            cbIncludeInactive.Help = "When set, inactive connection opportunities will be included in the list.";

            // Add ConnectionType Filter ddl
            var ddlConnectionTypeFilter = new RockDropDownList();
            controls.Add( ddlConnectionTypeFilter );
            ddlConnectionTypeFilter.Label = "Connection Type";
            ddlConnectionTypeFilter.Help = "Select a Connection Type to limit selection to a specific connection type.  Leave blank to allow selection of connections from any connection type";
            ddlConnectionTypeFilter.SelectedIndexChanged += OnQualifierUpdated;
            ddlConnectionTypeFilter.AutoPostBack = true;

            var connectionTypeService = new ConnectionTypeService( new RockContext() );
            ddlConnectionTypeFilter.Items.Add( new ListItem() );
            ddlConnectionTypeFilter.Items.AddRange( connectionTypeService.Queryable().Select( x => new ListItem { Text = x.Name, Value = x.Id.ToString() } ).ToArray() );

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
            configurationValues.Add( INCLUDE_INACTIVE_KEY, new ConfigurationValue( "Include Inactive", "When set, inactive connection opportunities will be included in the list.", string.Empty ) );
            configurationValues.Add( CONNECTION_TYPE_FILTER, new ConfigurationValue( "Connection Type", "Select a Connection Type to limit selection to a specific connection type.  Leave blank to allow selection of connections from any connection type", string.Empty ) );

            if ( controls != null && controls.Count() == 2)
            {
                if ( controls[0] != null && controls[0] is CheckBox )
                {
                    configurationValues[INCLUDE_INACTIVE_KEY].Value = ( (CheckBox)controls[0] ).Checked.ToString();
                }

                if ( controls[1] != null && controls[1] is DropDownList )
                {
                    configurationValues[CONNECTION_TYPE_FILTER].Value = ( ( DropDownList ) controls[1] ).SelectedValue;
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
            if ( controls != null && controls.Count() == 2 && configurationValues != null )
            {
                if ( controls[0] != null && controls[0] is CheckBox && configurationValues.ContainsKey( INCLUDE_INACTIVE_KEY ) )
                {
                    ( (CheckBox)controls[0] ).Checked = configurationValues[INCLUDE_INACTIVE_KEY].Value.AsBoolean();
                }

                if ( controls[1] != null && controls[1] is DropDownList && configurationValues.ContainsKey( CONNECTION_TYPE_FILTER ) )
                {
                    ( ( DropDownList ) controls[1] ).SelectedValue = configurationValues[CONNECTION_TYPE_FILTER].Value;
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

            Guid? guid = value.AsGuidOrNull();
            if (guid.HasValue)
            {
                using ( var rockContext = new RockContext() )
                {
                    var opportunity = new ConnectionOpportunityService( rockContext ).GetNoTracking( guid.Value );
                    if ( opportunity != null )
                    {
                        formattedValue = opportunity.Name;
                    }
                }
            }

            return base.FormatValue( parentControl, formattedValue, configurationValues, condensed );
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
            bool includeInactive = false;
            int? groupTypeFilterId = null;
            
            if ( configurationValues != null)
            {
                includeInactive = configurationValues.ContainsKey( INCLUDE_INACTIVE_KEY ) && configurationValues[INCLUDE_INACTIVE_KEY].Value.AsBoolean();
                groupTypeFilterId = configurationValues.ContainsKey( CONNECTION_TYPE_FILTER ) ? configurationValues[CONNECTION_TYPE_FILTER].Value.AsIntegerOrNull() : null;
            }

            var opportunities = new ConnectionOpportunityService( new RockContext() )
                .Queryable().AsNoTracking()
                .Where( o => o.IsActive || includeInactive )
                .OrderBy( o => o.ConnectionType.Name )
                .ThenBy( o => o.Name )
                .Select( o => new { o.Guid, o.Name, o.ConnectionType } )
                .ToList();

            var editControl = new RockDropDownList { ID = id };
            editControl.Items.Add( new ListItem() );

            if ( opportunities.Any() )
            {
                foreach ( var opportunity in opportunities )
                {
                    if ( groupTypeFilterId != null && opportunity.ConnectionType.Id != groupTypeFilterId )
                    {
                        continue;
                    }

                    var listItem = new ListItem( opportunity.Name, opportunity.Guid.ToString().ToUpper() );

                    // Don't add an option group if there is a filter since that would be only one group.
                    if ( groupTypeFilterId == null )
                    {
                        listItem.Attributes.Add( "OptionGroup", opportunity.ConnectionType.Name );
                    }

                    editControl.Items.Add( listItem );
                }

                return editControl;
            }

            return null;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as DropDownList;
            if (picker != null)
            {
                // picker has value as ConnectionOpportunity.Guid
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
            var editControl = control as ListControl;
            if ( editControl != null )
            {
                editControl.SetValue( value );
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
        public int? GetEditValueAsEntityId( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            Guid guid = GetEditValue( control, configurationValues ).AsGuid();
            var item = new ConnectionOpportunityService( new RockContext() ).Get( guid );
            return item != null ? item.Id : (int?)null;
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            var item = new ConnectionOpportunityService( new RockContext() ).Get( id ?? 0 );
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
                return new ConnectionOpportunityService( rockContext ).Get( guid.Value );
            }

            return null;
        }

        #endregion
    }
}