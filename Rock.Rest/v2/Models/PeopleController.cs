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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.ViewModels.Rest.Models;

#if WEBFORMS
using Rock.Web.Cache;

using FromBodyAttribute = System.Web.Http.FromBodyAttribute;
using HttpGetAttribute = System.Web.Http.HttpGetAttribute;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using IActionResult = System.Web.Http.IHttpActionResult;
using RouteAttribute = System.Web.Http.RouteAttribute;
using RoutePrefixAttribute = System.Web.Http.RoutePrefixAttribute;
#else
    using Microsoft.AspNetCore.Mvc;

    using RoutePrefixAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;
#endif

namespace Rock.Rest.v2.Models
{
    public partial class PeopleController : ApiControllerBase
    {
        /// <summary>
        /// Performs a search of people using the provided information. Optionally creates a new person in the database.
        /// </summary>
        /// <param name="value">The person to be found or created.</param>
        /// <param name="createPersonIfMissing">Whether to create a new person if no match is found.</param>
        /// <returns>An object that contains the identifier values or a 404 Not Found response.</returns>
        /// <remarks>
        /// Because this Action can both read and write data, security is handled a little differently than most REST Actions.
        /// If the caller has "Execute Unrestricted Write" authorization, they will be allowed to both find and create People.
        /// If they only have "Execute Unrestricted Read" authorization, they will be allowed to find People but not create them.
        /// </remarks>
        [HttpPost]
        [Route( "" )]
        [Authenticate]
        [Secured( Security.Authorization.EXECUTE_UNRESTRICTED_READ, Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]
        [ExcludeSecurityActions( Security.Authorization.EXECUTE_READ, Security.Authorization.EXECUTE_WRITE )]
        [ProducesResponse( HttpStatusCode.OK, Type = typeof( ItemIdentifierBag ) )]
        [ProducesResponse( HttpStatusCode.Created, Type = typeof( CreatedAtResponseBag ) )]
        [ProducesResponse( HttpStatusCode.BadRequest )]
        [ProducesResponse( HttpStatusCode.NotFound )]
        [ProducesResponse( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "8bb61c8b-4db9-5dc1-ad74-bac74224d556" )]
        public IActionResult PostItem( [FromBody] Rock.Model.Person value, bool createPersonIfMissing = true )
        {
            if ( value == null )
            {
                return BadRequest( "Person to be found or created cannot be null." );
            }

            if ( !value.IsValid )
            {
                var errorMessage = value.ValidationResults.Select( r => r.ErrorMessage ).JoinStrings( ", " );

                return BadRequest( errorMessage );
            }

            var mobilePhone = value.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );

            var personMatchQuery = new PersonService.PersonMatchQuery(
                firstName: value.FirstName,
                lastName: value.LastName,
                email: value.Email,
                mobilePhone: mobilePhone?.Number,
                gender: value.Gender,
                birthDate: value.BirthDate,
                suffixValueId: value.SuffixValueId
            );

            using ( var rockContext = new RockContext() )
            {
                return FindOrCreatePerson( rockContext, personMatchQuery, createPersonIfMissing ? value : null );
            }
        }

