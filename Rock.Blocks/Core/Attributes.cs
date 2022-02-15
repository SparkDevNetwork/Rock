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
using Rock.Data;
using Rock.Model;
using Rock.ViewModel.NonEntities;
using Rock.Web.Cache;

/*
 * WORK IN PROGRESS
 * 
 * This block is a work in progress, but we needed something to test field types.
 * 
 * What doesn't work:
 * Filtering
 * Reorder
 * Security
 * Grid (pagination)
 * EntityId is UNTESTED.
 * Security enforcement probably not correct.
 */
namespace Rock.Blocks.Core
{
    /// <summary>
    /// Allows the user to view and edit attributes.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianBlockType" />

    [DisplayName( "Attributes" )]
    [Category( "Obsidian > Core" )]
    [Description( "Allows for the managing of attributes." )]
    [IconCssClass( "fa fa-list-ul" )]

    #region Block Attributes

    [EntityTypeField( "Entity",
        Description = "Entity Name",
        IsRequired = false,
        Category = "Applies To",
        Order = 0,
        Key = AttributeKey.Entity )]

    [TextField( "Entity Qualifier Column",
        Description = "The entity column to evaluate when determining if this attribute applies to the entity",
        IsRequired = false,
        Category = "Applies To",
        Order = 1,
        Key = AttributeKey.EntityQualifierColumn )]

    [TextField( "Entity Qualifier Value",
        Description = "The entity column value to evaluate.  Attributes will only apply to entities with this value",
        IsRequired = false,
        Category = "Applies To",
        Order = 2,
        Key = AttributeKey.EntityQualifierValue )]

    [BooleanField( "Allow Setting of Values",
        Description = "Should UI be available for setting values of the specified Entity ID?",
        DefaultValue = "false",
        Order = 3,
        Key = AttributeKey.AllowSettingofValues )]

    [IntegerField( "Entity Id",
        Description = "The entity id that values apply to",
        IsRequired = false,
        DefaultValue = "0",
        Order = 4,
        Key = AttributeKey.EntityId )]

    [BooleanField( "Enable Show In Grid",
        Description = "Should the 'Show In Grid' option be displayed when editing attributes?",
        DefaultValue = "false",
        Order = 5,
        Key = AttributeKey.EnableShowInGrid )]

    [TextField( "Category Filter",
        Description = "A comma separated list of category GUIDs to limit the display of attributes to.",
        IsRequired = false,
        Order = 6,
        Key = AttributeKey.CategoryFilter )]

    [CustomCheckboxListField(
        "Hide Columns on Grid",
        Description = "The grid columns that should be hidden.",
        ListSource = "Ordering, Id, Category, Qualifier, Value",
        IsRequired = false,
        Order = 7,
        Key = AttributeKey.HideColumnsOnGrid )]

    #endregion

    public class Attributes : RockObsidianBlockType
    {
        public static class AttributeKey
        {
            public const string Entity = "Entity";
            public const string EntityQualifierColumn = "EntityQualifierColumn";
            public const string EntityQualifierValue = "EntityQualifierValue";
            public const string AllowSettingofValues = "AllowSettingofValues";
            public const string EntityId = "EntityId";
            public const string EnableShowInGrid = "EnableShowInGrid";
            public const string CategoryFilter = "CategoryFilter";
            public const string HideColumnsOnGrid = "HideColumnsOnGrid";
        }

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var entityTypeGuid = GetAttributeValue( AttributeKey.Entity ).AsGuidOrNull();

            return new
            {
                AttributeEntityTypeId = EntityTypeCache.Get<Rock.Model.Attribute>().Id,
                EntityTypeGuid = entityTypeGuid,
                EntityTypes = !entityTypeGuid.HasValue ? GetEntityTypes() : null,
                Attributes = entityTypeGuid.HasValue ? GetAttributeRows( entityTypeGuid ) : new List<GridRow>(),
                HideColumns = GetAttributeValue( AttributeKey.HideColumnsOnGrid ).SplitDelimitedValues(),
                EnableShowInGrid = GetAttributeValue( AttributeKey.EnableShowInGrid ).AsBoolean(),
                AllowSettingOfValues = GetAttributeValue( AttributeKey.AllowSettingofValues ).AsBoolean()
            };
        }

