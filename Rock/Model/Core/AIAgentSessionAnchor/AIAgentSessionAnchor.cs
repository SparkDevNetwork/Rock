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
    /// Represents an anchor for an agent session that provides additional
    /// context to the language model. For example, you might have an anchor
    /// on Person:Ted Decker as well on Group:Alisha Marble's Small Group. You
    /// cannot have multiple anchors on the same entity type.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "AIAgentSessionAnchor" )]
    [DataContract]
    [CodeGenExclude( CodeGenFeature.DefaultRestController )] // Do not generate a v1 API controller.
    [CodeGenerateRest( DisableEntitySecurity = true )]
    [Rock.SystemGuid.EntityTypeGuid( "3ed1476a-b7fc-40e2-bbab-af084c82d7f1" )]
    public partial class AIAgentSessionAnchor : Entity<AIAgentSessionAnchor>, IHasAdditionalSettings
    {
        #region Entity Properties

        /// <summary>
        /// The identifier of the session that the anchor is associated with.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int AIAgentSessionId { get; set; }

        /// <summary>
        /// The date and time the anchor was added to the session. This will
        /// be set automatically when the anchor is created.
        /// </summary>
        [DataMember]
        public DateTime AddedDateTime { get; set; }

        /// <summary>
        /// The date and time the anchor was removed from the session. This
        /// will be set automatically when the anchor is made inactive.
        /// </summary>
        [DataMember]
        public DateTime? RemovedDateTime { get; set; }

        /// <summary>
        /// The identifier of the entity type that <see cref="EntityId"/> is
        /// referring to.
        /// </summary>
        [DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// The identifier of the entity that this anchor is associated with.
        /// </summary>
        [DataMember]
        public int? EntityId { get; set; }

        /// <summary>
        /// Indicates whether the anchor is currently active. An anchor that is
        /// not active will not be sent to the language model.
        /// </summary>
        [DataMember]
        public bool IsActive { get; set; }

        /// <summary>
        /// The date and time the payload was last refreshed. This is used to
        /// automatically refresh the payload after a period of time. This is
        /// required since the referenced entity might have changed.
        /// </summary>
        [DataMember]
        public DateTime PayloadLastRefreshedDateTime { get; set; }

        /// <summary>
        /// The JSON payload that contains the context for the anchor. This
        /// exact contents will vary based on the entity type.
        /// </summary>
        [DataMember]
        public string PayloadJson { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public string AdditionalSettingsJson { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// The session that the history is associated with.
        /// </summary>
        public virtual AIAgentSession AIAgentSession { get; set; }

        /// <summary>
        /// The entity type that <see cref="EntityId"/> is referring to.
        /// </summary>
        public virtual EntityType EntityType { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class AIAgentSessionAnchorConfiguration : EntityTypeConfiguration<AIAgentSessionAnchor>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AIAgentSessionAnchorConfiguration"/> class.
        /// </summary>
        public AIAgentSessionAnchorConfiguration()
        {
            this.HasRequired( a => a.AIAgentSession ).WithMany( b => b.AIAgentSessionAnchors ).HasForeignKey( a => a.AIAgentSessionId ).WillCascadeOnDelete( true );

            // Do not cascade as that will be handled via AIAgentSession.
            this.HasOptional( a => a.EntityType ).WithMany().HasForeignKey( a => a.EntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}
