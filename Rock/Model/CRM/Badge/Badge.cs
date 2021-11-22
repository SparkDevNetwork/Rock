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
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a badge.
    /// </summary>
    [RockDomain( "CRM" )]
    [Table( "Badge" )]
    [DataContract]
    public partial class Badge : Model<Badge>, IOrdered, ICacheable, IHasActiveFlag
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the given Name of the badge. This value is an alternate key and is required.
        /// </summary>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description of the badge.
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Id of the badge component entity type
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int BadgeComponentEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that this Badge describes.
        /// </summary>
        [DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the entity type qualifier column that contains the value (see <see cref="EntityTypeQualifierValue"/>) that is used narrow the scope of the Badge to a subset or specific instance of an EntityType.
        /// </summary>
        [MaxLength( 50 )]
        [DataMember]
        public string EntityTypeQualifierColumn { get; set; }

        /// <summary>
        /// Gets or sets the entity type qualifier value that is used to narrow the scope of the Badge to a subset or specific instance of an EntityType.
        /// </summary>
        [MaxLength( 200 )]
        [DataMember]
        public string EntityTypeQualifierValue { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this item is active or not.
        /// </summary>
        [DataMember]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        [DataMember]
        public int Order { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the badge component <see cref="Rock.Model.EntityType"/>.
        /// </summary>
        [DataMember]
        public virtual EntityType BadgeComponentEntityType { get; set; }

        /// <summary>
        /// Gets or sets the subject entity <see cref="Rock.Model.EntityType"/>.
        /// </summary>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        #endregion Navigation Properties

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion Methods
    }

    #region Entity Configuration

    /// <summary>
    /// File Configuration class.
    /// </summary>
    public partial class BadgeConfiguration : EntityTypeConfiguration<Badge>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BadgeConfiguration"/> class.
        /// </summary>
        public BadgeConfiguration()
        {
            HasRequired( b => b.BadgeComponentEntityType ).WithMany().HasForeignKey( b => b.BadgeComponentEntityTypeId ).WillCascadeOnDelete( false );
            HasRequired( b => b.EntityType ).WithMany().HasForeignKey( b => b.EntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
