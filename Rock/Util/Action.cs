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

namespace Rock.Util
{
    /// <summary>
    /// Action POCO Entity.
    /// </summary>
    [Table( "utilAction" )]
    public partial class Action : Model<Action>
    {
        #region Properties

        /// <summary>
        /// Gets or sets the activity id.
        /// </summary>
        /// <value>
        /// The activity id.
        /// </value>
        public int ActivityId { get; set; }

        /// <summary>
        /// Gets or sets the  activity.
        /// </summary>
        /// <value>
        /// The activity.
        /// </value>
        public virtual Activity Activity { get; set; }

        /// <summary>
        /// Gets or sets the activity type id.
        /// </summary>
        /// <value>
        /// The activity type id.
        /// </value>
        public int ActionTypeId { get; set; }

        /// <summary>
        /// Gets or sets the type of the activity.
        /// </summary>
        /// <value>
        /// The type of the activity.
        /// </value>
        public virtual ActionType ActionType { get; set; }

        /// <summary>
        /// Gets or sets the last processed date time.
        /// </summary>
        /// <value>
        /// The last processed date time.
        /// </value>
        public DateTime? LastProcessedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the completed date time.
        /// </summary>
        /// <value>
        /// The completed date time.
        /// </value>
        public DateTime? CompletedDateTime { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsActive
        {
            get
            {
                return !CompletedDateTime.HasValue;
            }
        }

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.Activity;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Processes this instance.
        /// </summary>
        /// <exception cref="System.SystemException"></exception>
        public virtual void Process()
        {
            WorkflowActionComponent workflowAction = this.ActionType.WorkflowAction;
            if ( workflowAction == null )
            {
                throw new SystemException( string.Format( "The '{0}' component does not exist, or is not active", workflowAction));
            }

            bool success = workflowAction.Execute(this.Activity.Workflow);

            if ( success )
            {
                if ( this.ActionType.IsActionCompletedOnSuccess )
                {
                    this.CompletedDateTime = DateTime.Now;
                }

                if ( this.ActionType.IsActivityCompletedOnSuccess )
                {
                    this.Activity.CompletedDateTime = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format( "{0} : {1}", this.ActionType.ToString(), this.Id );
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Static Method to return an object based on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static Action Read( int id )
        {
            return Read<Action>( id );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public static Action Read( Guid guid )
        {
            return Read<Action>( guid );
        }

        #endregion

    }

    #region EF Configuration

    /// <summary>
    /// Action Configuration class.
    /// </summary>
    public partial class ActionConfiguration : EntityTypeConfiguration<Action>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionConfiguration"/> class.
        /// </summary>
        public ActionConfiguration()
        {
            this.HasRequired( m => m.Activity ).WithMany( m => m.Actions ).HasForeignKey( m => m.ActivityId ).WillCascadeOnDelete( true );
            this.HasRequired( m => m.ActionType ).WithMany().HasForeignKey( m => m.ActionTypeId).WillCascadeOnDelete( false );
        }
    }

    #endregion
}

