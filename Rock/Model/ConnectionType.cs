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

        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        [Required]
        [DataMember( IsRequired = true )]
        public string Description { get; set; }

        [DataMember]
        public string IconCssClass { get; set; }

        [Required]
        [DataMember]
        public bool EnableFutureFollowup { get; set; }

        [Required]
        [DataMember]
        public bool EnableFullActivityList { get; set; }

        [DataMember]
        public int? OwnerPersonAliasId {get;set;}

        #endregion

        #region Virtual Properties

        [DataMember]
        public virtual PersonAlias OwnerPersonAlias { get; set; }

        [DataMember]
        public virtual ICollection<ConnectionStatus> ConnectionStatuses
        {
            get { return _connectionStatuses; }
            set { _connectionStatuses = value; }
        }

        private ICollection<ConnectionStatus> _connectionStatuses;

        [DataMember]
        public virtual ICollection<ConnectionWorkflow> ConnectionWorkflows
        {
            get { return _connectionWorkflows; }
            set { _connectionWorkflows = value; }
        }

        private ICollection<ConnectionWorkflow> _connectionWorkflows;

        [DataMember]
        public virtual ICollection<ConnectionActivityType> ConnectionActions
        {
            get { return _connectionActions; }
            set { _connectionActions = value; }
        }

        private ICollection<ConnectionActivityType> _connectionActions;

        [DataMember]
        public virtual ICollection<ConnectionOpportunity> ConnectionOpportunities
        {
            get { return _connectionOpportunities; }
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