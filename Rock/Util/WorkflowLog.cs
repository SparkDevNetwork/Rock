//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// WorkflowLog POCO Entity.
    /// </summary>
    [Table( "WorkflowLog" )]
    public partial class WorkflowLog : Entity<WorkflowLog>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the workflow id.
        /// </summary>
        /// <value>
        /// The workflow id.
        /// </value>
        public int WorkflowId { get; set; }

        /// <summary>
        /// Gets or sets the log date time.
        /// </summary>
        /// <value>
        /// The log date time.
        /// </value>
        [Required]
        public DateTime LogDateTime { get; set; }

        /// <summary>
        /// Gets or sets the log entry.
        /// </summary>
        /// <value>
        /// The log entry.
        /// </value>
        [Required]
        public string LogText { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the  workflow.
        /// </summary>
        /// <value>
        /// The workflow.
        /// </value>
        public virtual Workflow Workflow { get; set; }

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
            return string.Format( "{0}: {1}", this.LogDateTime.ToString(), this.LogText );
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Static Method to return an object based on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static WorkflowLog Read( int id )
        {
            return Read<WorkflowLog>( id );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public static WorkflowLog Read( Guid guid )
        {
            return Read<WorkflowLog>( guid );
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// WorkflowLog Configuration class.
    /// </summary>
    public partial class WorkflowLogConfiguration : EntityTypeConfiguration<WorkflowLog>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowLogConfiguration"/> class.
        /// </summary>
        public WorkflowLogConfiguration()
        {
            this.HasRequired( m => m.Workflow ).WithMany( m => m.LogEntries).HasForeignKey( m => m.WorkflowId ).WillCascadeOnDelete( true );
        }
    }

    #endregion

}

