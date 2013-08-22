//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Web;
using Rock.Model;
using Rock.Storage;
using Rock.Storage.Provider;

namespace RockWeb
{
    /// <summary>
    /// Handles retrieving file data from storage
    /// </summary>
    public class GetFile : IHttpAsyncHandler
    {
        // TODO: Does security need to be taken into consideration in order to view a file?

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

                if ( !( queryString["id"] == null || queryString["guid"] == null ) )
                {
                    throw new Exception( "file id must be provided" );
                }

                var id = !string.IsNullOrEmpty( queryString["id"] ) ? queryString["id"] : queryString["guid"];
                int fileId;
                Guid fileGuid = Guid.Empty;

                if ( !( int.TryParse( id, out fileId ) || Guid.TryParse( id, out fileGuid ) ) )
                {
                    throw new Exception( "file id key must be a guid or an int" );
                }

                SqlConnection conn = new SqlConnection( string.Format( "{0};Asynchronous Processing=true;", ConfigurationManager.ConnectionStrings["RockContext"].ConnectionString ) );
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "BinaryFile_sp_getByID";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add( new SqlParameter( "@Id", fileId ) );
                cmd.Parameters.Add( new SqlParameter( "@Guid", fileGuid ) );

                // store our Command to be later retrieved by EndProcessRequest
                context.Items.Add( "cmd", cmd );

                // start async DB read
                return cmd.BeginExecuteReader( cb, context,
                    CommandBehavior.SequentialAccess |  // doesn't load whole column into memory
                    CommandBehavior.SingleRow |         // performance improve since we only want one row
                    CommandBehavior.CloseConnection );  // close connection immediately after read
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, context );
                context.Response.StatusCode = 500;
                context.Response.StatusDescription = ex.Message;
                context.Response.End();
                return null;
            }
        }

        /// <summary>
        /// Provides an end method for an asynchronous process. 
        /// </summary>
        /// <param name="result">An IAsyncResult that contains information about the status of the process.</param>
        public void EndProcessRequest( IAsyncResult result )
        {
            HttpContext context = (HttpContext)result.AsyncState;

            try
            {
                // restore the command from the context
                SqlCommand cmd = (SqlCommand)context.Items["cmd"];
                using ( SqlDataReader reader = cmd.EndExecuteReader( result ) )
                {
                    reader.Read();
                    context.Response.Clear();
                    var fileName = (string) reader["FileName"];
                    context.Response.AddHeader( "content-disposition", string.Format( "inline;filename={0}", fileName ) );
                    context.Response.ContentType = (string) reader["MimeType"];

                    var entityTypeName = (string) reader["StorageTypeName"];
                    var provider = ProviderContainer.GetComponent( entityTypeName );

                    if ( provider is Database )
                    {
                        context.Response.BinaryWrite( (byte[]) reader["Data"] );
                    }
                    else
                    {
                        var url = (string) reader["Url"];
                        Stream stream;

                        if ( url.StartsWith( "~/" ) )
                        {
                            var path = context.Server.MapPath( url );
                            var fileInfo = new FileInfo( path );
                            stream = fileInfo.Open( FileMode.Open, FileAccess.Read );
                        }
                        else
                        {
                            var request = WebRequest.Create( url );
                            var response = request.GetResponse();
                            stream = response.GetResponseStream();
                        }

                        if ( stream != null )
                        {
                            using ( var memoryStream = new MemoryStream() )
                            {
                                stream.CopyTo( memoryStream );
                                stream.Close();
                                context.Response.BinaryWrite( memoryStream.ToArray() );
                            }
                        }
                        else
                        {
                            context.Response.StatusCode = 404;
                            context.Response.StatusDescription = "Unable to find the requested file.";
                        }
                    }

                    context.Response.Flush();
                    context.Response.End();
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, context );
                context.Response.StatusCode = 500;
                context.Response.StatusDescription = ex.Message;
                context.Response.Flush();
                context.Response.End();
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