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
    /// SignatureDocumentTemplate Entity.
    /// </summary>
    [Table( "SignatureDocumentTemplate" )]
    [DataContract]
    public partial class SignatureDocumentTemplate : Model<SignatureDocumentTemplate>
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
        public int? InviteSystemEmailId { get; set; }

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
        /// Gets or sets the type of the binary file.
        /// </summary>
        /// <value>
        /// The type of the binary file.
        /// </value>
        [DataMember]
        public virtual SystemEmail InviteSystemEmail { get; set; }

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
            this.HasOptional( t => t.InviteSystemEmail ).WithMany().HasForeignKey( t => t.InviteSystemEmailId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}

