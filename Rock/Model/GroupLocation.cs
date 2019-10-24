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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a <see cref="Rock.Model.Location"/> that is associated with a <see cref="Rock.Model.Group"/>.
    /// </summary>
    /// <remarks>
    /// In Rock a <see cref="Rock.Model.Group"/> is defined any party or collection of <see cref="Rock.Model.Person">Persons</see>.  Examples of GroupLocaitons
    /// could include a Person/Family's address, a Business' address, a church campus, a room where a Bible study meets.  Pretty much, it is any place where a 
    /// group of people meet or are located. 
    /// </remarks>
    [RockDomain( "Group" )]
    [Table( "GroupLocation" )]
    [DataContract]
    public partial class GroupLocation : Model<GroupLocation>, IOrdered
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Group"/> that is associated with this GroupLocation. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Group"/> that this GroupLocation is associated with.
        /// </value>
        [DataMember]
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Location"/> that is associated with this GroupLocation. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> referencing the Id of the <see cref="Rock.Model.Location"/> that is associated with this GroupLocation. 
        /// </value>
        [DataMember]
        public int LocationId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the GroupLocationType <see cref="Rock.Model.DefinedValue"/> that is used to identify the type of <see cref="Rock.Model.GroupLocation"/>
        /// that this is.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> referencing the Id of the GroupLocationType <see cref="Rock.Model.DefinedValue"/> that identifies the type of group location that this is.
        /// If a GroupLocationType <see cref="Rock.Model.DefinedValue"/> is not associated with this GroupLocation this value will be null.
        /// </value>
        [DataMember]
        [DefinedValue(SystemGuid.DefinedType.GROUP_LOCATION_TYPE)]
        public int? GroupLocationTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the <see cref="Rock.Model.Location"/> referenced by this GroupLocation is the mailing address/location for the <see cref="Rock.Model.Group"/>.  
        /// This field is only supported in the UI for family groups
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this is the mailing address/location for this <see cref="Rock.Model.Group"/>.
        /// </value>
        [DataMember]
        public bool IsMailingLocation { get; set; }

        //TODO: Document
        /// <summary>
        /// Gets or sets a flag indicating if this is the mappable location for this 
        /// This field is only supported in the UI for family groups
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is location; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsMappedLocation { get; set; }

        /// <summary>
        /// Gets or sets the group member person alias identifier.  A GroupLocation can optionally be created by selecting one of the group 
        /// member's locations.  If the GroupLocation is created this way, the member's person alias id is saved with the group location
        /// </summary>
        /// <value>
        /// The group member person alias identifier.
        /// </value>
        [DataMember]
        public int? GroupMemberPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the display order of the GroupLocation in the group location list. The lower the number the higher the 
        /// display priority this GroupLocation has. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the display order of the GroupLocation.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int Order { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Group"/> that is associated with this GroupLocation
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Group"/> that is associated with this GroupLocation.
        /// </value>
        [LavaInclude]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Location"/> that is associated with this GroupLocation.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Location"/> that is associated with this GroupLocation.
        /// </value>
        [DataMember]
        public virtual Location Location { get; set; }

        /// <summary>
        /// Gets or sets the Location Type <see cref="Rock.Model.DefinedValue"/> of this GroupLocation.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.DefinedValue"/> that represents the type of this GroupLocation.
        /// </value>
        [DataMember]
        public virtual DefinedValue GroupLocationTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the group member person alias. A GroupLocation can optionally be created by selecting one of the 
        /// group member's locations. If the GroupLocation is created this way, the member is saved with the group location
        /// </summary>
        /// <value>
        /// The group member person alias.
        /// </value>
        [LavaInclude]
        public virtual PersonAlias GroupMemberPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.Schedule">Schedules</see> that are associated with this GroupLocation.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.Schedule"/> that are associated with this GroupLocation.
        /// </value>
        [DataMember]
        public virtual ICollection<Schedule> Schedules { get; set; } = new Collection<Schedule>();

        /// <summary>
        /// Gets or sets properties that are specific to Group+Location+Schedule 
        /// </summary>
        /// <value>
        /// The group location schedule configs.
        /// </value>
        [DataMember]
        public virtual ICollection<GroupLocationScheduleConfig> GroupLocationScheduleConfigs { get; set; } = new Collection<GroupLocationScheduleConfig>();

        /// <summary>
        /// Gets or sets the history changes.
        /// </summary>
        /// <value>
        /// The history changes.
        /// </value>
        [NotMapped]
        private History.HistoryChangeList GroupHistoryChanges { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Method that will be called on an entity immediately before the item is saved by context
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="entry"></param>
        public override void PreSaveChanges( Data.DbContext dbContext, DbEntityEntry entry )
        {
            var rockContext = (RockContext)dbContext;

            GroupHistoryChanges = new History.HistoryChangeList();

            switch ( entry.State )
            {
                case EntityState.Added:
                    {
                        string locationType = History.GetDefinedValueValue( null, GroupLocationTypeValueId );
                        locationType = locationType.IsNotNullOrWhiteSpace() ? locationType : "Unknown";
                        History.EvaluateChange( GroupHistoryChanges, $"{locationType} Location", (int?)null, Location, LocationId, rockContext );
                        History.EvaluateChange( GroupHistoryChanges, $"{locationType} Is Mailing", false, IsMailingLocation );
                        History.EvaluateChange( GroupHistoryChanges, $"{locationType} Is Map Location", false, IsMappedLocation );

                        break;
                    }

                case EntityState.Modified:
                    {
                        string locationTypeName = DefinedValueCache.GetName( GroupLocationTypeValueId ) ?? "Unknown";
                        int? oldLocationTypeId = entry.OriginalValues["GroupLocationTypeValueId"].ToStringSafe().AsIntegerOrNull();
                        if ( ( oldLocationTypeId ?? 0 ) == ( GroupLocationTypeValueId ?? 0 ) )
                        {
                            History.EvaluateChange( GroupHistoryChanges, $"{locationTypeName} Location", entry.OriginalValues["LocationId"].ToStringSafe().AsIntegerOrNull(), Location, LocationId, rockContext );
                        }
                        else
                        {
                            Location newLocation = null;
                            History.EvaluateChange( GroupHistoryChanges, $"{DefinedValueCache.GetName( oldLocationTypeId ) ?? "Unknown"} Location", entry.OriginalValues["LocationId"].ToStringSafe().AsIntegerOrNull(), newLocation, (int?)null, rockContext );
                            History.EvaluateChange( GroupHistoryChanges, $"{locationTypeName} Location", (int?)null, Location, LocationId, rockContext );
                        }

                        History.EvaluateChange( GroupHistoryChanges, $"{locationTypeName} Is Mailing", entry.OriginalValues["IsMailingLocation"].ToStringSafe().AsBoolean(), IsMailingLocation );
                        History.EvaluateChange( GroupHistoryChanges, $"{locationTypeName} Is Map Location", entry.OriginalValues["IsMappedLocation"].ToStringSafe().AsBoolean(), IsMappedLocation );

                        break;
                    }

                case EntityState.Deleted:
                    {
                        string locationType = History.GetDefinedValueValue( null, entry.OriginalValues["GroupLocationTypeValueId"].ToStringSafe().AsIntegerOrNull() );
                        locationType = locationType.IsNotNullOrWhiteSpace() ? locationType : "Unknown";
                        Location loc = null;
                        History.EvaluateChange( GroupHistoryChanges, $"{locationType} Location", entry.OriginalValues["LocationId"].ToStringSafe().AsIntegerOrNull(), loc, (int?)null, rockContext );
                        break;
                    }
            }

            base.PreSaveChanges( dbContext, entry );
        }

        /// <summary>
        /// Posts the save changes.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public override void PostSaveChanges( Data.DbContext dbContext )
        {
            var rockContext = dbContext as RockContext;
            if ( GroupHistoryChanges != null && GroupHistoryChanges.Any() )
            {
                HistoryService.SaveChanges( rockContext, typeof( Group ), Rock.SystemGuid.Category.HISTORY_GROUP_CHANGES.AsGuid(), GroupId, GroupHistoryChanges, true, this.ModifiedByPersonAliasId );
            }

            // If this is a Group of type Family, update the ModifiedDateTime on the Persons that are members of this family (since one of their Addresses changed)
            int groupTypeIdFamily = GroupTypeCache.GetFamilyGroupType().Id;
            var groupService = new GroupService( rockContext );

            int groupTypeId = groupService.GetSelect( this.GroupId, s => s.GroupTypeId );
            if ( groupTypeId == groupTypeIdFamily )
            {
                var currentDateTime = RockDateTime.Now;
                var qryPersonsToUpdate = new GroupMemberService( rockContext ).Queryable().Where( a => a.GroupId == this.GroupId ).Select( a => a.Person );
                rockContext.BulkUpdate( qryPersonsToUpdate, p => new Person { ModifiedDateTime = currentDateTime, ModifiedByPersonAliasId = this.ModifiedByPersonAliasId } );
            }

            base.PostSaveChanges( dbContext );
        }

        /// <summary>
        /// Returns a <see cref="System.String" />  that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Group.ToStringSafe() + " at " + Location.ToStringSafe();
        }

        #endregion

    }
    
    #region Entity Configuration

    /// <summary>
    /// GroupLocation Configuration class
    /// </summary>
    public partial class GroupLocationConfiguration : EntityTypeConfiguration<GroupLocation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupLocationConfiguration"/> class.
        /// </summary>
        public GroupLocationConfiguration()
        {
            this.HasRequired( t => t.Group ).WithMany( t => t.GroupLocations ).HasForeignKey( t => t.GroupId );
            this.HasRequired( t => t.Location ).WithMany( l => l.GroupLocations).HasForeignKey( t => t.LocationId );
            this.HasOptional( t => t.GroupLocationTypeValue ).WithMany().HasForeignKey( t => t.GroupLocationTypeValueId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.GroupMemberPersonAlias ).WithMany().HasForeignKey( t => t.GroupMemberPersonAliasId ).WillCascadeOnDelete( true );
            this.HasMany( t => t.Schedules ).WithMany().Map( t => { t.MapLeftKey( "GroupLocationId" ); t.MapRightKey( "ScheduleId" ); t.ToTable( "GroupLocationSchedule" ); } );
        }
    }

    #endregion

}