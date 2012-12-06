//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Rock.Data;
using Rock.Model;

namespace Rock.Model
{
    /// <summary>
    /// WorkflowType POCO Entity.
    /// </summary>
    [Table( "WorkflowType" )]
    public partial class WorkflowType : Model<WorkflowType>, IOrdered
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// Determines whether the job is a system job..
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the is active.
        /// </summary>
        /// <value>
        /// The is active.
        /// </value>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        /// <value>
        /// Friendly name for the job..
        /// </value>
        [Required]
        [MaxLength( 100 )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        /// <value>
        /// Notes about the job..
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the category id.
        /// </summary>
        /// <value>
        /// The category id.
        /// </value>
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the Order.
        /// </summary>
        /// <value>
        /// Order.
        /// </value>
        [Required]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the file id.
        /// </summary>
        /// <value>
        /// The file id.
        /// </value>
        public int? FileId { get; set; }

        /// <summary>
        /// Gets or sets the work term.
        /// </summary>
        /// <value>
        /// The work term.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        public string WorkTerm { get; set; }

        /// <summary>
        /// Gets or sets the processing interval seconds.
        /// </summary>
        /// <value>
        /// The processing interval seconds.
        /// </value>
        public int? ProcessingIntervalSeconds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is persisted.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is persisted; otherwise, <c>false</c>.
        /// </value>
        public bool IsPersisted { get; set; }

        /// <summary>
        /// Gets or sets the logging level.
        /// </summary>
        /// <value>
        /// The logging level.
        /// </value>
        public WorkflowLoggingLevel LoggingLevel { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets or sets the file.
        /// </summary>
        /// <value>
        /// The file.
        /// </value>
        public virtual BinaryFile File { get; set; }

        /// <summary>
        /// Gets or sets the activity types.
        /// </summary>
        /// <value>
        /// The activity types.
        /// </value>
        public virtual ICollection<WorkflowActivityType> ActivityTypes
        {
            get { return _activityTypes ?? ( _activityTypes = new Collection<WorkflowActivityType>() ); }
            set { _activityTypes = value; }
        }
        private ICollection<WorkflowActivityType> _activityTypes;

        /// <summary>
        /// Gets the dto.
        /// </summary>
        /// <returns></returns>
        public override IDto Dto
        {
            get { return this.ToDto(); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Static Method to return an object based on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static WorkflowType Read( int id )
        {
            return Read<WorkflowType>( id );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public static WorkflowType Read( Guid guid )
        {
            return Read<WorkflowType>( guid );
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
            this.HasOptional( m => m.File ).WithMany().HasForeignKey( m => m.FileId ).WillCascadeOnDelete( false );
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

