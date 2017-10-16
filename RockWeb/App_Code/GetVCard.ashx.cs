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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace RockWeb
{
    /// <summary>
    /// Handles retrieving file data from storage
    /// </summary>
    public class GetVCard : IHttpHandler
    {
        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler" /> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        public void ProcessRequest( HttpContext context )
        {
            var rockContext = new RockContext();

            var currentUser = new UserLoginService( rockContext ).GetByUserName( UserLogin.GetCurrentUserName() );
            Person currentPerson = currentUser != null ? currentUser.Person : null;

            Person person = GetPerson( context, rockContext );
            if ( person == null )
            {
                SendNotFound( context );
                return;
            }
            else
            {
                if ( !person.IsAuthorized( Authorization.VIEW, currentPerson ) )
                {
                    SendNotAuthorized( context );
                    return;
                }
            }

            var response = context.Response;
            response.ContentType = "text/vcard";

            // http://stackoverflow.com/questions/93551/how-to-encode-the-filename-parameter-of-content-disposition-header-in-http
            string contentDisposition;
            string fileName = person.FullName + ".vcf";
            if ( context.Request.Browser.Browser == "IE" )
            {
                contentDisposition = "attachment; filename=" + Uri.EscapeDataString( fileName );
            }
            else if ( context.Request.Browser.Browser == "Safari" )
            {
                contentDisposition = "attachment; filename=" + fileName;
            }
            else
            {
                contentDisposition = "attachment; filename*=UTF-8''" + Uri.EscapeDataString( fileName );
            }
            response.AddHeader( "Content-Disposition", contentDisposition );

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, currentPerson );
            mergeFields.Add( "Person", person );
            string vCard = GlobalAttributesCache.Value( "VCardFormat" ).ResolveMergeFields( mergeFields );

            var inputEncoding = Encoding.Default;
            var outputEncoding = Encoding.GetEncoding( 28591 );
            var cardBytes = inputEncoding.GetBytes( vCard );
            var outputBytes = Encoding.Convert( inputEncoding, outputEncoding, cardBytes );

            response.OutputStream.Write( outputBytes, 0, outputBytes.Length );
        }

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private Person GetPerson( HttpContext context, RockContext rockContext )
        {
            string personParam = context.Request.QueryString["Person"];

            int? personId = personParam.AsIntegerOrNull();
            if ( personId.HasValue )
            {
                return new PersonService( rockContext ).Get( personId.Value );
            }

            Guid? personGuid = personParam.AsGuidOrNull();
            if ( personGuid.HasValue )
            {
                return new PersonService( rockContext ).Get( personGuid.Value );
            }

            return null;
        }

        /// <summary>
        /// Sends 404 status.
        /// </summary>
        /// <param name="context">The context.</param>
        private void SendNotFound( HttpContext context )
        {
            context.Response.StatusCode = System.Net.HttpStatusCode.NotFound.ConvertToInt();
            context.Response.StatusDescription = "The requested person could not be found.";
            context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// Sends a 403 (forbidden)
        /// </summary>
        /// <param name="context">The context.</param>
        private void SendNotAuthorized( HttpContext context )
        {
            context.Response.StatusCode = System.Net.HttpStatusCode.Forbidden.ConvertToInt();
            context.Response.StatusDescription = "Not authorized to view person";
            context.ApplicationInstance.CompleteRequest();
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