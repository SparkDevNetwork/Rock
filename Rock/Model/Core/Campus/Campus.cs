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
using Rock.Data;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// Represents a physical or virtual Campus/Site for an organization.  
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "Campus" )]
    [DataContract]
    [Analytics( false, true )]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.CAMPUS )]
    public partial class Campus : Model<Campus>, IOrdered, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if the Campus is a part of the Rock system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this Block is part of the Rock core system/framework, otherwise is <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the name of the Campus. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the Campus name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [Index( IsUnique = true )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the is active.
        /// </summary>
        /// <value>
        /// The is active.
        /// </value>
        [DataMember]
        public bool? IsActive { get; set; }

        /// <summary>
        /// Gets or sets an optional short code identifier for the campus.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> value that represents a short code identifier for a campus. If the campus does not have a ShortCode
        /// this value is null.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string ShortCode { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        [DataMember]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Location"/> that is associated with this campus. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> that represents the Id of the (physical) location of the campus. If none exists, this value is null.
        /// </value>
        [DataMember]
        public int? LocationId { get; set; }

        /// <summary>
        /// Gets or sets the phone number of the campus.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the campus phone number.
        /// </value>
        [DataMember]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Person"/> that is the leader of the campus.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> that represents the Id of the person who leads the campus.
        /// </value>
        [DataMember]
        public int? LeaderPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the service times (Stored as a delimited list)
        /// </summary>
        /// <value>
        /// The service times.
        /// </value>
        [DataMember]
        [MaxLength( 500 )]
        public string ServiceTimes { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the time zone identifier (<see cref="System.TimeZoneInfo.Id"/>)
        /// If this is not set, the Campus time zone will be the default Rock time zone (<see cref="Rock.RockDateTime.OrgTimeZoneInfo" /> )
        /// </summary>
        /// <value>
        /// The time zone identifier. 
        /// NOTE: System.TimeZoneInfo.Id can be any length, but documentation recommends that it 32 chars or less, so we'll limit it to 50
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string TimeZoneId { get; set; }

        /// <summary>
        /// Gets or sets the campus status value identifier.
        /// </summary>
        /// <value>
        /// The campus status value identifier.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.CAMPUS_STATUS )]
        public int? CampusStatusValueId { get; set; }

        /// <summary>
        /// Gets or sets the campus type value identifier.
        /// </summary>
        /// <value>
        /// The campus type value identifier.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.CAMPUS_TYPE )]
        public int? CampusTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the team group identifier.
        /// </summary>
        /// <value>
        /// The team group identifier.
        /// </value>
        [DataMember]
        public int? TeamGroupId { get; set; }

        /// <summary>
        /// Gets or sets the opened date.
        /// </summary>
        /// <value>
        /// The opened date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? OpenedDate { get; set; }

        /// <summary>
        /// Gets or sets the closed date.
        /// </summary>
        /// <value>
        /// The closed date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? ClosedDate { get; set; }

        /// <summary>
        /// Gets or sets the tithe metric.
        /// </summary>
        /// <value>
        /// The tithe metric.
        /// </value>
        [DataMember]
        [DecimalPrecision( 8, 2 )]
        public decimal? TitheMetric { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Location"/> entity that is associated with this campus.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Location"/> that is associated with this campus.
        /// </value>
        [DataMember]
        public virtual Location Location { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Person"/> entity that is associated with the leader of the campus.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Person"/> that is associated as the leader of the campus.
        /// </value>
        [DataMember]
        public virtual PersonAlias LeaderPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the campus status.
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object representing the campus status.
        /// </value>
        [DataMember]
        public virtual DefinedValue CampusStatusValue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the campus type.
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object representing the campus type
        /// </value>
        [DataMember]
        public virtual DefinedValue CampusTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the team group.
        /// </summary>
        /// <value>
        /// The team group.
        /// </value>
        [DataMember]
        public virtual Group TeamGroup { get; set; }

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.CampusSchedule">Schedules</see> that are associated with this Campus.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.CampusSchedule"/>s that are associated with this Campus.
        /// </value>
        [DataMember]
        public virtual ICollection<CampusSchedule> CampusSchedules { get; set; } = new Collection<CampusSchedule>();

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.CampusTopic">Topics</see> that are associated with this Campus.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.CampusTopic"/>s that are associated with this Campus.
        /// </value>
        [DataMember]
        public virtual ICollection<CampusTopic> CampusTopics { get; set; } = new Collection<CampusTopic>();

        #endregion Navigation Properties

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String"/> containing the Location's Name that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> containing the Location's Name that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion Public Methods
    }

    #region Entity Configuration

    /// <summary>
    /// Campus Configuration class.
    /// </summary>
    public partial class CampusConfiguration : EntityTypeConfiguration<Campus>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CampusConfiguration"/> class.
        /// </summary>
        public CampusConfiguration()
        {
            this.HasOptional( c => c.Location ).WithMany().HasForeignKey( c => c.LocationId ).WillCascadeOnDelete( false );
            this.HasOptional( c => c.LeaderPersonAlias ).WithMany().HasForeignKey( c => c.LeaderPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( c => c.TeamGroup ).WithMany().HasForeignKey( c => c.TeamGroupId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}
