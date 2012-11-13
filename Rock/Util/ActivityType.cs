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
using System.Linq;
using Rock.Data;

namespace Rock.Util
{
    /// <summary>
    /// ActivityType POCO Entity.
    /// </summary>
    [Table( "utilActivityType" )]
    public partial class ActivityType : Model<ActivityType>, IOrdered
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the Active.
        /// </summary>
        /// <value>
        /// Determines is the job is currently active..
        /// </value>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Gets or sets the workflow type id.
        /// </summary>
        /// <value>
        /// The workflow type id.
        /// </value>
        public int WorkflowTypeId { get; set; }

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
        /// Gets or sets a value indicating whether this instance is activatedd with workflow.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is activatedd with workflow; otherwise, <c>false</c>.
        /// </value>
        public bool IsActivatedWithWorkflow { get; set; }

        /// <summary>
        /// Gets or sets the Order.
        /// </summary>
        /// <value>
        /// Order.
        /// </value>
        [Required]
        public int Order { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the type of the workflow.
        /// </summary>
        /// <value>
        /// The type of the workflow.
        /// </value>
        public virtual WorkflowType WorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the action types.
        /// </summary>
        /// <value>
        /// The action types.
        /// </value>
        public virtual ICollection<ActionType> ActionTypes
        {
            get { return _actionTypes ?? ( _actionTypes = new Collection<ActionType>() ); }
            set { _actionTypes = value; }
        }
        private ICollection<ActionType> _actionTypes;

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
                return this.WorkflowType;
            }
        }

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
        public static ActivityType Read( int id )
        {
            return Read<ActivityType>( id );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public static ActivityType Read( Guid guid )
        {
            return Read<ActivityType>( guid );
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// ActivityType Configuration class.
    /// </summary>
    public partial class ActivityTypeConfiguration : EntityTypeConfiguration<ActivityType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityTypeConfiguration"/> class.
        /// </summary>
        public ActivityTypeConfiguration()
        {
            this.HasRequired( m => m.WorkflowType ).WithMany( m => m.ActivityTypes ).HasForeignKey( m => m.WorkflowTypeId ).WillCascadeOnDelete( true );
        }
    }

    #endregion

}

