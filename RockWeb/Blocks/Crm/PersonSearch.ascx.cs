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
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI.Controls;
using System.Text.RegularExpressions;
using System.Data.Entity.SqlServer;
using Rock.Data;
using Rock.Web.Cache;
using System.Diagnostics;
using System.Data.Entity.Core.Objects;
using System.Text;

namespace RockWeb.Blocks.Crm
{
    [DisplayName( "Person Search" )]
    [Category( "CRM" )]
    [Description( "Displays list of people that match a given search type and term." )]

    [LinkedPage("Person Detail Page")]
    [BooleanField("Show Performance", "Displays how long the search took.", false)]
    public partial class PersonSearch : Rock.Web.UI.RockBlock
    {
        #region Fields

        private DefinedValueCache _inactiveStatus = null;
        private Stopwatch _sw = new Stopwatch();
        private Literal _lPerf = new Literal();
        #endregion

        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            _sw.Start();
            
            base.OnInit( e );

            RockPage.AddScriptLink( ResolveRockUrl( "~/Scripts/jquery.lazyload.min.js" ) );

            gPeople.DataKeyNames = new string[] { "Id" };
            gPeople.Actions.ShowAdd = false;
            gPeople.GridRebind += gPeople_GridRebind;
            gPeople.RowDataBound += gPeople_RowDataBound;
            gPeople.PersonIdField = "Id";

