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
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Core.AttributeValues;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Types.Mobile.Core
{
    /// <summary>
    /// Displays and allows edit of attribute values (that have a supported field type) in the mobile shell.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />
    [DisplayName( "Attribute Values" )]
    [Category( "Mobile > Core" )]
    [Description( "Used to display attribute values based on the category." )]
    [IconCssClass( "fa fa-edit" )]
    [ContextAware]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [CategoryField(
        "Category",
        Key = AttributeKey.Category,
        AllowMultiple = true,
        Description = "The Attribute Categories to display attributes from",
        EntityTypeName = "Rock.Model.Attribute",
        IsRequired = true,
        Order = 0 )]

    [BooleanField(
        "Use Abbreviated Name",
        Key = AttributeKey.UseAbbreviatedName,
        Description = "Display the abbreviated name for the attribute if it exists, otherwise the full name is shown.",
        IsRequired = true,
        DefaultBooleanValue = false,
        Order = 2 )]

    [EntityTypeField(
        "Entity Type",
        Description = "The type of entity to display attribute values of.",
        IsRequired = false,
        Key = AttributeKey.ContextEntityType,
        Order = 3 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_CORE_ATTRIBUTE_VALUES )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.MOBILE_CORE_ATTRIBUTE_VALUES )]
    public class AttributeValues : RockBlockType
    {
        #region IRockMobileBlockType

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 5 );

        #endregion

        #region Keys

        private static class AttributeKey
        {
            public const string Category = "Category";
            public const string AttributeOrder = "AttributeOrder";
            public const string UseAbbreviatedName = "UseAbbreviatedName";
            public const string ContextEntityType = "ContextEntityType";
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the attribute value bags.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>List&lt;PublicAttributeBag&gt;.</returns>
        private List<AttributeField> GetAttributeValueBags( IHasAttributes entity )
        {
            var attributes = GetViewableAttributes( entity );
            var categories = GetAttributeValue( AttributeKey.Category ).SplitDelimitedValues( false ).AsGuidList();
            var useAbbrevNames = GetAttributeValue( AttributeKey.UseAbbreviatedName ).AsBoolean();

            if ( !categories.Any() )
            {
                return null;
            }

            var attributeBags = new List<AttributeField>();
            foreach ( var attribute in attributes )
            {
                AttributeField attributeBag;

                string value = attribute.DefaultValue;

                // If there is a value for this attribute, grab it.
                if ( entity.AttributeValues != null && entity.AttributeValues.ContainsKey( attribute.Key )
                    && entity.AttributeValues[attribute.Key] != null )
                {
                    value = entity.AttributeValues[attribute.Key].Value;
                }

                // We want to grab the corresponding attribute info dependent on the usage.
                var canEdit = attribute.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
                if ( canEdit )
                {
                    attributeBag = ToAttributeField( attribute, useAbbrevNames, true, value );
                }
                else
                {
                    attributeBag = ToAttributeField( attribute, useAbbrevNames, false, value );
                }

                attributeBags.Add( attributeBag );
            }

            return attributeBags;
        }

        /// <summary>
        /// Converts an attribute to a shell friendly AttributeField.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="useAbbreviatedName">if set to <c>true</c> [use abbreviated name].</param>
        /// <param name="canEdit">if set to <c>true</c> [can edit].</param>
        /// <param name="privateValue">The private value.</param>
        /// <returns>AttributeField.</returns>
        private AttributeField ToAttributeField( AttributeCache attribute, bool useAbbreviatedName, bool canEdit, string privateValue )
        {
            // Get the corresponding attribute depending on if we're editing or not.
            var publicAttribute = canEdit ? PublicAttributeHelper.GetPublicAttributeForEdit( attribute )
                : PublicAttributeHelper.GetPublicAttributeForView( attribute, privateValue );

            // If this person is able to edit the attribute we are going to assume that
            // those are the values we want.
            var valueUsage = canEdit ? Field.ConfigurationValueUsage.Edit : Field.ConfigurationValueUsage.View;

            // Get the public configuration values for the attribute.
            var configurationValues = attribute.FieldType.Field?.GetPublicConfigurationValues( attribute.ConfigurationValues, valueUsage, null )
                // We can safely fall back to the qualifier values if there aren't any.
                ?? attribute.QualifierValues.ToDictionary( v => v.Key, v => v.Value.Value );


            var publicEditValue = attribute.FieldType.Field?.GetPublicEditValue( privateValue, attribute.ConfigurationValues );

            // Title will be dependent on whether we want the abbreviated name or not.
            string title;
            if ( useAbbreviatedName && attribute.AbbreviatedName.IsNotNullOrWhiteSpace() )
            {
                title = attribute.AbbreviatedName;
            }
            else
            {
                title = attribute.Name;
            }

            return new AttributeField
            {
                AttributeCategories = publicAttribute.Categories.Select( x => new Common.Mobile.Blocks.Core.AttributeValues.PublicAttributeCategoryBag
                {
                    Name = x.Name,
                    Guid = x.Guid,
                    Order = x.Order
                } ).ToList(),
                CanEdit = canEdit,
                AttributeGuid = attribute.Guid,
                ConfigurationValues = configurationValues,
                FieldTypeGuid = attribute.FieldType?.Guid,
                IsRequired = attribute.IsRequired,
                Key = attribute.Key,
                Title = title,
                Value = canEdit && publicEditValue.IsNotNullOrWhiteSpace() ? publicEditValue : privateValue,
                FormattedValue = attribute.FieldType.Field?.FormatValueAsHtml( null, privateValue, attribute.QualifierValues )
            };
        }

        /// <summary>
        /// Gets the editable attributes.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        private List<AttributeCache> GetViewableAttributes( IHasAttributes entity )
        {
            // A list of Guids to limit the attributes by.
            var attributeCategories = GetAttributeValue( AttributeKey.Category )
                .SplitDelimitedValues( false )
                .AsGuidList();

            if ( attributeCategories.Any() )
            {
                return entity.Attributes.Values
                    // Ensure that one of the categories on the attribute is in our list.
                    .Where( a => a.Categories.Any( c => attributeCategories.Contains( c.Guid ) ) )
                    .Where( a => a.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name )
                    .ToList();
            }

            return new List<AttributeCache>();
        }


        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the attribute value data for the current person.
        /// </summary>
        /// <returns>BlockActionResult.</returns>
        [BlockAction]
        public BlockActionResult GetAttributeValueData()
        {
            if ( RequestContext.CurrentPerson == null )
            {
                return ActionForbidden();
            }

            var contextEntityType = GetAttributeValue( AttributeKey.ContextEntityType );
            var entityType = EntityTypeCache.Get( contextEntityType );

            if ( entityType == null )
            {
                return ActionNotFound( "There was no context entity type provided." );
            }

            using ( var rockContext = new RockContext() )
            {
                var entity = RequestContext.GetContextEntity( entityType.GetEntityType() );
                if ( entity == null || !( entity is IHasAttributes attributeEntity ) )
                {
                    return ActionNotFound( "There was no context entity found." );
                }

                return ActionOk( GetAttributeValueBags( attributeEntity ) );
            }
        }

        /// <summary>
        /// Updates a attribute value.
        /// </summary>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>BlockActionResult.</returns>
        [BlockAction]
        public BlockActionResult UpdateAttributeValue( Guid attributeGuid, string key, string value )
        {
            if ( RequestContext.CurrentPerson == null )
            {
                return ActionForbidden();
            }

            if ( attributeGuid == null )
            {
                return ActionNotFound();
            }

            var attribute = AttributeCache.Get( attributeGuid );
            if ( attribute == null )
            {
                return ActionNotFound();
            }

            var contextEntityType = GetAttributeValue( AttributeKey.ContextEntityType );
            var entityType = EntityTypeCache.Get( contextEntityType );

            if ( entityType == null )
            {
                return ActionNotFound( "There was no context entity type provided." );
            }

            using ( var rockContext = new RockContext() )
            {
                var entity = RequestContext.GetContextEntity( entityType.GetEntityType() );
                if ( entity == null || !( entity is IHasAttributes attributeEntity ) )
                {
                    return ActionNotFound( "There was no context entity found." );
                }

                attributeEntity.LoadAttributes();

                if ( !attribute.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionForbidden();
                }

                attributeEntity.SetAttributeValue( key, value );
                rockContext.SaveChanges();
                attributeEntity.SaveAttributeValues( rockContext );

                var formattedValue = attribute.FieldType.Field?.FormatValueAsHtml( null, value, attribute.QualifierValues );
                return ActionOk( new
                {
                    FormattedValue = formattedValue.IsNotNullOrWhiteSpace() ? formattedValue : value,
                } );
            }

        }

        #endregion
    }
}
