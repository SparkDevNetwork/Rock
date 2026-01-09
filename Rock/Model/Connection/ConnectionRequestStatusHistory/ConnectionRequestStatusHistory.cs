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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// Represents a connection request status history
    /// </summary>
    [RockDomain( "Engagement" )]
    [Table( "ConnectionRequestStatusHistory" )]
    [DataContract]
    [CodeGenerateRest]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.CONNECTION_REQUEST_STATUS_HISTORY )]
    public partial class ConnectionRequestStatusHistory : Model<ConnectionRequestStatusHistory>
    {
        #region Entity Properties

        /// <summary>
        /// The identifier of the connection type status.
        /// </summary>
        [DataMember]
        public int ConnectionStatusId { get; set; }

        /// <summary>
        /// The start date time.
        /// </summary>
        [DataMember]
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// The end date time.
        /// </summary>
        [DataMember]
        public DateTime EndDateTime { get; set; }

        /// <summary>
        /// The person alias who completed the request, if applicable.
        /// </summary>
        [DataMember]
        public int? CompletedByPersonAliasId { get; set; }

        /// <summary>
        /// Indicates whether the request was completed on time while in this status.
        /// </summary>
        [DataMember]
        public bool WasCompletedOnTime { get; set; }

        /// <summary>
        /// Additional notes recorded when this status entry was created.
        /// </summary>
        [DataMember]
        public string Note { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ConnectionStatus">type</see> of the connection.
        /// </summary>
        /// <value>
        /// The status of the connection.
        /// </value>
        [LavaVisible]
        public virtual ConnectionStatus ConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias">type</see> that the connection request status was completed by.
        /// </summary>
        /// <value>
        /// The person alias that completed the connection request status.
        /// </value>
        [LavaVisible]
        public virtual PersonAlias CompletedByPersonAlias { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// ConnectionRequestStatusHistory Configuration class.
    /// </summary>
    public partial class ConnectionRequestStatusHistoryConfiguration : EntityTypeConfiguration<ConnectionRequestStatusHistory>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionRequestStatusHistoryConfiguration" /> class.
        /// </summary>
        public ConnectionRequestStatusHistoryConfiguration()
        {
            this.HasRequired( p => p.ConnectionStatus ).WithMany().HasForeignKey( p => p.ConnectionStatusId ).WillCascadeOnDelete( true );
            this.HasOptional( p => p.CompletedByPersonAlias ).WithMany().HasForeignKey( p => p.CompletedByPersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}