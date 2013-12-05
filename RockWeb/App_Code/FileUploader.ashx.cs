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

                BinaryFileService fileService = new BinaryFileService();
                var id = context.Request.QueryString["fileId"];
                BinaryFile file = null;
                BinaryFileType fileType = null;

                // Attempt to find file by an Id or Guid passed in
                if ( !string.IsNullOrEmpty( id ) )
                {
                    int fileId;
                    file = int.TryParse( id, out fileId ) ? fileService.Get( fileId ) : fileService.GetByEncryptedKey( id );
                }

                // ...otherwise create a new BinaryFile
                if ( file == null )
                {
                    file = new BinaryFile();
                }

                // Check to see if BinaryFileType info was sent
                BinaryFileTypeService fileTypeService = new BinaryFileTypeService();
                var guid = context.Request.QueryString["fileTypeGuid"];

                if ( !string.IsNullOrEmpty( guid ) )
                {
                    Guid fileTypeGuid;
                    fileType = Guid.TryParse( guid, out fileTypeGuid ) ? fileTypeService.Get( fileTypeGuid ) : fileTypeService.GetByEncryptedKey( guid );
                }
                else if ( file.BinaryFileType != null )
                {
                    fileType = file.BinaryFileType;
                }

                // If we're dealing with a new BinaryFile and a BinaryFileType Guid was passed in,
                // set its Id before the BinaryFile gets saved to the DB.
                if ( file.BinaryFileType == null && fileType != null )
                {
                    file.BinaryFileTypeId = fileType.Id;
                }

                file.MimeType = uploadedFile.ContentType;
                file.FileName = Path.GetFileName( uploadedFile.FileName );
                file.StorageEntityTypeId = fileType.StorageEntityTypeId;

                file.Data = GetFileData( context, uploadedFile );

                // Save the file using the fileService
                if ( file.Id == 0 )
                {
                    fileService.Add( file, null );
                }

                fileService.Save( file, null );

                context.Response.Write( new { Id = file.Id, FileName = file.FileName }.ToJson() );

            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, context );
                context.Response.Write( "err:" + ex.Message + "<br>" + ex.StackTrace );
            }
        }

        /// <summary>
        /// Gets the file data.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="uploadedFile">The uploaded file.</param>
        /// <returns></returns>
        public virtual BinaryFileData GetFileData( HttpContext context, HttpPostedFile uploadedFile)
        {
            var bytes = new byte[uploadedFile.ContentLength];
            uploadedFile.InputStream.Read( bytes, 0, uploadedFile.ContentLength );
            return new BinaryFileData { Content = bytes };
        }
    }
}