        /// <summary>
        /// Performs a search of people using the provided information. Optionally creates a new person in the database.
        /// </summary>
        /// <param name="firstName">The first name of the person to be found or created.</param>
        /// <param name="lastName">The last name of the person to be found or created.</param>
        /// <param name="email">The email address of the person to be found or created.</param>
        /// <param name="mobilePhone">The mobile phone number of the person to be found or created.</param>
        /// <param name="createPersonIfMissing">Whether to create a new person if no match is found.</param>
        /// <returns>An object that contains the identifier values or a 404 Not Found response.</returns>
        /// <remarks>
        /// Because this Action can both read and write data, security is handled a little differently than most REST Actions.
        /// If the caller has "Execute Unrestricted Write" authorization, they will be allowed to both find and create People.
        /// If they only have "Execute Unrestricted Read" authorization, they will be allowed to find People but not create them.
        /// </remarks>
        [HttpGet]
        [Route( "findperson" )]
        [Authenticate]
        [Secured( Security.Authorization.EXECUTE_UNRESTRICTED_READ, Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]
        [ExcludeSecurityActions( Security.Authorization.EXECUTE_READ, Security.Authorization.EXECUTE_WRITE )]
        [ProducesResponse( HttpStatusCode.OK, Type = typeof( ItemIdentifierBag ) )]
        [ProducesResponse( HttpStatusCode.Created, Type = typeof( CreatedAtResponseBag ) )]
        [ProducesResponse( HttpStatusCode.BadRequest )]
        [ProducesResponse( HttpStatusCode.NotFound )]
        [ProducesResponse( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "BF012848-CFB9-4A72-ABDA-DC65D1CC7A73" )]
        public IActionResult FindPerson( string firstName, string lastName, string email = null, string mobilePhone = null, bool createPersonIfMissing = false )
        {
            var personMatchQuery = new PersonService.PersonMatchQuery( firstName, lastName, email, mobilePhone );

            Person personToCreateIfMissing = null;
            if ( createPersonIfMissing )
            {
                personToCreateIfMissing = new Person
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email
                };

                if ( mobilePhone.IsNotNullOrWhiteSpace() )
                {
                    personToCreateIfMissing.PhoneNumbers = new List<PhoneNumber>
                    {
                        new PhoneNumber
                        {
                            Number = mobilePhone,
                            NumberTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() )
                        }
                    };
                }
            }

            using ( var rockContext = new RockContext() )
            {
                return FindOrCreatePerson( rockContext, personMatchQuery, personToCreateIfMissing );
            }
        }

        #region Private Methods

        /// <summary>
        /// Performs a search of people using the provided information. Optionally creates a new person in the database.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="personMatchQuery">The information to use when searching for a matching person.</param>
        /// <param name="personToCreateIfMissing">The information to use when creating a new person if a match cannot be found.</param>
        /// <returns>An object that contains the identifier values or a 404 Not Found response.</returns>
        private IActionResult FindOrCreatePerson( RockContext rockContext, PersonService.PersonMatchQuery personMatchQuery, Person personToCreateIfMissing )
        {
            if ( personMatchQuery.FirstName.IsNullOrWhiteSpace() || personMatchQuery.LastName.IsNullOrWhiteSpace() )
            {
                return BadRequest( "First name and last name are required." );
            }

            // Try to find an existing person matching the provided information.
            var matchedPerson = new PersonService( rockContext ).FindPerson( personMatchQuery, updatePrimaryEmail: false );

            if ( matchedPerson != null )
            {
                var identifierResponse = new ItemIdentifierBag
                {
                    Id = matchedPerson.Id,
                    Guid = matchedPerson.Guid,
                    IdKey = matchedPerson.IdKey
                };

                return Ok( identifierResponse );
            }

            if ( personToCreateIfMissing == null )
            {
                return NotFound( "The person was not found." );
            }

            if ( !IsCurrentPersonAuthorized( Security.Authorization.EXECUTE_UNRESTRICTED_WRITE ) )
            {
                return Unauthorized( "You are not authorized to create this person." );
            }

            // Create a new person using the provided information.
            var createdPerson = personToCreateIfMissing.CloneWithoutIdentity();

            if ( personToCreateIfMissing.PhoneNumbers?.Any() == true )
            {
                createdPerson.PhoneNumbers = new List<PhoneNumber>();

                foreach ( var phoneNumber in personToCreateIfMissing.PhoneNumbers )
                {
                    createdPerson.PhoneNumbers.Add( phoneNumber.CloneWithoutIdentity() );
                }
            }

            PersonService.SaveNewPerson( createdPerson, rockContext );

            var routePrefixAttribute = GetType().GetCustomAttribute<RoutePrefixAttribute>();
            var locationUri = new Uri( $"{routePrefixAttribute.Prefix}/{createdPerson.Id}", UriKind.Relative );

            var rootUrlPath = RockRequestContext.RootUrlPath?.TrimEnd( '/' ) ?? string.Empty;
            var locationPath = locationUri.ToString().TrimStart( '/' );
            var createdAtResponse = new CreatedAtResponseBag
            {
                Id = createdPerson.Id,
                Guid = createdPerson.Guid,
                IdKey = createdPerson.IdKey,
                Location = $"{rootUrlPath}/{locationPath}"
            };

            return Created( locationUri, createdAtResponse );
        }

        #endregion Private Methods
    }
}
