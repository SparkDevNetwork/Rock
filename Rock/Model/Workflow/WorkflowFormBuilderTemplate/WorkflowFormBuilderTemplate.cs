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
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a WorkflowFormBuilderTemplate in Rock
    /// </summary>
    [RockDomain( "Workflow" )]
    [Table( "WorkflowFormBuilderTemplate" )]
    [DataContract]
    public partial class WorkflowFormBuilderTemplate : Model<WorkflowFormBuilderTemplate>, IHasActiveFlag, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the friendly Name of this WorkflowFormBuilderTemplate. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the friendly name of this WorkflowFormBuilderTemplate
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description or summary about this WorkflowFormBuilderTemplate.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing a description or summary about this WorkflowFormBuilderTemplate.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the form header.
        /// </summary>
        /// <value>
        /// The form header.
        /// </value>
        [DataMember]
        public string FormHeader { get; set; }

        /// <summary>
        /// Gets or sets the footer.
        /// </summary>
        /// <value>
        /// The footer.
        /// </value>
        [DataMember]
        public string FormFooter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a new person (and spouse) can be added
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow person entry]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowPersonEntry { get; set; }

        /// <summary>
        /// Gets or sets the person entry settings json.
        /// </summary>
        /// <value>
        /// The person entry settings json.
        /// </value>
        [DataMember]
        public string PersonEntrySettingsJson { get; set; }

        /// <summary>
        /// Gets or sets the confirmation email settings json.
        /// </summary>
        /// <value>
        /// The confirmation email settings json.
        /// </value>
        [DataMember]
        public string ConfirmationEmailSettingsJson { get; set; }

        /// <summary>
        /// Gets or sets the completion settings json.
        /// </summary>
        /// <value>
        /// The completion settings json.
        /// </value>
        [DataMember]
        public string CompletionSettingsJson { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is login required].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is login required]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsLoginRequired { get; set; }

        #endregion

        #region overrides

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
    /// Workflow Form Builder Template Configuration class.
    /// </summary>
    public partial class WorkflowFormBuilderTemplateConfiguration : EntityTypeConfiguration<WorkflowFormBuilderTemplate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowFormBuilderTemplateConfiguration"/> class.
        /// </summary>
        public WorkflowFormBuilderTemplateConfiguration()
        {
        }
    }

    #endregion Entity Configuration
}
