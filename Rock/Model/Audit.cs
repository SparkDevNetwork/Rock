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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents an Audit Log entry that is created when an add/update/delete is performed against an <see cref="Rock.Data.IEntity"/> of an
    /// auditable <see cref="Rock.Model.EntityType"/>.
    /// </summary>
    [RockDomain( "Core" )]
    [NotAudited]
    [Table( "Audit" )]
    [DataContract]
    [HideFromReporting]
    public partial class Audit : Entity<Audit>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the EntityTypeId for the <see cref="Rock.Model.EntityType"/> of entity that was modified. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId for the <see cref="Rock.Model.EntityType"/> of the entity that was modified.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the specific entity that was modified. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the entity that was modified.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the Name/Title of the specific entity that was updated. This is usually the value that is return when the entity's ToString() function is called. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the Entity's Name.
        /// </value>
        [Required]
        [MaxLength( 200 )]
        [DataMember( IsRequired = true )]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the type of change that was made to the entity. This property is required.
        /// </summary>
        /// <value>
        /// The type of the audit.
        /// A <see cref="Rock.Model.AuditType"/> enumeration that indicates the type of change that was made. 
        ///     <c>AuditType.Add</c> (0) indicates that a new entity was added; 
        ///     <c>AuditType.Modify</c> (1) indicates that the entity was modified; 
        ///     <c>AuditType.Delete</c> (2) indicates that the entity was deleted.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public AuditType AuditType { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the entity was modified and the audit entry was created.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> that represents the date and time that the entity was modified.
        /// </value>
        [DataMember]
        public DateTime? DateTime { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [DataMember]
        public int? PersonAliasId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EntityType"/> of the entity that was modified.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> of the entity that was modified.
        /// </value>
        [DataMember]
        public virtual Model.EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [LavaInclude]
        public virtual Rock.Model.PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the details.
        /// </summary>
        /// <value>
        /// The details.
        /// </value>
        [DataMember]
        public virtual ICollection<AuditDetail> Details
        {
            get { return _details; }
            set { _details = value; }
        }
        private ICollection<AuditDetail> _details;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Audit"/> class.
        /// </summary>
        public Audit() : base()
        {
            Details = new Collection<AuditDetail>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Title;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Entity Change Configuration class.
    /// </summary>
    public partial class AuditConfiguration : EntityTypeConfiguration<Audit>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityTypeConfiguration" /> class.
        /// </summary>
        public AuditConfiguration()
        {
            this.HasOptional( p => p.PersonAlias ).WithMany().HasForeignKey( p => p.PersonAliasId ).WillCascadeOnDelete( true );
            this.HasRequired( p => p.EntityType ).WithMany().HasForeignKey( p => p.EntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// Type of audit done to an entity
    /// </summary>
    public enum AuditType
    {
        /// <summary>
        /// Add
        /// </summary>
        Add = 0,

        /// <summary>
        /// Modify
        /// </summary>
        Modify = 1,

        /// <summary>
        /// Delete
        /// </summary>
        Delete = 2
    }

    #endregion

}
