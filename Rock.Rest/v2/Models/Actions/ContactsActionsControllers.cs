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
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;

using Rock.Common.Mobile.Blocks.Engagement.MyContact;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.ViewModels.Rest.Models;

using Rock.Mobile;
using Rock.Utility;

#if WEBFORMS
using IActionResult = System.Web.Http.IHttpActionResult;
using RoutePrefixAttribute = System.Web.Http.RoutePrefixAttribute;
using RouteAttribute = System.Web.Http.RouteAttribute;
#endif

namespace Rock.Rest.v2.Models.Actions
{
    /// <summary>
    /// Provides action API endpoints for contacts.
    /// </summary>
    [RoutePrefix( "api/v2/models/contacts/actions" )]
    [Rock.SystemGuid.RestControllerGuid( "4463B226-6AA0-4217-87EF-484673A55A94" )]
    public class ContactsActionsController : ApiControllerBase
    {
        /// <summary>
        /// Searches your contacts given the filter option.
        /// </summary>
        /// <param name="option">The filtering option use for searching for your contacts.</param>
        /// <returns>An array of contact that match the search option.</returns>
        [HttpPost]
        [Route( "search/my" )]
        [Authenticate]
        [ExcludeSecurityActions( Security.Authorization.EXECUTE_READ, Security.Authorization.EXECUTE_WRITE, Security.Authorization.EXECUTE_UNRESTRICTED_READ, Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]
        [ProducesResponse( HttpStatusCode.OK, Type = typeof( List<ItemIdentifierBag> ) )]
        [ProducesResponse( HttpStatusCode.BadRequest )]
        [ProducesResponse( HttpStatusCode.NotFound )]
        [ProducesResponse( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "5ba11dbf-6342-4a80-a27d-ae6d893607e3" )]
        public IActionResult PostSearchMy( [FromBody] ContactSearchOptions option )
        {
            using ( var rockContext = new RockContext() )
            {
                // If the person isn't logged in then they can't have
                // anything followed.
                if ( RockRequestContext.CurrentPerson == null )
                {
                    return BadRequest( "Must be logged in." );
                }
                var currentPerson = RockRequestContext.CurrentPerson;
                if ( currentPerson == null )
                {
                    return BadRequest( "You are not logged in" );
                }

                var personAliasId = currentPerson.PrimaryAliasId;
                if ( personAliasId == null )
                {
                    return BadRequest( "The current person doesn't have a primary alias Id" );
                }

                ContactService contactService = new ContactService( rockContext );

                var qry = contactService
                    .Queryable()
                    .Where( c => c.OwnerPersonAliasId == personAliasId );

                if ( option.SearchTerm.IsNotNullOrWhiteSpace() )
                {
                    var searchTerm = option.SearchTerm.ToLower().Trim();
                    qry = qry.Where( c =>
                        ( c.FirstName ?? "" ).ToLower().Contains( searchTerm ) ||
                        ( c.LastName ?? "" ).ToLower().Contains( searchTerm ) ||
                        ( ( ( c.FirstName ?? "" ) + " " + ( c.LastName ?? "" ) ).ToLower().Contains( searchTerm ) )
                    );
                }

                var contacts = qry.OrderByDescending( c => c.Id )
                    .Skip( option.Offset )
                    .Take( option.Limit )
                    .ToList();

                var result = contacts.Select( c => new ContactItem
                {
                    ContactIdKey = c.IdKey,
                    ContactId = c.Id,
                    Name = c.FirstName + " " + c.LastName,
                    ProfilePhotoUrl = c.PhotoId != null ? MobileHelper.BuildPublicApplicationRootUrl( FileUrlHelper.GetImageUrl( c.PhotoId.Value, new GetImageUrlOptions { Width = 256, Height = 256 } ) ) : string.Empty,
                    Email = c.Email,
                    PhoneNumber = c.MobilePhone
                } );

                return Ok( result );
            }
        }
    }
}
