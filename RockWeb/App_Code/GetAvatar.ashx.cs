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
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;

using Rock;
using Rock.Data;
using Rock.Drawing.Avatar;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace RockWeb
{
    /// <summary>
    /// Handles retrieving file (image) data from storage
    /// </summary>
    public class GetAvatar : IHttpAsyncHandler
    {
        // Implemented this as an IHttpAsyncHandler instead of IHttpHandler to improve performance
        // https://stackoverflow.com/questions/48528773/ihttphandler-versus-httptaskasynchandler-performance
        // Good overview on how to implement an IHttpAsyncHandler
        // https://www.madskristensen.net/blog/how-to-use-the-ihttpasynchandler-in-aspnet/

        // Delegate setup variables
        private AsyncProcessorDelegate _Delegate;
        protected delegate void AsyncProcessorDelegate( HttpContext context );

        /// <summary>
        /// Called to initialize an asynchronous call to the HTTP handler. 
        /// </summary>
        /// <param name="context">An HttpContext that provides references to intrinsic server objects used to service HTTP requests.</param>
        /// <param name="cb">The AsyncCallback to call when the asynchronous method call is complete.</param>
        /// <param name="extraData">Any state data needed to process the request.</param>
        /// <returns>An IAsyncResult that contains information about the status of the process.</returns>
        public IAsyncResult BeginProcessRequest( HttpContext context, AsyncCallback cb, object extraData )
        {
            _Delegate = new AsyncProcessorDelegate( ProcessRequest );

            return _Delegate.BeginInvoke( context, cb, extraData );
        }

        /// <summary>
        /// Provides an end method for an asynchronous process. 
        /// </summary>
        /// <param name="result">An IAsyncResult that contains information about the status of the process.</param>
        public void EndProcessRequest( IAsyncResult result )
        {
            _Delegate.EndInvoke( result );
        }

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler" /> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void ProcessRequest( HttpContext context )
        {
            // Read query string parameters
            var settings = ReadSettingsFromRequest( context.Request );

            string cacheFolder = context.Request.MapPath( $"~/App_Data/Avatar/Cache/" );
            string cachedFilePath = $"{cacheFolder}{settings.CacheKey}.png";

            // Fill in missing colors. This must be done after the creation of the cache key to prevent caching avatars in every color of the rainbow
            settings.AvatarColors.GenerateMissingColors();

            // Process any cache refresh request for single item
            if ( context.Request.QueryString["RefreshItemCache"] != null && context.Request.QueryString["RefreshItemCache"].AsBoolean() )
            {
                RefreshItemCache( cachedFilePath );
            }

            // Process any cache refresh for all items
            if ( context.Request.QueryString["RefreshCache"] != null && context.Request.QueryString["RefreshCache"].AsBoolean() )
            {
                RefreshCache( cacheFolder );
            }

            Stream fileContent = null;
            try
            {
                fileContent = FetchFromCache( cachedFilePath );

                // The file is not in the cache so we'll create it
                if ( fileContent == null )
                {
                    fileContent = AvatarHelper.CreateAvatar( settings );
                }

                // Something has gone really wrong so we'll send an error message
                if ( fileContent == null )
                {
                    context.Response.StatusCode = System.Net.HttpStatusCode.InternalServerError.ConvertToInt();
                    context.Response.StatusDescription = "The requested avatar could not be created.";
                    context.ApplicationInstance.CompleteRequest();
                    return;
                }

                // Add cache validation headers
                context.Response.AddHeader( "Last-Modified", DateTime.Now.ToUniversalTime().ToString( "R" ) );
                context.Response.AddHeader( "ETag", DateTime.Now.ToString().XxHash() );

                // Configure client to cache image locally for 1 week
                context.Response.Cache.SetCacheability( HttpCacheability.Public );
                context.Response.Cache.SetMaxAge( new TimeSpan( 7, 0, 0, 0, 0 ) );

                context.Response.ContentType = "image/png";

                // Stream the contents of the file to the response
                using ( var responseStream = fileContent )
                {
                    context.Response.AddHeader( "content-disposition", "inline;filename=" + $"{settings.CacheKey}.png".UrlEncode() );
                    if ( responseStream.CanSeek )
                    {
                        responseStream.Seek( 0, SeekOrigin.Begin );
                    }
                    responseStream.CopyTo( context.Response.OutputStream );
                    context.Response.Flush();
                }
            }
            /*
                8/31/2023 - PA

                Catch and ignore exceptions caused when the client browser drops the connection before the request is complete.

                Reason: https://github.com/SparkDevNetwork/Rock/issues/5521
            */
            catch ( System.Web.HttpException ex )
            {
                if ( ex.Message.IsNotNullOrWhiteSpace() && ex.Message.Contains( "The remote host closed the connection." ) )
                {
                    // Ignore the exception
                    context.ClearError();
                    context.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                fileContent?.Dispose();
            }
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

        #region Private Methods

        /// <summary>
        /// Refreshs the cache for a specific item
        /// </summary>
        /// <param name="filePath"></param>
        private void RefreshItemCache( string filePath )
        {
            // Ensure the person is allowed to refresh the cache
            if ( !IsPersonAllowedRefeshCache() )
            {
                return;
            }

            // Delete the file if it exists
            if ( File.Exists( filePath ) )
            {
                File.Delete( filePath );
            }
        }

        /// <summary>
        /// Determines if the person is in a role that allows them to refresh cache
        /// </summary>
        /// <returns></returns>
        private bool IsPersonAllowedRefeshCache()
        {
            var rockContext = new RockContext();
            var currentUser = new UserLoginService( rockContext ).GetByUserName( UserLogin.GetCurrentUserName() );
            var currentPerson = currentUser != null ? currentUser.Person : null;

            return RoleCache.AllRoles()
                        .Where( r =>
                            AvatarHelper.AuthorizedRefreshCacheRoleGuids.Contains( r.Guid )
                            && r.IsPersonInRole( currentPerson.Guid )
                        )
                        .Any();
        }

        /// <summary>
        /// Refreshes the cache on all cached avatars
        /// </summary>
        /// <param name="cacheFolder"></param>
        private void RefreshCache( string cacheFolder )
        {
            // Ensure the person is allowed to refresh the cache
            if ( !IsPersonAllowedRefeshCache() )
            {
                return;
            }

            // Delete all files
            foreach ( string sFile in System.IO.Directory.GetFiles( cacheFolder, "*.png" ) )
            {
                File.Delete( sFile );
            }
        }

        /// <summary>
        /// Reads the settings from request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>RockWeb.AvatarSettings.</returns>
        private AvatarSettings ReadSettingsFromRequest( HttpRequest request )
        {
            var settings = new AvatarSettings();

            // Calculate the physical path to store the cached files to
            settings.CachePath = request.MapPath( $"~/App_Data/Avatar/Cache/" );

            // Colors
            var backgroundColor = string.Empty;
            var foregroundColor = string.Empty;

            if ( request.QueryString["BackgroundColor"] != null )
            {
                backgroundColor = $"#{request.QueryString["BackgroundColor"]}".AsHexColorString();
            }

            if ( request.QueryString["ForegroundColor"] != null )
            {
                foregroundColor = $"#{request.QueryString["ForegroundColor"]}".AsHexColorString();
            }

            settings.AvatarColors.BackgroundColor = backgroundColor;
            settings.AvatarColors.ForegroundColor = foregroundColor;
            
            // Size
            if ( request.QueryString["Size"] != null )
            {
                settings.Size = request.QueryString["Size"].AsInteger();
            }

            if ( request.QueryString["w"] != null )
            {
                settings.Size = request.QueryString["w"].AsInteger();
            }

            if ( request.QueryString["h"] != null )
            {
                settings.Size = request.QueryString["h"].AsInteger();
            }

            if ( request.QueryString["width"] != null )
            {
                settings.Size = request.QueryString["width"].AsInteger();
            }

            if ( request.QueryString["height"] != null )
            {
                settings.Size = request.QueryString["height"].AsInteger();
            }

            if ( request.QueryString["maxwidth"] != null )
            {
                settings.Size = request.QueryString["maxwidth"].AsInteger();
            }

            if ( request.QueryString["maxheight"] != null )
            {
                settings.Size = request.QueryString["maxheight"].AsInteger();
            }

            // Style
            if ( request.QueryString["Style"] != null )
            {
                if ( request.QueryString["Style"].ToLower() == "icon" )
                {
                    settings.AvatarStyle = AvatarStyle.Icon;
                }
            }

            // Age Classification
            if ( request.QueryString["AgeClassification"] != null )
            {
                settings.AgeClassification = ( AgeClassification ) Enum.Parse( typeof( AgeClassification ), request.QueryString["AgeClassification"], true );
            }

            // Gender
            if ( request.QueryString["Gender"] != null )
            {
                settings.Gender = ( Gender ) Enum.Parse( typeof( Gender ), request.QueryString["Gender"], true );
            }

            // Text
            if ( request.QueryString["Text"] != null )
            {
                settings.Text = request.QueryString["Text"];
            }

            // Photo Id
            if ( request.QueryString["PhotoId"] != null )
            {
                settings.PhotoId = request.QueryString["PhotoId"].AsIntegerOrNull();
            }

            // Record Type Guid
            if ( request.QueryString["RecordTypeId"] != null )
            {
                settings.RecordTypeId = request.QueryString["RecordTypeId"].AsIntegerOrNull();
            }

            // Bold
            if ( request.QueryString["Bold"] != null )
            {
                settings.IsBold = request.QueryString["Bold"].AsBoolean();
            }

            // Corner Radius + Circle
            if ( request.QueryString["Radius"] != null )
            {
                var radius = request.QueryString["Radius"];

                if ( radius.ToLower() == "circle" )
                {
                    settings.IsCircle = true;
                }
                else
                {
                    settings.CornerRadius = radius.AsInteger();
                }
            }

            // Prefers Light
            if ( request.QueryString["PrefersLight"] != null )
            {
                settings.PrefersLight = request.QueryString["PrefersLight"].AsBoolean();
            }

            // Logic for loading from Person Objects
            // ----------------------------------------

            Person person = null;
            // Person Guid
            if ( request.QueryString["PersonGuid"] != null )
            {
                settings.PersonGuid = request.QueryString["PersonGuid"].AsGuidOrNull();

                if ( settings.PersonGuid.HasValue )
                {
                    person = new PersonService( new RockContext() ).Get( settings.PersonGuid.Value );
                }
            }

            // Person Id
            if ( request.QueryString["PersonId"] != null )
            {
                settings.PersonId = request.QueryString["PersonId"].AsIntegerOrNull();

                if ( settings.PersonId.HasValue )
                {
                    person = new PersonService( new RockContext() ).Get( settings.PersonId.Value );
                }
            }

            // Person Alias Guid
            if ( request.QueryString["PersonAliasGuid"] != null )
            {
                var personAliasGuid = request.QueryString["PersonAliasGuid"].AsGuidOrNull();

                if ( personAliasGuid.HasValue )
                {
                    person = new PersonAliasService( new RockContext() ).GetPerson( personAliasGuid.Value );
                    settings.PersonId = person?.Id;
                }
            }

            // Person Alias Id
            if ( request.QueryString["PersonAliasId"] != null )
            {
                var personAliasId = request.QueryString["PersonAliasId"].AsIntegerOrNull();

                if ( personAliasId.HasValue )
                {
                    person = new PersonAliasService( new RockContext() ).GetPerson( personAliasId.Value );
                    settings.PersonId = person?.Id;
                }
            }

            // Load configuration from the person object
            if ( person != null )
            {
                settings.RecordTypeId = person.RecordStatusValueId;
                settings.Gender = person.Gender;
                settings.Text = person.Initials;
                settings.AgeClassification = person.AgeClassification;
                settings.PhotoId = person.PhotoId;
            }

            return settings;
        }

        /// <summary>
        /// Attempts to retrieve the file from cache.
        /// </summary>
        /// <param name="physicalPath">The physical path.</param>
        /// <returns>Stream.</returns>
        private Stream FetchFromCache( string physicalPath )
        {
            try
            {
                if ( File.Exists( physicalPath ) )
                {
                    // Touch the file to update the last modified date
                    File.SetLastWriteTimeUtc( physicalPath, DateTime.UtcNow );

                    return File.Open( physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read );
                }

                return null;
            }
            catch
            {
                // if it fails, return null, which will result in fetching it from the database instead
                return null;
            }
        }

        /// <summary>
        /// Sends a 403 (forbidden)
        /// </summary>
        /// <param name="context">The context.</param>
        private void SendNotAuthorized( HttpContext context )
        {
            context.Response.StatusCode = System.Net.HttpStatusCode.Forbidden.ConvertToInt();
            context.Response.StatusDescription = "Not authorized to view image";
            context.ApplicationInstance.CompleteRequest();
        }

        #endregion
    }
}