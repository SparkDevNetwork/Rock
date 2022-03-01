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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Lava;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a WorkflowActionFormSection in Rock
    /// </summary>
    [RockDomain( "Workflow" )]
    [Table( "WorkflowActionFormSection" )]
    [DataContract]
    public partial class WorkflowActionFormSection : Model<WorkflowActionFormSection>, IOrdered, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Title. This is usually the value that is return when the entity's ToString() function is called. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the Entity's Title.
        /// </value>
        [Required]
        [MaxLength( 500 )]
        [DataMember( IsRequired = true )]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description or summary about this WorkflowActionFormSection.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing a description or summary about this WorkflowActionFormSection.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show heading separator.
        /// </summary>
        /// <value>
        ///   A <see cref="System.Boolean"/> value that is <c>true</c> if heading separator will be shown; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool ShowHeadingSeparator { get; set; }

        /// <summary>
        /// Gets or sets the section visibility rules json.
        /// </summary>
        /// <value>
        /// The section visibility rules json.
        /// </value>
        [DataMember]
        public string SectionVisibilityRulesJSON { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [Required]
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the header.
        /// </summary>
        /// <value>
        /// The header.
        /// </value>
        [Required]
        [DataMember]
        public int WorkflowActionFormId { get; set; }

        /// <summary>
        /// Gets or sets the DefinedValueId of the <see cref="Rock.Model.DefinedValue"/> that represents the SectionType for this Workflow Avtion Form Section.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing DefinedValueId of the SectionType's <see cref="Rock.Model.DefinedValue"/> for this Workflow Avtion Form Section.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.SECTION_TYPE )]
        public int? SectionTypeValueId { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the workflow action form.
        /// </summary>
        /// <value>
        /// The workflow action form.
        /// </value>
        [LavaVisible]
        public virtual WorkflowActionForm WorkflowActionForm { get; set; }

        /// <summary>
        /// Gets or sets the Section Type <see cref="Rock.Model.DefinedValue"/> for this Action Form Section.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.DefinedValue"/> that represents the AccountType for this Action Form Section.
        /// </value>
        [DataMember]
        public virtual DefinedValue SectionType { get; set; }

        #endregion Navigation Properties

    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    public partial class WorkflowActionFormSectionConfiguration : EntityTypeConfiguration<WorkflowActionFormSection>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowActionFormSectionConfiguration"/> class.
        /// </summary>
        public WorkflowActionFormSectionConfiguration()
        {
            this.HasRequired( a => a.WorkflowActionForm ).WithMany( f => f.FormSections ).HasForeignKey( a => a.WorkflowActionFormId ).WillCascadeOnDelete( true );
            this.HasOptional( a => a.SectionType ).WithMany().HasForeignKey( a => a.SectionTypeValueId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}