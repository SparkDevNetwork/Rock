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
using System.Runtime.Serialization;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Web.UI
{
    /// <summary>
    /// Provides base functionality for a block that displays information about a registration instance.
    /// </summary>
    [ContextAware( typeof( RegistrationInstance ) )]
    public abstract class RegistrationInstanceBlock : ContextEntityBlock
    {
        #region Shared Item Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        protected static class SharedItemKey
        {
            /// <summary>
            /// The linked page used to display registration details.
            /// </summary>
            public const string RegistrationDateRange = "RegistrationDateRange";

            /// <summary>
            /// Should discount codes be displayed in the list?
            /// </summary>
            public const string DisplayDiscountCodes = "DisplayDiscountCodes";
        }

        #endregion Shared Item Keys

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters.
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The Registration Instance identifier
            /// </summary>
            public const string RegistrationInstanceId = "RegistrationInstanceId";

            /// <summary>
            /// The Registration Template identifier.
            /// </summary>
            public const string RegistrationTemplateId = "RegistrationTemplateId";

            /// <summary>
            /// The registration identifier
            /// </summary>
            public const string RegistrationId = "RegistrationId";

            /// <summary>
            /// The registrant identifier
            /// </summary>
            public const string RegistrantId = "RegistrantId";
        }

        #endregion Page Parameter Keys

        #region ViewState Keys

        /// <summary>
        /// Keys to use for ViewState
        /// </summary>
        protected static class ViewStateKeyBase
        {
            /// <summary>
            /// The registrant fields
            /// </summary>
            public const string RegistrantFields = "RegistrantFields";

            /// <summary>
            /// The available registration attribute ids for grid
            /// </summary>
            public const string AvailableRegistrationAttributeIdsForGrid = "AvailableRegistrationAttributeIdsForGrid";
        }

        #endregion ViewState Keys

        #region User Preference Keys

        /// <summary>
        /// Keys to use for User Preferences
        /// NOTE: This is a not a static class since RegistrationInstance blocks will need to be able to inherit from thois
        /// </summary>
        protected static class UserPreferenceKeyBase
        {
            /// <summary>
            /// The grid filter grade
            /// </summary>
            public const string GridFilter_Grade = "Grade";

            /// <summary>
            /// The grid filter middle name
            /// </summary>
            public const string GridFilter_MiddleName = "MiddleName";

            /// <summary>
            /// The grid filter home phone
            /// </summary>
            public const string GridFilter_HomePhone = "HomePhone";

            /// <summary>
            /// The grid filter cell phone
            /// </summary>
            public const string GridFilter_CellPhone = "CellPhone";

            /// <summary>
            /// The grid filter email
            /// </summary>
            public const string GridFilter_Email = "Email";

            /// <summary>
            /// The grid filter marital status
            /// </summary>
            public const string GridFilter_MaritalStatus = "Marital Status";

            /// <summary>
            /// The grid filter birthdate range
            /// </summary>
            public const string GridFilter_BirthdateRange = "Birthdate Range";

            /// <summary>
            /// The grid filter anniversary date range
            /// </summary>
            public const string GridFilter_AnniversaryDateRange = "AnniversaryDate Range";

            /// <summary>
            /// The grid filter gender
            /// </summary>
            public const string GridFilter_Gender = "Gender";

            /// <summary>
            /// The grid filter home campus
            /// </summary>
            public const string GridFilter_HomeCampus = "Home Campus";

            /// <summary>
            /// The grid filter date range
            /// </summary>
            public const string GridFilter_DateRange = "Date Range";

            /// <summary>
            /// The grid filter first name
            /// </summary>
            public const string GridFilter_FirstName = "First Name";

            /// <summary>
            /// The grid filter last name
            /// </summary>
            public const string GridFilter_LastName = "Last Name";

            /// <summary>
            /// The grid filter registrants date range
            /// </summary>
            public const string GridFilter_RegistrantsDateRange = "Registrants DateRange";

            /// <summary>
            /// The grid filter in group
            /// </summary>
            public const string GridFilter_InGroup = "In Group";

            /// <summary>
            /// The grid filter signed document
            /// </summary>
            public const string GridFilter_SignedDocument = "Signed Document";

            /// <summary>
            /// The grid filter fee date range
            /// </summary>
            public const string GridFilter_FeeDateRange = "Fee Date Range";

            /// <summary>
            /// The grid filter fee options
            /// </summary>
            public const string GridFilter_FeeOptions = "Fee Options";

            /// <summary>
            /// The grid filter fee name
            /// </summary>
            public const string GridFilter_FeeName = "Fee Name";

            /// <summary>
            /// The grid filter discount date range
            /// </summary>
            public const string GridFilter_DiscountDateRange = "Discount Date Range";

            /// <summary>
            /// The grid filter discount code
            /// </summary>
            public const string GridFilter_DiscountCode = "Discount Code";

            /// <summary>
            /// The grid filter discount code search
            /// </summary>
            public const string GridFilter_DiscountCodeSearch = "Discount Code Search";

            /// <summary>
            /// The grid filter campus
            /// </summary>
            public const string GridFilter_Campus = "Campus";

            /// <summary>
            /// The grid filter payments date range
            /// </summary>
            public const string GridFilter_PaymentsDateRange = "Payments Date Range";

            /// <summary>
            /// The grid filter registrations date range
            /// </summary>
            public const string GridFilter_RegistrationsDateRange = "Registrations Date Range";

            /// <summary>
            /// The grid filter payment status
            /// </summary>
            public const string GridFilter_PaymentStatus = "Payment Status";

            /// <summary>
            /// The grid filter registered by first name
            /// </summary>
            public const string GridFilter_RegisteredByFirstName = "Registered By First Name";

            /// <summary>
            /// The grid filter registered by last name
            /// </summary>
            public const string GridFilter_RegisteredByLastName = "Registered By Last Name";

            /// <summary>
            /// The grid filter registrant first name
            /// </summary>
            public const string GridFilter_RegistrantFirstName = "Registrant First Name";

            /// <summary>
            /// The grid filter registrant last name
            /// </summary>
            public const string GridFilter_RegistrantLastName = "Registrant Last Name";
        }

        #endregion User Preference Keys

        #region Properties and Fields

        /// <summary>
        /// Filter Campus Identifier
        /// </summary>
        protected const string FILTER_CAMPUS_ID = "ddlCampus";

        /// <summary>
        /// Filter Email Identifier
        /// </summary>
        protected const string FILTER_EMAIL_ID = "tbEmailFilter";

        /// <summary>
        /// Filter BirthDate Identifier
        /// </summary>
        protected const string FILTER_BIRTHDATE_ID = "drpBirthdateFilter";

        /// <summary>
        /// Filter Middle name Identifier
        /// </summary>
        protected const string FILTER_MIDDLE_NAME_ID = "tbMiddleNameFilter";

        /// <summary>
        /// Filter Anniversary date Identifier
        /// </summary>
        protected const string FILTER_ANNIVERSARY_DATE_ID = "drpAnniversaryDateFilter";

        /// <summary>
        /// Filter grade Identifier
        /// </summary>
        protected const string FILTER_GRADE_ID = "gpGradeFilter";

        /// <summary>
        /// Filter gender Identifier
        /// </summary>
        protected const string FILTER_GENDER_ID = "ddlGenderFilter";

        /// <summary>
        /// Filter martial status Identifier
        /// </summary>
        protected const string FILTER_MARTIAL_STATUS_ID = "dvpMaritalStatusFilter";

        /// <summary>
        /// Filter mmobile phone Identifier
        /// </summary>
        protected const string FILTER_MOBILE_PHONE_ID = "tbMobilePhoneFilter";

        /// <summary>
        /// Filter home phone Identifier
        /// </summary>
        protected const string FILTER_HOME_PHONE_ID = "tbHomePhoneFilter";

        /// <summary>
        /// Filter attribute prefix
        /// </summary>
        protected const string FILTER_ATTRIBUTE_PREFIX = "filterAttribute_";

        /// <summary>
        /// The active RegistrationInstance in this context.
        /// </summary>
        public RegistrationInstance RegistrationInstance
        {
            get => Entity as RegistrationInstance;
            set => Entity = value;
        }

        /// <summary>
        /// Gets or sets the registration instance identifier.
        /// </summary>
        /// <value>
        /// The registration instance identifier.
        /// </value>
        protected int? RegistrationInstanceId { get; private set; }

        /// <summary>
        /// Gets or sets the registration template identifier.
        /// </summary>
        /// <value>
        /// The registration template identifier.
        /// </value>
        protected int? RegistrationTemplateId
        {
            get
            {
                int? registrationTemplateId = this.RegistrationInstance?.RegistrationTemplateId;

                if ( registrationTemplateId == null )
                {
                    registrationTemplateId = this.PageParameter( PageParameterKey.RegistrationTemplateId ).AsIntegerOrNull();
                }

                return registrationTemplateId;
            }
        }

        /// <summary>
        /// Does the current user have permission to edit the block and its specific content?
        /// </summary>
        protected bool UserCanEditBlockContent
        {
            get
            {
                var registrationInstance = this.RegistrationInstance;

                if ( registrationInstance == null )
                {
                    return false;
                }

                return this.UserCanEdit
                    || registrationInstance.IsAuthorized( Authorization.EDIT, CurrentPerson )
                    || registrationInstance.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
            }
        }

        private bool? _registrationInstanceHasPayments;

        /// <summary>
        /// Does the active Registration Instance have associated payments?
        /// </summary>
        protected bool RegistrationInstanceHasPayments
        {
            get
            {
                if ( !_registrationInstanceHasPayments.HasValue )
                {
                    var registrationInstance = this.RegistrationInstance;

                    if ( registrationInstance != null )
                    {
                        var dataContext = new RockContext();

                        var registrationIdQry = new RegistrationService( dataContext )
                            .Queryable().AsNoTracking()
                            .Where( r =>
                                r.RegistrationInstanceId == registrationInstance.Id &&
                                !r.IsTemporary )
                            .Select( r => r.Id );

                        var registrationEntityType = EntityTypeCache.Get( typeof( Rock.Model.Registration ) );

                        _registrationInstanceHasPayments = new FinancialTransactionDetailService( dataContext )
                            .Queryable().AsNoTracking()
                            .Where( d =>
                                d.EntityTypeId.HasValue &&
                                d.EntityId.HasValue &&
                                d.EntityTypeId.Value == registrationEntityType.Id &&
                                registrationIdQry.Contains( d.EntityId.Value ) )
                            .Any();
                    }
                }

                return _registrationInstanceHasPayments.GetValueOrDefault();
            }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RegistrationInstanceId = PageParameter( PageParameterKey.RegistrationInstanceId ).AsIntegerOrNull();

            if ( RegistrationInstance == null
                 && RegistrationInstanceId.HasValue )
            {
                RegistrationInstance = GetSharedRegistrationInstance( this.RegistrationInstanceId.Value );

                if ( RegistrationInstance != null )
                {
                    RegistrationInstance?.LoadAttributes();
                }
            }
        }

        #endregion

        #region Secondary Block Functions

        /// <summary>
        /// A prefix added to the filter key. 
        /// </summary>
        protected string FilterPreferenceKeyPrefix { get; set; }

        /// <summary>
        /// Adds the filter controls and grid columns for all of the registration template's form fields.
        /// that were configured to 'Show on Grid'
        /// </summary>
        protected void AddRegistrationTemplateFieldsToGrid( RegistrantFormField[] registrantFields, PlaceHolder filterFieldsContainer, Grid grid, GridFilter gridFilter, bool setValues, bool disableAddressFieldExport )
        {
            filterFieldsContainer.Controls.Clear();

            ClearGrid( grid );

            string dataFieldExpression = string.Empty;

            if ( registrantFields != null )
            {
                foreach ( var field in registrantFields )
                {
                    if ( field.FieldSource == RegistrationFieldSource.PersonField
                         && field.PersonFieldType.HasValue )
                    {
                        switch ( field.PersonFieldType.Value )
                        {
                            case RegistrationPersonFieldType.Campus:
                                {
                                    var ddlCampus = new RockDropDownList();
                                    ddlCampus.ID = FILTER_CAMPUS_ID;
                                    ddlCampus.Label = "Home Campus";
                                    ddlCampus.DataValueField = "Id";
                                    ddlCampus.DataTextField = "Name";
                                    ddlCampus.DataSource = CampusCache.All();
                                    ddlCampus.DataBind();
                                    ddlCampus.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );

                                    if ( setValues )
                                    {
                                        ddlCampus.SetValue( gridFilter.GetUserPreference( UserPreferenceKeyBase.GridFilter_HomeCampus ) );
                                    }

                                    filterFieldsContainer.Controls.Add( ddlCampus );

                                    var templateField = new RockLiteralField();
                                    templateField.ID = "lCampus";
                                    templateField.HeaderText = "Campus";
                                    grid.Columns.Add( templateField );
                                }

                                break;

                            case RegistrationPersonFieldType.Email:
                                {
                                    var tbEmailFilter = new RockTextBox();
                                    tbEmailFilter.ID = FILTER_EMAIL_ID;
                                    tbEmailFilter.Label = "Email";

                                    if ( setValues )
                                    {
                                        tbEmailFilter.Text = gridFilter.GetUserPreference( UserPreferenceKeyBase.GridFilter_Email );
                                    }

                                    filterFieldsContainer.Controls.Add( tbEmailFilter );

                                    dataFieldExpression = "PersonAlias.Person.Email";

                                    var emailField = new RockBoundField();
                                    emailField.DataField = dataFieldExpression;
                                    emailField.HeaderText = "Email";
                                    emailField.SortExpression = dataFieldExpression;
                                    grid.Columns.Add( emailField );
                                }

                                break;

                            case RegistrationPersonFieldType.Birthdate:
                                {
                                    var drpBirthdateFilter = new DateRangePicker();
                                    drpBirthdateFilter.ID = FILTER_BIRTHDATE_ID;
                                    drpBirthdateFilter.Label = "Birthdate Range";

                                    if ( setValues )
                                    {
                                        drpBirthdateFilter.DelimitedValues = gridFilter.GetUserPreference( UserPreferenceKeyBase.GridFilter_BirthdateRange );
                                    }

                                    filterFieldsContainer.Controls.Add( drpBirthdateFilter );

                                    dataFieldExpression = "PersonAlias.Person.BirthDate";

                                    var birthdateField = new DateField();
                                    birthdateField.DataField = dataFieldExpression;
                                    birthdateField.HeaderText = "Birthdate";
                                    birthdateField.IncludeAge = true;
                                    birthdateField.SortExpression = dataFieldExpression;
                                    grid.Columns.Add( birthdateField );
                                }

                                break;
                            case RegistrationPersonFieldType.MiddleName:
                                {
                                    var tbMiddleNameFilter = new RockTextBox();
                                    tbMiddleNameFilter.ID = FILTER_MIDDLE_NAME_ID;
                                    tbMiddleNameFilter.Label = "MiddleName";

                                    if ( setValues )
                                    {
                                        tbMiddleNameFilter.Text = gridFilter.GetUserPreference( UserPreferenceKeyBase.GridFilter_MiddleName );
                                    }

                                    filterFieldsContainer.Controls.Add( tbMiddleNameFilter );

                                    dataFieldExpression = "PersonAlias.Person.MiddleName";

                                    var middleNameField = new RockBoundField();
                                    middleNameField.DataField = dataFieldExpression;
                                    middleNameField.HeaderText = "MiddleName";
                                    middleNameField.SortExpression = dataFieldExpression;
                                    grid.Columns.Add( middleNameField );
                                }

                                break;

                            case RegistrationPersonFieldType.AnniversaryDate:
                                {
                                    var drpAnniversaryDateFilter = new DateRangePicker();
                                    drpAnniversaryDateFilter.ID = FILTER_ANNIVERSARY_DATE_ID;
                                    drpAnniversaryDateFilter.Label = "AnniversaryDate Range";

                                    if ( setValues )
                                    {
                                        drpAnniversaryDateFilter.DelimitedValues = gridFilter.GetUserPreference( UserPreferenceKeyBase.GridFilter_AnniversaryDateRange );
                                    }

                                    filterFieldsContainer.Controls.Add( drpAnniversaryDateFilter );

                                    dataFieldExpression = "PersonAlias.Person.AnniversaryDate";

                                    var anniversaryDateField = new DateField();
                                    anniversaryDateField.DataField = dataFieldExpression;
                                    anniversaryDateField.HeaderText = "Anniversary Date";
                                    anniversaryDateField.IncludeAge = true;
                                    anniversaryDateField.SortExpression = dataFieldExpression;
                                    grid.Columns.Add( anniversaryDateField );
                                }

                                break;
                            case RegistrationPersonFieldType.Grade:
                                {
                                    var gpGradeFilter = new GradePicker();
                                    gpGradeFilter.ID = FILTER_GRADE_ID;
                                    gpGradeFilter.Label = "Grade";
                                    gpGradeFilter.UseAbbreviation = true;
                                    gpGradeFilter.UseGradeOffsetAsValue = true;
                                    gpGradeFilter.CssClass = "input-width-md";

                                    // Since 12th grade is the 0 Value, we need to handle the "no user preference" differently
                                    // by not calling SetValue otherwise it will select 12th grade.
                                    if ( setValues )
                                    {
                                        var groupPlacementsGradeUserPreference = gridFilter.GetUserPreference( UserPreferenceKeyBase.GridFilter_Grade ).AsIntegerOrNull();
                                        if ( groupPlacementsGradeUserPreference != null )
                                        {
                                            gpGradeFilter.SetValue( groupPlacementsGradeUserPreference );
                                        }
                                    }

                                    filterFieldsContainer.Controls.Add( gpGradeFilter );

                                    // 2017-01-13 as discussed, changing this to Grade but keeping the sort based on grad year
                                    dataFieldExpression = "PersonAlias.Person.GradeFormatted";

                                    var gradeField = new RockBoundField();
                                    gradeField.DataField = dataFieldExpression;
                                    gradeField.HeaderText = "Grade";
                                    grid.Columns.Add( gradeField );
                                }

                                break;

                            case RegistrationPersonFieldType.Gender:
                                {
                                    var ddlGenderFilter = new RockDropDownList();
                                    ddlGenderFilter.BindToEnum<Gender>( true );
                                    ddlGenderFilter.ID = FILTER_GENDER_ID;
                                    ddlGenderFilter.Label = "Gender";

                                    if ( setValues )
                                    {
                                        ddlGenderFilter.SetValue( gridFilter.GetUserPreference( UserPreferenceKeyBase.GridFilter_Gender ) );
                                    }

                                    filterFieldsContainer.Controls.Add( ddlGenderFilter );

                                    dataFieldExpression = "PersonAlias.Person.Gender";

                                    var genderField = new EnumField();
                                    genderField.DataField = dataFieldExpression;
                                    genderField.HeaderText = "Gender";
                                    genderField.SortExpression = dataFieldExpression;
                                    grid.Columns.Add( genderField );
                                }

                                break;

                            case RegistrationPersonFieldType.MaritalStatus:
                                {
                                    var dvpMaritalStatusFilter = new DefinedValuePicker();
                                    dvpMaritalStatusFilter.ID = FILTER_MARTIAL_STATUS_ID;
                                    dvpMaritalStatusFilter.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid() ).Id;
                                    dvpMaritalStatusFilter.Label = "Marital Status";

                                    if ( setValues )
                                    {
                                        dvpMaritalStatusFilter.SetValue( gridFilter.GetUserPreference( UserPreferenceKeyBase.GridFilter_MaritalStatus ) );
                                    }

                                    filterFieldsContainer.Controls.Add( dvpMaritalStatusFilter );

                                    dataFieldExpression = "PersonAlias.Person.MaritalStatusValue.Value";

                                    var maritalStatusField = new RockBoundField();
                                    maritalStatusField.DataField = dataFieldExpression;
                                    maritalStatusField.HeaderText = "MaritalStatus";
                                    maritalStatusField.SortExpression = dataFieldExpression;
                                    grid.Columns.Add( maritalStatusField );
                                }

                                break;

                            case RegistrationPersonFieldType.MobilePhone:
                                {
                                    // Per discussion this should not have "Phone" appended to the end if it's missing.
                                    var mobileLabel = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Value;

                                    var tbMobilePhoneFilter = new RockTextBox();
                                    tbMobilePhoneFilter.ID = FILTER_MOBILE_PHONE_ID;
                                    tbMobilePhoneFilter.Label = mobileLabel;

                                    if ( setValues )
                                    {
                                        tbMobilePhoneFilter.Text = gridFilter.GetUserPreference( UserPreferenceKeyBase.GridFilter_CellPhone );
                                    }

                                    filterFieldsContainer.Controls.Add( tbMobilePhoneFilter );

                                    var phoneNumbersField = new RockLiteralField();
                                    phoneNumbersField.ID = "lMobile";
                                    phoneNumbersField.HeaderText = mobileLabel;
                                    grid.Columns.Add( phoneNumbersField );
                                }

                                break;

                            case RegistrationPersonFieldType.HomePhone:
                                {
                                    // Per discussion this should not have "Phone" appended to the end if it's missing.
                                    var homePhoneLabel = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME ).Value;

                                    var tbHomePhoneFilter = new RockTextBox();
                                    tbHomePhoneFilter.ID = FILTER_HOME_PHONE_ID;
                                    tbHomePhoneFilter.Label = homePhoneLabel;

                                    if ( setValues )
                                    {
                                        tbHomePhoneFilter.Text = gridFilter.GetUserPreference( UserPreferenceKeyBase.GridFilter_HomePhone );
                                    }

                                    filterFieldsContainer.Controls.Add( tbHomePhoneFilter );

                                    var homePhoneNumbersField = new RockLiteralField();
                                    homePhoneNumbersField.ID = "lHomePhone";
                                    homePhoneNumbersField.HeaderText = homePhoneLabel;
                                    grid.Columns.Add( homePhoneNumbersField );
                                }

                                break;

                            case RegistrationPersonFieldType.Address:
                                {
                                    var addressField = new RockLiteralField();
                                    addressField.HeaderText = "Address";

                                    // If the grid has specific Street1, Street2, City, etc. fields included, do not export the full address column.
                                    if ( disableAddressFieldExport )
                                    {
                                        addressField.ExcelExportBehavior = ExcelExportBehavior.NeverInclude;
                                    }

                                    addressField.ID = "lGroupPlacementsAddress";
                                    grid.Columns.Add( addressField );
                                }

                                break;
                        }
                    }
                    else if ( field.Attribute != null )
                    {
                        var attribute = field.Attribute;

                        // Add dynamic filter fields
                        var filterFieldControl = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, FILTER_ATTRIBUTE_PREFIX + attribute.Id.ToString(), false, Rock.Reporting.FilterMode.SimpleFilter );
                        if ( filterFieldControl != null )
                        {
                            if ( filterFieldControl is IRockControl )
                            {
                                var rockControl = ( IRockControl ) filterFieldControl;
                                rockControl.Label = attribute.Name;
                                rockControl.Help = attribute.Description;
                                filterFieldsContainer.Controls.Add( filterFieldControl );
                            }
                            else
                            {
                                var wrapper = new RockControlWrapper();
                                wrapper.ID = filterFieldControl.ID + "_wrapper";
                                wrapper.Label = attribute.Name;
                                wrapper.Controls.Add( filterFieldControl );
                                filterFieldsContainer.Controls.Add( wrapper );
                            }

                            if ( setValues )
                            {
                                string savedValue = gridFilter.GetUserPreference( attribute.Key );
                                if ( !string.IsNullOrWhiteSpace( savedValue ) )
                                {
                                    try
                                    {
                                        var values = JsonConvert.DeserializeObject<List<string>>( savedValue );
                                        attribute.FieldType.Field.SetFilterValues( filterFieldControl, attribute.QualifierValues, values );
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }

                        dataFieldExpression = attribute.Id.ToString() + attribute.Key;

                        bool columnExists = grid.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;

                        if ( !columnExists )
                        {
                            AttributeField boundField = new AttributeField();
                            boundField.DataField = dataFieldExpression;
                            boundField.AttributeId = attribute.Id;
                            boundField.HeaderText = attribute.Name;

                            var attributeCache = Rock.Web.Cache.AttributeCache.Get( attribute.Id );
                            if ( attributeCache != null )
                            {
                                boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                            }

                            grid.Columns.Add( boundField );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get the collection of fields (PersonFields, RegistrantFields, GroupMemberFields, RegistrationFields) that are included in the forms associated with the registration template.
        /// </summary>
        /// <returns></returns>
        protected RegistrantFormField[] GetRegistrantFormFields()
        {
            var fields = new List<RegistrantFormField>();

            if ( !this.RegistrationTemplateId.HasValue )
            {
                return new RegistrantFormField[0];
            }

            List<RegistrationTemplateForm> registrationTemplateForms = new RegistrationTemplateService( new RockContext() ).GetSelect( this.RegistrationTemplateId.Value, s => s.Forms )?.ToList();

            if ( registrationTemplateForms == null )
            {
                return new RegistrantFormField[0];
            }

            foreach ( var form in registrationTemplateForms )
            {
                var formFields = form.Fields
                    .OrderBy( f => f.Order )
                    .ToList();

                foreach ( var formField in formFields )
                {
                    if ( formField.FieldSource == RegistrationFieldSource.PersonField )
                    {
                        if ( formField.PersonFieldType != RegistrationPersonFieldType.FirstName &&
                            formField.PersonFieldType != RegistrationPersonFieldType.LastName )
                        {
                            fields.Add(
                                new RegistrantFormField
                                {
                                    FieldSource = formField.FieldSource,
                                    PersonFieldType = formField.PersonFieldType,
                                    IsGridField = formField.IsGridField
                                } );
                        }
                    }
                    else
                    {
                        fields.Add(
                            new RegistrantFormField
                            {
                                FieldSource = formField.FieldSource,
                                AttributeId = formField.AttributeId.Value,
                                IsGridField = formField.IsGridField
                            } );
                    }
                }
            }

            return fields.ToArray();
        }

        /// <summary>
        /// Remove the dynamic columns from the grid.
        /// </summary>
        /// <param name="grid"></param>
        protected void ClearGrid( Grid grid )
        {
            // Remove any of the dynamic person fields
            var dynamicColumns = new List<string> {
                "PersonAlias.Person.BirthDate",
            };
            foreach ( var column in grid.Columns
                .OfType<BoundField>()
                .Where( c => dynamicColumns.Contains( c.DataField ) )
                .ToList() )
            {
                grid.Columns.Remove( column );
            }

            // Remove any of the dynamic attribute fields
            foreach ( var column in grid.Columns
                .OfType<AttributeField>()
                .ToList() )
            {
                grid.Columns.Remove( column );
            }

            // Remove the fees field
            foreach ( var column in grid.Columns
                .OfType<TemplateField>()
                .Where( c => c.HeaderText == "Fees" )
                .ToList() )
            {
                grid.Columns.Remove( column );
            }

            // Remove the delete field
            foreach ( var column in grid.Columns
                .OfType<DeleteField>()
                .ToList() )
            {
                grid.Columns.Remove( column );
            }

            // Remove the group picker field
            foreach ( var column in grid.Columns
                .OfType<GroupPickerField>()
                .ToList() )
            {
                grid.Columns.Remove( column );
            }
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Gets the shared registration instance.
        /// </summary>
        /// <param name="registrationInstanceId">The registration instance identifier.</param>
        /// <returns></returns>
        public RegistrationInstance GetSharedRegistrationInstance( int registrationInstanceId )
        {
            string key = string.Format( "RegistrationInstance:{0}", registrationInstanceId );

            var registrationInstance = RockPage.GetSharedItem( key ) as RegistrationInstance;

            if ( registrationInstance == null )
            {
                registrationInstance = GetRegistrationInstance( registrationInstanceId, new RockContext() );
                RockPage.SaveSharedItem( key, registrationInstance );
            }

            return registrationInstance;
        }

        /// <summary>
        /// Load the registration instance data, but do not populate the display properties.
        /// Use this method to load data for postback processing.
        /// </summary>
        /// <param name="registrationInstanceId">The registration instance identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        protected RegistrationInstance GetRegistrationInstance( int? registrationInstanceId, RockContext rockContext )
        {
            if ( !registrationInstanceId.HasValue || registrationInstanceId.Value == 0 )
            {
                return null;
            }

            var registrationInstance = new RegistrationInstanceService( rockContext )
                .Queryable()
                .Include( a => a.RegistrationTemplate )
                .Include( a => a.Account )
                .Include( a => a.RegistrationTemplate.Forms )
                .Include( a => a.RegistrationTemplate.Forms.Select( s => s.Fields ) )
                .Where( a => a.Id == registrationInstanceId.Value )
                .AsNoTracking().FirstOrDefault();

            if ( registrationInstance == null )
            {
                return null;
            }

            // Load the Registration Template.
            if ( registrationInstance.RegistrationTemplate == null
                 && registrationInstance.RegistrationTemplateId > 0 )
            {
                registrationInstance.RegistrationTemplate = new RegistrationTemplateService( rockContext )
                    .Get( registrationInstance.RegistrationTemplateId );
            }

            return registrationInstance;
        }

        /// <summary>
        /// Get a dictionary lookup of person phone numbers.
        /// </summary>
        /// <param name="rockContext"></param>
        /// <param name="registrantFields"></param>
        /// <param name="personIds"></param>
        /// <returns></returns>
        protected Dictionary<int, PhoneNumber> GetPersonHomePhoneLookup( RockContext rockContext, IEnumerable<RegistrantFormField> registrantFields, List<int> personIds )
        {
            return GetPersonPhoneDictionary( rockContext, registrantFields, personIds, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME, RegistrationPersonFieldType.HomePhone );
        }

        /// <summary>
        /// Get a dictionary lookup of person phone numbers.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="registrantFields">The registrant fields.</param>
        /// <param name="personIds">The person ids.</param>
        /// <returns></returns>
        protected Dictionary<int, PhoneNumber> GetPersonMobilePhoneLookup( RockContext rockContext, IEnumerable<RegistrantFormField> registrantFields, List<int> personIds )
        {
            return GetPersonPhoneDictionary( rockContext, registrantFields, personIds, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE, RegistrationPersonFieldType.MobilePhone );
        }

        /// <summary>
        /// Get a dictionary lookup of person phone numbers.
        /// </summary>
        /// <param name="rockContext"></param>
        /// <param name="registrantFields"></param>
        /// <param name="personIds"></param>
        /// <param name="phoneTypeGuid"></param>
        /// <param name="registrationFieldType"></param>
        /// <returns></returns>
        private Dictionary<int, PhoneNumber> GetPersonPhoneDictionary( RockContext rockContext, IEnumerable<RegistrantFormField> registrantFields, List<int> personIds, string phoneTypeGuid, RegistrationPersonFieldType registrationFieldType )
        {
            var phoneNumbers = new Dictionary<int, PhoneNumber>();

            if ( registrantFields.Any( f => f.PersonFieldType != null && f.PersonFieldType == registrationFieldType ) )
            {
                var phoneNumberService = new PhoneNumberService( rockContext );
                foreach ( var personId in personIds )
                {
                    phoneNumbers[personId] = phoneNumberService.GetNumberByPersonIdAndType( personId, phoneTypeGuid );
                }
            }

            return phoneNumbers;
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Helper class for tracking registration form fields
        /// </summary>
        [Serializable]
        public class RegistrantFormField
        {
            /// <summary>
            /// Gets or sets the field source.
            /// </summary>
            /// <value>
            /// The field source.
            /// </value>
            [DataMember]
            public RegistrationFieldSource FieldSource { get; set; }

            /// <summary>
            /// Gets or sets the type of the person field.
            /// </summary>
            /// <value>
            /// The type of the person field.
            /// </value>
            [DataMember]
            public RegistrationPersonFieldType? PersonFieldType { get; set; }

            /// <summary>
            /// Gets or sets the attribute identifier.
            /// </summary>
            /// <value>
            /// The attribute identifier.
            /// </value>
            [DataMember]
            public int AttributeId { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is grid field.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is grid field; otherwise, <c>false</c>.
            /// </value>
            [DataMember]
            public bool IsGridField { get; set; }

            /// <summary>
            /// Gets or sets the attribute.
            /// </summary>
            /// <value>
            /// The attribute.
            /// </value>
            [IgnoreDataMember]
            public AttributeCache Attribute => AttributeCache.Get( AttributeId );
        }

        #endregion
    }
}
