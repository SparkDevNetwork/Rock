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
using System.Linq;
using System.Data.Entity;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using System.Collections.Generic;
using Rock.Web;
using Rock.Security;
using Rock.Web.UI.Controls;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

namespace Rock.Blocks.Types.Web.Events
{
    /// <summary>
    /// Provides base functionality for a block that displays information about a registration instance.
    /// </summary>
    [ContextAware( typeof( RegistrationInstance ) )]
    public abstract class RegistrationInstanceBlock : ContextEntityBlock
    {
        /// <summary>
        /// The Registration Instance identifier
        /// </summary>
        private const string PageParameterRegistrationInstanceId = "RegistrationInstanceId";

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

        #region Properties and Fields

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
            get => this.RegistrationInstance?.RegistrationTemplateId;
        }

        /// <summary>
        /// Is wait-listing enabled for the active Registration Instance?
        /// </summary>
        protected bool WaitListIsEnabled { get; private set; }

        /// <summary>
        /// Is group placement enabled for the active Registration Instance?
        /// </summary>
        protected bool GroupPlacementIsEnabled { get; private set; }

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

        private bool? _RegistrationInstanceHasCosts;

        /// <summary>
        /// Does the active Registration Instance have associated costs?
        /// </summary>
        protected bool RegistrationInstanceHasCosts
        {
            get
            {
                if ( !_RegistrationInstanceHasCosts.HasValue )
                {
                    var registrationInstance = this.RegistrationInstance;

                    if ( registrationInstance != null )
                    {
                        // Check if this Registration Instance has associated costs or fees.
                        _RegistrationInstanceHasCosts = ( registrationInstance.RegistrationTemplate != null
                            && ( ( registrationInstance.RegistrationTemplate.SetCostOnInstance.HasValue && registrationInstance.RegistrationTemplate.SetCostOnInstance == true && registrationInstance.Cost.HasValue && registrationInstance.Cost.Value > 0 )
                            || registrationInstance.RegistrationTemplate.Cost > 0
                            || registrationInstance.RegistrationTemplate.Fees.Count > 0 ) );

                    }
                }

                return _RegistrationInstanceHasCosts.GetValueOrDefault();
            }
        }

        private bool? _RegistrationInstanceHasPayments;

        /// <summary>
        /// Does the active Registration Instance have associated payments?
        /// </summary>
        protected bool RegistrationInstanceHasPayments
        {
            get
            {
                if ( !_RegistrationInstanceHasPayments.HasValue )
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

                        _RegistrationInstanceHasPayments = new FinancialTransactionDetailService( dataContext )
                            .Queryable().AsNoTracking()
                            .Where( d =>
                                d.EntityTypeId.HasValue &&
                                d.EntityId.HasValue &&
                                d.EntityTypeId.Value == registrationEntityType.Id &&
                                registrationIdQry.Contains( d.EntityId.Value ) )
                            .Any();

                    }
                }

                return _RegistrationInstanceHasCosts.GetValueOrDefault();
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

            RegistrationInstanceId = PageParameter( PageParameterRegistrationInstanceId ).AsIntegerOrNull();

            if ( RegistrationInstance == null
                 && RegistrationInstanceId.HasValue )
            {
                RegistrationInstance = GetSharedRegistrationInstance( this.RegistrationInstanceId.Value );

                if ( RegistrationInstance != null )
                {
                    RegistrationInstance?.LoadAttributes();
                }
            }

