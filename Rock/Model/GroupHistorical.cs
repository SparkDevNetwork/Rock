﻿// <copyright>
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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a snapshot of some of the group's values at a point in history
    /// </summary>
    [RockDomain( "Group" )]
    [Table( "GroupHistorical" )]
    [DataContract]
    public class GroupHistorical : Model<GroupHistorical>, IHistoricalTracking
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the group id of the group for this group historical record
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        [DataMember]
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the name of the group at this point in history
        /// </summary>
        /// <value>
        /// The name of the group.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets the group type identifier. Normally, a GroupTypeId can't be changed, but just in case, this will be the group type at this point in history
        /// </summary>
        /// <value>
        /// The group type identifier.
        /// </value>
        [DataMember]
        public int GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the name of the group type at this point in history
        /// </summary>
        /// <value>
        /// The name of the group type.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string GroupTypeName { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier for this group at this point in history
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        [DataMember]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the parent group identifier at this point in history
        /// </summary>
        /// <value>
        /// The parent group identifier.
        /// </value>
        [DataMember]
        public int? ParentGroupId { get; set; }

        /// <summary>
        /// Gets or sets the description for this group at this point in history
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Group Status Id.  DefinedType depends on this group's <see cref="Rock.Model.GroupType.GroupStatusDefinedType"/>
        /// </summary>
        /// <value>
        /// The status value identifier.
        /// </value>
        [DataMember]
        public int? StatusValueId { get; set; }

        /// <summary>
        /// If this group's group type supports a schedule for a group, this is the schedule id for that group at this point in history
        /// NOTE: If this Group has Schedules at it's Locations, those will be in GroupLocationHistorical.GroupLocationHistoricalSchedules
        /// </summary>
        /// <value>
        /// The schedule identifier.
        /// </value>
        [DataMember]
        public int? ScheduleId { get; set; }

        /// <summary>
        /// If this group's group type supports a schedule for a group, this is the schedule text (Schedule.ToString()) for that group at this point in history
        /// NOTE: If this Group has Schedules at it's Locations, those will be in GroupLocationHistorical.GroupLocationHistoricalSchedules
        /// </summary>
        /// <value>
        /// The schedule name.
        /// </value>
        [DataMember]
        public string ScheduleName { get; set; }

        /// <summary>
        /// Gets or sets the Schedule's ModifiedDateTime. This is used internally to detect if the group's schedule has changed
        /// </summary>
        /// <value>
        /// The schedule's iCalendarContent
        /// </value>
        [DataMember]
        public DateTime? ScheduleModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this group was archived at this point in history
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is archived; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsArchived { get; set; }

        /// <summary>
        /// Gets or sets the archived date time value of this group at this point in history
        /// </summary>
        /// <value>
        /// The archived date time.
        /// </value>
        [DataMember]
        public DateTime? ArchivedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the PersonAliasId that archived (soft deleted) this group at this point in history
        /// </summary>
        /// <value>
        /// The archived by person alias identifier.
        /// </value>
        [DataMember]
        public int? ArchivedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this group had IsActive==True at this point in history
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is inactive; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the InActiveDateTime value of the group at this point in history
        /// </summary>
        /// <value>
        /// The in active date time.
        /// </value>
        [DataMember]
        public DateTime? InactiveDateTime { get; set; }

        #endregion

        #region IHistoricalTracking

        /// <summary>
        /// Gets or sets the effective date.
        /// This is the starting date that the tracked record had the values reflected in this record
        /// </summary>
        /// <value>
        /// The effective date.
        /// </value>
        [DataMember]
        public DateTime EffectiveDateTime { get; set; }

        /// <summary>
        /// Gets or sets the expire date time
        /// This is the last date that the tracked record had the values reflected in this record
        /// For example, if a tracked record's Name property changed on '2016-07-14', the ExpireDate of the previously current record will be '2016-07-13', and the EffectiveDate of the current record will be '2016-07-14'
        /// If this is most current record, the ExpireDate will be '9999-01-01'
        /// </summary>
        /// <value>
        /// The expire date.
        /// </value>
        [DataMember]
        public DateTime ExpireDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [current row indicator].
        /// This will be True if this represents the same values as the current tracked record for this
        /// </summary>
        /// <value>
        ///   <c>true</c> if [current row indicator]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool CurrentRowIndicator { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the group for this group historical record
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        [DataMember]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the parent group of this group at this point in history
        /// </summary>
        /// <value>
        /// The parent group.
        /// </value>
        [DataMember]
        public virtual Group ParentGroup { get; set; }

        /// <summary>
        /// Gets or sets the group type of this group at this point in history
        /// </summary>
        /// <value>
        /// The type of the group.
        /// </value>
        [DataMember]
        public virtual GroupType GroupType { get; set; }

        /// <summary>
        /// If this group's group type supports a schedule for a group, this is the schedule for that group at this point in history
        /// NOTE: If this Group has Schedules at it's Locations, those will be in GroupLocationHistorical[n].GroupLocationHistoricalSchedules
        /// </summary>
        /// <value>
        /// The schedule identifier.
        /// </value>
        [DataMember]
        public virtual Schedule Schedule { get; set; }

        /// <summary>
        /// Gets or sets the campus of this group at this point in history
        /// </summary>
        /// <value>
        /// The campus.
        /// </value>
        [DataMember]
        public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets the PersonAlias that archived (soft deleted) this group at this point in history
        /// </summary>
        /// <value>
        /// The archived by person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias ArchivedByPersonAlias { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a GroupHistorical with CurrentRowIndicator = true for the specified group
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="effectiveDateTime">The effective date time.</param>
        /// <returns></returns>
        public static GroupHistorical CreateCurrentRowFromGroup( Group group, DateTime effectiveDateTime )
        {
            var groupHistoricalCurrent = new GroupHistorical
            {
                GroupId = group.Id,
                GroupName = group.Name,
                GroupTypeId = group.GroupTypeId,
                GroupTypeName = group.GroupType.Name,
                CampusId = group.CampusId,
                ParentGroupId = group.ParentGroupId,
                ScheduleId = group.ScheduleId,
                ScheduleName = group.Schedule?.ToString(),
                ScheduleModifiedDateTime = group.Schedule?.ModifiedDateTime,
                Description = group.Description,
                StatusValueId = group.StatusValueId,
                IsArchived = group.IsArchived,
                ArchivedDateTime = group.ArchivedDateTime,
                ArchivedByPersonAliasId = group.ArchivedByPersonAliasId,
                IsActive = group.IsActive,
                InactiveDateTime = group.InactiveDateTime,

                // Set the Modified/Created fields for GroupHistorical to be the current values from Group table
                ModifiedDateTime = group.ModifiedDateTime,
                ModifiedByPersonAliasId = group.ModifiedByPersonAliasId,
                CreatedByPersonAliasId = group.CreatedByPersonAliasId,
                CreatedDateTime = group.CreatedDateTime,

                // Set HistoricalTracking fields
                CurrentRowIndicator = true,
                EffectiveDateTime = effectiveDateTime,
                ExpireDateTime = HistoricalTracking.MaxExpireDateTime
            };

            return groupHistoricalCurrent;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    public partial class GroupHistoricalConfiguration : EntityTypeConfiguration<GroupHistorical>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupHistoricalConfiguration"/> class.
        /// </summary>
        public GroupHistoricalConfiguration()
        {
            this.HasRequired( p => p.Group ).WithMany().HasForeignKey( p => p.GroupId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.GroupType ).WithMany().HasForeignKey( p => p.GroupTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.ParentGroup ).WithMany().HasForeignKey( p => p.ParentGroupId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.Schedule ).WithMany().HasForeignKey( p => p.ScheduleId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.Campus ).WithMany().HasForeignKey( p => p.CampusId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.ArchivedByPersonAlias ).WithMany().HasForeignKey( p => p.ArchivedByPersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
