﻿// <copyright>
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
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using System.Web.Http.OData;

using Rock.BulkExport;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    ///
    /// </summary>
    public partial class PeopleController
    {
        #region Get

        /// <summary>
        /// GET endpoint to get a single person record
        /// </summary>
        /// <param name="id">The Id of the record</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [Authenticate, Secured]
        [ActionName( "GetById" )]
        public override Person GetById( int id )
        {
            // NOTE: We want PrimaryAliasId to be populated, so call this.Get( true ) which includes "Aliases"
            var person = this.Get( true ).Include( a => a.PhoneNumbers ).FirstOrDefault( a => a.Id == id );
            if ( person == null )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            return person;
        }

        /// <summary>
        /// GET endpoint to get a single person record
        /// </summary>
        /// <param name="key">The Id of the record</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [EnableQuery]
        public override Person Get( [FromODataUri] int key )
        {
            // NOTE: We want PrimaryAliasId to be populated, so call this.GetById( key ) which includes "Aliases"
            return this.GetById( key );
        }

        /// <summary>
        /// Queryable GET endpoint. Note that records that are marked as Deceased are not included
        /// </summary>
        /// <returns></returns>
        [Authenticate, Secured]
        [EnableQuery]
        public override IQueryable<Person> Get()
        {
            // NOTE: We want PrimaryAliasId to be populated, so include Aliases
            return base.Get().Include( a => a.Aliases );
        }

        /// <summary>
        /// Queryable GET endpoint with an option to include person records that have been marked as Deceased
        /// </summary>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [EnableQuery]
        [Rock.SystemGuid.RestActionGuid( "2A3BE8FB-0A64-4096-9AFA-D11AEB6E169D" )]
        public IQueryable<Person> Get( bool includeDeceased )
        {
            var rockContext = this.Service.Context as RockContext;

            // NOTE: We want PrimaryAliasId to be populated, so include "Aliases"
            return new PersonService( rockContext ).Queryable( includeDeceased ).Include( a => a.Aliases );
        }

        /// <summary>
        /// Gets the currently authenticated person
        /// </summary>
        /// <returns>A person</returns>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/GetCurrentPerson" )]
        [Rock.SystemGuid.RestActionGuid( "D1F55DCD-AE00-4C82-B35A-F4C59496D3E8" )]
        public Person GetCurrentPerson()
        {
            var rockContext = new Rock.Data.RockContext();
            var person = GetPerson();
            if ( person == null )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            return new PersonService( rockContext ).Get( person.Id );
        }

        /// <summary>
        /// Searches the person records based on the specified email address
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/GetByEmail/{email}" )]
        [System.Web.Http.Route( "api/People/GetByEmail" )]
        [Rock.SystemGuid.RestActionGuid( "A6D9B02B-814C-4A92-9D6A-723B168CFABB" )]
        public IQueryable<Person> GetByEmail( string email )
        {
            var rockContext = new Rock.Data.RockContext();
            return new PersonService( rockContext ).GetByEmail( email, true ).Include( a => a.Aliases );
        }

        /// <summary>
        /// Searches the person records based on the specified phone number. NOTE that partial matches are included
        /// </summary>
        /// <param name="number">The phone number.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/GetByPhoneNumber/{number}" )]
        [Rock.SystemGuid.RestActionGuid( "4470749A-9F47-46AB-B89E-ADABE9517A2A" )]
        public IQueryable<Person> GetByPhoneNumber( string number )
        {
            var rockContext = new Rock.Data.RockContext();
            return new PersonService( rockContext ).GetByPhonePartial( number, true ).Include( a => a.Aliases );
        }

        /// <summary>
        /// GET a person record based on a temporary person token and increment the usage count of the token
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/GetByToken/{token}" )]
        [Rock.SystemGuid.RestActionGuid( "22AC8710-A6C1-4E1D-8C36-D4FF8DC0C7FD" )]
        public Person GetByToken( string token )
        {
            if ( token.IsNullOrWhiteSpace() )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            var personService = Service as PersonService;
            var person = personService.GetByImpersonationToken( token, true, null );

            if ( person == null )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            return person;
        }

        /// <summary>
        /// GET a person record based on the specified username
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/GetByUserName/{username}" )]
        [Rock.SystemGuid.RestActionGuid( "C482EF4E-2A9A-47C7-82A0-65EB974C6275" )]
        public Person GetByUserName( string username )
        {
            int? personId = new UserLoginService( ( Rock.Data.RockContext ) Service.Context ).Queryable()
                .Where( u => u.UserName.Equals( username ) )
                .Select( a => a.PersonId )
                .FirstOrDefault();

            if ( personId != null )
            {
                return Service.Queryable().Include( a => a.PhoneNumbers ).Include( a => a.Aliases )
                    .FirstOrDefault( p => p.Id == personId.Value );
            }

            throw new HttpResponseException( System.Net.HttpStatusCode.NotFound );
        }

        /// <summary>
        /// Gets a list of people's names, email, gender and birthdate, to see if there are potential duplicates.
        /// For example, you might want to use this during account creation to warn that the person might already have an account.
        /// </summary>
        /// <param name="lastName">The last name.</param>
        /// <param name="emailAddress">The email address.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/GetPotentialDuplicates" )]
        [Rock.SystemGuid.RestActionGuid( "4136C8EB-2295-4A0C-B184-7F45E08CF655" )]
        public IEnumerable<DuplicatePersonInfo> GetPotentialDuplicates( string lastName, string emailAddress )
        {
            // return a limited number of fields so that this endpoint could be made available to a wider audience
            return Get().Where( a => a.Email == emailAddress && a.LastName == lastName ).ToList().Select( a => new DuplicatePersonInfo
            {
                Id = a.Id,
                Name = a.FullName,
                Email = a.Email,
                Gender = a.Gender,
                BirthDay = a.BirthDay,
                BirthMonth = a.BirthMonth,
                BirthYear = a.BirthYear
            } );
        }

        /// <summary>
        ///
        /// </summary>
        public class DuplicatePersonInfo
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the email.
            /// </summary>
            /// <value>
            /// The email.
            /// </value>
            public string Email { get; set; }

            /// <summary>
            /// Gets or sets the gender.
            /// </summary>
            /// <value>
            /// The gender.
            /// </value>
            public Gender Gender { get; set; }

            /// <summary>
            /// Gets or sets the birth month.
            /// </summary>
            /// <value>
            /// The birth month.
            /// </value>
            public int? BirthMonth { get; set; }

            /// <summary>
            /// Gets or sets the birth day.
            /// </summary>
            /// <value>
            /// The birth day.
            /// </value>
            public int? BirthDay { get; set; }

            /// <summary>
            /// Gets or sets the birth year.
            /// </summary>
            /// <value>
            /// The birth year.
            /// </value>
            public int? BirthYear { get; set; }
        }

        /// <summary>
        /// GET the Person by person alias identifier.
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/GetByPersonAliasId/{personAliasId}" )]
        [Rock.SystemGuid.RestActionGuid( "DD2D2F23-F674-4FB3-8B6C-03AD7032DF30" )]
        public Person GetByPersonAliasId( int personAliasId )
        {
            int? personId = new PersonAliasService( ( Rock.Data.RockContext ) Service.Context ).Queryable()
                .Where( u => u.Id.Equals( personAliasId ) ).Select( a => a.PersonId ).FirstOrDefault();
            if ( personId != null )
            {
                return this.Get( personId.Value );
            }

            throw new HttpResponseException( System.Net.HttpStatusCode.NotFound );
        }

        /// <summary>
        /// Gets the graduation year based on the provided GradeOffset
        /// </summary>
        /// <param name="gradeOffset">The grade offset for the person.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/GetGraduationYear/{gradeOffset}" )]
        [Rock.SystemGuid.RestActionGuid( "08D951D2-F731-452B-A3CD-C612C530D4FA" )]
        public int GetGraduationYear( int gradeOffset )
        {
            int? graduationYear = Person.GraduationYearFromGradeOffset( gradeOffset );
            if ( graduationYear.HasValue )
            {
                return graduationYear.Value;
            }

            throw new HttpResponseException( System.Net.HttpStatusCode.NotFound );
        }

        /// <summary>
        /// Gets the count of interactions over several timeframes for the current or specified person.
        /// </summary>
        /// <param name="date">The date. Optional. This defaults to today.</param>
        /// <param name="personId">The person identifier. Optional. This defaults to the currently authenticated person.</param>
        /// <param name="interactionChannelId">The interaction channel identifier. Optional filter.</param>
        /// <param name="interactionComponentId">The interaction component identifier. Optional filter.</param>
        /// <param name="interactionChannelGuid">The interaction channel unique identifier. Optional filter.</param>
        /// <param name="interactionComponentGuid">The interaction component unique identifier. Optional filter.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/GetInteractionStatistics/{personId?}" )]
        [Rock.SystemGuid.RestActionGuid( "BC12A776-BFE9-487D-AEBA-1099D2DB5C6B" )]
        public virtual PersonInteractionStatistics InteractionStatistics(
            int? personId = null,
            [FromUri] DateTime? date = null,
            [FromUri] int? interactionChannelId = null,
            [FromUri] int? interactionComponentId = null,
            [FromUri] Guid? interactionChannelGuid = null,
            [FromUri] Guid? interactionComponentGuid = null )
        {
            var rockContext = new RockContext();

            // Default to the current person if the person id was not specified
            if ( !personId.HasValue )
            {
                personId = GetPerson( rockContext )?.Id;

                if ( !personId.HasValue )
                {
                    var errorMessage = "The personId for the current user did not resolve";
                    var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, errorMessage );
                    throw new HttpResponseException( errorResponse );
                }
            }

            // Default the date to today if the date was not specified
            if ( !date.HasValue )
            {
                date = RockDateTime.Today;
            }
            else
            {
                date = date.Value.Date;
            }

            // Build the initial query for interactions
            var interactionsService = new InteractionService( rockContext );
            var query = interactionsService.Queryable().AsNoTracking().Where( i => i.PersonAlias.PersonId == personId );

            // Filter by the channel guid if set
            if ( interactionChannelGuid.HasValue )
            {
                query = query.Where( i => i.InteractionComponent.InteractionChannel.Guid == interactionChannelGuid.Value );
            }

            // Filter by the channel id if set
            if ( interactionChannelId.HasValue )
            {
                query = query.Where( i => i.InteractionComponent.InteractionChannel.Id == interactionChannelId.Value );
            }

            // Filter by the component guid if set
            if ( interactionComponentGuid.HasValue )
            {
                query = query.Where( i => i.InteractionComponent.Guid == interactionComponentGuid.Value );
            }

            // Filter by the component id if set
            if ( interactionComponentId.HasValue )
            {
                query = query.Where( i => i.InteractionComponentId == interactionComponentId.Value );
            }

            // Read the results from the database. The intent here is to make one database call to get all of the counts rather than 4 calls.
            // https://stackoverflow.com/a/8895028
            var personInteractionStatistics = (
                from interaction in query
                group interaction by 1 into interactions
                select new PersonInteractionStatistics
                {
                    InteractionsAllTime = interactions.Count(),
                    InteractionsThatDay = interactions.Count( i => DbFunctions.TruncateTime( i.InteractionDateTime ) == date.Value ),
                    InteractionsThatYear = interactions.Count( i => i.InteractionDateTime.Year == date.Value.Year ),
                    InteractionsThatMonth = interactions.Count( i =>
                        i.InteractionDateTime.Month == date.Value.Month &&
                        i.InteractionDateTime.Year == date.Value.Year )
                }

            ).FirstOrDefault();

            return personInteractionStatistics ?? new PersonInteractionStatistics();
        }

        #endregion Get

        #region Post

        /// <summary>
        /// Adds a new person and puts them into a new family
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public override System.Net.Http.HttpResponseMessage Post( Person person )
        {
            SetProxyCreation( true );

            CheckCanEdit( person );

            if ( !person.IsValid )
            {
                return ControllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    string.Join( ",", person.ValidationResults.Select( r => r.ErrorMessage ).ToArray() ) );
            }

            System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", GetPerson() );

            var rockContext = ( Rock.Data.RockContext ) Service.Context;

            var matchPerson = new PersonService( rockContext ).FindPerson( new PersonService.PersonMatchQuery( person.FirstName, person.LastName, person.Email, null, person.Gender, person.BirthDate, person.SuffixValueId ), false );

            if ( matchPerson != null )
            {
                return ControllerContext.Request.CreateResponse( HttpStatusCode.OK, matchPerson.Id );
            }

            PersonService.SaveNewPerson( person, rockContext, null, false );

            return ControllerContext.Request.CreateResponse( HttpStatusCode.Created, person.Id );
        }

        /// <summary>
        /// Adds a new person and adds them to the specified family.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="familyId">The family identifier.</param>
        /// <param name="groupRoleId">The group role identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/People/AddNewPersonToFamily/{familyId}" )]
        [Rock.SystemGuid.RestActionGuid( "5280CFDB-C02E-44AD-822D-53472D004EB6" )]
        public System.Net.Http.HttpResponseMessage AddNewPersonToFamily( Person person, int familyId, int groupRoleId )
        {
            SetProxyCreation( true );

            CheckCanEdit( person );

            if ( !person.IsValid )
            {
                return ControllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    string.Join( ",", person.ValidationResults.Select( r => r.ErrorMessage ).ToArray() ) );
            }

            System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", GetPerson() );

            PersonService.AddPersonToFamily( person, person.Id == 0, familyId, groupRoleId, ( Rock.Data.RockContext ) Service.Context );

            return ControllerContext.Request.CreateResponse( HttpStatusCode.Created, person.Id );
        }

        /// <summary>
        /// Adds the existing person to family, optionally removing them from any other families they belong to
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="familyId">The family identifier.</param>
        /// <param name="groupRoleId">The group role identifier.</param>
        /// <param name="removeFromOtherFamilies">if set to <c>true</c> [remove from other families].</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/People/AddExistingPersonToFamily" )]
        [Rock.SystemGuid.RestActionGuid( "307B6DA5-9D19-4E0A-AE76-92C9BA0F740B" )]
        public System.Net.Http.HttpResponseMessage AddExistingPersonToFamily( int personId, int familyId, int groupRoleId, bool removeFromOtherFamilies )
        {
            SetProxyCreation( true );

            System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", GetPerson() );
            var person = this.Get( personId );
            CheckCanEdit( person );

            PersonService.AddPersonToFamily( person, false, familyId, groupRoleId, ( Rock.Data.RockContext ) Service.Context );

            if ( removeFromOtherFamilies )
            {
                PersonService.RemovePersonFromOtherFamilies( familyId, personId, ( Rock.Data.RockContext ) Service.Context );
            }

            return ControllerContext.Request.CreateResponse( HttpStatusCode.Created, person.Id );
        }

        /// <summary>
        /// Allows setting a configuration for the person for text-to-give.
        /// </summary>
        /// <param name="personId">The person to configure text-to-give options</param>
        /// <param name="args">The options to set</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/People/ConfigureTextToGive/{personId}" )]
        [Rock.SystemGuid.RestActionGuid( "3AB8A7BF-A614-46A8-A6DD-2FF574F4D79E" )]
        public HttpResponseMessage ConfigureTextToGive( int personId, [FromBody] ConfigureTextToGiveArgs args )
        {
            var personService = Service as PersonService;
            var success = personService.ConfigureTextToGive( personId, args.ContributionFinancialAccountId, args.FinancialPersonSavedAccountId, out var errorMessage );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                return ControllerContext.Request.CreateResponse( HttpStatusCode.BadRequest, errorMessage );
            }

            if ( !success )
            {
                return ControllerContext.Request.CreateResponse( HttpStatusCode.InternalServerError, "The action was not successful but not error was specified" );
            }

            Service.Context.SaveChanges();
            return ControllerContext.Request.CreateResponse( HttpStatusCode.OK );
        }

        /// <summary>
        /// Unsubscribes a person from all email communications or a specific communication list if one is provided.
        /// </summary>
        /// <param name="personActionIdentifier">The person action identifier. Action should be "Unsubscribe".</param>
        /// <param name="communicationListIdKey">The communication list identifier key.</param>
        /// <remarks>
        /// <para>This endpoint is for email client one-click unsubscribe functionality (<a href="https://datatracker.ietf.org/doc/html/rfc8058">RFC8058</a>).</para>
        /// <para>As of 12/13/2023, <a href="https://datatracker.ietf.org/doc/html/rfc8058#section-3.1">RFC8058 Section 3.1</a> states:</para>
        /// <para>
        /// The POST request MUST NOT include cookies, HTTP authorization, or any
        /// other context information. The unsubscribe operation is logically
        /// unrelated to any previous web activity, and context information could
        /// inappropriately link the unsubscribe to previous activity.
        /// </para>
        /// <para>For this reason, this endpoint does not and must not require authentication or authorization other than the encrypted person action identifier.</para>
        /// </remarks>
        [HttpPost]
        [System.Web.Http.Route( "api/People/OneClickUnsubscribe/{personActionIdentifier}" )]
        [Rock.SystemGuid.RestActionGuid( "D9B2C190-B881-4691-8941-079F47CE0E2F" )]
        public HttpResponseMessage OneClickUnsubscribe( string personActionIdentifier, string communicationListIdKey = null )
        {
            // The service for this controller should be an instance of the PersonService,
            // but create one if it is not.
            var rockContext = ( RockContext ) this.Service.Context;
            if ( !( this.Service is PersonService personService ) )
            {
                personService = new PersonService( rockContext );
            }

            // Enable lazy loading.
            SetProxyCreation( true );

            // Get the person from the action identifier, ensuring "Unsubscribe" is the action.
            var person = personService.GetByPersonActionIdentifier( personActionIdentifier, "Unsubscribe" );
            if ( person == null )
            {
                return new HttpResponseMessage( HttpStatusCode.BadRequest )
                {
                    Content = new StringContent( "Invalid person" )
                };
            }

            if ( communicationListIdKey.IsNotNullOrWhiteSpace() )
            {
                // Unsubscribe from email communication list.
                var communicationListQuery = new GroupService( rockContext )
                    .GetQueryableByKey( communicationListIdKey )
                    .IsCommunicationList();

                // Execute the query once to determine if a valid communication list ID key was provided.
                if ( !communicationListQuery.Any() )
                {
                    return new HttpResponseMessage( HttpStatusCode.BadRequest )
                    {
                        Content = new StringContent( "Invalid communication list" )
                    };
                }

                personService.UnsubscribeFromEmail( person, communicationListQuery );
            }
            else
            {
                personService.OneClickUnsubscribeFromEmail( person );
            }

            rockContext.SaveChanges();

            return new HttpResponseMessage( HttpStatusCode.NoContent );
        }

        /// <summary>
        /// Updates the profile photo of the logged in person.
        /// </summary>
        /// <param name="photoBytes">The photo bytes.</param>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        [Authenticate]
        [System.Web.Http.Route( "api/People/UpdateProfilePhoto" )]
        [HttpPost]
        [Rock.SystemGuid.RestActionGuid( "DA70741A-30DF-4E63-AD26-0444E2E10689" )]
        public IHttpActionResult UpdateProfilePhoto( [NakedBody] byte[] photoBytes, string filename )
        {
            var personGuid = GetPerson()?.Guid;

            if ( !personGuid.HasValue )
            {
                return NotFound();
            }

            return Ok( PersonService.UpdatePersonProfilePhoto( personGuid.Value, photoBytes, filename ) );
        }

        /// <summary>
        /// Updates the person profile photo.
        /// </summary>
        /// <param name="photoBytes">The photo bytes.</param>
        /// <param name="personGuid">The person unique identifier.</param>
        /// <param name="filename">The filename.</param>
        /// <returns>IHttpActionResult.</returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/People/UpdatePersonProfilePhoto" )]
        [HttpPost]
        [Rock.SystemGuid.RestActionGuid( "7AB0E53E-28BD-4EE6-AD31-EBAEC23B123C" )]
        public IHttpActionResult UpdatePersonProfilePhoto( [NakedBody] byte[] photoBytes, Guid personGuid, string filename )
        {
            return Ok( PersonService.UpdatePersonProfilePhoto( personGuid, photoBytes, filename ) );
        }

        /// <summary>
        /// Saves the currently logged in <see cref="Rock.Model.Person">person's</see> user preference.
        /// Note: If the user preference is for a specific block, use ~/api/People/SetBlockUserPreference instead.
        /// </summary>
        /// <param name="userPreferenceKey">The user preference key.</param>
        /// <param name="value">The value.</param>
        [Authenticate]
        [System.Web.Http.Route( "api/People/SetUserPreference" )]
        [HttpPost]
        [Rock.SystemGuid.RestActionGuid( "E6ED42BF-701C-4C06-822D-ED9FBA2F2E5F" )]
        [RockObsolete( "1.16" )]
        [Obsolete( "Use the new PersonPreference endpoints in the v2 API." )]
        public void SetUserPreference( string userPreferenceKey, string value )
        {
            PersonPreferenceCollection preferences;

            if ( RockRequestContext.CurrentVisitorId.HasValue )
            {
                preferences = PersonPreferenceCache.GetVisitorPreferenceCollection( RockRequestContext.CurrentVisitorId.Value );
            }
            else if ( RockRequestContext.CurrentPerson != null )
            {
                preferences = PersonPreferenceCache.GetPersonPreferenceCollection( RockRequestContext.CurrentPerson );
            }
            else
            {
                return;
            }

            preferences.SetValue( userPreferenceKey, value );
            preferences.Save();
        }

        /// <summary>
        /// Saves the currently logged in <see cref="Rock.Model.Person">person's</see> user preference for the specified block
        /// </summary>
        /// <param name="blockId">The block identifier.</param>
        /// <param name="userPreferenceKey">The user preference key.</param>
        /// <param name="value">The value.</param>
        [Authenticate]
        [System.Web.Http.Route( "api/People/SetBlockUserPreference" )]
        [HttpPost]
        [Rock.SystemGuid.RestActionGuid( "B7380EB9-81E5-4ED0-8488-EBEE04991902" )]
        [RockObsolete( "1.16" )]
        [Obsolete( "Use the new PersonPreference endpoints in the v2 API." )]
        public void SetBlockUserPreference( int blockId, string userPreferenceKey, string value )
        {
            PersonPreferenceCollection preferences;
            var blockEntityTypeCache = EntityTypeCache.Get<Block>();

            if ( RockRequestContext.CurrentVisitorId.HasValue )
            {
                preferences = PersonPreferenceCache.GetVisitorPreferenceCollection( RockRequestContext.CurrentVisitorId.Value, blockEntityTypeCache, blockId );
            }
            else if ( RockRequestContext.CurrentPerson != null )
            {
                preferences = PersonPreferenceCache.GetPersonPreferenceCollection( RockRequestContext.CurrentPerson, blockEntityTypeCache, blockId );
            }
            else
            {
                return;
            }

            preferences.SetValue( userPreferenceKey, value );
            preferences.Save();
        }

        /// <summary>
        /// Saves the currently logged in <see cref="Rock.Model.Person">person's</see> user preference for the specified block
        /// </summary>
        /// <param name="blockGuid">The block identifier.</param>
        /// <param name="userPreferenceKey">The user preference key.</param>
        /// <param name="value">The value.</param>
        [Authenticate]
        [System.Web.Http.Route( "api/People/SetBlockUserPreference/{blockGuid}" )]
        [HttpPost]
        [Rock.SystemGuid.RestActionGuid( "223827C2-3731-4C3F-A3F0-C8CCAF8BECE6" )]
        [RockObsolete( "1.16" )]
        [Obsolete( "Use the new PersonPreference endpoints in the v2 API." )]
        public IHttpActionResult SetBlockUserPreference( Guid blockGuid, string userPreferenceKey, string value )
        {
            var blockId = BlockCache.Get( blockGuid )?.Id;

            if ( blockId == null )
            {
                return BadRequest( "Unable to find the specific block." );
            }

            SetBlockUserPreference( blockId.Value, userPreferenceKey, value );
            return Ok();
        }

        /// <summary>
        /// Gets the currently logged in <see cref="Rock.Model.Person">person's</see> user preference.
        /// Note: If the user preference is for a specific block, use ~/api/People/GetBlockUserPreference instead.
        /// </summary>
        /// <param name="userPreferenceKey">The user preference key.</param>
        /// <returns></returns>
        [Authenticate]
        [System.Web.Http.Route( "api/People/GetUserPreference" )]
        [HttpGet]
        [Rock.SystemGuid.RestActionGuid( "E3A05482-ADAF-46DF-9047-B95B8950EBCE" )]
        [RockObsolete( "1.16" )]
        [Obsolete( "Use the new PersonPreference endpoints in the v2 API." )]
        public string GetUserPreference( string userPreferenceKey )
        {
            PersonPreferenceCollection preferences;

            if ( RockRequestContext.CurrentVisitorId.HasValue )
            {
                preferences = PersonPreferenceCache.GetVisitorPreferenceCollection( RockRequestContext.CurrentVisitorId.Value );
            }
            else if ( RockRequestContext.CurrentPerson != null )
            {
                preferences = PersonPreferenceCache.GetPersonPreferenceCollection( RockRequestContext.CurrentPerson );
            }
            else
            {
                return string.Empty;
            }

            return preferences.GetValue( userPreferenceKey );
        }

        /// <summary>
        /// Gets the currently logged in <see cref="Rock.Model.Person">person's</see> user preference for the specified block
        /// </summary>
        /// <param name="blockId">The block identifier.</param>
        /// <param name="userPreferenceKey">The user preference key.</param>
        /// <returns></returns>
        [Authenticate]
        [System.Web.Http.Route( "api/People/GetBlockUserPreference" )]
        [HttpGet]
        [Rock.SystemGuid.RestActionGuid( "66B32878-DED4-4847-8FA6-21FFD51E4094" )]
        [RockObsolete( "1.16" )]
        [Obsolete( "Use the new PersonPreference endpoints in the v2 API." )]
        public string GetBlockUserPreference( int blockId, string userPreferenceKey )
        {
            PersonPreferenceCollection preferences;
            var blockEntityTypeCache = EntityTypeCache.Get<Block>();

            if ( RockRequestContext.CurrentVisitorId.HasValue )
            {
                preferences = PersonPreferenceCache.GetVisitorPreferenceCollection( RockRequestContext.CurrentVisitorId.Value, blockEntityTypeCache, blockId );
            }
            else if ( RockRequestContext.CurrentPerson != null )
            {
                preferences = PersonPreferenceCache.GetPersonPreferenceCollection( RockRequestContext.CurrentPerson, blockEntityTypeCache, blockId );
            }
            else
            {
                return string.Empty;
            }

            return preferences.GetValue( userPreferenceKey );
        }

        /// <summary>
        /// Gets the currently logged in <see cref="Rock.Model.Person">person's</see> user preference for the specified block
        /// </summary>
        /// <param name="blockGuid">The block identifier.</param>
        /// <param name="userPreferenceKey">The user preference key.</param>
        /// <returns></returns>
        [Authenticate]
        [System.Web.Http.Route( "api/People/GetBlockUserPreference/{blockGuid}" )]
        [HttpGet]
        [Rock.SystemGuid.RestActionGuid( "B6AB08EF-2962-48EA-87F5-30153BCC35CC" )]
        [RockObsolete( "1.16" )]
        [Obsolete( "Use the new PersonPreference endpoints in the v2 API." )]
        public string GetBlockUserPreference( Guid blockGuid, string userPreferenceKey )
        {
            var blockId = BlockCache.Get( blockGuid )?.Id;
            if ( blockId == null )
            {
                return string.Empty;
            }

            return GetBlockUserPreference( blockId.Value, userPreferenceKey );
        }

        #endregion

        #region Search

        /// <summary>
        /// Returns results to the Person Picker
        /// </summary>
        /// <param name="name">The search parameter for the person's name.</param>
        /// <param name="includeDetails">Set to <c>true</c> details will be included instead of lazy loaded.</param>
        /// <param name="includeBusinesses">Set to <c>true</c> to also search businesses.</param>
        /// <param name="includeDeceased">Set to <c>true</c> to include deceased people.</param>
        /// <param name="address">The search parameter for the person's address.</param>
        /// <param name="phone">The search parameter for the person's phone.</param>
        /// <param name="email">The search parameter for the person's name email.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/Search" )]
        [Rock.SystemGuid.RestActionGuid( "D9FC468F-03DF-4ABC-844F-DD3EB40E2B6B" )]
        public IQueryable<PersonSearchResult> Search(
            string name = null,
            bool includeDetails = false,
            bool includeBusinesses = false,
            bool includeDeceased = false,
            string address = null,
            string phone = null,
            string email = null )
        {
            // Enable Proxy Creation so that LazyLoading will work.
            SetProxyCreation( true );
            return SearchForPeople( Service.Context as RockContext, name, address, phone, email, includeDetails, includeBusinesses, includeDeceased, true );
        }

        /// <summary>
        /// Returns results of a person search that can be used in things like Person Picker.
        /// </summary>
        /// <param name="rockContext">The rock database context.</param>
        /// <param name="name">The search parameter for the person's name.</param>
        /// <param name="includeDetails">Set to <c>true</c> details will be included instead of lazy loaded.</param>
        /// <param name="includeBusinesses">Set to <c>true</c> to also search businesses.</param>
        /// <param name="includeDeceased">Set to <c>true</c> to include deceased people.</param>
        /// <param name="address">The search parameter for the person's address.</param>
        /// <param name="phone">The search parameter for the person's phone.</param>
        /// <param name="email">The search parameter for the person's name email.</param>
        /// <param name="includeHtml"><c>true</c> if the results should include the pre-formatted HTML values.</param>
        /// <returns></returns>
        internal static IQueryable<PersonSearchResult> SearchForPeople(
            RockContext rockContext,
            string name,
            string address,
            string phone,
            string email,
            bool includeDetails,
            bool includeBusinesses,
            bool includeDeceased,
            bool includeHtml )
        {
            if ( name.IsNullOrWhiteSpace() && address.IsNullOrWhiteSpace() && phone.IsNullOrWhiteSpace() && email.IsNullOrWhiteSpace() )
            {
                // no search terms specified
                return null;
            }

            int count = GlobalAttributesCache.Value( "core.PersonPickerFetchCount" ).AsIntegerOrNull() ?? 60;
            bool sortbyFullNameReversed = false;
            bool allowFirstNameOnly = false;

            var searchComponent = Rock.Search.SearchContainer.GetComponent( typeof( Rock.Search.Person.Name ).FullName );
            if ( searchComponent != null )
            {
                allowFirstNameOnly = searchComponent.GetAttributeValue( "FirstNameSearch" ).AsBoolean();
            }

            var activeRecordStatusValue = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() );
            int activeRecordStatusValueId = activeRecordStatusValue != null ? activeRecordStatusValue.Id : 0;

            var personService = new PersonService( rockContext );

            var personSearchOptions = new PersonService.PersonSearchOptions
            {
                Name = name,
                Address = address,
                Phone = phone,
                Email = email,
                AllowFirstNameOnly = allowFirstNameOnly,
                IncludeBusinesses = includeBusinesses,
                IncludeDeceased = includeDeceased
            };

            IQueryable<Person> personSearchQry = personService.Search( personSearchOptions );

            // limit to count
            personSearchQry = personSearchQry.Take( count );

            // make sure we don't use EF ChangeTracking
            personSearchQry = personSearchQry.AsNoTracking();

            if ( includeDetails == false )
            {
                var simpleResult = personSearchQry.ToList().Select( a =>
                {
                    var spouse = personService.GetSpouse( a );

                    return new PersonSearchResult
                    {
                        Id = a.Id,
                        IdKey = a.IdKey,
                        Name = sortbyFullNameReversed
                            ? Person.FormatFullNameReversed( a.LastName, a.NickName, a.SuffixValueId, a.RecordTypeValueId )
                            : Person.FormatFullName( a.NickName, a.LastName, a.SuffixValueId, a.RecordTypeValueId ),
                        IsActive = a.RecordStatusValueId.HasValue && a.RecordStatusValueId == activeRecordStatusValueId,
                        IsDeceased = a.IsDeceased,
                        RecordStatus = a.RecordStatusValueId.HasValue ? DefinedValueCache.Get( a.RecordStatusValueId.Value ).Value : string.Empty,
                        Age = Person.GetAge( a.BirthDate, a.DeceasedDate ) ?? -1,
                        FormattedAge = a.FormatAge(),
                        SpouseNickName = spouse?.NickName,
                        SpouseName = spouse != null
                            ? Person.FormatFullName( spouse.NickName, spouse.LastName, spouse.SuffixValueId )
                            : null,
                    };
                } );

                return simpleResult.AsQueryable();
            }
            else
            {
                List<PersonSearchResult> searchResult = SearchWithDetails( rockContext, personSearchQry, sortbyFullNameReversed, includeHtml );
                return searchResult.AsQueryable();
            }
        }

        /// <summary>
        /// Gets the search details (for the person picker)
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/GetSearchDetails" )]
        [Rock.SystemGuid.RestActionGuid( "55A6B73A-3F29-4CCB-A227-3B77530F4B12" )]
        public string GetSearchDetails( int id )
        {
            SetProxyCreation( true );
            PersonSearchResult personSearchResult = new PersonSearchResult();

            var person = this.Get()
                .Where( a => a.Id == id )
                .FirstOrDefault();

            if ( person != null )
            {
                GetPersonSearchDetails( Service.Context as RockContext, personSearchResult, person, true );
                return personSearchResult.SearchDetailsHtml;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns a List of PersonSearchRecord based on the sorted person query
        /// </summary>
        /// <param name="rockContext">The rock database context.</param>
        /// <param name="sortedPersonQry">The sorted person qry.</param>
        /// <param name="showFullNameReversed">if set to <c>true</c> [show full name reversed].</param>
        /// <param name="includeHtml"><c>true</c> if the results should include the pre-formatted HTML values.</param>
        /// <returns></returns>
        private static List<PersonSearchResult> SearchWithDetails( RockContext rockContext, IQueryable<Person> sortedPersonQry, bool showFullNameReversed, bool includeHtml )
        {
            var phoneNumbersQry = new PhoneNumberService( rockContext ).Queryable();

            var sortedPersonList = sortedPersonQry
                .AsNoTracking()
                .ToList();

            Guid activeRecord = new Guid( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE );

            List<PersonSearchResult> searchResult = new List<PersonSearchResult>();
            foreach ( var person in sortedPersonList )
            {
                PersonSearchResult personSearchResult = new PersonSearchResult();
                personSearchResult.Id = person.Id;
                personSearchResult.IdKey = person.IdKey;
                personSearchResult.Guid = person.Guid;
                personSearchResult.PrimaryAliasGuid = person.PrimaryAlias.Guid;
                personSearchResult.Name = showFullNameReversed ? person.FullNameReversed : person.FullName;
                if ( person.RecordStatusValueId.HasValue )
                {
                    var recordStatus = DefinedValueCache.Get( person.RecordStatusValueId.Value );
                    personSearchResult.RecordStatus = recordStatus.Value;
                    personSearchResult.IsActive = recordStatus.Guid.Equals( activeRecord );
                }
                else
                {
                    personSearchResult.RecordStatus = string.Empty;
                    personSearchResult.IsActive = false;
                }

                GetPersonSearchDetails( rockContext, personSearchResult, person, includeHtml );

                searchResult.Add( personSearchResult );
            }

            return searchResult;
        }

        /// <summary>
        /// Gets the person search details.
        /// </summary>
        /// <param name="rockContext">The rock database context.</param>
        /// <param name="personSearchResult">The person search result.</param>
        /// <param name="person">The person.</param>
        /// <param name="includeHtml">if set to <c>true</c> [include HTML].</param>
        private static void GetPersonSearchDetails( RockContext rockContext, PersonSearchResult personSearchResult, Person person, bool includeHtml )
        {
            var appPath = System.Web.VirtualPathUtility.ToAbsolute( "~" );

            // figure out Family, Address, Spouse

            Guid? recordTypeValueGuid = null;
            if ( person.RecordTypeValueId.HasValue )
            {
                recordTypeValueGuid = DefinedValueCache.Get( person.RecordTypeValueId.Value ).Guid;
            }

            personSearchResult.IsDeceased = person.IsDeceased;
            personSearchResult.IsBusiness = person.IsBusiness();
            if ( includeHtml )
            {
                personSearchResult.ImageHtmlTag = Person.GetPersonPhotoImageTag( person, 50, 50 );
            }

            var connectionStatus = person.ConnectionStatusValueId.HasValue ? DefinedValueCache.Get( person.ConnectionStatusValueId.Value ) : null;
            var campus = person.PrimaryCampusId.HasValue ? CampusCache.Get( person.PrimaryCampusId.Value ) : null;

            personSearchResult.ImageUrl = Person.GetPersonPhotoUrl( person );
            personSearchResult.Age = person.Age.HasValue ? person.Age.Value : -1;
            personSearchResult.AgeClassification = person.AgeClassification;
            personSearchResult.FormattedAge = person.FormatAge();
            personSearchResult.ConnectionStatus = connectionStatus?.Value ?? string.Empty;
            personSearchResult.ConnectionStatusColor = connectionStatus?.GetAttributeValue( "Color" );
            personSearchResult.Gender = person.Gender.ConvertToString();
            personSearchResult.Email = person.Email;
            personSearchResult.CampusName = campus?.Name;
            personSearchResult.CampusShortCode = campus?.ShortCode;

            var phoneNumbers = new PhoneNumberService( rockContext ).Queryable().Where( a => a.PersonId == person.Id ).Select( a => new
            {
                a.NumberTypeValueId,
                a.IsUnlisted,
                a.NumberFormatted
            } ).ToList();

            personSearchResult.PhoneNumbers = phoneNumbers
                .Select( p => new PersonSearchPhoneNumber
                {
                    Type = DefinedValueCache.Get( p.NumberTypeValueId ?? 0 )?.Value ?? string.Empty,
                    Number = p.NumberFormatted,
                    IsUnlisted = p.IsUnlisted
                } )
                .ToList();

            string imageHtml = string.Format(
                "<div class='person-image' style='background-image:url({0}&width=65);'></div>",
                Person.GetPersonPhotoUrl( person ) );

            StringBuilder personInfoHtmlBuilder = new StringBuilder();
            int? groupLocationTypeValueId;
            bool isBusiness = person.IsBusiness();
            if ( isBusiness )
            {
                groupLocationTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK.AsGuid() );
            }
            else
            {
                groupLocationTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );
            }

            int? personAge = person.Age;

            if ( isBusiness )
            {
                personInfoHtmlBuilder.Append( "Business" );
            }
            else if ( person.AgeClassification != AgeClassification.Unknown )
            {
                personInfoHtmlBuilder.Append( "<span class='role'>" + person.AgeClassification.ConvertToString() + "</span>" );
            }

            if ( personAge != null )
            {
                personInfoHtmlBuilder.Append( " <em class='age'>(" + person.FormatAge() + " old)</em>" );
            }

            if ( person.AgeClassification != AgeClassification.Child )
            {
                var personService = new PersonService( rockContext );
                var spouse = personService.GetSpouse(
                    person,
                    a => new
                    {
                        a.Person.NickName,
                        a.Person.LastName,
                        a.Person.SuffixValueId
                    } );

                if ( spouse != null )
                {
                    string spouseFullName = Person.FormatFullName( spouse.NickName, spouse.LastName, spouse.SuffixValueId );
                    personInfoHtmlBuilder.Append( "<p class='spouse'><strong>Spouse:</strong> " + spouseFullName + "</p>" );
                    personSearchResult.SpouseName = spouseFullName;
                    personSearchResult.SpouseNickName = spouse.NickName;
                }
            }

            if ( person.PrimaryFamilyId.HasValue )
            {
                var primaryLocation = new GroupService( rockContext ).GetSelect( person.PrimaryFamilyId.Value, s => s.GroupLocations
                       .Where( a => a.GroupLocationTypeValueId == groupLocationTypeValueId )
                       .Select( a => a.Location )
                       .FirstOrDefault() );

                if ( primaryLocation != null )
                {
                    var fullStreetAddress = primaryLocation.GetFullStreetAddress();
                    string addressHtml = $"<dl class='address'><dt>Address</dt><dd>{fullStreetAddress.ConvertCrLfToHtmlBr()}</dd></dl>";
                    personSearchResult.Address = fullStreetAddress.ConvertCrLfToHtmlBr();
                    personInfoHtmlBuilder.Append( addressHtml );
                }
            }

            // Generate the HTML for Email and PhoneNumbers
            if ( !string.IsNullOrWhiteSpace( person.Email ) || phoneNumbers.Any() )
            {
                StringBuilder sbEmailAndPhoneHtml = new StringBuilder();
                sbEmailAndPhoneHtml.Append( "<div class='margin-t-sm'>" );
                sbEmailAndPhoneHtml.Append( "<span class='email'>" + person.Email + "</span>" );
                string phoneNumberList = "<ul class='phones list-unstyled'>";

                foreach ( var phoneNumber in phoneNumbers )
                {
                    var phoneType = DefinedValueCache.Get( phoneNumber.NumberTypeValueId ?? 0 );
                    phoneNumberList += string.Format(
                        "<li x-ms-format-detection='none'>{0} <small>{1}</small></li>",
                        phoneNumber.IsUnlisted ? "Unlisted" : phoneNumber.NumberFormatted,
                        phoneType != null ? phoneType.Value : string.Empty );
                }

                sbEmailAndPhoneHtml.Append( phoneNumberList + "</ul></div>" );

                personInfoHtmlBuilder.Append( sbEmailAndPhoneHtml.ToString() );
            }

            // force the link to open a new scrollable, re-sizable browser window (and make it work in FF, Chrome and IE) http://stackoverflow.com/a/2315916/1755417
            personInfoHtmlBuilder.Append( $"<p class='margin-t-sm'><small><a href='/person/{person.Id}' class='cursor-pointer' onclick=\"javascript: window.open('/person/{person.Id}', '_blank', 'scrollbars=1,resizable=1,toolbar=1'); return false;\" data-toggle=\"tooltip\" title=\"View Profile\" tabindex=\"-1\">View Profile</a></small></p>" );

            if ( includeHtml )
            {
                personSearchResult.PickerItemDetailsImageHtml = imageHtml;
                personSearchResult.PickerItemDetailsPersonInfoHtml = personInfoHtmlBuilder.ToString();
                string itemDetailHtml = $@"
<div class='picker-select-item-details js-picker-select-item-details clearfix''>
	{imageHtml}
	<div class='contents'>
        {personSearchResult.PickerItemDetailsPersonInfoHtml}
	</div>
</div>
";

                personSearchResult.PickerItemDetailsHtml = itemDetailHtml;

                var connectionStatusHtml = string.IsNullOrWhiteSpace( personSearchResult.ConnectionStatus ) ? string.Empty : string.Format( "<span class='label label-default pull-right'>{0}</span>", personSearchResult.ConnectionStatus );
                var searchDetailsFormat = @"{0}{1}<div class='contents'>{2}</div>";
                personSearchResult.SearchDetailsHtml = string.Format( searchDetailsFormat, personSearchResult.PickerItemDetailsImageHtml, connectionStatusHtml, personSearchResult.PickerItemDetailsPersonInfoHtml );
            }
        }

        /// <summary>
        /// Creates and stores a new PersonToken for a person using the specified ExpireDateTime, UsageLimit, and Page
        /// Returns the encrypted URLEncoded Token along with the ImpersonationParameter key in the form of "rckipid={ImpersonationParameter}"
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="expireDateTime">The expire date time.</param>
        /// <param name="usageLimit">The usage limit.</param>
        /// <param name="pageId">The page identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/GetImpersonationParameter" )]
        [Rock.SystemGuid.RestActionGuid( "D1A585F7-A272-4DAC-8400-839B3219C062" )]
        public string GetImpersonationParameter( int personId, DateTime? expireDateTime = null, int? usageLimit = null, int? pageId = null )
        {
            string result = string.Empty;

            var rockContext = this.Service.Context as Rock.Data.RockContext;

            var person = new PersonService( rockContext ).Queryable().Include( a => a.Aliases ).AsNoTracking().FirstOrDefault( a => a.Id == personId );

            if ( person != null )
            {
                return person.GetImpersonationParameter( expireDateTime, usageLimit, pageId );
            }
            else
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }
        }

        /// <summary>
        /// Gets the current person's impersonation token. This is used by external apps who might have a logged in person but
        /// need to create a impersonation token to link to the website. For instance a mobile app might have the current person
        /// and have a cookie, but would like to link out to the website.
        /// </summary>
        /// <param name="expireDateTime">The expire date time.</param>
        /// <param name="usageLimit">The usage limit.</param>
        /// <param name="pageId">The page identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/GetCurrentPersonImpersonationToken" )]
        [Rock.SystemGuid.RestActionGuid( "A4765A37-043B-49CE-AA9F-C3FFF055176C" )]
        public string GetCurrentPersonImpersonationToken( DateTimeOffset? expireDateTime = null, int? usageLimit = null, int? pageId = null )
        {
            var currentPerson = GetPerson();

            if ( currentPerson == null )
            {
                return string.Empty;
            }

            // Convert to organization date time so that we don't expire
            // the token from timezone differences.
            DateTime? orgExpireDateTime = null;
            if ( expireDateTime.HasValue )
            {
                orgExpireDateTime = expireDateTime.Value.ToOrganizationDateTime();
            }

            return GetImpersonationParameter( currentPerson.Id, orgExpireDateTime, usageLimit, pageId ).Substring( 8 );
        }

        /// <summary>
        /// Gets the popup html for the selected person
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/PopupHtml/{personId}" )]
        [Rock.SystemGuid.RestActionGuid( "5231C211-9517-4C8E-8934-1BFB74A392E4" )]
        public PersonSearchResult GetPopupHtml( string personId )
        {
            return GetPopupHtml( personId, true );
        }

        /// <summary>
        /// Gets the popup html for the selected person
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <param name="emailAsLink">Determines if the email address should be formatted as a link.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/PopupHtml/{personId}/{emailAsLink}" )]
        [Rock.SystemGuid.RestActionGuid( "EB110632-B6B3-4AE4-8A0F-9711B8C85F4C" )]
        public PersonSearchResult GetPopupHtml( string personId, bool emailAsLink )
        {
            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                Person person = null;

                int? id = personId.AsIntegerOrNull();

                if ( !id.HasValue )
                {
                    var guid = personId.AsGuidOrNull();

                    if ( guid.HasValue )
                    {
                        person = personService.Get( guid.Value );
                        id = person?.Id;
                    }
                    else
                    {
                        id = Rock.Utility.IdHasher.Instance.GetId( personId );
                    }
                }

                var result = new PersonSearchResult();
                result.Id = id ?? 0;
                result.PickerItemDetailsHtml = "No Details Available";

                var html = new StringBuilder();

                // Create new service (need ProxyServiceEnabled)
                person = new PersonService( rockContext ).Queryable( "ConnectionStatusValue, PhoneNumbers" )
                    .Where( p => p.Id == id )
                    .FirstOrDefault();

                if ( person != null )
                {
                    Guid? recordTypeValueGuid = null;
                    if ( person.RecordTypeValueId.HasValue )
                    {
                        recordTypeValueGuid = DefinedValueCache.Get( person.RecordTypeValueId.Value ).Guid;
                    }

                    var appPath = System.Web.VirtualPathUtility.ToAbsolute( "~" );
                    html.AppendFormat(
                        "<header>{0} <h3>{1}<small>{2}</small></h3></header>",
                        Person.GetPersonPhotoImageTag( person, 65, 65 ),
                        person.FullName,
                        person.ConnectionStatusValue != null ? person.ConnectionStatusValue.Value : string.Empty );

                    html.Append( "<div class='body'>" );

                    var spouse = person.GetSpouse( rockContext );
                    if ( spouse != null )
                    {
                        html.AppendFormat(
                            "<div><strong>Spouse</strong> {0}</div>",
                            spouse.LastName == person.LastName ? spouse.FirstName : spouse.FullName );
                    }

                    int? age = person.Age;
                    if ( age.HasValue )
                    {
                        html.AppendFormat( "<div><strong>Age</strong> {0}</div>", age );
                    }

                    if ( !string.IsNullOrWhiteSpace( person.Email ) )
                    {
                        if ( emailAsLink )
                        {
                            html.AppendFormat( "<div style='text-overflow: ellipsis; white-space: nowrap; overflow:hidden; width: 245px;'><strong>Email</strong> {0}</div>", person.GetEmailTag( VirtualPathUtility.ToAbsolute( "~/" ) ) );
                        }
                        else
                        {
                            html.AppendFormat( "<div style='text-overflow: ellipsis; white-space: nowrap; overflow:hidden; width: 245px;'><strong>Email</strong> {0}</div>", person.Email );
                        }
                    }

                    foreach ( var phoneNumber in person.PhoneNumbers.Where( n => n.IsUnlisted == false && n.NumberTypeValueId.HasValue ).OrderBy( n => n.NumberTypeValue.Order ) )
                    {
                        html.AppendFormat( "<div><strong>{0}</strong> {1}</div>", phoneNumber.NumberTypeValue.Value, phoneNumber.ToString() );
                    }

                    html.Append( "</div>" );

                    result.PickerItemDetailsHtml = html.ToString();
                }

                return result;
            }
        }

        #endregion

        #region Delete Override

        /// <summary>
        /// DELETE endpoint for a Person Record. NOTE: Person records can not be deleted using REST, so this will always return a 405
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <exception cref="HttpResponseException"></exception>
        public override void Delete( int id )
        {
            // we don't want to support DELETE on a Person in ROCK (especially from REST).  So, return a MethodNotAllowed.
            throw new HttpResponseException( System.Net.HttpStatusCode.MethodNotAllowed );
        }

        #endregion

        #region Export

        /// <summary>
        /// Exports Person Records
        /// </summary>
        /// <param name="page">The page being requested (where first page is 1).</param>
        /// <param name="pageSize">The number of records to provide per page. NOTE: This is limited to the 'API Max Items Per Page' global attribute.</param>
        /// <param name="sortBy">Optional field to sort by. This must be a mapped property on the Person model.</param>
        /// <param name="sortDirection">The sort direction (1 = Ascending, 0 = Descending). Default is 1 (Ascending).</param>
        /// <param name="dataViewId">The optional data view to use for filtering.</param>
        /// <param name="modifiedSince">The optional date/time to filter to only get newly updated items.</param>
        /// <param name="attributeKeys">Optional comma-delimited list of attribute keys for the attribute values that should be included with each exported record, or specify 'all' to include all attributes.</param>
        /// <param name="attributeReturnType">Raw/Formatted (default is Raw)</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/Export" )]
        [Rock.SystemGuid.RestActionGuid( "B32254A7-18CA-4548-8946-92CEA9E3D47A" )]
        public PeopleExport Export(
            int page = 1,
            int pageSize = 1000,
            string sortBy = null,
            System.Web.UI.WebControls.SortDirection sortDirection = System.Web.UI.WebControls.SortDirection.Ascending,
            int? dataViewId = null,
            DateTime? modifiedSince = null,
            string attributeKeys = null,
            AttributeReturnType attributeReturnType = AttributeReturnType.Raw )
        {
            // limit to 'API Max Items Per Page' global attribute
            int maxPageSize = GlobalAttributesCache.Get().GetValue( "core_ExportAPIsMaxItemsPerPage" ).AsIntegerOrNull() ?? 1000;
            var actualPageSize = Math.Min( pageSize, maxPageSize );

            ExportOptions exportOptions = new ExportOptions
            {
                SortBy = sortBy,
                SortDirection = sortDirection,
                DataViewId = dataViewId,
                ModifiedSince = modifiedSince,
                AttributeList = AttributesExport.GetAttributesFromAttributeKeys<Person>( attributeKeys ),
                AttributeReturnType = attributeReturnType
            };

            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            return personService.GetPeopleExport( page, actualPageSize, exportOptions );
        }

        #endregion

        #region VCard

        /// <summary>
        /// Returns VCard for person.
        /// </summary>
        /// <param name="personGuid">The person Guid.</param>
        /// <returns></returns>
        [HttpGet]
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/People/VCard/{personGuid}" )]
        [Rock.SystemGuid.RestActionGuid( "C4D4BCBD-B65B-4BAA-AEE0-9B7266EA9B02" )]
        public HttpResponseMessage GetVCard( Guid personGuid )
        {
            var rockContext = ( Rock.Data.RockContext ) Service.Context;

            var person = new PersonService( rockContext ).Get( personGuid );
            if ( person == null )
            {
                throw new HttpResponseException( new System.Net.Http.HttpResponseMessage( HttpStatusCode.NotFound ) );
            }

            string fileName = person.FullName + ".vcf";
            HttpResponseMessage result = new HttpResponseMessage( HttpStatusCode.OK );

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, GetPerson() );
            mergeFields.Add( "Person", person );
            string vCard = GlobalAttributesCache.Value( "VCardFormat" ).ResolveMergeFields( mergeFields ).Trim();

            // remove empty lines (the vcard spec is very picky)
            vCard = Regex.Replace( vCard, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline );

            var inputEncoding = Encoding.Default;
            var outputEncoding = Encoding.GetEncoding( 28591 );
            var cardBytes = inputEncoding.GetBytes( vCard );
            var outputBytes = Encoding.Convert( inputEncoding, outputEncoding, cardBytes );
            result.Content = new ByteArrayContent( outputBytes );
            result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue( "text/vcard" );
            result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue( "attachment" );
            result.Content.Headers.ContentDisposition.FileName = fileName;
            return result;
        }

        #endregion
    }

    /// <summary>
    ///
    /// </summary>
    [RockClientInclude( "Search result item from api/People/Search" )]
    public class PersonSearchResult
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the id key of the person.
        /// </summary>
        /// <value>The id key</value>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the person.
        /// </summary>
        /// <value>Gets or sets the unique identifier of the person.</value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the person's primary alias.
        /// </summary>
        /// <value>Gets or sets the unique identifier of the person's primary alias.</value>
        public Guid PrimaryAliasGuid { get; set; }

        /// <summary>
        /// Gets or sets the full name last first.
        /// </summary>
        /// <value>
        /// The full name last first.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is deceased.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is deceased; otherwise, <c>false</c>.
        /// </value>
        public bool IsDeceased { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is a business.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is a business; otherwise, <c>false</c>.
        /// </value>
        public bool IsBusiness { get; set; }

        /// <summary>
        /// Gets or sets the person photo image to display.
        /// </summary>
        /// <value>
        /// The person photo image to display.
        /// </value>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the image HTML tag.
        /// </summary>
        /// <value>
        /// The image HTML tag.
        /// </value>
        public string ImageHtmlTag { get; set; }

        /// <summary>
        /// Gets or sets the age in years
        /// NOTE: returns -1 if age is unknown
        /// </summary>
        /// <value>The age.</value>
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets the formatted age.
        /// </summary>
        /// <value>
        /// The formatted age.
        /// </value>
        public string FormattedAge { get; set; }

        /// <summary>
        /// Gets or sets the age classification value.
        /// </summary>
        /// <value>
        /// The age classification value.
        /// </value>
        public AgeClassification AgeClassification { get; set; }

        /// <summary>
        /// Gets or sets the gender.
        /// </summary>
        /// <value>The gender.</value>
        public string Gender { get; set; }

        /// <summary>
        /// Gets or sets the connection status.
        /// </summary>
        /// <value>The connection status.</value>
        public string ConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets the color of the connection status.
        /// </summary>
        /// <value>The color of the connection status.</value>
        public string ConnectionStatusColor { get; set; }

        /// <summary>
        /// Gets or sets the record status.
        /// </summary>
        /// <value>The member status.</value>
        public string RecordStatus { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the name of the spouse.
        /// </summary>
        /// <value>
        /// The name of the spouse.
        /// </value>
        public string SpouseName { get; set; }

        /// <summary>
        /// Gets or sets the nickname of the spouse.
        /// </summary>
        /// <value>
        /// The nickname of the spouse.
        /// </value>
        public string SpouseNickName { get; set; }

        /// <summary>
        /// Gets or sets the name of the campus.
        /// </summary>
        /// <value>The name of the campus.</value>
        public string CampusName { get; set; }

        /// <summary>
        /// Gets or sets the campus short code.
        /// </summary>
        /// <value>The campus short code.</value>
        public string CampusShortCode { get; set; }

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        /// <value>
        /// The address.
        /// </value>
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the phone numbers for this person.
        /// </summary>
        /// <value>
        /// The phone numbers for this person.
        /// </value>
        public List<PersonSearchPhoneNumber> PhoneNumbers { get; set; }

        /// <summary>
        /// Gets or sets the picker item details HTML.
        /// </summary>
        /// <value>
        /// The picker item details HTML.
        /// </value>
        public string PickerItemDetailsHtml { get; set; }

        /// <summary>
        /// The search details
        /// </summary>
        public string SearchDetailsHtml { get; set; }

        /// <summary>
        /// Gets or sets the picker item details image HTML.
        /// </summary>
        /// <value>
        /// The picker item details image HTML.
        /// </value>
        public string PickerItemDetailsImageHtml { get; set; }

        /// <summary>
        /// Gets or sets the picker item details person information HTML.
        /// </summary>
        /// <value>
        /// The picker item details person information HTML.
        /// </value>
        public string PickerItemDetailsPersonInfoHtml { get; set; }
    }

    /// <summary>
    /// A phone number that will be included in the search results.
    /// </summary>
    [RockClientInclude( "Search result PersonSearchResult.PhoneNumbers from api/People/Search" )]
    public class PersonSearchPhoneNumber
    {
        /// <summary>
        /// Gets or sets the type of phone number this instance represents.
        /// </summary>
        /// <value>
        /// The type of phone number this instance represents.
        /// </value>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the formatted phone number this instance represents.
        /// </summary>
        /// <value>
        /// The formatted phone number this instance represents.
        /// </value>
        public string Number { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates if this phone number is unlisted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this phone number is unlisted; otherwise, <c>false</c>.
        /// </value>
        public bool IsUnlisted { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class ConfigureTextToGiveArgs
    {
        /// <summary>
        /// The Financial Account Id that will be the default gift designation for the person. Null value
        /// clears the setting and requires the user to set before text-to-give will work for them.
        /// </summary>
        public int? ContributionFinancialAccountId { get; set; }

        /// <summary>
        /// The Saved Account associated with the person that will be used as the default payment method for
        /// the person throughout Rock
        /// </summary>
        public int? FinancialPersonSavedAccountId { get; set; }
    }

    /// <summary>
    /// Person Interaction Statistics
    /// </summary>
    public class PersonInteractionStatistics
    {
        /// <summary>
        /// Gets or sets the interactions all time.
        /// </summary>
        /// <value>
        /// The interactions all time.
        /// </value>
        public int InteractionsAllTime { get; set; }

        /// <summary>
        /// Gets or sets the interactions that day.
        /// </summary>
        /// <value>
        /// The interactions that day.
        /// </value>
        public int InteractionsThatDay { get; set; }

        /// <summary>
        /// Gets or sets the interactions that month.
        /// </summary>
        /// <value>
        /// The interactions that month.
        /// </value>
        public int InteractionsThatMonth { get; set; }

        /// <summary>
        /// Gets or sets the interactions that year.
        /// </summary>
        /// <value>
        /// The interactions that year.
        /// </value>
        public int InteractionsThatYear { get; set; }
    }
}