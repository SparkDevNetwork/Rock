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
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Utility;

namespace Rock.Model
{
    /// <summary>
    /// Represents a preferences an individual has indicated. This is most often
    /// used for preferences related to blocks, but they can be associated with
    /// any entity.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "PersonPreference" )]
    [CodeGenExclude( CodeGenFeature.DefaultRestController )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.PERSON_PREFERENCE )]
    public partial class PersonPreference : Entity<PersonPreference>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="PersonAlias"/> that
        /// owns this preference.
        /// </summary>
        /// <value>
        /// An <see cref="int"/> representing the Id of a <see cref="PersonAlias"/>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Index( "IX_PersonAliasIdKey", 0, IsUnique = true )]
        public int PersonAliasId { get; set; }

        /// <summary>
        /// <para>
        /// Gets or sets the unique key that identifies this preference. This
        /// is unique to each related <see cref="PersonAliasId"/>.
        /// </para>
        /// <para>
        /// The key should always follow the pattern of <c>{entity-type-slug}-{entity-id}-{user-key}</c>.
        /// For example, a block preference might look like <c>block-283-show-inactive</c>.
        /// </para>
        /// <para>
        /// In the case of a preference not attached to any entity it should
        /// follow the pattern of <c>global-0-{user-key}</c>. For example,
        /// a global person preference might look like <c>global-0-default-grid-page-size</c>.
        /// </para>
        /// </summary>
        /// <value>The key that identifies this preference.</value>
        [Required]
        [MaxLength( 250 )]
        [DataMember( IsRequired = true )]
        [Index( "IX_PersonAliasIdKey", 1, IsUnique = true )]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="EntityType"/> that this
        /// preference is associated with. This is used to limit the preferences
        /// that are automatically loaded to only the ones related to the request.
        /// </summary>
        /// <value>The entity type identifier.</value>
        [DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="IEntity"/> that this
        /// preference is associated with. This is used to limit the preferences
        /// that are automatically loaded to only the ones related to the request.
        /// </summary>
        /// <value>The entity identifier.</value>
        [DataMember]
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the preference value.
        /// </summary>
        /// <value>The preference value.</value>
        [DataMember]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this preference is should
        /// have an extended life. Enduring preferences have a life of 18 months
        /// since last accessed.
        /// </summary>
        /// <value><c>true</c> if this preference is enduring; otherwise, <c>false</c>.</value>
        public bool IsEnduring { get; set; }

        /// <summary>
        /// Gets or sets the date this preference was last accessed by the owner.
        /// This should only be updated once per day.
        /// </summary>
        /// <value>
        /// The date this preference was last accessed by the owner.
        /// </value>
        [DataMember]
        public DateTime LastAccessedDateTime { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="PersonAlias"/> that owns this preference.
        /// </summary>
        /// <value>
        /// The <see cref="PersonAlias"/> that owns this preference.
        /// </value>
        [DataMember]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the the <see cref="EntityType"/> that this preference
        /// is associated with.
        /// </summary>
        /// <value>The entity type identifier.</value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// PersonPreference Configuration class.
    /// </summary>
    public partial class PersonPreferenceConfiguration : EntityTypeConfiguration<PersonPreference>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonPreferenceConfiguration"/> class.
        /// </summary>
        public PersonPreferenceConfiguration()
        {
            this.HasRequired( pp => pp.PersonAlias ).WithMany().HasForeignKey( pp => pp.PersonAliasId ).WillCascadeOnDelete( true );
            this.HasOptional( pp => pp.EntityType ).WithMany().HasForeignKey( pp => pp.EntityTypeId ).WillCascadeOnDelete( true );
        }
    }

    #endregion
}
