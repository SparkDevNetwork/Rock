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
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a WorkflowActivityType or set of <see cref="Rock.Model.WorkflowActionType">ActionsTypes</see> that are executed/performed as part of a <see cref="Rock.Model.WorkflowType"/>
    /// </summary>
    [Table( "WorkflowActivityType" )]
    [DataContract]
    public partial class WorkflowActivityType : Model<WorkflowActivityType>, IOrdered
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this WorkflowActivityType is active.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the WorkflowActivityType is active; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool? IsActive { get; set; }

        /// <summary>
        /// Gets or sets the WorkflowTypeId of the <see cref="Rock.Model.WorkflowType"/> that this WorkflowActivityType belongs to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the WorkflowTypeId of the <see cref="Rock.Model.WorkflowType"/> that this WorkflowActivityType belongs to.
        /// </value>
        [DataMember]
        public int WorkflowTypeId { get; set; }

        /// <summary>
        /// Gets or sets the friendly Name of this WorkflowActivityType. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the friendly name of this WorkflowActivityType
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description or summary about this WorkflowActivityType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing a description or summary about this WorkflowActivityType.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if this WorkflowActivityType is activated with the workflow.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this instance is activated with workflow; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActivatedWithWorkflow { get; set; }

        /// <summary>
        /// Gets or sets the order that this WorkflowActivityType will be executed in the WorkflowType's process. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> indicating the order that this Activity will be executed in the Workflow.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int Order { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.WorkflowType"/> that runs this WorkflowActivityType.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.WorkflowType"/> that runs this WorkflowActivityType.
        /// </value>
        public virtual WorkflowType WorkflowType { get; set; }

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.WorkflowActionType">WorkflowActionTypes</see> that are 
        /// performed by this WorkflowActivityType.
        /// </summary>
        /// <value>
        /// The action types.
        /// </value>
        [DataMember]
        public virtual ICollection<WorkflowActionType> ActionTypes
        {
            get { return _actionTypes ?? ( _actionTypes = new Collection<WorkflowActionType>() ); }
            set { _actionTypes = value; }
        }
        private ICollection<WorkflowActionType> _actionTypes;

        /// <summary>
        /// Gets the parent security authority for this WorkflowActivityType. 
        /// </summary>
        /// <value>
        /// An entity object implementing the  <see cref="Security.ISecured"/> interface, representing the parent security authority.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.WorkflowType != null ? this.WorkflowType : base.ParentAuthority;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this WorkflowActivityType.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this WorkflowActivityType.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// ActivityType Configuration class.
    /// </summary>
    public partial class WorkflowActivityTypeConfiguration : EntityTypeConfiguration<WorkflowActivityType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowActivityTypeConfiguration"/> class.
        /// </summary>
        public WorkflowActivityTypeConfiguration()
        {
            this.HasRequired( m => m.WorkflowType ).WithMany( m => m.ActivityTypes ).HasForeignKey( m => m.WorkflowTypeId ).WillCascadeOnDelete( true );
        }
    }

    #endregion

}

