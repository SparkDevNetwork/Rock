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
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Utility;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Documents REST API
    /// </summary>
    public partial class DocumentsController
    {
        /// <summary>
        /// Uploads a file, storing it as a binary file and linking it to the specified entity with a new document record.
        /// </summary>
        /// <param name="documentTypeId">The document type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="documentName">The name to be used for the document (will use the uploaded file name if not specified).</param>
        /// <returns>The ID of the newly-saved document record.</returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/Documents/UploadDocument" )]
        public HttpResponseMessage UploadDocument( int documentTypeId, int entityId, string documentName = null )
        {
            try
            {
                // Ensure a file was uploaded.
                var files = HttpContext.Current.Request.Files;
                var uploadedFile = files.AllKeys.Select( fk => files[fk] ).FirstOrDefault();

                if ( uploadedFile == null )
                {
                    GenerateResponse( HttpStatusCode.BadRequest, "No file was sent." );
                }

                var rockContext = new RockContext();

                // Ensure the caller is authorized to save a document of the specified type.
                var documentType = new DocumentTypeService( rockContext )
                    .Queryable( "BinaryFileType" )
                    .AsNoTracking()
                    .Where( dt => dt.Id == documentTypeId )
                    .FirstOrDefault();

                if ( documentType == null )
                {
                    GenerateResponse( HttpStatusCode.InternalServerError, "Invalid document type." );
                }

                if ( !documentType.IsAuthorized( Rock.Security.Authorization.EDIT, GetPerson() ) )
                {
                    GenerateResponse( HttpStatusCode.Unauthorized, "Not authorized to upload this type of document." );
                }

                // Ensure the caller is authorized to save a binary file of the specified type.
                if ( documentType.BinaryFileType == null )
                {
                    GenerateResponse( HttpStatusCode.InternalServerError, "Invalid binary file type." );
                }

                if ( !documentType.BinaryFileType.IsAuthorized( Rock.Security.Authorization.EDIT, GetPerson() ) )
                {
                    GenerateResponse( HttpStatusCode.Unauthorized, "Not authorized to upload this type of file." );
                }

                string fileName = Path.GetFileName( uploadedFile.FileName );

                // Create the binary file.
                var binaryFile = new BinaryFile
                {
                    BinaryFileTypeId = documentType.BinaryFileTypeId,
                    MimeType = uploadedFile.ContentType,
                    FileName = fileName,
                    FileSize = uploadedFile.ContentLength,
                    ContentStream = FileUtilities.GetFileContentStream( uploadedFile )
                };

                new BinaryFileService( rockContext ).Add( binaryFile );

                // Create the document, linking the entity and binary file.
                var document = new Document
                {
                    DocumentTypeId = documentTypeId,
                    EntityId = entityId,
                    Name = !string.IsNullOrWhiteSpace( documentName ) ? documentName : fileName,
                    BinaryFile = binaryFile
                };

                new DocumentService( rockContext ).Add( document );

                // Save the object graph.
                rockContext.SaveChanges();

                // Return the ID of the newly-saved document.
                return new HttpResponseMessage( HttpStatusCode.Created )
                {
                    Content = new StringContent( document.Id.ToString() )
                };
            }
            catch ( HttpResponseException exception )
            {
                return exception.Response;
            }
            catch
            {
                return new HttpResponseMessage( HttpStatusCode.InternalServerError )
                {
                    Content = new StringContent( "Unhandled exception" )
                };
            }
        }

        /// <summary>
        /// Uploads a file, storing it as a binary file and linking it to the specified person - by way of the foreign ID - with a new document record.
        /// </summary>
        /// <param name="documentTypeId">The document type identifier.</param>
        /// <param name="foreignPersonId">The foreign person identifier.</param>
        /// <param name="documentName">The name to be used for the document (will use the uploaded file name if not specified).</param>
        /// <returns>The ID of the newly-saved document record.</returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/Documents/UploadDocumentForForeignPersonId" )]
        public HttpResponseMessage UploadDocumentForForeignPersonId( int documentTypeId, int foreignPersonId, string documentName = null )
        {
            try
            {
                // Find the Person to whom this Document belongs.
                int personId = new PersonService( new RockContext() )
                    .Queryable()
                    .Where( p => p.ForeignId == foreignPersonId )
                    .Select( p => p.Id )
                    .FirstOrDefault();

                if ( personId == 0 )
                {
                    GenerateResponse( HttpStatusCode.InternalServerError, "No matching person found." );
                }

                // Return the ID of the newly-saved document.
                return UploadDocument( documentTypeId, personId, documentName );
            }
            catch ( HttpResponseException exception )
            {
                return exception.Response;
            }
            catch
            {
                return new HttpResponseMessage( HttpStatusCode.InternalServerError )
                {
                    Content = new StringContent( "Unhandled exception" )
                };
            }
        }

        /// <summary>
        /// Generates the response.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="message">The message.</param>
        /// <exception cref="HttpResponseException"></exception>
        private void GenerateResponse( HttpStatusCode code, string message = null )
        {
            var response = new HttpResponseMessage( code );

            if ( !string.IsNullOrWhiteSpace( message ) )
            {
                response.Content = new StringContent( message );
            }

            throw new HttpResponseException( response );
        }
    }
}
