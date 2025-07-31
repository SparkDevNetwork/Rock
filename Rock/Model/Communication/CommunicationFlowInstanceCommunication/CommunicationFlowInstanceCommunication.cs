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

namespace Rock.Model
{
    /// <summary>
    /// Represents a Communication Flow Instance Communication in Rock.
    /// </summary>
    [RockDomain( "Communication" )]
    [Table( "CommunicationFlowInstanceCommunication" )]
    [DataContract]
    [CodeGenerateRest]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.COMMUNICATION_FLOW_INSTANCE_COMMUNICATION )]
    public partial class CommunicationFlowInstanceCommunication : Model<CommunicationFlowInstanceCommunication>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the identifier of the Communication Flow Instance.
        /// </summary>
        [Required]
        [DataMember]
        public int CommunicationFlowInstanceId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the Communication Flow Communication.
        /// </summary>
        [Required]
        [DataMember]
        public int CommunicationFlowCommunicationId { get; set; }
        
        /// <summary>
        /// Gets or sets the identifier of the Communication.
        /// </summary>
        [Required]
        [DataMember]
        public int CommunicationId { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the Communication Flow Instance.
        /// </summary>
        [DataMember]
        public virtual CommunicationFlowInstance CommunicationFlowInstance { get; set; }

        /// <summary>
        /// Gets or sets the Communication Flow Communication.
        /// </summary>
        [DataMember]
        public virtual CommunicationFlowCommunication CommunicationFlowCommunication { get; set; }

        /// <summary>
        /// Gets or sets the Communication.
        /// </summary>
        [DataMember]
        public virtual Communication Communication { get; set; }

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// Communication Flow Instance Communication Configuration class.
    /// </summary>
    public partial class CommunicationFlowInstanceCommunicationConfiguration : EntityTypeConfiguration<CommunicationFlowInstanceCommunication>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationFlowInstanceCommunicationConfiguration"/> class.
        /// </summary>
        public CommunicationFlowInstanceCommunicationConfiguration()
        {
            this.HasRequired( c => c.CommunicationFlowInstance ).WithMany( i => i.CommunicationFlowInstanceCommunications ).HasForeignKey( c => c.CommunicationFlowInstanceId ).WillCascadeOnDelete( false );
            this.HasRequired( c => c.CommunicationFlowCommunication ).WithMany( i => i.CommunicationFlowInstanceCommunications ).HasForeignKey( c => c.CommunicationFlowCommunicationId ).WillCascadeOnDelete( false );
            this.HasRequired( c => c.Communication ).WithMany().HasForeignKey( c => c.CommunicationId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}