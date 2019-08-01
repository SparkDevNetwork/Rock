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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
//using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.Infrastructure;

using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a step in Rock.
    /// </summary>
    [RockDomain( "Steps" )]
    [Table( "Step" )]
    [DataContract]
    public partial class Step : Model<Step>, IOrdered
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="StepType"/> to which this step belongs. This property is required.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int StepTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="StepStatus"/> to which this step belongs.
        /// </summary>
        [DataMember]
        public int? StepStatusId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="PersonAlias"/> that identifies the Person associated with taking this step. This property is required.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Campus"/> associated with this step.
        /// </summary>
        [DataMember]
        public int? CampusId { get; set; }
        
        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> associated with the completion of this step.
        /// </summary>
        [DataMember]
        public DateTime? CompletedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> associated with the start of this step.
        /// </summary>
        [DataMember]
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> associated with the end of this step.
        /// </summary>
        [DataMember]
        public DateTime? EndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        [DataMember]
        public string Note { get; set; }

        #endregion Entity Properties

        #region IOrdered

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        [DataMember]
        public int Order { get; set; }

        #endregion IOrdered

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the Step Type.
        /// </summary>
        [DataMember]
        public virtual StepType StepType { get; set; }

        /// <summary>
        /// Gets or sets the Step Status.
        /// </summary>
        [DataMember]
        public virtual StepStatus StepStatus { get; set; }

        /// <summary>
        /// Gets or sets the Person Alias.
        /// </summary>
        [DataMember]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the Campus.
        /// </summary>
        [DataMember]
        public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets a collection containing the <see cref="StepWorkflow">StepWorkflows</see> that are of this step.
        /// </summary>
        [DataMember]
        public virtual ICollection<StepWorkflow> StepWorkflows
        {
            get => _stepWorkflows ?? ( _stepWorkflows = new Collection<StepWorkflow>() );
            set => _stepWorkflows = value;
        }
        private ICollection<StepWorkflow> _stepWorkflows;

        /// <summary>
        /// Indicates if this step has been completed
        /// </summary>
        [DataMember]
        public virtual bool IsComplete
        {
            get => StepStatus != null && StepStatus.IsCompleteStatus;
        }

        #endregion Virtual Properties

        #region Overrides

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        public override bool IsValid
        {
            get
            {
                var isValid = base.IsValid;

                if ( StartDateTime.HasValue && EndDateTime.HasValue && StartDateTime.Value > EndDateTime.Value )
                {
                    ValidationResults.Add( new ValidationResult( "The StartDateTime must occur before the EndDateTime" ) );
                    isValid = false;
                }

                if ( StartDateTime.HasValue && CompletedDateTime.HasValue && StartDateTime.Value > CompletedDateTime.Value )
                {
                    ValidationResults.Add( new ValidationResult( "The StartDateTime must occur before the CompletedDateTime" ) );
                    isValid = false;
                }

                return isValid;
            }
        }

        /// <summary>
        /// Perform tasks prior to saving changes to this entity.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The entry.</param>
        public override void PreSaveChanges( DbContext dbContext, DbEntityEntry entry )
        {
            // Add a transaction to process workflows associated with changes to this Step.
            var transaction = new Rock.Transactions.StepChangeTransaction( entry );

            Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );

            base.PreSaveChanges( dbContext, entry );
        }

        #endregion Overrides

    #region Entity Configuration

    /// <summary>
    /// Step Configuration class.
    /// </summary>
    public partial class StepConfiguration : EntityTypeConfiguration<Step>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StepConfiguration"/> class.
            /// </summary>
            public StepConfiguration()
            {
                HasRequired( s => s.StepType ).WithMany( st => st.Steps ).HasForeignKey( s => s.StepTypeId ).WillCascadeOnDelete( true );
                HasRequired( s => s.PersonAlias ).WithMany().HasForeignKey( s => s.PersonAliasId ).WillCascadeOnDelete( true );

                HasOptional( s => s.Campus ).WithMany().HasForeignKey( s => s.CampusId ).WillCascadeOnDelete( false );
                HasOptional( s => s.StepStatus ).WithMany( ss => ss.Steps ).HasForeignKey( s => s.StepStatusId ).WillCascadeOnDelete( false );
            }
        }

        #endregion
    }
}
