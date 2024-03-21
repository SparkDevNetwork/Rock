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

using Rock.Data;
using Rock.Lava;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// Represents a snippet
    /// </summary>
    [RockDomain( "Communication" )]
    [Table( "Snippet" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "93548852-201B-4EF6-AF27-BBF535A2CC2B" )]
    public partial class Snippet : Model<Snippet>, IOrdered
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the Snippet.
        /// </summary>
        /// <value>
        /// The name of the pipeline.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [MaxLength( 100 )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the snippet type identifier.
        /// </summary>
        [DataMember]
        public int SnippetTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this Snippet is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this Snippet is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>The content.</value>
        [DataMember]
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the owner <see cref="Rock.Model.PersonAlias"/> identifier.
        /// </summary>
        /// <value>
        /// The owner person alias identifier.
        /// </value>
        [DataMember]
        public int? OwnerPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the category identifier.
        /// </summary>
        /// <value>
        /// The category identifier.
        /// </value>
        [DataMember]
        public int? CategoryId { get; set; }

        #endregion Entity Properties

        #region Methods

        /// <summary>
        /// Returns a <see cref="string" /> that represents this snippet.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this snippet.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the owner <see cref="Rock.Model.PersonAlias"/>.
        /// </summary>
        /// <value>
        /// The owner person alias.
        /// </value>
        [LavaVisible]
        public virtual PersonAlias OwnerPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        [DataMember]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets or sets the type of the snippet.
        /// </summary>
        /// <value>
        /// The type of the snippet.
        /// </value>
        [DataMember]
        public virtual SnippetType SnippetType { get; set; }

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// Snippet Configuration class.
    /// </summary>
    public partial class SnippetConfiguration : EntityTypeConfiguration<Snippet>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnippetConfiguration" /> class.
        /// </summary>
        public SnippetConfiguration()
        {
            this.HasOptional( s => s.OwnerPersonAlias ).WithMany().HasForeignKey( s => s.OwnerPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( s => s.Category ).WithMany().HasForeignKey( s => s.CategoryId ).WillCascadeOnDelete( false );
            this.HasRequired( s => s.SnippetType ).WithMany( st => st.Snippets ).HasForeignKey( s => s.SnippetTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}
