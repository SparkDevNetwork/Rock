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
using System.Linq;
#if WEBFORMS
using System.Web.UI;
using System.Web.UI.WebControls;

#endif
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// 
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.NOTE_TYPES )]
    public class NoteTypesFieldType : CategoryFieldType, IEntityReferenceFieldType
    {
        private const string REPEAT_COLUMNS = "repeatColumns";
        private const string VALUES_PUBLIC_KEY = "values";
        private const string ENTITY_TYPES = "entityTypes";

        #region Configuration

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string privateValue )
        {
            // Commenting out the below call to the super class as currently, Note Types Field Type is inheriting from Category Field Type. This was causing errors.
            // this may be changed in future if needed.
            // var publicConfigurationValues = base.GetPublicConfigurationValues( privateConfigurationValues, usage, privateValue );

            var publicConfigurationValues = new Dictionary<string, string>( privateConfigurationValues );
            if ( usage == ConfigurationValueUsage.Configure )
            {
                // Put all the entities under the to be displayed in EntityPicker Dropdown in the Attribute Configuration Modal.
                publicConfigurationValues[ENTITY_TYPES] = new EntityTypeService( new RockContext() )
                    .GetEntities()
                    .OrderBy( e => e.FriendlyName )
                    .ThenBy( e => e.Name )
                    .ToList()
                    .Select( e => e.ToListItemBag() )
                    .ToCamelCaseJson( false, true );
            }

            var entityTypeName = privateConfigurationValues.GetValueOrDefault( ENTITY_TYPE_NAME_KEY, None.IdValue );
            var entityType = EntityTypeCache.Get( entityTypeName );
            // The value of the Entity Type Name is set from either the Webforms or the Obsidian Attributes Configuration Modal.
            // The Entity Type Name to represent NO_ENTITY could be either "0" or an empty string based on which modal it is set from.
            // We are trying to cater to both as of now.
            if ( !string.IsNullOrWhiteSpace( entityType?.Name ) && entityTypeName != None.IdValue )
            {
                publicConfigurationValues[ENTITY_TYPE_NAME_KEY] = entityType.Guid.ToString();
            }

            using ( var rockContext = new RockContext() )
            {
                if ( !string.IsNullOrWhiteSpace( entityType?.Name ) && entityTypeName != None.IdValue )
                {
                    string qualifierColumn = publicConfigurationValues.GetValueOrDefault( QUALIFIER_COLUMN_KEY, string.Empty );
                    string qualifierValue = publicConfigurationValues.GetValueOrDefault( QUALIFIER_VALUE_KEY, string.Empty );

                    publicConfigurationValues[VALUES_PUBLIC_KEY] = new NoteTypeService( rockContext )
                        .Get( entityType.Id, qualifierColumn, qualifierValue )
                        .OrderBy( n => n.Name )
                        .Select( n => new ListItemBag
                        {
                            Text = n.Name,
                            Value = n.Guid.ToString().ToUpper()
                        } )
                        .ToList()
                        .ToCamelCaseJson( false, true );
                }
                // Show all the notes types if no entity is specified.
                else
                {
                    publicConfigurationValues[VALUES_PUBLIC_KEY] = new NoteTypeService( rockContext )
                        .Queryable()
                        .OrderBy( n => n.EntityType.Name )
                        .ThenBy( n => n.Name )
                        .Select( n => new
                        {
                            EntityTypeFriendlyName = n.EntityType.FriendlyName,
                            n.Name,
                            n.Guid
                        } ) // creating this anonymous object to prevent further calls database
                        .ToList() // getting the results to the memory so that the subsequent operations do not throw InvalidOperationException
                        .Select( n => new ListItemBag
                        {
                            Text = $"{n.EntityTypeFriendlyName}: {n.Name}",
                            Value = n.Guid.ToString().ToUpper()
                        } )
                        .ToCamelCaseJson( false, true );
                }
            }
            return publicConfigurationValues;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPrivateConfigurationValues( Dictionary<string, string> publicConfigurationValues )
        {
            var privateConfigurationValues = new Dictionary<string, string>( publicConfigurationValues );
            // Remove the values which should not be stored in the database.
            privateConfigurationValues.Remove( ENTITY_TYPES );
            privateConfigurationValues.Remove( VALUES_PUBLIC_KEY );

            privateConfigurationValues[ENTITY_TYPE_NAME_KEY] = string.Empty;
            var entityTypeGuid = publicConfigurationValues.GetValueOrNull( ENTITY_TYPE_NAME_KEY );
            if ( entityTypeGuid != null )
            {
                var entityType = EntityTypeCache.Get( entityTypeGuid.AsGuid() );

                if ( !string.IsNullOrWhiteSpace( entityType?.Name ) && entityType.Name != None.IdValue )
                {
                    privateConfigurationValues[ENTITY_TYPE_NAME_KEY] = entityType.Name;
                }
            }

            return privateConfigurationValues;
        }

        /// <inheritdoc/>
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return privateValue;
        }

        /// <inheritdoc/>
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            return publicValue;
        }

        #endregion Configuration

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            string formattedValue = string.Empty;

            if ( !string.IsNullOrWhiteSpace( privateValue ) )
            {
                var names = new List<string>();

                foreach ( string guidString in privateValue.SplitDelimitedValues() )
                {
                    Guid? guid = guidString.AsGuidOrNull();
                    if ( guid.HasValue )
                    {
                        var noteType = NoteTypeCache.Get( guid.Value );
                        if ( noteType != null )
                        {
                            names.Add( noteType.Name );
                        }
                    }
                }

                formattedValue = names.AsDelimited( ", " );
            }

            // The parent is CategoryFieldType. CategoryFieldType's FormatValue expects a list of category guids, so we should not call the base FormatValue.
            return formattedValue;
        }

        #endregion

        #region EditControl

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

        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var referencedEntities = new List<ReferencedEntity>();
            var noteTypeEntityTypeId = EntityTypeCache.GetId<Rock.Model.NoteType>().Value;

            foreach ( string guidString in privateValue.SplitDelimitedValues() )
            {
                Guid? guid = guidString.AsGuidOrNull();
                if ( guid.HasValue )
                {
                    var noteTypeId = NoteTypeCache.GetId( guid.Value );
                    if ( noteTypeId != null )
                    {
                        referencedEntities.Add( new ReferencedEntity( noteTypeEntityTypeId, noteTypeId.Value ) );
                    }
                }
            }

            return referencedEntities;
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the Name property of a NoteType and
            // should have its persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<NoteType>().Value, nameof( NoteType.Name ) )
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
            List<string> configKeys = new List<string>();
            configKeys.Add( REPEAT_COLUMNS );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            List<Control> controls = base.ConfigurationControls();

            var tbRepeatColumns = new NumberBox();
            tbRepeatColumns.Label = "Columns";
            tbRepeatColumns.Help = "Select how many columns the list should use before going to the next row. If blank or 0 then 4 columns will be displayed. There is no upper limit enforced here however the block this is used in might add contraints due to available space.";
            tbRepeatColumns.MinimumValue = "0";
            tbRepeatColumns.AutoPostBack = true;
            tbRepeatColumns.TextChanged += OnQualifierUpdated;
            controls.Add( tbRepeatColumns );

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = base.ConfigurationValues( controls );

            string description = "Select how many columns the list should use before going to the next row. If blank 4 is used.";
            configurationValues.Add( REPEAT_COLUMNS, new ConfigurationValue( "Repeat Columns", description, string.Empty ) );

            // NOTE: Indicies 0-2 come from the base class CategoryFieldType.
            if ( controls != null && controls.Count > 3 )
            {
                var tbRepeatColumns = controls[3] as NumberBox;
                configurationValues[REPEAT_COLUMNS].Value = tbRepeatColumns.Visible ? tbRepeatColumns.Text : string.Empty;
            }

            return configurationValues;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="configurationValues">The configuration values.</param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            base.SetConfigurationValues( controls, configurationValues );

            // NOTE: Indicies 0-2 come from the base class CategoryFieldType.
            if ( controls != null && controls.Count > 3 && configurationValues != null )
            {
                var tbRepeatColumns = controls[3] as NumberBox;
                tbRepeatColumns.Text = configurationValues.ContainsKey( REPEAT_COLUMNS ) ? configurationValues[REPEAT_COLUMNS].Value : string.Empty;
            }
        }

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns>System.String.</returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            return GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
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
            int entityTypeId = 0;
            string entityTypeName = string.Empty;
            string qualifierColumn = string.Empty;
            string qualifierValue = string.Empty;

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
                if ( configurationValues.ContainsKey( QUALIFIER_COLUMN_KEY ) )
                {
                    qualifierColumn = configurationValues[QUALIFIER_COLUMN_KEY].Value;
                }

                if ( configurationValues.ContainsKey( QUALIFIER_VALUE_KEY ) )
                {
                    qualifierValue = configurationValues[QUALIFIER_VALUE_KEY].Value;
                }
            }

            RockCheckBoxList editControl = new RockCheckBoxList { ID = id };
            editControl.RepeatDirection = RepeatDirection.Horizontal;

            if ( configurationValues.ContainsKey( REPEAT_COLUMNS ) )
            {
                ( ( RockCheckBoxList ) editControl ).RepeatColumns = configurationValues[REPEAT_COLUMNS].Value.AsInteger();
            }

            using ( var rockContext = new RockContext() )
            {
                if ( string.IsNullOrWhiteSpace( entityTypeName ) )
                {
                    foreach ( var noteType in new NoteTypeService( rockContext )
                        .Queryable()
                        .OrderBy( n => n.EntityType.Name )
                        .ThenBy( n => n.Name ) )
                    {
                        editControl.Items.Add( new ListItem( string.Format( "{0}: {1}", noteType.EntityType.FriendlyName, noteType.Name ), noteType.Guid.ToString().ToUpper() ) );
                    }
                }
                else
                {
                    foreach ( var noteType in new NoteTypeService( rockContext )
                        .Get( entityTypeId, qualifierColumn, qualifierValue )
                        .OrderBy( n => n.Name ) )
                    {
                        editControl.Items.Add( new ListItem( noteType.Name, noteType.Guid.ToString().ToUpper() ) );
                    }
                }
            }

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
            var picker = control as RockCheckBoxList;
            if ( picker != null )
            {
                return picker.SelectedValues.AsDelimited( "," );
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var picker = control as ListControl;
            if ( picker != null )
            {
                List<string> values = value?.Split( ',' ).ToList() ?? new List<string>();
                foreach ( ListItem li in picker.Items )
                {
                    li.Selected = values.Contains( li.Value, StringComparer.OrdinalIgnoreCase );
                }
            }
        }

#endif
        #endregion
    }
}