            if ( GetAttributeValue( "ShowPerformance" ).AsBoolean() )
            {
                gPeople.Actions.AddCustomActionControl( _lPerf );
            }
        }

        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );
            _sw.Stop();

            
            _lPerf.Text = string.Format( "<small class='pull-left' style='margin-top: 6px;'>Search time: {0} ms.</small>", _sw.Elapsed.Milliseconds );
            
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                BindGrid();
            }
        }

        #endregion

        #region Events

        void gPeople_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        void gPeople_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var person = e.Row.DataItem as PersonSearchResult;
                if ( person != null )
                {
                    if (_inactiveStatus != null && 
                        person.RecordStatusValueId.HasValue && 
                        person.RecordStatusValueId.Value == _inactiveStatus.Id)
                    {
                        e.Row.AddCssClass( "inactive" );
                    }

                    if ( person.IsDeceased ?? false )
                    {
                        e.Row.AddCssClass( "deceased" );
                    }

                    string delimitedCampuses = string.Empty;
                    if ( person.CampusIds.Any() )
                    {
                        var campuses = new List<string>();
                        foreach ( var campusId in person.CampusIds )
                        {
                            var campus = CampusCache.Read( campusId );
                            if ( campus != null )
                            {
                                campuses.Add( campus.Name );
                            }
                        }
                        if ( campuses.Any() )
                        {
                            delimitedCampuses = campuses.AsDelimited( ", " );
                            var lCampus = e.Row.FindControl( "lCampus" ) as Literal;
                            if ( lCampus != null )
                            {
                                lCampus.Text = delimitedCampuses;
                            }
                        }
                    }

                    var lPerson = e.Row.FindControl( "lPerson" ) as Literal;

                    if ( !person.IsBusiness )
                    {
                        StringBuilder sbPersonDetails = new StringBuilder();
                        sbPersonDetails.Append( string.Format( "<div class=\"photo-round photo-round-sm pull-left\" data-original=\"{0}&w=100\" style=\"background-image: url('{1}');\"></div>", person.PhotoUrl, ResolveUrl("~/Assets/Images/person-no-photo-male.svg") ) );
                        sbPersonDetails.Append("<div class=\"pull-left margin-l-sm\">");
                        sbPersonDetails.Append(string.Format("<strong>{0}</strong> ", person.FullNameReversed));
                        sbPersonDetails.Append( string.Format( "<small class=\"hidden-sm hidden-md hidden-lg\"><br>{0}</br></small>", delimitedCampuses ) );
                        sbPersonDetails.Append( string.Format( "<small class=\"hidden-sm hidden-md hidden-lg\">{0}</small>", DefinedValueCache.GetName( person.ConnectionStatusValueId ) ) );
                        sbPersonDetails.Append(string.Format(" <small class=\"hidden-md hidden-lg\">{0}</small>", person.AgeFormatted));
                        if (!string.IsNullOrWhiteSpace(person.Email)){
                            sbPersonDetails.Append(string.Format("<br/><small>{0}</small>", person.Email));
                        }
                        
                        // add home addresses
                        foreach(var location in person.HomeAddresses )
                        {
                            if ( string.IsNullOrWhiteSpace( location.Street1 ) &&
                                string.IsNullOrWhiteSpace( location.Street2 ) &&
                                string.IsNullOrWhiteSpace( location.City ) )
                            {
                                continue;
                            }

                            string format = string.Empty;
                            var countryValue = Rock.Web.Cache.DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.LOCATION_COUNTRIES.AsGuid() )
                                .DefinedValues
                                .Where( v => v.Value.Equals( location.Country, StringComparison.OrdinalIgnoreCase ) )
                                .FirstOrDefault();

                            if ( countryValue != null )
                            {
                                format = countryValue.GetAttributeValue( "AddressFormat" );
                            }

                            if ( !string.IsNullOrWhiteSpace( format ) )
                            {
                                var dict = location.ToDictionary();
                                dict["Country"] = countryValue.Description;
                                sbPersonDetails.Append( string.Format( "<small><br>{0}</small>", format.ResolveMergeFields( dict ).ConvertCrLfToHtmlBr().Replace( "<br/><br/>", "<br/>" ) ) );
                            }
                            else
                            {
                                sbPersonDetails.Append( string.Format( string.Format( "<small><br>{0}<br>{1} {2}, {3} {4}</small>", location.Street1, location.Street2, location.City, location.City, location.PostalCode ) ) );
                            }
                        }
                        sbPersonDetails.Append("</div>");

                        lPerson.Text = sbPersonDetails.ToString();
                    }
                    else
                    {
                        lPerson.Text = string.Format( "{0}", person.LastName );
                    }
                }
            }
        }

        protected void gPeople_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "PersonDetailPage", "PersonId", (int)e.RowKeyId );
        }

        #endregion

        #region Internal Methods

        private void BindGrid()
        {

            string type = PageParameter( "SearchType" );
            string term = PageParameter( "SearchTerm" );

            if ( !String.IsNullOrWhiteSpace( type ) && !String.IsNullOrWhiteSpace( term ) )
            {
                var rockContext = new RockContext();

                var personService = new PersonService( rockContext );
                IQueryable<Person> people = null;

                switch ( type.ToLower() )
                {
                    case ( "name" ):
                        {
                            bool allowFirstNameOnly = false;
                            if ( !bool.TryParse( PageParameter( "allowFirstNameOnly" ), out allowFirstNameOnly ) )
                            {
                                allowFirstNameOnly = false;
                            }
                            people = personService.GetByFullName( term, allowFirstNameOnly, true );
                            break;
                        }
                    case ( "phone" ):
                        {
                            var phoneService = new PhoneNumberService( rockContext );
                            var personIds = phoneService.GetPersonIdsByNumber( term );
                            people = personService.Queryable().Where( p => personIds.Contains( p.Id ) );
                            break;
                        }
                    case ( "address" ):
                        {
                            var groupMemberService = new GroupMemberService( rockContext );
                            var personIds2 = groupMemberService.GetPersonIdsByHomeAddress( term );
                            people = personService.Queryable().Where( p => personIds2.Contains( p.Id ) );
                            break;
                        }
                    case ( "email" ):
                        {
                            people = personService.Queryable().Where( p => p.Email.Contains( term ) );
                            break;
                        }
                }

                SortProperty sortProperty = gPeople.SortProperty;
                if ( sortProperty != null )
                {
                    people = people.Sort( sortProperty );
                }
                else
                {
                    people = people.OrderBy( p => p.LastName ).ThenBy( p => p.FirstName );
                }

                Guid familyGuid = new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
                Guid homeAddressTypeGuid = new Guid( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME );

                var personList = people.Select( p => new PersonSearchResult
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    NickName = p.NickName,
                    LastName = p.LastName,
                    BirthDate = p.BirthDate,
                    BirthYear = p.BirthYear,
                    BirthMonth = p.BirthMonth,
                    BirthDay = p.BirthDay,
                    ConnectionStatusValueId = p.ConnectionStatusValueId,
                    RecordStatusValueId = p.RecordStatusValueId,
                    RecordTypeValueId = p.RecordTypeValueId,
                    SuffixValueId = p.SuffixValueId,
                    IsDeceased = p.IsDeceased,
                    Email = p.Email,
                    Gender = p.Gender,
                    PhotoId = p.PhotoId,
                    CampusIds = p.Members
                        .Where( m =>
                            m.Group.GroupType.Guid.Equals( familyGuid ) &&
                            m.Group.CampusId.HasValue )
                        .Select( m => m.Group.CampusId.Value )
                        .ToList(),
                    HomeAddresses = p.Members
                        .Where( m => m.Group.GroupType.Guid == familyGuid )
                        .SelectMany( m => m.Group.GroupLocations )
                        .Where( gl => gl.GroupLocationTypeValue.Guid.Equals( homeAddressTypeGuid ) )
                        .Select( gl => gl.Location )
                } ).ToList();

                if ( personList.Count == 1 )
                {
                    Response.Redirect( string.Format( "~/Person/{0}", personList[0].Id ), false );
                    Context.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    if ( type.ToLower() == "name" )
                    {
                        var similiarNames = personService.GetSimiliarNames( term,
                            personList.Select( p => p.Id ).ToList(), true );
                        if ( similiarNames.Any() )
                        {
                            var hyperlinks = new List<string>();
                            foreach ( string name in similiarNames.Distinct() )
                            {
                                var pageRef = CurrentPageReference;
                                pageRef.Parameters["SearchTerm"] = name;
                                hyperlinks.Add( string.Format( "<a href='{0}'>{1}</a>", pageRef.BuildUrl(), name ) );
                            }
                            string altNames = string.Join( ", ", hyperlinks );
                            nbNotice.Text = string.Format( "Other Possible Matches: {0}", altNames );
                            nbNotice.Visible = true;
                        }
                    }

                    _inactiveStatus = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE );

                    gPeople.DataSource = personList;
                    gPeople.DataBind();
                }
            }
        }

        #endregion

}
    #region result models
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
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the name of the nick.
        /// </summary>
        /// <value>
        /// The name of the nick.
        /// </value>
        public string NickName { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is business.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is business; otherwise, <c>false</c>.
        /// </value>
        public bool IsBusiness
        {
            get
            {
                int recordTypeValueIdBusiness = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;
                return this.RecordTypeValueId.HasValue && this.RecordTypeValueId.Value == recordTypeValueIdBusiness;
            }
        }

        /// <summary>
        /// Gets or sets the home addresses.
        /// </summary>
        /// <value>
        /// The home addresses.
        /// </value>
        public IEnumerable<Location> HomeAddresses { get; set; }

        /// <summary>
        /// Gets the photo URL.
        /// </summary>
        /// <value>
        /// The photo URL.
        /// </value>
        public string PhotoUrl
        {
            get
            {
                if ( RecordTypeValueId.HasValue )
                {
                    var recordType = DefinedValueCache.Read( RecordTypeValueId.Value );
                    if ( recordType != null )
                    {
                        return Person.GetPhotoUrl( this.PhotoId, this.Age, this.Gender, recordType.Guid );
                    }
                }
                return Person.GetPhotoUrl( this.PhotoId, this.Age, this.Gender );
            }
            private set { }
        }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        public string LastName { get; set; }

        /// <summary>
        /// Gets the full name reversed.
        /// </summary>
        /// <value>
        /// The full name reversed.
        /// </value>
        public virtual string FullNameReversed
        {
            get
            {
                if ( this.IsBusiness )
                {
                    return LastName;
                }

                var fullName = new StringBuilder();

                fullName.Append( LastName );

                // Use the SuffixValueId and DefinedValue cache instead of referencing SuffixValue property so 
                // that if FullName is used in datagrid, the SuffixValue is not lazy-loaded for each row
                if ( SuffixValueId.HasValue )
                {
                    var suffix = DefinedValueCache.GetName( SuffixValueId.Value );
                    if ( suffix != null )
                    {
                        fullName.AppendFormat( " {0}", suffix );
                    }
                }

                fullName.AppendFormat( ", {0}", NickName );
                return fullName.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the birth date.
        /// </summary>
        /// <value>
        /// The birth date.
        /// </value>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// Gets or sets the birth year.
        /// </summary>
        /// <value>
        /// The birth year.
        /// </value>
        public int? BirthYear { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the photo identifier.
        /// </summary>
        /// <value>
        /// The photo identifier.
        /// </value>
        public int? PhotoId { get; set; }

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
        /// Gets or sets the families.
        /// </summary>
        /// <value>
        /// The families.
        /// </value>
        public List<int> CampusIds { get; set; }

        /// <summary>
        /// Gets or sets the gender.
        /// </summary>
        /// <value>The gender.</value>
        public Gender Gender { get; set; }

        /// <summary>
        /// Gets or sets the is deceased.
        /// </summary>
        /// <value>
        /// The is deceased.
        /// </value>
        public bool? IsDeceased { get; set; }

        /// <summary>
        /// Gets the age.
        /// </summary>
        /// <value>
        /// The age.
        /// </value>
        public int? Age
        {
            get
            {
                if ( BirthYear.HasValue )
                {
                    DateTime? bd = BirthDate;
                    if ( bd.HasValue )
                    {
                        DateTime today = RockDateTime.Today;
                        int age = today.Year - bd.Value.Year;
                        if ( bd.Value > today.AddYears( -age ) ) age--;
                        return age;
                    }
                }
                return null;
            }
            private set { }
        }

        /// <summary>
        /// Gets the age formatted.
        /// </summary>
        /// <value>
        /// The age formatted.
        /// </value>
        public string AgeFormatted
        {
            get
            {
                if ( this.Age.HasValue )
                {
                    return string.Format( "({0})", this.Age.Value.ToString() );
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the connection status.
        /// </summary>
        /// <value>The connection status.</value>
        public int? ConnectionStatusValueId { get; set; }

        /// <summary>
        /// Gets or sets the record type value.
        /// </summary>
        /// <value>
        /// The record type value.
        /// </value>
        public int? RecordTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the suffix value.
        /// </summary>
        /// <value>
        /// The suffix value.
        /// </value>
        public int? SuffixValueId { get; set; }

        /// <summary>
        /// Gets or sets the record status.
        /// </summary>
        /// <value>The member status.</value>
        public int? RecordStatusValueId { get; set; }

        /// <summary>
        /// Gets or sets the name of the spouse.
        /// </summary>
        /// <value>
        /// The name of the spouse.
        /// </value>
        public string SpouseName { get; set; }

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
#endregion
}