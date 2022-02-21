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
using System.Collections.Concurrent;
using System.Linq;

using Rock.Field;
using Rock.ViewModel.NonEntities;
using Rock.Web.Cache;

namespace Rock.Attribute
{
    /// <summary>
    /// Marked internal until we have decision on this being the final name and
    /// location for these methods.
    /// </summary>
    internal static class PublicAttributeHelper
    {
        #region Fields

        /// <summary>
        /// The field types associated with their unique identifiers. Because we
        /// expect to do these lookups so often, this provides a slight speed
        /// improvement over the cache. It also handles mapping unknown field
        /// types to the default field type.
        /// </summary>
        internal static ConcurrentDictionary<Guid, IFieldType> _fieldTypes = new ConcurrentDictionary<Guid, IFieldType>();

        #endregion

        #region Methods

        /// <summary>
        /// Converts an Attribute Value to a view model that can be sent to a public device.
        /// </summary>
        /// <param name="attributeValue">The attribute value.</param>
        /// <returns>A <see cref="PublicAttributeValueViewModel"/> instance.</returns>
        internal static PublicAttributeValueViewModel ToPublicAttributeValue( AttributeValueCache attributeValue )
        {
            var attribute = AttributeCache.Get( attributeValue.AttributeId );

            return ToPublicAttributeValue( attribute, attributeValue.Value );
        }

        /// <summary>
        /// Converts an Attribute Value to a view model that can be sent to a public device.
        /// </summary>
        /// <param name="attribute">The attribute the value is associated with.</param>
        /// <param name="value">The value to be encoded for public use.</param>
        /// <returns>A <see cref="PublicAttributeValueViewModel"/> instance.</returns>
        internal static PublicAttributeValueViewModel ToPublicAttributeValue( AttributeCache attribute, string value )
        {
            var fieldType = _fieldTypes.GetOrAdd( attribute.FieldType.Guid, GetFieldType );

            return new PublicAttributeValueViewModel
            {
                FieldTypeGuid = attribute.FieldType.Guid,
                AttributeGuid = attribute.Guid,
                Name = attribute.Name,
                Categories = attribute.Categories.Select( c => new PublicAttributeValueCategoryViewModel
                {
                    Guid = c.Guid,
                    Name = c.Name,
                    Order = c.Order
                } ).ToList(),
                Order = attribute.Order,
                TextValue = fieldType.GetTextValue( value, attribute.ConfigurationValues ),
                Value = fieldType.GetPublicValue( value, attribute.ConfigurationValues )
            };
        }

        /// <summary>
        /// Converts an Attribute Value to a view model that can be sent to a public device.
        /// </summary>
        /// <param name="attributeValue">The attribute value.</param>
        /// <returns>A <see cref="PublicEditableAttributeValueViewModel"/> instance.</returns>
        internal static PublicEditableAttributeValueViewModel ToPublicEditableAttributeValue( AttributeValueCache attributeValue )
        {
            var attribute = AttributeCache.Get( attributeValue.AttributeId );

            return ToPublicEditableAttributeValue( attribute, attributeValue.Value );
        }

        /// <summary>
        /// Converts an Attribute and the specified value to a view model that can be
        /// sent to a public device.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="value">The value to be encoded.</param>
        /// <returns>A <see cref="PublicEditableAttributeValueViewModel"/> instance.</returns>
        internal static PublicEditableAttributeValueViewModel ToPublicEditableAttributeValue( AttributeCache attribute, string value )
        {
            var fieldType = _fieldTypes.GetOrAdd( attribute.FieldType.Guid, GetFieldType );

            return new PublicEditableAttributeValueViewModel
            {
                FieldTypeGuid = attribute.FieldType.Guid,
                AttributeGuid = attribute.Guid,
                Name = attribute.Name,
                Categories = attribute.Categories.Select( c => new PublicAttributeValueCategoryViewModel
                {
                    Guid = c.Guid,
                    Name = c.Name,
                    Order = c.Order
                } ).ToList(),
                Order = attribute.Order,
                TextValue = fieldType.GetTextValue( value, attribute.ConfigurationValues ),
                Value = fieldType.GetPublicEditValue( value, attribute.ConfigurationValues ),
                Key = attribute.Key,
                IsRequired = attribute.IsRequired,
                Description = attribute.Description,
                ConfigurationValues = fieldType.GetPublicConfigurationValues( attribute.ConfigurationValues )
            };
        }

