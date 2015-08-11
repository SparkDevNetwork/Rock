// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a connection status
    /// </summary>
    [Table( "ConnectionStatus" )]
    [DataContract]
    public partial class ConnectionStatus : Model<ConnectionStatus>
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
        public bool IsActive { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the type of the connection.
        /// </summary>
        /// <value>
        /// The type of the connection.
        /// </value>
        public virtual ConnectionType ConnectionType { get; set; }

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