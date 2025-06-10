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
using Rock.Utility;

namespace Rock.Model
{
    /// <summary>
    /// Represents an existing chat session for a specific agent and person.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "AIAgentSessionHistory" )]
    [DataContract]
    [CodeGenExclude( CodeGenFeature.DefaultRestController )] // Do not generate a v1 API controller.
    [CodeGenerateRest( DisableEntitySecurity = true )]
    [Rock.SystemGuid.EntityTypeGuid( "6ad212be-f8d0-4c58-ab0e-d723ed5e2155" )]
    public partial class AIAgentSessionHistory : Entity<AIAgentSessionHistory>, IHasAdditionalSettings
    {
        #region Entity Properties

        /// <summary>
        /// The identifier of the session that the history is associated with.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int AIAgentSessionId { get; set; }

        /// <summary>
        /// The role of the message in the session chat history. This is used
        /// to determine if this was a message from the individual, a response
        /// from the agent, or some other message type.
        /// </summary>
        [DataMember]
        public int MessageRole { get; set; }

        /// <summary>
        /// The date and time the message was posted.
        /// </summary>
        [DataMember]
        public DateTime MessageDateTime { get; set; }

        /// <summary>
        /// The text content of the message.
        /// </summary>
        [DataMember]
        public string Message { get; set; }

        /// <summary>
        /// Indicates whether the message is currently in context for the
        /// session. A message that is in context is included in the
        /// chat history and can be used to generate new responses.
        /// </summary>
        [DataMember]
        public bool IsCurrentlyInContext { get; set; }

        /// <summary>
        /// Indicates whether the message is a summary of the session. This is
        /// used when the chat history grows too large and must be summarized
        /// to stay relevant. Only one summary message should be marked as
        /// <see cref="IsCurrentlyInContext"/>.
        /// </summary>
        [DataMember]
        public bool IsSummary { get; set; }

        /// <summary>
        /// The number of input tokens used for the message. This may be <c>0</c>
        /// if the token count could not be determined.
        /// </summary>
        [DataMember]
        public int InputTokenCount { get; set; }

        /// <summary>
        /// The number of output tokens used for the message. This may be <c>0</c>
        /// if the token count could not be determined.
        /// </summary>
        [DataMember]
        public int OutputTokenCount { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public string AdditionalSettingsJson { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// The session that the history is associated with.
        /// </summary>
        public virtual AIAgentSession AIAgentSession { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class AIAgentSessionHistoryConfiguration : EntityTypeConfiguration<AIAgentSessionHistory>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AIAgentSessionHistoryConfiguration"/> class.
        /// </summary>
        public AIAgentSessionHistoryConfiguration()
        {
            this.HasRequired( a => a.AIAgentSession ).WithMany( b => b.AIAgentSessionHistories ).HasForeignKey( a => a.AIAgentSessionId ).WillCascadeOnDelete( true );
        }
    }

    #endregion Entity Configuration
}
