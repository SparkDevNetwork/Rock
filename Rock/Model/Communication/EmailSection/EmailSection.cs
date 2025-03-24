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
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents an email section
    /// </summary>
    [RockDomain( "Communication" )]
    [Table( "EmailSection" )]
    [DataContract]
    [CodeGenerateRest( DisableEntitySecurity = true )]
    [Rock.SystemGuid.EntityTypeGuid( "86B2CE94-9DC3-463C-B2B1-DEECAB70474E" )]
    public partial class EmailSection : Model<EmailSection>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the email section. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> that represents the name of the email section.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the usage summary of the email section. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> that represents the usage summary of the email section.
        /// </value>
        [Required]
        [MaxLength( 500 )]
        [DataMember( IsRequired = true )]
        public string UsageSummary { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a system email section.
        /// </summary>
        /// <value>
        /// A value indicating whether this is a system email section.
        /// </value>
        [DataMember]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the source markup.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public string SourceMarkup { get; set; }

        /// <summary>
        /// Gets or sets the email section <see cref="Rock.Model.Category"/>.
        /// </summary>
        /// <value>
        /// The category identifier.
        /// </value>
        /// <remarks>
        /// [IgnoreCanDelete] since there is a ON DELETE SET NULL cascade on this
        /// </remarks>
        [IgnoreCanDelete]
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the thumbnail <see cref="Rock.Model.BinaryFile"/>.
        /// </summary>
        /// <value>
        /// The binary file identifier.
        /// </value>
        /// <remarks>
        /// [IgnoreCanDelete] since there is a ON DELETE SET NULL cascade on this
        /// </remarks>
        [IgnoreCanDelete]
        public int? ThumbnailBinaryFileId { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Category"></see>.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        [DataMember]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets or sets the thumbnail <see cref="Rock.Model.BinaryFile"></see>.
        /// </summary>
        /// <value>
        /// The thumbnail binary file.
        /// </value>
        [DataMember]
        public virtual BinaryFile ThumbnailBinaryFile { get; set; }

        #endregion Navigation Properties

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> containing the Email Section's Name that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> containing the Email Section's Name that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Communication Configuration class.
    /// </summary>
    public partial class EmailSectionConfiguration : EntityTypeConfiguration<EmailSection>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailSectionConfiguration" /> class.
        /// </summary>
        public EmailSectionConfiguration()
        {
        }
    }

    #endregion
}
