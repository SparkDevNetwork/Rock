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

namespace Rock.Model
{
    /// <summary>
    /// Represents a step workflow in Rock.
    /// </summary>
    [RockDomain( "Engagement" )]
    [Table( "StepWorkflow" )]
    [DataContract]
    public partial class StepWorkflow : Model<StepWorkflow>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.StepWorkflowTrigger"/> by which this Workflow was triggered. This property is required.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int StepWorkflowTriggerId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Workflow"/> that was triggered. This property is required.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int WorkflowId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Step"/> that triggered the workflow. This property is required.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int StepId { get; set; }

        #endregion Entity Properties

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.StepWorkflowTrigger"/>.
        /// </summary>
        [DataMember]
        public virtual StepWorkflowTrigger StepWorkflowTrigger { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Step"/>.
        /// </summary>
        [DataMember]
        public virtual Step Step { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Workflow"/>.
        /// </summary>
        [DataMember]
        public virtual Workflow Workflow { get; set; }

        #endregion Virtual Properties

        #region Entity Configuration

        /// <summary>
        /// Step Workflow Configuration class.
        /// </summary>
        public partial class StepWorkflowConfiguration : EntityTypeConfiguration<StepWorkflow>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StepWorkflowConfiguration"/> class.
            /// </summary>
            public StepWorkflowConfiguration()
            {
                HasRequired( sw => sw.StepWorkflowTrigger ).WithMany( swt => swt.StepWorkflows ).HasForeignKey( sw => sw.StepWorkflowTriggerId ).WillCascadeOnDelete( true );
                HasRequired( sw => sw.Workflow ).WithMany().HasForeignKey( sw => sw.WorkflowId ).WillCascadeOnDelete( true );
                HasRequired( sw => sw.Step ).WithMany( s => s.StepWorkflows ).HasForeignKey( sw => sw.StepId ).WillCascadeOnDelete( true );
            }
        }

        #endregion
    }
}
