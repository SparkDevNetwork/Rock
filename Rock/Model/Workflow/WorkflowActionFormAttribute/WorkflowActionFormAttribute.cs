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
using Rock.Data;
using Rock.Lava;
using Rock.Web.Cache;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// Represents an <see cref="Rock.Model.WorkflowActionFormAttribute"/> used by a <see cref="Rock.Model.WorkflowActionForm"/>.
    /// </summary>
    [RockDomain( "Workflow" )]
    [Table( "WorkflowActionFormAttribute" )]
    [DataContract]
    public partial class WorkflowActionFormAttribute : Model<WorkflowActionFormAttribute>, IOrdered, ICacheable
    {
        #region Entity Properties

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
        /// Gets or sets the attribute identifier.
        /// </summary>
        /// <value>
        /// The attribute identifier.
        /// </value>
        [Required]
        [DataMember]
        public int AttributeId { get; set; }

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
        /// Gets or sets a value indicating whether [is visible].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is visible]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is read only].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is read only]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is required].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is required]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [hide label].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [hide label]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool HideLabel { get; set; }

        /// <summary>
        /// Gets or sets the PreHTML.
        /// </summary>
        /// <value>
        /// The preHTML.
        /// </value>
        [DataMember]
        public string PreHtml { get; set; }

        /// <summary>
        /// Gets or sets the post HTML.
        /// </summary>
        /// <value>
        /// The post HTML.
        /// </value>
        [DataMember]
        public string PostHtml { get; set; }

        /// <summary>
        /// Gets the field visibility rules json.
        /// </summary>
        /// <value>
        /// The field visibility rules json.
        /// </value>
        /// <remarks>This value should never be used outside of Rock. FieldVisibilityRules should be used. </remarks>
        [DataMember]
        public string FieldVisibilityRulesJSON { get; set; }

        /// <summary>
        /// Gets or sets the size of the column.
        /// </summary>
        /// <value>
        /// The size of the column.
        /// </value>
        [DataMember]
        public int? ColumnSize { get; set; }

        /// <summary>
        /// Gets or sets the action form section identifier.
        /// </summary>
        /// <value>
        /// The action form section identifier.
        /// </value>
        [DataMember]
        public int? ActionFormSectionId { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the action form section.
        /// </summary>
        /// <value>
        /// The action form section.
        /// </value>
        [LavaVisible]
        public virtual WorkflowActionFormSection ActionFormSection { get; set; }

        /// <summary>
        /// Gets or sets the workflow action form.
        /// </summary>
        /// <value>
        /// The workflow action form.
        /// </value>
        [LavaVisible]
        public virtual WorkflowActionForm WorkflowActionForm { get; set; }

        /// <summary>
        /// Gets or sets the attribute.
        /// </summary>
        /// <value>
        /// The attribute.
        /// </value>
        [DataMember]
        public virtual Attribute Attribute { get; set; }

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// WorkflowActionFormAttribute Configuration class.
    /// </summary>
    public partial class WorkflowActionFormAttributeConfiguration : EntityTypeConfiguration<WorkflowActionFormAttribute>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowActionFormAttributeConfiguration"/> class.
        /// </summary>
        public WorkflowActionFormAttributeConfiguration()
        {
            this.HasOptional( a => a.ActionFormSection ).WithMany().HasForeignKey( a => a.ActionFormSectionId ).WillCascadeOnDelete( false );
            this.HasRequired( a => a.WorkflowActionForm ).WithMany( f => f.FormAttributes ).HasForeignKey( a => a.WorkflowActionFormId ).WillCascadeOnDelete( true );
            this.HasRequired( a => a.Attribute ).WithMany().HasForeignKey( a => a.AttributeId ).WillCascadeOnDelete( true );
        }
    }

    #endregion Entity Configuration
}

