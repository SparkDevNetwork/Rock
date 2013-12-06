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

                using ( new Rock.Data.UnitOfWorkScope() )
                {
                    BinaryFileService binaryFileService = new BinaryFileService();
                    var fileIdParameter = context.Request.QueryString["fileId"];


                    // TODO var fileGuidParameter = context.Request.QueryString["fileGuid"];


                    BinaryFile binaryFile = null;
                    BinaryFileType binaryFileType = null;

                    // Attempt to find file by an Id or EncryptedKey passed in
                    if ( !string.IsNullOrEmpty( fileIdParameter ) )
                    {
                        int? fileId = fileIdParameter.AsInteger();
                        if ( fileId.HasValue )
                        {
                            binaryFile = binaryFileService.Get( fileId.Value );
                        }
                        else
                        {
                            binaryFile = binaryFileService.GetByEncryptedKey( fileIdParameter );
                        }
                    }

                    // ...otherwise create a new BinaryFile
                    if ( binaryFile == null )
                    {
                        binaryFile = new BinaryFile();
                    }

                    // Check to see if BinaryFileType info was sent
                    BinaryFileTypeService binaryFileTypeService = new BinaryFileTypeService();
                    var fileTypeParameter = context.Request.QueryString["fileTypeGuid"];

                    if ( !string.IsNullOrEmpty( fileTypeParameter ) )
                    {
                        Guid fileTypeGuid = fileTypeParameter.AsGuid();
                        if ( fileTypeGuid != Guid.Empty )
                        {
                            binaryFileType = binaryFileTypeService.Get( fileTypeGuid );
                        }
                        else
                        {
                            binaryFileType = binaryFileTypeService.GetByEncryptedKey( fileTypeParameter );
                        }
                    }
                    else if ( binaryFile.BinaryFileType != null )
                    {
                        binaryFileType = binaryFile.BinaryFileType;
                    }

                    // If we're dealing with a new BinaryFile and a BinaryFileType Guid was passed in,
                    // set its Id before the BinaryFile gets saved to the DB.
                    if ( binaryFile.BinaryFileType == null && binaryFileType != null )
                    {
                        binaryFile.BinaryFileTypeId = binaryFileType.Id;
                    }

                    binaryFile.MimeType = uploadedFile.ContentType;
                    binaryFile.FileName = Path.GetFileName( uploadedFile.FileName );
                    // NOTE: Don't set binaryFile.StorageEntityType here, let the BinaryFileService take care of it
                    binaryFile.Data = binaryFile.Data ?? new BinaryFileData();
                    binaryFile.Data.Content = GetFileBytes( context, uploadedFile );

                    // Save the file using the fileService (id = 0 means it is a new file)
                    if ( binaryFile.Id == 0 )
                    {
                        binaryFileService.Add( binaryFile, null );
                    }

                    binaryFileService.Save( binaryFile, null );

                    var response = new
                    {
                        Id = binaryFile.Id,
                        FileName = binaryFile.FileName
                    };

                    context.Response.Write( response.ToJson() );
                }
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