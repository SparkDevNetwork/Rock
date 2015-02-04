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
using Rock.Reporting;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a dropdown list of Defined Values for a specific Defined Type
    /// </summary>
    public class AttributeFieldType : FieldType
    {

        #region Configuration

        private const string ENTITY_TYPE_KEY = "entitytype";
        private const string ALLOW_MULTIPLE_KEY = "allowmultiple";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( ENTITY_TYPE_KEY );
            configKeys.Add( ALLOW_MULTIPLE_KEY );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            // build a drop down list of enity types (the one that gets selected is
            // used to build a list of attributes) 
            var etp = new EntityTypePicker();
            controls.Add( etp );
            etp.AutoPostBack = true;
            etp.SelectedIndexChanged += OnQualifierUpdated;
            etp.Label = "Entity Type";
            etp.Help = "The Entity Type to select attributes for.";

            var entityTypeList = new Model.EntityTypeService( new RockContext() ).GetEntities().ToList();
            etp.EntityTypes = entityTypeList;

            // Add checkbox for deciding if the defined values list is renedered as a drop
            // down list or a checkbox list.
            var cb = new RockCheckBox();
            controls.Add( cb );
            cb.AutoPostBack = true;
            cb.CheckedChanged += OnQualifierUpdated;
            cb.Label = "Allow Multiple Values";
            cb.Text = "Yes";
            cb.Help = "When set, allows multiple attributes to be selected.";
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
            configurationValues.Add( ENTITY_TYPE_KEY, new ConfigurationValue( "Entity Type", "The Entity Type to select attributes for.", "" ) );
            configurationValues.Add( ALLOW_MULTIPLE_KEY, new ConfigurationValue( "Allow Multiple Values", "When set, allows multiple attributes to be selected.", "" ) );

            if ( controls != null && controls.Count == 2 )
            {
                if ( controls[0] != null && controls[0] is EntityTypePicker )
                {
                    string value = string.Empty;
                    int? entityTypeId = ( (EntityTypePicker)controls[0] ).SelectedValue.AsIntegerOrNull();
                    if ( entityTypeId.HasValue )
                    {
                        var entityType = EntityTypeCache.Read( entityTypeId.Value );
                        if ( entityType != null )
                        {
                            value = entityType.Guid.ToString();
                        }
                    }
                    configurationValues[ENTITY_TYPE_KEY].Value = value;
                }

                if ( controls[1] != null && controls[1] is CheckBox )
                {
                    configurationValues[ALLOW_MULTIPLE_KEY].Value = ( (CheckBox)controls[1] ).Checked.ToString();
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
            if ( controls != null && controls.Count == 2 && configurationValues != null )
            {
                if ( controls[0] != null && controls[0] is EntityTypePicker && configurationValues.ContainsKey( ENTITY_TYPE_KEY ) )
                {
                    string value = string.Empty;
                    Guid? entityTypeGuid = configurationValues[ENTITY_TYPE_KEY].Value.AsGuidOrNull();
                    if ( entityTypeGuid.HasValue )
                    {
                        var entityType = EntityTypeCache.Read( entityTypeGuid.Value );
                        if ( entityType != null )
                        {
                            value = entityType.Id.ToString();
                        }
                    }
                    ( (EntityTypePicker)controls[0] ).SelectedValue = value;
                }

                if ( controls[1] != null && controls[1] is CheckBox && configurationValues.ContainsKey( ALLOW_MULTIPLE_KEY ) )
                {
                    ( (CheckBox)controls[1] ).Checked = configurationValues[ALLOW_MULTIPLE_KEY].Value.AsBoolean();
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
                var names = new List<string>();
                foreach ( Guid guid in value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList() )
                {
                    var attribute = AttributeCache.Read( guid );
                    if ( attribute != null )
                    {
                        names.Add( attribute.Name );
                    }
                }

                formattedValue = names.AsDelimited( ", " );
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

            if ( configurationValues != null && configurationValues.ContainsKey( ALLOW_MULTIPLE_KEY ) && configurationValues[ALLOW_MULTIPLE_KEY].Value.AsBoolean() )
            {
                editControl = new RockCheckBoxList { ID = id };
                editControl.AddCssClass( "checkboxlist-group" );
            }
            else
            {
                editControl = new RockDropDownList { ID = id };
                editControl.Items.Add( new ListItem() );
            }

            if ( configurationValues != null && configurationValues.ContainsKey( ENTITY_TYPE_KEY ) )
            {
                Guid? entityTypeGuid = configurationValues[ENTITY_TYPE_KEY].Value.AsGuidOrNull();
                if ( entityTypeGuid.HasValue )
                {
                    var entityType = EntityTypeCache.Read( entityTypeGuid.Value );
                    if ( entityType != null )
                    {
                        Rock.Model.AttributeService attributeService = new Model.AttributeService( new RockContext() );
                        var attributes = attributeService.GetByEntityTypeId( entityType.Id );
                        if ( attributes.Any() )
                        {
                            foreach ( var attribute in attributes.OrderBy( a => a.Name ) )
                            {
                                editControl.Items.Add( new ListItem( attribute.Name, attribute.Id.ToString() ) );
                            }
                        }
                        return editControl;
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

            if ( control != null && control is ListControl )
            {
                if ( control is Rock.Web.UI.Controls.RockDropDownList )
                {
                    ids.Add( ( (ListControl)control ).SelectedValue );
                }
                else if ( control is Rock.Web.UI.Controls.RockCheckBoxList )
                {
                    var cblControl = control as Rock.Web.UI.Controls.RockCheckBoxList;

                    ids.AddRange( cblControl.Items.Cast<ListItem>()
                        .Where( i => i.Selected )
                        .Select( i => i.Value ) );
                }
            }

            var guids = new List<string>();

            foreach ( int attributeId in ids.AsIntegerList() )
            {
                var attribute = Rock.Web.Cache.AttributeCache.Read( attributeId );
                if ( attribute != null )
                {
                    guids.Add( attribute.Guid.ToString() );
                }
            }

            return guids.AsDelimited( "," );
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
                if ( control != null && control is ListControl )
                {
                    var ids = new List<string>();
                    foreach ( Guid guid in value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList() )
                    {
                        var attribute = Rock.Web.Cache.AttributeCache.Read( guid );
                        if ( attribute != null )
                        {
                            ids.Add( attribute.Id.ToString() );
                        }
                    }

                    var listControl = control as ListControl;

                    if ( ids.Any() )
                    {
                        foreach ( ListItem li in listControl.Items )
                        {
                            li.Selected = ids.Contains( li.Value );
                        }
                    }
                    else
                    {
                        if ( listControl.Items.Count > 0 )
                        {
                            listControl.SelectedIndex = 0;
                        }
                    }
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
        /// <returns></returns>
        public override Control FilterValueControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required )
        {
            var overrideConfigValues = new Dictionary<string, ConfigurationValue>();
            foreach ( var keyVal in configurationValues )
            {
                overrideConfigValues.Add( keyVal.Key, keyVal.Value );
            }
            overrideConfigValues.AddOrReplace( ALLOW_MULTIPLE_KEY, new ConfigurationValue( "false" ) );
            
            return  base.FilterValueControl( overrideConfigValues, id, required );
        }

        #endregion

    }
}