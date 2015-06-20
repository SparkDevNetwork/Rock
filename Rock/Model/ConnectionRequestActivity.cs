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
    /// Represents a connection request activity
    /// </summary>
    [Table( "ConnectionRequestActivity" )]
    [DataContract]
    public partial class ConnectionRequestActivity : Model<ConnectionRequestActivity>
    {

        #region Entity Properties

        [Required]
        [DataMember( IsRequired = true )]
        public int ConnectionRequestId { get; set; }

        [Required]
        [DataMember]
        public int ConnectionActivityTypeId { get; set; }

        [DataMember]
        public int? ConnectorPersonAliasId { get; set; }

        [Required]
        [DataMember]
        public int? ConnectionOpportunityId { get; set; }

        [DataMember]
        public String Note { get; set; }

        #endregion

        #region Virtual Properties

        [DataMember]
        public virtual ConnectionRequest ConnectionRequest { get; set; }

        [DataMember]
        public virtual ConnectionActivityType ConnectionActivityType { get; set; }

        [DataMember]
        public virtual PersonAlias ConnectorPersonAlias { get; set; }

        [DataMember]
        public virtual ConnectionOpportunity ConnectionOpportunity { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Pres the save changes.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The entry.</param>
        public override void PreSaveChanges( DbContext dbContext, System.Data.Entity.Infrastructure.DbEntityEntry entry )
        {
            var transaction = new Rock.Transactions.ConnectionRequestActivityChangeTransaction( entry );
            Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );

            base.PreSaveChanges( dbContext, entry );
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// ConnectionRequestActivity Configuration class.
    /// </summary>
    public partial class ConnectionRequestActivityConfiguration : EntityTypeConfiguration<ConnectionRequestActivity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionRequestActivityConfiguration" /> class.
        /// </summary>
        public ConnectionRequestActivityConfiguration()
        {
            this.HasOptional( p => p.ConnectorPersonAlias ).WithMany().HasForeignKey( p => p.ConnectorPersonAliasId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.ConnectionRequest ).WithMany( p => p.ConnectionRequestActivities ).HasForeignKey( p => p.ConnectionRequestId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.ConnectionActivityType ).WithMany().HasForeignKey( p => p.ConnectionActivityTypeId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.ConnectionOpportunity ).WithMany().HasForeignKey( p => p.ConnectionOpportunityId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
