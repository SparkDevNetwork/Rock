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
using Rock.Enums.Security;
using Rock.Lava;
using Rock.Security;

namespace Rock.Model
{
    /// <summary>
    /// Represents a schema-less JSON document in the flexible document store. The
    /// payload lives in <see cref="ContentJson"/>, its type is described by the
    /// owning <see cref="FlexibleDocumentModel"/>, and five generic typed columns
    /// provide fast filtering without a per-model schema.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Documents carry no link columns. Every association to another Rock entity,
    /// including the primary "this document belongs to entity X" relationship, is a
    /// <see cref="RelatedEntity"/> row with this document as the source and
    /// <see cref="RelatedEntityPurposeKey.FlexibleDocumentPrimary"/> marking the
    /// primary link.
    /// </para>
    /// <para>
    /// Documents hold untrusted, often AI-authored payloads and have no row-level
    /// security (see <see cref="ParentAuthority"/>). Do not store PII, financial
    /// data, credentials, or anything needing row-level access control in
    /// <see cref="ContentJson"/> or the indexed columns, and validate and encode
    /// every payload before rendering it or feeding it into Lava.
    /// </para>
    /// </remarks>
    /*
        8/31/2026 - CLAUDE

        CodeGenerateRest is intentionally omitted. The store's callers today are
        server-side, and generated endpoints would expose ContentJson writes that
        bypass any future integration-layer validation. See the FlexibleDocument
        spec's "Considered but Rejected" section before adding it.

        Reason: The REST surface decision belongs to the integration layer, not the plumbing.
    */
    [RockDomain( "Core" )]
    [Table( "FlexibleDocument" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "962D0C52-FA32-4863-977F-D6A4B2DF0C09" )]
    public partial class FlexibleDocument : Model<FlexibleDocument>, ICategorized
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of this document.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the document.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        [StringValidation( StringValidationProfile.Name )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.FlexibleDocumentModel"/> that describes this document's type.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the owning <see cref="Rock.Model.FlexibleDocumentModel"/>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int FlexibleDocumentModelId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Category"/> this document belongs to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Category"/>, or <c>null</c> if uncategorized.
        /// </value>
        [DataMember]
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the JSON payload of this document.
        /// </summary>
        /// <remarks>
        /// The database enforces well-formed JSON with an <c>ISJSON</c> check
        /// constraint, but nothing validates the shape against the model's
        /// documentation. The payload is untrusted input: readers must validate and
        /// encode it before use, and it must never contain PII, financial data, or
        /// credentials because access control is per model, never per row.
        /// </remarks>
        /// <value>
        /// A <see cref="System.String"/> containing the JSON payload.
        /// </value>
        [DataMember]
        [StringValidation( StringValidationProfile.Unrestricted )]
        public string ContentJson { get; set; }

        /// <summary>
        /// Gets or sets the first generic indexed text filter value. What this column
        /// means is defined per model in <see cref="FlexibleDocumentModel.Documentation"/>.
        /// </summary>
        /// <remarks>
        /// Indexed columns are for filtering, not for sensitive data; the same
        /// no-PII rule as <see cref="ContentJson"/> applies.
        /// </remarks>
        /// <value>
        /// A <see cref="System.String"/> filter value, or <c>null</c> when the model does not use this slot.
        /// </value>
        [MaxLength( 100 )]
        [Index]
        [DataMember]
        [StringValidation( StringValidationProfile.PlainText )]
        public string IndexedText1 { get; set; }

        /// <summary>
        /// Gets or sets the second generic indexed text filter value. What this column
        /// means is defined per model in <see cref="FlexibleDocumentModel.Documentation"/>.
        /// </summary>
        /// <remarks>
        /// Indexed columns are for filtering, not for sensitive data; the same
        /// no-PII rule as <see cref="ContentJson"/> applies.
        /// </remarks>
        /// <value>
        /// A <see cref="System.String"/> filter value, or <c>null</c> when the model does not use this slot.
        /// </value>
        [MaxLength( 100 )]
        [Index]
        [DataMember]
        [StringValidation( StringValidationProfile.PlainText )]
        public string IndexedText2 { get; set; }

        /// <summary>
        /// Gets or sets the generic indexed integer filter value. Use this for
        /// integer dimensions (counts, years, enum values) rather than burning the
        /// decimal slot; an int is narrower and cheaper to compare.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> filter value, or <c>null</c> when the model does not use this slot.
        /// </value>
        [Index]
        [DataMember]
        public int? IndexedInteger1 { get; set; }

        /// <summary>
        /// Gets or sets the generic indexed decimal filter value. What this column
        /// means is defined per model in <see cref="FlexibleDocumentModel.Documentation"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.Decimal"/> filter value, or <c>null</c> when the model does not use this slot.
        /// </value>
        [Index]
        [DataMember]
        [DecimalPrecision( 18, 4 )]
        public decimal? IndexedDecimal1 { get; set; }

        /// <summary>
        /// Gets or sets the generic indexed date filter value. What this column
        /// means is defined per model in <see cref="FlexibleDocumentModel.Documentation"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> filter value, or <c>null</c> when the model does not use this slot.
        /// </value>
        [Index]
        [DataMember]
        public DateTime? IndexedDate1 { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.PersonAlias"/> this document is on behalf of.
        /// </summary>
        /// <remarks>
        /// This is deliberately distinct from <see cref="Model{T}.CreatedByPersonAliasId"/>:
        /// the audit column records the actor, often an AI agent, that wrote the row,
        /// while this column records who the document is about or for. It records
        /// intent and ownership only and enforces nothing.
        /// </remarks>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the owning <see cref="Rock.Model.PersonAlias"/>, or <c>null</c>.
        /// </value>
        [DataMember]
        public int? OwnerPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the manual sort order of this document within its model or category.
        /// </summary>
        /// <remarks>
        /// Nullable on purpose: unordered documents are the norm, so this does not
        /// implement <see cref="Rock.Data.IOrdered"/>, which requires a value.
        /// </remarks>
        /// <value>
        /// A <see cref="System.Int32"/> sort order, or <c>null</c> when unordered.
        /// </value>
        [DataMember]
        public int? Order { get; set; }

        /// <summary>
        /// Gets or sets the optional date and time after which this document may be purged.
        /// </summary>
        /// <remarks>
        /// The column ships ahead of any cleanup job; nothing purges expired rows today.
        /// </remarks>
        /// <value>
        /// A <see cref="System.DateTime"/> after which the document is expired, or <c>null</c> for no expiry.
        /// </value>
        [DataMember]
        public DateTime? ExpireDateTime { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this document is active (soft delete).
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if the document is active; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsActive { get; set; } = true;

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FlexibleDocumentModel"/> that describes this document's type.
        /// </summary>
        /// <value>
        /// The owning <see cref="Rock.Model.FlexibleDocumentModel"/>.
        /// </value>
        [LavaVisible]
        public virtual FlexibleDocumentModel FlexibleDocumentModel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Category"/> this document belongs to.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Category"/>, or <c>null</c> if uncategorized.
        /// </value>
        [LavaVisible]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/> this document is on behalf of.
        /// </summary>
        /// <value>
        /// The owning <see cref="Rock.Model.PersonAlias"/>, or <c>null</c>.
        /// </value>
        [LavaVisible]
        public virtual PersonAlias OwnerPersonAlias { get; set; }

        #endregion Navigation Properties

        #region ISecured

        /// <summary>
        /// Gets the parent authority for this document: its
        /// <see cref="Rock.Model.FlexibleDocumentModel"/>.
        /// </summary>
        /// <remarks>
        /// Security lives on the type, not the row. Rules authored on a model flow
        /// to every document of that model; individual documents never carry their
        /// own access control entries.
        /// </remarks>
        /// <value>
        /// The owning <see cref="Rock.Model.FlexibleDocumentModel"/> when available; otherwise the default parent authority.
        /// </value>
        public override ISecured ParentAuthority => FlexibleDocumentModel ?? base.ParentAuthority;

        #endregion ISecured

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.IsNullOrWhiteSpace( Name ) ? base.ToString() : Name;
        }

        #endregion Methods
    }

    #region Entity Configuration

    /// <summary>
    /// FlexibleDocument Configuration class.
    /// </summary>
    public partial class FlexibleDocumentConfiguration : EntityTypeConfiguration<FlexibleDocument>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlexibleDocumentConfiguration"/> class.
        /// </summary>
        public FlexibleDocumentConfiguration()
        {
            // A model with documents cannot be deleted out from under them, so the
            // required FK deliberately does not cascade.
            this.HasRequired( d => d.FlexibleDocumentModel ).WithMany().HasForeignKey( d => d.FlexibleDocumentModelId ).WillCascadeOnDelete( false );
            this.HasOptional( d => d.Category ).WithMany().HasForeignKey( d => d.CategoryId ).WillCascadeOnDelete( false );
            this.HasOptional( d => d.OwnerPersonAlias ).WithMany().HasForeignKey( d => d.OwnerPersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}
