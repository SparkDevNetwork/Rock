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
    internal static class ClientAttributeHelper
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
        /// Converts an Attribute Value to a view model that can be sent to the client.
        /// </summary>
        /// <param name="attributeValue">The attribute value.</param>
        /// <returns>A <see cref="ClientAttributeValueViewModel"/> instance.</returns>
        /// <remarks>Internal until this is moved to a permanent location.</remarks>
        internal static ClientAttributeValueViewModel ToClientAttributeValue( AttributeValueCache attributeValue )
        {
            var attribute = AttributeCache.Get( attributeValue.AttributeId );

            var fieldType = _fieldTypes.GetOrAdd( attribute.FieldType.Guid, GetFieldType );

            return new ClientAttributeValueViewModel
            {
                FieldTypeGuid = attribute.FieldType.Guid,
                AttributeGuid = attribute.Guid,
                Name = attributeValue.AttributeName,
                Categories = attribute.Categories.Select( c => new ClientAttributeValueCategoryViewModel
                {
                    Guid = c.Guid,
                    Name = c.Name,
                    Order = c.Order
                } ).ToList(),
                Order = attribute.Order,
                TextValue = fieldType.GetTextValue( attributeValue.Value, attribute.ConfigurationValues ),
                Value = fieldType.GetClientValue( attributeValue.Value, attribute.ConfigurationValues )
            };
        }

        /// <summary>
        /// Converts an Attribute Value to a view model that can be sent to the client.
        /// </summary>
        /// <param name="attributeValue">The attribute value.</param>
        /// <returns>A <see cref="ClientEditableAttributeValueViewModel"/> instance.</returns>
        /// <remarks>Internal until this is moved to a permanent location.</remarks>
        internal static ClientEditableAttributeValueViewModel ToClientEditableAttributeValue( AttributeValueCache attributeValue )
        {
            var attribute = AttributeCache.Get( attributeValue.AttributeId );

            return ToClientEditableAttributeValue( attribute, attributeValue.Value );
        }

        /// <summary>
        /// Converts an Attribute and the specified value to a view model that can be
        /// sent to the client.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="value">The value to be encoded.</param>
        /// <returns>A <see cref="ClientEditableAttributeValueViewModel"/> instance.</returns>
        /// <remarks>Internal until this is moved to a permanent location.</remarks>
        internal static ClientEditableAttributeValueViewModel ToClientEditableAttributeValue( AttributeCache attribute, string value )
        {
            var fieldType = _fieldTypes.GetOrAdd( attribute.FieldType.Guid, GetFieldType );

            return new ClientEditableAttributeValueViewModel
            {
                FieldTypeGuid = attribute.FieldType.Guid,
                AttributeGuid = attribute.Guid,
                Name = attribute.Name,
                Categories = attribute.Categories.Select( c => new ClientAttributeValueCategoryViewModel
                {
                    Guid = c.Guid,
                    Name = c.Name,
                    Order = c.Order
                } ).ToList(),
                Order = attribute.Order,
                TextValue = fieldType.GetTextValue( value, attribute.ConfigurationValues ),
                Value = fieldType.GetClientEditValue( value, attribute.ConfigurationValues ),
                Key = attribute.Key,
                IsRequired = attribute.IsRequired,
                Description = attribute.Description,
                ConfigurationValues = fieldType.GetClientConfigurationValues( attribute.ConfigurationValues )
            };
        }

        /// <summary>
        /// Converts a client provided value into one that can be stored in
        /// the database.
        /// </summary>
        /// <param name="attribute">The attribute being set.</param>
        /// <param name="clientValue">The value provided by the client.</param>
        /// <returns>A string value.</returns>
        /// <remarks>Internal until this is moved to a permanent location.</remarks>
        internal static string GetValueFromClient( AttributeCache attribute, string clientValue )
        {
            var fieldType = _fieldTypes.GetOrAdd( attribute.FieldType.Guid, GetFieldType );

            return fieldType.GetValueFromClient( clientValue, attribute.ConfigurationValues );
        }

        /// <summary>
        /// Converts a database value into one that can be sent to a client.
        /// </summary>
        /// <param name="attribute">The attribute being set.</param>
        /// <param name="databaseValue">The value that came from the database.</param>
        /// <returns>A string value.</returns>
        /// <remarks>Internal until this is moved to a permanent location.</remarks>
        internal static string GetClientValue( AttributeCache attribute, string databaseValue )
        {
            var fieldType = _fieldTypes.GetOrAdd( attribute.FieldType.Guid, GetFieldType );

            return fieldType.GetClientValue( databaseValue, attribute.ConfigurationValues );
        }

        /// <summary>
        /// Converts a database value into an editable one that can be sent to a client.
        /// </summary>
        /// <param name="attribute">The attribute being set.</param>
        /// <param name="databaseValue">The value that came from the database.</param>
        /// <returns>A string value.</returns>
        /// <remarks>Internal until this is moved to a permanent location.</remarks>
        internal static string GetClientEditValue( AttributeCache attribute, string databaseValue )
        {
            var fieldType = _fieldTypes.GetOrAdd( attribute.FieldType.Guid, GetFieldType );

            return fieldType.GetClientEditValue( databaseValue, attribute.ConfigurationValues );
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
