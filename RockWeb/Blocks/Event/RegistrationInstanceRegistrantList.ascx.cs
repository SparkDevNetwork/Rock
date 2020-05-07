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
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// A Block that displays the list of Registrants related to a Registration Instance.
    /// </summary>
    [DisplayName( "Registration Instance - Registrant List" )]
    [Category( "Event" )]
    [Description( "Displays the list of Registrants related to a Registration Instance." )]

    #region Block Attributes

    [LinkedPage(
        "Registration Page",
        Description = "The page for editing registration and registrant information",
        Key = AttributeKey.RegistrationPage,
        DefaultValue = Rock.SystemGuid.Page.REGISTRATION_DETAIL,
        IsRequired = false,
        Order = 1 )]

    [LinkedPage(
        "Group Placement Page",
        Description = "The page for managing the registrant's group placements",
        Key = AttributeKey.GroupPlacementPage,
        DefaultValue = Rock.SystemGuid.Page.REGISTRATION_INSTANCE_PLACEMENT_GROUPS,
        IsRequired = false,
        Order = 2 )]

    [LinkedPage(
        "Group Detail Page",
        Description = "The page for viewing details about a group",
        Key = AttributeKey.GroupDetailPage,
        IsRequired = true,
        DefaultValue = Rock.SystemGuid.Page.GROUP_VIEWER,
        Order = 3 )]

    #endregion

    public partial class RegistrationInstanceRegistrantList : RegistrationInstanceBlock, ISecondaryBlock
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
            /// The group placement page
            /// </summary>
            public const string GroupPlacementPage = "GroupPlacementPage";

            public const string GroupDetailPage = "GroupDetailPage";
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
        }

        #endregion Page Parameter Keys

        #region Properties and Fields

        private Dictionary<int, Location> _homeAddresses = new Dictionary<int, Location>();
        private Dictionary<int, PhoneNumber> _mobilePhoneNumbers = new Dictionary<int, PhoneNumber>();
        private Dictionary<int, PhoneNumber> _homePhoneNumbers = new Dictionary<int, PhoneNumber>();
        private List<RegistrationTemplatePlacement> _registrationTemplatePlacements = null;
        private List<PlacementGroupInfo> _placementGroupInfoList = null;
        private RockLiteralField _placementsField = null;

        private bool _isExporting = false;

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
        /// Gets or sets the signed person ids.
        /// </summary>
        /// <value>
        /// The signed person ids.
        /// </value>
        private List<int> SignersPersonAliasIds { get; set; }

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

            SetUserPreferencePrefix( this.RegistrationTemplateId.GetValueOrDefault() );

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

            ddlRegistrantsInGroup.Items.Clear();
            ddlRegistrantsInGroup.Items.Add( new ListItem() );
            ddlRegistrantsInGroup.Items.Add( new ListItem( "Yes", "Yes" ) );
            ddlRegistrantsInGroup.Items.Add( new ListItem( "No", "No" ) );

            ddlRegistrantsSignedDocument.Items.Clear();
            ddlRegistrantsSignedDocument.Items.Add( new ListItem() );
            ddlRegistrantsSignedDocument.Items.Add( new ListItem( "Yes", "Yes" ) );
            ddlRegistrantsSignedDocument.Items.Add( new ListItem( "No", "No" ) );

            fRegistrants.ApplyFilterClick += fRegistrants_ApplyFilterClick;

            gRegistrants.EmptyDataText = "No Registrants Found";
            gRegistrants.DataKeyNames = new string[] { "Id" };
            gRegistrants.Actions.ShowAdd = true;
            gRegistrants.Actions.AddClick += gRegistrants_AddClick;
            gRegistrants.RowDataBound += gRegistrants_RowDataBound;
            gRegistrants.GridRebind += gRegistrants_GridRebind;

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
        /// Handles the ApplyFilterClick event of the fRegistrants control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fRegistrants_ApplyFilterClick( object sender, EventArgs e )
        {
            fRegistrants.SaveUserPreference( UserPreferenceKeyBase.GridFilter_RegistrantsDateRange, "Registrants Date Range", sdrpRegistrantsRegistrantDateRange.DelimitedValues );
            fRegistrants.SaveUserPreference( UserPreferenceKeyBase.GridFilter_FirstName, tbRegistrantsRegistrantFirstName.Text );
            fRegistrants.SaveUserPreference( UserPreferenceKeyBase.GridFilter_LastName, tbRegistrantsRegistrantLastName.Text );
            fRegistrants.SaveUserPreference( UserPreferenceKeyBase.GridFilter_InGroup, ddlRegistrantsInGroup.SelectedValue );
            fRegistrants.SaveUserPreference( UserPreferenceKeyBase.GridFilter_SignedDocument, ddlRegistrantsSignedDocument.SelectedValue );

            if ( RegistrantFields != null )
            {
                foreach ( var field in RegistrantFields )
                {
                    if ( field.FieldSource == RegistrationFieldSource.PersonField && field.PersonFieldType.HasValue )
                    {
                        switch ( field.PersonFieldType.Value )
                        {
                            case RegistrationPersonFieldType.Campus:
                                var ddlCampus = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_CAMPUS_ID ) as RockDropDownList;
                                if ( ddlCampus != null )
                                {
                                    fRegistrants.SaveUserPreference( UserPreferenceKeyBase.GridFilter_HomeCampus, ddlCampus.SelectedValue );
                                }

                                break;

                            case RegistrationPersonFieldType.Email:
                                var tbEmailFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_EMAIL_ID ) as RockTextBox;
                                if ( tbEmailFilter != null )
                                {
                                    fRegistrants.SaveUserPreference( UserPreferenceKeyBase.GridFilter_Email, tbEmailFilter.Text );
                                }

                                break;

                            case RegistrationPersonFieldType.Birthdate:
                                var drpBirthdateFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_BIRTHDATE_ID ) as DateRangePicker;
                                if ( drpBirthdateFilter != null )
                                {
                                    fRegistrants.SaveUserPreference( UserPreferenceKeyBase.GridFilter_BirthdateRange, drpBirthdateFilter.DelimitedValues );
                                }

                                break;
                            case RegistrationPersonFieldType.MiddleName:
                                var tbMiddleNameFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_MIDDLE_NAME_ID ) as RockTextBox;
                                if ( tbMiddleNameFilter != null )
                                {
                                    fRegistrants.SaveUserPreference( UserPreferenceKeyBase.GridFilter_MiddleName, tbMiddleNameFilter.Text );
                                }

                                break;
                            case RegistrationPersonFieldType.AnniversaryDate:
                                var drAnniversaryDateFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_ANNIVERSARY_DATE_ID ) as DateRangePicker;
                                if ( drAnniversaryDateFilter != null )
                                {
                                    fRegistrants.SaveUserPreference( UserPreferenceKeyBase.GridFilter_AnniversaryDateRange, drAnniversaryDateFilter.DelimitedValues );
                                }

                                break;
                            case RegistrationPersonFieldType.Grade:
                                var gpGradeFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_GRADE_ID ) as GradePicker;
                                if ( gpGradeFilter != null )
                                {
                                    int? gradeOffset = gpGradeFilter.SelectedValueAsInt( false );
                                    fRegistrants.SaveUserPreference( UserPreferenceKeyBase.GridFilter_Grade, gradeOffset.HasValue ? gradeOffset.Value.ToString() : string.Empty );
                                }

                                break;

                            case RegistrationPersonFieldType.Gender:
                                var ddlGenderFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_GENDER_ID ) as RockDropDownList;
                                if ( ddlGenderFilter != null )
                                {
                                    fRegistrants.SaveUserPreference( UserPreferenceKeyBase.GridFilter_Gender, ddlGenderFilter.SelectedValue );
                                }

                                break;

                            case RegistrationPersonFieldType.MaritalStatus:
                                var dvpMaritalStatusFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_MARTIAL_STATUS_ID ) as DefinedValuePicker;
                                if ( dvpMaritalStatusFilter != null )
                                {
                                    fRegistrants.SaveUserPreference( UserPreferenceKeyBase.GridFilter_MaritalStatus, dvpMaritalStatusFilter.SelectedValue );
                                }

                                break;

                            case RegistrationPersonFieldType.MobilePhone:
                                var tbMobilePhoneFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_MOBILE_PHONE_ID ) as RockTextBox;
                                if ( tbMobilePhoneFilter != null )
                                {
                                    fRegistrants.SaveUserPreference( UserPreferenceKeyBase.GridFilter_CellPhone, "Cell Phone", tbMobilePhoneFilter.Text );
                                }

                                break;

                            case RegistrationPersonFieldType.HomePhone:
                                var tbHomePhoneFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_HOME_PHONE_ID ) as RockTextBox;
                                if ( tbHomePhoneFilter != null )
                                {
                                    fRegistrants.SaveUserPreference( UserPreferenceKeyBase.GridFilter_HomePhone, tbHomePhoneFilter.Text );
                                }

                                break;
                        }
                    }

                    if ( field.Attribute != null )
                    {
                        var attribute = field.Attribute;
                        var filterControl = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_ATTRIBUTE_PREFIX + attribute.Id.ToString() );
                        if ( filterControl != null )
                        {
                            try
                            {
                                var values = attribute.FieldType.Field.GetFilterValues( filterControl, field.Attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
                                fRegistrants.SaveUserPreference( attribute.Key, attribute.Name, attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter ).ToJson() );
                            }
                            catch
                            {
                                // ignore
                            }
                        }
                    }
                }
            }

            BindRegistrantsGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the fRegistrants control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fRegistrants_ClearFilterClick( object sender, EventArgs e )
        {
            fRegistrants.DeleteUserPreferences();

            foreach ( var control in phRegistrantsRegistrantFormFieldFilters.ControlsOfTypeRecursive<Control>().Where( a => a.ID != null && a.ID.StartsWith( "filter" ) && a.ID.Contains( "_" ) ) )
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
                                var ddlCampus = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_CAMPUS_ID ) as RockDropDownList;
                                if ( ddlCampus != null )
                                {
                                    ddlCampus.SetValue( ( Guid? ) null );
                                }

                                break;

                            case RegistrationPersonFieldType.Email:
                                var tbEmailFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_EMAIL_ID ) as RockTextBox;
                                if ( tbEmailFilter != null )
                                {
                                    tbEmailFilter.Text = string.Empty;
                                }

                                break;

                            case RegistrationPersonFieldType.Birthdate:
                                var drpBirthdateFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_BIRTHDATE_ID ) as DateRangePicker;
                                if ( drpBirthdateFilter != null )
                                {
                                    drpBirthdateFilter.LowerValue = null;
                                    drpBirthdateFilter.UpperValue = null;
                                }

                                break;
                            case RegistrationPersonFieldType.MiddleName:
                                var tbMiddleNameFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_MIDDLE_NAME_ID ) as RockTextBox;
                                if ( tbMiddleNameFilter != null )
                                {
                                    tbMiddleNameFilter.Text = string.Empty;
                                }

                                break;
                            case RegistrationPersonFieldType.AnniversaryDate:
                                var drAnniversaryDateFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_ANNIVERSARY_DATE_ID ) as DateRangePicker;
                                if ( drAnniversaryDateFilter != null )
                                {
                                    drAnniversaryDateFilter.LowerValue = null;
                                    drAnniversaryDateFilter.UpperValue = null;
                                }

                                break;
                            case RegistrationPersonFieldType.Grade:
                                var gpGradeFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_GRADE_ID ) as GradePicker;
                                if ( gpGradeFilter != null )
                                {
                                    gpGradeFilter.SetValue( ( Guid? ) null );
                                }

                                break;

                            case RegistrationPersonFieldType.Gender:
                                var ddlGenderFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_GENDER_ID ) as RockDropDownList;
                                if ( ddlGenderFilter != null )
                                {
                                    ddlGenderFilter.SetValue( ( Guid? ) null );
                                }

                                break;

                            case RegistrationPersonFieldType.MaritalStatus:
                                var dvpMaritalStatusFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_MARTIAL_STATUS_ID ) as DefinedValuePicker;
                                if ( dvpMaritalStatusFilter != null )
                                {
                                    dvpMaritalStatusFilter.SetValue( ( Guid? ) null );
                                }

                                break;

                            case RegistrationPersonFieldType.MobilePhone:
                                var tbMobilePhoneFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_MOBILE_PHONE_ID ) as RockTextBox;
                                if ( tbMobilePhoneFilter != null )
                                {
                                    tbMobilePhoneFilter.Text = string.Empty;
                                }

                                break;

                            case RegistrationPersonFieldType.HomePhone:
                                var tbHomePhoneFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_HOME_PHONE_ID ) as RockTextBox;
                                if ( tbHomePhoneFilter != null )
                                {
                                    tbHomePhoneFilter.Text = string.Empty;
                                }

                                break;
                        }
                    }
                }
            }

            BindRegistrantsFilter( null );
        }

        /// <summary>
        /// Gets the display value for a filter field.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void fRegistrants_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            if ( RegistrantFields != null )
            {
                var attribute = RegistrantFields
                    .Where( a =>
                        a.Attribute != null &&
                        a.Attribute.Key == e.Key )
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

            switch ( e.Key )
            {
                case "Registrants Date Range":
                    e.Value = SlidingDateRangePicker.FormatDelimitedValues( e.Value );
                    break;

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
                case "Signed Document":
                case "Home Phone":
                case "Cell Phone":
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
        /// Handles the GridRebind event of the gRegistrants control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gRegistrants_GridRebind( object sender, GridRebindEventArgs e )
        {
            var registrationInstanceId = hfRegistrationInstanceId.Value.AsInteger();

            var registrationInstance = GetRegistrationInstance( registrationInstanceId, new RockContext() );

            var name = registrationInstance.Name.FormatAsHtmlTitle();

            gRegistrants.ExportTitleName = name + " - Registrants";
            gRegistrants.ExportFilename = gRegistrants.ExportFilename ?? name + "Registrants";
            BindRegistrantsGrid( e.IsExporting );
        }

        /// <summary>
        /// Handles the RowDataBound event of the gRegistrants control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        private void gRegistrants_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var registrant = e.Row.DataItem as RegistrationRegistrant;
            if ( registrant == null )
            {
                return;
            }

            // Set the registrant name value
            var lRegistrant = e.Row.FindControl( "lRegistrant" ) as Literal;
            if ( lRegistrant != null )
            {
                if ( registrant.PersonAlias != null && registrant.PersonAlias.Person != null )
                {
                    lRegistrant.Text = registrant.PersonAlias.Person.FullNameReversed +
                        ( SignersPersonAliasIds != null && !SignersPersonAliasIds.Contains( registrant.PersonAlias.PersonId ) ? " <i class='fa fa-edit text-danger'></i>" : string.Empty );
                }
                else
                {
                    lRegistrant.Text = string.Empty;
                }
            }

            // Set the Group Name
            if ( registrant.GroupMember != null && GroupLinks.ContainsKey( registrant.GroupMember.GroupId ) )
            {
                var lGroup = e.Row.FindControl( "lGroup" ) as Literal;
                if ( lGroup != null )
                {
                    lGroup.Text = GroupLinks[registrant.GroupMember.GroupId];
                }
            }

            // Set the campus
            var lCampus = e.Row.FindControl( "lRegistrantsCampus" ) as Literal;

            // if it's null, try looking for the "lGroupPlacementsCampus" control since this RowDataBound event is shared between
            // two different grids.
            if ( lCampus == null )
            {
                lCampus = e.Row.FindControl( "lGroupPlacementsCampus" ) as Literal;
            }

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

            // Set the Fees
            var lFees = e.Row.FindControl( "lFees" ) as Literal;
            if ( lFees != null )
            {
                if ( registrant.Fees != null && registrant.Fees.Any() )
                {
                    var feeDesc = new List<string>();
                    foreach ( var fee in registrant.Fees )
                    {
                        feeDesc.Add( string.Format(
                            "{0}{1} ({2})",
                            fee.Quantity > 1 ? fee.Quantity.ToString( "N0" ) + " " : string.Empty,
                            fee.Quantity > 1 ? fee.RegistrationTemplateFee.Name.Pluralize() : fee.RegistrationTemplateFee.Name,
                            fee.Cost.FormatAsCurrency() ) );
                    }

                    lFees.Text = feeDesc.AsDelimited( "<br/>" );
                }
            }

            var lPlacements = e.Row.FindControl( "lPlacements" ) as Literal;
            if ( lPlacements != null )
            {
                SetPlacementFieldHtml( registrant, lPlacements );
            }

            if ( _homeAddresses.Any() && _homeAddresses.ContainsKey( registrant.PersonId.Value ) )
            {
                var location = _homeAddresses[registrant.PersonId.Value];

                // break up addresses if exporting
                if ( _isExporting )
                {
                    var lStreet1 = e.Row.FindControl( "lStreet1" ) as Literal;
                    var lStreet2 = e.Row.FindControl( "lStreet2" ) as Literal;
                    var lCity = e.Row.FindControl( "lCity" ) as Literal;
                    var lState = e.Row.FindControl( "lState" ) as Literal;
                    var lPostalCode = e.Row.FindControl( "lPostalCode" ) as Literal;
                    var lCountry = e.Row.FindControl( "lCountry" ) as Literal;

                    if ( location != null )
                    {
                        lStreet1.Text = location.Street1;
                        lStreet2.Text = location.Street2;
                        lCity.Text = location.City;
                        lState.Text = location.State;
                        lPostalCode.Text = location.PostalCode;
                        lCountry.Text = location.Country;
                    }
                }
                else
                {
                    var addressField = e.Row.FindControl( "lRegistrantsAddress" ) as Literal ?? e.Row.FindControl( "lGroupPlacementsAddress" ) as Literal;
                    if ( addressField != null )
                    {
                        addressField.Text = location != null && location.FormattedAddress.IsNotNullOrWhiteSpace() ? location.FormattedAddress : string.Empty;
                    }
                }
            }

            if ( _mobilePhoneNumbers.Any() )
            {
                var mobileNumber = _mobilePhoneNumbers[registrant.PersonId.Value];
                var mobileField = e.Row.FindControl( "lRegistrantsMobile" ) as Literal ?? e.Row.FindControl( "lGroupPlacementsMobile" ) as Literal;
                if ( mobileField != null )
                {
                    if ( mobileNumber == null || mobileNumber.NumberFormatted.IsNullOrWhiteSpace() )
                    {
                        mobileField.Text = string.Empty;
                    }
                    else
                    {
                        mobileField.Text = mobileNumber.IsUnlisted ? "Unlisted" : mobileNumber.NumberFormatted;
                    }
                }
            }

            if ( _homePhoneNumbers.Any() )
            {
                var homePhoneNumber = _homePhoneNumbers[registrant.PersonId.Value];
                var homePhoneField = e.Row.FindControl( "lRegistrantsHomePhone" ) as Literal ?? e.Row.FindControl( "lGroupPlacementsHomePhone" ) as Literal;
                if ( homePhoneField != null )
                {
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

        /// <summary>
        /// Sets the placement field HTML.
        /// </summary>
        /// <param name="registrant">The registrant.</param>
        /// <param name="lPlacements">The l placements.</param>
        private void SetPlacementFieldHtml( RegistrationRegistrant registrant, Literal lPlacements )
        {
            var placementsHtmlBuilder = new StringBuilder();
            foreach ( var registrationTemplatePlacement in _registrationTemplatePlacements )
            {
                var queryParams = new Dictionary<string, string>();
                queryParams.Add( "RegistrationTemplatePlacementId", registrationTemplatePlacement.Id.ToString() );
                queryParams.Add( "RegistrationInstanceId", this.RegistrationInstanceId.ToString() );

                /* NOTE: MDP - 2020-02-12
                  We could add RegistrantId has a parameter, but decided not to do this (yet).
                  // queryParams.Add( "RegistrantId", registrant.Id.ToString() );
                */

                var groupPlacementUrl = LinkedPageUrl( AttributeKey.GroupPlacementPage, queryParams );
                groupPlacementUrl += "#PersonId_" + registrant.PersonId.ToString();

                var registrantPlacedGroups = this._placementGroupInfoList.Where( a =>
                     ( a.RegistrationTemplatePlacementId.HasValue && a.RegistrationTemplatePlacementId.Value == registrationTemplatePlacement.Id )
                     || ( !a.RegistrationTemplatePlacementId.HasValue && a.Group.GroupTypeId == registrationTemplatePlacement.GroupTypeId ) )
                    .Where( a => a.PersonIds.Contains( registrant.PersonAlias.PersonId ) ).ToList();

                var groupCount = registrantPlacedGroups.Count();
                var toolTip = registrantPlacedGroups.Select( a => a.Group.Name ).ToList().AsDelimited( ", ", " and " );

                string btnClass;
                if ( groupCount > 0 )
                {
                    btnClass = "btn btn-success btn-xs btn-placement-status registrant-is-placed";
                }
                else
                {
                    btnClass = "btn btn-default btn-xs btn-placement-status registrant-not-placed";
                }

                string iconCssClass = registrationTemplatePlacement.GetIconCssClass();
                if ( iconCssClass.IsNullOrWhiteSpace() )
                {
                    iconCssClass = "fa fa-users";
                }

                string groupCountText;
                if ( registrationTemplatePlacement.AllowMultiplePlacements )
                {
                    groupCountText = groupCount.ToString();
                }
                else
                {
                    groupCountText = string.Empty;
                }

                if ( _isExporting )
                {
                    placementsHtmlBuilder.AppendLine( toolTip );
                }
                else
                {
                    placementsHtmlBuilder.AppendLine(
                        string.Format(
                            @"<a class='{0}' href='{1}' title='{2}'><i class='{3}'></i>{4}</a>",
                            btnClass, // {0}
                            groupPlacementUrl, // {1}
                            toolTip, // {2}
                            iconCssClass, // {3}
                            groupCountText ) ); // {4}
                }
            }

            lPlacements.Text = string.Format( "<div class='placement-list'>{0}</div>", placementsHtmlBuilder.ToString() );
        }

        /// <summary>
        /// Handles the AddClick event of the gRegistrants control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gRegistrants_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.RegistrationPage, "RegistrationId", 0, "RegistrationInstanceId", hfRegistrationInstanceId.ValueAsInt() );
        }

        /// <summary>
        /// Handles the RowSelected event of the gRegistrants control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gRegistrants_RowSelected( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var registrantService = new RegistrationRegistrantService( rockContext );
                var registrant = registrantService.Get( e.RowKeyId );
                if ( registrant != null )
                {
                    var qryParams = new Dictionary<string, string>();
                    qryParams.Add( "RegistrationId", registrant.RegistrationId.ToString() );
                    qryParams.Add( "RegistrationInstanceId", hfRegistrationInstanceId.Value );
                    string url = LinkedPageUrl( AttributeKey.RegistrationPage, qryParams );
                    url += "#" + e.RowKeyValue;
                    Response.Redirect( url, false );
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
        }

        /// <summary>
        /// Handles the Delete event of the gRegistrants control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gRegistrants_Delete( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var registrantService = new RegistrationRegistrantService( rockContext );
                var registrant = registrantService.Get( e.RowKeyId );
                if ( registrant != null )
                {
                    string errorMessage;
                    if ( !registrantService.CanDelete( registrant, out errorMessage ) )
                    {
                        mdRegistrantsGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    registrantService.Delete( registrant );
                    rockContext.SaveChanges();
                }
            }

            BindRegistrantsGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            var registrationInstance = this.RegistrationInstance;

            if ( registrationInstance == null )
            {
                pnlDetails.Visible = false;
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                hfRegistrationInstanceId.Value = registrationInstance.Id.ToString();
                hfRegistrationTemplateId.Value = registrationInstance.RegistrationTemplateId.ToString();

                this.RegistrantFields = GetRegistrantFormFields().Where( a => a.IsGridField ).ToArray();

                SetUserPreferencePrefix( hfRegistrationTemplateId.ValueAsInt() );

                AddDynamicControls( true );

                BindRegistrantsFilter( registrationInstance );
                BindRegistrantsGrid();
            }
        }

        /// <summary>
        /// Sets the user preference prefix.
        /// </summary>
        private void SetUserPreferencePrefix( int registrationTemplateId )
        {
            fRegistrants.UserPreferenceKeyPrefix = string.Format( "{0}-", registrationTemplateId );
        }

        /// <summary>
        /// Binds the registrants filter.
        /// </summary>
        private void BindRegistrantsFilter( RegistrationInstance instance )
        {
            sdrpRegistrantsRegistrantDateRange.DelimitedValues = fRegistrants.GetUserPreference( UserPreferenceKeyBase.GridFilter_RegistrantsDateRange );
            tbRegistrantsRegistrantFirstName.Text = fRegistrants.GetUserPreference( UserPreferenceKeyBase.GridFilter_FirstName );
            tbRegistrantsRegistrantLastName.Text = fRegistrants.GetUserPreference( UserPreferenceKeyBase.GridFilter_LastName );
            ddlRegistrantsInGroup.SetValue( fRegistrants.GetUserPreference( UserPreferenceKeyBase.GridFilter_InGroup ) );

            ddlRegistrantsSignedDocument.SetValue( fRegistrants.GetUserPreference( UserPreferenceKeyBase.GridFilter_SignedDocument ) );
            ddlRegistrantsSignedDocument.Visible = instance != null && instance.RegistrationTemplate != null && instance.RegistrationTemplate.RequiredSignatureDocumentTemplateId.HasValue;
        }

        /// <summary>
        /// Binds the registrants grid.
        /// </summary>
        private void BindRegistrantsGrid( bool isExporting = false )
        {
            _isExporting = isExporting;

            int? instanceId = this.RegistrationInstanceId;

            if ( !instanceId.HasValue )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var registrationInstanceService = new RegistrationInstanceService( rockContext );

                var registrationInstance = registrationInstanceService.GetNoTracking( instanceId.Value );
                if ( registrationInstance == null )
                {
                    return;
                }

                var requiredSignatureDocumentTemplateId = registrationInstance.RegistrationTemplate.RequiredSignatureDocumentTemplateId;

                if ( requiredSignatureDocumentTemplateId.HasValue )
                {
                    SignersPersonAliasIds = new SignatureDocumentService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( d =>
                            d.SignatureDocumentTemplateId == requiredSignatureDocumentTemplateId.Value &&
                            d.Status == SignatureDocumentStatus.Signed &&
                            d.BinaryFileId.HasValue &&
                            d.AppliesToPersonAlias != null )
                        .OrderByDescending( d => d.LastStatusDate )
                        .Select( d => d.AppliesToPersonAlias.PersonId )
                        .ToList();
                }

                // Start query for registrants
                var registrationRegistrantService = new RegistrationRegistrantService( rockContext );
                var qry = registrationRegistrantService
                .Queryable( "PersonAlias.Person.PhoneNumbers.NumberTypeValue,Fees.RegistrationTemplateFee,GroupMember.Group" ).AsNoTracking()
                .Where( r =>
                    r.Registration.RegistrationInstanceId == instanceId.Value &&
                    r.PersonAlias != null &&
                    r.PersonAlias.Person != null &&
                    r.OnWaitList == false );

                // Filter by daterange
                var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( sdrpRegistrantsRegistrantDateRange.DelimitedValues );
                if ( dateRange.Start.HasValue )
                {
                    qry = qry.Where( r =>
                        r.CreatedDateTime.HasValue &&
                        r.CreatedDateTime.Value >= dateRange.Start.Value );
                }

                if ( dateRange.End.HasValue )
                {
                    qry = qry.Where( r =>
                        r.CreatedDateTime.HasValue &&
                        r.CreatedDateTime.Value < dateRange.End.Value );
                }

                // Filter by first name
                if ( !string.IsNullOrWhiteSpace( tbRegistrantsRegistrantFirstName.Text ) )
                {
                    string rfname = tbRegistrantsRegistrantFirstName.Text;
                    qry = qry.Where( r =>
                        r.PersonAlias.Person.NickName.StartsWith( rfname ) ||
                        r.PersonAlias.Person.FirstName.StartsWith( rfname ) );
                }

                // Filter by last name
                if ( !string.IsNullOrWhiteSpace( tbRegistrantsRegistrantLastName.Text ) )
                {
                    string rlname = tbRegistrantsRegistrantLastName.Text;
                    qry = qry.Where( r =>
                        r.PersonAlias.Person.LastName.StartsWith( rlname ) );
                }

                // Filter by signed documents
                if ( SignersPersonAliasIds != null )
                {
                    if ( ddlRegistrantsSignedDocument.SelectedValue.AsBooleanOrNull() == true )
                    {
                        qry = qry.Where( r => SignersPersonAliasIds.Contains( r.PersonAlias.PersonId ) );
                    }
                    else if ( ddlRegistrantsSignedDocument.SelectedValue.AsBooleanOrNull() == false )
                    {
                        qry = qry.Where( r => !SignersPersonAliasIds.Contains( r.PersonAlias.PersonId ) );
                    }
                }

                if ( ddlRegistrantsInGroup.SelectedValue.AsBooleanOrNull() == true )
                {
                    qry = qry.Where( r => r.GroupMemberId.HasValue );
                }
                else if ( ddlRegistrantsInGroup.SelectedValue.AsBooleanOrNull() == false )
                {
                    qry = qry.Where( r => !r.GroupMemberId.HasValue );
                }

                bool preloadCampusValues = false;
                var registrantAttributes = new List<AttributeCache>();
                var personAttributes = new List<AttributeCache>();
                var groupMemberAttributes = new List<AttributeCache>();
                var registrantAttributeIds = new List<int>();
                var personAttributesIds = new List<int>();
                var groupMemberAttributesIds = new List<int>();

                var personIds = qry.Select( r => r.PersonAlias.PersonId ).Distinct().ToList();

                if ( isExporting || ( RegistrantFields != null && RegistrantFields.Any( f => f.PersonFieldType != null && f.PersonFieldType == RegistrationPersonFieldType.Address ) ) )
                {
                    _homeAddresses = Person.GetHomeLocations( personIds );
                }

                _registrationTemplatePlacements = registrationInstance.RegistrationTemplate.Placements.OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();

                _placementsField.Visible = _registrationTemplatePlacements.Any();

                if ( _registrationTemplatePlacements.Any() )
                {
                    var registrationTemplatePlacementService = new RegistrationTemplatePlacementService( rockContext );
                    var instancePlacementGroupsQry = registrationInstanceService.GetRegistrationInstancePlacementGroups( registrationInstance );
                    _placementGroupInfoList = instancePlacementGroupsQry.AsNoTracking().Select( s => new
                    {
                        Group = s,
                        PersonIds = s.Members.Select( m => m.PersonId ).ToList()
                    } )
                        .ToList()
                        .Select( a => new PlacementGroupInfo
                        {
                            Group = a.Group,
                            RegistrationTemplatePlacementId = null,
                            PersonIds = a.PersonIds.ToArray(),
                        } ).ToList();

                    foreach ( var placementTemplate in registrationInstance.RegistrationTemplate.Placements )
                    {
                        var registrationTemplatePlacementPlacementGroupsQuery = registrationTemplatePlacementService.GetRegistrationTemplatePlacementPlacementGroups( placementTemplate );

                        var templatePlacementGroupInfoList = registrationTemplatePlacementPlacementGroupsQuery.AsNoTracking()
                            .Select( s => new
                            {
                                Group = s,
                                PersonIds = s.Members.Select( m => m.PersonId ).ToList()
                            } )
                            .ToList()
                            .Select( a => new PlacementGroupInfo
                            {
                                Group = a.Group,
                                RegistrationTemplatePlacementId = placementTemplate.Id,
                                PersonIds = a.PersonIds.ToArray()
                            } ).ToList();

                        _placementGroupInfoList = _placementGroupInfoList.Union( templatePlacementGroupInfoList ).ToList();
                    }
                }
                else
                {
                    _placementGroupInfoList = new List<PlacementGroupInfo>();
                }

                if ( RegistrantFields != null )
                {
                    _mobilePhoneNumbers = GetPersonMobilePhoneLookup( rockContext, this.RegistrantFields, personIds );
                    _homePhoneNumbers = GetPersonHomePhoneLookup( rockContext, this.RegistrantFields, personIds );

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

                                var ddlCampus = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_CAMPUS_ID ) as RockDropDownList;
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
                                var tbEmailFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_EMAIL_ID ) as RockTextBox;
                                if ( tbEmailFilter != null && !string.IsNullOrWhiteSpace( tbEmailFilter.Text ) )
                                {
                                    qry = qry.Where( r =>
                                        r.PersonAlias.Person.Email != null &&
                                        r.PersonAlias.Person.Email.Contains( tbEmailFilter.Text ) );
                                }

                                break;

                            case RegistrationPersonFieldType.Birthdate:
                                var drpBirthdateFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_BIRTHDATE_ID ) as DateRangePicker;
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
                                var tbMiddleNameFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_MIDDLE_NAME_ID ) as RockTextBox;
                                if ( tbMiddleNameFilter != null && !string.IsNullOrWhiteSpace( tbMiddleNameFilter.Text ) )
                                {
                                    qry = qry.Where( r =>
                                        r.PersonAlias.Person.MiddleName != null &&
                                        r.PersonAlias.Person.MiddleName.Contains( tbMiddleNameFilter.Text ) );
                                }

                                break;
                            case RegistrationPersonFieldType.AnniversaryDate:
                                var drpAnniversaryDateFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_ANNIVERSARY_DATE_ID ) as DateRangePicker;
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
                                var gpGradeFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_GRADE_ID ) as GradePicker;
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
                                var ddlGenderFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_GENDER_ID ) as RockDropDownList;
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
                                var dvpMaritalStatusFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_MARTIAL_STATUS_ID ) as DefinedValuePicker;
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
                                var tbMobilePhoneFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_MOBILE_PHONE_ID ) as RockTextBox;
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
                                var tbHomePhoneFilter = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_HOME_PHONE_ID ) as RockTextBox;
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
                            var filterControl = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_ATTRIBUTE_PREFIX + attribute.Id.ToString() );
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
                            var filterControl = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_ATTRIBUTE_PREFIX + attribute.Id.ToString() );
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
                            var filterControl = phRegistrantsRegistrantFormFieldFilters.FindControl( FILTER_ATTRIBUTE_PREFIX + attribute.Id.ToString() );
                            groupMemberQry = attribute.FieldType.Field.ApplyAttributeQueryFilter( groupMemberQry, filterControl, attribute, groupMemberService, Rock.Reporting.FilterMode.SimpleFilter );
                        }

                        qry = qry.Where( r => groupMemberQry.Any( g => g.Id == r.GroupMemberId ) );
                    }
                }

                // Sort the query
                IOrderedQueryable<RegistrationRegistrant> orderedQry = null;
                SortProperty sortProperty = gRegistrants.SortProperty;
                if ( sortProperty != null )
                {
                    orderedQry = qry.Sort( sortProperty );
                }
                else
                {
                    orderedQry = qry
                        .OrderBy( r => r.PersonAlias.Person.LastName )
                        .ThenBy( r => r.PersonAlias.Person.NickName );
                }

                // increase the timeout just in case. A complex filter on the grid might slow things down
                rockContext.Database.CommandTimeout = 180;

                // Set the grids LinqDataSource which will run query and set results for current page
                gRegistrants.SetLinqDataSource<RegistrationRegistrant>( orderedQry );

                if ( RegistrantFields != null )
                {
                    // Get the query results for the current page
                    var currentPageRegistrants = gRegistrants.DataSource as List<RegistrationRegistrant>;
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

                            string linkedPageUrl = LinkedPageUrl( AttributeKey.GroupDetailPage, new Dictionary<string, string> { { "GroupId", groupMember.GroupId.ToString() } } );
                            GroupLinks.AddOrIgnore(
                                groupMember.GroupId,
                                isExporting ? groupMember.Group.Name : string.Format( "<a href='{0}'>{1}</a>", linkedPageUrl, groupMember.Group.Name ) );
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
                            gRegistrants.ObjectList = new Dictionary<string, object>();

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
                                gRegistrants.ObjectList.Add( registrant.Id.ToString(), attributeFieldObject );
                            }
                        }
                    }
                }

                gRegistrants.DataBind();
            }
        }

        /// <summary>
        /// Add dynamically-generated controls to the form.
        /// </summary>
        /// <param name="setValues"></param>
        private void AddDynamicControls( bool setValues )
        {
            AddRegistrationTemplateFieldsToGrid( this.RegistrantFields, phRegistrantsRegistrantFormFieldFilters, gRegistrants, fRegistrants, setValues, false );

            var feeField = new RockLiteralField();
            feeField.ID = "lFees";
            feeField.HeaderText = "Fees";
            gRegistrants.Columns.Add( feeField );

            _placementsField = new RockLiteralField();
            _placementsField.ID = "lPlacements";
            _placementsField.HeaderText = "Placements";
            gRegistrants.Columns.Add( _placementsField );

            var deleteField = new DeleteField();
            gRegistrants.Columns.Add( deleteField );
            deleteField.Click += gRegistrants_Delete;
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

        #region classes

        /// <summary>
        ///
        /// </summary>
        [System.Diagnostics.DebuggerDisplay( "{Group.Name}, RegistrationTemplatePlacementId = {RegistrationTemplatePlacementId} " )]
        protected class PlacementGroupInfo
        {
            /// <summary>
            /// Gets or sets the group.
            /// </summary>
            /// <value>
            /// The group.
            /// </value>
            public Group Group { get; set; }

            /// <summary>
            /// Gets or sets the registration template placement identifier.
            /// </summary>
            /// <value>
            /// The registration template placement identifier.
            /// </value>
            public int? RegistrationTemplatePlacementId { get; set; }

            /// <summary>
            /// Gets or sets the person ids.
            /// </summary>
            /// <value>
            /// The person ids.
            /// </value>
            public int[] PersonIds { get; set; }
        }

        #endregion classes
    }
}