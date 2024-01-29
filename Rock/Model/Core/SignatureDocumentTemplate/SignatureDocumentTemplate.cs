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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// SignatureDocumentTemplate Entity.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "SignatureDocumentTemplate" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "3F9828CC-8224-4AB0-98A5-6D60001EBE32")]
    public partial class SignatureDocumentTemplate : Model<SignatureDocumentTemplate>, IHasActiveFlag
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the friendly Name of the SignatureDocumentTemplate. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the friendly Name of the SignatureDocumentTemplate.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        [IncludeForReporting]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a user defined description or summary about the SignatureDocumentTemplate.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing a description/summary of the SignatureDocumentTemplate.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the provider entity type identifier.
        /// </summary>
        /// <value>
        /// The provider entity type identifier.
        /// </value>
        [DataMember]
        public int? ProviderEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the provider template key.
        /// </summary>
        /// <value>
        /// The provider template key.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string ProviderTemplateKey { get; set; }

        /// <summary>
        /// Gets or sets the binary file type identifier.
        /// </summary>
        /// <value>
        /// The binary file type identifier.
        /// </value>
        [DataMember]
        public int? BinaryFileTypeId { get; set; }

        /// <summary>
        /// Gets or sets the invite system email identifier.
        /// </summary>
        /// <value>
        /// The invite system email identifier.
        /// </value>
        [DataMember]
        public int? InviteSystemCommunicationId { get; set; }

        /// <summary>
        /// Gets or sets the invite system email identifier.
        /// </summary>
        /// <value>
        /// The invite system email identifier.
        /// </value>
        [DataMember]
        [Obsolete( "Use InviteSystemCommunicationId instead.", true )]
        [RockObsolete( "1.10" )]
        public int? InviteSystemEmailId { get; set; }

        /// <summary>
        /// The Lava template that will be used to build the signature document.
        /// </summary>
        [DataMember]
        public string LavaTemplate { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this item is active or not.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is  <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// The term used to simply describe the document (wavier, release form, etc.).
        /// </summary>
        /// <value>
        /// The document term.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string DocumentTerm { get; set; }

        /// <summary>
        /// This is used to define which kind of signature is being collected from the individual.
        /// Ex: <see cref="SignatureType.Drawn"/> or <see cref="SignatureType.Typed"/>, etc.
        /// </summary>
        /// <value>
        /// The type of the signature.
        /// </value>
        [DataMember]
        public SignatureType SignatureType { get; set; }

        /// <summary>
        /// The System Communication that will be used when sending the signature document completion email.
        /// </summary>
        /// <value>
        /// The completion system communication identifier.
        /// </value>
        [DataMember]
        public int? CompletionSystemCommunicationId { get; set; }

        /// <summary>
        /// Determines if documents of this type should be considered valid for future eligibility needs.
        /// </summary>
        [DataMember]
        public bool IsValidInFuture { get; set; }

        /// <summary>
        /// The number of days a signed document of this type is valid once it is signed.
        /// </summary>
        [DataMember]
        public int? ValidityDurationInDays { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        [DataMember]
        public virtual EntityType ProviderEntityType { get; set; }

        /// <summary>
        /// Gets or sets the type of the <see cref="Rock.Model.BinaryFile"/>.
        /// </summary>
        /// <value>
        /// The type of the binary file.
        /// </value>
        [DataMember]
        public virtual BinaryFileType BinaryFileType { get; set; }

        /// <summary>
        /// Gets or sets the system email to use when a person is invited to sign a document.
        /// </summary>
        /// <value>
        /// The system email.
        /// </value>
        [DataMember]
        [Obsolete( "Use InviteSystemCommunication instead.", true )]
        [RockObsolete( "1.10" )]
        public virtual SystemEmail InviteSystemEmail { get; set; }

        /// <summary>
        /// Gets or sets the system communication to use when a person is invited to sign a document.
        /// </summary>
        /// <value>
        /// The system communication.
        /// </value>
        [DataMember]
        public virtual SystemCommunication InviteSystemCommunication { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.SignatureDocument">documents</see>.
        /// </summary>
        /// <value>
        /// The documents.
        /// </value>
        public virtual ICollection<SignatureDocument> Documents
        {
            get { return _documents ?? ( _documents = new Collection<SignatureDocument>() ); }
            set { _documents = value; }
        }
        private ICollection<SignatureDocument> _documents;

        /// <summary>
        /// The System Communication that will be used when sending the signature document completion email.
        /// </summary>
        /// <value>
        /// The completion system communication.
        /// </value>
        [DataMember]
        public virtual SystemCommunication CompletionSystemCommunication { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this SignatureDocumentTemplate.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this SignatureDocumentTemplate.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// SignatureDocumentTemplate Configuration class.
    /// </summary>
    public partial class SignatureDocumentTemplateConfiguration : EntityTypeConfiguration<SignatureDocumentTemplate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SignatureDocumentTemplateConfiguration"/> class.
        /// </summary>
        public SignatureDocumentTemplateConfiguration()
        {
            this.HasOptional( t => t.BinaryFileType ).WithMany().HasForeignKey( t => t.BinaryFileTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.ProviderEntityType ).WithMany().HasForeignKey( t => t.ProviderEntityTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.InviteSystemCommunication ).WithMany().HasForeignKey( t => t.InviteSystemCommunicationId ).WillCascadeOnDelete( false );

            this.HasOptional( t => t.CompletionSystemCommunication ).WithMany().HasForeignKey( t => t.CompletionSystemCommunicationId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}

