// <copyright>
// Copyright by Central Christian Church
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using com.centralaz.Baptism.Data;
using Rock;
using Rock.Data;
using Rock.Model;
namespace com.centralaz.Baptism.Model
{
    /// <summary>
    /// A Baptizee
    /// </summary>
    [Table( "_com_centralaz_Baptism_Baptizee" )]
    [DataContract]
    public class Baptizee : Rock.Data.Model<Baptizee>
    {

        #region Entity Properties

        /// <summary>
        /// The Group Id
        /// </summary>
        [DataMember]
        public int GroupId { get; set; }

        /// <summary>
        /// The Id of the baptizee
        /// </summary>
        [DataMember]
        public int PersonAliasId { get; set; }

        /// <summary>
        /// The Id of the first Baptizer
        /// </summary>
        [DataMember]
        public int? Baptizer1AliasId { get; set; }

        /// <summary>
        /// The Id of the second Baptizer
        /// </summary>
        [DataMember]
        public int? Baptizer2AliasId { get; set; }

        /// <summary>
        /// The Id of the approver of the baptism
        /// </summary>
        [DataMember]
        public int? ApproverAliasId { get; set; }

        /// <summary>
        /// The bool for whether the baptism is confirmed or not
        /// </summary>
        [DataMember]
        public bool IsConfirmed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is deleted.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is deleted; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is private.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is private; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsPrivateBaptism { get; set; }

        /// <summary>
        /// The baptism time
        /// </summary>
        [DataMember]
        public DateTime BaptismDateTime { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// The Group
        /// </summary>
        public virtual Group Group { get; set; }

        /// <summary>
        /// The Baptizee
        /// </summary>
        public virtual PersonAlias Person { get; set; }

        /// <summary>
        /// The first baptizer of the baptizee
        /// </summary>
        public virtual PersonAlias Baptizer1 { get; set; }

        /// <summary>
        /// The second baptizer of the baptizee
        /// </summary>
        public virtual PersonAlias Baptizer2 { get; set; }

        /// <summary>
        /// The approver of the baptism
        /// </summary>
        public virtual PersonAlias Approver { get; set; }

        #endregion

    }

    #region Entity Configuration


    public partial class BaptizeeConfiguration : EntityTypeConfiguration<Baptizee>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaptizeeConfiguration"/> class.
        /// </summary>
        public BaptizeeConfiguration()
        {
            this.HasRequired( r => r.Group ).WithMany().HasForeignKey( r => r.GroupId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.Person ).WithMany().HasForeignKey( r => r.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.Baptizer1 ).WithMany().HasForeignKey( r => r.Baptizer1AliasId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.Baptizer2 ).WithMany().HasForeignKey( r => r.Baptizer2AliasId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.Approver ).WithMany().HasForeignKey( r => r.ApproverAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}
