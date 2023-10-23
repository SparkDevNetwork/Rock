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
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using Rock.Data;
using Rock.Tasks;
using Rock.Transactions;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class GroupType
    {
        #region Properties

        /// <summary>
        /// Gets or sets the attendance reminder followup days list.  This is the logical representation of <see cref="AttendanceReminderFollowupDays"/>.
        /// </summary>
        /// <value>The attendance reminder followup days list.</value>
        [CodeGenExclude(CodeGenFeature.ViewModelFile)]
        public List<int> AttendanceReminderFollowupDaysList
        {
            get
            {
                var intValues = new List<int>();

                var values = this.AttendanceReminderFollowupDays.SplitDelimitedValues( "," );
                foreach ( var value in values )
                {
                    if ( int.TryParse( value, out var intValue ) )
                    {
                        intValues.Add( intValue );
                    }
                }

                return intValues;
            }
            set
            {
                this.AttendanceReminderFollowupDays = value.AsDelimited( "," );
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets a queryable collection of <see cref="Rock.Model.Group">Groups</see> that belong to this GroupType.
        /// </summary>
        /// <value>
        /// A queryable collection of <see cref="Rock.Model.Group">Groups</see> that belong to this GroupType.
        /// </value>
        public virtual IQueryable<Group> GroupQuery
        {
            get
            {
                var groupService = new GroupService( new RockContext() );
                var qry = groupService.Queryable().Where( a => a.GroupTypeId.Equals( this.Id ) );
                return qry;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public override bool IsValid
        {
            get
            {
                var result = base.IsValid;
                if ( result )
                {
                    // make sure it isn't getting saved with a recursive parent hierarchy
                    var parentIds = new List<int>();
                    parentIds.Add( this.Id );
                    var parent = this.InheritedGroupTypeId.HasValue ? ( this.InheritedGroupType ?? new GroupTypeService( new RockContext() ).Get( this.InheritedGroupTypeId.Value ) ) : null;
                    while ( parent != null )
                    {
                        if ( parentIds.Contains( parent.Id ) )
                        {
                            this.ValidationResults.Add( new ValidationResult( "Parent Group Type cannot be a child of this Group Type (recursion)" ) );
                            return false;
                        }
                        else
                        {
                            parentIds.Add( parent.Id );
                            parent = parent.InheritedGroupType;
                        }
                    }

                    if ( string.IsNullOrEmpty( GroupViewLavaTemplate ) )
                    {
                        this.ValidationResults.Add( new ValidationResult( "Lava template for group view is mandatory." ) );
                        return false;
                    }

                    if ( this.IsSchedulingEnabled && !Enum.GetValues( typeof( ScheduleType ) ).Cast<ScheduleType>().Where( v => v != ScheduleType.None && AllowedScheduleTypes.HasFlag( v ) ).Any() )
                    {
                        this.ValidationResults.Add( new ValidationResult( "A 'Group Schedule Option' must be selected under 'Attendance / Check-In' when Scheduling is enabled." ) );
                        return false;
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Gets a list of GroupType Ids, including our own Id, that identifies the
        /// inheritance tree.
        /// </summary>
        /// <param name="rockContext">The database context to operate in.</param>
        /// <returns>A list of GroupType Ids, including our own Id, that identifies the inheritance tree.</returns>
        public List<int> GetInheritedGroupTypeIds( Rock.Data.RockContext rockContext )
        {
            // Attempt to get an existing GroupTypeCache object from the cache
            // manager without loading it from the database. If the cache system
            // were to try and load from the database then it could cause a
            // recursive loop since the cache object would load attributes which
            // would in turn call us again.
            if ( GroupTypeCache.TryGet( Id, out var groupTypeCache ) )
            {
                return groupTypeCache.GetInheritedGroupTypeIds();
            }

            rockContext = rockContext ?? new RockContext();

            var groupTypeService = new GroupTypeService( rockContext );
            var groupTypeIds = new List<int>();
            var groupType = this;

            //
            // Loop until we find a recursive loop or run out of parent group types.
            //
            while ( groupType != null && !groupTypeIds.Contains( groupType.Id ) )
            {
                groupTypeIds.Insert( 0, groupType.Id );

                if ( groupType.InheritedGroupTypeId.HasValue )
                {
                    groupType = groupType.InheritedGroupType ?? groupTypeService
                        .Queryable().AsNoTracking().FirstOrDefault( t => t.Id == ( groupType.InheritedGroupTypeId ?? 0 ) );
                }
                else
                {
                    groupType = null;
                }
            }

            return groupTypeIds;
        }

        /// <summary>
        /// Gets all dependent group type ids.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public List<int> GetAllDependentGroupTypeIds( Rock.Data.RockContext rockContext )
        {
            rockContext = rockContext ?? new RockContext();

            var groupTypeService = new GroupTypeService( rockContext );
            var groupTypeIds = new List<int>( 10 );

            var groupType = this;

            //
            // Loop until we find a recursive loop or run out of parent group types.
            //
            List<int> childGroupTypeIds = null;
            do
            {
                if ( childGroupTypeIds == null )
                {
                    childGroupTypeIds = groupTypeService
                                    .Queryable()
                                    .AsNoTracking()
                                    .Where( t => t.InheritedGroupTypeId == groupType.Id )
                                    .Select( t => t.Id ).ToList();
                }
                else
                {
                    childGroupTypeIds = groupTypeService
                                    .Queryable()
                                    .AsNoTracking()
                                    .Where( t => t.InheritedGroupTypeId != null && childGroupTypeIds.Contains( t.InheritedGroupTypeId.Value ) )
                                    .Select( t => t.Id ).ToList();
                }

                groupTypeIds.AddRange( childGroupTypeIds );
            } while ( childGroupTypeIds.Count > 0 );

            return groupTypeIds;
        }

        /// <summary>
        /// Gets a list of all attributes defined for the GroupTypes specified that
        /// match the entityTypeQualifierColumn and the GroupType Ids.
        /// </summary>
        /// <param name="rockContext">The database context to operate in.</param>
        /// <param name="entityTypeId">The Entity Type Id for which Attributes to load.</param>
        /// <param name="entityTypeQualifierColumn">The EntityTypeQualifierColumn value to match against.</param>
        /// <returns>A list of attributes defined in the inheritance tree.</returns>
        public List<AttributeCache> GetInheritedAttributesForQualifier( Rock.Data.RockContext rockContext, int entityTypeId, string entityTypeQualifierColumn )
        {
            var groupTypeIds = GetInheritedGroupTypeIds( rockContext );

            var inheritedAttributes = new Dictionary<int, List<AttributeCache>>();
            groupTypeIds.ForEach( g => inheritedAttributes.Add( g, new List<AttributeCache>() ) );

            //
            // Walk each group type and generate a list of matching attributes.
            //
            foreach ( var entityTypeAttribute in AttributeCache.GetByEntityType( entityTypeId ) )
            {
                // group type ids exist and qualifier is for a group type id
                if ( string.Compare( entityTypeAttribute.EntityTypeQualifierColumn, entityTypeQualifierColumn, true ) == 0 )
                {
                    int groupTypeIdValue = int.MinValue;
                    if ( int.TryParse( entityTypeAttribute.EntityTypeQualifierValue, out groupTypeIdValue ) && groupTypeIds.Contains( groupTypeIdValue ) )
                    {
                        inheritedAttributes[groupTypeIdValue].Add( entityTypeAttribute );
                    }
                }
            }

            //
            // Walk the generated list of attribute groups and put them, ordered, into a list
            // of inherited attributes.
            //
            var attributes = new List<AttributeCache>();
            foreach ( var attributeGroup in inheritedAttributes )
            {
                foreach ( var attribute in attributeGroup.Value.OrderBy( a => a.Order ) )
                {
                    attributes.Add( attribute );
                }
            }

            return attributes;
        }

        /// <summary>
        /// Get a list of all inherited Attributes that should be applied to this entity.
        /// </summary>
        /// <returns>A list of all inherited AttributeCache objects.</returns>
        public override List<AttributeCache> GetInheritedAttributes( Rock.Data.RockContext rockContext )
        {
            return GetInheritedAttributesForQualifier( rockContext, TypeId, "Id" );
        }

        /// <summary>
        /// Get the group location picker mode for the provided location.
        /// </summary>
        /// <param name="location">The location whose group location picker mode should be determined.</param>
        /// <returns>The group location picker mode for the provided location.</returns>
        public static GroupLocationPickerMode GetGroupLocationPickerMode( Location location )
        {
            if ( location != null )
            {
                if ( location.IsNamedLocation )
                {
                    return GroupLocationPickerMode.Named;
                }

                if ( !string.IsNullOrWhiteSpace( location.GetFullStreetAddress().Replace( ",", string.Empty ) ) )
                {
                    return GroupLocationPickerMode.Address;
                }

                if ( location.GeoPoint != null )
                {
                    return GroupLocationPickerMode.Point;
                }

                if ( location.GeoFence != null )
                {
                    return GroupLocationPickerMode.Polygon;
                }
            }

            return GroupLocationPickerMode.None;
        }

        #endregion

        #region Index Methods

        /// <summary>
        /// Queues groups of this type to have their indexes deleted
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        public void DeleteIndexedDocumentsByGroupType( int groupTypeId )
        {
            var groupIds = new GroupService( new RockContext() ).Queryable()
                .Where( i => i.GroupTypeId == groupTypeId )
                .Select( a => a.Id ).ToList();

            int groupEntityTypeId = EntityTypeCache.GetId<Rock.Model.Group>().Value;

            foreach ( var groupId in groupIds )
            {
                var deleteEntityTypeIndexMsg = new DeleteEntityTypeIndex.Message
                {
                    EntityTypeId = groupEntityTypeId,
                    EntityId = groupId
                };

                deleteEntityTypeIndexMsg.Send();
            }
        }

        /// <summary>
        /// Queues groups of this type to have their indexes updated
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        public void BulkIndexDocumentsByGroupType( int groupTypeId )
        {
            var groupIds = new GroupService( new RockContext() ).Queryable()
                .Where( i => i.GroupTypeId == groupTypeId )
                .Select( a => a.Id ).ToList();

            int groupEntityTypeId = EntityTypeCache.GetId<Rock.Model.Group>().Value;

            foreach ( var groupId in groupIds )
            {
                var indexEntityTransaction = new IndexEntityTransaction( new EntityIndexInfo() { EntityTypeId = groupEntityTypeId, EntityId = groupId } );
                indexEntityTransaction.Enqueue();
            }
        }

        #endregion

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return GroupTypeCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            var parentGroupTypeIds = new GroupTypeService( dbContext as RockContext ).GetParentGroupTypes( this.Id ).Select( a => a.Id ).ToList();
            if ( parentGroupTypeIds?.Any() == true )
            {
                foreach ( var parentGroupTypeId in parentGroupTypeIds )
                {
                    GroupTypeCache.UpdateCachedEntity( parentGroupTypeId, EntityState.Detached );
                }
            }

            GroupTypeCache.UpdateCachedEntity( this.Id, entityState );
        }

        #endregion
    }
}
