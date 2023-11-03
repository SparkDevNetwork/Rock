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
using System.Web.UI.WebControls;
using System.Web.UI;
#endif

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.ViewModels.Utility;

namespace Rock.Field.Types
{
    /// <summary>
    /// Stored as a delimited list of DataView's Guids
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.DATAVIEWS )]
    public class DataViewsFieldType : FieldType, IEntityReferenceFieldType
    {
        #region Configuration

        /// <summary>
        /// Entity Type Name Key
        /// </summary>
        protected const string ENTITY_TYPE_NAME_KEY = "entityTypeName";

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( privateValue.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            using ( var rockContext = new RockContext() )
            {
                var guids = privateValue.SplitDelimitedValues().AsGuidList();

                var names = new DataViewService( rockContext )
                    .Queryable()
                    .Where( dv => guids.Contains( dv.Guid ) )
                    .Select( dv => dv.Name )
                    .ToList();

                return names.JoinStrings( ", " );
            }
        }

        #endregion

        #region Edit Control

        /// <inheritdoc/>
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetTextValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            var dataViewsValues = publicValue.FromJsonOrNull<List<ListItemBag>>();

            if ( dataViewsValues != null && dataViewsValues.Any() )
            {
                return string.Join( ",", dataViewsValues.Select( s => s.Value ) );
            }

            return string.Empty;
        }

        /// <inheritdoc/>
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( !string.IsNullOrWhiteSpace( privateValue ) )
            {
                var scheduleValues = new List<ListItemBag>();

                foreach ( string guidValue in privateValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
                {
                    Guid? guid = guidValue.AsGuidOrNull();
                    if ( guid.HasValue )
                    {
                        var schedule = NamedScheduleCache.Get( guid.Value );
                        if ( schedule != null )
                        {
                            var scheduleValue = new ListItemBag()
                            {
                                Text = schedule.Name,
                                Value = schedule.Guid.ToString(),
                            };

                            scheduleValues.Add( scheduleValue );
                        }
                    }
                }

                if ( scheduleValues.Any() )
                {
                    return scheduleValues.ToCamelCaseJson( false, true );
                }
            }

            return string.Empty;
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

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var guids = privateValue.SplitDelimitedValues().AsGuidList();

            if ( !guids.Any() )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var dataViewIds = new DataViewService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( d => guids.Contains( d.Guid ) )
                    .Select( d => d.Id )
                    .ToList();

                if ( !dataViewIds.Any() )
                {
                    return null;
                }

                var referencedEntities = new List<ReferencedEntity>();

                foreach ( var dataViewId in dataViewIds )
                {
                    referencedEntities.Add( new ReferencedEntity( EntityTypeCache.GetId<DataView>().Value, dataViewId ) );
                }

                return referencedEntities;
            }
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<DataView>().Value, nameof( DataView.Name ) )
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
                    int? entityTypeId = ( ( EntityTypePicker ) controls[0] ).SelectedValueAsInt();
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
                    ( ( EntityTypePicker ) controls[0] ).SetValue( entityType != null ? entityType.Id : ( int? ) null );
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

#endif
        #endregion
    }
}
