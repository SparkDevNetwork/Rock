//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// WorkflowLog POCO Entity.
    /// </summary>
    [Table( "WorkflowLog" )]
    [DataContract( IsReference = true )]
    public partial class WorkflowLog : Entity<WorkflowLog>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the workflow id.
        /// </summary>
        /// <value>
        /// The workflow id.
        /// </value>
        [DataMember]
        public int WorkflowId { get; set; }

        /// <summary>
        /// Gets or sets the log date time.
        /// </summary>
        /// <value>
        /// The log date time.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public DateTime LogDateTime { get; set; }

        /// <summary>
        /// Gets or sets the log entry.
        /// </summary>
        /// <value>
        /// The log entry.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public string LogText { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the  workflow.
        /// </summary>
        /// <value>
        /// The workflow.
        /// </value>
        [DataMember]
        public virtual Workflow Workflow { get; set; }

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

