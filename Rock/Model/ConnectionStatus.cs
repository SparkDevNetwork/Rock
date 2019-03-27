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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a connection status
    /// </summary>
    [RockDomain( "Connection" )]
    [Table( "ConnectionStatus" )]
    [DataContract]
    public partial class ConnectionStatus : Model<ConnectionStatus>, IHasActiveFlag
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the connection type identifier.
        /// </summary>
        /// <value>
        /// The connection type identifier.
        /// </value>
        [DataMember]
        public int? ConnectionTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is critical.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is critical; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsCritical { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is default.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is default; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsDefault { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }
        private bool _isActive = true;

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the type of the connection.
        /// </summary>
        /// <value>
        /// The type of the connection.
        /// </value>
        [LavaInclude]
        public virtual ConnectionType ConnectionType { get; set; }

        #endregion

        #region overrides

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// ConnectionStatus Configuration class.
    /// </summary>
    public partial class ConnectionStatusConfiguration : EntityTypeConfiguration<ConnectionStatus>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionStatusConfiguration" /> class.
        /// </summary>
        public ConnectionStatusConfiguration()
        {
            this.HasOptional( p => p.ConnectionType ).WithMany( p => p.ConnectionStatuses ).HasForeignKey( p => p.ConnectionTypeId ).WillCascadeOnDelete( true );
        }
    }

    #endregion
}