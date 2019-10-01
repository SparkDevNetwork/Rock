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
    [Table( "AuditDetail" )]
    [DataContract]
    [HideFromReporting]
    public partial class AuditDetail : Entity<AuditDetail>
    {

        #region EntityProperties

        /// <summary>
        /// Gets or sets the EntityTypeId for the <see cref="Rock.Model.EntityType"/> of entity that was modified. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId for the <see cref="Rock.Model.EntityType"/> of the entity that was modified.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int AuditId { get; set; }

        /// <summary>
        /// Gets or sets the Property.
        /// </summary>
        /// <value>
        /// Property.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Property { get; set; }

        /// <summary>
        /// Gets or sets the Original Value.
        /// </summary>
        /// <value>
        /// Original Value.
        /// </value>
        [DataMember]
        public string OriginalValue { get; set; }

        /// <summary>
        /// Gets or sets the Current Value.
        /// </summary>
        /// <value>
        /// Current Value.
        /// </value>
        [DataMember]
        public string CurrentValue { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Audit"/> parent entity.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Audit"/>
        /// </value>
        [LavaInclude]
        public virtual Model.Audit Audit { get; set; }

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
            return this.CurrentValue;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Entity Change Configuration class.
    /// </summary>
    public partial class AuditDetailConfiguration : EntityTypeConfiguration<AuditDetail>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityTypeConfiguration" /> class.
        /// </summary>
        public AuditDetailConfiguration()
        {
            this.HasRequired( p => p.Audit ).WithMany( a => a.Details ).HasForeignKey( p => p.AuditId).WillCascadeOnDelete( true );
        }
    }

    #endregion

}
