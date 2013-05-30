//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.IO;
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
        public void ProcessRequest( HttpContext context )
        {
            // *********************************************
            // TODO: verify user is authorized to save file!
            // *********************************************

            if ( !context.User.Identity.IsAuthenticated )
                throw new WebFaultException<string>( "Must be logged in", System.Net.HttpStatusCode.Forbidden );

            try
            {
                HttpPostedFile uploadedFile = null;
                HttpFileCollection hfc = context.Request.Files;
                foreach ( string fk in hfc.AllKeys )
                {
                    uploadedFile = hfc[fk];
                    break;
                }

                // No file or no data?  No good.
                if ( uploadedFile == null || uploadedFile.ContentLength == 0 )
                {
                    context.Response.Write( "0" );
                    return;
                }

                BinaryFileService fileService = new BinaryFileService();
                Rock.Model.BinaryFile cmsFile = null;

                // was an ID given? if so, fetch that file and replace it with the new one
                if ( context.Request.QueryString.Count > 0 )
                {
                    string anID = context.Request.QueryString[0];
                    int id;
                    cmsFile = ( int.TryParse( anID, out id ) ) ? fileService.Get( id ) : fileService.GetByEncryptedKey( anID );
                }

                if (cmsFile == null)
                {
                    // ...otherwise create a new Cms File
                    cmsFile = new Rock.Model.BinaryFile();
                    cmsFile.IsTemporary = true;
                    fileService.Add( cmsFile, null );
                }

                cmsFile.MimeType = uploadedFile.ContentType;
                cmsFile.FileName = Path.GetFileName( uploadedFile.FileName );
                SaveData( context, uploadedFile.InputStream, cmsFile );

                fileService.Save( cmsFile, null );

                cmsFile.Data = null;
                context.Response.Write( new { Id = cmsFile.Id, FileName = cmsFile.FileName }.ToJson() );

            }
            catch ( Exception ex )
            {
                context.Response.Write( "err:" + ex.Message + "<br>" + ex.StackTrace );
            }
        }

        /// <summary>
        /// Saves the data.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="inputStream">The input stream.</param>
        /// <param name="file">The file.</param>
        public virtual void SaveData( HttpContext context,  Stream inputStream, BinaryFile file )
        {
            using ( MemoryStream ms = new MemoryStream() )
            {
                inputStream.CopyTo( ms );
                if ( file.Data == null )
                {
                    file.Data = new BinaryFileData();
                }
                file.Data.Content = ms.ToArray();
            }
        }
    }
}