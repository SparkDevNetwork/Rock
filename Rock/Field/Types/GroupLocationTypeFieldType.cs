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
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a dropdown list of Defined Values for a specific Defined Type
    /// </summary>
    [Serializable]
    public class GroupLocationTypeFieldType : FieldType
    {

        #region Configuration

        private const string GROUP_TYPE_KEY = "groupTypeGuid";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( GROUP_TYPE_KEY );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            // build a drop down list of group types (the one that gets selected is
            // used to build a list of group location type defined values that the
            // group type allows) 
            var ddl = new RockDropDownList();
            controls.Add( ddl );
            ddl.AutoPostBack = true;
            ddl.SelectedIndexChanged += OnQualifierUpdated;
            ddl.Label = "Group Type";
            ddl.Help = "The Group Type to select location types from.";

            Rock.Model.GroupTypeService groupTypeService = new Model.GroupTypeService( new RockContext() );
            foreach ( var groupType in groupTypeService.Queryable().OrderBy( g => g.Name ) )
            {
                ddl.Items.Add( new ListItem( groupType.Name, groupType.Guid.ToString() ) );
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
            configurationValues.Add( GROUP_TYPE_KEY, new ConfigurationValue( "Group Type", "The Group Type to select location types from.", "" ) );

            if ( controls != null && controls.Count == 1 )
            {
                if ( controls[0] != null && controls[0] is DropDownList )
                {
                    configurationValues[GROUP_TYPE_KEY].Value = ( (DropDownList)controls[0] ).SelectedValue;
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
                if ( controls[0] != null && controls[0] is DropDownList && configurationValues.ContainsKey( GROUP_TYPE_KEY ) )
                {
                    ( (DropDownList)controls[0] ).SelectedValue = configurationValues[GROUP_TYPE_KEY].Value;
                }
            }
        }

        #endregion

        #region Formatting

        /// <summary>
        /// Returns the field's current value
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
                Guid guid = Guid.Empty;
                if ( Guid.TryParse( value, out guid ) )
                {
                    var definedValue = Rock.Web.Cache.DefinedValueCache.Read( guid );
                    if ( definedValue != null )
                    {
                        formattedValue = definedValue.Value;
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

            if ( configurationValues != null && configurationValues.ContainsKey( GROUP_TYPE_KEY ) )
            {
                Guid groupTypeGuid = Guid.Empty;
                if ( Guid.TryParse( configurationValues[GROUP_TYPE_KEY].Value, out groupTypeGuid ) )
                {
                    var groupType = Rock.Web.Cache.GroupTypeCache.Read( groupTypeGuid );
                    if (groupType != null)
                    {
                        var locationTypeValues = groupType.LocationTypeValues;
                        if ( locationTypeValues != null )
                        {
                            if ( locationTypeValues.Any() )
                            {
                                foreach ( var locationTypeValue in locationTypeValues )
                                {
                                    editControl.Items.Add( new ListItem( locationTypeValue.Value, locationTypeValue.Id.ToString() ) );
                                }

                                return editControl;
                            }
                        }
                    }
                }
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
            var ids = new List<string>();

            if ( control != null && control is RockDropDownList )
            {
                string id  = ( (ListControl)control ).SelectedValue;

                int definedValueId = int.MinValue;
                if ( int.TryParse( id, out definedValueId ) )
                {
                    var definedValue = Rock.Web.Cache.DefinedValueCache.Read( definedValueId );
                    if ( definedValue != null )
                    {
                        return definedValue.Guid.ToString();
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues"></param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( value != null )
            {
                if ( control != null && control is RockDropDownList )
                {
                    string id = string.Empty;

                    Guid guid = Guid.Empty;
                    if ( Guid.TryParse( value, out guid ) )
                    {
                        var definedValue = Rock.Web.Cache.DefinedValueCache.Read( guid );
                        if ( definedValue != null )
                        {
                            id = definedValue.Id.ToString();
                        }
                    }

                    var listControl = control as RockDropDownList;
                    foreach ( ListItem li in listControl.Items )
                    {
                        li.Selected = id == li.Value;
                    }
                }
            }
        }

        #endregion

    }
}