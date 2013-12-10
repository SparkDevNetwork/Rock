//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Web;
using Rock;
using Rock.Model;

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
            try
            {
                var queryString = context.Request.QueryString;

                int fileId = queryString["id"].AsInteger() ?? 0;
                Guid fileGuid = queryString["guid"].AsGuid();

                if ( fileId == 0 && fileGuid.Equals( Guid.Empty ) )
                {
                    throw new Exception( "file id key must be a guid or an int" );
                }

                BinaryFileService binaryFileService = new BinaryFileService();
                if ( fileGuid != Guid.Empty )
                {
                    return binaryFileService.BeginGet( cb, context, fileGuid );
                }
                else
                {
                    return binaryFileService.BeginGet( cb, context, fileId );
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, context );
                context.Response.StatusCode = 500;
                context.Response.StatusDescription = ex.Message;
                context.ApplicationInstance.CompleteRequest();

                return null;
            }
        }

        /// <summary>
        /// Provides an end method for an asynchronous process. 
        /// </summary>
        /// <param name="result">An IAsyncResult that contains information about the status of the process.</param>
        public void EndProcessRequest( IAsyncResult result )
        {
            // restore the context from the asyncResult.AsyncState 
            HttpContext context = (HttpContext)result.AsyncState;

            try
            {
                context.Response.Clear();

                BinaryFile binaryFile = new BinaryFileService().EndGet( result, context );
                if ( binaryFile != null )
                {
                    context.Response.AddHeader( "content-disposition", string.Format( "inline;filename={0}", binaryFile.FileName ) );
                    context.Response.ContentType = binaryFile.MimeType;

                    if ( binaryFile.Data != null )
                    {
                        if ( binaryFile.Data.Content != null )
                        {
                            context.Response.BinaryWrite( binaryFile.Data.Content );
                            context.Response.Flush();
                            context.ApplicationInstance.CompleteRequest();
                            return;
                        }
                    }
                }

                context.Response.StatusCode = 404;
                context.Response.StatusDescription = "Unable to find the requested file.";
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, context );
                context.Response.StatusCode = 500;
                context.Response.StatusDescription = ex.Message;
                context.Response.Flush();
                context.ApplicationInstance.CompleteRequest();
            }
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
                return false;
            }
        }
    }
}