        /// <summary>
        /// Converts an Attribute to a view model that can be sent to a public
        /// device to use when editing the filter value.
        /// </summary>
        /// <param name="attribute">The attribute to be converted.</param>
        /// <returns>The view mode that is safe to transmit to a remote device.</returns>
        internal static PublicFilterableAttributeViewModel ToPublicFilterAttribute( AttributeCache attribute )
        {
            var fieldType = _fieldTypes.GetOrAdd( attribute.FieldType.Guid, GetFieldType );

            return new PublicFilterableAttributeViewModel
            {
                AttributeGuid = attribute.Guid,
                Name = attribute.Name,
                Description = attribute.Description,
                FieldTypeGuid = attribute.FieldType.Guid,
                ConfigurationValues = fieldType.GetPublicFilterConfigurationValues( attribute.ConfigurationValues )
            };
        }

        /// <summary>
        /// Converts an Attribute and database value into a view model that can
        /// be sent to a remote device for editing the value.
        /// </summary>
        /// <param name="attribute">The attribute to be converted.</param>
        /// <param name="privateValue">The database value to be converted.</param>
        /// <returns>The view model that is safe to transmit to a remote device.</returns>
        internal static PublicFilterableAttributeValueViewModel ToPublicFilterAttributeValue( AttributeCache attribute, string privateValue )
        {
            var fieldType = _fieldTypes.GetOrAdd( attribute.FieldType.Guid, GetFieldType );

            var publicValue = fieldType.GetPublicFilterValue( privateValue, attribute.ConfigurationValues );

            return new PublicFilterableAttributeValueViewModel
            {
                AttributeGuid = attribute.Guid,
                Name = attribute.Name,
                Description = attribute.Description,
                FieldTypeGuid = attribute.FieldType.Guid,
                ConfigurationValues = fieldType.GetPublicFilterConfigurationValues( attribute.ConfigurationValues ),
                Value = new PublicComparisonValueViewModel
                {
                    ComparisonType = ( int? ) publicValue.ComparisonType,
                    Value = publicValue.Value
                }
            };
        }

        /// <summary>
        /// Converts a public device value into one that can be stored in the
        /// database.
        /// </summary>
        /// <param name="attribute">The attribute being set.</param>
        /// <param name="publicValue">The value provided by a public device.</param>
        /// <returns>A string value.</returns>
        internal static string GetPrivateValue( AttributeCache attribute, string publicValue )
        {
            var fieldType = _fieldTypes.GetOrAdd( attribute.FieldType.Guid, GetFieldType );

            return fieldType.GetPrivateEditValue( publicValue, attribute.ConfigurationValues );
        }

        /// <summary>
        /// Converts a database value into one that can be sent to a public device.
        /// </summary>
        /// <param name="attribute">The attribute being set.</param>
        /// <param name="privateValue">The value that came from the database.</param>
        /// <returns>A string value.</returns>
        internal static string GetPublicValue( AttributeCache attribute, string privateValue )
        {
            var fieldType = _fieldTypes.GetOrAdd( attribute.FieldType.Guid, GetFieldType );

            return fieldType.GetPublicValue( privateValue, attribute.ConfigurationValues );
        }

        /// <summary>
        /// Converts a database value into an editable one that can be sent to a
        /// public device.
        /// </summary>
        /// <param name="attribute">The attribute being set.</param>
        /// <param name="privateValue">The value that came from the database.</param>
        /// <returns>A string value.</returns>
        internal static string GetPublicEditValue( AttributeCache attribute, string privateValue )
        {
            var fieldType = _fieldTypes.GetOrAdd( attribute.FieldType.Guid, GetFieldType );

            return fieldType.GetPublicEditValue( privateValue, attribute.ConfigurationValues );
        }

        /// <summary>
        /// Gets the <see cref="IFieldType"/> that handles the specified
        /// unique identifier.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns>A <see cref="IFieldType"/> instance.</returns>
        private static IFieldType GetFieldType( Guid guid )
        {
            return FieldTypeCache.Get( guid )?.Field ?? new Field.Types.TextFieldType();
        }

        #endregion
    }
}