            this.BlockUpdated += Block_BlockUpdated;
        }

        /// <summary>
        /// Override this method to respond to the BlockUpdated event.
        /// </summary>
        protected virtual void OnBlockUpdated()
        {
            //
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            this.OnBlockUpdated();
        }

        /// <summary>
        /// Gets the breadcrumbs for the page on which this block resides.
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? registrationInstanceId = PageParameter( pageReference, PageParameterRegistrationInstanceId ).AsIntegerOrNull();

            if ( registrationInstanceId.HasValue )
            {
                var registrationInstance = GetSharedRegistrationInstance( registrationInstanceId.Value );

                if ( registrationInstance != null )
                {
                    breadCrumbs.Add( new BreadCrumb( registrationInstance.ToString(), pageReference ) );

                    return breadCrumbs;
                }
            }

            breadCrumbs.Add( new BreadCrumb( "New Registration Instance", pageReference ) );

            return breadCrumbs;
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
        protected void AddRegistrationTemplateFieldsToGrid( List<RegistrantFormField> RegistrantFields, PlaceHolder filterFieldsContainer, Grid grid, GridFilter gridFilter, bool setValues, bool disableAddressFieldExport )
        {
            filterFieldsContainer.Controls.Clear();

            ClearGrid( grid );

            string dataFieldExpression = string.Empty;

            if ( RegistrantFields != null )
            {
                foreach ( var field in RegistrantFields )
                {
                    if ( field.FieldSource == RegistrationFieldSource.PersonField
                         && field.PersonFieldType.HasValue )
                    {
                        switch ( field.PersonFieldType.Value )
                        {
                            case RegistrationPersonFieldType.Campus:
                                {
                                    var ddlCampus = new RockDropDownList();
                                    ddlCampus.ID = "ddlGroupPlacementsCampus";
                                    ddlCampus.Label = "Home Campus";
                                    ddlCampus.DataValueField = "Id";
                                    ddlCampus.DataTextField = "Name";
                                    ddlCampus.DataSource = CampusCache.All();
                                    ddlCampus.DataBind();
                                    ddlCampus.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );

                                    if ( setValues )
                                    {
                                        ddlCampus.SetValue( gridFilter.GetUserPreference(  "Home Campus" ) );
                                    }

                                    filterFieldsContainer.Controls.Add( ddlCampus );

                                    var templateField = new RockLiteralField();
                                    templateField.ID = "lGroupPlacementsCampus";
                                    templateField.HeaderText = "Campus";
                                    grid.Columns.Add( templateField );
                                }
                                break;

                            case RegistrationPersonFieldType.Email:
                                {
                                    var tbEmailFilter = new RockTextBox();
                                    tbEmailFilter.ID = "tbGroupPlacementsEmailFilter";
                                    tbEmailFilter.Label = "Email";

                                    if ( setValues )
                                    {
                                        tbEmailFilter.Text = gridFilter.GetUserPreference( "Email" );
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
                                    drpBirthdateFilter.ID = "drpGroupPlacementsBirthdateFilter";
                                    drpBirthdateFilter.Label = "Birthdate Range";

                                    if ( setValues )
                                    {
                                        drpBirthdateFilter.DelimitedValues = gridFilter.GetUserPreference(  "Birthdate Range" );
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
                                    tbMiddleNameFilter.ID = "tbGroupPlacementsMiddleNameFilter";
                                    tbMiddleNameFilter.Label = "MiddleName";

                                    if ( setValues )
                                    {
                                        tbMiddleNameFilter.Text = gridFilter.GetUserPreference( "MiddleName" );
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
                                    drpAnniversaryDateFilter.ID = "drpGroupPlacementsAnniversaryDateFilter";
                                    drpAnniversaryDateFilter.Label = "AnniversaryDate Range";

                                    if ( setValues )
                                    {
                                        drpAnniversaryDateFilter.DelimitedValues = gridFilter.GetUserPreference(  "AnniversaryDate Range" );
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
                                    gpGradeFilter.ID = "gpGroupPlacementsGradeFilter";
                                    gpGradeFilter.Label = "Grade";
                                    gpGradeFilter.UseAbbreviation = true;
                                    gpGradeFilter.UseGradeOffsetAsValue = true;
                                    gpGradeFilter.CssClass = "input-width-md";

                                    // Since 12th grade is the 0 Value, we need to handle the "no user preference" differently
                                    // by not calling SetValue otherwise it will select 12th grade.
                                    if ( setValues )
                                    {
                                        var groupPlacementsGradeUserPreference = gridFilter.GetUserPreference(  "Grade" ).AsIntegerOrNull();
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
                                    ddlGenderFilter.ID = "ddlGroupPlacementsGenderFilter";
                                    ddlGenderFilter.Label = "Gender";

                                    if ( setValues )
                                    {
                                        ddlGenderFilter.SetValue( gridFilter.GetUserPreference(  "Gender" ) );
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
                                    dvpMaritalStatusFilter.ID = "dvpGroupPlacementsMaritalStatusFilter";
                                    dvpMaritalStatusFilter.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid() ).Id;
                                    dvpMaritalStatusFilter.Label = "Marital Status";

                                    if ( setValues )
                                    {
                                        dvpMaritalStatusFilter.SetValue( gridFilter.GetUserPreference(  "Marital Status" ) );
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
                                    tbMobilePhoneFilter.ID = "tbGroupPlacementsMobilePhoneFilter";
                                    tbMobilePhoneFilter.Label = mobileLabel;

                                    if ( setValues )
                                    {
                                        tbMobilePhoneFilter.Text = gridFilter.GetUserPreference(  "Phone" );
                                    }

                                    filterFieldsContainer.Controls.Add( tbMobilePhoneFilter );

                                    var phoneNumbersField = new RockLiteralField();
                                    phoneNumbersField.ID = "lGroupPlacementsMobile";
                                    phoneNumbersField.HeaderText = mobileLabel;
                                    grid.Columns.Add( phoneNumbersField );
                                }
                                break;

                            case RegistrationPersonFieldType.HomePhone:
                                {
                                    // Per discussion this should not have "Phone" appended to the end if it's missing.
                                    var homePhoneLabel = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME ).Value;

                                    var tbHomePhoneFilter = new RockTextBox();
                                    tbHomePhoneFilter.ID = "tbGroupPlacementsHomePhoneFilter";
                                    tbHomePhoneFilter.Label = homePhoneLabel;

                                    if ( setValues )
                                    {
                                        tbHomePhoneFilter.Text = gridFilter.GetUserPreference(  "HomePhone" );
                                    }

                                    filterFieldsContainer.Controls.Add( tbHomePhoneFilter );

                                    var homePhoneNumbersField = new RockLiteralField();
                                    homePhoneNumbersField.ID = "lGroupPlacementsHomePhone";
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
                        var filterFieldControl = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, "filterGroupPlacements_" + attribute.Id.ToString(), false, Rock.Reporting.FilterMode.SimpleFilter );
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
                                string savedValue = gridFilter.GetUserPreference(  attribute.Key );
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
        /// Get the collection of fields that are included in the forms associated with the registration template.
        /// </summary>
        /// <returns></returns>
        protected List<RegistrantFormField> GetRegistrantFormFields()
        {
            var fields = new List<RegistrantFormField>();

            var registrationInstance = this.RegistrationInstance;

            if ( registrationInstance != null )
            {
                foreach ( var form in registrationInstance.RegistrationTemplate.Forms )
                {
                    var formFields = form.Fields
                        .Where( f => f.IsGridField )
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
                                        PersonFieldType = formField.PersonFieldType
                                    } );
                            }
                        }
                        else
                        {
                            fields.Add(
                                new RegistrantFormField
                                {
                                    FieldSource = formField.FieldSource,
                                    Attribute = AttributeCache.Get( formField.AttributeId.Value )
                                } );
                        }
                    }
                }
            }

            return fields;
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
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private RegistrationInstance GetSharedRegistrationInstance( int registrationInstanceId, RockContext rockContext = null )
        {
            string key = string.Format( "RegistrationInstance:{0}", registrationInstanceId );

            var registrationInstance = RockPage.GetSharedItem( key ) as RegistrationInstance;

            if ( registrationInstance == null )
            {
                registrationInstance = GetRegistrationInstance( registrationInstanceId );

                var registrationTemplate = registrationInstance?.RegistrationTemplate;

                if ( registrationTemplate != null )
                {
                    this.WaitListIsEnabled = registrationTemplate.WaitListEnabled;

                    this.GroupPlacementIsEnabled = registrationTemplate.AllowGroupPlacement;
                }

                RockPage.SaveSharedItem( key, registrationInstance );
            }

            return registrationInstance;
        }

        /// <summary>
        /// Load the registration instance data, but do not populate the display properties.
        /// Use this method to load data for postback processing.
        /// </summary>
        /// <param name="registrationInstanceId"></param>
        /// <param name="parentTemplateId"></param>
        protected RegistrationInstance GetRegistrationInstance( int? registrationInstanceId, int? parentTemplateId = null )
        {
            var rockContext = new RockContext();

            var registrationInstance = new RegistrationInstanceService( rockContext )
                .Queryable( "RegistrationTemplate,Account,RegistrationTemplate.Forms.Fields" )
                .AsNoTracking()
                .FirstOrDefault( i => i.Id == registrationInstanceId );

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
            public RegistrationFieldSource FieldSource { get; set; }

            /// <summary>
            /// Gets or sets the type of the person field.
            /// </summary>
            /// <value>
            /// The type of the person field.
            /// </value>
            public RegistrationPersonFieldType? PersonFieldType { get; set; }

            /// <summary>
            /// Gets or sets the attribute.
            /// </summary>
            /// <value>
            /// The attribute.
            /// </value>
            public AttributeCache Attribute { get; set; }
        }

        #endregion

    }
}
