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
    /// Represents a connection request workflow
    /// </summary>
    [Table( "ConnectionRequestWorkflow" )]
    [DataContract]
    public partial class ConnectionRequestWorkflow : Model<ConnectionRequestWorkflow>
    {

        #region Entity Properties

        [Required]
        [DataMember]
        public int? ConnectionRequestId { get; set; }

        [Required]
        [DataMember]
        public int? ConnectionWorkflowId { get; set; }

        [Required]
        [DataMember]
        public int? WorkflowId { get; set; }

        [DataMember]
        public ConnectionWorkflowTriggerType TriggerType { get; set; }

        [DataMember]
        public String TriggerQualifier;

        #endregion

        #region Virtual Properties

        [DataMember]
        public virtual ConnectionRequest ConnectionRequest { get; set; }

        [DataMember]
        public virtual ConnectionWorkflow ConnectionWorkflow { get; set; }

        [DataMember]
        public virtual Workflow Workflow { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// ConnectionRequestWorkflow Configuration class.
    /// </summary>
    public partial class ConnectionRequestWorkflowConfiguration : EntityTypeConfiguration<ConnectionRequestWorkflow>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionRequestWorkflowConfiguration" /> class.
        /// </summary>
        public ConnectionRequestWorkflowConfiguration()
        {
            this.HasRequired( p => p.ConnectionWorkflow ).WithMany().HasForeignKey( p => p.ConnectionWorkflowId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.ConnectionRequest ).WithMany().HasForeignKey( p => p.ConnectionRequestId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.Workflow ).WithMany().HasForeignKey( p => p.WorkflowId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}