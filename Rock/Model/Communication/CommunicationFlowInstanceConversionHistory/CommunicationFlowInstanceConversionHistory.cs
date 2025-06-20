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
    /// Represents a Communication Flow Instance Conversion History in Rock.
    /// </summary>
    [RockDomain( "Communication" )]
    [Table( "CommunicationFlowInstanceConversionHistory" )]
    [DataContract]
    [CodeGenerateRest]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.COMMUNICATION_FLOW_INSTANCE_CONVERSION_HISTORY )]
    public partial class CommunicationFlowInstanceConversionHistory : Model<CommunicationFlowInstanceConversionHistory>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the identifier of the Communication Flow Instance.
        /// </summary>
        [Required]
        [DataMember]
        public int CommunicationFlowInstanceId { get; set; }
        
        /// <summary>
        /// Gets or sets the date that the conversion occurred.
        /// </summary>
        [DataMember]
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the Person Alias whom this conversion is for.
        /// </summary>
        [Required]
        [DataMember]
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the Communication Flow Communication identifier which this conversion is for.
        /// </summary>
        [Required]
        [DataMember]
        public int CommunicationFlowCommunicationId { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the Communication Flow Instance.
        /// </summary>
        [DataMember]
        public virtual CommunicationFlowInstance CommunicationFlowInstance { get; set; }

        /// <summary>
        /// Gets or sets the Person Alias whom this conversion is for.
        /// </summary>
        [DataMember]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the Communication Flow Communication which this conversion is for.
        /// </summary>
        [DataMember]
        public virtual CommunicationFlowCommunication CommunicationFlowCommunication { get; set; }

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// Communication Flow Instance Conversion History Configuration class.
    /// </summary>
    public partial class CommunicationFlowInstanceConversionHistoryConfiguration : EntityTypeConfiguration<CommunicationFlowInstanceConversionHistory>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationFlowInstanceConversionHistoryConfiguration"/> class.
        /// </summary>
        public CommunicationFlowInstanceConversionHistoryConfiguration()
        {
            this.HasRequired( c => c.CommunicationFlowInstance ).WithMany( i => i.CommunicationFlowInstanceConversionHistories ).HasForeignKey( c => c.CommunicationFlowInstanceId ).WillCascadeOnDelete( false );
            this.HasRequired( c => c.PersonAlias ).WithMany().HasForeignKey( c => c.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasRequired( c => c.CommunicationFlowCommunication ).WithMany().HasForeignKey( c => c.CommunicationFlowCommunicationId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}