// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Http;

using Rock.Rest.Filters;
using Rock.Search.Person;
using Rock.Model;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class PeopleController : IHasCustomRoutes
    {
        /// <summary>
        /// Adds the routes.
        /// </summary>
        /// <param name="routes">The routes.</param>
        public void AddRoutes( System.Web.Routing.RouteCollection routes )
        {
            routes.MapHttpRoute(
                name: "PeopleSearchParam",
                routeTemplate: "api/People/Search",
                defaults: new
                {
                    controller = "People",
                    action = "Search"
                } );

            routes.MapHttpRoute(
                name: "PeopleSearch",
                routeTemplate: "api/People/Search/{name}/{includeHtml}",
                defaults: new
                {
                    controller = "People",
                    action = "Search"
                } );

            routes.MapHttpRoute(
                name: "PeopleGetByUserName",
                routeTemplate: "api/People/GetByUserName/{username}",
                defaults: new
                {
                    controller = "People",
                    action = "GetByUserName"
                } );

            routes.MapHttpRoute(
                name: "PeopleGetByPersonAliasId",
                routeTemplate: "api/People/GetByPersonAliasId/{personAliasId}",
                defaults: new
                {
                    controller = "People",
                    action = "GetByPersonAliasId"
                } );

            routes.MapHttpRoute(
                name: "PeoplePopupHtml",
                routeTemplate: "api/People/PopupHtml/{personId}",
                defaults: new
                {
                    controller = "People",
                    action = "GetPopupHtml"
                } );

        }

        /// <summary>
        /// Searches the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        public IQueryable<PersonSearchResult> Search( string name )
        {
            return Search( name, false );
        }

        /// <summary>
        /// Searches the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        public IQueryable<PersonSearchResult> Search( string name, bool includeHtml )
        {
            int count = 20;
            bool reversed;
            bool allowFirstNameOnly = false;

            var searchComponent = Rock.Search.SearchContainer.GetComponent( typeof( Rock.Search.Person.Name ) );
            if ( searchComponent != null )
            {
                allowFirstNameOnly = searchComponent.GetAttributeValue( "FirstNameSearch" ).AsBoolean();
            }

            IOrderedQueryable<Person> sortedPersonQry = new PersonService().GetByFullNameOrdered( name, false, allowFirstNameOnly, out reversed );

            var topQry = sortedPersonQry.Take( count );
            List<Person> sortedPersonList = topQry.ToList();

            var appPath = System.Web.VirtualPathUtility.ToAbsolute( "~" );
            string itemDetailFormat = @"
<div class='picker-select-item-details clearfix' style='display: none;'>
	{0}
	<div class='contents'>
        {1}
	</div>
</div>
";

            Guid activeRecord = new Guid( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE );

            // figure out Family, Address, Spouse
            GroupMemberService groupMemberService = new GroupMemberService();

            List<PersonSearchResult> searchResult = new List<PersonSearchResult>();
            foreach ( var person in sortedPersonList )
            {
                PersonSearchResult personSearchResult = new PersonSearchResult();
                personSearchResult.Name = reversed ? person.FullNameReversed : person.FullName;
                personSearchResult.ImageHtmlTag = Person.GetPhotoImageTag( person.PhotoId, person.Gender, 50, 50 );
                personSearchResult.Age = person.Age.HasValue ? person.Age.Value : -1;
                personSearchResult.ConnectionStatus = person.ConnectionStatusValue != null ? person.ConnectionStatusValue.Name : string.Empty;
                personSearchResult.Gender = person.Gender.ConvertToString();
                personSearchResult.Email = person.Email;

                if ( person.RecordStatusValue != null )
                {
                    personSearchResult.RecordStatus = person.RecordStatusValue.Name;
                    personSearchResult.IsActive = person.RecordStatusValue.Guid.Equals( activeRecord );
                }
                else
                {
                    personSearchResult.RecordStatus = string.Empty;
                    personSearchResult.IsActive = false;
                }

                personSearchResult.Id = person.Id;

                string imageHtml = string.Format(
                    "<div class='person-image' style='background-image:url({0}&width=65);background-size:cover;background-position:50%'></div>",
                    Person.GetPhotoUrl( person.PhotoId, person.Gender ) );

                string personInfo = string.Empty;

                var groupMemberQry = groupMemberService.Queryable().Where( a => a.PersonId.Equals( person.Id ) );
                List<GroupMember> personGroupMember = groupMemberQry.ToList();

                Guid familyGuid = new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
                Guid adultGuid = new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT );

                GroupMember familyGroupMember = personGroupMember.Where( a => a.Group.GroupType.Guid.Equals( familyGuid ) ).FirstOrDefault();
                if ( familyGroupMember != null )
                {
                    personInfo += familyGroupMember.GroupRole.Name;
                    if ( person.Age != null )
                    {
                        personInfo += " <em>(" + person.Age.ToString() + " yrs old)</em>";
                    }

                    // Figure out spouse (Implied by "the other GROUPROLE_FAMILY_MEMBER_ADULT that is of the opposite gender")
                    if ( familyGroupMember.GroupRole.Guid.Equals( adultGuid ) )
                    {
                        person.GetSpouse();
                        GroupMember spouseMember = familyGroupMember.Group.Members.Where( a => !a.PersonId.Equals( person.Id ) && a.GroupRole.Guid.Equals( adultGuid ) ).FirstOrDefault();
                        if ( spouseMember != null )
                        {
                            if ( !familyGroupMember.Person.Gender.Equals( spouseMember.Person.Gender ) )
                            {
                                personInfo += "<p><strong>Spouse:</strong> " + spouseMember.Person.FullName + "</p>";
                                personSearchResult.SpouseName = spouseMember.Person.FullName;
                            }
                        }
                    }
                }
                else
                {
                    if ( person.Age != null )
                    {
                        personInfo += person.Age.ToString() + " yrs old";
                    }
                }

                if ( familyGroupMember != null )
                {
                    var groupLocation = familyGroupMember.Group.GroupLocations.FirstOrDefault();
                    if ( groupLocation != null )
                    {
                        var location = groupLocation.Location;
                        if ( location != null )
                        {
                            string streetInfo;
                            if ( !string.IsNullOrWhiteSpace( location.Street1 ) )
                            {
                                streetInfo = location.Street1 + " " + location.Street2;
                            }
                            else
                            {
                                streetInfo = location.Street2;
                            }

                            string addressHtml = string.Format( "<h5>Address</h5>{0} <br />{1}, {2}, {3}", streetInfo, location.City, location.State, location.Zip );
                            personSearchResult.Address = location.ToString();
                            personInfo += addressHtml;
                        }
                    }

                    if ( includeHtml )
                    {
                        personSearchResult.PickerItemDetailsHtml = string.Format( itemDetailFormat, imageHtml, personInfo );
                    }
                }

                searchResult.Add( personSearchResult );
            }

            return searchResult.AsQueryable();
        }

        /// <summary>
        /// Gets the name of the by user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        public Person GetByUserName( string username )
        {
            int? personId = new UserLoginService().Queryable().Where( u => u.UserName.Equals( username ) ).Select( a => a.PersonId ).FirstOrDefault();
            if ( personId != null )
            {
                return this.Get( personId.Value );
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
        public Person GetByPersonAliasId( int personAliasId )
        {
            int? personId = new PersonAliasService().Queryable().Where( u => u.Id.Equals( personAliasId ) ).Select( a => a.PersonId ).FirstOrDefault();
            if ( personId != null )
            {
                return this.Get( personId.Value );
            }

            throw new HttpResponseException( System.Net.HttpStatusCode.NotFound );
        }

        /// <summary>
        /// Gets the popup html for the selected person
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        public PersonSearchResult GetPopupHtml( int personId )
        {
            var result = new PersonSearchResult();
            result.Id = personId;
            result.PickerItemDetailsHtml = "No Details Available";

            var html = new StringBuilder();
            var person = new PersonService().Get( personId );
            if ( person != null )
            {
                var appPath = System.Web.VirtualPathUtility.ToAbsolute( "~" );
                html.AppendFormat( "<header>{0} <h3>{1}<small>{2}</small></h3></header>",
                    Person.GetPhotoImageTag( person.PhotoId, person.Gender, 65, 65 ),
                    person.FullName,
                    person.ConnectionStatusValueId.HasValue ? person.ConnectionStatusValue.Name : string.Empty );

                var spouse = person.GetSpouse();
                if ( spouse != null )
                {
                    html.AppendFormat( "<strong>Spouse</strong> {0}",
                        spouse.LastName == person.LastName ? spouse.FirstName : spouse.FullName );
                }

                int? age = person.Age;
                if ( age.HasValue )
                {
                    html.AppendFormat( "<br/><strong>Age</strong> {0}", age );
                }

                if ( !string.IsNullOrWhiteSpace( person.Email ) )
                {
                    html.AppendFormat( "<br/><strong>Email</strong> <a href='mailto:{0}'>{0}</a>", person.Email );
                }

                foreach ( var phoneNumber in person.PhoneNumbers.Where( n => n.IsUnlisted == false ).OrderBy( n => n.NumberTypeValue.Order ) )
                {
                    html.AppendFormat( "<br/><strong>{0}</strong> {1}", phoneNumber.NumberTypeValue.Name, phoneNumber.NumberFormatted );
                }

                // TODO: Should also show area: <br /><strong>Area</strong> WestwingS

                result.PickerItemDetailsHtml = html.ToString();
            }

            return result;
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
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the picker item details HTML.
        /// </summary>
        /// <value>
        /// The picker item details HTML.
        /// </value>
        public string PickerItemDetailsHtml { get; set; }
    }
}
