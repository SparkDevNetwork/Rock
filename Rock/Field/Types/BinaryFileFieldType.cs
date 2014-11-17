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
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a dropdown list of binary files of a specific type
    /// Stored as BinaryFile's Guid
    /// </summary>
    public class BinaryFileFieldType : FieldType, IEntityFieldType
    {
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

            Guid? guid = value.AsGuid();
            if (guid.HasValue)
            {
                var result = new BinaryFileService( new RockContext() )
                    .Queryable()
                    .Where( f => f.Guid == guid.Value )
                    .Select( f => new { f.Id, f.FileName } )
                    .FirstOrDefault();

                if ( result != null )
                {
                    formattedValue = result.FileName;
                }
            }

            return base.FormatValue( parentControl, formattedValue, null, condensed );
        }

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( "binaryFileType" );
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
            ddl.Items.Clear();
            ddl.Items.Add( new ListItem( string.Empty, string.Empty ) );
            foreach ( var ft in new BinaryFileTypeService( new RockContext() )
                .Queryable()
                .OrderBy( f => f.Name )
                .Select( f => new { f.Guid, f.Name } ) )
            {
                ddl.Items.Add( new ListItem( ft.Name, ft.Guid.ToString().ToLower()));
            }
            ddl.Label = "File Type";
            ddl.Help = "The type of files to list.";

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
            configurationValues.Add( "binaryFileType", new ConfigurationValue( "File Type", "The type of files to list", string.Empty ) );

            if ( controls != null && controls.Count == 1 &&
                controls[0] != null && controls[0] is DropDownList )
            {
                configurationValues["binaryFileType"].Value = ( (DropDownList)controls[0] ).SelectedValue;
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
                controls[0] != null && controls[0] is DropDownList && configurationValues.ContainsKey( "binaryFileType" ) )
            {
                ( (DropDownList)controls[0] ).SetValue( configurationValues["binaryFileType"].Value.ToLower() );
            }
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
            var control = new BinaryFilePicker { ID = id }; 

            if ( configurationValues != null && configurationValues.ContainsKey( "binaryFileType" ) )
            {
                control.BinaryFileTypeGuid = configurationValues["binaryFileType"].Value.AsGuidOrNull();
            }

            return control;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// returns BinaryFile.Guid
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as BinaryFilePicker;
            
            if ( picker != null )
            {
                int? id = picker.SelectedValue.AsIntegerOrNull();
                if (id.HasValue)
                {
                    var binaryFile = new BinaryFileService( new RockContext() ).Get( id.Value );
                    if (binaryFile != null)
                    {
                        return binaryFile.Guid.ToString();
                    }
                }
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
            var picker = control as BinaryFilePicker;
            
            if ( picker != null )
            {
                Guid guid = value.AsGuid();

                // get the item (or null) and set it
                var binaryFile = new BinaryFileService( new RockContext() ).Get( guid );
                picker.SetValue( binaryFile );
            }
        }

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public int? GetEditValueAsEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            Guid guid = GetEditValue( control, configurationValues ).AsGuid();
            int? itemId = new BinaryFileService( new RockContext() ).Queryable().Where( a => a.Guid == guid ).Select( a => a.Id ).FirstOrDefault();
            return itemId != null ? itemId : (int?)null;
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void SetEditValueFromEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            Guid? itemGuid = null;
            if (id.HasValue)
            {
                itemGuid = new BinaryFileService( new RockContext() ).Queryable().Where( a => a.Id == id.Value ).Select( a => a.Guid ).FirstOrDefault();
            }

            string guidValue = itemGuid.HasValue ? itemGuid.ToString() : string.Empty;
            SetEditValue( control, configurationValues, guidValue );
        }
    }
}