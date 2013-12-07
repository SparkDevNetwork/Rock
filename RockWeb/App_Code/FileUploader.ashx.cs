//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.IO;
using System.Linq;
using System.ServiceModel.Web;
using System.Web;
using System.Web.SessionState;
using Rock;
using Rock.Data;
using Rock.Model;

namespace RockWeb
{
    /// <summary>
    /// Handles retrieving file data from storage
    /// </summary>
    public class FileUploader : IHttpHandler, IRequiresSessionState
    {
        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler" /> instance.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Web.IHttpHandler" /> instance is reusable; otherwise, false.</returns>
        public bool IsReusable
        {
            get { return false; }
        }

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler" /> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        /// <exception cref="WebFaultException">Must be logged in</exception>
        public virtual void ProcessRequest( HttpContext context )
        {
            if ( !context.User.Identity.IsAuthenticated )
            {
                throw new WebFaultException<string>( "Must be logged in", System.Net.HttpStatusCode.Forbidden );
            }

            try
            {
                HttpFileCollection hfc = context.Request.Files;
                HttpPostedFile uploadedFile = hfc.AllKeys.Select( fk => hfc[fk] ).FirstOrDefault();

                // No file or no data?  No good.
                if ( uploadedFile == null || uploadedFile.ContentLength == 0 )
                {
                    // TODO: Is there a better response we could send than a 200?
                    context.Response.Write( "0" );
                    return;
                }
                
                // Check to see if BinaryFileType info was sent
                Guid fileTypeGuid = context.Request.QueryString["fileTypeGuid"].AsGuid();

                RockContext rockContext = new RockContext();
                BinaryFileType binaryFileType = new BinaryFileTypeService( rockContext ).Get( fileTypeGuid );

                // always create a new BinaryFile record of IsTemporary when a file is uploaded
                BinaryFile binaryFile = new BinaryFile();
                binaryFile.IsTemporary = true;
                binaryFile.BinaryFileTypeId = binaryFileType.Id;
                binaryFile.MimeType = uploadedFile.ContentType;
                binaryFile.FileName = Path.GetFileName( uploadedFile.FileName );
                binaryFile.Data = new BinaryFileData();

                //NOTE: GetFileBytes can get overridden by a child class (ImageUploader.ashx.cs for example)
                binaryFile.Data.Content = GetFileBytes( context, uploadedFile );

                var binaryFileService = new BinaryFileService( rockContext );
                binaryFileService.Add( binaryFile, null );
                binaryFileService.Save( binaryFile, null );

                var response = new
                {
                    Id = binaryFile.Id,
                    FileName = binaryFile.FileName
                };

                context.Response.Write( response.ToJson() );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, context );
                context.Response.Write( "err:" + ex.Message + "<br>" + ex.StackTrace );
            }
        }

        /// <summary>
        /// Gets the file bytes.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="uploadedFile">The uploaded file.</param>
        /// <returns></returns>
        public virtual byte[] GetFileBytes( HttpContext context, HttpPostedFile uploadedFile )
        {
            var bytes = new byte[uploadedFile.ContentLength];
            uploadedFile.InputStream.Read( bytes, 0, uploadedFile.ContentLength );
            return bytes;
        }
    }
}