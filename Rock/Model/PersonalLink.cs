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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// Personal Link Entity.
    /// </summary>
    [RockDomain( "CMS" )]
    [Table( "PersonalLink" )]
    [DataContract]
    public partial class PersonalLink : Model<PersonalLink>, IOrdered
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [DataMember]
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        [DataMember]
        [MaxLength( 2048 )]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the personal link section.
        /// </summary>
        /// <value>
        /// The personal link section.
        /// </value>
        [DataMember]
        public int SectionId { get; set; }

        #endregion

        #region IOrdered

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        [DataMember]
        public int Order { get; set; }

        #endregion IOrdered

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [LavaVisible]
        public virtual Rock.Model.PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the personal link section.
        /// </summary>
        /// <value>
        /// The personal link section.
        /// </value>
        [LavaVisible]
        public virtual PersonalLinkSection Section { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Personal Link Configuration class.
    /// </summary>
    public partial class PersonalLinkConfiguration : EntityTypeConfiguration<PersonalLink>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonalLinkConfiguration"/> class.
        /// </summary>
        public PersonalLinkConfiguration()
        {
            this.HasRequired( r => r.Section ).WithMany( r => r.PersonalLinks ).HasForeignKey( r => r.SectionId ).WillCascadeOnDelete( true );
            this.HasOptional( r => r.PersonAlias ).WithMany().HasForeignKey( r => r.PersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}