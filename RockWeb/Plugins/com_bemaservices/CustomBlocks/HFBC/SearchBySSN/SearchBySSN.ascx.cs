// <copyright>
// Copyright by BEMA Information Technologies
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
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Field.Types;
using Rock.Model;
using Rock.Rest.Controllers;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RockWeb.Plugins.com_bemadev.SearchBySSN
{
    [DisplayName("Search By Social Security Number")]
    [Category("BemaDev")]
	[LinkedPage( "Detail Page", "Page used to view details")]
    [Description("Provides a convenient way to search by SSN or SocialSecurity numbers.")]
    public partial class SearchBySSN : Rock.Web.UI.RockBlock
    {
        #region Fields

        private List<Guid> _phoneTypeGuids = new List<Guid>();
        private bool _showSpouse = false;
        private DefinedValueCache _inactiveStatus = null;
        private Stopwatch _sw = new Stopwatch();
        private Literal _lPerf = new Literal();
        private Dictionary<int, string> _envelopeNumbers = null;

        #endregion

        #region Events

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            RockPage.AddScriptLink(ResolveRockUrl("~/Scripts/jquery.lazyload.min.js"));
            
            gPeople.DataKeyNames = new string[] { "Id" };
            gPeople.Actions.ShowAdd = false;
            gPeople.GridRebind += gPeople_GridRebind;
            gPeople.RowDataBound += gPeople_RowDataBound;
            gPeople.PersonIdField = "Id";

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger(upnlContent);
        }

        /**
         *  Block updated event handler
         */
        protected void Block_BlockUpdated(object sender, EventArgs e)
        {
            BindGrid();
        }

        /**
         * OnLoad.  
         */
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                BindGrid();
            }
        }

        /// <summary>
        /// Provides a search by Social Security Number
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void rbb_SearchBySSN_Click(object sender, EventArgs e)
        {
            var data = inputbox_ssn.Text;
            List<int> personEntityIdList = new List<int>();

            // Pull out all of the encrypted values from the AttributeValues where the attribute is a social security number
            using (var context = new RockContext())
            {
                int ssnAttributeId = GetSSNAttributeId(context);

                if (ssnAttributeId > 0)
                {
                    AttributeValueService attributeValueService = new AttributeValueService(context);
                    List<AttributeValue> attributeValueList = attributeValueService.GetByAttributeId(ssnAttributeId).ToList();

                    // Todo Run through list, and unencrypt the data.
                    foreach (AttributeValue attributeValue in attributeValueList)
                    {
                        string value = attributeValue.Value;

                        var unEncryptedText = SSNFieldType.UnencryptAndClean(value);

                        try
                        {
                            var x = unEncryptedText.Trim(new Char[] { ' ', '-' });

                            if (x.Substring(x.Length - 4).Equals(data) || x.Substring(x.Length - 5).Equals(data))
                            {
                                personEntityIdList.Add(attributeValue.EntityId.GetValueOrDefault());
                            }

                        }
                        catch (Exception ex)
                        {
                            // Ignore
                        }
                    }

                }
                else
                {
                    // todo display error
                }

                // Build the Grid
                BindGrid(personEntityIdList, context);
            }
        }

        void gPeople_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var person = e.Row.DataItem as PersonSearchResult;
                if (person != null)
                {
                    if (_inactiveStatus != null &&
                        person.RecordStatusValueId.HasValue &&
                        person.RecordStatusValueId.Value == _inactiveStatus.Id)
                    {
                        e.Row.AddCssClass("inactive");
                    }

                    if (person.IsDeceased)
                    {
                        e.Row.AddCssClass("deceased");
                    }

                    string delimitedCampuses = string.Empty;
                    if (person.CampusIds.Any())
                    {
                        var campuses = new List<string>();
                        foreach (var campusId in person.CampusIds)
                        {
                            var campus = CampusCache.Read(campusId);
                            if (campus != null)
                            {
                                campuses.Add(campus.Name);
                            }
                        }
                        if (campuses.Any())
                        {
                            delimitedCampuses = campuses.AsDelimited(", ");
                            var lCampus = e.Row.FindControl("lCampus") as Literal;
                            if (lCampus != null)
                            {
                                lCampus.Text = delimitedCampuses;
                            }
                        }
                    }

                    var lPerson = e.Row.FindControl("lPerson") as Literal;

                    if (!person.IsBusiness)
                    {
                        StringBuilder sbPersonDetails = new StringBuilder();
                        sbPersonDetails.Append(string.Format("<div class=\"photo-round photo-round-sm pull-left\" data-original=\"{0}&w=100\" style=\"background-image: url('{1}');\"></div>", person.PhotoUrl, ResolveUrl("~/Assets/Images/person-no-photo-male.svg")));
                        sbPersonDetails.Append("<div class=\"pull-left margin-l-sm\">");
                        sbPersonDetails.Append(string.Format("<strong>{0}</strong> ", person.FullNameReversed));
                        sbPersonDetails.Append(string.Format("<small class=\"hidden-sm hidden-md hidden-lg\"><br>{0}</br></small>", delimitedCampuses));
                        sbPersonDetails.Append(string.Format("<small class=\"hidden-sm hidden-md hidden-lg\">{0}</small>", DefinedValueCache.GetName(person.ConnectionStatusValueId)));
                        sbPersonDetails.Append(string.Format(" <small class=\"hidden-md hidden-lg\">{0}</small>", person.AgeFormatted));

                        foreach (Guid phGuid in _phoneTypeGuids)
                        {
                            var dv = DefinedValueCache.Read(phGuid);
                            if (dv != null)
                            {
                                var pn = person.PhoneNumbers.FirstOrDefault(n => n.NumberTypeValueId == dv.Id);
                                if (pn != null)
                                {
                                    sbPersonDetails.Append(string.Format("<br/><small>{0}: {1}</small>", dv.Value.Left(1).ToUpper(), pn.Number));
                                }
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(person.Email))
                        {
                            sbPersonDetails.Append(string.Format("<br/><small>{0}</small>", person.Email));
                        }

                        // add home addresses
                        foreach (var location in person.HomeAddresses)
                        {
                            if (string.IsNullOrWhiteSpace(location.Street1) &&
                                string.IsNullOrWhiteSpace(location.Street2) &&
                                string.IsNullOrWhiteSpace(location.City))
                            {
                                continue;
                            }

                            string format = string.Empty;
                            var countryValue = Rock.Web.Cache.DefinedTypeCache.Read(Rock.SystemGuid.DefinedType.LOCATION_COUNTRIES.AsGuid())
                                .DefinedValues
                                .Where(v => v.Value.Equals(location.Country, StringComparison.OrdinalIgnoreCase))
                                .FirstOrDefault();

                            if (countryValue != null)
                            {
                                format = countryValue.GetAttributeValue("AddressFormat");
                            }

                            if (!string.IsNullOrWhiteSpace(format))
                            {
                                var dict = location.ToDictionary();
                                dict["Country"] = countryValue.Description;
                                sbPersonDetails.Append(string.Format("<small><br>{0}</small>", format.ResolveMergeFields(dict).ConvertCrLfToHtmlBr().Replace("<br/><br/>", "<br/>")));
                            }
                            else
                            {
                                sbPersonDetails.Append(string.Format(string.Format("<small><br>{0}<br>{1} {2}, {3} {4}</small>", location.Street1, location.Street2, location.City, location.State, location.PostalCode)));
                            }
                        }
                        sbPersonDetails.Append("</div>");

                        lPerson.Text = sbPersonDetails.ToString();

                        if (_showSpouse)
                        {
                            using (var rockContext = new RockContext())
                            {
                                var personRec = new PersonService(rockContext).Get(person.Id);
                                if (personRec != null)
                                {
                                    var lSpouse = e.Row.FindControl("lSpouse") as Literal;
                                    var spouse = personRec.GetSpouse(rockContext);
                                    if (lSpouse != null && spouse != null)
                                    {
                                        lSpouse.Text = spouse.FullName;
                                    }
                                }
                            }
                        }

                        if (_envelopeNumbers != null && _envelopeNumbers.ContainsKey(person.Id))
                        {
                            var lEnvelopeNumber = e.Row.FindControl("lEnvelopeNumber") as Literal;
                            if (lEnvelopeNumber != null)
                            {
                                lEnvelopeNumber.Text = _envelopeNumbers[person.Id];
                            }
                        }
                    }
                    else
                    {
                        lPerson.Text = string.Format("{0}", person.LastName);
                    }
                }
            }
        }

        void gPeople_GridRebind(object sender, EventArgs e)
        {
            BindGrid();
        }

        /// <summary>
        /// On row selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gPeople_RowSelected(object sender, Rock.Web.UI.Controls.RowEventArgs e)
        {
            String url = String.Format("/Person/{0}/FaithCenter", (int) e.RowKeyId);

            //http://localhost:6229/Person/3/Benevolence

            String resolvedRockUrl = ResolveRockUrl(url);

            Response.Redirect(url);
        }
		
		protected void lbfStartEntry_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            if( GetAttributeValue( "DetailPage" ).AsGuidOrNull() != null )
            {
                var page = PageCache.Read( GetAttributeValue( "DetailPage" ).AsGuid() );
                String url = String.Format( "/page/{0}?BenevolenceRequestId=0&PersonId={1}", (int) page.Id, ( int ) e.RowKeyId );
                Response.Redirect( url );
            }
            else
            {
                nbNotice.Text = "Block Attribute for Detail Page must be defined";
                nbNotice.Visible = true;
            }
        }
		
        #endregion

        #region Methods

        /// <summary>
        /// Bind the Grid with what was found in the list
        /// </summary>
        /// <param name="personEntityIdList"></param>
        /// <param name="rockContext"></param>
        private void BindGrid(List<int> personEntityIdList = null, RockContext rockContext = null)
        {
            List<PersonSearchResult> personSearchResultList = new List<PersonSearchResult>();

            if (personEntityIdList != null && rockContext != null)
            {
                PersonService personService = new PersonService(rockContext);
                IQueryable<Person> people = null;

                people = personService.Queryable(true).Where(p => personEntityIdList.Contains(p.Id));

                var familyGroupType = GroupTypeCache.GetFamilyGroupType();
                int familyGroupTypeId = familyGroupType != null ? familyGroupType.Id : 0;

                var groupLocationTypeHome = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid());
                int homeAddressTypeId = groupLocationTypeHome != null ? groupLocationTypeHome.Id : 0;

                personSearchResultList = people.Select(p => new PersonSearchResult
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
                        .Where(m =>
                             m.Group.GroupTypeId == familyGroupTypeId &&
                             m.Group.CampusId.HasValue)
                        .Select(m => m.Group.CampusId.Value)
                        .ToList(),
                    HomeAddresses = p.Members
                        .Where(m => m.Group.GroupTypeId == familyGroupTypeId)
                        .SelectMany(m => m.Group.GroupLocations)
                        .Where(gl => gl.GroupLocationTypeValueId == homeAddressTypeId)
                        .Select(gl => gl.Location),
                    PhoneNumbers = p.PhoneNumbers
                        .Where(n => n.NumberTypeValueId.HasValue)
                        .Select(n => new PersonSearchResultPhone
                        {
                            NumberTypeValueId = n.NumberTypeValueId.Value,
                            Number = n.NumberFormatted
                        })
                    .ToList()
                }).ToList();

                _inactiveStatus = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE);
                var personIds = personSearchResultList.Select(a => a.Id).ToList();

            }

            gPeople.EntityTypeId = EntityTypeCache.GetId<Person>();

            gPeople.DataSource = personSearchResultList;
            gPeople.DataBind();
        }

        /// <summary>
        /// Get the SSN Attribute Id by the public key
        /// </summary>
        /// <param name="rockContext"></param>
        /// <returns>int</returns>
        private int GetSSNAttributeId(RockContext rockContext)
        {
            int id = 0;

            List<Guid> list = new List<Guid>()
            {
                Guid.Parse("1FAB9A4C-C5A2-4938-B9BD-80935F0A598C"), // Rock SSN
                Guid.Parse("A4B9A300-630A-43C5-9975-9BD6E63FF0FB") // Person Attribute SocialSecurity (Taken from HFBC)
            };

            List<Rock.Model.Attribute> attributeList = new List<Rock.Model.Attribute>();

            AttributeService attributeService = new AttributeService(rockContext);
            attributeList = attributeService.GetByGuids(list).ToList();

            foreach (Rock.Model.Attribute attribute in attributeList)
            {
                id = attribute.Id;
            }

            return id;
        }

        #endregion

        #region Models

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
                    int recordTypeValueIdBusiness = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid()).Id;
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
                    if (RecordTypeValueId.HasValue)
                    {
                        var recordType = DefinedValueCache.Read(RecordTypeValueId.Value);
                        if (recordType != null)
                        {
                            return Person.GetPersonPhotoUrl(this.Id, this.PhotoId, this.Age, this.Gender, recordType.Guid, 200, 200);
                        }
                    }
                    return Person.GetPersonPhotoUrl(this.Id, this.PhotoId, this.Age, this.Gender, null, 200, 200);
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
                    if (this.IsBusiness)
                    {
                        return LastName;
                    }

                    var fullName = new StringBuilder();

                    fullName.Append(LastName);

                    // Use the SuffixValueId and DefinedValue cache instead of referencing SuffixValue property so 
                    // that if FullName is used in datagrid, the SuffixValue is not lazy-loaded for each row
                    if (SuffixValueId.HasValue)
                    {
                        var suffix = DefinedValueCache.GetName(SuffixValueId.Value);
                        if (suffix != null)
                        {
                            fullName.AppendFormat(" {0}", suffix);
                        }
                    }

                    fullName.AppendFormat(", {0}", NickName);
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
            public bool IsDeceased { get; set; }

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
                    if (BirthYear.HasValue)
                    {
                        DateTime? bd = BirthDate;
                        if (bd.HasValue)
                        {
                            DateTime today = RockDateTime.Today;
                            int age = today.Year - bd.Value.Year;
                            if (bd.Value > today.AddYears(-age)) age--;
                            return age;
                        }
                    }
                    return null;
                }
                set { }
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
                    if (this.Age.HasValue)
                    {
                        return string.Format("({0})", this.Age.Value.ToString());
                    }
                    return string.Empty;
                }
                set { }
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

            /// <summary>
            /// Gets or sets the phone numbers.
            /// </summary>
            /// <value>
            /// The phone numbers.
            /// </value>
            public List<PersonSearchResultPhone> PhoneNumbers { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class PersonSearchResultPhone
        {
            /// <summary>
            /// Gets or sets the number type value identifier.
            /// </summary>
            /// <value>
            /// The number type value identifier.
            /// </value>
            public int NumberTypeValueId { get; set; }

            /// <summary>
            /// Gets or sets the number.
            /// </summary>
            /// <value>
            /// The number.
            /// </value>
            public string Number { get; set; }
        }

        #endregion
    }

}