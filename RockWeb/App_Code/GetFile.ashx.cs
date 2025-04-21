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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;

namespace RockWeb
{
    /// <summary>
    /// Handles retrieving file data from storage
    /// </summary>
    public class GetFile : IHttpAsyncHandler
    {
        /// <summary>
        /// Called to initialize an asynchronous call to the HTTP handler. 
        /// </summary>
        /// <param name="context">An HttpContext that provides references to intrinsic server objects used to service HTTP requests.</param>
        /// <param name="cb">The AsyncCallback to call when the asynchronous method call is complete.</param>
        /// <param name="extraData">Any state data needed to process the request.</param>
        /// <returns>An IAsyncResult that contains information about the status of the process.</returns>
        public IAsyncResult BeginProcessRequest( HttpContext context, AsyncCallback cb, object extraData )
        {
            var securitySettings = new SecuritySettingsService().SecuritySettings;
            var disablePredictableIds = securitySettings.DisablePredictableIds;

            if ( disablePredictableIds )
            {
                var fileIdKey = context.Request.QueryString["fileIdKey"];
                if ( !string.IsNullOrEmpty( fileIdKey ) )
                {
                    int? fileId = IdHasher.Instance.GetId( fileIdKey );
                    if ( fileId.HasValue )
                    {
                        return new BinaryFileService( new RockContext() ).BeginGet( cb, context, fileId.Value );
                    }
                }

                var fileGuidString = context.Request.QueryString["guid"];
                if ( !string.IsNullOrEmpty( fileGuidString ) )
                {
                    Guid fileGuid = new Guid( fileGuidString );
                    return new BinaryFileService( new RockContext() ).BeginGet( cb, context, fileGuid );
                }

            }
            else
            {
                int fileId = context.Request.QueryString["id"].AsInteger();
                Guid fileGuid = context.Request.QueryString["guid"].AsGuid();

                if ( fileGuid != Guid.Empty )
                {
                    return new BinaryFileService( new RockContext() ).BeginGet( cb, context, fileGuid );
                }
                else if ( fileId != 0 )
                {
                    return new BinaryFileService( new RockContext() ).BeginGet( cb, context, fileId );
                }
            }

            return new SendErrorDelegate( SendError ).BeginInvoke( context, 400, "File id key must be a guid or an int.", cb, extraData );
        }

        /// <summary>
        /// Provides an end method for an asynchronous process. 
        /// </summary>
        /// <param name="result">An IAsyncResult that contains information about the status of the process.</param>
        public void EndProcessRequest( IAsyncResult result )
        {
            // restore the context from the asyncResult.AsyncState 
            HttpContext context = ( HttpContext ) result.AsyncState;

            if ( context != null )
            {
                context.Response.Clear();

                var rockContext = new RockContext();

                bool requiresViewSecurity;
                BinaryFile binaryFile = new BinaryFileService( rockContext ).EndGet( result, context, out requiresViewSecurity );
                if ( binaryFile != null )
                {
                    binaryFile.BinaryFileType = binaryFile.BinaryFileType ?? new BinaryFileTypeService( rockContext ).Get( binaryFile.BinaryFileTypeId.Value );
                    //UserLogin currentUser = UserLoginService.GetCurrentUser();
                    var currentUser = new UserLoginService( rockContext ).GetByUserName( UserLogin.GetCurrentUserName() );
                    Person currentPerson = currentUser?.Person;
                    var parentEntityAllowsView = binaryFile.ParentEntityAllowsView( currentPerson );

                    // If no parent entity is specified then check if there is scecurity on the BinaryFileType
                    // Use BinaryFileType.RequiresViewSecurity because checking security for every file is slow (~40ms+ per request)
                    if ( parentEntityAllowsView == null && requiresViewSecurity )
                    {
                        var securityGrant = SecurityGrant.FromToken( context.Request.QueryString["securityGrant"] );

                        if ( !binaryFile.IsAuthorized( Authorization.VIEW, currentPerson ) && securityGrant?.IsAccessGranted( binaryFile, Authorization.VIEW ) != true )
                        {
                            SendError( context, 403, "Not authorized to view file." );
                            return;
                        }
                    }

                    // Since this has a value use it
                    if ( parentEntityAllowsView == false )
                    {
                        SendError( context, 403, "Not authorized to view file." );
                        return;
                    }

                    // Security checks pass so send the file
                    var binaryFileType = BinaryFileTypeCache.Get( binaryFile.BinaryFileTypeId.Value );
                    SendFile( context, binaryFile.ContentStream, binaryFile.MimeType, binaryFile.FileName, binaryFile.Guid.ToString( "N" ), binaryFileType.CacheControlHeader );
                    return;
                }

                SendError( context, 404, "File could not be found." );
            }
        }

