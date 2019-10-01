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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a physical or virtual Campus/Site for an organization.  
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "Campus" )]
    [DataContract]
    [Analytics( false, true )]
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

        #endregion

        #region Virtual Properties

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
        /// Gets the current date time.
        /// </summary>
        /// <value>
        /// The current date time.
        /// </value>
        [NotMapped]
        public virtual DateTime CurrentDateTime
        {
            get
            {
                if ( TimeZoneId.IsNotNullOrWhiteSpace() )
                {
                    var campusTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById( TimeZoneId );
                    if ( campusTimeZoneInfo != null )
                    {
                        return TimeZoneInfo.ConvertTime( DateTime.UtcNow, campusTimeZoneInfo );
                    }
                }

                return RockDateTime.Now;
            }
        }


        #endregion

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

        #endregion

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return CampusCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            CampusCache.UpdateCachedEntity( this.Id, entityState );
        }

        #endregion
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
        }
    }

    #endregion

}
