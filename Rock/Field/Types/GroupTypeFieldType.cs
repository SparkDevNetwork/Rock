// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a single (or null) GroupType
    /// </summary>
    public class GroupTypeFieldType : FieldType, IEntityFieldType
    {

        #region Configuration

        private const string GROUP_TYPE_PURPOSE_VALUE_GUID = "groupTypePurposeValueGuid";
        
        /// <summary>
        /// Configurations the keys.
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            List<string> configKeys = new List<string>();
            configKeys.Add( GROUP_TYPE_PURPOSE_VALUE_GUID );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            var ddl = new RockDropDownList();
            controls.Add( ddl );
            ddl.AutoPostBack = true;
            ddl.SelectedIndexChanged += OnQualifierUpdated;

            var definedType = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.GROUPTYPE_PURPOSE.AsGuid() );
            ddl.BindToDefinedType( definedType, true );
            ddl.Label = "Purpose";
            ddl.Help = "An optional setting to limit the selection of group types to those that have the selected purpose.";

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
            configurationValues.Add( GROUP_TYPE_PURPOSE_VALUE_GUID, new ConfigurationValue( "Purpose", "An optional setting to limit the selection of group types to those that have the selected purpose.", string.Empty ) );

            if ( controls != null && controls.Count == 1 &&
                controls[0] != null && controls[0] is DropDownList )
            {
                int? definedValueId = ( (DropDownList)controls[0] ).SelectedValueAsInt();
                if ( definedValueId.HasValue )
                {
                    var definedValue = DefinedValueCache.Read( definedValueId.Value );
                    if ( definedValue != null )
                    {
                        configurationValues[GROUP_TYPE_PURPOSE_VALUE_GUID].Value = definedValue.Guid.ToString();
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
            if ( controls != null && controls.Count == 1 && configurationValues != null &&
                controls[0] != null && controls[0] is DropDownList && configurationValues.ContainsKey( GROUP_TYPE_PURPOSE_VALUE_GUID ) )
            {
                Guid? definedValueGuid = configurationValues[GROUP_TYPE_PURPOSE_VALUE_GUID].Value.AsGuidOrNull();
                if ( definedValueGuid.HasValue )
                {
                    var definedValue = DefinedValueCache.Read( definedValueGuid.Value );
                    if ( definedValue != null )
                    {
                        ( (DropDownList)controls[0] ).SetValue( definedValue.Id.ToString() );
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
            string formattedValue = string.Empty;

            Guid guid = Guid.Empty;
            if ( Guid.TryParse( value, out guid ) )
            {
                var groupType = GroupTypeCache.Read( guid );
                if ( groupType != null )
                {
                    formattedValue = groupType.Name;
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
            var editControl = new GroupTypePicker { ID = id };
            var qryGroupTypes = new GroupTypeService( new RockContext() ).Queryable();
            
            if ( configurationValues.ContainsKey( GROUP_TYPE_PURPOSE_VALUE_GUID ) )
            {
                var groupTypePurposeValueGuid = ( configurationValues[GROUP_TYPE_PURPOSE_VALUE_GUID] ).Value.AsGuidOrNull();
                if ( groupTypePurposeValueGuid.HasValue )
                {
                    qryGroupTypes = qryGroupTypes.Where( a => a.GroupTypePurposeValue.Guid == groupTypePurposeValueGuid.Value );
                }
            }

            editControl.GroupTypes = qryGroupTypes.OrderBy( a => a.Name ).ToList();
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
            List<string> values = new List<string>();

            GroupTypePicker groupTypePicker = control as GroupTypePicker;
            if ( groupTypePicker != null )
            {
                if ( groupTypePicker.SelectedGroupTypeId.HasValue )
                {
                    var groupType = GroupTypeCache.Read( groupTypePicker.SelectedGroupTypeId.Value );
                    if (groupType != null)
                    {
                        return groupType.Guid.ToString();
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
            GroupTypePicker groupTypePicker = control as GroupTypePicker;
            if (groupTypePicker != null && !string.IsNullOrWhiteSpace(value))
            {
                Guid groupTypeGuid = Guid.Empty;
                if (Guid.TryParse(value, out groupTypeGuid))
                {
                    groupTypePicker.SelectedGroupTypeId = new GroupTypeService( new RockContext() ).Queryable()
                        .Where( g => g.Guid.Equals(groupTypeGuid))
                        .Select( g => g.Id )
                        .FirstOrDefault();
                }
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
            var item = new GroupTypeService( new RockContext() ).Get( guid );
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
            var item = new GroupTypeService( new RockContext() ).Get( id ?? 0 );
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
                return new GroupTypeService( rockContext ).Get( guid.Value );
            }

            return null;
        }

        #endregion

    }
}