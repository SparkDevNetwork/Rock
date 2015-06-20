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
    /// Represents a connection workflow
    /// </summary>
    [Table( "ConnectionWorkflow" )]
    [DataContract]
    public partial class ConnectionWorkflow : Model<ConnectionWorkflow>
    {

        #region Entity Properties

        [DataMember]
        public int? ConnectionTypeId { get; set; }

        [DataMember]
        public int? ConnectionOpportunityId { get; set; }

        [Required]
        [DataMember]
        public int? WorkflowTypeId { get; set; }

        [DataMember]
        public ConnectionWorkflowTriggerType TriggerType { get; set; }

        [DataMember]
        public string QualifierValue { get; set; }

        #endregion

        #region Virtual Properties

        [DataMember]
        public virtual ConnectionType ConnectionType { get; set; }

        [DataMember]
        public virtual ConnectionOpportunity ConnectionOpportunity { get; set; }

        [DataMember]
        public virtual WorkflowType WorkflowType { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// ConnectionWorkflow Configuration class.
    /// </summary>
    public partial class ConnectionWorkflowConfiguration : EntityTypeConfiguration<ConnectionWorkflow>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionWorkflowConfiguration" /> class.
        /// </summary>
        public ConnectionWorkflowConfiguration()
        {
            this.HasOptional( p => p.ConnectionType ).WithMany( p => p.ConnectionWorkflows ).HasForeignKey( p => p.ConnectionTypeId ).WillCascadeOnDelete( true );
            this.HasOptional( p => p.ConnectionOpportunity ).WithMany( p => p.ConnectionWorkflows ).HasForeignKey( p => p.ConnectionOpportunityId ).WillCascadeOnDelete( true );
            this.HasRequired( p => p.WorkflowType ).WithMany().HasForeignKey( p => p.WorkflowTypeId ).WillCascadeOnDelete( true );

        }
    }

    #endregion
}

#region Enumerations

/// <summary>
/// Type of workflow trigger
/// </summary>
public enum ConnectionWorkflowTriggerType
{
    /// <summary>
    /// Request Started
    /// </summary>
    RequestStarted = 0,

    /// <summary>
    /// Request Completed
    /// </summary>
    RequestCompleted = 1,

    /// <summary>
    /// Status Changed
    /// </summary>
    StatusChanged = 2,

    /// <summary>
    /// State Changed
    /// </summary>
    StateChanged = 3,

    /// <summary>
    /// Activity Added
    /// </summary>
    ActivityAdded = 4,

    /// <summary>
    /// Activity Group Assigned
    /// </summary>
    ActivityGroupAssigned = 5,

    /// <summary>
    /// Manual
    /// </summary>
    Manual = 6
}

#endregion