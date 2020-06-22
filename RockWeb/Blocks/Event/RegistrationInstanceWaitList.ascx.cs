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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// Block for editing the wait list associated with an event registration instance.
    /// </summary>
    [DisplayName( "Registration Instance - Wait List" )]
    [Category( "Event" )]
    [Description( "Block for editing the wait list associated with an event registration instance." )]

    #region Block Attributes

    [LinkedPage(
        "Wait List Process Page",
        "The page for moving a person from the wait list to a full registrant.",
        Key = AttributeKey.WaitListProcessingPage,
        DefaultValue = Rock.SystemGuid.Page.REGISTRATION_WAIT_LIST_CONFIRMATION,
        IsRequired = false,
        Order = 1 )]

    [LinkedPage(
        "Registration Page",
        "The page for editing registration and registrant information",
        Key = AttributeKey.RegistrationPage,
        DefaultValue = Rock.SystemGuid.Page.REGISTRATION_DETAIL,
        IsRequired = false,
        Order = 2 )]

    #endregion

    public partial class RegistrationInstanceWaitList : RegistrationInstanceBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys for block attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The linked page used to display registration details.
            /// </summary>
            public const string RegistrationPage = "RegistrationPage";

            /// <summary>
            /// The page for processing a wait list entry.
            /// </summary>
            public const string WaitListProcessingPage = "WaitListProcessingPage";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The Registration Instance identifier
            /// </summary>
            public const string RegistrationInstanceId = "RegistrationInstanceId";

            public const string RegistrationId = "RegistrationId";

            public const string WaitListSetId = "WaitListSetId";
        }

        #endregion Page Parameter Keys

        #region Properties and Fields

        private Dictionary<int, Location> _homeAddresses = new Dictionary<int, Location>();
        private Dictionary<int, PhoneNumber> _mobilePhoneNumbers = new Dictionary<int, PhoneNumber>();
        private Dictionary<int, PhoneNumber> _homePhoneNumbers = new Dictionary<int, PhoneNumber>();
        private List<int> _waitListOrder = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the registrant form fields that were configured as 'Show on Grid' for the registration template
        /// </summary>
        /// <value>
        /// The registrant fields.
        /// </value>
        public RegistrantFormField[] RegistrantFields { get; set; }

        /// <summary>
        /// Gets or sets the person campus ids.
        /// </summary>
        /// <value>
        /// The person campus ids.
        /// </value>
        private Dictionary<int, List<int>> PersonCampusIds { get; set; }

        /// <summary>
        /// Gets or sets the group links.
        /// </summary>
        /// <value>
        /// The group links.
        /// </value>
        private Dictionary<int, string> GroupLinks { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            RegistrantFields = ViewState[ViewStateKeyBase.RegistrantFields] as RegistrantFormField[];

            SetUserPreferencePrefix( RegistrationTemplateId.Value );

            // Don't set the dynamic control values if this is a postback from a grid 'ClearFilter'.
            bool setValues = this.Request.Params["__EVENTTARGET"] == null || !this.Request.Params["__EVENTTARGET"].EndsWith( "_lbClearFilter" );

            AddDynamicControls( setValues );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            fWaitList.ApplyFilterClick += fWaitList_ApplyFilterClick;

            gWaitList.EmptyDataText = "There are no items to show in this view.";
            gWaitList.DataKeyNames = new string[] { "Id" };
            gWaitList.Actions.ShowAdd = true;
            gWaitList.Actions.AddClick += gWaitList_AddClick;
            gWaitList.RowDataBound += gWaitList_RowDataBound;
            gWaitList.GridRebind += gWaitList_GridRebind;

            // add button to the wait list action grid
            Button btnProcessWaitlist = new Button();
            btnProcessWaitlist.CssClass = "pull-left margin-l-none btn btn-sm btn-default";
            btnProcessWaitlist.Text = "Move From Wait List";
            btnProcessWaitlist.Click += btnProcessWaitlist_Click;
            gWaitList.Actions.AddCustomActionControl( btnProcessWaitlist );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetail();
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
            ViewState[ViewStateKeyBase.RegistrantFields] = RegistrantFields;

            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the RowSelected event of the gWaitList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gWaitList_RowSelected( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var registrantService = new RegistrationRegistrantService( rockContext );
                var registrant = registrantService.Get( e.RowKeyId );
                if ( registrant != null )
                {
                    var qryParams = new Dictionary<string, string>();
                    qryParams.Add( PageParameterKey.RegistrationId, registrant.RegistrationId.ToString() );
                    string url = LinkedPageUrl( AttributeKey.RegistrationPage, qryParams );
                    url += "#" + e.RowKeyValue;
                    Response.Redirect( url, false );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnProcessWaitlist control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnProcessWaitlist_Click( object sender, EventArgs e )
        {
            // create entity set with selected individuals
            var keys = gWaitList.SelectedKeys.ToList();
            if ( keys.Any() )
            {
                var entitySet = new Rock.Model.EntitySet();
                entitySet.EntityTypeId = Rock.Web.Cache.EntityTypeCache.Get<Rock.Model.RegistrationRegistrant>().Id;
                entitySet.ExpireDateTime = RockDateTime.Now.AddMinutes( 20 );

                foreach ( var key in keys )
                {
                    try
                    {
                        var item = new Rock.Model.EntitySetItem();
                        item.EntityId = ( int ) key;
                        entitySet.Items.Add( item );
                    }
                    catch
                    {
                        // ignore
                    }
                }

                if ( entitySet.Items.Any() )
                {
                    var rockContext = new RockContext();
                    var service = new Rock.Model.EntitySetService( rockContext );
                    service.Add( entitySet );
                    rockContext.SaveChanges();

                    // redirect to the waitlist page
                    Dictionary<string, string> queryParms = new Dictionary<string, string>();
                    queryParms.Add( PageParameterKey.WaitListSetId, entitySet.Id.ToString() );
                    NavigateToLinkedPage( AttributeKey.WaitListProcessingPage, queryParms );
                }
            }
        }

        /// <summary>
        /// Handles the AddClick event of the gWaitList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gWaitList_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.RegistrationPage, PageParameterKey.RegistrationId, 0, PageParameterKey.RegistrationInstanceId, this.RegistrationInstanceId.GetValueOrDefault() );
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the fWaitList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fWaitList_ApplyFilterClick( object sender, EventArgs e )
        {
            fWaitList.SaveUserPreference( UserPreferenceKeyBase.GridFilter_DateRange, "Date Range", drpWaitListDateRange.DelimitedValues );
            fWaitList.SaveUserPreference( UserPreferenceKeyBase.GridFilter_FirstName, "First Name", tbWaitListFirstName.Text );
            fWaitList.SaveUserPreference( UserPreferenceKeyBase.GridFilter_LastName, "Last Name", tbWaitListLastName.Text );

            if ( RegistrantFields != null )
            {
                foreach ( var field in RegistrantFields )
                {
                    if ( field.FieldSource == RegistrationFieldSource.PersonField && field.PersonFieldType.HasValue )
                    {
                        switch ( field.PersonFieldType.Value )
                        {
                            case RegistrationPersonFieldType.Campus:
                                var ddlCampus = phWaitListFormFieldFilters.FindControl( FILTER_CAMPUS_ID ) as RockDropDownList;
                                if ( ddlCampus != null )
                                {
                                    fWaitList.SaveUserPreference( UserPreferenceKeyBase.GridFilter_HomeCampus, "Home Campus", ddlCampus.SelectedValue );
                                }

                                break;

                            case RegistrationPersonFieldType.Email:
                                var tbEmailFilter = phWaitListFormFieldFilters.FindControl( FILTER_EMAIL_ID ) as RockTextBox;
                                if ( tbEmailFilter != null )
                                {
                                    fWaitList.SaveUserPreference( UserPreferenceKeyBase.GridFilter_Email, "Email", tbEmailFilter.Text );
                                }

                                break;

                            case RegistrationPersonFieldType.Birthdate:
                                var drpBirthdateFilter = phWaitListFormFieldFilters.FindControl( FILTER_BIRTHDATE_ID ) as DateRangePicker;
                                if ( drpBirthdateFilter != null )
                                {
                                    fWaitList.SaveUserPreference( UserPreferenceKeyBase.GridFilter_BirthdateRange, "Birthdate Range", drpBirthdateFilter.DelimitedValues );
                                }

                                break;
                            case RegistrationPersonFieldType.MiddleName:
                                var tbMiddleNameFilter = phWaitListFormFieldFilters.FindControl( FILTER_MIDDLE_NAME_ID ) as RockTextBox;
                                if ( tbMiddleNameFilter != null )
                                {
                                    fWaitList.SaveUserPreference( UserPreferenceKeyBase.GridFilter_MiddleName, "MiddleName", tbMiddleNameFilter.Text );
                                }

                                break;
                            case RegistrationPersonFieldType.AnniversaryDate:
                                var drpAnniversaryDateFilter = phWaitListFormFieldFilters.FindControl( FILTER_ANNIVERSARY_DATE_ID ) as DateRangePicker;
                                if ( drpAnniversaryDateFilter != null )
                                {
                                    fWaitList.SaveUserPreference( UserPreferenceKeyBase.GridFilter_AnniversaryDateRange, "AnniversaryDate Range", drpAnniversaryDateFilter.DelimitedValues );
                                }

                                break;
                            case RegistrationPersonFieldType.Grade:
                                var gpGradeFilter = phWaitListFormFieldFilters.FindControl( FILTER_GRADE_ID ) as GradePicker;
                                if ( gpGradeFilter != null )
                                {
                                    int? gradeOffset = gpGradeFilter.SelectedValueAsInt( false );
                                    fWaitList.SaveUserPreference( UserPreferenceKeyBase.GridFilter_Grade, "Grade", gradeOffset.HasValue ? gradeOffset.Value.ToString() : string.Empty );
                                }

                                break;

                            case RegistrationPersonFieldType.Gender:
                                var ddlGenderFilter = phWaitListFormFieldFilters.FindControl( FILTER_GENDER_ID ) as RockDropDownList;
                                if ( ddlGenderFilter != null )
                                {
                                    fWaitList.SaveUserPreference( UserPreferenceKeyBase.GridFilter_Gender, "Gender", ddlGenderFilter.SelectedValue );
                                }

                                break;

                            case RegistrationPersonFieldType.MaritalStatus:
                                var dvpMaritalStatusFilter = phWaitListFormFieldFilters.FindControl( FILTER_MARTIAL_STATUS_ID ) as DefinedValuePicker;
                                if ( dvpMaritalStatusFilter != null )
                                {
                                    fWaitList.SaveUserPreference( UserPreferenceKeyBase.GridFilter_MaritalStatus, "Marital Status", dvpMaritalStatusFilter.SelectedValue );
                                }

                                break;

                            case RegistrationPersonFieldType.MobilePhone:
                                var tbMobilePhoneFilter = phWaitListFormFieldFilters.FindControl( FILTER_MOBILE_PHONE_ID ) as RockTextBox;
                                if ( tbMobilePhoneFilter != null )
                                {
                                    fWaitList.SaveUserPreference( UserPreferenceKeyBase.GridFilter_CellPhone, "Cell Phone", tbMobilePhoneFilter.Text );
                                }

                                break;

                            case RegistrationPersonFieldType.HomePhone:
                                var tbHomePhoneFilter = phWaitListFormFieldFilters.FindControl( FILTER_HOME_PHONE_ID ) as RockTextBox;
                                if ( tbHomePhoneFilter != null )
                                {
                                    fWaitList.SaveUserPreference( UserPreferenceKeyBase.GridFilter_HomePhone, "Home Phone", tbHomePhoneFilter.Text );
                                }

                                break;
                        }
                    }

                    if ( field.Attribute != null )
                    {
                        var attribute = field.Attribute;
                        var filterControl = phWaitListFormFieldFilters.FindControl( FILTER_ATTRIBUTE_PREFIX + attribute.Id.ToString() );
                        if ( filterControl != null )
                        {
                            try
                            {
                                var values = attribute.FieldType.Field.GetFilterValues( filterControl, field.Attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
                                fWaitList.SaveUserPreference( attribute.Key, attribute.Name, attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter ).ToJson() );
                            }
                            catch
                            {
                                // ignore
                            }
                        }
                    }
                }
            }

            BindWaitListGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the fWaitList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fWaitList_ClearFilterClick( object sender, EventArgs e )
        {
            fWaitList.DeleteUserPreferences();

            foreach ( var control in phWaitListFormFieldFilters.ControlsOfTypeRecursive<Control>().Where( a => a.ID != null && a.ID.StartsWith( "filter" ) && a.ID.Contains( "_" ) ) )
            {
                var attributeId = control.ID.Split( '_' )[1].AsInteger();
                var attribute = AttributeCache.Get( attributeId );
                if ( attribute != null )
                {
                    attribute.FieldType.Field.SetFilterValues( control, attribute.QualifierValues, new List<string>() );
                }
            }

            if ( RegistrantFields != null )
            {
                foreach ( var field in RegistrantFields )
                {
                    if ( field.FieldSource == RegistrationFieldSource.PersonField && field.PersonFieldType.HasValue )
                    {
                        switch ( field.PersonFieldType.Value )
                        {
                            case RegistrationPersonFieldType.Campus:
                                var ddlCampus = phWaitListFormFieldFilters.FindControl( FILTER_CAMPUS_ID ) as RockDropDownList;
                                if ( ddlCampus != null )
                                {
                                    ddlCampus.SetValue( ( Guid? ) null );
                                }

                                break;

                            case RegistrationPersonFieldType.Email:
                                var tbEmailFilter = phWaitListFormFieldFilters.FindControl( FILTER_EMAIL_ID ) as RockTextBox;
                                if ( tbEmailFilter != null )
                                {
                                    tbEmailFilter.Text = string.Empty;
                                }

                                break;

                            case RegistrationPersonFieldType.Birthdate:
                                var drpBirthdateFilter = phWaitListFormFieldFilters.FindControl( FILTER_BIRTHDATE_ID ) as DateRangePicker;
                                if ( drpBirthdateFilter != null )
                                {
                                    drpBirthdateFilter.UpperValue = null;
                                    drpBirthdateFilter.LowerValue = null;
                                }

                                break;
                            case RegistrationPersonFieldType.MiddleName:
                                var tbMiddleNameFilter = phWaitListFormFieldFilters.FindControl( FILTER_MIDDLE_NAME_ID ) as RockTextBox;
                                if ( tbMiddleNameFilter != null )
                                {
                                    tbMiddleNameFilter.Text = string.Empty;
                                }

                                break;
                            case RegistrationPersonFieldType.AnniversaryDate:
                                var drpAnniversaryDateFilter = phWaitListFormFieldFilters.FindControl( FILTER_ANNIVERSARY_DATE_ID ) as DateRangePicker;
                                if ( drpAnniversaryDateFilter != null )
                                {
                                    drpAnniversaryDateFilter.UpperValue = null;
                                    drpAnniversaryDateFilter.LowerValue = null;
                                }

                                break;
                            case RegistrationPersonFieldType.Grade:
                                var gpGradeFilter = phWaitListFormFieldFilters.FindControl( FILTER_GRADE_ID ) as GradePicker;
                                if ( gpGradeFilter != null )
                                {
                                    gpGradeFilter.SetValue( ( Guid? ) null );
                                }

                                break;

                            case RegistrationPersonFieldType.Gender:
                                var ddlGenderFilter = phWaitListFormFieldFilters.FindControl( FILTER_GENDER_ID ) as RockDropDownList;
                                if ( ddlGenderFilter != null )
                                {
                                    ddlGenderFilter.SetValue( ( Guid? ) null );
                                }

                                break;

                            case RegistrationPersonFieldType.MaritalStatus:
                                var dvpMaritalStatusFilter = phWaitListFormFieldFilters.FindControl( FILTER_MARTIAL_STATUS_ID ) as DefinedValuePicker;
                                if ( dvpMaritalStatusFilter != null )
                                {
                                    dvpMaritalStatusFilter.SetValue( ( Guid? ) null );
                                }

                                break;

                            case RegistrationPersonFieldType.MobilePhone:
                                var tbMobilePhoneFilter = phWaitListFormFieldFilters.FindControl( FILTER_MOBILE_PHONE_ID ) as RockTextBox;
                                if ( tbMobilePhoneFilter != null )
                                {
                                    tbMobilePhoneFilter.Text = string.Empty;
                                }

                                break;

                            case RegistrationPersonFieldType.HomePhone:
                                var tbHomePhoneFilter = phWaitListFormFieldFilters.FindControl( FILTER_HOME_PHONE_ID ) as RockTextBox;
                                if ( tbHomePhoneFilter != null )
                                {
                                    tbHomePhoneFilter.Text = string.Empty;
                                }

                                break;
                        }
                    }
                }
            }

            BindWaitListFilter( null );
        }

        /// <summary>
        /// fs the wait list_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void fWaitList_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            var key = e.Key;

            if ( RegistrantFields != null )
            {
                var attribute = RegistrantFields
                    .Where( a =>
                        a.Attribute != null &&
                        a.Attribute.Key == key )
                    .Select( a => a.Attribute )
                    .FirstOrDefault();

                if ( attribute != null )
                {
                    try
                    {
                        var values = JsonConvert.DeserializeObject<List<string>>( e.Value );
                        e.Value = attribute.FieldType.Field.FormatFilterValues( attribute.QualifierValues, values );
                        return;
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }

            switch ( key )
            {
                case "Date Range":
                case "Birthdate Range":
                    // The value might either be from a SlidingDateRangePicker or a DateRangePicker, so try both
                    var storedValue = e.Value;
                    e.Value = SlidingDateRangePicker.FormatDelimitedValues( storedValue );
                    if ( e.Value.IsNullOrWhiteSpace() )
                    {
                        e.Value = DateRangePicker.FormatDelimitedValues( storedValue );
                    }

                    break;

                case "Grade":
                    e.Value = Person.GradeFormattedFromGradeOffset( e.Value.AsIntegerOrNull() );
                    break;

                case "First Name":
                case "Last Name":
                case "Email":
                case "Cell Phone":
                case "Home Phone":
                case "Signed Document":
                    break;

                case "Gender":
                    var gender = e.Value.ConvertToEnumOrNull<Gender>();
                    e.Value = gender.HasValue ? gender.ConvertToString() : string.Empty;
                    break;

                case "Campus":
                    int? campusId = e.Value.AsIntegerOrNull();
                    if ( campusId.HasValue )
                    {
                        var campus = CampusCache.Get( campusId.Value );
                        e.Value = campus != null ? campus.Name : string.Empty;
                    }
                    else
                    {
                        e.Value = string.Empty;
                    }

                    break;

                case "Marital Status":
                    int? dvId = e.Value.AsIntegerOrNull();
                    if ( dvId.HasValue )
                    {
                        var maritalStatus = DefinedValueCache.Get( dvId.Value );
                        e.Value = maritalStatus != null ? maritalStatus.Value : string.Empty;
                    }
                    else
                    {
                        e.Value = string.Empty;
                    }

                    break;

                case "In Group":
                    e.Value = e.Value;
                    break;

                default:
                    e.Value = string.Empty;
                    break;
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gWaitList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        private void gWaitList_GridRebind( object sender, GridRebindEventArgs e )
        {
            string title;

            if ( this.RegistrationInstance == null )
            {
                title = ActionTitle.Add( RegistrationInstance.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                title = this.RegistrationInstance.Name;
            }

            gWaitList.ExportTitleName = title + " - Registration Wait List";
            gWaitList.ExportFilename = gWaitList.ExportFilename ?? title + " - Registration Wait List";

            BindWaitListGrid( e.IsExporting );
        }

        /// <summary>
        /// Handles the RowDataBound event of the gWaitList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        private void gWaitList_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var registrant = e.Row.DataItem as RegistrationRegistrant;
            if ( registrant != null )
            {
                // Set the wait list individual name value
                var lWaitListIndividual = e.Row.FindControl( "lWaitListIndividual" ) as Literal;
                if ( lWaitListIndividual != null )
                {
                    if ( registrant.PersonAlias != null && registrant.PersonAlias.Person != null )
                    {
                        lWaitListIndividual.Text = registrant.PersonAlias.Person.FullNameReversed;
                    }
                    else
                    {
                        lWaitListIndividual.Text = string.Empty;
                    }
                }

                var lWaitListOrder = e.Row.FindControl( "lWaitListOrder" ) as Literal;
                if ( lWaitListOrder != null )
                {
                    lWaitListOrder.Text = ( _waitListOrder.IndexOf( registrant.Id ) + 1 ).ToString();
                }

                // Set the campus
                var lCampus = e.Row.FindControl( "lWaitlistCampus" ) as Literal;
                if ( lCampus != null && PersonCampusIds != null )
                {
                    if ( registrant.PersonAlias != null )
                    {
                        if ( PersonCampusIds.ContainsKey( registrant.PersonAlias.PersonId ) )
                        {
                            var campusIds = PersonCampusIds[registrant.PersonAlias.PersonId];
                            if ( campusIds.Any() )
                            {
                                var campusNames = new List<string>();
                                foreach ( int campusId in campusIds )
                                {
                                    var campus = CampusCache.Get( campusId );
                                    if ( campus != null )
                                    {
                                        campusNames.Add( campus.Name );
                                    }
                                }

                                lCampus.Text = campusNames.AsDelimited( "<br/>" );
                            }
                        }
                    }
                }

                var lAddress = e.Row.FindControl( "lWaitlistAddress" ) as Literal;
                if ( lAddress != null && _homeAddresses.Count() > 0 && _homeAddresses.ContainsKey( registrant.PersonId.Value ) )
                {
                    var location = _homeAddresses[registrant.PersonId.Value];
                    lAddress.Text = location != null && location.FormattedAddress.IsNotNullOrWhiteSpace() ? location.FormattedAddress : string.Empty;
                }

                var mobileField = e.Row.FindControl( "lWaitlistMobile" ) as Literal;
                if ( mobileField != null )
                {
                    var mobilePhoneNumber = _mobilePhoneNumbers[registrant.PersonId.Value];
                    if ( mobilePhoneNumber == null || mobilePhoneNumber.NumberFormatted.IsNullOrWhiteSpace() )
                    {
                        mobileField.Text = string.Empty;
                    }
                    else
                    {
                        mobileField.Text = mobilePhoneNumber.IsUnlisted ? "Unlisted" : mobilePhoneNumber.NumberFormatted;
                    }
                }

                var homePhoneField = e.Row.FindControl( "lWaitlistHomePhone" ) as Literal;
                if ( homePhoneField != null )
                {
                    var homePhoneNumber = _homePhoneNumbers[registrant.PersonId.Value];
                    if ( homePhoneNumber == null || homePhoneNumber.NumberFormatted.IsNullOrWhiteSpace() )
                    {
                        homePhoneField.Text = string.Empty;
                    }
                    else
                    {
                        homePhoneField.Text = homePhoneNumber.IsUnlisted ? "Unlisted" : homePhoneNumber.NumberFormatted;
                    }
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            int? registrationInstanceId = PageParameter( PageParameterKey.RegistrationInstanceId ).AsIntegerOrNull();

            var registrationInstance = this.RegistrationInstance;

            if ( registrationInstance == null )
            {
                return;
            }

            var rockContext = new RockContext();

            this.RegistrantFields = GetRegistrantFormFields().Where( a => a.IsGridField ).ToArray();

            SetUserPreferencePrefix( this.RegistrationInstanceId );

            AddDynamicControls( true );

            BindWaitListFilter( registrationInstance );
            BindWaitListGrid();
        }

        /// <summary>
        /// Sets the user preference prefix.
        /// </summary>
        private void SetUserPreferencePrefix( int? registrationTemplateId )
        {
            fWaitList.UserPreferenceKeyPrefix = string.Format( "{0}-WL-", registrationTemplateId.GetValueOrDefault() );
        }

        /// <summary>
        /// Binds the wait list filter.
        /// </summary>
        /// <param name="instance">The instance.</param>
        private void BindWaitListFilter( RegistrationInstance instance )
        {
            drpWaitListDateRange.DelimitedValues = fWaitList.GetUserPreference( UserPreferenceKeyBase.GridFilter_DateRange );
            tbWaitListFirstName.Text = fWaitList.GetUserPreference( UserPreferenceKeyBase.GridFilter_FirstName );
            tbWaitListLastName.Text = fWaitList.GetUserPreference( UserPreferenceKeyBase.GridFilter_LastName );
        }

        /// <summary>
        /// Binds the wait list grid.
        /// </summary>
        /// <param name="isExporting">if set to <c>true</c> [is exporting].</param>
        private void BindWaitListGrid( bool isExporting = false )
        {
            var instanceId = this.RegistrationInstanceId;

            if ( instanceId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var registrationInstance = new RegistrationInstanceService( rockContext ).Get( instanceId.Value );

                    _waitListOrder = new RegistrationRegistrantService( rockContext ).Queryable().Where( r =>
                                            r.Registration.RegistrationInstanceId == instanceId.Value &&
                                            r.PersonAlias != null &&
                                            r.PersonAlias.Person != null &&
                                            r.OnWaitList )
                                        .OrderBy( r => r.CreatedDateTime )
                                        .Select( r => r.Id ).ToList();

                    // Start query for registrants
                    var registrationRegistrantService = new RegistrationRegistrantService( rockContext );
                    var qry = registrationRegistrantService
                    .Queryable( "PersonAlias.Person.PhoneNumbers.NumberTypeValue,Fees.RegistrationTemplateFee" ).AsNoTracking()
                    .Where( r =>
                        r.Registration.RegistrationInstanceId == instanceId.Value &&
                        r.PersonAlias != null &&
                        r.PersonAlias.Person != null &&
                        r.OnWaitList );

                    // Filter by daterange
                    if ( drpWaitListDateRange.LowerValue.HasValue )
                    {
                        qry = qry.Where( r =>
                            r.CreatedDateTime.HasValue &&
                            r.CreatedDateTime.Value >= drpWaitListDateRange.LowerValue.Value );
                    }

                    if ( drpWaitListDateRange.UpperValue.HasValue )
                    {
                        qry = qry.Where( r =>
                            r.CreatedDateTime.HasValue &&
                            r.CreatedDateTime.Value <= drpWaitListDateRange.UpperValue.Value );
                    }

                    // Filter by first name
                    if ( !string.IsNullOrWhiteSpace( tbWaitListFirstName.Text ) )
                    {
                        string rfname = tbWaitListFirstName.Text;
                        qry = qry.Where( r =>
                            r.PersonAlias.Person.NickName.StartsWith( rfname ) ||
                            r.PersonAlias.Person.FirstName.StartsWith( rfname ) );
                    }

                    // Filter by last name
                    if ( !string.IsNullOrWhiteSpace( tbWaitListLastName.Text ) )
                    {
                        string rlname = tbWaitListLastName.Text;
                        qry = qry.Where( r =>
                            r.PersonAlias.Person.LastName.StartsWith( rlname ) );
                    }

                    var personIds = qry.Select( r => r.PersonAlias.PersonId ).Distinct().ToList();
                    if ( isExporting || ( RegistrantFields != null && RegistrantFields.Any( f => f.PersonFieldType != null && f.PersonFieldType == RegistrationPersonFieldType.Address ) ) )
                    {
                        _homeAddresses = Person.GetHomeLocations( personIds );
                    }

                    _mobilePhoneNumbers = GetPersonMobilePhoneLookup( rockContext, this.RegistrantFields, personIds );
                    _homePhoneNumbers = GetPersonHomePhoneLookup( rockContext, this.RegistrantFields, personIds );

                    bool preloadCampusValues = false;
                    var registrantAttributes = new List<AttributeCache>();
                    var personAttributes = new List<AttributeCache>();
                    var groupMemberAttributes = new List<AttributeCache>();
                    var registrantAttributeIds = new List<int>();
                    var personAttributesIds = new List<int>();
                    var groupMemberAttributesIds = new List<int>();

                    if ( RegistrantFields != null )
                    {
                        // Filter by any selected
                        foreach ( var personFieldType in RegistrantFields
                            .Where( f =>
                                f.FieldSource == RegistrationFieldSource.PersonField &&
                                f.PersonFieldType.HasValue )
                            .Select( f => f.PersonFieldType.Value ) )
                        {
                            switch ( personFieldType )
                            {
                                case RegistrationPersonFieldType.Campus:
                                    preloadCampusValues = true;

                                    var ddlCampus = phWaitListFormFieldFilters.FindControl( FILTER_CAMPUS_ID ) as RockDropDownList;
                                    if ( ddlCampus != null )
                                    {
                                        var campusId = ddlCampus.SelectedValue.AsIntegerOrNull();
                                        if ( campusId.HasValue )
                                        {
                                            var familyGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.Members.Any( m =>
                                                    m.Group.GroupType.Guid == familyGroupTypeGuid &&
                                                    m.Group.CampusId.HasValue &&
                                                    m.Group.CampusId.Value == campusId ) );
                                        }
                                    }

                                    break;

                                case RegistrationPersonFieldType.Email:
                                    var tbEmailFilter = phWaitListFormFieldFilters.FindControl( FILTER_EMAIL_ID ) as RockTextBox;
                                    if ( tbEmailFilter != null && !string.IsNullOrWhiteSpace( tbEmailFilter.Text ) )
                                    {
                                        qry = qry.Where( r =>
                                            r.PersonAlias.Person.Email != null &&
                                            r.PersonAlias.Person.Email.Contains( tbEmailFilter.Text ) );
                                    }

                                    break;

                                case RegistrationPersonFieldType.Birthdate:
                                    var drpBirthdateFilter = phWaitListFormFieldFilters.FindControl( FILTER_BIRTHDATE_ID ) as DateRangePicker;
                                    if ( drpBirthdateFilter != null )
                                    {
                                        if ( drpBirthdateFilter.LowerValue.HasValue )
                                        {
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.BirthDate.HasValue &&
                                                r.PersonAlias.Person.BirthDate.Value >= drpBirthdateFilter.LowerValue.Value );
                                        }

                                        if ( drpBirthdateFilter.UpperValue.HasValue )
                                        {
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.BirthDate.HasValue &&
                                                r.PersonAlias.Person.BirthDate.Value <= drpBirthdateFilter.UpperValue.Value );
                                        }
                                    }

                                    break;
                                case RegistrationPersonFieldType.MiddleName:
                                    var tbMiddleNameFilter = phWaitListFormFieldFilters.FindControl( FILTER_MIDDLE_NAME_ID ) as RockTextBox;
                                    if ( tbMiddleNameFilter != null && !string.IsNullOrWhiteSpace( tbMiddleNameFilter.Text ) )
                                    {
                                        qry = qry.Where( r =>
                                            r.PersonAlias.Person.MiddleName != null &&
                                            r.PersonAlias.Person.MiddleName.Contains( tbMiddleNameFilter.Text ) );
                                    }

                                    break;

                                case RegistrationPersonFieldType.AnniversaryDate:
                                    var drpAnniversaryDateFilter = phWaitListFormFieldFilters.FindControl( FILTER_ANNIVERSARY_DATE_ID ) as DateRangePicker;
                                    if ( drpAnniversaryDateFilter != null )
                                    {
                                        if ( drpAnniversaryDateFilter.LowerValue.HasValue )
                                        {
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.AnniversaryDate.HasValue &&
                                                r.PersonAlias.Person.AnniversaryDate.Value >= drpAnniversaryDateFilter.LowerValue.Value );
                                        }

                                        if ( drpAnniversaryDateFilter.UpperValue.HasValue )
                                        {
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.AnniversaryDate.HasValue &&
                                                r.PersonAlias.Person.AnniversaryDate.Value <= drpAnniversaryDateFilter.UpperValue.Value );
                                        }
                                    }

                                    break;
                                case RegistrationPersonFieldType.Grade:
                                    var gpGradeFilter = phWaitListFormFieldFilters.FindControl( FILTER_GRADE_ID ) as GradePicker;
                                    if ( gpGradeFilter != null )
                                    {
                                        int? graduationYear = Person.GraduationYearFromGradeOffset( gpGradeFilter.SelectedValueAsInt( false ) );
                                        if ( graduationYear.HasValue )
                                        {
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.GraduationYear.HasValue &&
                                                r.PersonAlias.Person.GraduationYear == graduationYear.Value );
                                        }
                                    }

                                    break;

                                case RegistrationPersonFieldType.Gender:
                                    var ddlGenderFilter = phWaitListFormFieldFilters.FindControl( FILTER_GENDER_ID ) as RockDropDownList;
                                    if ( ddlGenderFilter != null )
                                    {
                                        var gender = ddlGenderFilter.SelectedValue.ConvertToEnumOrNull<Gender>();
                                        if ( gender.HasValue )
                                        {
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.Gender == gender );
                                        }
                                    }

                                    break;

                                case RegistrationPersonFieldType.MaritalStatus:
                                    var dvpMaritalStatusFilter = phWaitListFormFieldFilters.FindControl( FILTER_MARTIAL_STATUS_ID ) as DefinedValuePicker;
                                    if ( dvpMaritalStatusFilter != null )
                                    {
                                        var maritalStatusId = dvpMaritalStatusFilter.SelectedValue.AsIntegerOrNull();
                                        if ( maritalStatusId.HasValue )
                                        {
                                            qry = qry.Where( r =>
                                                r.PersonAlias.Person.MaritalStatusValueId.HasValue &&
                                                r.PersonAlias.Person.MaritalStatusValueId.Value == maritalStatusId.Value );
                                        }
                                    }

                                    break;

                                case RegistrationPersonFieldType.MobilePhone:
                                    var tbMobilePhoneFilter = phWaitListFormFieldFilters.FindControl( FILTER_MOBILE_PHONE_ID ) as RockTextBox;
                                    if ( tbMobilePhoneFilter != null && !string.IsNullOrWhiteSpace( tbMobilePhoneFilter.Text ) )
                                    {
                                        string numericPhone = tbMobilePhoneFilter.Text.AsNumeric();

                                        if ( !string.IsNullOrEmpty( numericPhone ) )
                                        {
                                            var phoneNumberPersonIdQry = new PhoneNumberService( rockContext )
                                                .Queryable()
                                                .Where( a => a.Number.Contains( numericPhone ) )
                                                .Select( a => a.PersonId );

                                            qry = qry.Where( r => phoneNumberPersonIdQry.Contains( r.PersonAlias.PersonId ) );
                                        }
                                    }

                                    break;

                                case RegistrationPersonFieldType.HomePhone:
                                    var tbHomePhoneFilter = phWaitListFormFieldFilters.FindControl( FILTER_HOME_PHONE_ID ) as RockTextBox;
                                    if ( tbHomePhoneFilter != null && !string.IsNullOrWhiteSpace( tbHomePhoneFilter.Text ) )
                                    {
                                        string numericPhone = tbHomePhoneFilter.Text.AsNumeric();

                                        if ( !string.IsNullOrEmpty( numericPhone ) )
                                        {
                                            var phoneNumberPersonIdQry = new PhoneNumberService( rockContext )
                                                .Queryable()
                                                .Where( a => a.Number.Contains( numericPhone ) )
                                                .Select( a => a.PersonId );

                                            qry = qry.Where( r => phoneNumberPersonIdQry.Contains( r.PersonAlias.PersonId ) );
                                        }
                                    }

                                    break;
                            }
                        }

                        // Get all the registrant attributes selected to be on grid
                        registrantAttributes = RegistrantFields
                            .Where( f =>
                                f.Attribute != null &&
                                f.FieldSource == RegistrationFieldSource.RegistrantAttribute )
                            .Select( f => f.Attribute )
                            .ToList();
                        registrantAttributeIds = registrantAttributes.Select( a => a.Id ).Distinct().ToList();

                        // Filter query by any configured registrant attribute filters
                        if ( registrantAttributes != null && registrantAttributes.Any() )
                        {
                            foreach ( var attribute in registrantAttributes )
                            {
                                var filterControl = phWaitListFormFieldFilters.FindControl( FILTER_ATTRIBUTE_PREFIX + attribute.Id.ToString() );
                                qry = attribute.FieldType.Field.ApplyAttributeQueryFilter( qry, filterControl, attribute, registrationRegistrantService, Rock.Reporting.FilterMode.SimpleFilter );
                            }
                        }

                        // Get all the person attributes selected to be on grid
                        personAttributes = RegistrantFields
                            .Where( f =>
                                f.Attribute != null &&
                                f.FieldSource == RegistrationFieldSource.PersonAttribute )
                            .Select( f => f.Attribute )
                            .ToList();
                        personAttributesIds = personAttributes.Select( a => a.Id ).Distinct().ToList();

                        // Filter query by any configured person attribute filters
                        if ( personAttributes != null && personAttributes.Any() )
                        {
                            PersonService personService = new PersonService( rockContext );
                            var personQry = personService.Queryable().AsNoTracking();

                            foreach ( var attribute in personAttributes )
                            {
                                var filterControl = phWaitListFormFieldFilters.FindControl( FILTER_ATTRIBUTE_PREFIX + attribute.Id.ToString() );
                                personQry = attribute.FieldType.Field.ApplyAttributeQueryFilter( personQry, filterControl, attribute, personService, Rock.Reporting.FilterMode.SimpleFilter );
                            }

                            qry = qry.Where( r => personQry.Any( p => p.Id == r.PersonAlias.PersonId ) );
                        }

                        // Get all the group member attributes selected to be on grid
                        groupMemberAttributes = RegistrantFields
                            .Where( f =>
                                f.Attribute != null &&
                                f.FieldSource == RegistrationFieldSource.GroupMemberAttribute )
                            .Select( f => f.Attribute )
                            .ToList();
                        groupMemberAttributesIds = groupMemberAttributes.Select( a => a.Id ).Distinct().ToList();

                        // Filter query by any configured person attribute filters
                        if ( groupMemberAttributes != null && groupMemberAttributes.Any() )
                        {
                            var groupMemberService = new GroupMemberService( rockContext );
                            var groupMemberQry = groupMemberService.Queryable().AsNoTracking();

                            foreach ( var attribute in groupMemberAttributes )
                            {
                                var filterControl = phWaitListFormFieldFilters.FindControl( FILTER_ATTRIBUTE_PREFIX + attribute.Id.ToString() );
                                groupMemberQry = attribute.FieldType.Field.ApplyAttributeQueryFilter( groupMemberQry, filterControl, attribute, groupMemberService, Rock.Reporting.FilterMode.SimpleFilter );
                            }

                            qry = qry.Where( r => groupMemberQry.Any( g => g.Id == r.GroupMemberId ) );
                        }
                    }

                    // Sort the query
                    IOrderedQueryable<RegistrationRegistrant> orderedQry = null;
                    SortProperty sortProperty = gWaitList.SortProperty;
                    if ( sortProperty != null )
                    {
                        orderedQry = qry.Sort( sortProperty );
                    }
                    else
                    {
                        orderedQry = qry
                            .OrderBy( r => r.Id );
                    }

                    // increase the timeout just in case. A complex filter on the grid might slow things down
                    rockContext.Database.CommandTimeout = 180;

                    // Set the grids LinqDataSource which will run query and set results for current page
                    gWaitList.SetLinqDataSource<RegistrationRegistrant>( orderedQry );

                    if ( RegistrantFields != null )
                    {
                        // Get the query results for the current page
                        var currentPageRegistrants = gWaitList.DataSource as List<RegistrationRegistrant>;
                        if ( currentPageRegistrants != null )
                        {
                            // Get all the registrant ids in current page of query results
                            var registrantIds = currentPageRegistrants
                                .Select( r => r.Id )
                                .Distinct()
                                .ToList();

                            // Get all the person ids in current page of query results
                            var currentPagePersonIds = currentPageRegistrants
                                .Select( r => r.PersonAlias.PersonId )
                                .Distinct()
                                .ToList();

                            // Get all the group member ids and the group id in current page of query results
                            var groupMemberIds = new List<int>();
                            GroupLinks = new Dictionary<int, string>();
                            foreach ( var groupMember in currentPageRegistrants
                                .Where( m =>
                                    m.GroupMember != null &&
                                    m.GroupMember.Group != null )
                                .Select( m => m.GroupMember ) )
                            {
                                groupMemberIds.Add( groupMember.Id );
                                string linkedPageUrl = LinkedPageUrl( "GroupDetailPage", new Dictionary<string, string> { { "GroupId", groupMember.GroupId.ToString() } } );
                                GroupLinks.AddOrIgnore( groupMember.GroupId, isExporting ? groupMember.Group.Name : string.Format( "<a href='{0}'>{1}</a>", linkedPageUrl, groupMember.Group.Name ) );
                            }

                            // If the campus column was selected to be displayed on grid, preload all the people's
                            // campuses so that the databind does not need to query each row
                            if ( preloadCampusValues )
                            {
                                PersonCampusIds = new Dictionary<int, List<int>>();

                                Guid familyGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
                                foreach ( var personCampusList in new GroupMemberService( rockContext )
                                    .Queryable().AsNoTracking()
                                    .Where( m =>
                                        m.Group.GroupType.Guid == familyGroupTypeGuid &&
                                        currentPagePersonIds.Contains( m.PersonId ) )
                                    .GroupBy( m => m.PersonId )
                                    .Select( m => new
                                    {
                                        PersonId = m.Key,
                                        CampusIds = m
                                            .Where( g => g.Group.CampusId.HasValue )
                                            .Select( g => g.Group.CampusId.Value )
                                            .ToList()
                                    } ) )
                                {
                                    PersonCampusIds.Add( personCampusList.PersonId, personCampusList.CampusIds );
                                }
                            }

                            // If there are any attributes that were selected to be displayed, we're going
                            // to try and read all attribute values in one query and then put them into a
                            // custom grid ObjectList property so that the AttributeField columns don't need
                            // to do the LoadAttributes and querying of values for each row/column
                            if ( personAttributesIds.Any() || groupMemberAttributesIds.Any() || registrantAttributeIds.Any() )
                            {
                                // Query the attribute values for all rows and attributes
                                var attributeValues = new AttributeValueService( rockContext )
                                    .Queryable( "Attribute" ).AsNoTracking()
                                    .Where( v =>
                                        v.EntityId.HasValue &&
                                        (
                                            (
                                                personAttributesIds.Contains( v.AttributeId ) &&
                                                currentPagePersonIds.Contains( v.EntityId.Value )
                                            ) ||
                                            (
                                                groupMemberAttributesIds.Contains( v.AttributeId ) &&
                                                groupMemberIds.Contains( v.EntityId.Value )
                                            ) ||
                                            (
                                                registrantAttributeIds.Contains( v.AttributeId ) &&
                                                registrantIds.Contains( v.EntityId.Value )
                                            )
                                        ) ).ToList();

                                // Get the attributes to add to each row's object
                                var attributes = new Dictionary<string, AttributeCache>();
                                RegistrantFields
                                        .Where( f => f.Attribute != null )
                                        .Select( f => f.Attribute )
                                        .ToList()
                                    .ForEach( a => attributes
                                        .Add( a.Id.ToString() + a.Key, a ) );

                                // Initialize the grid's object list
                                gWaitList.ObjectList = new Dictionary<string, object>();

                                // Loop through each of the current page's registrants and build an attribute
                                // field object for storing attributes and the values for each of the registrants
                                foreach ( var registrant in currentPageRegistrants )
                                {
                                    // Create a row attribute object
                                    var attributeFieldObject = new AttributeFieldObject();

                                    // Add the attributes to the attribute object
                                    attributeFieldObject.Attributes = attributes;

                                    // Add any person attribute values to object
                                    attributeValues
                                        .Where( v =>
                                            personAttributesIds.Contains( v.AttributeId ) &&
                                            v.EntityId.Value == registrant.PersonAlias.PersonId )
                                        .ToList()
                                        .ForEach( v => attributeFieldObject.AttributeValues
                                            .Add( v.AttributeId.ToString() + v.Attribute.Key, new AttributeValueCache( v ) ) );

                                    // Add any group member attribute values to object
                                    if ( registrant.GroupMemberId.HasValue )
                                    {
                                        attributeValues
                                            .Where( v =>
                                                groupMemberAttributesIds.Contains( v.AttributeId ) &&
                                                v.EntityId.Value == registrant.GroupMemberId.Value )
                                            .ToList()
                                            .ForEach( v => attributeFieldObject.AttributeValues
                                                .Add( v.AttributeId.ToString() + v.Attribute.Key, new AttributeValueCache( v ) ) );
                                    }

                                    // Add any registrant attribute values to object
                                    attributeValues
                                        .Where( v =>
                                            registrantAttributeIds.Contains( v.AttributeId ) &&
                                            v.EntityId.Value == registrant.Id )
                                        .ToList()
                                        .ForEach( v => attributeFieldObject.AttributeValues
                                            .Add( v.AttributeId.ToString() + v.Attribute.Key, new AttributeValueCache( v ) ) );

                                    // Add row attribute object to grid's object list
                                    gWaitList.ObjectList.Add( registrant.Id.ToString(), attributeFieldObject );
                                }
                            }
                        }
                    }

                    gWaitList.DataBind();
                }
            }
        }

        /// <summary>
        /// Add dynamically-generated controls to the form.
        /// </summary>
        /// <param name="setValues"></param>
        private void AddDynamicControls( bool setValues )
        {
            AddRegistrationTemplateFieldsToGrid( this.RegistrantFields, phWaitListFormFieldFilters, gWaitList, fWaitList, setValues, false );
        }

        #endregion

        #region ISecondaryBlock

        /// <summary>
        /// Sets the visibility of the block.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlDetails.Visible = visible;
        }

        #endregion       
    }
}