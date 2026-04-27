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
    /// Represents a top-level knowledge base in Rock. A knowledge base is a coarse subject
    /// grouping (for example "Policies and Procedures" or "Sermon Transcriptions") that
    /// organizes one or more <see cref="KnowledgeBaseFolder"/> entities, each bound to a
    /// specific Rock content source. Security is enforced at the knowledge base level for
    /// all child folders and documents.
    /// </summary>
    [RockDomain( "AI" )]
    [Table( "KnowledgeBase" )]
    [DataContract]
    [CodeGenerateRest]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.KNOWLEDGE_BASE )]
    public partial class KnowledgeBase : Model<KnowledgeBase>, IHasAdditionalSettings
    {
        #region Entity Properties

        /// <summary>
        /// The friendly name of the knowledge base used to identify it in the UI.
        /// </summary>
        [Required]
        [MaxLength( 250 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// The long-form description of the knowledge base.
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Optional context that should be passed to retrieval or LLM prompts to help
        /// describe what kind of content this knowledge base contains (for example,
        /// "These documents are sermon transcriptions from City Church").
        /// </summary>
        [DataMember]
        public string ContextHint { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public string AdditionalSettingsJson { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// A collection containing the <see cref="KnowledgeBaseFolder"/> entities that
        /// belong to this knowledge base.
        /// </summary>
        [DataMember]
        public virtual ICollection<KnowledgeBaseFolder> KnowledgeBaseFolders { get; set; } = new Collection<KnowledgeBaseFolder>();

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
    /// KnowledgeBase Configuration class.
    /// </summary>
    public partial class KnowledgeBaseConfiguration : EntityTypeConfiguration<KnowledgeBase>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgeBaseConfiguration"/> class.
        /// </summary>
        public KnowledgeBaseConfiguration()
        {
        }
    }

    #endregion Entity Configuration
}
