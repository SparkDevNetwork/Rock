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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a Interaction Channel. See notes on <seealso cref="ChannelEntityId"/>
    /// </summary>
    [RockDomain( "Core" )]
    [NotAudited]
    [Table( "InteractionChannel" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "08606092-5FF5-4A34-A7A6-3DEE43F2843A")]
    public partial class InteractionChannel : Model<InteractionChannel>, IHasActiveFlag, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the interaction channel name.
        /// </summary>
        /// <value>
        /// The interaction channel name.
        /// </value>
        [DataMember]
        [MaxLength( 250 )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the channel data.
        /// </summary>
        /// <value>
        /// The channel data.
        /// </value>
        [DataMember]
        public string ChannelData { get; set; }

        /// <summary>
        /// Gets or sets the engagement strength.
        /// </summary>
        /// <value>
        /// The engagement strength.
        /// </value>
        [DataMember]
        public int? EngagementStrength { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EntityType"/> for each of this channel's <see cref="Rock.Model.InteractionComponent">components</see>.
        /// The Id of the <see cref="ComponentEntityTypeId"/> is stored in down in <see cref="Rock.Model.InteractionComponent.EntityId" />.
        /// For example:
        /// <list type="bullet">
        /// <item>
        ///     <term>PageView</term>
        ///     <description>EntityType is <see cref="Rock.Model.Page" />. Page.Id is stored down in <see cref="Rock.Model.InteractionComponent.EntityId" /></description></item>
        /// <item>
        ///     <term>Communication Recipient Activity</term>
        ///     <description>EntityType is <see cref="Rock.Model.Communication" />. Communication.Id is stored down in <see cref="Rock.Model.InteractionComponent.EntityId" /> </description></item>
        /// <item>
        ///     <term>Workflow Entry Form</term>
        ///     <description>EntityType is <see cref="Rock.Model.WorkflowType" />. WorkflowType.Id is stored down in <see cref="Rock.Model.InteractionComponent.EntityId" /></description></item>
        /// </list>
        /// </summary>
        /// <remarks>
        /// <see cref="Rock.Model.InteractionChannel.ComponentEntityTypeId"/>
        /// </remarks>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId for the <see cref="Rock.Model.EntityType"/>
        /// </value>
        [DataMember]
        public int? ComponentEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId for the <see cref="Rock.Model.EntityType"/> of entity that was modified. For example:
        /// <list type="bullet">
        /// <item>
        ///     <term>PageView</term>
        ///     <description>null</description></item>
        /// <item>
        ///     <term>Communication Recipient Activity</term>
        ///     <description><see cref="Rock.Model.CommunicationRecipient" /></description></item>
        /// <item>
        ///     <term>Workflow Entry Form</term>
        ///     <description><see cref="Rock.Model.Workflow" /></description></item>
        /// </list>
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId for the <see cref="Rock.Model.EntityType"/> of the entity that was modified.
        /// </value>
        [DataMember]
        public int? InteractionEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the channel entity identifier.
        /// Note, the ChannelEntityType is inferred based on what the ChannelTypeMediumValue is:
        /// <list type="bullet">
        /// <item>
        ///     <term>Page Views (<see cref="Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE"/>)</term>
        ///     <description><see cref="Rock.Model.Site" /> Id</description></item>
        /// <item>
        ///     <term>Communication Recipient Activity (<see cref="Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_COMMUNICATION"/>)</term>
        ///     <description><see cref="Rock.Model.Communication" /> Id</description></item>
        /// <item>
        ///     <term>Content Channel Activity (<see cref="Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_CONTENTCHANNEL" />)</term>
        ///     <description><see cref="Rock.Model.ContentChannel" /> Id</description></item>
        /// <item>
        ///     <term>System Events, like Workflow Form Entry (<see cref="Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_SYSTEM_EVENTS" />)</term>
        ///     <description>null, only one Channel</description></item>
        /// </list>
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <value>
        /// The channel entity identifier.
        /// </value>
        [DataMember]
        public int? ChannelEntityId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Channel Type <see cref="Rock.Model.DefinedValue" /> representing what type of Interaction Channel this is.
        /// This helps determine the <seealso cref="ChannelEntityId"/>
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.DefinedValue"/> identifying the interaction channel type. If no value is selected this can be null.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.INTERACTION_CHANNEL_MEDIUM )]
        public int? ChannelTypeMediumValueId { get; set; }

        /// <summary>
        /// Gets or sets the retention days.
        /// </summary>
        /// <value>
        /// The retention days.
        /// </value>
        [DataMember]
        public int? RetentionDuration { get; set; }

        /// <summary>
        /// Gets or sets the length of time (in minutes) that components of this channel should be cached
        /// </summary>
        /// <value>
        /// The duration (in minutes) of the component cache.
        /// </value>
        [DataMember]
        public int? ComponentCacheDuration { get; set; }

        /// <summary>
        /// Gets or sets the channel list template.
        /// </summary>
        /// <value>
        /// The channel list template.
        /// </value>
        [DataMember]
        public string ChannelListTemplate { get; set; }

        /// <summary>
        /// Gets or sets the channel detail template.
        /// </summary>
        /// <value>
        /// The channel detail template.
        /// </value>
        [DataMember]
        public string ChannelDetailTemplate { get; set; }

        /// <summary>
        /// Gets or sets the component list template.
        /// </summary>
        /// <value>
        /// The component list template.
        /// </value>
        [DataMember]
        public string ComponentListTemplate { get; set; }

        /// <summary>
        /// Gets or sets the component detail template.
        /// </summary>
        /// <value>
        /// The component detail template.
        /// </value>
        [DataMember]
        public string ComponentDetailTemplate { get; set; }

        /// <summary>
        /// Gets or sets the session list template.
        /// </summary>
        /// <value>
        /// The session list template.
        /// </value>
        [DataMember]
        public string SessionListTemplate { get; set; }

        /// <summary>
        /// Gets or sets the session detail template.
        /// </summary>
        /// <value>
        /// The session detail template.
        /// </value>
        [DataMember]
        public string SessionDetailTemplate { get; set; }

        /// <summary>
        /// Gets or sets the interaction list template.
        /// </summary>
        /// <value>
        /// The interaction list template.
        /// </value>
        [DataMember]
        public string InteractionListTemplate { get; set; }

        /// <summary>
        /// Gets or sets the interaction detail template.
        /// </summary>
        /// <value>
        /// The interaction detail template.
        /// </value>
        [DataMember]
        public string InteractionDetailTemplate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [uses session].
        /// Set to true if interactions in this channel from a web browser session (for example: PageViews).
        /// Set to false if interactions in this channel are not associated with a web browser session (for example: communication clicks and opens from an email client or sms device).
        /// </summary>
        /// <value>
        ///   <c>true</c> if [uses session]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool UsesSession { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this is an active group. This value is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this group is active, otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Previewable]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the interaction custom 1 label.
        /// </summary>
        /// <value>
        /// The interaction custom 1 label.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string InteractionCustom1Label { get; set; }

        /// <summary>
        /// Gets or sets the interaction custom 2 label.
        /// </summary>
        /// <value>
        /// The interaction custom 2 label.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string InteractionCustom2Label { get; set; }

        /// <summary>
        /// Gets or sets the interaction custom indexed 1 label.
        /// </summary>
        /// <value>
        /// The interaction custom indexed 1 label.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string InteractionCustomIndexed1Label { get; set; }

        /// <summary>
        /// Gets or sets the component custom 1 label.
        /// </summary>
        /// <value>
        /// The component custom 1 label.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string ComponentCustom1Label { get; set; }

        /// <summary>
        /// Gets or sets the component custom 2 label.
        /// </summary>
        /// <value>
        /// The component custom 2 label.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string ComponentCustom2Label { get; set; }

        /// <summary>
        /// Gets or sets the component custom indexed 1 label.
        /// </summary>
        /// <value>
        /// The component custom indexed 1 label.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string ComponentCustomIndexed1Label { get; set; }

        #endregion

        #region Navigation Properties

        /// <inheritdoc cref="ComponentEntityTypeId"/>
        [DataMember]
        public virtual Model.EntityType ComponentEntityType { get; set; }

        /// <inheritdoc cref="InteractionEntityTypeId"/>
        [DataMember]
        public virtual Model.EntityType InteractionEntityType { get; set; }

        /// <inheritdoc cref="ChannelTypeMediumValueId"/>
        [DataMember]
        public virtual DefinedValue ChannelTypeMediumValue { get; set; }

        /// <summary>
        /// Gets or sets the interaction sessions for this channel.
        /// </summary>
        /// <value>
        /// The interaction sessions.
        /// </value>
        [DataMember]
        public virtual ICollection<InteractionSession> InteractionSessions { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    public partial class InteractionChannelConfiguration : EntityTypeConfiguration<InteractionChannel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionChannelConfiguration"/> class.
        /// </summary>
        public InteractionChannelConfiguration()
        {
            this.HasOptional( r => r.ComponentEntityType ).WithMany( ).HasForeignKey( r => r.ComponentEntityTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.InteractionEntityType ).WithMany().HasForeignKey( r => r.InteractionEntityTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.ChannelTypeMediumValue ).WithMany().HasForeignKey( r => r.ChannelTypeMediumValueId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
