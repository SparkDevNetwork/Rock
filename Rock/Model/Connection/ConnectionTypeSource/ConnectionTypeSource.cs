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
    /// Represents a connection type source
    /// </summary>
    [RockDomain( "Engagement" )]
    [Table( "ConnectionTypeSource" )]
    [DataContract]
    [CodeGenerateRest]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.CONNECTION_TYPE_SOURCE )]
    public partial class ConnectionTypeSource : Model<ConnectionTypeSource>
    {
        #region Entity Properties

        /// <summary>
        /// The display name of this connection type source.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// The identifier of the connection type this source belongs to.
        /// </summary>
        [DataMember]
        public int ConnectionTypeId { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ConnectionType">type</see> of the connection.
        /// </summary>
        /// <value>
        /// The type of the connection.
        /// </value>
        [LavaVisible]
        public virtual ConnectionType ConnectionType { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// ConnectionTypeSource Configuration class.
    /// </summary>
    public partial class ConnectionTypeSourceConfiguration : EntityTypeConfiguration<ConnectionTypeSource>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionTypeSourceConfiguration" /> class.
        /// </summary>
        public ConnectionTypeSourceConfiguration()
        {
            this.HasRequired( p => p.ConnectionType ).WithMany().HasForeignKey( p => p.ConnectionTypeId ).WillCascadeOnDelete( true );
        }
    }

    #endregion
}