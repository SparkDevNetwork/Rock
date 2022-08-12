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

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Lava;
using Rock.Security;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [RockDomain( "Group" )]
    [Table( "GroupRequirementType" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "8E67E852-D1BF-485C-9898-09F19998CC40" )]
    public partial class GroupRequirementType : Model<GroupRequirementType>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this requirement can expire.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can expire; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [DataMember]
        public bool CanExpire { get; set; }

        /// <summary>
        /// Gets or sets the number of days after the requirement is met before it expires (If CanExpire is true). NULL means never expires
        /// </summary>
        /// <value>
        /// The expire in days.
        /// </value>
        [DataMember]
        public int? ExpireInDays { get; set; }

        /// <summary>
        /// Gets or sets the type of the requirement check.
        /// </summary>
        /// <value>
        /// The type of the requirement check.
        /// </value>
        [DataMember]
        public RequirementCheckType RequirementCheckType { get; set; }

        /// <summary>
        /// Gets or sets the SQL expression.
        /// </summary>
        /// <value>
        /// The SQL expression.
        /// </value>
        [DataMember]
        public string SqlExpression { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DataView"/> identifier.
        /// </summary>
        /// <value>
        /// The data view identifier.
        /// </value>
        [DataMember]
        public int? DataViewId { get; set; }

        /// <summary>
        /// Gets or sets the warning SQL expression.
        /// </summary>
        /// <value>
        /// The warning SQL expression.
        /// </value>
        [DataMember]
        public string WarningSqlExpression { get; set; }

        /// <summary>
        /// Gets or sets the warning <see cref="Rock.Model.DataView"/> identifier.
        /// </summary>
        /// <value>
        /// The warning data view identifier.
        /// </value>
        [DataMember]
        public int? WarningDataViewId { get; set; }

        /// <summary>
        /// Gets or sets the positive label. This is the text that is displayed when the requirement is met.
        /// </summary>
        /// <value>
        /// The positive label.
        /// </value>
        [MaxLength( 150 )]
        [DataMember]
        public string PositiveLabel { get; set; }

        /// <summary>
        /// Gets or sets the negative label. This is the text that is displayed when the requirement is not met.
        /// </summary>
        /// <value>
        /// The negative label.
        /// </value>
        [MaxLength( 150 )]
        [DataMember]
        public string NegativeLabel { get; set; }

        /// <summary>
        /// Gets or sets the warning label.
        /// </summary>
        /// <value>
        /// The warning label.
        /// </value>
        [DataMember]
        public string WarningLabel { get; set; }

        /// <summary>
        /// Gets or sets the checkbox label. This is the text that is used for the checkbox if this is a manually set requirement
        /// </summary>
        /// <value>
        /// The checkbox label.
        /// </value>
        [MaxLength( 150 )]
        [DataMember]
        public string CheckboxLabel { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the type of due date.
        /// </summary>
        /// <value>
        /// The type of due date.
        /// </value>
        [DataMember]
        [DefaultValue( DueDateType.Immediate )]
        public DueDateType DueDateType { get; set; }

        /// <summary>
        /// Gets or sets the number of days before the requirement is due.
        /// </summary>
        /// <value>
        /// The due date offset in days.
        /// </value>
        [DataMember]
        public int? DueDateOffsetInDays { get; set; }

        /// <summary>
        /// Gets or sets the category identifier.
        /// </summary>
        /// <value>
        /// The category identifier.
        /// </value>
        [DataMember]
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.WorkflowType"/> identifier for the group requirement type it does not meet.
        /// </summary>
        /// <value>
        /// The workflow type identifier.
        /// </value>
        [DataMember]
        public int? DoesNotMeetWorkflowTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this requirement type's "Does Not Meet" workflow should auto-initiate.
        /// </summary>
        [DataMember]
        [DefaultValue( false )]
        public bool ShouldAutoInitiateDoesNotMeetWorkflow { get; set; }

        /// <summary>
        /// Gets or sets the text for the "Does Not Meet" workflow link.
        /// </summary>
        [MaxLength( 50 )]
        [DataMember]
        public string DoesNotMeetWorkflowLinkText { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.WorkflowType"/> identifier for the group requirement type's warning.
        /// </summary>
        /// <value>
        /// The workflow type identifier.
        /// </value>
        [DataMember]
        public int? WarningWorkflowTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this requirement type's "Warning" workflow should auto-initiate.
        /// </summary>
        [DataMember]
        [DefaultValue( false )]
        public bool ShouldAutoInitiateWarningWorkflow { get; set; }

        /// <summary>
        /// Gets or sets the text for the "Warning" workflow link.
        /// </summary>
        [MaxLength( 50 )]
        [DataMember]
        public string WarningWorkflowLinkText { get; set; }

        /// <summary>
        /// Gets or sets the summary.
        /// </summary>
        /// <value>
        /// The summary.
        /// </value>
        [MaxLength( 2000 )]
        [DataMember]
        public string Summary { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DataView"/>.
        /// </summary>
        /// <value>
        /// The data view.
        /// </value>
        [LavaVisible]
        public virtual DataView DataView { get; set; }

        /// <summary>
        /// Gets or sets the warning <see cref="Rock.Model.DataView"/>.
        /// </summary>
        /// <value>
        /// The warning data view.
        /// </value>
        [LavaVisible]
        public virtual DataView WarningDataView { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        [DataMember]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets or sets "Does Not Meet" workflow type.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        [DataMember]
        public virtual WorkflowType DoesNotMeetWorkflowType { get; set; }

        /// <summary>
        /// Gets or sets "Warning" workflow type.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        [DataMember]
        public virtual WorkflowType WarningWorkflowType { get; set; }

        /// <summary>
        /// Provides a <see cref="Dictionary{TKey, TValue}"/> of actions that this model supports, and the description of each.
        /// </summary>
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                if ( _supportedActions == null )
                {
                    _supportedActions = new Dictionary<string, string>();
                    _supportedActions.Add( Authorization.VIEW, "The roles and/or users that have access to view." );
                    _supportedActions.Add( Authorization.EDIT, "The roles and/or users that have access to edit." );
                    _supportedActions.Add( Authorization.ADMINISTRATE, "The roles and/or users that have access to administrate." );
                    _supportedActions.Add( Authorization.OVERRIDE, "The roles and/or users that have access to override these requirement types." );
                }

                return _supportedActions;
            }
        }

        private Dictionary<string, string> _supportedActions;

        #endregion

        #region Public Methods

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
    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class GroupRequirementTypeConfiguration : EntityTypeConfiguration<GroupRequirementType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupRequirementTypeConfiguration"/> class.
        /// </summary>
        public GroupRequirementTypeConfiguration()
        {
            this.HasOptional( a => a.DataView ).WithMany().HasForeignKey( a => a.DataViewId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.WarningDataView ).WithMany().HasForeignKey( a => a.WarningDataViewId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.Category ).WithMany().HasForeignKey( a => a.CategoryId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.DoesNotMeetWorkflowType ).WithMany().HasForeignKey( a => a.DoesNotMeetWorkflowTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.WarningWorkflowType ).WithMany().HasForeignKey( a => a.WarningWorkflowTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}