        /// <summary>
        /// Gets a list of the entity types that can be selected in the picker.
        /// </summary>
        /// <returns>A collection of ListItemViewModel objects that represent the entity types.</returns>
        private List<ListItemViewModel> GetEntityTypes()
        {
            var entityTypes = EntityTypeCache.All()
                .Where( t => t.IsEntity )
                .OrderByDescending( t => t.IsCommon )
                .ThenBy( t => t.FriendlyName )
                .Select( t => new ListItemViewModel
                {
                    Value = t.Guid.ToString(),
                    Text = t.FriendlyName,
                    Category = t.IsCommon ? "Common" : "All Entities"
                } )
                .ToList();

            entityTypes.Insert( 0, new ListItemViewModel
            {
                Value = Guid.Empty.ToString(),
                Text = "None (Global Attributes)"
            } );

            return entityTypes;
        }

        /// <summary>
        /// Gets the attribute rows that will be sent to the client.
        /// </summary>
        /// <param name="entityTypeGuid">The entity type unique identifier</param>
        /// <returns>A list of grid row data.</returns>
        private List<GridRow> GetAttributeRows( Guid? entityTypeGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var data = GetAttributeQuery( rockContext, entityTypeGuid );

                return data.Select( a => a.Id )
                    .ToList()
                    .Select( id => AttributeCache.Get( id ) )
                    .Select( a => GetAttributeRow( a, rockContext ) )
                    .ToList();
            }
        }

        /// <summary>
        /// Gets a single data row to be displayed in the grid for the attribute.
        /// </summary>
        /// <param name="attribute">The attribute to be displayed.</param>
        /// <param name="rockContext">The database context used for queries.</param>
        /// <returns>A grid row object that represents this attribute on the grid.</returns>
        private GridRow GetAttributeRow( AttributeCache attribute, RockContext rockContext )
        {
            return new GridRow
            {
                Guid = attribute.Guid,
                Id = attribute.Id,
                Name = attribute.Name,
                Categories = attribute.Categories.Select( c => c.Name ).ToList().AsDelimited( ", " ),
                IsActive = attribute.IsActive,
                Qualifier = GetAttributeQualifier( attribute ),
                Value = GetAttributeValue( rockContext, attribute ),
                IsDeleteEnabled = !attribute.IsSystem,
                IsSecurityEnabled = false
            };
        }

        /// <summary>
        /// Gets the query for all attributes that will be presented in the list.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A queryable that will enumerate to all the attributes.</returns>
        private IQueryable<Rock.Model.Attribute> GetAttributeQuery( RockContext rockContext, Guid? entityTypeGuid )
        {
            IQueryable<Rock.Model.Attribute> query = null;
            AttributeService attributeService = new AttributeService( rockContext );

            if ( entityTypeGuid.HasValue )
            {
                if ( entityTypeGuid.Value == default )
                {
                    // entity type not configured in block or in filter, so get Global Attributes
                    query = attributeService.GetByEntityTypeId( null, true );
                    query = query.Where( t => t.EntityTypeQualifierColumn == null || t.EntityTypeQualifierColumn == "" );
                }
                else if ( GetAttributeValue( AttributeKey.Entity ).AsGuidOrNull().HasValue )
                {
                    // entity type is configured in block, so get by the entityType and qualifiers specified in the block settings
                    var entityTypeCache = EntityTypeCache.Get( entityTypeGuid.Value );
                    var entityQualifierColumn = GetAttributeValue( AttributeKey.EntityQualifierColumn );
                    var entityQualifierValue = GetAttributeValue( AttributeKey.EntityQualifierValue );
                    query = attributeService.GetByEntityTypeQualifier( entityTypeCache.Id, entityQualifierColumn, entityQualifierValue, true );
                }
                else
                {
                    // entity type is selected in the filter, so get all the attributes for that entityType. (There is no userfilter for qualifiers, so don't filter by those)
                    var entityTypeCache = EntityTypeCache.Get( entityTypeGuid.Value );
                    query = attributeService.GetByEntityTypeId( entityTypeCache.Id, true );
                }
            }

            // if filtering by block setting of categories
            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.CategoryFilter ) ) )
            {
                try
                {
                    var categoryGuids = GetAttributeValue( AttributeKey.CategoryFilter ).Split( ',' ).Select( Guid.Parse ).ToList();

                    query = query.Where( a => a.Categories.Any( c => categoryGuids.Contains( c.Guid ) ) );
                }
                catch { }
            }

            query = query.OrderBy( a => a.Order );

            return query;
        }

        /// <summary>
        /// Get the friendly attribute qualifier text.
        /// </summary>
        /// <param name="attribute">The attribute whose qualifier data will be formatted.</param>
        /// <returns>A string that represents the qualifier requirements.</returns>
        private string GetAttributeQualifier( AttributeCache attribute )
        {
            if ( attribute.EntityTypeId.HasValue )
            {
                var entityTypeName = EntityTypeCache.Get( attribute.EntityTypeId.Value ).FriendlyName;

                if ( !string.IsNullOrWhiteSpace( attribute.EntityTypeQualifierColumn ) )
                {
                    return $"{entityTypeName} where [{attribute.EntityTypeQualifierColumn}] = '{attribute.EntityTypeQualifierValue}'";
                }
                else
                {
                    return entityTypeName;
                }
            }
            else
            {
                return "Global Attribute";
            }
        }

        /// <summary>
        /// Gets the entity identifier from the block settings, accounting for 0 meaning null.
        /// </summary>
        /// <returns>A nullable integer that will not be 0.</returns>
        private int? GetEntityId()
        {
            var entityId = GetAttributeValue( AttributeKey.EntityId ).AsIntegerOrNull();

            if ( entityId == 0 )
            {
                entityId = null;
            }

            return entityId;
        }

        /// <summary>
        /// Gets the attribute value as a model that can be displayed on the
        /// user's device. This handles special block settings that change what
        /// value is available.
        /// </summary>
        /// <param name="rockContext">The rock database context.</param>
        /// <param name="attribute">The attribute whose value will be viewed.</param>
        /// <returns>A <see cref="ClientAttributeValueViewModel"/> that represents the attribute value.</returns>
        private PublicAttributeValueViewModel GetAttributeValue( RockContext rockContext, AttributeCache attribute )
        {
            var entityId = GetEntityId();

            if ( GetAttributeValue( AttributeKey.AllowSettingofValues ).AsBooleanOrNull() ?? false )
            {
                AttributeValueService attributeValueService = new AttributeValueService( rockContext );
                var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attribute.Id, entityId );

                if ( attributeValue != null && !attributeValue.Value.IsNullOrWhiteSpace() )
                {
                    return PublicAttributeHelper.ToPublicAttributeValue( attribute, attributeValue.Value );
                }
                else
                {
                    return PublicAttributeHelper.ToPublicAttributeValue( attribute, attribute.DefaultValue );
                }
            }
            else
            {
                return PublicAttributeHelper.ToPublicAttributeValue( attribute, attribute.DefaultValue );
            }
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the attributes for the spefified entity type unique identifier.
        /// </summary>
        /// <param name="entityTypeGuid">The entity type unique identifier whose attributes will be retrieved.</param>
        /// <returns>A response that includes the attributes to be displayed.</returns>
        [BlockAction]
        public BlockActionResult GetAttributes( Guid entityTypeGuid )
        {
            var entityTypeSettingGuid = GetAttributeValue( AttributeKey.Entity ).AsGuidOrNull();

            if ( entityTypeSettingGuid.HasValue && entityTypeGuid != entityTypeSettingGuid )
            {
                return ActionBadRequest( "Cannot request attributes for entity type that does not match block settings." );
            }

            return ActionOk( GetAttributeRows( entityTypeGuid ) );
        }

        /// <summary>
        /// Gets the attribute value representation for editing purposes.
        /// </summary>
        /// <param name="attributeGuid">The unique identifier of the attribute whose value will be edited.</param>
        /// <returns>A response that includes the editable representation of the attribute value.</returns>
        [BlockAction]
        public BlockActionResult GetEditAttributeValue( Guid attributeGuid )
        {
            if ( GetAttributeValue( AttributeKey.AllowSettingofValues ).AsBooleanOrNull() != true )
            {
                return ActionBadRequest( "Setting values is not enabled." );
            }

            var attribute = Rock.Web.Cache.AttributeCache.Get( attributeGuid );

            if ( attribute == null )
            {
                return ActionBadRequest();
            }

            var entityId = GetEntityId();

            var attributeValue = new AttributeValueService( new RockContext() ).GetByAttributeIdAndEntityId( attribute.Id, entityId );
            string value = attributeValue != null && !string.IsNullOrWhiteSpace( attributeValue.Value ) ? attributeValue.Value : attribute.DefaultValue;

            return ActionOk( PublicAttributeHelper.ToPublicEditableAttributeValue( attribute, value ) );
        }

        /// <summary>
        /// Saves the value from editing an attribute value.
        /// </summary>
        /// <param name="attributeGuid">The unique identifier of the attribute to be updated.</param>
        /// <param name="value">The new public value representation.</param>
        /// <returns>A response that includes the new grid row information for this attribute.</returns>
        [BlockAction]
        public BlockActionResult SaveEditAttributeValue( Guid attributeGuid, string value )
        {
            if ( GetAttributeValue( AttributeKey.AllowSettingofValues ).AsBooleanOrNull() != true )
            {
                return ActionBadRequest( "Setting values is not enabled." );
            }

            var attribute = AttributeCache.Get( attributeGuid );

            if ( attribute == null )
            {
                return ActionBadRequest();
            }

            using ( var rockContext = new RockContext() )
            {
                var entityId = GetEntityId();
                var attributeValueService = new AttributeValueService( rockContext );
                var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attribute.Id, entityId );

                if ( attributeValue == null )
                {
                    attributeValue = new AttributeValue
                    {
                        AttributeId = attribute.Id,
                        EntityId = entityId
                    };
                    attributeValueService.Add( attributeValue );
                }

                attributeValue.Value = PublicAttributeHelper.GetPrivateValue( attribute, value );

                rockContext.SaveChanges();

                return ActionOk( GetAttributeRow( attribute, rockContext ) );
            }
        }

        /// <summary>
        /// Gets the attribute representation for editing purposes.
        /// </summary>
        /// <param name="attributeGuid">The unique identifier of the attribute to be edited.</param>
        /// <returns>A response that includes the editable representation of the attribute.</returns>
        [BlockAction]
        public BlockActionResult GetEditAttribute( Guid attributeGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var attributeService = new AttributeService( rockContext );
                var attribute = attributeService.Get( attributeGuid );

                if ( attribute == null )
                {
                    return ActionBadRequest();
                }

                return ActionOk( new EditAttributeViewModel
                {
                    Attribute = Helper.GetPublicEditableAttributeViewModel( attribute ),
                    EntityTypeQualifierColumn = attribute.EntityTypeQualifierColumn,
                    EntityTypeQualifierValue = attribute.EntityTypeQualifierValue
                } );
            }
        }

        /// <summary>
        /// Saves the updated information from an editable attribute.
        /// </summary>
        /// <param name="entityTypeGuid">The entity type unique identifier used when creating a new attribute.</param>
        /// <param name="entityQualifierColumn">The entity qualifier column used when creating a new attribute.</param>
        /// <param name="entityQualifierValue">The entity qualifier value used when creating a new attribute.</param>
        /// <param name="attribute">The attribute to be created or updated.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult SaveEditAttribute( Guid? entityTypeGuid, string entityTypeQualifierColumn, string entityTypeQualifierValue, PublicEditableAttributeViewModel attribute )
        {
            using ( var rockContext = new RockContext() )
            {
                var attributeService = new AttributeService( rockContext );

                if ( attribute.Guid.HasValue )
                {
                    var existingAttribute = attributeService.Get( attribute.Guid ?? Guid.Empty );

                    if ( existingAttribute == null )
                    {
                        return ActionBadRequest( "Attribute was not found." );
                    }
                }

                Rock.Model.Attribute newAttr;

                var blockEntityTypeGuid = GetAttributeValue( AttributeKey.Entity ).AsGuidOrNull();
                if ( blockEntityTypeGuid.HasValue )
                {
                    var entityTypeId = blockEntityTypeGuid.Value == default
                        ? ( int? ) null
                        : EntityTypeCache.Get( blockEntityTypeGuid.Value ).Id;

                    newAttr = Helper.SaveAttributeEdits( attribute,
                        entityTypeId,
                        GetAttributeValue( AttributeKey.EntityQualifierColumn ),
                        GetAttributeValue( AttributeKey.EntityQualifierValue ),
                        rockContext );
                }
                else if ( entityTypeGuid.HasValue )
                {
                    var entityTypeId = entityTypeGuid.Value != Guid.Empty ? ( int? ) EntityTypeCache.Get( entityTypeGuid.Value ).Id : null;

                    newAttr = Helper.SaveAttributeEdits( attribute,
                        entityTypeId,
                        entityTypeQualifierColumn,
                        entityTypeQualifierValue,
                        rockContext );
                }
                else
                {
                    return ActionBadRequest( "EntityTypeGuid must be specified." );
                }


                if ( newAttr == null )
                {
                    return ActionBadRequest();
                }

                return ActionOk( GetAttributeRow( AttributeCache.Get( newAttr.Id ), rockContext ) );
            }
        }

        /// <summary>
        /// Delete the specified attribute from the database.
        /// </summary>
        /// <param name="attributeGuid">The attribute unique identifier to delete.</param>
        /// <returns>A response that indicates if the operation succeded.</returns>
        [BlockAction]
        public BlockActionResult DeleteAttribute( Guid attributeGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var attributeService = new AttributeService( rockContext );
                var attribute = attributeService.Get( attributeGuid );

                if ( attribute == null )
                {
                    return ActionBadRequest( "Attribute not found." );
                }

                if ( attribute.IsSystem )
                {
                    return ActionBadRequest( "System attributes cannot be deleted." );
                }

                if ( !attribute.EntityTypeId.HasValue && attribute.EntityTypeQualifierColumn == string.Empty && attribute.EntityTypeQualifierValue == string.Empty && !GetEntityId().HasValue )
                {
                    GlobalAttributesCache.Remove();
                }

                attributeService.Delete( attribute );
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        #endregion
    }

    public class EditAttributeViewModel
    {
        public string EntityTypeQualifierColumn { get; set; }

        public string EntityTypeQualifierValue { get; set; }

        public PublicEditableAttributeViewModel Attribute { get; set; }
    }

    public class GridRow
    {
        public Guid Guid { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Qualifier { get; set; }

        public string Categories { get; set; }

        public bool IsActive { get; set; }

        public PublicAttributeValueViewModel Value { get; set; }

        public bool IsDeleteEnabled { get; set; }

        public bool IsSecurityEnabled { get; set; }
    }
}
