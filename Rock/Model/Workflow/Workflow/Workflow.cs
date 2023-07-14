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
using Rock.Data;
using Rock.Lava;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// Represents a persisted <see cref="Rock.Model.Workflow"/> execution/instance in Rock.
    /// </summary>
    [RockDomain( "Workflow" )]
    [Table( "Workflow" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.WORKFLOW )]
    public partial class Workflow : Model<Workflow>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the WorkflowTypeId of the <see cref="Rock.Model.WorkflowType"/> that this Workflow instance is executing.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the WorkflowTypeId of the <see cref="Rock.Model.WorkflowType"/> that is being executed.
        /// </value>
        [DataMember]
        public int WorkflowTypeId { get; set; }

        /// <summary>
        /// A type specific number to uniquely identify a workflow.
        /// </summary>
        /// <value>
        /// The type identifier number.
        /// </value>
        [DataMember]
        public int WorkflowIdNumber { get; set; }

        /// <summary>
        /// This value is a string of the WorkflowType's WorkflowIdPrefix combined with the WorkflowIdNumber.
        /// </summary>
        /// <value>
        /// The workflow identifier.
        /// </value>
        [DataMember]
        public virtual string WorkflowId { get; set; }

        /// <summary>
        /// Gets or sets a friendly name for this Workflow instance. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> representing a friendly name of this Workflow instance.
        /// </value>
        [Required]
        [MaxLength( 250 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description or summary about this Workflow instance.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description or summary about this Workflow instance.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the status of this Workflow instance. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the status of this Workflow instance.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether this instance is processing.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this instance is processing; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        [NotAudited]
        public bool IsProcessing { get; set; }

        /// <summary>
        /// Gets or sets the date and time that this Workflow instance was activated.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> that represents the date and time that this Workflow instance was activated.
        /// </value>
        [DataMember]
        public DateTime? ActivatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the Workflow was last processed.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> that represents when the Workflow was last processed.
        /// </value>
        [DataMember]
        [NotAudited]
        public DateTime? LastProcessedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the Workflow completed.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time that the Workflow completed.
        /// </value>
        [DataMember]
        public DateTime? CompletedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the initiator person alias identifier.
        /// </summary>
        /// <value>
        /// The initiator person alias identifier.
        /// </value>
        [DataMember]
        public int? InitiatorPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the Entity Id.
        /// </summary>
        /// <value>
        /// The Entity Id.
        /// </value>
        [DataMember]
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the Entity Type Id.
        /// </summary>
        /// <value>
        /// The Entity Type Id.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Campus Id of the workflow campus
        /// </summary>
        /// <value>
        /// The Campus Id
        /// </value>
        [DataMember]
        public int? CampusId { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.WorkflowType"/> that is being executed in this persisted Workflow instance.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.WorkflowType"/> that is being executed in this persisted Workflow instance.
        /// </value>
        [LavaVisible]
        public virtual WorkflowType WorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the initiator person alias.
        /// </summary>
        /// <value>
        /// The initiator person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias InitiatorPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets a collection containing all the <see cref="Rock.Model.WorkflowActivity">WorkflowActivities</see> that are a part of this Workflow instance.
        /// </summary>
        /// <value>
        /// A collection containing the <see cref="Rock.Model.WorkflowActivity">WorkflowActivities</see> that are a part of this Workflow instance.
        /// </value>
        [DataMember]
        public virtual ICollection<WorkflowActivity> Activities
        {
            get { return _activities ?? ( _activities = new Collection<WorkflowActivity>() ); }
            set { _activities = value; }
        }
        private ICollection<WorkflowActivity> _activities;

        /// <summary>
        /// Gets or sets the campus tied to the <see cref="CampusId"/>.
        /// </summary>
        /// <value>
        /// The initiator person alias.
        /// </value>
        [DataMember]
        public virtual Campus Campus { get; set; }

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// Workflow Configuration class.
    /// </summary>
    public partial class WorkflowConfiguration : EntityTypeConfiguration<Workflow>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowConfiguration"/> class.
        /// </summary>
        public WorkflowConfiguration()
        {
            this.HasRequired( w => w.WorkflowType ).WithMany().HasForeignKey( w => w.WorkflowTypeId ).WillCascadeOnDelete( true );
            this.HasOptional( w => w.InitiatorPersonAlias ).WithMany().HasForeignKey( w => w.InitiatorPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( w => w.Campus ).WithMany().HasForeignKey( w => w.CampusId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}

