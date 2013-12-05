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
    /// Represents a WorkflowLog entry of a <see cref="Rock.Model.Workflow"/> instance event.
    /// </summary>
    [Table( "WorkflowLog" )]
    [DataContract]
    public partial class WorkflowLog : Entity<WorkflowLog>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the WorkflowId of the <see cref="Rock.Model.Workflow"/> instance that is being logged.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the WorkflowId of the <see cref="Rock.Model.Workflow"/> instance that is being logged.
        /// </value>
        [DataMember]
        public int WorkflowId { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the WorkflowLog entry was created. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time that this WorkflowLog entry was created.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public DateTime LogDateTime { get; set; }

        /// <summary>
        /// Gets or sets the body/text of the WorkflowLog entry. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the body/text of the WorkflowLog entry.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public string LogText { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Workflow"/> instance that is being logged.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Workflow"/> that is being logged.
        /// </value>
        [DataMember]
        public virtual Workflow Workflow { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this WorkflowLog.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this WorkflowLog.
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

