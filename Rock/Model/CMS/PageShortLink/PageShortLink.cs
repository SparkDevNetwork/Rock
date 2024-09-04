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
using Rock.Attribute;
using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// Used to map a site and token to a specific URL. 
    /// </summary>
    [RockDomain( "CMS" )]
    [Table( "PageShortLink" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "83D8C6DF-1D53-438B-93B2-75A2038BBEE6" )]
    public partial class PageShortLink : Model<PageShortLink>, IHasAdditionalSettings
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Site"/> that this PageShortLink references. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> containing the Id of the <see cref="Rock.Model.Site"/> that this PageShortLink references.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int SiteId { get; set; }

        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public string Url { get; set; }

        /// <inheritdoc/>
        [RockInternal( "1.16.6" )]
        [DataMember]
        public string AdditionalSettingsJson { get; set; }

        /// <summary>
        /// Gets a flag that determines if this short link has schedules enabled.
        /// When <c>true</c>, the schedule details will be contained in the
        /// additional settings.
        /// </summary>
        [DataMember]
        public bool IsScheduled
        {
            get => HasSchedules();
            private set { /* Make EF happy */ }
        }

        /// <summary>
        /// Gets or sets the identifier of the category this short link is for.
        /// Categories are used for reporting purposes only. They do not affect
        /// the ability to use a short link.
        /// </summary>
        [DataMember]
        public int? CategoryId { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Site"/> that is associated with this PageShortLink.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Site"/> that this PageShortLink is associated with.
        /// </value>
        [LavaVisible]
        public virtual Site Site { get; set; }

        /// <summary>
        /// Gets or sets the category this short link is for. Categories are used
        /// for reporting purposes only. They do not affect the ability to use a
        /// short link.
        /// </summary>
        [LavaVisible]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets the short link URL.
        /// </summary>
        /// <value>
        /// The short link URL.
        /// </value>
        [LavaVisible]
        public virtual string ShortLinkUrl
        {
            get
            {
                string domain = new SiteService( new RockContext() ).GetDefaultDomainUri( this.SiteId ).ToString();
                return domain.EnsureTrailingForwardslash() + this.Token;
            }
        }

        #endregion Navigation Properties

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> containing the token name and represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> containing the token name  that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Token;
        }

        #endregion Methods
    }

    #region Entity Configuration

    /// <summary>
    /// Site UrlMap Configuration class.
    /// </summary>
    public partial class PageShortLinkConfiguration : EntityTypeConfiguration<PageShortLink>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageShortLinkConfiguration"/> class.
        /// </summary>
        public PageShortLinkConfiguration()
        {
            this.HasRequired( p => p.Site ).WithMany().HasForeignKey( p => p.SiteId ).WillCascadeOnDelete( true );
            this.HasOptional( p => p.Category ).WithMany().HasForeignKey( p => p.CategoryId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}
