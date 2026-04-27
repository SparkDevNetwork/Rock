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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Utility;

namespace Rock.Model
{
    /// <summary>
    /// Represents a child of a <see cref="KnowledgeBase"/> that is bound to a Rock content
    /// source. The polymorphic <see cref="SourceEntityTypeId"/> + <see cref="SourceKey"/>
    /// pattern lets a single folder represent different kinds of Rock content (content
    /// channels, Rocumentation books, etc.). New source types can be added without
    /// schema changes. Security is enforced at the parent <see cref="KnowledgeBase"/>
    /// level for v1; folders inherit security from the knowledge base.
    /// </summary>
    [RockDomain( "AI" )]
    [Table( "KnowledgeBaseFolder" )]
    [DataContract]
    [CodeGenerateRest]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.KNOWLEDGE_BASE_FOLDER )]
    public partial class KnowledgeBaseFolder : Model<KnowledgeBaseFolder>, IHasAdditionalSettings
    {
        #region Entity Properties

        /// <summary>
        /// The friendly name of the folder used to identify it in the UI.
        /// </summary>
        [Required]
        [MaxLength( 250 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// The long-form description of the folder.
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Optional context the folder can contribute at retrieval time. Combined with
        /// the parent <see cref="KnowledgeBase"/>'s context hint to give the LLM more
        /// information about what kind of content this folder contains.
        /// </summary>
        [DataMember]
        public string ContextHint { get; set; }

        /// <summary>
        /// The Id of the parent <see cref="Rock.Model.KnowledgeBase"/>.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int KnowledgeBaseId { get; set; }

        /// <summary>
        /// The Id of the <see cref="Rock.Model.EntityType"/> that identifies the
        /// polymorphic kind of content provided by this folder (for example, content
        /// channel or Rocumentation book). Nullable to allow folders that have not
        /// yet been bound to a source.
        /// </summary>
        [DataMember]
        public int? SourceEntityTypeId { get; set; }

        /// <summary>
        /// The identifier of the specific source entity within the source kind
        /// referenced by <see cref="SourceEntityTypeId"/>. For content channel sources
        /// this is the content channel Id; for Rocumentation sources this is the book
        /// Id. Nullable for the same reason as <see cref="SourceEntityTypeId"/>.
        /// </summary>
        [MaxLength( 250 )]
        [DataMember]
        public string SourceKey { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public string AdditionalSettingsJson { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// The parent <see cref="Rock.Model.KnowledgeBase"/>.
        /// </summary>
        [DataMember]
        public virtual KnowledgeBase KnowledgeBase { get; set; }

        /// <summary>
        /// The <see cref="Rock.Model.EntityType"/> that identifies the polymorphic kind
        /// of content provided by this folder.
        /// </summary>
        [DataMember]
        public virtual EntityType SourceEntityType { get; set; }

        /// <summary>
        /// A collection containing the <see cref="KnowledgeBaseDocument"/> entities that
        /// belong to this folder.
        /// </summary>
        [DataMember]
        public virtual ICollection<KnowledgeBaseDocument> KnowledgeBaseDocuments { get; set; } = new Collection<KnowledgeBaseDocument>();

        #endregion

        #region Public Methods

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// KnowledgeBaseFolder Configuration class.
    /// </summary>
    public partial class KnowledgeBaseFolderConfiguration : EntityTypeConfiguration<KnowledgeBaseFolder>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgeBaseFolderConfiguration"/> class.
        /// </summary>
        public KnowledgeBaseFolderConfiguration()
        {
            this.HasRequired( f => f.KnowledgeBase ).WithMany( kb => kb.KnowledgeBaseFolders ).HasForeignKey( f => f.KnowledgeBaseId ).WillCascadeOnDelete( true );
            this.HasOptional( f => f.SourceEntityType ).WithMany().HasForeignKey( f => f.SourceEntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}
