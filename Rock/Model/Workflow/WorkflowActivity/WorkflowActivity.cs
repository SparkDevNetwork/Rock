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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// Represents a persisted WorkflowActivity in Rock
    /// </summary>
    [RockDomain( "Workflow" )]
    [Table( "WorkflowActivity" )]
    [DataContract]
    public partial class WorkflowActivity : Model<WorkflowActivity>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the WorkflowId of the <see cref="Rock.Model.Workflow"/> instance that is performing this WorkflowActivity.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the WorkflowId of the <see cref="Rock.Model.Workflow"/> instance that is performing this WorkflowActivity.
        /// </value>
        [DataMember]
        public int WorkflowId { get; set; }

        /// <summary>
        /// Gets or sets the ActivityTypeId of the <see cref="Rock.Model.WorkflowActivityType"/> that is being executed.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the ActivityTypeID of the <see cref="Rock.Model.WorkflowActivity"/> that is being performed.
        /// </value>
        [DataMember]
        public int ActivityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the assigned person alias identifier.
        /// </summary>
        /// <value>
        /// The assigned person alias identifier.
        /// </value>
        [DataMember]
        public int? AssignedPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the assigned group identifier.
        /// </summary>
        /// <value>
        /// The assigned group identifier.
        /// </value>
        [DataMember]
        public int? AssignedGroupId { get; set; }

        /// <summary>
        /// Gets or sets the date and time that this WorkflowActivity was activated.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time that this WorkflowActivity was activated.
        /// </value>
        [DataMember]
        public DateTime? ActivatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the activated by activity identifier.
        /// </summary>
        /// <value>
        /// The activated by activity identifier.
        /// </value>
        [DataMember]
        public int? ActivatedByActivityId { get; set; }

        /// <summary>
        /// Gets or sets the date and time that this WorkflowActivity was last processed.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time that this WorkflowActivity was last processed.
        /// </value>
        [DataMember]
        [NotAudited]
        public DateTime? LastProcessedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time that this WorkflowActivity completed.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time that this WorkflowActivity completed.
        /// </value>
        [DataMember]
        public DateTime? CompletedDateTime { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Workflow"/> instance that is performing this WorkflowActivity.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Workflow"/> instance that is performing this WorkflowActivity.
        /// </value>
        [LavaVisible]
        public virtual Workflow Workflow { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.WorkflowActivityType"/> that is being performed by this WorkflowActivity instance.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.WorkflowActivityType"/> that is being performed by this WorkflowActivity instance.
        /// </value>
        [LavaVisible]
        public virtual WorkflowActivityType ActivityType { get; set; }

        /// <summary>
        /// Gets or sets the assigned person alias.
        /// </summary>
        /// <value>
        /// The assigned person alias.
        /// </value>
        [LavaVisible]
        public virtual PersonAlias AssignedPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the assigned group.
        /// </summary>
        /// <value>
        /// The assigned group.
        /// </value>
        [LavaVisible]
        public virtual Group AssignedGroup { get; set; }

        /// <summary>
        /// Gets or sets the activated by activity.
        /// </summary>
        /// <value>
        /// The activated by activity.
        /// </value>
        [LavaVisible]
        public virtual WorkflowActivity ActivatedByActivity { get; set; }

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.WorkflowAction">WorkflowActions</see> that are run by this WorkflowActivity.
        /// </summary>
        /// <value>
        /// A collection containing the <see cref="Rock.Model.WorkflowAction">WorkflowActions</see> that are being run by this WorkflowActivity.
        /// </value>
        [DataMember]
        public virtual ICollection<WorkflowAction> Actions
        {
            get { return _actions ?? ( _actions = new Collection<WorkflowAction>() ); }
            set { _actions = value; }
        }
        private ICollection<WorkflowAction> _actions;

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// WorkflowActivity Configuration class.
    /// </summary>
    public partial class WorkflowActivityConfiguration : EntityTypeConfiguration<WorkflowActivity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowActivityConfiguration"/> class.
        /// </summary>
        public WorkflowActivityConfiguration()
        {
            this.HasRequired( a => a.Workflow ).WithMany( a => a.Activities ).HasForeignKey( a => a.WorkflowId ).WillCascadeOnDelete( true );
            this.HasRequired( a => a.ActivityType ).WithMany().HasForeignKey( a => a.ActivityTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.AssignedPersonAlias ).WithMany().HasForeignKey( a => a.AssignedPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.AssignedGroup ).WithMany().HasForeignKey( a => a.AssignedGroupId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.ActivatedByActivity ).WithMany().HasForeignKey( a => a.ActivatedByActivityId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}

