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
    /// SignatureDocumentType Entity.
    /// </summary>
    [Table( "SignatureDocumentType" )]
    [DataContract]
    public partial class SignatureDocumentType : Model<SignatureDocumentType>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the friendly Name of the SignatureDocumentType. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the friendly Name of the SignatureDocumentType.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        [IncludeForReporting]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a user defined description or summary about the SignatureDocumentType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing a description/summary of the SignatureDocumentType.
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
        /// Gets or sets the name of the request email template from.
        /// </summary>
        /// <value>
        /// The name of the request email template from.
        /// </value>
        [DataMember]
        public string RequestEmailTemplateFromName { get; set; }

        /// <summary>
        /// Gets or sets the request email template from address.
        /// </summary>
        /// <value>
        /// The request email template from address.
        /// </value>
        [DataMember]
        public string RequestEmailTemplateFromAddress { get; set; }

        /// <summary>
        /// Gets or sets the request email template subject.
        /// </summary>
        /// <value>
        /// The request email template subject.
        /// </value>
        [DataMember]
        public string RequestEmailTemplateSubject { get; set; }

        /// <summary>
        /// Gets or sets the request email template body.
        /// </summary>
        /// <value>
        /// The request email template body.
        /// </value>
        [DataMember]
        public string RequestEmailTemplateBody { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        [DataMember]
        public virtual EntityType ProviderEntityType { get; set; }

        /// <summary>
        /// Gets or sets the type of the binary file.
        /// </summary>
        /// <value>
        /// The type of the binary file.
        /// </value>
        [DataMember]
        public virtual BinaryFileType BinaryFileType { get; set; }

        /// <summary>
        /// Gets or sets the documents.
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

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this SignatureDocumentType.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this SignatureDocumentType.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }


        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// SignatureDocumentType Configuration class.
    /// </summary>
    public partial class SignatureDocumentTypeConfiguration : EntityTypeConfiguration<SignatureDocumentType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SignatureDocumentTypeConfiguration"/> class.
        /// </summary>
        public SignatureDocumentTypeConfiguration()
        {
            this.HasOptional( m => m.BinaryFileType ).WithMany().HasForeignKey( m => m.BinaryFileTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( m => m.ProviderEntityType ).WithMany().HasForeignKey( a => a.ProviderEntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}

