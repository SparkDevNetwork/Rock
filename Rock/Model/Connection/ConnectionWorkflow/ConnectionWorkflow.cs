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
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// Represents a connection workflow
    /// </summary>
    [RockDomain( "Engagement" )]
    [Table( "ConnectionWorkflow" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.CONNECTION_WORKFLOW )]
    public partial class ConnectionWorkflow : Model<ConnectionWorkflow>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ConnectionType"/> identifier.
        /// </summary>
        /// <value>
        /// The connection type identifier.
        /// </value>
        [DataMember]
        public int? ConnectionTypeId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ConnectionOpportunity"/> identifier.
        /// </summary>
        /// <value>
        /// The connection opportunity identifier.
        /// </value>
        [DataMember]
        public int? ConnectionOpportunityId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.WorkflowType"/> identifier.
        /// </summary>
        /// <value>
        /// The workflow type identifier.
        /// </value>
        [Required]
        [DataMember]
        public int? WorkflowTypeId { get; set; }

        /// <summary>
        /// Gets or sets the type of the trigger.
        /// </summary>
        /// <value>
        /// The type of the trigger.
        /// </value>
        [DataMember]
        public ConnectionWorkflowTriggerType TriggerType { get; set; }

        /// <summary>
        /// Gets or sets the qualifier value.
        /// </summary>
        /// <value>
        /// The qualifier value.
        /// </value>
        [DataMember]
        public string QualifierValue { get; set; }


        /// <summary>
        /// Gets or sets Connection Status Id used to filter workflows with manual trigger.
        /// </summary>
        [DataMember]
        public int? ManualTriggerFilterConnectionStatusId { get; set; }

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

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ConnectionOpportunity"/>.
        /// </summary>
        /// <value>
        /// The connection opportunity.
        /// </value>
        [LavaVisible]
        public virtual ConnectionOpportunity ConnectionOpportunity { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.WorkflowType">type</see> of the workflow.
        /// </summary>
        /// <value>
        /// The type of the workflow.
        /// </value>
        [DataMember]
        public virtual WorkflowType WorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ConnectionStatus"/>.
        /// </summary>
        [LavaVisible]
        public virtual ConnectionStatus ManualTriggerFilterConnectionStatus { get; set; }

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
            this.HasOptional( p => p.ManualTriggerFilterConnectionStatus ).WithMany().HasForeignKey( p => p.ManualTriggerFilterConnectionStatusId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}