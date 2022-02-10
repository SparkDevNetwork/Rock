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
using Rock.Financial;

namespace Rock.Model
{
    /// <summary>
    /// Represents a financial statement Template in Rock.
    /// </summary>
    [RockDomain( "Finance" )]
    [Table( "FinancialStatementTemplate" )]
    [DataContract]
    public partial class FinancialStatementTemplate : Model<FinancialStatementTemplate>, IHasActiveFlag
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the Financial Statement Template
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the name of the Financial Statement Template
        /// </value>
        [Required]
        [MaxLength( 50 )]
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
        /// Gets or sets a flag indicating if this is an active financial statement template. This value is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this financial statement template is active, otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Previewable]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the report template.
        /// </summary>
        /// <value>
        /// The report template.
        /// </value>
        [DataMember]
        public string ReportTemplate { get; set; }

        /// <summary>
        /// Gets or sets the JSON for <see cref="FooterSettings"/>
        /// </summary>
        /// <value>
        /// The footer template.
        /// </value>
        [DataMember]
        public string FooterSettingsJson
        {
            get
            {
                return FooterSettings.ToJson();
            }

            set
            {
                FooterSettings = value.FromJsonOrNull<FinancialStatementTemplateHeaderFooterSettings>() ?? new FinancialStatementTemplateHeaderFooterSettings();
            }
        }

        /// <summary>
        /// Gets or sets the JSON for <see cref="ReportSettings"/>
        /// </summary>
        /// <value>
        /// The report settings.
        /// </value>
        [DataMember]
        public string ReportSettingsJson
        {
            get
            {
                return ReportSettings.ToJson();
            }

            set
            {
                ReportSettings = value.FromJsonOrNull<FinancialStatementTemplateReportSettings>() ?? new FinancialStatementTemplateReportSettings();
            }
        }

        /// <summary>
        /// Gets or sets the image file identifier for the Logo Image
        /// </summary>
        /// <value>
        /// The Logo file identifier.
        /// </value>
        [DataMember]
        public int? LogoBinaryFileId { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the logo binary file.
        /// </summary>
        /// <value>
        /// The logo binary file.
        /// </value>
        [DataMember]
        public virtual BinaryFile LogoBinaryFile { get; set; }

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// Financial Statement Template Configuration class.
    /// </summary>
    public partial class FinancialStatementTemplateConfiguration : EntityTypeConfiguration<FinancialStatementTemplate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialStatementTemplateConfiguration"/> class.
        /// </summary>
        public FinancialStatementTemplateConfiguration()
        {
            this.HasOptional( c => c.LogoBinaryFile ).WithMany().HasForeignKey( c => c.LogoBinaryFileId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}