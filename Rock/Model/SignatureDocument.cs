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
    /// Represents a persisted <see cref="Rock.Model.SignatureDocument"/> execution/instance in Rock.
    /// </summary>
    [Table( "SignatureDocument" )]
    [DataContract]
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

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.SignatureDocumentTemplate"/> that is being executed in this persisted SignatureDocument instance.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.SignatureDocumentTemplate"/> that is being executed in this persisted SignatureDocument instance.
        /// </value>
        [LavaInclude]
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

        #endregion

        #region Methods

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
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// The status of a signature document
    /// </summary>
    public enum SignatureDocumentStatus
    {

        /// <summary>
        /// Document has not yet been sent
        /// </summary>
        None = 0,

        /// <summary>
        /// Document has been sent but not yet signed
        /// </summary>
        Sent = 1,

        /// <summary>
        /// Document has been signed
        /// </summary>
        Signed = 2,

        /// <summary>
        /// Document was cancelled
        /// </summary>
        Cancelled = 3
    }

    #endregion

}