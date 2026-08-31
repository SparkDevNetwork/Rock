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
using Rock.Enums.Security;
using Rock.Security;

namespace Rock.Model
{
    /// <summary>
    /// Represents a registered document type in the flexible document store: the
    /// contract that tells humans and AI agents what a <see cref="FlexibleDocument"/>
    /// of this model is for and what its JSON payload should contain.
    /// </summary>
    /// <remarks>
    /// Security for the store lives here, on the type, and never on the individual
    /// document rows. Grants and denies made on a model flow to every
    /// <see cref="FlexibleDocument"/> of that model through
    /// <see cref="FlexibleDocument.ParentAuthority"/> delegation. If two documents
    /// of one model need different access, they belong in different models or in a
    /// purpose-built secured entity.
    /// </remarks>
    /*
        8/31/2026 - CLAUDE

        CodeGenerateRest is intentionally omitted. The store's callers today are
        server-side, and generated endpoints would expose ContentJson writes that
        bypass any future integration-layer validation while Rock's permissive
        default entity security applied. See the FlexibleDocument spec's
        "Considered but Rejected" section before adding it.

        Reason: The REST surface decision belongs to the integration layer, not the plumbing.
    */
    [RockDomain( "Core" )]
    [Table( "FlexibleDocumentModel" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "28A1D38E-333C-46C5-A896-7500DFFEAB74" )]
    public partial class FlexibleDocumentModel : Model<FlexibleDocumentModel>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the unique string identifier of this model (e.g. <c>AgentMemory</c>).
        /// </summary>
        /// <remarks>
        /// This is the handle callers and agent tools use to address the model, so it
        /// reads naturally as a tool argument and never changes once documents exist.
        /// </remarks>
        /// <value>
        /// A <see cref="System.String"/> representing the unique key of the model.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [Index( IsUnique = true )]
        [DataMember( IsRequired = true )]
        [StringValidation( StringValidationProfile.PlainText )]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the friendly name of this model.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the model.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        [StringValidation( StringValidationProfile.Name )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a short description of this model.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description of the model.
        /// </value>
        [DataMember]
        [StringValidation( StringValidationProfile.PlainText )]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the long-form guidance describing this model for humans and
        /// AI agents: what it is for, what the JSON payload should contain, and how
        /// the indexed filter columns are used.
        /// </summary>
        /// <remarks>
        /// This is the agent contract. A caller producing or consuming documents of
        /// this model reads this column to know how to shape and interpret
        /// <see cref="FlexibleDocument.ContentJson"/>.
        /// </remarks>
        /// <value>
        /// A <see cref="System.String"/> containing the documentation for the model.
        /// </value>
        [DataMember]
        [StringValidation( StringValidationProfile.Unrestricted )]
        public string Documentation { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this model is part of the core system.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if the model is part of the core system; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this model is active.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if the model is active; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsActive { get; set; } = true;

        #endregion Entity Properties

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.IsNullOrWhiteSpace( Name ) ? Key : Name;
        }

        #endregion Methods
    }

    #region Entity Configuration

    /// <summary>
    /// FlexibleDocumentModel Configuration class.
    /// </summary>
    public partial class FlexibleDocumentModelConfiguration : EntityTypeConfiguration<FlexibleDocumentModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlexibleDocumentModelConfiguration"/> class.
        /// </summary>
        public FlexibleDocumentModelConfiguration()
        {
        }
    }

    #endregion Entity Configuration
}
