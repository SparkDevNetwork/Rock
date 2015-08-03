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
    /// Represents a connection type
    /// </summary>
    [Table( "ConnectionType" )]
    [DataContract]
    public partial class ConnectionType : Model<ConnectionType>
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
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [DataMember]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether future follow-ups are enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if future follow-ups are enabled; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [DataMember]
        public bool EnableFutureFollowup { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether full activity lists are enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if full activity lists are enabled; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [DataMember]
        public bool EnableFullActivityList { get; set; }

        /// <summary>
        /// Gets or sets the owner person alias identifier.
        /// </summary>
        /// <value>
        /// The owner person alias identifier.
        /// </value>
        [DataMember]
        public int? OwnerPersonAliasId {get;set;}

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the owner person alias.
        /// </summary>
        /// <value>
        /// The owner person alias.
        /// </value>
        public virtual PersonAlias OwnerPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.ConnectionStatus">ConnectionStatuses</see> who are associated with the ConnectionType.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.ConnectionStatus">ConnectionStatuses</see> who are associated with the ConnectionType.
        /// </value>
        public virtual ICollection<ConnectionStatus> ConnectionStatuses
        {
            get { return _connectionStatuses ?? ( _connectionStatuses = new Collection<ConnectionStatus>() ); }
            set { _connectionStatuses = value; }
        }

        private ICollection<ConnectionStatus> _connectionStatuses;

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.ConnectionWorkflow">ConnectionWorkflows</see> who are associated with the ConnectionType.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.ConnectionWorkflow">ConnectionWorkflows</see> who are associated with the ConnectionType.
        /// </value>
        public virtual ICollection<ConnectionWorkflow> ConnectionWorkflows
        {
            get { return _connectionWorkflows ?? ( _connectionWorkflows = new Collection<ConnectionWorkflow>() ); }
            set { _connectionWorkflows = value; }
        }

        private ICollection<ConnectionWorkflow> _connectionWorkflows;

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.ConnectionActivityType">ConnectionActivityTypes</see> who are associated with the ConnectionType.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.ConnectionActivityType">ConnectionActivityTypes</see> who are associated with the ConnectionType.
        /// </value>
        public virtual ICollection<ConnectionActivityType> ConnectionActivityTypes
        {
            get { return _connectionActivityTypes ?? ( _connectionActivityTypes = new Collection<ConnectionActivityType>() ); }
            set { _connectionActivityTypes = value; }
        }

        private ICollection<ConnectionActivityType> _connectionActivityTypes;

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.ConnectionOpportunity">ConnectionOpportunities</see> who are associated with the ConnectionType.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.ConnectionOpportunity">ConnectionOpportunities</see> who are associated with the ConnectionType.
        /// </value>
        public virtual ICollection<ConnectionOpportunity> ConnectionOpportunities
        {
            get { return _connectionOpportunities ?? ( _connectionOpportunities = new Collection<ConnectionOpportunity>() ); }
            set { _connectionOpportunities = value; }
        }

        private ICollection<ConnectionOpportunity> _connectionOpportunities;

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// ConnectionType Configuration class.
    /// </summary>
    public partial class ConnectionTypeConfiguration : EntityTypeConfiguration<ConnectionType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionTypeConfiguration" /> class.
        /// </summary>
        public ConnectionTypeConfiguration()
        {
            this.HasOptional( p => p.OwnerPersonAlias ).WithMany().HasForeignKey( p => p.OwnerPersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}