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

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a connection request workflow
    /// </summary>
    [RockDomain( "Connection" )]
    [Table( "ConnectionRequestWorkflow" )]
    [DataContract]
    public partial class ConnectionRequestWorkflow : Model<ConnectionRequestWorkflow>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the connection request identifier.
        /// </summary>
        /// <value>
        /// The connection request identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int ConnectionRequestId { get; set; }

        /// <summary>
        /// Gets or sets the connection workflow identifier.
        /// </summary>
        /// <value>
        /// The connection workflow identifier.
        /// </value>
        [Required]
        [DataMember]
        public int ConnectionWorkflowId { get; set; }

        /// <summary>
        /// Gets or sets the workflow identifier.
        /// </summary>
        /// <value>
        /// The workflow identifier.
        /// </value>
        [Required]
        [DataMember]
        public int WorkflowId { get; set; }

        /// <summary>
        /// Gets or sets the type of the trigger.
        /// </summary>
        /// <value>
        /// The type of the trigger.
        /// </value>
        [DataMember]
        public ConnectionWorkflowTriggerType TriggerType { get; set; }

        /// <summary>
        /// Gets or sets the trigger qualifier.
        /// </summary>
        /// <value>
        /// The trigger qualifier.
        /// </value>
        [DataMember]
        public String TriggerQualifier { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the connection request.
        /// </summary>
        /// <value>
        /// The connection request.
        /// </value>
        [LavaInclude]
        public virtual ConnectionRequest ConnectionRequest { get; set; }

        /// <summary>
        /// Gets or sets the connection workflow.
        /// </summary>
        /// <value>
        /// The connection workflow.
        /// </value>
        [LavaInclude]
        public virtual ConnectionWorkflow ConnectionWorkflow { get; set; }

        /// <summary>
        /// Gets or sets the workflow.
        /// </summary>
        /// <value>
        /// The workflow.
        /// </value>
        [LavaInclude]
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
            this.HasRequired( p => p.ConnectionRequest ).WithMany( p => p.ConnectionRequestWorkflows ).HasForeignKey( p => p.ConnectionRequestId ).WillCascadeOnDelete( true );
            this.HasRequired( p => p.Workflow ).WithMany().HasForeignKey( p => p.WorkflowId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}