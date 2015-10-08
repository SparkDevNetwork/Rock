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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Security;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// WorkflowType POCO Entity.
    /// Represents a WorkflowType or workflow definition in Rock. WorkflowTypes are categorizable and orderable, through the implementation of <see cref="Rock.Data.ICategorized"/>
    /// and <see cref="Rock.Data.IOrdered"/> respectively. A WorkFlowType is a predetermined set of steps or <see cref="Rock.Model.WorkflowActivityType">activities</see>
    /// to be performed by the system, by a user or a combination of the two. A workflow can be used for any process that can have multiple steps that need to be performed in 
    /// a specific and constant order, and can have divergent paths based on input or data.  A workflow can either be persisted to the database, 
    /// for long running workflows, or non persisted (see <see cref="Rock.Model.Workflow"/>for real-time processes (i.e. a wizard or triggered job).  A workflow can be triggered by a user/process
    /// performing an action or an entity being updated (through <see cref="Rock.Model.WorkflowTrigger">WorkflowTriggers</see>).
    /// </summary>
    [Table( "WorkflowType" )]
    [DataContract]
    public partial class WorkflowType : Model<WorkflowType>, IOrdered, ICategorized
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this WorkflowType is part of the Rock core system/framework.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if the WorkflowType is part of the Rock core system/framework; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this WorkflowType is active.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if the WorkflowType is active; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool? IsActive { get; set; }

        /// <summary>
        /// Gets or sets the friendly Name of the WorkflowType. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the friendly Name of the WorkflowType.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a user defined description or summary about the WorkflowType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing a description/summary of the WorkflowType.
        /// </value>
        [DataMember]
        public string Description { get; set; }


        /// <summary>
        /// Gets or sets the CategoryId of the <see cref="Rock.Model.Category"/> that this WorkflowType belongs to. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the CategoryId of the <see cref="Rock.Model.Category"/> that the WorkflowType belongs to. 
        /// If the WorkflowType does not belong to a category, this value will be null.
        /// </value>
        [DataMember]
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the display order of the WorkFlowType, the lower the number the higher up that the WorkflowType will display in the workflow list. This
        /// property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the display order of the WorkflowType.  
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the work term for the WorkflowType. This is the action that is being performed by this WorkflowType. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the WorkItem that is being performed by this WorkflowType
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string WorkTerm { get; set; }

        /// <summary>
        /// Gets or sets the minimum length of time, in seconds, before a persisted <see cref="Rock.Model.Workflow"/> instance that implements this 
        /// WorkflowType can be re-executed.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the minimum length of time, in seconds, that must pass before the same persisted <see cref="Rock.Model.Workflow"/> instance
        /// that implements this WorkflowType can be re-executed.
        /// </value>
        [DataMember]
        public int? ProcessingIntervalSeconds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="Rock.Model.Workflow"/> instances of this WorkflowType are persisted.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if <see cref="Rock.Model.Workflow"/> instances of this WorkflowType are persisted; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsPersisted { get; set; }

        /// <summary>
        /// Gets or sets the logging level.
        /// Gets or sets the <see cref="Rock.Model.WorkflowLoggingLevel"/> indicating the level of detail that should be logged when instances of this WorkflowType are executed.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.WorkflowLoggingLevel"/> value indicating the level of detail that should be logged when instances of this WorkflowType are executed.
        /// When <c>WorkflowLoggingLevel.None</c> no details of the Workflow instance's execution will be logged.
        /// When <c>WorkflowLoggingLevel.Workflow</c> only workflow events will be logged (i.e. begin and end).
        /// When <c>WorkflowLoggingLevel.Activity</c> workflow and activity events will be logged.
        /// When <c>WorkflowLoggingLevel.Action</c> workflow, activity and action events will be logged.
        /// </value>
        [DataMember]
        public WorkflowLoggingLevel LoggingLevel { get; set; }

        /// <summary>
        /// Gets or sets the name of the icon CSS class. This property is only used for CSS based icons.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the icon CSS class. This property will be null if a file based icon is being used.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string IconCssClass { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Category"/> that this WorkflowType belongs to.
        /// </summary>
        /// <value>
        /// Teh <see cref="Rock.Model.Category"/> that this WorkflowType belongs to.
        /// </value>
        [DataMember]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets or sets a collection containing  the <see cref="Rock.Model.WorkflowActivityType">ActivityTypes</see> that will be executed/performed as part of this WorkflowType.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.WorkflowActivityType">ActivityTypes</see> that are executed/performed as part of this WorkflowType.
        /// </value>
        [DataMember]
        public virtual ICollection<WorkflowActivityType> ActivityTypes
        {
            get { return _activityTypes ?? ( _activityTypes = new Collection<WorkflowActivityType>() ); }
            set { _activityTypes = value; }
        }
        private ICollection<WorkflowActivityType> _activityTypes;

        /// <summary>
        /// Gets a value indicating whether this instance has active forms.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has active forms; otherwise, <c>false</c>.
        /// </value>
        public bool HasActiveForms
        {
            get
            {
                return ActivityTypes
                    .Where( t => t.IsActive.HasValue && t.IsActive.Value )
                    .SelectMany( t => t.ActionTypes )
                    .Where( a => a.WorkflowFormId.HasValue )
                    .Any();
            }
        }

        /// <summary>
        /// Gets the supported actions.
        /// </summary>
        /// <value>
        /// The supported actions.
        /// </value>
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                var supportedActions = base.SupportedActions;
                supportedActions.AddOrReplace( "ViewList", "The roles and/or users that have access to view the workflow lists of this type." );
                return supportedActions;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this WorkflowType.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this WorkflowType.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }


        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// WorkflowType Configuration class.
    /// </summary>
    public partial class WorkflowTypeConfiguration : EntityTypeConfiguration<WorkflowType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowTypeConfiguration"/> class.
        /// </summary>
        public WorkflowTypeConfiguration()
        {
            this.HasOptional( m => m.Category ).WithMany().HasForeignKey( m => m.CategoryId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// The level of details to log
    /// </summary>
    public enum WorkflowLoggingLevel
    {

        /// <summary>
        /// Don't log any details
        /// </summary>
        None = 0,

        /// <summary>
        /// Log workflow events
        /// </summary>
        Workflow = 1,

        /// <summary>
        /// Log workflow and activity events
        /// </summary>
        Activity = 2,

        /// <summary>
        /// Log workflow, activity, and action events
        /// </summary>
        Action = 3
    }

    #endregion

}

