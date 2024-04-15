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
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.Attributes;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;


namespace Rock.Blocks.Core
{
    /// <summary>
    /// Allows the user to view and edit attributes.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Attributes" )]
    [Category( "Core" )]
    [Description( "Allows for the managing of attributes." )]
    [IconCssClass( "fa fa-list-ul" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

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

    [Rock.SystemGuid.EntityTypeGuid( "A7D9C259-1CD0-42C2-B708-4D95F2469B18" )]
    [Rock.SystemGuid.BlockTypeGuid( "791DB49B-58A4-44E1-AEF5-ABFF2F37E197" )]
    public class Attributes : RockEntityListBlockType<Model.Attribute>
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

        private static class PreferenceKey
        {
            public const string FilterEntityTypeGuid = "filter-entity-type-guid";
            public const string FilterCategories = "filter-categories";
            public const string FilterActive = "filter-active";
        }

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var entityTypeGuid = GetAttributeValue( AttributeKey.Entity ).AsGuidOrNull();

            var box = new ListBlockBox<AttributesOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = true;
            box.IsDeleteEnabled = true;
            box.ExpectedRowCount = null;
            box.Options = new AttributesOptionsBag
            {
                EntityTypeGuid = entityTypeGuid,
                EntityTypes = !entityTypeGuid.HasValue ? GetEntityTypes() : null,
                HideColumns = GetAttributeValue( AttributeKey.HideColumnsOnGrid ).SplitDelimitedValues().ToList(),
                EnableShowInGrid = GetAttributeValue( AttributeKey.EnableShowInGrid ).AsBoolean(),
                AllowSettingOfValues = GetAttributeValue( AttributeKey.AllowSettingofValues ).AsBoolean(),
            };
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets a list of the entity types that can be selected in the picker.
        /// </summary>
        /// <returns>A collection of ListItemViewModel objects that represent the entity types.</returns>
        private List<ListItemBag> GetEntityTypes()
        {
            var entityTypes = EntityTypeCache.All()
                .Where( t => t.IsEntity )
                .OrderByDescending( t => t.IsCommon )
                .ThenBy( t => t.FriendlyName )
                .Select( t => new ListItemBag
                {
                    Value = t.Guid.ToString(),
                    Text = t.FriendlyName,
                    Category = t.IsCommon ? "Common" : "All Entities"
                } )
                .ToList();

            entityTypes.Insert( 0, new ListItemBag
            {
                Value = Guid.Empty.ToString(),
                Text = "None (Global Attributes)"
            } );

            return entityTypes;
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

        /// <inheritdoc/>
        protected override IQueryable<Model.Attribute> GetListQueryable( RockContext rockContext )
        {
            var entityTypeGuid = GetAttributeValue( AttributeKey.Entity ).AsGuidOrNull();
            var isBlockSetting = entityTypeGuid.HasValue;

            if ( !isBlockSetting )
            {
                entityTypeGuid = GetBlockPersonPreferences().GetValue( PreferenceKey.FilterEntityTypeGuid ).AsGuidOrNull();
            }

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

            var activeFilter = GetBlockPersonPreferences().GetValue( PreferenceKey.FilterActive );

            if ( activeFilter.IsNotNullOrWhiteSpace() )
            {
                var booleanActiveFilter = activeFilter.AsBoolean();
                query = query.Where( a => a.IsActive == booleanActiveFilter );
            }

            return query;
        }

        /// <inheritdoc/>
        protected override IQueryable<Model.Attribute> GetOrderedListQueryable( IQueryable<Model.Attribute> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( a => a.Order );
        }

        /// <inheritdoc/>
        protected override GridBuilder<Model.Attribute> GetGridBuilder()
        {
            // Grid data is built later so we can't dispose of rockContext via `using`
            var rockContext = new RockContext();
            var builder = new GridBuilder<Model.Attribute>()
                .WithBlock( this )
                .AddTextField( "id", a => a.Id.ToString() )
                .AddTextField( "idKey", a => a.IdKey )
                .AddField( "guid", a => a.Guid )
                .AddField( "key", a => a.Key )
                .AddField( "entityTypeGuid", a => EntityTypeCache.Get( a.EntityTypeId ?? 0 )?.Guid ?? Guid.Empty )
                .AddTextField( "name", a => a.Name )
                .AddField( "categories", a => a.Categories.Select( c => c.Name ).ToList().AsDelimited( ", " ) )
                .AddField( "order", a => a.Order )
                .AddField( "isActive", a => a.IsActive )
                .AddField( "qualifier", a => GetAttributeQualifier( a ) )
                .AddField( "attribute", a => GetPublicAttribute( rockContext, a ) )
                .AddTextField( "value", a => GetPublicAttributeValue( rockContext, a ) )
                .AddTextField( "defaultValue", a => GetPublicAttributeDefaultValue( a ) )
                .AddField( "isDeleteEnabled", a => !a.IsSystem )
                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) );

            return builder;
        }

        /// <summary>
        /// Get the friendly attribute qualifier text.
        /// </summary>
        /// <param name="attribute">The attribute whose qualifier data will be formatted.</param>
        /// <returns>A string that represents the qualifier requirements.</returns>
        private string GetAttributeQualifier( Model.Attribute attribute )
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
        /// Gets the attribute value as a model that can be displayed on the
        /// user's device. This handles special block settings that change what
        /// value is available.
        /// </summary>
        /// <param name="rockContext">The rock database context.</param>
        /// <param name="attribute">The attribute whose value will be viewed.</param>
        /// <returns>A <see cref="PublicAttributeValueViewModel"/> that represents the attribute value.</returns>
        private PublicAttributeBag GetPublicAttribute( RockContext rockContext, Model.Attribute attribute )
        {
            var entityId = GetEntityId();
            var attributeCache = AttributeCache.Get( attribute.Id );

            if ( GetAttributeValue( AttributeKey.AllowSettingofValues ).AsBooleanOrNull() ?? false )
            {
                AttributeValueService attributeValueService = new AttributeValueService( rockContext );
                var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attribute.Id, entityId );

                if ( attributeValue != null && !attributeValue.Value.IsNullOrWhiteSpace() )
                {
                    return PublicAttributeHelper.GetPublicAttributeForView( attributeCache, attributeValue.Value );
                }
                else
                {
                    return PublicAttributeHelper.GetPublicAttributeForView( attributeCache, attribute.DefaultValue );
                }
            }
            else
            {
                return PublicAttributeHelper.GetPublicAttributeForView( attributeCache, attribute.DefaultValue );
            }
        }

