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
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.ViewModels.Utility;

namespace Rock.Field.Types
{
    /// <summary>
    /// Stored as Category.Guid
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian)]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.CATEGORY )]
    public class CategoryFieldType : FieldType, IEntityFieldType, IEntityReferenceFieldType
    {
        #region Configuration

        /// <summary>
        /// Entity Type Name Key
        /// </summary>
        protected const string ENTITY_TYPE_NAME_KEY = "entityTypeName";

        /// <summary>
        /// Qualifier Column Key
        /// </summary>
        protected const string QUALIFIER_COLUMN_KEY = "qualifierColumn";

        /// <summary>
        /// Qualifier Value Key
        /// </summary>
        protected const string QUALIFIER_VALUE_KEY = "qualifierValue";

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( this is CategoriesFieldType )
            {
                return privateValue;
            }
            else
            {
                if ( !string.IsNullOrWhiteSpace( privateValue ) )
                {
                    var category = CategoryCache.Get( privateValue.AsGuid() );
                    if ( category != null )
                    {
                        return category.Name;
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Returns the value using the most appropriate datatype
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override object ValueAsFieldType( string value, Dictionary<string, ConfigurationValue> configurationValues )
        {
            return value.AsGuidOrNull();
        }

        #endregion

        #region Edit Control

        /// <inheritdoc/>
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( Guid.TryParse( privateValue, out Guid categoryGuid ) )
            {
                var category = CategoryCache.Get( categoryGuid );

                if ( category != null )
                {
                    return category.Name;
                }
            }

            return string.Empty;
        }

        /// <inheritdoc/>
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( string.IsNullOrWhiteSpace( publicValue ) )
            {
                return string.Empty;
            }

            var categoryValue = publicValue.FromJsonOrNull<ListItemBag>();

            if ( categoryValue != null )
            {
                return categoryValue.Value;
            }

            return string.Empty;
        }

        /// <inheritdoc/>
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( Guid.TryParse( privateValue, out Guid categoryGuid ) )
            {
                var category = CategoryCache.Get( categoryGuid );

                if ( category != null )
                {
                    return new ListItemBag()
                    {
                        Text = category.Name,
                        Value = category.Guid.ToString()
                    }.ToCamelCaseJson( false, true );
                }
            }

            return string.Empty;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPrivateConfigurationValues( Dictionary<string, string> publicConfigurationValues )
        {
            var privateConfigurationValues = base.GetPrivateConfigurationValues( publicConfigurationValues );

            if ( publicConfigurationValues.ContainsKey( ENTITY_TYPE_NAME_KEY ) )
            {
                var entityTypeNameValue = privateConfigurationValues[ENTITY_TYPE_NAME_KEY].FromJsonOrNull<ListItemBag>();
                if ( entityTypeNameValue != null )
                {
                    var entityType = EntityTypeCache.Get( entityTypeNameValue.Value.AsGuid() );

                    if ( entityType != null )
                    {
                        privateConfigurationValues[ENTITY_TYPE_NAME_KEY] = entityType.Name;
                    }
                }
            }

            return privateConfigurationValues;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string value )
        {
            var publicConfigurationValues = base.GetPublicConfigurationValues( privateConfigurationValues, usage, value );

            if ( publicConfigurationValues.ContainsKey( ENTITY_TYPE_NAME_KEY ) )
            {
                var entityTypeName = publicConfigurationValues[ENTITY_TYPE_NAME_KEY];
                var entityType = EntityTypeCache.Get( entityTypeName );
                if ( entityType != null )
                {
                    publicConfigurationValues[ENTITY_TYPE_NAME_KEY] = new ListItemBag()
                    {
                        Text = entityType.FriendlyName,
                        Value = entityType.Guid.ToString()
                    }.ToCamelCaseJson( false, true );
                }
            }

            return publicConfigurationValues;
        }

        #endregion

        #region Entity Methods

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
                return new CategoryService( rockContext ).Get( guid.Value );
            }

            return null;
        }

        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( this is CategoriesFieldType )
            {
                return null;
            }

            if ( !string.IsNullOrWhiteSpace( privateValue ) )
            {
                var category = CategoryCache.Get( privateValue.AsGuid() );
                if ( category != null )
                {
                    return new List<ReferencedEntity>() { new ReferencedEntity( EntityTypeCache.GetId<Category>().Value, category.Id ) };
                }
            }

            return null;
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<Category>().Value, nameof( Category.Name ) )
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
            configKeys.Add( ENTITY_TYPE_NAME_KEY );
            configKeys.Add( QUALIFIER_COLUMN_KEY );
            configKeys.Add( QUALIFIER_VALUE_KEY );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            List<Control> controls = new List<Control>();

            var ddl = new RockDropDownList();
            controls.Add( ddl );
            ddl.Items.Add( new ListItem( None.Text, None.IdValue ) );
            foreach ( var entityType in new EntityTypeService( new RockContext() ).GetEntities().OrderBy( e => e.FriendlyName ).ThenBy( e => e.Name ) )
            {
                ddl.Items.Add( new ListItem( entityType.FriendlyName, entityType.Name ) );
            }
            ddl.AutoPostBack = true;
            ddl.SelectedIndexChanged += OnQualifierUpdated;
            ddl.Label = "Entity Type";
            ddl.Help = "The type of entity to display categories for.";

            var tbColumn = new RockTextBox();
            controls.Add( tbColumn );
            tbColumn.AutoPostBack = true;
            tbColumn.TextChanged += OnQualifierUpdated;
            tbColumn.Label = "Qualifier Column";
            tbColumn.Help = "Entity column qualifier.";

            var tbValue = new RockTextBox();
            controls.Add( tbValue );
            tbValue.AutoPostBack = true;
            tbValue.TextChanged += OnQualifierUpdated;
            tbValue.Label = "Qualifier Value";
            tbValue.Help = "Entity column value.";

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
            configurationValues.Add( ENTITY_TYPE_NAME_KEY, new ConfigurationValue( "Entity Type", "The type of entity to display categories for", "" ) );
            configurationValues.Add( QUALIFIER_COLUMN_KEY, new ConfigurationValue( "Qualifier Column", "Entity column qualifier", "" ) );
            configurationValues.Add( QUALIFIER_VALUE_KEY, new ConfigurationValue( "Qualifier Value", "Entity column value", "" ) );

            if ( controls != null && controls.Count >= 3 )
            {
                if ( controls[0] != null && controls[0] is DropDownList )
                    configurationValues[ENTITY_TYPE_NAME_KEY].Value = ( ( DropDownList ) controls[0] ).SelectedValue;

                if ( controls[1] != null && controls[1] is TextBox )
                    configurationValues[QUALIFIER_COLUMN_KEY].Value = ( ( TextBox ) controls[1] ).Text;

                if ( controls[2] != null && controls[2] is TextBox )
                    configurationValues[QUALIFIER_VALUE_KEY].Value = ( ( TextBox ) controls[2] ).Text;
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
            if ( controls != null && controls.Count >= 3 && configurationValues != null )
            {
                if ( controls[0] != null && controls[0] is DropDownList && configurationValues.ContainsKey( ENTITY_TYPE_NAME_KEY ) )
                    ( ( DropDownList ) controls[0] ).SelectedValue = configurationValues[ENTITY_TYPE_NAME_KEY].Value;

                if ( controls[1] != null && controls[1] is TextBox && configurationValues.ContainsKey( QUALIFIER_COLUMN_KEY ) )
                    ( ( TextBox ) controls[1] ).Text = configurationValues[QUALIFIER_COLUMN_KEY].Value;

                if ( controls[2] != null && controls[2] is TextBox && configurationValues.ContainsKey( QUALIFIER_VALUE_KEY ) )
                    ( ( TextBox ) controls[2] ).Text = configurationValues[QUALIFIER_VALUE_KEY].Value;
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
            var picker = new CategoryPicker { ID = id };

            if ( configurationValues != null )
            {
                if ( configurationValues.ContainsKey( ENTITY_TYPE_NAME_KEY ) )
                {
                    string entityTypeName = configurationValues[ENTITY_TYPE_NAME_KEY].Value;
                    if ( !string.IsNullOrWhiteSpace( entityTypeName ) && entityTypeName != None.IdValue )
                    {
                        picker.EntityTypeName = entityTypeName;
                        if ( configurationValues.ContainsKey( QUALIFIER_COLUMN_KEY ) )
                        {
                            picker.EntityTypeQualifierColumn = configurationValues[QUALIFIER_COLUMN_KEY].Value;
                            if ( configurationValues.ContainsKey( QUALIFIER_VALUE_KEY ) )
                            {
                                picker.EntityTypeQualifierValue = configurationValues[QUALIFIER_VALUE_KEY].Value;
                            }
                        }
                    }
                }
            }

            return picker;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// return value as Category.Guid
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as CategoryPicker;

            if ( picker != null )
            {
                var id = picker.ItemId.AsIntegerOrNull();
                if ( id.HasValue )
                {
                    var category = CategoryCache.Get( id.Value );
                    if ( category != null )
                    {
                        return category.Guid.ToString();
                    }
                }

                return string.Empty;
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// value is a Category.Guid string
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var picker = control as CategoryPicker;

            if ( picker != null )
            {
                Guid? guid = value.AsGuidOrNull();
                if ( guid != null )
                {
                    var category = new CategoryService( new RockContext() ).Get( guid.Value );
                    picker.SetValue( category );
                }
            }
        }

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public int? GetEditValueAsEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            Guid guid = GetEditValue( control, configurationValues ).AsGuid();
            var category = new CategoryService( new RockContext() ).Get( guid );
            return category != null ? category.Id : ( int? ) null;
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            var category = new CategoryService( new RockContext() ).Get( id ?? 0 );
            string guidValue = category != null ? category.Guid.ToString() : string.Empty;
            SetEditValue( control, configurationValues, guidValue );
        }

#endif
        #endregion
    }
}
