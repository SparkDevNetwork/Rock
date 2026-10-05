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

using Rock.Data;
using Rock.Field;
using Rock.Security;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Attribute
{
    /// <summary>
    /// Provides various methods for working with attributes and values in
    /// the context of being sent to a client application.
    /// </summary>
    public static class PublicAttributeHelper
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
        /// Gets the public editable attribute bag. This contains all the
        /// information required for the individual to make changes to the
        /// attribute itself.
        /// </summary>
        /// <remarks>This is for editing the attribute itself, not the attribute value.</remarks>
        /// <param name="attribute">The attribute that will be represented.</param>
        /// <returns>A <see cref="PublicEditableAttributeBag"/> that represents the attribute.</returns>
        public static PublicEditableAttributeBag GetPublicEditableAttribute( Rock.Model.Attribute attribute )
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
                IsSuppressHistoryLogging = attribute.IsSuppressHistoryLogging,
                FieldTypeGuid = fieldTypeCache.ControlFieldTypeGuid,
                RealFieldTypeGuid = fieldTypeCache.Guid,
                IconCssClass = attribute.IconCssClass,
                AttributeColor = attribute.AttributeColor,
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
        /// Converts an Attribute and value to a view model that can be sent to a
        /// public device for the purpose of viewing the value.
        /// </summary>
        /// <param name="attribute">The attribute the value is associated with.</param>
        /// <param name="value">The value to be encoded for public use.</param>
        /// <returns>A <see cref="PublicAttributeBag"/> instance that contains details about the attribute but not the value.</returns>
        public static PublicAttributeBag GetPublicAttributeForView( AttributeCache attribute, string value )
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
        /// Converts an Attribute  to a view model that can be sent to a public
        /// device for the purpose of editing a value.
        /// </summary>
        /// <returns>A <see cref="PublicAttributeBag"/> instance that contains details about the attribute but not the value.</returns>
        /// <returns>A <see cref="PublicAttributeBag"/> instance.</returns>
        public static PublicAttributeBag GetPublicAttributeForEdit( AttributeCache attribute )
        {
            var fieldType = _fieldTypes.GetOrAdd( attribute.FieldType.Guid, GetFieldType );

            var bag = new PublicAttributeBag
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
                PostHtml = attribute.PostHtml,
            };

            // Only field types whose rules are safe to give to anyone that can
            // see the edit control get their own security grant. Other field
            // types may include rules that allow changes, such as the asset
            // manager, so they must rely on a security grant from the block.
            if ( fieldType is IPublicSecurityGrantFieldType securityGrantFieldType )
            {
                var securityGrant = new SecurityGrant();

                securityGrantFieldType.AddRulesToSecurityGrant( securityGrant, attribute.ConfigurationValues );

                if ( securityGrant.Rules.Count > 0 )
                {
                    // Force attribute security grants to be valid for 1 day.
                    // This way if we change the default we don't suddenly make
                    // field types difficult to work with.
                    securityGrant.SetLifetime( TimeSpan.FromDays( 1 ) );
                    bag.SecurityGrantToken = securityGrant.ToToken();
                }
            }

            return bag;
        }

        /// <summary>
        /// Converts a public device value into one that can be stored in the
        /// database.
        /// </summary>
        /// <param name="attribute">The attribute being set.</param>
        /// <param name="publicValue">The value provided by a public device.</param>
        /// <returns>A string value.</returns>
        public static string GetPrivateValue( AttributeCache attribute, string publicValue )
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
        public static string GetPublicValueForView( AttributeCache attribute, string privateValue )
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
        public static string GetPublicValueForEdit( AttributeCache attribute, string privateValue )
        {
            var fieldType = _fieldTypes.GetOrAdd( attribute.FieldType.Guid, GetFieldType );

            return fieldType.GetPublicEditValue( privateValue, attribute.ConfigurationValues );
        }

        /// <summary>
        /// Determines whether the attribute edits sent by a client only reference
        /// attributes that are new or that already belong to the specified entity
        /// type and qualifier. This prevents an existing attribute that belongs to
        /// something else from being taken over when the edits are saved with
        /// <see cref="Helper.SaveAttributeEdits(PublicEditableAttributeBag, int?, string, string, RockContext)"/>.
        /// </summary>
        /// <param name="attributes">The attribute bags sent by the client.</param>
        /// <param name="entityTypeId">The entity type identifier the attributes must belong to.</param>
        /// <param name="qualifierColumn">The qualifier column the attributes must belong to.</param>
        /// <param name="qualifierValue">The qualifier value the attributes must belong to, or <c>null</c> if the owning entity is new.</param>
        /// <param name="rockContext">The rock context to use when loading existing attributes.</param>
        /// <returns><c>true</c> if every attribute can be saved; otherwise <c>false</c>.</returns>
        public static bool AreAttributeEditsAllowed( IEnumerable<PublicEditableAttributeBag> attributes, int? entityTypeId, string qualifierColumn, string qualifierValue, RockContext rockContext )
        {
            var attributeGuids = attributes?
                .Where( a => a != null && a.Guid.HasValue )
                .Select( a => a.Guid.Value )
                .Distinct()
                .ToList();

            if ( attributeGuids == null || !attributeGuids.Any() )
            {
                return true;
            }

            var existingAttributes = new Rock.Model.AttributeService( rockContext ).Queryable()
                .Where( a => attributeGuids.Contains( a.Guid ) )
                .Select( a => new
                {
                    a.EntityTypeId,
                    a.EntityTypeQualifierColumn,
                    a.EntityTypeQualifierValue
                } )
                .ToList();

            // A new entity can not own any existing attributes yet.
            if ( qualifierValue == null )
            {
                return !existingAttributes.Any();
            }

            return existingAttributes.All( a => a.EntityTypeId == entityTypeId
                && string.Equals( a.EntityTypeQualifierColumn ?? string.Empty, qualifierColumn ?? string.Empty, StringComparison.OrdinalIgnoreCase )
                && string.Equals( a.EntityTypeQualifierValue ?? string.Empty, qualifierValue, StringComparison.Ordinal ) );
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
