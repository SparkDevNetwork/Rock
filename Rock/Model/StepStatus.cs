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
    /// Represents a step status in Rock.
    /// </summary>
    [RockDomain( "Steps" )]
    [Table( "StepStatus" )]
    [DataContract]
    public partial class StepStatus : Model<StepStatus>, IOrdered, IHasActiveFlag
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the status. This property is required.
        /// </summary>
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="StepProgram"/> to which this status belongs. This property is required.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int StepProgramId { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this status means that the step is complete.
        /// </summary>
        [DataMember]
        public bool IsCompleteStatus { get; set; } = false;

        /// <summary>
        /// Gets or sets the color of the status.
        /// </summary>
        [MaxLength( 100 )]
        [DataMember]
        public string StatusColor { get; set; }

        #endregion Entity Properties

        #region IHasActiveFlag

        /// <summary>
        /// Gets or sets a flag indicating if this item is active or not.
        /// </summary>
        [DataMember]
        public bool IsActive { get; set; } = true;

        #endregion

        #region IOrdered

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        [DataMember]
        public int Order { get; set; }

        #endregion IOrdered

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the StepProgram.
        /// </summary>
        [DataMember]
        public virtual StepProgram StepProgram { get; set; }

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Step">Steps</see> that are of this step status.
        /// </summary>
        [DataMember]
        public virtual ICollection<Step> Steps
        {
            get => _steps ?? ( _steps = new Collection<Step>() );
            set => _steps = value;
        }
        private ICollection<Step> _steps;

        #endregion Virtual Properties

        #region Entity Configuration

        /// <summary>
        /// Step Status Configuration class.
        /// </summary>
        public partial class StepStatusConfiguration : EntityTypeConfiguration<StepStatus>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StepStatusConfiguration"/> class.
            /// </summary>
            public StepStatusConfiguration()
            {
                HasRequired( ss => ss.StepProgram ).WithMany( sp => sp.StepStatuses ).HasForeignKey( sp => sp.StepProgramId ).WillCascadeOnDelete( true );
            }
        }

        #endregion
    }
}
