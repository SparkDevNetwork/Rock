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
using System.Text;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Used to map a site and token to a specific url 
    /// </summary>
    [RockDomain( "CMS" )]
    [Table( "PageShortLink" )]
    [DataContract]
    public partial class PageShortLink : Model<PageShortLink>
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

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Site"/> that is associated with this PageShortLink.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Site"/> that this PageShortLink is associated with.
        /// </value>
        [LavaInclude]
        public virtual Site Site { get; set; }

        /// <summary>
        /// Gets the short link URL.
        /// </summary>
        /// <value>
        /// The short link URL.
        /// </value>
        [LavaInclude]
        public virtual string ShortLinkUrl
        {
            get
            {
                string domain = new SiteService( new RockContext() ).GetDefaultDomainUri( this.SiteId ).ToString();
                return domain.EnsureTrailingForwardslash() + this.Token;
            }
        }

        #endregion

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

        #endregion

        #region Static Properties/Methods 

        private static Random _random = new Random( Guid.NewGuid().GetHashCode() );
        private static char[] alphaCharacters = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        /// <summary>
        /// Gets a random token.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static string GetRandomToken( int length )
        {
            StringBuilder sb = new StringBuilder();
            int poolSize = alphaCharacters.Length;
            for ( int i = 0; i < length; i++ )
            {
                sb.Append( alphaCharacters[_random.Next( poolSize )] );
            }
            return sb.ToString();
        }

        #endregion

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
            this.HasRequired( p => p.Site ).WithMany().HasForeignKey( p => p.SiteId ).WillCascadeOnDelete(true);
        }
    }

    #endregion
}
