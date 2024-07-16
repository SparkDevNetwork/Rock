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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Rock.Field;
using Rock.ViewModels.Utility;
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
        /// Gets the public editable attribute view model. This contains all the
        /// information required for the individual to make changes to the attribute.
        /// </summary>
        /// <remarks>This is for editing the attribute itself, not the attribute value.</remarks>
        /// <param name="attribute">The attribute that will be represented.</param>
        /// <returns>A <see cref="PublicEditableAttributeBag"/> that represents the attribute.</returns>
        internal static PublicEditableAttributeBag GetPublicEditableAttributeViewModel( Rock.Model.Attribute attribute )
        {
            var fieldTypeCache = FieldTypeCache.Get( attribute.FieldTypeId );
            var configurationValues = attribute.AttributeQualifiers.ToDictionary( q => q.Key, q => q.Value );

            return new PublicEditableAttributeBag
            {
                Guid = attribute.Guid,
                Name = attribute.Name,
                Key = attribute.Key,
                AbbreviatedName = attribute.AbbreviatedName,
                Description = attribute.Description,
                IsActive = attribute.IsActive,
                IsAnalytic = attribute.IsAnalytic,
                IsAnalyticHistory = attribute.IsAnalyticHistory,
                PreHtml = attribute.PreHtml,
                PostHtml = attribute.PostHtml,
                IsAllowSearch = attribute.AllowSearch,
                IsEnableHistory = attribute.EnableHistory,
                IsIndexEnabled = attribute.IsIndexEnabled,
                IsPublic = attribute.IsPublic,
                IsRequired = attribute.IsRequired,
                IsSystem = attribute.IsSystem,
                IsShowInGrid = attribute.IsGridColumn,
                IsShowOnBulk = attribute.ShowOnBulk,
                FieldTypeGuid = fieldTypeCache.ControlFieldTypeGuid,
                RealFieldTypeGuid = fieldTypeCache.Guid,
                Categories = attribute.Categories
                    .Select( c => new ListItemBag
                    {
                        Value = c.Guid.ToString(),
                        Text = c.Name
                    } )
                    .ToList(),
                ConfigurationValues = fieldTypeCache.Field?.GetPublicConfigurationValues( configurationValues, Field.ConfigurationValueUsage.Configure, null ) ?? new Dictionary<string, string>(),
                DefaultValue = fieldTypeCache.Field?.GetPublicEditValue( attribute.DefaultValue, configurationValues ) ?? string.Empty
            };
        }

        /// <summary>
        /// Converts an Attribute Value to a view model that can be sent to a
        /// public device for the purpose of viewing the value.
        /// </summary>
        /// <param name="attributeValue">The attribute value.</param>
        /// <returns>A <see cref="PublicAttributeBag"/> instance that contains details about the attribute but not the value.</returns>
        internal static PublicAttributeBag GetPublicAttributeForView( AttributeValueCache attributeValue )
        {
            var attribute = AttributeCache.Get( attributeValue.AttributeId );

            return GetPublicAttributeForView( attribute, attributeValue.Value );
        }

        /// <summary>
        /// Converts an Attribute and value to a view model that can be sent to a
        /// public device for the purpose of viewing the value.
        /// </summary>
        /// <param name="attribute">The attribute the value is associated with.</param>
        /// <param name="value">The value to be encoded for public use.</param>
        /// <returns>A <see cref="PublicAttributeBag"/> instance that contains details about the attribute but not the value.</returns>
        internal static PublicAttributeBag GetPublicAttributeForView( AttributeCache attribute, string value )
        {
            var fieldType = _fieldTypes.GetOrAdd( attribute.FieldType.Guid, GetFieldType );

            return new PublicAttributeBag
            {
                FieldTypeGuid = attribute.FieldType.ControlFieldTypeGuid,
                AttributeGuid = attribute.Guid,
                Name = attribute.Name,
                Categories = attribute.Categories.OrderBy( c => c.Order ).Select( c => new PublicAttributeCategoryBag
                {
                    Guid = c.Guid,
                    Name = c.Name,
                    Order = c.Order
                } ).ToList(),
                Key = attribute.Key,
                IsRequired = attribute.IsRequired,
                Description = attribute.Description,
                ConfigurationValues = fieldType.GetPublicConfigurationValues( attribute.ConfigurationValues, ConfigurationValueUsage.View, value ),
                Order = attribute.Order
            };
        }

        /// <summary>
        /// Converts an Attribute Value to a view model that can be sent to a
        /// public device for the purpose of editing the value.
        /// </summary>
        /// <param name="attributeValue">The attribute value.</param>
        /// <returns>A <see cref="PublicAttributeBag"/> instance that contains details about the attribute but not the value.</returns>
        internal static PublicAttributeBag GetPublicAttributeForEdit( AttributeValueCache attributeValue )
        {
            var attribute = AttributeCache.Get( attributeValue.AttributeId );

            return GetPublicAttributeForEdit( attribute );
        }

        /// <summary>
        /// Converts an Attribute  to a view model that can be sent to a public
        /// device for the purpose of editing a value.
        /// </summary>
        /// <returns>A <see cref="PublicAttributeBag"/> instance that contains details about the attribute but not the value.</returns>
        /// <returns>A <see cref="PublicAttributeBag"/> instance.</returns>
        internal static PublicAttributeBag GetPublicAttributeForEdit( AttributeCache attribute )
        {
            var fieldType = _fieldTypes.GetOrAdd( attribute.FieldType.Guid, GetFieldType );

            return new PublicAttributeBag
            {
                FieldTypeGuid = attribute.FieldType.ControlFieldTypeGuid,
                AttributeGuid = attribute.Guid,
                Name = attribute.Name,
                Categories = attribute.Categories.OrderBy( c => c.Order ).Select( c => new PublicAttributeCategoryBag
                {
                    Guid = c.Guid,
                    Name = c.Name,
                    Order = c.Order
                } ).ToList(),
                Order = attribute.Order,
                Key = attribute.Key,
                IsRequired = attribute.IsRequired,
                Description = attribute.Description,
                ConfigurationValues = fieldType.GetPublicConfigurationValues( attribute.ConfigurationValues, ConfigurationValueUsage.Edit, null ),
                PreHtml = attribute.PreHtml,
                PostHtml = attribute.PostHtml
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
        /// Converts a database value into one that can be sent to a public device
        /// for the purpose of viewing the value.
        /// </summary>
        /// <param name="attribute">The attribute being set.</param>
        /// <param name="privateValue">The value that came from the database.</param>
        /// <returns>A string value.</returns>
        internal static string GetPublicValueForView( AttributeCache attribute, string privateValue )
        {
            var fieldType = _fieldTypes.GetOrAdd( attribute.FieldType.Guid, GetFieldType );

            return fieldType.GetPublicValue( privateValue, attribute.ConfigurationValues );
        }

        /// <summary>
        /// Converts a database value into one that can be sent to a public device
        /// for the purpose of editing the value.
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