        /// <summary>
        /// Sends the file.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="fileContents">The file contents.</param>
        /// <param name="mimeType">Type of the MIME.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="eTag">The e tag.</param>
        private void SendFile( HttpContext context, Stream fileContents, string mimeType, string fileName, string eTag, RockCacheability rockCacheability )
        {
            /* 
                9/23/2021 - JME - Range Support
                The original implementation of range support did not take into account the size of the file compared to the requested range in bytes. For
                example if the Azure CDN requested 8MB of bytes starting at the 8MB byte (the second chunk 8MB chunk) and the file is only 12MB is size the
                "Content-Range" response header was being sent as: "bytes 8388608-16777215/12582912". Basically saying "I'm going to send you 8MB but only
                sending ~4MB.

                Also, if the file was quite large (say 50MB) and the Azure CDN made the same request it would send back the correct "Content-Range" header
                ("bytes 8388608-16777215/52428800") but would send the contents of the file starting at 8MB all the way to the end of the file (instead of
                stopping at the 16MB point).

                The logic had to be updated to send the correct header if the file does not have the full range available and to send only the requested range
                of content.

                More Details: https://developer.mozilla.org/en-US/docs/Web/HTTP/Range_requests

                Worries:
                Little nervous of some of the +1 -1 logic from the SO article. Worried that when there is more content than requested in the range we could be
                off by one byte. Used the curl below to does the size of the output. Looks correct.

                In the Azure CDN example above the request header from Azure for the range is:
                "Range: bytes=8388608-16777215"

                curl -so curl.bin -i --location --request GET 'http://{server_name}/GetFile.ashx?Id=538' --header 'Range: bytes=8388608-16777215' -w '%{size_download}'
                Outputs: 8388608

                According to the documentation here: https://developer.mozilla.org/en-US/docs/Web/HTTP/Range_requests this is correct. In their example of
                "Range: bytes=0-1023" the response length should be 1024 (inclusive of start and end).
                so
                "Range: bytes=8388608-16777215" = 16777215 - 8388608 + 1 = 8388608
            */

            const int _defaultBufferSize = 4096;

            var rangeStartIndex = 0;
            var rangeEndIndex = 0;
            var fileLength = ( int ) fileContents.Length;
            var responseLength = fileLength;
            var isRangeRequest = false;
            var numberOfBytesSent = 0;

            // Rresumable logic from http://stackoverflow.com/a/6475414/1755417
            if ( context.Request.Headers["Range"] != null && ( context.Request.Headers["If-Range"] == null ) )
            {
                isRangeRequest = true;
                var match = Regex.Match( context.Request.Headers["Range"], @"bytes=(\d*)-(\d*)" );
                rangeStartIndex = match.Groups[1].Value.AsInteger();
                rangeEndIndex = match.Groups[2].Value.AsIntegerOrNull() + 1 ?? fileLength;

                var requestedRangeSize = rangeEndIndex - rangeStartIndex;
                var rangeAvailable = fileLength - rangeStartIndex;

                // Response length will be the range requested or the remaining file content size
                responseLength = Math.Min( requestedRangeSize, rangeAvailable );

                // Send the Content-Range header
                context.Response.Headers["Content-Range"] = "bytes " + rangeStartIndex + "-" + ( rangeStartIndex + responseLength - 1 ) + "/" + fileLength;

                // Send 209 status code 'Partial Content'
                context.Response.StatusCode = ( int ) System.Net.HttpStatusCode.PartialContent;
            }

            context.Response.Clear();
            context.Response.Buffer = false;
            context.Response.Headers["Accept-Ranges"] = "bytes";

            bool sendAsAttachment = context.Request.QueryString["attachment"].AsBooleanOrNull() ?? false;

            context.Response.AddHeader( "content-disposition", string.Format( "{1};filename={0}", fileName.MakeValidFileName(), sendAsAttachment ? "attachment" : "inline" ) );
            context.Response.AddHeader( "content-length", responseLength.ToString() );

            rockCacheability.SetupHttpCachePolicy( context.Response.Cache );

            context.Response.Cache.SetETag( eTag ); // required for IE9 resumable downloads
            context.Response.ContentType = mimeType;
            byte[] buffer = new byte[_defaultBufferSize];

            if ( context.Response.IsClientConnected )
            {
                using ( var fileStream = fileContents )
                {
                    if ( fileStream.CanSeek )
                    {
                        fileStream.Seek( rangeStartIndex, SeekOrigin.Begin );
                    }

                    while ( true )
                    {
                        // If range request we may need to adjust the size of the buffer if we don't need to read the entire 4k default size
                        if ( isRangeRequest )
                        {
                            var amountLeftToRead = responseLength - numberOfBytesSent;
                            if ( amountLeftToRead < _defaultBufferSize )
                            {
                                buffer = new byte[amountLeftToRead];
                            }
                        }

                        var bytesRead = fileStream.Read( buffer, 0, buffer.Length );
                        if ( bytesRead == 0 )
                        {
                            break;
                        }

                        if ( !context.Response.IsClientConnected )
                        {
                            // quit sending if the client isn't connected
                            break;
                        }

                        try
                        {
                            context.Response.OutputStream.Write( buffer, 0, bytesRead );
                            numberOfBytesSent = numberOfBytesSent + bytesRead;
                        }
                        catch ( HttpException ex )
                        {
                            if ( !context.Response.IsClientConnected )
                            {
                                // if client disconnected during the .write, ignore
                            }
                            else
                            {
                                throw ex;
                            }
                        }
                    }
                }
            }

            context.ApplicationInstance.CompleteRequest();
        }


        private delegate void SendErrorDelegate( HttpContext context, int code, string message );

        /// <summary>
        /// Sends an error code response and completes the request.
        /// </summary>
        /// <param name="context">THe HttpContext for this request.</param>
        /// <param name="code">The response code to send.</param>
        /// <param name="message">The response message to send.</param>
        private void SendError( HttpContext context, int code, string message )
        {
            context.Response.Clear();
            context.Response.StatusCode = code;
            context.Response.StatusDescription = message;
            context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler" /> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void ProcessRequest( HttpContext context )
        {
            throw new NotImplementedException( "The method or operation is not implemented. This is an asynchronous file handler." );
        }

        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler" /> instance.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Web.IHttpHandler" /> instance is reusable; otherwise, false.</returns>
        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }
}