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
using Newtonsoft.Json;

using Rock.Data;
using Rock.Enums.Communication;

namespace Rock.Model
{
    /// <summary>
    /// Represents a Communication Flow Instance in Rock.
    /// </summary>
    [RockDomain( "Communication" )]
    [Table( "CommunicationFlowInstanceRecipient" )]
    [DataContract]
    [CodeGenerateRest]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.COMMUNICATION_FLOW_INSTANCE_RECIPIENT )]
    public partial class CommunicationFlowInstanceRecipient : Model<CommunicationFlowInstanceRecipient>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the identifier of the Communication Flow Instance.
        /// </summary>
        [Required]
        [DataMember]
        public int CommunicationFlowInstanceId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the recipient Person Alias.
        /// </summary>
        [Required]
        [DataMember]
        public int RecipientPersonAliasId { get; set; }
        
        /// <summary>
        /// Gets or sets the status for this Communication Flow Instance Recipient.
        /// </summary>
        [DataMember]
        public CommunicationFlowInstanceRecipientStatus Status { get; set; }
        
        /// <summary>
        /// Gets or sets the inactive reason for this Communication Flow Instance Recipient.
        /// </summary>
        [DataMember]
        public CommunicationFlowInstanceRecipientInactiveReason? InactiveReason { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the Communication Recipient that was unsubscribed.
        /// </summary>
        /// <remarks>
        /// This can be used to find the exact Communication that caused the recipient to unsubscribe from the Communication Flow Instance.
        /// </remarks>
        [DataMember]
        public int? UnsubscribeCommunicationRecipientId { get; set; }

        /// <summary>
        /// Gets or sets the unsubscribe scope for this Communication Flow Instance Recipient.
        /// </summary>
        [DataMember]
        public CommunicationFlowInstanceRecipientUnsubscribeScope? UnsubscribeScope { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the Communication Flow Instance.
        /// </summary>
        [DataMember]
        public virtual CommunicationFlowInstance CommunicationFlowInstance { get; set; }

        /// <summary>
        /// Gets or sets the recipient Person Alias.
        /// </summary>
        [DataMember]
        public PersonAlias RecipientPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the Communication Recipient that was unsubscribed.
        /// </summary>
        /// <remarks>
        /// This can be used to find the exact Communication that caused the recipient to unsubscribe from the Communication Flow Instance.
        /// </remarks>
        [DataMember]
        public CommunicationRecipient UnsubscribeCommunicationRecipient { get; set; }

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// Communication Flow Instance Configuration class.
    /// </summary>
    public partial class CommunicationFlowInstanceRecipientConfiguration : EntityTypeConfiguration<CommunicationFlowInstanceRecipient>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationFlowInstanceRecipientConfiguration"/> class.
        /// </summary>
        public CommunicationFlowInstanceRecipientConfiguration()
        {
            this.HasRequired( c => c.CommunicationFlowInstance ).WithMany( i => i.CommunicationFlowInstanceRecipients ).HasForeignKey( c => c.CommunicationFlowInstanceId ).WillCascadeOnDelete( false );
            this.HasRequired( c => c.RecipientPersonAlias ).WithMany().HasForeignKey( c => c.RecipientPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( c => c.UnsubscribeCommunicationRecipient ).WithMany().HasForeignKey( c => c.UnsubscribeCommunicationRecipientId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}