﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a physical or virtual Campus/Site for an organization.  
    /// </summary>
    [Table( "Campus" )]
    [DataContract]
    public partial class Campus : Model<Campus>
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
        /// Gets or sets the service times (Stored as a delimeted list)
        /// </summary>
        /// <value>
        /// The service times.
        /// </value>
        [DataMember]
        [MaxLength( 500 )]
        public string ServiceTimes { get; set; }

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
