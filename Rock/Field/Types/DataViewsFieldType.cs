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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Stored as a delimited list of DataView's Guids
    /// </summary>
    public class DataViewsFieldType : FieldType
    {
        #region Configuration

        /// <summary>
        /// Entity Type Name Key
        /// </summary>
        protected const string ENTITY_TYPE_NAME_KEY = "entityTypeName";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( ENTITY_TYPE_NAME_KEY );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            List<Control> controls = new List<Control>();

            var etp = new EntityTypePicker();
            controls.Add( etp );
            etp.EntityTypes = new EntityTypeService( new RockContext() )
                .GetEntities()
                .OrderBy( t => t.FriendlyName )
                .ToList();
            etp.AutoPostBack = true;
            etp.SelectedIndexChanged += OnQualifierUpdated;
            etp.Label = "Entity Type";
            etp.Help = "The type of entity to display dataviews for.";

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
            configurationValues.Add( ENTITY_TYPE_NAME_KEY, new ConfigurationValue( "Entity Type", "The type of entity to display dataviews for", "" ) );

            if ( controls != null && controls.Count == 1 )
            {
                if ( controls[0] != null && controls[0] is EntityTypePicker )
                {
                    int? entityTypeId = ( (EntityTypePicker)controls[0] ).SelectedValueAsInt();
                    if ( entityTypeId.HasValue )
                    {
                        var entityType = EntityTypeCache.Get( entityTypeId.Value );
                        configurationValues[ENTITY_TYPE_NAME_KEY].Value = entityType != null ? entityType.Name : string.Empty;
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
            if ( controls != null && controls.Count == 1 && configurationValues != null )
            {
                if ( controls[0] != null && controls[0] is EntityTypePicker && configurationValues.ContainsKey( ENTITY_TYPE_NAME_KEY ) )
                {
                    var entityType = EntityTypeCache.Get( configurationValues[ENTITY_TYPE_NAME_KEY].Value );
                    ( (EntityTypePicker)controls[0] ).SetValue( entityType != null ? entityType.Id : (int?)null );
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
                using ( var rockContext = new RockContext() )
                {
                    var guids = value.SplitDelimitedValues();
                    var dataviews = new DataViewService( rockContext ).Queryable().AsNoTracking().Where( a => guids.Contains( a.Guid.ToString() ) );
                    if ( dataviews.Any() )
                    {
                        formattedValue = string.Join( ", ", ( from dataview in dataviews select dataview.Name ).ToArray() );
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
            string entityTypeName = string.Empty;
            int entityTypeId = 0;

            if ( configurationValues != null )
            {
                if ( configurationValues.ContainsKey( ENTITY_TYPE_NAME_KEY ) )
                {
                    entityTypeName = configurationValues[ENTITY_TYPE_NAME_KEY].Value;
                    if ( !string.IsNullOrWhiteSpace( entityTypeName ) && entityTypeName != None.IdValue )
                    {
                        var entityType = EntityTypeCache.Get( entityTypeName );
                        if ( entityType != null )
                        {
                            entityTypeId = entityType.Id;
                        }
                    }
                }
            }

            var editControl = new DataViewsPicker { ID = id, EntityTypeId = entityTypeId };

            return editControl;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as DataViewsPicker;
            string result = null;

            var selectedValues = new List<int>();
            if ( picker != null )
            {
                foreach ( System.Web.UI.WebControls.ListItem li in picker.Items )
                {
                    if ( li.Selected )
                    {
                        selectedValues.Add( li.Value.AsInteger() );
                    }
                }

                var guids = new List<Guid>();
                using ( var rockContext = new RockContext() )
                {
                    var dataViews = new DataViewService( rockContext ).Queryable().AsNoTracking().Where( a => selectedValues.Contains( a.Id ) );

                    if ( dataViews.Any() )
                    {
                        guids = dataViews.Select( a => a.Guid ).ToList();
                    }
                }

                result = string.Join( ",", guids );
            }

            return result;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( value != null )
            {
                var picker = control as DataViewsPicker;
                var guids = new List<Guid>();

                if ( picker != null )
                {
                    guids = value.SplitDelimitedValues().AsGuidList();

                    var dataViews = new DataViewService( new RockContext() ).Queryable().Where( a => guids.Contains( a.Guid ) ).Select( a => a.Id );
                    foreach ( System.Web.UI.WebControls.ListItem li in picker.Items )
                    {
                        li.Selected = dataViews.Contains( li.Value.AsInteger() );
                    }
                }
            }
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Formats the filter value value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override string FormatFilterValueValue( Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            string formattedValue = string.Empty;

            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                using ( var rockContext = new RockContext() )
                {
                    var guids = value.SplitDelimitedValues();
                    var dataviews = new DataViewService( rockContext ).Queryable().AsNoTracking().Where( a => guids.Contains( a.Guid.ToString() ) );
                    if ( dataviews.Any() )
                    {
                        formattedValue = string.Join( "' AND '", ( from dataview in dataviews select dataview.Name ).ToArray() );
                    }
                }
            }

            return AddQuotes( formattedValue );
        }

        #endregion
    }
}
