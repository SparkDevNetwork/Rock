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
using System.Collections;
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
    /// Represents a Communication Flow Instance in Rock.
    /// </summary>
    [RockDomain( "Communication" )]
    [Table( "CommunicationFlowInstance" )]
    [DataContract]
    [CodeGenerateRest]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.COMMUNICATION_FLOW_INSTANCE )]
    public partial class CommunicationFlowInstance : Model<CommunicationFlowInstance>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the identifier of the Communication Flow.
        /// </summary>
        [Required]
        [DataMember]
        public int CommunicationFlowId { get; set; }
        
        /// <summary>
        /// Gets or sets the date when this Communication Flow Instance starts.
        /// </summary>
        /// <value>The first communication is sent by adding <see cref="CommunicationFlowCommunication.DaysToWait"/> and setting <see cref="CommunicationFlowCommunication.TimeToSend"/> to this value.</value>
        [DataMember]
        public DateTime StartDate
        {
            get
            {
                return _startDate.Date;
            }
            set
            {
                _startDate = value.Date;
            }
        }
        private DateTime _startDate;

        /// <summary>
        /// Gets or sets a value indicating whether this flow instance
        /// has finished scheduling or sending all configured communications.
        /// </summary>
        public bool IsMessagingCompleted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this flow instance has finished
        /// all conversion goal tracking and no further evaluation is required.
        /// </summary>
        public bool IsConversionGoalTrackingCompleted { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the Communication Flow.
        /// </summary>
        [DataMember]
        public virtual CommunicationFlow CommunicationFlow { get; set; }
        
        /// <summary>
        /// Gets or sets the communications for this Communication Flow Instance.
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

        /// <summary>
        /// Gets or sets the recipients for this Communication Flow Instance.
        /// </summary>
        [DataMember]
        public virtual ICollection<CommunicationFlowInstanceRecipient> CommunicationFlowInstanceRecipients
        {
            get
            {
                return _communicationFlowInstanceRecipients ?? ( _communicationFlowInstanceRecipients = new Collection<CommunicationFlowInstanceRecipient>() );
            }
            set
            {
                _communicationFlowInstanceRecipients = value;
            }
        }

        private ICollection<CommunicationFlowInstanceRecipient> _communicationFlowInstanceRecipients;

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// Communication Flow Instance Configuration class.
    /// </summary>
    public partial class CommunicationFlowInstanceConfiguration : EntityTypeConfiguration<CommunicationFlowInstance>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationFlowInstanceConfiguration"/> class.
        /// </summary>
        public CommunicationFlowInstanceConfiguration()
        {
            this.HasRequired( c => c.CommunicationFlow ).WithMany( i => i.CommunicationFlowInstances ).HasForeignKey( c => c.CommunicationFlowId ).WillCascadeOnDelete( true );
        }
    }

    #endregion Entity Configuration
}