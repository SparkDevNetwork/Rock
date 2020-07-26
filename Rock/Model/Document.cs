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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;
using Rock.UniversalSearch;
using Rock.UniversalSearch.IndexModels;

namespace Rock.Model
{
    /// <summary>
    /// Represents any document in Rock.  
    /// </summary>
    [RockDomain( "CRM" )]
    [Table( "Document" )]
    [DataContract]
    public partial class Document : Model<Document>, IRockIndexable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this document is part of the Rock core system/framework.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean" /> value that is <c>true</c> if this document is part of the core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the id of the <see cref="Rock.Model.DocumentType"/> that this document belongs to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the <see cref="Rock.Model.DocumentType"/>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int DocumentTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the entity that this document is related to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the entity (object) that this document is related to.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the given Name of the document.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the given Name of the document. 
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description of the document.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description of the document.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /*
           edrotning 2020-02-21
            /// <summary>
            /// Document to BinaryFile is a 1:1 relationship. EF doesn't really support this but it can be kind of hacked by having a navigation property but not an ID property.
            /// There is a discussion here: https://stackoverflow.com/a/41847251
            /// Also attempted was to have the BinaryFileID as a property but not have it mapped. Trying to use the setter didn't work with the context.
            /// This may work as a method instead of a property but is no longer simplified.
            /// Leaving this commented out so someone else doesn't have to go through it again.
            /// </summary>
            /// <value>
            /// The binary file identifier.
            /// </value>
            //public int BinaryFileID
            //{
            //    get
            //    {
            //        return this.BinaryFile.Id;
            //    }
            //    set
            //    {
            //        using ( var rockContext = new RockContext() )
            //        {
            //            var binaryFileService = new BinaryFileService( rockContext );
            //            var binaryFile = binaryFileService.Get( value );
            //            if ( binaryFile != null )
            //            {
            //                this.BinaryFile = binaryFile;
            //            }
            //        }
            //    }
            //}
        */

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DocumentType"/> of the document.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.DocumentType"/> of the document.
        /// </value>
        [DataMember]
        public virtual DocumentType DocumentType { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="Rock.Model.BinaryFile"/> that contains the content of the file.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.BinaryFile"/> that contains the content of the file. 
        /// </value>
        [DataMember]
        public virtual BinaryFile BinaryFile { get; set; }

        /// <summary>
        /// Gets a value indicating whether [allows interactive bulk indexing].
        /// </summary>
        /// <value>
        /// <c>true</c> if [allows interactive bulk indexing]; otherwise, <c>false</c>.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        [NotMapped]
        public bool AllowsInteractiveBulkIndexing => true;

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> containing the name of the file and  represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> containing the name of the file and represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

        #region Indexing Methods

        /// <summary>
        /// Bulks the index documents.
        /// </summary>
        public void BulkIndexDocuments()
        {
            List<IndexModelBase> indexableItems = new List<IndexModelBase>();

            RockContext rockContext = new RockContext();

            // return people
            var documents = new DocumentService( rockContext ).Queryable().AsNoTracking();

            int recordCounter = 0;

            foreach ( var document in documents )
            {
                var indexableDocument = DocumentIndex.LoadByModel( document );
                indexableItems.Add( indexableDocument );

                recordCounter++;

                if ( recordCounter > 100 )
                {
                    IndexContainer.IndexDocuments( indexableItems );
                    indexableItems = new List<IndexModelBase>();
                    recordCounter = 0;
                }
            }

            IndexContainer.IndexDocuments( indexableItems );
        }

        /// <summary>
        /// Deletes the indexed documents.
        /// </summary>
        public void DeleteIndexedDocuments()
        {
            IndexContainer.DeleteDocumentsByType<DocumentIndex>();
        }

        /// <summary>
        /// Indexes the name of the model.
        /// </summary>
        /// <returns></returns>
        public Type IndexModelType()
        {
            return typeof( DocumentIndex );
        }

        /// <summary>
        /// Indexes the document.
        /// </summary>
        /// <param name="id"></param>
        public void IndexDocument( int id )
        {
            var documentEntity = new DocumentService( new RockContext() ).Get( id );

            var indexItem = DocumentIndex.LoadByModel( documentEntity );
            IndexContainer.IndexDocument( indexItem );
        }

        /// <summary>
        /// Deletes the indexed document.
        /// </summary>
        /// <param name="id"></param>
        public void DeleteIndexedDocument( int id )
        {
            Type indexType = Type.GetType( "Rock.UniversalSearch.IndexModels.DocumentIndex" );
            IndexContainer.DeleteDocumentById( indexType, id );
        }

        /// <summary>
        /// Gets the index filter values.
        /// </summary>
        /// <returns></returns>
        public ModelFieldFilterConfig GetIndexFilterConfig()
        {
            return new ModelFieldFilterConfig() { FilterLabel = "", FilterField = "" };
        }

        /// <summary>
        /// Gets the index filter field.
        /// </summary>
        /// <returns></returns>
        public bool SupportsIndexFieldFiltering()
        {
            return false;
        }
        #endregion

        /// <summary>
        /// Sets the binary file for the document. If the document is being added or updated use the same context.
        /// </summary>
        /// <param name="binaryFileId">The binary file identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        public void SetBinaryFile( int binaryFileId, RockContext rockContext )
        {
            var binaryFileService = new BinaryFileService( rockContext );
            var binaryFile = binaryFileService.Get( binaryFileId );
            if ( binaryFile != null )
            {
                binaryFile.IsTemporary = false;
                this.BinaryFile = binaryFile;
            }
        }
    }

    #region Entity Configuration

    /// <summary>
    /// Document Configuration class.
    /// </summary>
    public partial class DocumentConfiguration : EntityTypeConfiguration<Document>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentConfiguration"/> class.
        /// </summary>
        public DocumentConfiguration()
        {
            this.HasRequired( f => f.DocumentType ).WithMany().HasForeignKey( f => f.DocumentTypeId ).WillCascadeOnDelete( false );

            // This is a 1:1 relationship and is not very common in Rock. We cannot add BinaryFileId to the model because of the EF limitation
            // discussed here: https://stackoverflow.com/a/41847251
            this.HasRequired( f => f.BinaryFile ).WithOptional( a => a.Document ).Map( x => x.MapKey( "BinaryFileId" ) ).WillCascadeOnDelete();
        }
    }

    #endregion
}
