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
#if WEBFORMS
using System.Web.UI;
using Newtonsoft.Json.Linq;
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
    /// Stored as comma-delimited list of Category.Guids
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.CATEGORIES )]
    public class CategoriesFieldType : CategoryFieldType, IEntityReferenceFieldType, ISplitMultiValueFieldType
    {
        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( !string.IsNullOrWhiteSpace( privateValue ) )
            {
                using ( var rockContext = new RockContext() )
                {
                    var guids = privateValue.SplitDelimitedValues();
                    var categories = new CategoryService( rockContext ).Queryable().AsNoTracking().Where( a => guids.Contains( a.Guid.ToString() ) );
                    if ( categories.Any() )
                    {
                        return string.Join( ", ", ( from category in categories select category.Name ).ToArray() );
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
            return value.SplitDelimitedValues().AsGuidList();
        }

        #endregion

        #region EditControl


        /// <inheritdoc/>
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var categories = GetCategories( privateValue );

            return categories.Select( c => c.Name ).JoinStrings( "," );
        }

        /// <inheritdoc/>
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( string.IsNullOrWhiteSpace( publicValue ) )
            {
                return string.Empty;
            }

            var token = JToken.Parse( publicValue );

            if ( token is JArray )
            {
                var categoryValues = publicValue.FromJsonOrNull<List<ListItemBag>>();
                return categoryValues.ConvertAll( c => c.Value ).AsDelimited( "," );
            }
            else if ( token is JObject )
            {
                var categoryValue = publicValue.FromJsonOrNull<ListItemBag>();
                return categoryValue.Value;
            }

            return string.Empty;
        }

        /// <inheritdoc/>
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var categories = GetCategories( privateValue ).ConvertAll( c => new ListItemBag() { Text = c.Name, Value = c.Guid.ToString() } );

            return categories.ToCamelCaseJson( false, true );
        }

        /// <summary>
        /// Converts the saved private value into categories
        /// </summary>
        /// <param name="privateValue">The private value.</param>
        /// <returns></returns>
        private List<CategoryCache> GetCategories( string privateValue )
        {
            if ( string.IsNullOrWhiteSpace( privateValue ) )
            {
                return new List<CategoryCache>();
            }

            var guids = privateValue.SplitDelimitedValues().AsGuidList();

            if ( guids.Count == 0 )
            {
                return new List<CategoryCache>();
            }

            var categories = guids.ConvertAll( c => CategoryCache.Get( c ) )
                .ToList();

            return categories;
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

            var categoryIds = guids
                .Select( g => CategoryCache.Get( g ) )
                .Where( c => c != null )
                .Select( c => c.Id )
                .ToList();

            if ( !categoryIds.Any() )
            {
                return null;
            }

            var referencedEntities = new List<ReferencedEntity>();
            foreach ( var categoryId in categoryIds )
            {
                referencedEntities.Add( new ReferencedEntity( EntityTypeCache.GetId<Category>().Value, categoryId ) );
            }

            return referencedEntities;
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

        #region ISplitMultiValueFieldType

        /// <inheritdoc/>
        public ICollection<string> SplitMultipleValues( string privateValue )
        {
            return privateValue.Split( ',' );
        }

        #endregion

        #region WebForms
#if WEBFORMS

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
            var picker = new CategoryPicker { ID = id, AllowMultiSelect = true };

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
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as CategoryPicker;
            string result = null;

            if ( picker != null )
            {
                var guids = new List<Guid>();
                var ids = picker.SelectedValuesAsInt();
                using ( var rockContext = new RockContext() )
                {
                    var categories = new CategoryService( rockContext ).Queryable().AsNoTracking().Where( c => ids.Contains( c.Id ) );

                    if ( categories.Any() )
                    {
                        guids = categories.Select( c => c.Guid ).ToList();
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
                var picker = control as CategoryPicker;
                var guids = new List<Guid>();

                if ( picker != null )
                {
                    var ids = value.Split( new[] { ',' } );

                    foreach ( var id in ids )
                    {
                        Guid guid;

                        if ( Guid.TryParse( id, out guid ) )
                        {
                            guids.Add( guid );
                        }
                    }

                    var categories = new CategoryService( new RockContext() ).Queryable().Where( c => guids.Contains( c.Guid ) );
                    picker.SetValues( categories );
                }
            }
        }

#endif
        #endregion
    }
}
