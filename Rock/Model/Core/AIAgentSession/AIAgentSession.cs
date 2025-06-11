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
    /// Represents an existing chat session for a specific agent and person.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "AIAgentSession" )]
    [DataContract]
    [CodeGenExclude( CodeGenFeature.DefaultRestController )] // Do not generate a v1 API controller.
    [CodeGenerateRest( DisableEntitySecurity = true )]
    [Rock.SystemGuid.EntityTypeGuid( "2415941a-8a3f-49fe-8039-db27096b0edf" )]
    public partial class AIAgentSession : Entity<AIAgentSession>, IHasAdditionalSettings
    {
        #region Entity Properties

        /// <summary>
        /// The identifier of the agent that the session is associated with.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int AIAgentId { get; set; }

        /// <summary>
        /// The identifier of the <see cref="PersonAlias"/> that owns this
        /// session. This is used to ensure that a person cannot view another
        /// person's chat history.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int PersonAliasId { get; set; }

        /// <summary>
        /// An optional name for the session. This can be used to help identify
        /// this session later. If no name is specified then the date should be
        /// used as a way to identify the session in the UI.
        /// </summary>
        [MaxLength( 100 )]
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// An optional identifier for the type of entity that this session
        /// is related to.
        /// </summary>
        [DataMember]
        public int? RelatedEntityTypeId { get; set; }

        /// <summary>
        /// An optional identifier for the specific entity that this session
        /// is related to. Multiple sessions can be related to the same entity.
        /// </summary>
        [DataMember]
        public int? RelatedEntityId { get; set; }

        /// <summary>
        /// The date and time the session was started. This will be set
        /// automatically when the session is created.
        /// </summary>
        [DataMember]
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// The date and time the session was last used. This should be updated
        /// whenever a new message is added to the session history table. This
        /// will be set automatically when the session is first created.
        /// </summary>
        [DataMember]
        public DateTime LastMessageDateTime { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public string AdditionalSettingsJson { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// The agent that the session is associated with.
        /// </summary>
        public virtual AIAgent AIAgent { get; set; }

        /// <summary>
        /// The the <see cref="PersonAlias"/> that owns this session. This is
        /// used to ensure that a person cannot view another person's chat
        /// history.
        /// </summary>
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// The type of the entity that this session is related to.
        /// </summary>
        public virtual EntityType RelatedEntityType { get; set; }

        /// <summary>
        /// A collection containing the <see cref="AIAgentSessionHistory" /> entities
        /// that represent the chat history records for this session.
        /// </summary>
        [DataMember]
        public virtual ICollection<AIAgentSessionHistory> AIAgentSessionHistories { get; set; } = new Collection<AIAgentSessionHistory>();

        /// <summary>
        /// A collection containing the <see cref="AIAgentSessionAnchor" /> entities
        /// that represent the anchors that provide context to the session.
        /// </summary>
        [DataMember]
        public virtual ICollection<AIAgentSessionAnchor> AIAgentSessionAnchors { get; set; } = new Collection<AIAgentSessionAnchor>();

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class AIAgentSessionConfiguration : EntityTypeConfiguration<AIAgentSession>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AIAgentSessionConfiguration"/> class.
        /// </summary>
        public AIAgentSessionConfiguration()
        {
            this.HasRequired( a => a.AIAgent ).WithMany().HasForeignKey( a => a.AIAgentId ).WillCascadeOnDelete( true );
            this.HasRequired( a => a.PersonAlias ).WithMany().HasForeignKey( a => a.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.RelatedEntityType ).WithMany().HasForeignKey( a => a.RelatedEntityTypeId ).WillCascadeOnDelete( true );
        }
    }

    #endregion Entity Configuration
}
