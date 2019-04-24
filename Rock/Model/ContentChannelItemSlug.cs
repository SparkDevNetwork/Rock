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
    /// 
    /// </summary>
    [RockDomain( "CMS" )]
    [Table( "ContentChannelItemSlug" )]
    [DataContract]
    public partial class ContentChannelItemSlug : Model<ContentChannelItemSlug>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the content channel item identifier.
        /// </summary>
        /// <value>
        /// The content channel item identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [IgnoreCanDelete]
        public int ContentChannelItemId { get; set; }

        /// <summary>
        /// Gets or sets the slug.
        /// </summary>
        /// <value>
        /// The slug.
        /// </value>
        [MaxLength( 75 )]
        [DataMember]
        public string Slug { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating whether or not the slug is primary.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the slug is primary; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsPrimary { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the content channel item.
        /// </summary>
        /// <value>
        /// The content channel item.
        /// </value>
        [DataMember]
        public virtual ContentChannelItem ContentChannelItem { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Slug;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class ContentChannelItemSlugConfiguration : EntityTypeConfiguration<ContentChannelItemSlug>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentChannelItemSlugConfiguration" /> class.
        /// </summary>
        public ContentChannelItemSlugConfiguration()
        {
            this.HasRequired( i => i.ContentChannelItem ).WithMany( c => c.ContentChannelItemSlugs ).HasForeignKey( i => i.ContentChannelItemId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
