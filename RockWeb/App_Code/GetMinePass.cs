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
using System.Linq;
using System.Web;
using Rock;
using Rock.Data;
using Rock.Model;
using com.minecartstudio.MinePass.Client.Model;
using System.Data.Entity;

namespace com.minecartstudio.MinePass.Client
{
    /// <summary>
    /// Handles retrieving file data from storage
    /// </summary>
    public class GetMinePass : IHttpHandler
    {

        private HttpRequest _request;
        private HttpResponse _response;

        private const string PASS_TEMPLATE_ID_KEY = "PassTemplateId";
        private const string PERSON_KEY_KEY = "PersonKey"; // guid of a person alias

        public void ProcessRequest( HttpContext context )
        {
            _request = context.Request;
            _response = context.Response;

            var rockContext = new RockContext();

            // Get pass template
            var minePassTemplate = GetMinePassTemplate( rockContext );

            if ( minePassTemplate == null )
            {
                return;
            }

            // Get person
            var person = GetPerson( rockContext );

            if ( person == null )
            {
                return;
            }

            var serialNumber = MinePassUtilities.AddUpdateMinePass( person, minePassTemplate );

            if ( serialNumber.IsNullOrWhiteSpace() )
            {
                _response.Write( "Error Creating Pass." );
                _response.StatusCode = 500;
                return;
            }

            // Redirect to download the pass
            _response.Redirect( MinePassUtilities.GetPassRedirectUrl( serialNumber ) );
        }

        private MinePassTemplate GetMinePassTemplate( RockContext rockContext )
        {
            int? passTemplateId = null;

            // Get template id
            if ( _request.QueryString[PASS_TEMPLATE_ID_KEY] == null )
            {
                _response.Write( "No Pass Template Provided." );
                _response.StatusCode = 500;
                return null;
            }

            passTemplateId = _request.QueryString[PASS_TEMPLATE_ID_KEY].AsIntegerOrNull();

            if ( !passTemplateId.HasValue )
            {
                _response.Write( "Invalid Pass Template Provided." );
                _response.StatusCode = 500;
                return null;
            }

            var minePassTemplate = new MinePassTemplateService( rockContext ).Queryable().AsNoTracking()
                                    .Where( t => t.Id == passTemplateId.Value )
                                    .FirstOrDefault();

            if ( minePassTemplate == null )
            {
                _response.Write( "Pass Does Not Exist." );
                _response.StatusCode = 404;
                return null;
            }

            return minePassTemplate;
        }

        private Person GetPerson( RockContext rockContext )
        {
            Guid? personKey = null;

            if ( _request.QueryString[PERSON_KEY_KEY] == null )
            {
                _response.Write( "No Person Key (PersonAlias Guid) Provided." );
                _response.StatusCode = 500;
                return null;
            }

            personKey = _request.QueryString[PERSON_KEY_KEY].AsGuidOrNull();

            if ( !personKey.HasValue )
            {
                _response.Write( "Invalid Person Key (PersonAlias Guid) Provided." );
                _response.StatusCode = 500;
                return null;
            }

            var person = new PersonAliasService( rockContext ).Queryable().AsNoTracking()
                            .Where( p => p.Guid == personKey )
                            .Select( p => p.Person )
                            .FirstOrDefault();

            if ( person == null )
            {
                _response.Write( "Person Not Found." );
                _response.StatusCode = 404;
                return null;
            }

            return person;
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