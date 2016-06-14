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
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.OData;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    ///
    /// </summary>
    public partial class PeopleController
    {
        #region Get

        // GET api/<controller>/5
        [Authenticate, Secured]
        [ActionName( "GetById" )]
        public override Person GetById( int id )
        {
            // NOTE: We want PrimaryAliasId to be populated, so call this.Get( true ) which includes "Aliases"
            var person = this.Get( true ).FirstOrDefault( a => a.Id == id );
            if ( person == null )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            return person;
        }

        // GET api/<controller>(5)
        [Authenticate, Secured]
        [EnableQuery]
        public override Person Get( [FromODataUri] int key )
        {
            // NOTE: We want PrimaryAliasId to be populated, so call this.GetById( key ) which includes "Aliases"
            return this.GetById( key );
        }

        /// <summary>
        /// Returns a Queryable of Person records
        /// </summary>
        /// <returns>
        /// A queryable collection of Person records that matches the supplied Odata query.
        /// </returns>
        [Authenticate, Secured]
        [EnableQuery]
        public override IQueryable<Person> Get()
        {
            // NOTE: We want PrimaryAliasId to be populated, so include Aliases
            return base.Get().Include( a => a.Aliases );
        }

        /// <summary>
        /// Get api controller with option to include Person records for deceased individuals.
        /// </summary>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [EnableQuery]
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
        [System.Web.Http.Route("api/People/GetCurrentPerson")]
        public Person GetCurrentPerson()
        {
            var rockContext = new Rock.Data.RockContext();
            return new PersonService(rockContext).Get(GetPerson().Id);
        }

        /// <summary>
        /// Searches the person entit(ies) by email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/GetByEmail/{email}" )]
        public IQueryable<Person> GetByEmail( string email )
        {
            var rockContext = new Rock.Data.RockContext();
            return new PersonService( rockContext ).GetByEmail( email, true ).Include( a => a.Aliases );
        }

        /// <summary>
        /// Searches the person entit(ies) by phone number.
        /// </summary>
        /// <param name="number">The phone number.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/GetByPhoneNumber/{number}" )]
        public IQueryable<Person> GetByPhoneNumber( string number )
        {
            var rockContext = new Rock.Data.RockContext();
            return new PersonService( rockContext ).GetByPhonePartial( number, true ).Include( a => a.Aliases );
        }

        /// <summary>
        /// Gets the name of the by user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/GetByUserName/{username}" )]
        public Person GetByUserName( string username )
        {
            int? personId = new UserLoginService( (Rock.Data.RockContext)Service.Context ).Queryable()
                .Where( u => u.UserName.Equals( username ) )
                .Select( a => a.PersonId )
                .FirstOrDefault();

            if ( personId != null )
            {
                return Service.Queryable().Include( a => a.PhoneNumbers).Include(a => a.Aliases )
                    .FirstOrDefault( p => p.Id == personId.Value );
            }

            throw new HttpResponseException( System.Net.HttpStatusCode.NotFound );
        }

        /// <summary>
        /// Gets the Person by person alias identifier.
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/GetByPersonAliasId/{personAliasId}" )]
        public Person GetByPersonAliasId( int personAliasId )
        {
            int? personId = new PersonAliasService( (Rock.Data.RockContext)Service.Context ).Queryable()
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
        [System.Web.Http.Route("api/People/GetGraduationYear/{gradeOffset}")]
        public int GetGraduationYear( int gradeOffset )
        {
            int? graduationYear = Person.GraduationYearFromGradeOffset( gradeOffset );
            if( graduationYear.HasValue )
            {
                return graduationYear.Value;
            }

            throw new HttpResponseException( System.Net.HttpStatusCode.NotFound );
        }

        #endregion

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

            System.Web.HttpContext.Current.Items.Add( "CurrentPerson", GetPerson() );
            PersonService.SaveNewPerson( person, (Rock.Data.RockContext)Service.Context, null, false );

            return ControllerContext.Request.CreateResponse( HttpStatusCode.Created, person.Id );
        }

        public override void Put( int id, Person person )
        {
            SetProxyCreation( true );

            var rockContext = (RockContext)Service.Context;
            var existingPerson = Service.Get( id );
            if ( existingPerson != null )
            {
                var changes = new List<string>();
                History.EvaluateChange( changes, "Record Status", DefinedValueCache.GetName( existingPerson.RecordStatusValueId ), DefinedValueCache.GetName( person.RecordStatusValueId ) );
                History.EvaluateChange( changes, "Inactive Reason", DefinedValueCache.GetName( existingPerson.RecordStatusReasonValueId ), DefinedValueCache.GetName( person.RecordStatusReasonValueId ) );
                History.EvaluateChange( changes, "Title", DefinedValueCache.GetName( existingPerson.TitleValueId ), DefinedValueCache.GetName( person.TitleValueId ) );
                History.EvaluateChange( changes, "First Name", existingPerson.FirstName, person.FirstName );
                History.EvaluateChange( changes, "Nick Name", existingPerson.NickName, person.NickName );
                History.EvaluateChange( changes, "Middle Name", existingPerson.MiddleName, person.MiddleName );
                History.EvaluateChange( changes, "Last Name", existingPerson.LastName, person.LastName );
                History.EvaluateChange( changes, "Suffix", DefinedValueCache.GetName( existingPerson.SuffixValueId ), DefinedValueCache.GetName( person.SuffixValueId ) );
                History.EvaluateChange( changes, "Birth Month", existingPerson.BirthMonth, person.BirthMonth );
                History.EvaluateChange( changes, "Birth Day", existingPerson.BirthDay, person.BirthDay );
                History.EvaluateChange( changes, "Birth Year", existingPerson.BirthYear, person.BirthYear );
                History.EvaluateChange( changes, "Graduation Year", existingPerson.GraduationYear, person.GraduationYear );
                History.EvaluateChange( changes, "Anniversary Date", existingPerson.AnniversaryDate, person.AnniversaryDate );
                History.EvaluateChange( changes, "Gender", existingPerson.Gender, person.Gender );
                History.EvaluateChange( changes, "Marital Status", DefinedValueCache.GetName( existingPerson.MaritalStatusValueId ), DefinedValueCache.GetName( person.MaritalStatusValueId ) );
                History.EvaluateChange( changes, "Connection Status", DefinedValueCache.GetName( existingPerson.ConnectionStatusValueId ), DefinedValueCache.GetName( person.ConnectionStatusValueId ) );
                History.EvaluateChange( changes, "Email", existingPerson.Email, person.Email );
                History.EvaluateChange( changes, "Email Active", existingPerson.IsEmailActive, person.IsEmailActive );
                History.EvaluateChange( changes, "Email Preference", existingPerson.EmailPreference, person.EmailPreference );

                if ( person.GivingGroupId != existingPerson.GivingGroupId )
                {
                    string oldGivingGroupName = existingPerson.GivingGroup != null ? existingPerson.GivingGroup.Name : string.Empty;
                    string newGivingGroupName = person.GivingGroup != null ? person.GivingGroup.Name : string.Empty;
                    if ( person.GivingGroupId.HasValue && string.IsNullOrWhiteSpace( newGivingGroupName ) )
                    {
                        var givingGroup = new GroupService( rockContext ).Get( person.GivingGroupId.Value );
                        newGivingGroupName = givingGroup != null ? givingGroup.Name : string.Empty;
                    }
                    History.EvaluateChange( changes, "Giving Group", oldGivingGroupName, newGivingGroupName );
                }

                if ( changes.Any() )
                {
                    System.Web.HttpContext.Current.Items.Add( "CurrentPerson", GetPerson() );

                    int? modifiedByPersonAliasId = person.ModifiedAuditValuesAlreadyUpdated ? person.ModifiedByPersonAliasId : (int?)null;

                    HistoryService.SaveChanges(
                        rockContext,
                        typeof( Person ),
                        Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                        person.Id,
                        changes,
                        true,
                        modifiedByPersonAliasId );
                }
            }

            base.Put( id, person );
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

            System.Web.HttpContext.Current.Items.Add( "CurrentPerson", GetPerson() );

            PersonService.AddPersonToFamily( person, person.Id == 0, familyId, groupRoleId, (Rock.Data.RockContext)Service.Context );

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
        public System.Net.Http.HttpResponseMessage AddExistingPersonToFamily( int personId, int familyId, int groupRoleId, bool removeFromOtherFamilies )
        {
            SetProxyCreation( true );

            System.Web.HttpContext.Current.Items.Add( "CurrentPerson", GetPerson() );
            var person = this.Get( personId );
            CheckCanEdit( person );

            PersonService.AddPersonToFamily( person, false, familyId, groupRoleId, (Rock.Data.RockContext)Service.Context );

            if ( removeFromOtherFamilies )
            {
                PersonService.RemovePersonFromOtherFamilies( familyId, personId, (Rock.Data.RockContext)Service.Context );
            }

            return ControllerContext.Request.CreateResponse( HttpStatusCode.Created, person.Id );
        }

        #endregion

        #region Search

        /// <summary>
        /// Returns results to the Person Picker
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="includeHtml">if set to <c>true</c> [include HTML].</param>
        /// <param name="includeDetails">if set to <c>true</c> [include details].</param>
        /// <param name="includeBusinesses">if set to <c>true</c> [include businesses].</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/Search" )]
        public IQueryable<PersonSearchResult> Search( string name, bool includeHtml, bool includeDetails, bool includeBusinesses = false, bool includeDeceased = false )
        {
            int count = 20;
            bool showFullNameReversed;
            bool allowFirstNameOnly = false;

            var searchComponent = Rock.Search.SearchContainer.GetComponent( typeof( Rock.Search.Person.Name ).FullName );
            if ( searchComponent != null )
            {
                allowFirstNameOnly = searchComponent.GetAttributeValue( "FirstNameSearch" ).AsBoolean();
            }

            var activeRecordStatusValue = DefinedValueCache.Read( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() );
            int activeRecordStatusValueId = activeRecordStatusValue != null ? activeRecordStatusValue.Id : 0;

            IQueryable<Person> sortedPersonQry = ( this.Service as PersonService )
                .GetByFullNameOrdered( name, true, includeBusinesses, allowFirstNameOnly, out showFullNameReversed ).Take( count );

            if ( includeDetails == false )
            {
                var simpleResultQry = sortedPersonQry.Select( a => new { a.Id, a.FirstName, a.NickName, a.LastName, a.SuffixValueId, a.RecordTypeValueId, a.RecordStatusValueId } );
                var simpleResult = simpleResultQry.ToList().Select( a => new PersonSearchResult
                {
                    Id = a.Id,
                    Name = showFullNameReversed
                    ? Person.FormatFullNameReversed( a.LastName, a.NickName, a.SuffixValueId, a.RecordTypeValueId )
                    : Person.FormatFullName( a.NickName, a.LastName, a.SuffixValueId, a.RecordTypeValueId ),
                    IsActive = a.RecordStatusValueId.HasValue && a.RecordStatusValueId == activeRecordStatusValueId
                } );

                return simpleResult.AsQueryable();
            }
            else
            {
                List<PersonSearchResult> searchResult = SearchWithDetails( sortedPersonQry, showFullNameReversed );
                return searchResult.AsQueryable();
            }
        }

        /// <summary>
        /// Gets the search details (for the person picker)
        /// </summary>
        /// <param name="Id">The identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/GetSearchDetails" )]
        public string GetSearchDetails( int Id )
        {
            PersonSearchResult personSearchResult = new PersonSearchResult();
            var person = this.Get().Include( a => a.PhoneNumbers ).Where( a => a.Id == Id ).FirstOrDefault();
            if ( person != null )
            {
                GetPersonSearchDetails( personSearchResult, person );
                // Generate the HTML for the ConnectionStatus; "label-success" matches the default config of the
                // connection status badge on the Bio bar, but I think label-default works better here.
                string connectionStatusHtml = string.IsNullOrWhiteSpace( personSearchResult.ConnectionStatus ) ? string.Empty : string.Format( "<span class='label label-default pull-right'>{0}</span>", personSearchResult.ConnectionStatus );
                string searchDetailsFormat = @"{0}{1}<div class='contents'>{2}</div>";
                return string.Format( searchDetailsFormat, personSearchResult.PickerItemDetailsImageHtml, connectionStatusHtml, personSearchResult.PickerItemDetailsPersonInfoHtml );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Searches the with details.
        /// </summary>
        /// <param name="reversed">if set to <c>true</c> [reversed].</param>
        /// <param name="sortedPersonQry">The sorted person qry.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<PersonSearchResult> SearchWithDetails( IQueryable<Person> sortedPersonQry, bool showFullNameReversed )
        {
            var rockContext = this.Service.Context as Rock.Data.RockContext;
            var phoneNumbersQry = new PhoneNumberService( rockContext ).Queryable();
            var sortedPersonList = sortedPersonQry.Include( a => a.PhoneNumbers ).AsNoTracking().ToList();
            Guid activeRecord = new Guid( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE );

            List<PersonSearchResult> searchResult = new List<PersonSearchResult>();
            foreach ( var person in sortedPersonList )
            {
                PersonSearchResult personSearchResult = new PersonSearchResult();
                personSearchResult.Id = person.Id;
                personSearchResult.Name = showFullNameReversed ? person.FullNameReversed : person.FullName;
                if ( person.RecordStatusValueId.HasValue )
                {
                    var recordStatus = DefinedValueCache.Read( person.RecordStatusValueId.Value );
                    personSearchResult.RecordStatus = recordStatus.Value;
                    personSearchResult.IsActive = recordStatus.Guid.Equals( activeRecord );
                }
                else
                {
                    personSearchResult.RecordStatus = string.Empty;
                    personSearchResult.IsActive = false;
                }

                GetPersonSearchDetails( personSearchResult, person );

                searchResult.Add( personSearchResult );
            }

            return searchResult;
        }

        /// <summary>
        /// Gets the person search details.
        /// </summary>
        /// <param name="personSearchResult">The person search result.</param>
        /// <param name="person">The person.</param>
        private void GetPersonSearchDetails( PersonSearchResult personSearchResult, Person person )
        {
            var rockContext = this.Service.Context as Rock.Data.RockContext;

            var appPath = System.Web.VirtualPathUtility.ToAbsolute( "~" );
            string itemDetailFormat = @"
<div class='picker-select-item-details clearfix' style='display: none;'>
	{0}
	<div class='contents'>
        {1}
	</div>
</div>
";

            var familyGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
            int adultRoleId = familyGroupType.Roles.First( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Id;

            int groupTypeFamilyId = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() ).Id;

            // figure out Family, Address, Spouse
            GroupMemberService groupMemberService = new GroupMemberService( rockContext );

            Guid? recordTypeValueGuid = null;
            if ( person.RecordTypeValueId.HasValue )
            {
                recordTypeValueGuid = DefinedValueCache.Read( person.RecordTypeValueId.Value ).Guid;
            }

            personSearchResult.ImageHtmlTag = Person.GetPersonPhotoImageTag( person, 50, 50 );
            personSearchResult.Age = person.Age.HasValue ? person.Age.Value : -1;
            personSearchResult.ConnectionStatus = person.ConnectionStatusValueId.HasValue ? DefinedValueCache.Read( person.ConnectionStatusValueId.Value ).Value : string.Empty;
            personSearchResult.Gender = person.Gender.ConvertToString();
            personSearchResult.Email = person.Email;

            string imageHtml = string.Format(
                "<div class='person-image' style='background-image:url({0}&width=65);background-size:cover;background-position:50%'></div>",
                Person.GetPersonPhotoUrl( person, 200, 200 ) );

            string personInfoHtml = string.Empty;
            Guid matchLocationGuid;
            bool isBusiness;
            if ( recordTypeValueGuid.HasValue && recordTypeValueGuid == Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() )
            {
                isBusiness = true;
                matchLocationGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK.AsGuid();
            }
            else
            {
                isBusiness = false;
                matchLocationGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid();
            }

            var familyGroupMember = groupMemberService.Queryable()
                .Where( a => a.PersonId == person.Id )
                .Where( a => a.Group.GroupTypeId == groupTypeFamilyId )
                .Select( s => new
                {
                    s.GroupRoleId,
                    GroupLocation = s.Group.GroupLocations.Where( a => a.GroupLocationTypeValue.Guid == matchLocationGuid ).Select( a => a.Location ).FirstOrDefault()
                } ).FirstOrDefault();

            int? personAge = person.Age;

            if ( familyGroupMember != null )
            {
                if ( isBusiness )
                {
                    personInfoHtml += "Business";
                }
                else
                {
                    personInfoHtml += familyGroupType.Roles.First( a => a.Id == familyGroupMember.GroupRoleId ).Name;
                }

                if ( personAge != null )
                {
                    personInfoHtml += " <em>(" + personAge.ToString() + " yrs old)</em>";
                }

                if ( familyGroupMember.GroupRoleId == adultRoleId )
                {
                    var personService = this.Service as PersonService;
                    var spouse = personService.GetSpouse( person, a => new
                    {
                        a.Person.NickName,
                        a.Person.LastName,
                        a.Person.SuffixValueId
                    } );

                    if ( spouse != null )
                    {
                        string spouseFullName = Person.FormatFullName( spouse.NickName, spouse.LastName, spouse.SuffixValueId );
                        personInfoHtml += "<p><strong>Spouse:</strong> " + spouseFullName + "</p>";
                        personSearchResult.SpouseName = spouseFullName;
                    }
                }
            }
            else
            {
                if ( personAge != null )
                {
                    personInfoHtml += personAge.ToString() + " yrs old";
                }
            }

            if ( familyGroupMember != null )
            {
                var location = familyGroupMember.GroupLocation;

                if ( location != null )
                {
                    string addressHtml = "<h5>Address</h5>" + location.GetFullStreetAddress().ConvertCrLfToHtmlBr();
                    personSearchResult.Address = location.GetFullStreetAddress();
                    personInfoHtml += addressHtml;
                }
            }

            // Generate the HTML for Email and PhoneNumbers
            if ( !string.IsNullOrWhiteSpace( person.Email ) || person.PhoneNumbers.Any() )
            {
                string emailAndPhoneHtml = "<p class='margin-t-sm'>";
                emailAndPhoneHtml += person.Email;
                string phoneNumberList = string.Empty;
                foreach ( var phoneNumber in person.PhoneNumbers )
                {
                    var phoneType = DefinedValueCache.Read( phoneNumber.NumberTypeValueId ?? 0 );
                    phoneNumberList += string.Format(
                        "<br>{0} <small>{1}</small>",
                        phoneNumber.IsUnlisted ? "Unlisted" : phoneNumber.NumberFormatted,
                        phoneType != null ? phoneType.Value : string.Empty );
                }

                emailAndPhoneHtml += phoneNumberList + "<p>";

                personInfoHtml += emailAndPhoneHtml;
            }

            personSearchResult.PickerItemDetailsImageHtml = imageHtml;
            personSearchResult.PickerItemDetailsPersonInfoHtml = personInfoHtml;
            personSearchResult.PickerItemDetailsHtml = string.Format( itemDetailFormat, imageHtml, personInfoHtml );
        }

        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/GetSearchDetails/{personId}" )]
        public string GetImpersonationParameter( int personId )
        {
            string result = string.Empty;
            
            var rockContext = this.Service.Context as Rock.Data.RockContext;

            var person = new PersonService( rockContext ).Get( personId );

            if ( person != null )
            {
                result = person.ImpersonationParameter;
            }

            return result;
        }

        /// <summary>
        /// Gets the popup html for the selected person
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/PopupHtml/{personId}" )]
        public PersonSearchResult GetPopupHtml( int personId )
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
        public PersonSearchResult GetPopupHtml( int personId, bool emailAsLink )
        {
            var result = new PersonSearchResult();
            result.Id = personId;
            result.PickerItemDetailsHtml = "No Details Available";

            var html = new StringBuilder();

            // Create new service (need ProxyServiceEnabled)
            var rockContext = new Rock.Data.RockContext();
            var person = new PersonService( rockContext ).Queryable( "ConnectionStatusValue, PhoneNumbers" )
                .Where( p => p.Id == personId )
                .FirstOrDefault();

            if ( person != null )
            {
                Guid? recordTypeValueGuid = null;
                if ( person.RecordTypeValueId.HasValue )
                {
                    recordTypeValueGuid = DefinedValueCache.Read( person.RecordTypeValueId.Value ).Guid;
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
                    html.AppendFormat( "<div><strong>Age</strong> {0}</div>" , age );
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

        #endregion

        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public override void Delete( int id )
        {
            // we don't want to support DELETE on a Person in ROCK (especially from REST).  So, return a MethodNotAllowed.
            throw new HttpResponseException( System.Net.HttpStatusCode.MethodNotAllowed );
        }
    }

    /// <summary>
    ///
    /// </summary>
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
        /// Gets or sets the image HTML tag.
        /// </summary>
        /// <value>
        /// The image HTML tag.
        /// </value>
        public string ImageHtmlTag { get; set; }

        /// <summary>
        /// Gets or sets the age.
        /// </summary>
        /// <value>The age.</value>
        public int Age { get; set; }

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
        /// Gets or sets the address.
        /// </summary>
        /// <value>
        /// The address.
        /// </value>
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the picker item details HTML.
        /// </summary>
        /// <value>
        /// The picker item details HTML.
        /// </value>
        public string PickerItemDetailsHtml { get; set; }

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
}
