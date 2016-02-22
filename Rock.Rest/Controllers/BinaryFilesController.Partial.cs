// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
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
    ///
    /// </summary>
    public partial class BinaryFilesController
    {
        /// <summary>
        /// Sets a persons profile image
        /// </summary>
        /// <param name="personId">The person's ID</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/BinaryFiles/SetProfileImage/{personId}" )]
        public HttpResponseMessage SetProfileImage( int personId )
        {
            try
            {
                var rockContext = new RockContext();
                var context = HttpContext.Current;
                var files = context.Request.Files;
                var uploadedFile = files.AllKeys.Select( fk => files[fk] ).FirstOrDefault();

                var fileTypeGuid = Rock.SystemGuid.BinaryFiletype.PERSON_IMAGE.AsGuid();
                var binaryFileType = new BinaryFileTypeService( rockContext ).Get( fileTypeGuid );

                if ( binaryFileType == null )
                {
                    GenerateResponse( HttpStatusCode.InternalServerError, "There is no person image file type" );
                }

                var currentUser = UserLoginService.GetCurrentUser();
                var currentPerson = currentUser != null ? currentUser.Person : null;

                if ( !binaryFileType.IsAuthorized( Rock.Security.Authorization.EDIT, currentPerson ) )
                {
                    GenerateResponse( HttpStatusCode.Unauthorized, "Not authorized to upload this type of file" );
                }

                var binaryFileService = new BinaryFileService( rockContext );
                var binaryFile = new BinaryFile();
                var person = new PersonService( rockContext ).Get( personId );

                if ( person == null )
                {
                    GenerateResponse( HttpStatusCode.BadRequest, "Person does not exist" );
                }

                if ( person.Photo != null )
                {
                    binaryFileService.Delete( person.Photo );
                    person.PhotoId = null;
                }

                if ( uploadedFile != null )
                {
                    binaryFileService.Add( binaryFile );

                    binaryFile.IsTemporary = false;
                    binaryFile.BinaryFileTypeId = binaryFileType.Id;
                    binaryFile.MimeType = uploadedFile.ContentType;
                    binaryFile.FileName = Path.GetFileName( uploadedFile.FileName );
                    binaryFile.ContentStream = ImageUtilities.GetFileContentStream( uploadedFile, true );

                    person.PhotoId = binaryFile.Id;
                }

                rockContext.SaveChanges();

                var response = new
                {
                    Id = binaryFile.Id,
                    FileName = binaryFile.FileName,
                    Url = person.PhotoUrl
                };

                return new HttpResponseMessage( HttpStatusCode.Created )
                {
                    Content = new StringContent( response.ToJson() )
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
        /// Generates the response.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="message">The message.</param>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
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