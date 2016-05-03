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
using System.ComponentModel;
using System.Web.UI.WebControls;
using System.IO;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Data;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Web.Cache;
using System.Text;
using Newtonsoft.Json;

namespace RockWeb.Plugins.com_centralaz.Widgets
{

    [DisplayName( "Search Attribute Find Person" )]
    [Category( "com_centralaz > Widgets" )]
    [Description( "Allows a user to search for a person based on an attribute." )]

    [SecurityAction( Authorization.EDIT, "The roles and/or users that can edit the HTML content." )]
    [SecurityAction( Authorization.APPROVE, "The roles and/or users that have access to approve HTML content." )]

    [CustomDropdownListField( "Attribute", "The attribute that the search is performed on.", "", true, "", "CustomSetting" )]
    [LinkedPage( "Person Detail Page",  "The page to go to for a person.")]
    public partial class SearchAttributeFindPerson : RockBlockCustomSettings
    {
        #region Fields

        private DefinedValueCache _inactiveStatus = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the available attribute.
        /// </summary>
        /// <value>
        /// The available attribute.
        /// </value>
        public AttributeCache AvailableAttribute { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            AvailableAttribute = ViewState["AvailableAttribute"] as AttributeCache;

            AddDynamicControl();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {

            base.OnInit( e );

            RockPage.AddScriptLink( ResolveRockUrl( "~/Scripts/jquery.lazyload.min.js" ) );

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            gPeople.DataKeyNames = new string[] { "Id" };
            gPeople.Actions.ShowAdd = false;
            gPeople.GridRebind += gPeople_GridRebind;
            gPeople.RowDataBound += gPeople_RowDataBound;
            gPeople.PersonIdField = "Id";

            int? attributeId = GetAttributeValue( "Attribute" ).AsIntegerOrNull();
            if ( !attributeId.HasValue )
            {
                nbConfiguration.Text = "This block is not yet configured";
                nbConfiguration.Visible = true;
            }
            else
            {
                nbConfiguration.Visible = false;
            }

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlSearch );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            if ( !this.IsPostBack )
            {
                ShowView();
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["AvailableAttribute"] = AvailableAttribute;

            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            int? attributeId = GetAttributeValue( "Attribute" ).AsIntegerOrNull();
            if ( !attributeId.HasValue )
            {
                nbConfiguration.Text = "This block is not yet configured";
                nbConfiguration.Visible = true;
            }
            else
            {
                nbConfiguration.Visible = false;
            }
            upnlSearch.Update();
            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the lbOk control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbOk_Click( object sender, EventArgs e )
        {
            SetAttributeValue( "Attribute", ddlAttribute.SelectedValue );
            SaveAttributeValues();
            rFilter.DeleteUserPreferences();
            ShowView();
        }

        /// <summary>
        /// Handles the GridRebind event of the gPeople control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void gPeople_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gPeople control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        void gPeople_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var person = e.Row.DataItem as PersonSearchResult;
                if ( person != null )
                {
                    if ( _inactiveStatus != null &&
                        person.RecordStatusValueId.HasValue &&
                        person.RecordStatusValueId.Value == _inactiveStatus.Id )
                    {
                        e.Row.AddCssClass( "inactive" );
                    }

                    if ( person.IsDeceased )
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
                        sbPersonDetails.Append( string.Format( "<div class=\"photo-round photo-round-sm pull-left\" data-original=\"{0}&w=100\" style=\"background-image: url('{1}');\"></div>", person.PhotoUrl, ResolveUrl( "~/Assets/Images/person-no-photo-male.svg" ) ) );
                        sbPersonDetails.Append( "<div class=\"pull-left margin-l-sm\">" );
                        sbPersonDetails.Append( string.Format( "<strong>{0}</strong> ", person.FullNameReversed ) );
                        sbPersonDetails.Append( string.Format( "<small class=\"hidden-sm hidden-md hidden-lg\"><br>{0}</br></small>", delimitedCampuses ) );
                        sbPersonDetails.Append( string.Format( "<small class=\"hidden-sm hidden-md hidden-lg\">{0}</small>", DefinedValueCache.GetName( person.ConnectionStatusValueId ) ) );
                        sbPersonDetails.Append( string.Format( " <small class=\"hidden-md hidden-lg\">{0}</small>", person.AgeFormatted ) );
                        if ( !string.IsNullOrWhiteSpace( person.Email ) )
                        {
                            sbPersonDetails.Append( string.Format( "<br/><small>{0}</small>", person.Email ) );
                        }

                        // add home addresses
                        foreach ( var location in person.HomeAddresses )
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
                                sbPersonDetails.Append( string.Format( string.Format( "<small><br>{0}<br>{1} {2}, {3} {4}</small>", location.Street1, location.Street2, location.City, location.State, location.PostalCode ) ) );
                            }
                        }
                        sbPersonDetails.Append( "</div>" );

                        lPerson.Text = sbPersonDetails.ToString();
                    }
                    else
                    {
                        lPerson.Text = string.Format( "{0}", person.LastName );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the gPeople control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gPeople_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "PersonDetailPage", "PersonId", (int)e.RowKeyId );
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            if ( AvailableAttribute != null )
            {
                var filterControl = phAttributeFilter.FindControl( "filter_" + AvailableAttribute.Id.ToString() );
                if ( filterControl != null )
                {
                    try
                    {
                        var values = AvailableAttribute.FieldType.Field.GetFilterValues( filterControl, AvailableAttribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
                        rFilter.SaveUserPreference( AvailableAttribute.Key, AvailableAttribute.Name, AvailableAttribute.FieldType.Field.GetFilterValues( filterControl, AvailableAttribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter ).ToJson() );
                    }
                    catch
                    {
                        // intentionally ignore
                    }
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            if ( AvailableAttribute != null )
            {
                if ( e.Key == AvailableAttribute.Key )
                {
                    try
                    {
                        var values = JsonConvert.DeserializeObject<List<string>>( e.Value );
                        e.Value = AvailableAttribute.FieldType.Field.FormatFilterValues( AvailableAttribute.QualifierValues, values );
                        return;
                    }
                    catch
                    {
                        // intentionally ignore
                    }
                }
            }

            e.Value = string.Empty;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the settings.
        /// </summary>
        protected override void ShowSettings()
        {
            var rockContext = new RockContext();
            var attributeService = new AttributeService( rockContext );
            var entityType = new EntityTypeService( rockContext ).Get( Rock.SystemGuid.EntityType.PERSON.AsGuid() );
            if ( entityType != null )
            {
                ddlAttribute.Items.Clear();
                var personAttributeList = attributeService.Queryable()
                    .Where( a =>
                            a.EntityTypeId == entityType.Id );
                foreach ( var attribute in personAttributeList.ToList() )
                {
                    ddlAttribute.Items.Add( new ListItem( attribute.Name, attribute.Id.ToString() ) );
                }

                string currentAttributeIdAttribute = GetAttributeValue( "Attribute" ) ?? string.Empty;
                foreach ( ListItem item in ddlAttribute.Items )
                {
                    item.Selected = ( item.Value == currentAttributeIdAttribute );
                }

                pnlEdit.Visible = true;
                upnlSearch.Update();
                mdEdit.Show();
            }
        }

        /// <summary>
        /// Shows the view.
        /// </summary>
        protected void ShowView()
        {
            mdEdit.Hide();
            pnlEdit.Visible = false;
            upnlSearch.Update();

            pnlEdit.Visible = false;

            SetFilter();
            BindGrid();
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void SetFilter()
        {
            BindAttribute();
            AddDynamicControl();
        }

        /// <summary>
        /// Adds the dynamic control.
        /// </summary>
        private void AddDynamicControl()
        {
            // Clear the filter controls
            phAttributeFilter.Controls.Clear();

            if ( AvailableAttribute != null )
            {
                var control = AvailableAttribute.FieldType.Field.FilterControl( AvailableAttribute.QualifierValues, "filter_" + AvailableAttribute.Id.ToString(), false, Rock.Reporting.FilterMode.SimpleFilter );
                if ( control != null )
                {
                    if ( control is IRockControl )
                    {
                        var rockControl = (IRockControl)control;
                        rockControl.Label = AvailableAttribute.Name;
                        rockControl.Help = AvailableAttribute.Description;
                        phAttributeFilter.Controls.Add( control );
                    }
                    else
                    {
                        var wrapper = new RockControlWrapper();
                        wrapper.ID = control.ID + "_wrapper";
                        wrapper.Label = AvailableAttribute.Name;
                        wrapper.Controls.Add( control );
                        phAttributeFilter.Controls.Add( wrapper );
                    }

                    string savedValue = rFilter.GetUserPreference( AvailableAttribute.Key );
                    if ( !string.IsNullOrWhiteSpace( savedValue ) )
                    {
                        try
                        {
                            var values = JsonConvert.DeserializeObject<List<string>>( savedValue );
                            AvailableAttribute.FieldType.Field.SetFilterValues( control, AvailableAttribute.QualifierValues, values );
                        }
                        catch
                        {
                            // intentionally ignore
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Binds the attribute.
        /// </summary>
        private void BindAttribute()
        {
            // Parse the attribute filter
            var attributeId = GetAttributeValue( "Attribute" ).AsIntegerOrNull();
            if ( attributeId != null )
            {
                var attributeModel = new AttributeService( new RockContext() ).Get( attributeId.Value );
                if ( attributeModel != null )
                {
                    AvailableAttribute = AttributeCache.Read( attributeModel );
                }
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            IQueryable<Person> people = null;
            people = personService.Queryable();

            if ( AvailableAttribute != null )
            {
                lHeading.Text = string.Format( "Search People By {0}", AvailableAttribute.Name );

                var attributeValueService = new AttributeValueService( rockContext );
                var parameterExpression = attributeValueService.ParameterExpression;

                var filterControl = phAttributeFilter.FindControl( "filter_" + AvailableAttribute.Id.ToString() );
                if ( filterControl != null )
                {
                    var filterValues = AvailableAttribute.FieldType.Field.GetFilterValues( filterControl, AvailableAttribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
                    var expression = AvailableAttribute.FieldType.Field.AttributeFilterExpression( AvailableAttribute.QualifierValues, filterValues, parameterExpression );
                    if ( expression != null )
                    {
                        var attributeValues = attributeValueService
                            .Queryable()
                            .Where( v => v.Attribute.Id == AvailableAttribute.Id );

                        attributeValues = attributeValues.Where( parameterExpression, expression, null );

                        people = people.Where( w => attributeValues.Select( v => v.EntityId ).Contains( w.Id ) );

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

                        _inactiveStatus = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE );
                        gPeople.EntityTypeId = EntityTypeCache.GetId<Person>();
                        gPeople.DataSource = personList;
                        gPeople.DataBind();
                    }
                }
            }
           
            upnlSearch.Update();
        }

        #endregion
    }

    #region Helper Classes

    /// <summary>
    /// A Person Search Result
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
                        return Person.GetPersonPhotoUrl(this.Id, this.PhotoId, this.Age, this.Gender, recordType.Guid );
                    }
                }
                return Person.GetPersonPhotoUrl( this.Id, this.PhotoId, this.Age, this.Gender, null );
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