        /// <summary>
        /// Gets the attribute value as a model that can be displayed on the
        /// user's device. This handles special block settings that change what
        /// value is available.
        /// </summary>
        /// <param name="rockContext">The rock database context.</param>
        /// <param name="attribute">The attribute whose value will be viewed.</param>
        /// <returns>A string that represents the attribute value.</returns>
        private string GetPublicAttributeValue( RockContext rockContext, Model.Attribute attribute )
        {
            var entityId = GetEntityId();
            var attributeCache = AttributeCache.Get( attribute.Id );

            if ( GetAttributeValue( AttributeKey.AllowSettingofValues ).AsBoolean() )
            {
                AttributeValueService attributeValueService = new AttributeValueService( rockContext );
                var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attribute.Id, entityId );

                if ( attributeValue != null && !attributeValue.Value.IsNullOrWhiteSpace() )
                {
                    return PublicAttributeHelper.GetPublicValueForView( attributeCache, attributeValue.Value );
                }
            }

            return "";
        }

        /// <summary>
        /// Gets the attribute's default value as a model that can be displayed on the
        /// user's device. This handles special block settings that change what
        /// value is available.
        /// </summary>
        /// <param name="attribute">The attribute whose value will be viewed.</param>
        /// <returns>A string that represents the attribute's default value.</returns>
        private string GetPublicAttributeDefaultValue( Model.Attribute attribute )
        {
            var attributeCache = AttributeCache.Get( attribute.Id );

            return PublicAttributeHelper.GetPublicValueForView( attributeCache, attribute.DefaultValue );
        }

        /// <summary>
        /// Gets a single data row to be displayed in the grid for the attribute.
        /// </summary>
        /// <param name="attribute">The attribute to be displayed.</param>
        /// <returns>A grid row object that represents this attribute on the grid.</returns>
        private Dictionary<string, object> GetAttributeRow( Rock.Model.Attribute attribute )
        {
            var builder = GetGridBuilder();
            var items = new List<Rock.Model.Attribute> { attribute };

            var gridData = builder.Build( items );

            return gridData.Rows[0];
        }

        #endregion

        #region Block Actions

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

            return ActionOk( new
            {
                Attribute = PublicAttributeHelper.GetPublicAttributeForEdit( attribute ),
                Value = PublicAttributeHelper.GetPublicEditValue( attribute, value )
            } );
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

                var newValue = PublicAttributeHelper.GetPrivateValue( attribute, value );

                if ( attributeValue.Value != newValue )
                {
                    attributeValue.Value = newValue;

                    Helper.UpdateAttributeValuePersistedValues( attributeValue, attribute );

                    if ( attribute.IsReferencedEntityFieldType )
                    {
                        Helper.UpdateAttributeValueEntityReferences( attributeValue, rockContext );
                    }

                    rockContext.SaveChanges();
                }

                return ActionOk( GetAttributeRow( new AttributeService( rockContext ).Get( attribute.Id ) ) );
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
                    Attribute = PublicAttributeHelper.GetPublicEditableAttributeViewModel( attribute ),
                    EntityTypeQualifierColumn = attribute.EntityTypeQualifierColumn,
                    EntityTypeQualifierValue = attribute.EntityTypeQualifierValue,
                    EntityTypeGuid = attribute.EntityType?.Guid ?? Guid.Empty
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
        public BlockActionResult SaveEditAttribute( Guid? entityTypeGuid, string entityTypeQualifierColumn, string entityTypeQualifierValue, PublicEditableAttributeBag attribute )
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

                return ActionOk( GetAttributeRow( newAttr ) );
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

        /// <summary>
        /// Changes the ordered position of a single item.
        /// </summary>
        /// <param name="key">The identifier of the item that will be moved.</param>
        /// <param name="beforeKey">The identifier of the item it will be placed before.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult ReorderItem( string key, string beforeKey )
        {
            using ( var rockContext = new RockContext() )
            {
                // Get the queryable and make sure it is ordered correctly.
                var qry = GetListQueryable( rockContext );
                qry = GetOrderedListQueryable( qry, rockContext );

                // Get the entities from the database.
                var items = GetListItems( qry, rockContext );

                if ( !items.ReorderEntity( key, beforeKey ) )
                {
                    return ActionBadRequest( "Invalid reorder attempt." );
                }

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

        public Guid EntityTypeGuid { get; set; }

        public PublicEditableAttributeBag Attribute { get; set; }
    }

    public class GridRow
    {
        public Guid Guid { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Qualifier { get; set; }

        public string Categories { get; set; }

        public bool IsActive { get; set; }

        public PublicAttributeBag Attribute { get; set; }

        public string Value { get; set; }

        public bool IsDeleteEnabled { get; set; }

        public bool IsSecurityEnabled { get; set; }
    }
}
