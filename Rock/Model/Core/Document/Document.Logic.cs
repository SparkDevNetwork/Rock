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
using System.Linq;

using Rock.Data;
using Rock.UniversalSearch;
using Rock.UniversalSearch.IndexModels;

namespace Rock.Model
{
    public partial class Document
    {
        /// <summary>
        /// Gets a value indicating whether [allows interactive bulk indexing].
        /// </summary>
        /// <value>
        /// <c>true</c> if [allows interactive bulk indexing]; otherwise, <c>false</c>.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        [NotMapped]
        public bool AllowsInteractiveBulkIndexing => true;

        #region Methods

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// NOTE: Try using IsValidDocument instead
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public override bool IsValid
        {
            get
            {
                var result = base.IsValid;
                if ( result )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        result = this.IsValidDocument( rockContext, out string errorMessage );
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Calls IsValid with the specified context (to avoid deadlocks)
        /// Try to call this instead of IsValid when possible.
        /// Note that this same validation will be done by the service layer when SaveChanges() is called
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if [is valid group member] [the specified rock context]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValidDocument( RockContext rockContext, out string errorMessage )
        {
            var result = base.IsValid;
            errorMessage = string.Empty;
            if ( !result )
            {
                errorMessage = this.ValidationResults.FirstOrDefault()?.ErrorMessage;
                return false;
            }

            var documentType = DocumentType ?? new DocumentTypeService( rockContext ).Get( DocumentTypeId );

            if ( documentType == null )
            {
                errorMessage = $"Invalid document type found.";
                ValidationResults.Add( new ValidationResult( errorMessage ) );
                result = false;
            }

            if ( documentType != null && documentType.MaxDocumentsPerEntity.HasValue )
            {
                var documentsPerEntityCount = new DocumentService( rockContext )
                    .Queryable()
                    .Where( a => a.DocumentTypeId == DocumentTypeId && a.EntityId == this.EntityId && a.Id != this.Id )
                    .Count();

                if ( documentsPerEntityCount >= documentType.MaxDocumentsPerEntity.Value )
                {
                    errorMessage = $"Unable to add document because there is a limit of {documentType.MaxDocumentsPerEntity} documents for this type.";
                    ValidationResults.Add( new ValidationResult( errorMessage ) );
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check the default authorization on the current type, and
        /// then the authorization on the Rock.Security.GlobalDefault entity
        /// </summary>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.DocumentType != null ? this.DocumentType : base.ParentAuthority;
            }
        }

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
            return new ModelFieldFilterConfig() { FilterLabel = string.Empty, FilterField = string.Empty };
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

        #endregion Methods
    }
}
