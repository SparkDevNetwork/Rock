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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a snapshot of group location info at a point in history
    /// </summary>
    [RockDomain( "Group" )]
    [Table( "GroupLocationHistorical" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "03128778-5E7D-4FE4-9C7A-929936E06F90")]
    public partial class GroupLocationHistorical : Model<GroupLocationHistorical>, IHistoricalTracking
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the group location identifier that this is a Historical snapshot for
        /// </summary>
        /// <value>
        /// The group location identifier.
        /// </value>
        [DataMember]
        public int? GroupLocationId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Group"/> id for this group's location at this point in history
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        [DataMember]
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the group location type value identifier for this group location at this point in history
        /// </summary>
        /// <value>
        /// The group location type value identifier.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.GROUP_LOCATION_TYPE )]
        public int? GroupLocationTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the group's location type name at this point in history (Group.GroupLocation.GroupLocationTypeValue.Value)
        /// </summary>
        /// <value>
        /// The name of the group location type.
        /// </value>
        [MaxLength( 250 )]
        [DataMember]
        public string GroupLocationTypeName { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Location"/> id of this group's location at this point in history
        /// </summary>
        /// <value>
        /// The location identifier.
        /// </value>
        [DataMember]
        public int LocationId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Location"/> name of this group's location at this point in history (Group.GroupLocation.Location.ToString())
        /// </summary>
        /// <value>
        /// The location name.
        /// </value>
        [DataMember]
        public string LocationName { get; set; }

        /// <summary>
        /// Gets or sets the Location's ModifiedDateTime. This is used internally to detect if the group's location has changed
        /// </summary>
        /// <value>
        /// The schedule's iCalendarContent
        /// </value>
        [DataMember]
        public DateTime? LocationModifiedDateTime { get; set; }

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

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupLocation"/> that this is a historical snapshot for
        /// </summary>
        /// <value>
        /// The group location.
        /// </value>
        [DataMember]
        public virtual GroupLocation GroupLocation { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Group"/> for this group's location at this point in history
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        [DataMember]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Location"/> of this group's location at this point in history
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        [DataMember]
        public virtual Location Location { get; set; }

        /// <summary>
        /// Gets or sets the group location historical schedules.
        /// </summary>
        /// <value>
        /// The group location historical schedules.
        /// </value>
        [DataMember]
        [Obsolete( "The GroupLocationHistoricalSchedule entity and its usages will be removed." )]
        [RockObsolete( "1.16" )]
        public virtual ICollection<GroupLocationHistoricalSchedule> GroupLocationHistoricalSchedules { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    public partial class GroupLocationHistoricalConfiguration : EntityTypeConfiguration<GroupLocationHistorical>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupLocationHistoricalConfiguration"/> class.
        /// </summary>
        public GroupLocationHistoricalConfiguration()
        {
            this.HasOptional( p => p.GroupLocation ).WithMany().HasForeignKey( p => p.GroupLocationId ).WillCascadeOnDelete( true );
            this.HasRequired( p => p.Group ).WithMany().HasForeignKey( p => p.GroupId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.Location ).WithMany().HasForeignKey( p => p.LocationId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
