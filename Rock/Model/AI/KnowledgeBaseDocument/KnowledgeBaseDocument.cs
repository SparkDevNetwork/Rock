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
using Rock.Enums.AI;
using Rock.Utility;

namespace Rock.Model
{
    /// <summary>
    /// Represents a single piece of content within a <see cref="KnowledgeBaseFolder"/>
    /// that is sent to the indexing service (Ragie) for retrieval. One row per indexed
    /// source entity. Documents are not cached because volume can be high and rows
    /// mutate often as content syncs.
    /// </summary>
    [RockDomain( "AI" )]
    [Table( "KnowledgeBaseDocument" )]
    [DataContract]
    [CodeGenerateRest]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.KNOWLEDGE_BASE_DOCUMENT )]
    public partial class KnowledgeBaseDocument : Model<KnowledgeBaseDocument>, IHasAdditionalSettings
    {
        #region Entity Properties

        /// <summary>
        /// The friendly name of the document used to identify it in the UI.
        /// </summary>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// The name of the source entity in Rock (for example, a content channel item
        /// title or Rocumentation article name) cached for display so the UI can show
        /// provenance without an extra join.
        /// </summary>
        [MaxLength( 100 )]
        [DataMember]
        public string SourceName { get; set; }

        /// <summary>
        /// The Id of the parent <see cref="Rock.Model.KnowledgeBaseFolder"/>.
        /// </summary>
        [Required]
        [Index( "IX_KnowledgeBaseFolderId_SourceKey", IsUnique = true, Order = 1 )]
        [DataMember( IsRequired = true )]
        public int KnowledgeBaseFolderId { get; set; }

        /// <summary>
        /// The indexing service's identifier for this document. Populated after the
        /// service accepts the document. Used when reconciling status from the
        /// indexing service back to Rock.
        /// </summary>
        [Required]
        [MaxLength( 250 )]
        [Index]
        [DataMember( IsRequired = true )]
        public string DocumentKey { get; set; }

        /// <summary>
        /// The raw content sent to the indexing service. Stored locally for re-send
        /// and audit so re-syncs do not require re-fetching from the source.
        /// </summary>
        [DataMember]
        public string Content { get; set; }

        /// <summary>
        /// The source URL for this document if applicable. May be the Rock-side detail
        /// page URL or an external link.
        /// </summary>
        [MaxLength( 500 )]
        [DataMember]
        public string Url { get; set; }

        /// <summary>
        /// The Id of the <see cref="Rock.Model.BinaryFile"/> that contains the source
        /// content when it is a file (PDF, audio, etc.) rather than text.
        /// </summary>
        [DataMember]
        public int? BinaryFileId { get; set; }

        /// <summary>
        /// The Rock-side identifier of the source content within the parent folder's
        /// source type (for example, the content channel item Id or Rocumentation
        /// article Id). Indexed jointly with <see cref="KnowledgeBaseFolderId"/> for
        /// change-detection lookups; a given source entity may appear at most once
        /// within a folder.
        /// </summary>
        [Required]
        [MaxLength( 250 )]
        [Index( "IX_KnowledgeBaseFolderId_SourceKey", IsUnique = true, Order = 2 )]
        [DataMember( IsRequired = true )]
        public string SourceKey { get; set; }

        /// <summary>
        /// The lifecycle status of this document in the indexing service.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public IndexStatus IndexStatus { get; set; }

        /// <summary>
        /// The date and time the document was last successfully indexed.
        /// </summary>
        [DataMember]
        public DateTime? IndexDateTime { get; set; }

        /// <summary>
        /// Indicates whether the source content has changed since the last successful
        /// index. Drives the re-index queue. Defaults to <c>false</c>.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsIndexDirty { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public string AdditionalSettingsJson { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// The parent <see cref="Rock.Model.KnowledgeBaseFolder"/>.
        /// </summary>
        [DataMember]
        public virtual KnowledgeBaseFolder KnowledgeBaseFolder { get; set; }

        /// <summary>
        /// The <see cref="Rock.Model.BinaryFile"/> that contains the source content
        /// when it is a file rather than text.
        /// </summary>
        [DataMember]
        public virtual BinaryFile BinaryFile { get; set; }

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
    /// KnowledgeBaseDocument Configuration class.
    /// </summary>
    public partial class KnowledgeBaseDocumentConfiguration : EntityTypeConfiguration<KnowledgeBaseDocument>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgeBaseDocumentConfiguration"/> class.
        /// </summary>
        public KnowledgeBaseDocumentConfiguration()
        {
            this.HasRequired( d => d.KnowledgeBaseFolder ).WithMany( f => f.KnowledgeBaseDocuments ).HasForeignKey( d => d.KnowledgeBaseFolderId ).WillCascadeOnDelete( true );
            this.HasOptional( d => d.BinaryFile ).WithMany().HasForeignKey( d => d.BinaryFileId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}
