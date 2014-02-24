// <copyright>
// Copyright 2013 by the Spark Development Network
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
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents an instance of when a <see cref="Rock.Model.Person">Person's</see> person detail data was viewed in Rock.  Includes data on who was viewed, the person who viewed their record, and when/where their record
    /// was viewed.
    /// </summary>
    [Table( "PersonViewed" )]
    [NotAudited]
    [DataContract]
    public partial class PersonViewed : Entity<PersonViewed>
    {
        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Person"/> that was the viewer.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Person"/> who was the viewer.
        /// </value>
        [DataMember]
        public int? ViewerPersonId { get; set; }
        
        /// <summary>
        /// Gets or sets the Id of the Target/Viewed <see cref="Rock.Model.Person"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Target/Viewed <see cref="Rock.Model.Person"/> 
        /// </value>
        [DataMember]
        public int? TargetPersonId { get; set; }
        
        /// <summary>
        /// Gets or sets the Date and Time that the that the person was viewed.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the time that the person was viewed.
        /// </value>
        [DataMember]
        public DateTime? ViewDateTime { get; set; }
        
        /// <summary>
        /// Gets or sets the IP address of the computer/device that requested the page view.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the IP address of the computer/device that requested the page view.
        /// </value>
        [MaxLength( 25 )]
        [DataMember]
        public string IpAddress { get; set; }
        
        /// <summary>
        /// Gets or sets the source of the view (site id or application name)
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the source of the View.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string Source { get; set; }
        
        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Person"/> entity of the viewer.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.Person"/> entity representing the viewer.
        /// </value>
        [DataMember]
        public virtual Person ViewerPerson { get; set; }
        
        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Person"/> entity of the individual who was viewed.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.Person"/> entity representing the person who was viewed.
        /// </value>
        [DataMember]
        public virtual Person TargetPerson { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (ViewerPerson != null && TargetPerson != null)
                return string.Format( "{0} Viewed {1}", ViewerPerson.ToStringSafe(), TargetPerson.ToStringSafe() );
            return string.Empty;
        }
    }

    /// <summary>
    /// Person Viewed Configuration class.
    /// </summary>
    public partial class PersonViewedConfiguration : EntityTypeConfiguration<PersonViewed>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonViewedConfiguration"/> class.
        /// </summary>
        public PersonViewedConfiguration()
        {
            this.HasOptional( p => p.ViewerPerson ).WithMany().HasForeignKey( p => p.ViewerPersonId ).WillCascadeOnDelete(false);
            this.HasOptional( p => p.TargetPerson ).WithMany().HasForeignKey( p => p.TargetPersonId ).WillCascadeOnDelete(false);
        }
    }
}
