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
using System.Text.RegularExpressions;
using System.Web;
using System.Web.SessionState;
using Rock;
using Rock.Data;
using Rock.Model;

namespace RockWeb
{
    /// <summary>
    /// Generates image thumbnail from video url
    /// </summary>
    public class GetVideoEmbed : IHttpHandler, IRequiresSessionState
    {

        RockContext rockContext;
        BinaryFileTypeService binaryFileTypeService;
        BinaryFileType binaryFileType;

        public bool IsReusable
        {
            get { return false; }
        }

        private HttpRequest request;
        private HttpResponse response;

        public virtual void ProcessRequest( HttpContext context )
        {
            request = context.Request;
            response = context.Response;
            response.ContentType = "text/plain";

            if ( request.HttpMethod != "POST" )
            {
                response.Write( "Invalid request type." );
                response.StatusCode = 406;
                return;
            }
            var output = "";

            var currentUser = UserLoginService.GetCurrentUser();
            Person currentPerson = currentUser != null ? currentUser.Person : null;
            rockContext = new RockContext();
            binaryFileTypeService = new BinaryFileTypeService( rockContext );

            binaryFileType = binaryFileTypeService.Get( Rock.SystemGuid.BinaryFiletype.COMMUNICATION_IMAGE.AsGuid() );
            if ( !binaryFileType.IsAuthorized( "Edit", currentPerson ) )
            {
                response.Write( "Unauthorized." );
                response.StatusCode = 401;
                return;
            }

            try
            {
                var videoUrl = request.Form["video_url"];
                if ( videoUrl.IsNotNullOrWhiteSpace() )
                {
                    foreach ( var embedComponents in Rock.Communication.VideoEmbed.VideoEmbedContainer.Instance.Components )
                    {
                        var component = embedComponents.Value.Value;
                        if ( Regex.IsMatch( videoUrl, component.RegexFilter ) )
                        {
                            output = component.GetThumbnail( videoUrl );
                        }
                    }

                    response.Write( output );
                    response.StatusCode = 200;
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, context );
            }
            if ( string.IsNullOrWhiteSpace( output ) )
            {
                response.StatusCode = 404;
                response.Write( "Not Found" );
            }
        }
    }
}