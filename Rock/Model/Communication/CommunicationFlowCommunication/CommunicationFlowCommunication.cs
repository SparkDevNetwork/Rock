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
using Newtonsoft.Json;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a Communication Flow Communication in Rock.
    /// </summary>
    [RockDomain( "Communication" )]
    [Table( "CommunicationFlowCommunication" )]
    [DataContract]
    [CodeGenerateRest]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.COMMUNICATION_FLOW_COMMUNICATION )]
    public partial class CommunicationFlowCommunication : Model<CommunicationFlowCommunication>, IHasAdditionalSettings
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the Communication Flow Communication (maximum 100 characters).
        /// </summary>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the number of days to wait to send since the last communication in this flow.
        /// </summary>
        [DataMember]
        public int DaysToWait { get; set; }
        
        /// <summary>
        /// Gets or sets the time to send this communication flow communication.
        /// </summary>
        [DataMember]
        public TimeSpan TimeToSend { get; set; }

        /// <summary>
        /// Gets or sets the communication type for this communication flow communication.
        /// </summary>
        /// <remarks>
        /// <see cref="CommunicationType.RecipientPreference"/> is not supported for communication flow communications. 
        /// </remarks>
        [DataMember]
        public CommunicationType CommunicationType { get; set; }

        /// <summary>
        /// Gets or sets the Communication Flow identifier.
        /// </summary>
        [Required]
        [DataMember]
        public int CommunicationFlowId { get; set; }

        /// <summary>
        /// Gets or sets the Communication Template identifier used to create the communications that will be sent to recipients.
        /// </summary>
        [Required]
        [DataMember]
        public int CommunicationTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the order of this communication in the communication flow.
        /// </summary>
        [DataMember]
        public int Order { get; set; }

        /// <inheritdoc />
        [DataMember]
        public string AdditionalSettingsJson { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the Communication Flow.
        /// </summary>
        [DataMember]
        public virtual CommunicationFlow CommunicationFlow { get; set; }

        /// <summary>
        /// Gets or sets the Communication Template.
        /// </summary>
        [DataMember]
        public virtual CommunicationTemplate CommunicationTemplate { get; set; }

        /// <summary>
        /// Gets or sets the Communication Flow Instances that use this Communication Flow Communication.
        /// </summary>
        [DataMember]
        public virtual ICollection<CommunicationFlowInstanceCommunication> CommunicationFlowInstanceCommunications
        {
            get
            {
                return _communicationFlowInstanceCommunications ?? ( _communicationFlowInstanceCommunications = new Collection<CommunicationFlowInstanceCommunication>() );
            }

            set
            {
                _communicationFlowInstanceCommunications = value;
            }
        }

        private ICollection<CommunicationFlowInstanceCommunication> _communicationFlowInstanceCommunications;

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// Communication Flow Communication Configuration class.
    /// </summary>
    public partial class CommunicationFlowCommunicationConfiguration : EntityTypeConfiguration<CommunicationFlowCommunication>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationFlowCommunicationConfiguration"/> class.
        /// </summary>
        public CommunicationFlowCommunicationConfiguration()
        {
            this.HasRequired( c => c.CommunicationTemplate ).WithMany().HasForeignKey( c => c.CommunicationTemplateId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}