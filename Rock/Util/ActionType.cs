//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using Rock.Data;

namespace Rock.Util
{
    /// <summary>
    /// ActionType POCO Entity.
    /// </summary>
    [Table( "utilActionType" )]
    public partial class ActionType : Model<ActionType>, IOrdered
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the activity type id.
        /// </summary>
        /// <value>
        /// The activity type id.
        /// </value>
        public int ActivityTypeId { get; set; }

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
        /// Gets or sets the Order.
        /// </summary>
        /// <value>
        /// Order.
        /// </value>
        [Required]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the MEF service name of the action.
        /// </summary>
        /// <value>
        /// The name of the action service.
        /// </value>
        [Required]
        [MaxLength( 200 )]
        public string ActionServiceName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is action completed on success.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is action completed on success; otherwise, <c>false</c>.
        /// </value>
        public bool IsActionCompletedOnSuccess { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is activity completed on success.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is activity completed on success; otherwise, <c>false</c>.
        /// </value>
        public bool IsActivityCompletedOnSuccess { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the type of the activity.
        /// </summary>
        /// <value>
        /// The type of the activity.
        /// </value>
        public virtual ActivityType ActivityType { get; set; }

        /// <summary>
        /// Gets the workflow action.
        /// </summary>
        /// <value>
        /// The workflow action.
        /// </value>
        public virtual WorkflowActionComponent WorkflowAction
        {
            get
            {
                foreach ( var serviceEntry in WorkflowActionContainer.Instance.Components )
                {
                    var component = serviceEntry.Value.Value;
                    string componentName = component.GetType().FullName;
                    if (
                        componentName == this.ActionServiceName &&
                        component.AttributeValues.ContainsKey( "Active" ) &&
                        bool.Parse( component.AttributeValues["Active"][0].Value )
                    )
                    {
                        return component;
                    }
                }
                return null;
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
                return this.ActivityType;
            }
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
        public static ActionType Read( int id )
        {
            return Read<ActionType>( id );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public static ActionType Read( Guid guid )
        {
            return Read<ActionType>( guid );
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// ActionType Configuration class.
    /// </summary>
    public partial class ActionTypeConfiguration : EntityTypeConfiguration<ActionType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionTypeConfiguration"/> class.
        /// </summary>
        public ActionTypeConfiguration()
        {
            this.HasRequired( m => m.ActivityType ).WithMany( m => m.ActionTypes ).HasForeignKey( m => m.ActivityTypeId ).WillCascadeOnDelete( true );
        }
    }

    #endregion

}

