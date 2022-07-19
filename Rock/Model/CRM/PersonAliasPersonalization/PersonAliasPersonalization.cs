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
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// The PersonAlias personalization
    /// </summary>
    [RockDomain( "CRM" )]
    [Table( "PersonAliasPersonalization" )]
    [DataContract]
    public class PersonAliasPersonalization
    {
        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [DataMember]
        [Key]
        [DatabaseGenerated( DatabaseGeneratedOption.None )]
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the type of the personalization.
        /// </summary>
        /// <value>
        /// The type of the personalization.
        /// </value>
        [DataMember]
        [Key]
        [DatabaseGenerated( DatabaseGeneratedOption.None )]
        public PersonalizationType PersonalizationType { get; set; }

        /// <summary>
        /// Gets or sets the ID of the Entity based on the <see cref="PersonalizationType"/> the <see cref="PersonalizationEntityId"/>.
        /// Can be an identifier for <see cref="PersonalizationSegment"/> or a <see cref="RequestFilter"/>.
        /// </summary>
        /// <value>
        /// The personalization type identifier.
        /// </value>
        [DataMember]
        [Key]
        [DatabaseGenerated( DatabaseGeneratedOption.None )]
        public int PersonalizationEntityId { get; set; }

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>The person alias.</value>
        [DataMember]
        public virtual PersonAlias PersonAlias { get; set; }

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// PersonAlias Personalization Configuration class.
    /// </summary>
    public class PersonAliasPersonalizationConfiguration : EntityTypeConfiguration<PersonAliasPersonalization>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonAliasPersonalizationConfiguration"/> class.
        /// </summary>
        public PersonAliasPersonalizationConfiguration()
        {
            HasKey( a => new { a.PersonAliasId, a.PersonalizationType, a.PersonalizationEntityId } );
            HasRequired( a => a.PersonAlias ).WithMany().HasForeignKey( a => a.PersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
