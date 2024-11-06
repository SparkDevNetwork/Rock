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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Enums.CheckIn.Labels;
using Rock.Utility;
using Rock.ViewModels.CheckIn.Labels;
using Rock.ViewModels.Reporting;

namespace Rock.Model
{
    /// <summary>
    /// Represents a single label that will be used by the check-in system.
    /// </summary>
    [RockDomain( "Check-in" )]
    [Table( "CheckInLabel" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "8B651EB1-492F-46D0-821B-CA7355C6E6E7" )]
    public partial class CheckInLabel : Model<CheckInLabel>, IHasAdditionalSettings
    {
        #region Entity Properties

        /// <summary>
        /// The name of the check-in label that will be displayed in the UI.
        /// </summary>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// The text that describes the purpose of the label and what kind of
        /// information it shows.
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// <para>
        /// A flag indicating if this <see cref="CheckInLabel"/> is active.
        /// An in-active label will still be shown in the list of existing
        /// labels to be printed, but will not be available when adding a new
        /// label to be printed to a group.
        /// </para>
        /// <para>
        /// In-active labels will not be printed.
        /// </para>
        /// </summary>
        [DataMember]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// A flag indicating if this <see cref="CheckInLabel"/> is part of the
        /// Rock core system/framework. System labels cannot be edited or deleted.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// The format that the <see cref="Content"/> is stored in. This determines
        /// what UI is displayed for editing the label as well as how the label
        /// is printed.
        /// </summary>
        [DataMember]
        public LabelFormat LabelFormat { get; set; }

        /// <summary>
        /// The type of label. Label types are used to determine what kind of data
        /// is available to the label and also how many instances of the label are
        /// generated.
        /// </summary>
        [DataMember]
        public LabelType LabelType { get; set; }

        /// <summary>
        /// The content that describes how to generate the final label content that
        /// will be sent to the printer. The format of this value depends on
        /// <see cref="LabelFormat"/>.
        /// </summary>
        [DataMember]
        public string Content { get; set; }

        /// <summary>
        /// The image data that will be used to generate a preview of the label in
        /// the UI. This should be in PNG or JPG format.
        /// </summary>
        [DataMember]
        [HideFromReporting]
        [CodeGenExclude( CodeGenFeature.ViewModelFile )]
        public byte[] PreviewImage { get; set; }

        /// <inheritdoc/>
        [DataMember]
        [HideFromReporting]
        public string AdditionalSettingsJson { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// <para>
        /// Gets the filter criteria used to determine if this label should be
        /// printed during a check-in or check out operation.
        /// </para>
        /// <para>
        /// This will never return <c>null</c>. If no filter is defined then
        /// an instance with no rules will be returned.
        /// </para>
        /// </summary>
        /// <returns>A new instance of <see cref="FieldFilterGroupBag"/> that defines the criteria.</returns>
        public FieldFilterGroupBag GetConditionalPrintCriteria()
        {
            return this.GetAdditionalSettingsOrNull<FieldFilterGroupBag>( "Rock.Model.CheckInLabel.ConditionalCriteria" )
                ?? new FieldFilterGroupBag
                {
                    Guid = System.Guid.NewGuid(),
                    ExpressionType = FilterExpressionType.GroupAll,
                    Rules = new List<FieldFilterRuleBag>()
                };
        }

        /// <summary>
        /// Sets the filter critiera used to determine if this label should be
        /// printed during a check-in or check out operation.
        /// </summary>
        /// <param name="conditionalPrintCriteria">The filter criteria for the label.</param>
        public void SetConditionalPrintCriteria( FieldFilterGroupBag conditionalPrintCriteria )
        {
            if ( conditionalPrintCriteria == null )
            {
                conditionalPrintCriteria = new FieldFilterGroupBag
                {
                    Guid = System.Guid.NewGuid(),
                    ExpressionType = FilterExpressionType.GroupAll,
                    Rules = new List<FieldFilterRuleBag>()
                };
            }

            this.SetAdditionalSettings( "Rock.Model.CheckInLabel.ConditionalCriteria", conditionalPrintCriteria );
        }

        /// <summary>
        /// <para>
        /// Gets the size of the label as a text string in the format of <c>WxH</c>,
        /// such as <c>4x2</c>. The diminsions are in inches.
        /// </para>
        /// <para>
        /// If the diminsions are unkonwn, such as when <see cref="LabelFormat"/>
        /// is not <see cref="LabelFormat.Designed"/> then an empty string will
        /// be returned.
        /// </para>
        /// </summary>
        /// <returns>A string that represents the size of the label or <c>string.Empty</c> if it is unknown.</returns>
        public string GetLabelSizeDescription()
        {
            if ( LabelFormat != LabelFormat.Designed )
            {
                return string.Empty;
            }

            var designer = Content.FromJsonOrNull<DesignedLabelBag>();

            if ( designer == null )
            {
                return string.Empty;
            }

            return $"{designer.Width}x{designer.Height}";
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// File Configuration class.
    /// </summary>
    public partial class CheckInLabelConfiguration : EntityTypeConfiguration<CheckInLabel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInLabelConfiguration"/> class.
        /// </summary>
        public CheckInLabelConfiguration()
        {
        }
    }

    #endregion
}
