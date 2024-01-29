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
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// Represents a persisted <see cref="Rock.Model.SignatureDocument"/> execution/instance in Rock.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "SignatureDocument" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "C1724719-1C03-4D0C-8A66-E3545138F57F")]
    public partial class SignatureDocument : Model<SignatureDocument>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the SignatureDocumentTemplateId of the <see cref="Rock.Model.SignatureDocumentTemplate"/> that this SignatureDocument instance is executing.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the SignatureDocumentTemplateId fo the <see cref="Rock.Model.SignatureDocumentTemplate"/> that is being executed.
        /// </value>
        [DataMember]
        public int SignatureDocumentTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [MaxLength( 250 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the document key.
        /// </summary>
        /// <value>
        /// The document key.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string DocumentKey { get; set; }

        /// <summary>
        /// Gets or sets the request date.
        /// </summary>
        /// <value>
        /// The request date.
        /// </value>
        [DataMember]
        public DateTime? LastInviteDate { get; set; }

        /// <summary>
        /// Gets or sets the invite count.
        /// </summary>
        /// <value>
        /// The invite count.
        /// </value>
        [DataMember]
        public int InviteCount { get; set; }

        /// <summary>
        /// Gets or sets the applies to person alias identifier.
        /// </summary>
        /// <value>
        /// The applies to person alias identifier.
        /// </value>
        [DataMember]
        public int? AppliesToPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the assigned to person alias identifier.
        /// </summary>
        /// <value>
        /// The assigned to person alias identifier.
        /// </value>
        [DataMember]
        public int? AssignedToPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [DataMember]
        public SignatureDocumentStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the last status date.
        /// </summary>
        /// <value>
        /// The last status date.
        /// </value>
        [DataMember]
        public DateTime? LastStatusDate { get; set; }

        /// <summary>
        /// Gets or sets the binary file identifier.
        /// </summary>
        /// <value>
        /// The binary file identifier.
        /// </value>
        [DataMember]
        public int? BinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the signed by person alias identifier.
        /// </summary>
        /// <value>
        /// The signed by person alias identifier.
        /// </value>
        [DataMember]
        public int? SignedByPersonAliasId { get; set; }

        /// <summary>
        /// The resulting text/document using the Lava template from the <see cref="Rock.Model.SignatureDocumentTemplate"/> at the time the document was signed.
        /// Does not include the signature data. It would be what they saw just prior to signing.
        /// </summary>
        /// <value>
        /// The signed document text.
        /// </value>
        [DataMember]
        public string SignedDocumentText { get; set; }

        /// <summary>
        /// The name of the individual who signed the document.
        /// </summary>
        /// <value>
        /// The signed name.
        /// </value>
        [MaxLength( 250 )]
        [DataMember]
        public string SignedName { get; set; }

        /// <summary>
        /// The observed IP address of the client system of the individual who signed the document.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the signed client IP address.
        /// </value>
        [MaxLength( 128 )]
        [DataMember]
        public string SignedClientIp { get; set; }

        /// <summary>
        /// The observed 'user agent' of the client system of the individual who signed the document.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the signed client user agent.
        /// </value>
        [DataMember]
        public string SignedClientUserAgent { get; set; }

        /// <summary>
        /// The date and time the document was signed.
        /// </summary>
        /// <value>
        /// The signed date and time.
        /// </value>
        [DataMember]
        public DateTime? SignedDateTime { get; set; }

        /// <summary>
        /// The email address that was used to send the completion receipt.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the signed by email address.
        /// </value>
        [MaxLength( 75 )]
        [DataMember]
        public string SignedByEmail { get; set; }

        /// <summary>
        /// The date and time the document completion email was sent.
        /// </summary>
        /// <value>
        /// The completion email sent date and time.
        /// </value>
        [DataMember]
        public DateTime? CompletionEmailSentDateTime { get; set; }

        /// <summary>
        /// The encrypted data that was collected during a drawn signature type.
        /// Use <see cref="SignatureData"/> to set this from the unencrypted drawn signature.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the signature data.
        /// </value>
        [DataMember]
        [HideFromReporting]
        public string SignatureDataEncrypted { get; set; }

        /* 1/25/2022 MDP
        See engineering note on SignatureData
        */

        /// <summary>
        /// The computed SHA1 hash for the SignedDocumentText, SignedClientIP address, SignedClientUserAgent, SignedDateTime, SignedByPersonAliasId, SignatureData, and SignedName.
        /// This hash can be used to prove the authenticity of the unaltered signature document.
        /// This is only calculated once during the pre-save event when the SignedDateTime was originally null/empty but now has a value.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the signature data.
        /// </value>
        [MaxLength( 40 )]
        [DataMember]
        public string SignatureVerificationHash { get; set; }

        /// <summary>
        /// The EntityType that this document is related to (example Rock.Model.Registration)
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> of the entity that this signature document applies to.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// The ID of the entity to which the document is related.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityId of the entity that this signature document entity applies to.
        /// </value>
        [DataMember]
        public int? EntityId { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.SignatureDocumentTemplate"/> that is being executed in this persisted SignatureDocument instance.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.SignatureDocumentTemplate"/> that is being executed in this persisted SignatureDocument instance.
        /// </value>
        [LavaVisible]
        public virtual SignatureDocumentTemplate SignatureDocumentTemplate { get; set; }

        /// <summary>
        /// Gets or sets the applies to person alias.
        /// </summary>
        /// <value>
        /// The applies to person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias AppliesToPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the assigned to person alias.
        /// </summary>
        /// <value>
        /// The assigned to person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias AssignedToPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the binary file.
        /// </summary>
        /// <value>
        /// The binary file.
        /// </value>
        [DataMember]
        public virtual BinaryFile BinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the signed by person alias.
        /// </summary>
        /// <value>
        /// The signed by person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias SignedByPersonAlias { get; set; }

        /// <summary>
        /// Gets the parent security authority for this SignatureDocument instance.
        /// </summary>
        /// <value>
        /// The parent authority for this SignatureDocument instance.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.SignatureDocumentTemplate != null ? this.SignatureDocumentTemplate : base.ParentAuthority;
            }
        }

        /// <summary>
        /// The EntityType that this document is related to (example Rock.Model.Registration)
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

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
    /// SignatureDocument Configuration class.
    /// </summary>
    public partial class SignatureDocumentConfiguration : EntityTypeConfiguration<SignatureDocument>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SignatureDocumentConfiguration"/> class.
        /// </summary>
        public SignatureDocumentConfiguration()
        {
            this.HasRequired( d => d.SignatureDocumentTemplate ).WithMany( t => t.Documents ).HasForeignKey( d => d.SignatureDocumentTemplateId ).WillCascadeOnDelete( true );
            this.HasOptional( d => d.AppliesToPersonAlias ).WithMany().HasForeignKey( d => d.AppliesToPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( d => d.AssignedToPersonAlias ).WithMany().HasForeignKey( d => d.AssignedToPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( d => d.BinaryFile ).WithMany().HasForeignKey( d => d.BinaryFileId ).WillCascadeOnDelete( false );
            this.HasOptional( d => d.SignedByPersonAlias ).WithMany().HasForeignKey( d => d.SignedByPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( d => d.EntityType ).WithMany().HasForeignKey( d => d.EntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}