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
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class Attribute
    {
        #region Properties

        /// <summary>
        /// Gets or sets the shortened name of the attribute.
        /// If null or whitespace then the full name is returned.
        /// </summary>
        /// <value>
        /// The name of the abbreviated.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string AbbreviatedName
        {
            get
            {
                if ( _abbreviatedName.IsNullOrWhiteSpace() )
                {
                    return Name.Truncate( 100, false );
                }

                return _abbreviatedName;
            }

            set
            {
                _abbreviatedName = value;
            }
        }

        private string _abbreviatedName;

        #endregion
        #region Methods

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                int? entityTypeId = this.EntityTypeId;
                if ( !entityTypeId.HasValue && this.EntityType != null )
                {
                    entityTypeId = this.EntityType.Id;
                }

                // The first Parent of an Attribute should be the default Entity Type security for Rock.Model.Attribute so we return a new Attribute with an Id of 0 and we preserve the EntityTypeId to achieve that. (See AttributeCache.ParentAuthority)
                if ( this.Id > 0 && entityTypeId.HasValue )
                {
                    return new Attribute { Id = 0, EntityTypeId = entityTypeId.Value };
                }

                if ( entityTypeId.HasValue )
                {
                    var entityType = EntityTypeCache.Get( entityTypeId.Value );
                    var type = entityType.GetEntityType();
                    if ( type != null &&
                         typeof( ISecured ).IsAssignableFrom( type )  &&
                        ! typeof( Rock.Extension.Component ).IsAssignableFrom( type ) 
                    )
                    {
                        return ( ISecured ) Activator.CreateInstance( type );
                    }
                }

                return base.ParentAuthority;
            }
        }

        #endregion

        /// <summary>
        /// Pres the save.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="state">The state.</param>
        public override void PreSaveChanges( Data.DbContext dbContext, EntityState state )
        {
            if ( state != EntityState.Deleted )
            {
                // ensure that the BinaryFile.IsTemporary flag is set to false for any BinaryFiles that are associated with this record
                var fieldTypeCache = FieldTypeCache.Get( this.FieldTypeId );
                if ( fieldTypeCache?.Field is Rock.Field.Types.BinaryFileFieldType )
                {
                    Guid? binaryFileGuid = DefaultValue.AsGuidOrNull();
                    if ( binaryFileGuid.HasValue )
                    {
                        BinaryFileService binaryFileService = new BinaryFileService( ( RockContext ) dbContext );
                        var binaryFile = binaryFileService.Get( binaryFileGuid.Value );
                        if ( binaryFile != null && binaryFile.IsTemporary )
                        {
                            binaryFile.IsTemporary = false;
                        }
                    }
                }
            }

            base.PreSaveChanges( dbContext, state );
        }

        #region ICacheable

        private int? originalEntityTypeId;
        private string originalEntityTypeQualifierColumn;
        private string originalEntityTypeQualifierValue;

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return AttributeCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            AttributeCache.UpdateCachedEntity( this.Id, entityState );
            if ( originalEntityTypeId.HasValue && originalEntityTypeId.Value != this.EntityTypeId )
            {
                EntityTypeAttributesCache.FlushItem( originalEntityTypeId );
            }

            EntityTypeAttributesCache.FlushItem( this.EntityTypeId );

            int? entityTypeId;
            string entityTypeQualifierColumn;
            string entityTypeQualifierValue;

            if ( entityState == EntityState.Deleted )
            {
                entityTypeId = originalEntityTypeId;
                entityTypeQualifierColumn = originalEntityTypeQualifierColumn;
                entityTypeQualifierValue = originalEntityTypeQualifierValue;
            }
            else
            {
                entityTypeId = this.EntityTypeId;
                entityTypeQualifierColumn = this.EntityTypeQualifierColumn;
                entityTypeQualifierValue = this.EntityTypeQualifierValue;
            }

            if ( ( !entityTypeId.HasValue || entityTypeId.Value == 0 ) && string.IsNullOrEmpty( entityTypeQualifierColumn ) && string.IsNullOrEmpty( entityTypeQualifierValue ) )
            {
                GlobalAttributesCache.Remove();
            }

            if ( ( !entityTypeId.HasValue || entityTypeId.Value == 0 ) && entityTypeQualifierColumn == Attribute.SYSTEM_SETTING_QUALIFIER && string.IsNullOrEmpty( entityTypeQualifierValue ) )
            {
                Rock.Web.SystemSettings.Remove();
            }

            if ( entityTypeId.HasValue )
            {
                if ( entityTypeId == EntityTypeCache.GetId<Block>() )
                {
                    // Update BlockTypes/Blocks that reference this attribute
                    if ( entityTypeQualifierColumn.Equals( "BlockTypeId", StringComparison.OrdinalIgnoreCase ) )
                    {
                        int? blockTypeId = entityTypeQualifierValue.AsIntegerOrNull();
                        if ( blockTypeId.HasValue )
                        {
                            BlockTypeCache.FlushItem( blockTypeId.Value );

                            foreach ( var blockId in new BlockService( dbContext as RockContext ).GetByBlockTypeId( blockTypeId.Value ).Select( a => a.Id ).ToList() )
                            {
                                BlockCache.FlushItem( blockId );
                            }
                        }
                    }
                }
                else if ( entityTypeId == EntityTypeCache.GetId<DefinedValue>() )
                {
                    // Update DefinedTypes/DefinedValues that reference this attribute
                    if ( entityTypeQualifierColumn.Equals( "DefinedTypeId", StringComparison.OrdinalIgnoreCase ) )
                    {
                        int? definedTypeId = entityTypeQualifierValue.AsIntegerOrNull();
                        if ( definedTypeId.HasValue )
                        {
                            DefinedTypeCache.FlushItem( definedTypeId.Value );

                            foreach ( var definedValueId in new DefinedValueService( dbContext as RockContext ).GetByDefinedTypeId( definedTypeId.Value ).Select( a => a.Id ).ToList() )
                            {
                                DefinedValueCache.FlushItem( definedValueId );
                            }
                        }
                    }
                }
                else if ( entityTypeId == EntityTypeCache.GetId<WorkflowActivityType>() )
                {
                    if ( entityTypeQualifierColumn.Equals( "ActivityTypeId", StringComparison.OrdinalIgnoreCase ) )
                    {
                        int? activityTypeId = entityTypeQualifierValue.AsIntegerOrNull();
                        if ( activityTypeId.HasValue )
                        {
                            WorkflowActivityTypeCache.FlushItem( activityTypeId.Value );
                        }
                    }
                }
                else if ( entityTypeId == EntityTypeCache.GetId<GroupType>() )
                {
                    if ( entityTypeQualifierColumn.Equals( "Id", StringComparison.OrdinalIgnoreCase ) )
                    {
                        int? groupTypeId = entityTypeQualifierValue.AsIntegerOrNull();
                        if ( groupTypeId.HasValue )
                        {
                            GroupTypeCache.FlushItem( groupTypeId.Value );
                        }
                    }
                    else if ( entityTypeQualifierColumn.Equals( "GroupTypePurposeValueId", StringComparison.OrdinalIgnoreCase ) )
                    {
                        int? groupTypePurposeValueId = entityTypeQualifierValue.AsIntegerOrNull();
                        if ( groupTypePurposeValueId.HasValue )
                        {
                            foreach ( var groupTypeId in GroupTypeCache.All().Where( a => a.GroupTypePurposeValueId == groupTypePurposeValueId.Value ).Select( a => a.Id ).ToList() )
                            {
                                GroupTypeCache.FlushItem( groupTypeId );
                            }
                        }
                    }
                }
                else if ( entityTypeId.HasValue )
                {
                    // some other EntityType. If it the EntityType has a CacheItem associated with it, clear out all the CachedItems of that type to ensure they have a clean read of the Attributes that were Added, Changed or Removed
                    EntityTypeCache entityType = EntityTypeCache.Get( entityTypeId.Value, dbContext as RockContext );

                    if ( entityType?.HasEntityCache() == true )
                    {
                        entityType.ClearCachedItems();
                    }
                }
            }
        }
        #endregion
    }
}
