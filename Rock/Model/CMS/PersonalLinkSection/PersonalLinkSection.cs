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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Lava;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// This represents a section that a link can belong to.
    /// </summary>
    [RockDomain( "CMS" )]
    [Table( "PersonalLinkSection" )]
    [DataContract]
    public partial class PersonalLinkSection : Model<PersonalLinkSection>, ICacheable
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
        /// Gets or sets a flag indicating if this Link Section is shared.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this Link Section is shared; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsShared { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the Pre Save Changes Person Alias Identifier.
        /// </summary>
        /// <value>
        /// The Pre Save Changes Person Alias Identifier.
        /// </value>
        [NotMapped]
        protected int? PreSaveChangesPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets if presave changes person is shared.
        /// </summary>
        [NotMapped]
        protected bool PreSaveChangesIsShared { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [LavaVisible]
        public virtual Rock.Model.PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the personal links.
        /// </summary>
        /// <value>
        /// The personal links.
        /// </value>
        [DataMember]
        public virtual ICollection<PersonalLink> PersonalLinks
        {
            get { return _personalLinks ?? ( _personalLinks = new Collection<PersonalLink>() ); }
            set { _personalLinks = value; }
        }

        private ICollection<PersonalLink> _personalLinks;

        /// <summary>
        /// Gets or sets the personal links.
        /// </summary>
        /// <value>
        /// The personal links.
        /// </value>
        [DataMember]
        public virtual ICollection<PersonalLinkSectionOrder> PersonalLinkSectionOrders
        {
            get { return _personalLinkSectionOrders ?? ( _personalLinkSectionOrders = new Collection<PersonalLinkSectionOrder>() ); }
            set { _personalLinkSectionOrders = value; }
        }

        private ICollection<PersonalLinkSectionOrder> _personalLinkSectionOrders;

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion Overrides
    }

    #region Entity Configuration

    /// <summary>
    /// Personal Link Section Configuration class.
    /// </summary>
    public partial class PersonalLinkSectionConfiguration : EntityTypeConfiguration<PersonalLinkSection>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonalLinkSectionConfiguration"/> class.
        /// </summary>
        public PersonalLinkSectionConfiguration()
        {
            this.HasOptional( r => r.PersonAlias ).WithMany().HasForeignKey( r => r.PersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
