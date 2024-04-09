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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Field.Types;
using Rock.Web.Cache;
using Rock.Model;
using System.Collections.Generic;
using Rock.Attribute;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A <see cref="T:System.Web.UI.WebControls.TextBox"/> control for editing an address
    /// </summary>
    [ToolboxData( "<{0}:AddressControl runat=server></{0}:AddressControl>" )]
    public class AddressControl : CompositeControl, IRockControl
    {
        #region IRockControl implementation

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the label." )
        ]
        public string Label
        {
            get
            {
                return ViewState["Label"] as string ?? string.Empty;
            }

            set
            {
                ViewState["Label"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the form group class.
        /// </summary>
        /// <value>
        /// The form group class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        Description( "The CSS class to add to the form-group div." )
        ]
        public string FormGroupCssClass
        {
            get { return ViewState["FormGroupCssClass"] as string ?? string.Empty; }
            set { ViewState["FormGroupCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The help block." )
        ]
        public string Help
        {
            get
            {
                return HelpBlock != null ? HelpBlock.Text : string.Empty;
            }

            set
            {
                if ( HelpBlock != null )
                {
                    HelpBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the warning text.
        /// </summary>
        /// <value>
        /// The warning text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The warning block." )
        ]
        public string Warning
        {
            get
            {
                return WarningBlock != null ? WarningBlock.Text : string.Empty;
            }

            set
            {
                if ( WarningBlock != null )
                {
                    WarningBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this the Address control has a State drop-down list.
        /// </summary>
        /// <value>
        ///   <c>true</c> if it uses a drop-down list; otherwise, <c>false</c>.
        /// </value>
        public bool HasStateList
        {
            get
            {
                return ViewState["HasStateList"] as bool? ?? false;
            }

            set
            {
                ViewState["HasStateList"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether an Address must be entered.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "false" ),
        Description( "Is the value required?" )
        ]
        public bool Required
        {
            get
            {
                return ViewState["Required"] as bool? ?? false;
            }

            set
            {
                ViewState["Required"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>
        /// The required error message.
        /// </value>
        public string RequiredErrorMessage
        {
            get
            {
                return CustomValidator != null ? CustomValidator.ErrorMessage : string.Empty;
            }

            set
            {
                if ( CustomValidator != null )
                {
                    CustomValidator.ErrorMessage = value;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsValid
        {
            get
            {
                if ( this.Required )
                {
                    // The control must contain a valid address.
                    return CustomValidator == null || CustomValidator.IsValid;
                }
                else
                {
                    // The control must contain a valid address or nothing.
                    return this.HasValue || ( CustomValidator == null || CustomValidator.IsValid );
                }
            }
        }

        /// <summary>
        /// Validates the current control content.
        /// </summary>
        /// <param name="messages">If the validation is unsuccessful, contains the list of validation messages.</param>
        /// <returns>
        /// <c>true</c> if the content is valid; otherwise, <c>false</c>.
        /// </returns>
        [RockInternal( "1.16.3" )]
        public bool Validate( out List<string> messages )
        {
            messages = new List<string>();

            if ( CustomValidator != null )
            {
                CustomValidator.Validate();
                if ( !CustomValidator.IsValid )
                {
                    messages.Add( CustomValidator.ErrorMessage );
                }
            }
            if ( RequiredFieldValidator != null && !RequiredFieldValidator.IsValid )
            {
                messages.Add( RequiredFieldValidator.ErrorMessage );
            }

            return !messages.Any();
        }

        /// <summary>
        /// Gets or sets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public HelpBlock HelpBlock { get; set; }

        /// <summary>
        /// Gets or sets the warning block.
        /// </summary>
        /// <value>
        /// The warning block.
        /// </value>
        public WarningBlock WarningBlock { get; set; }

        /// <summary>
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        #endregion

        #region Fields

        private string _orgState = null;
        private string _orgCountry = null;

        private DataEntryRequirementLevelSpecifier _AddressLine1Requirement = DataEntryRequirementLevelSpecifier.Optional;
        private DataEntryRequirementLevelSpecifier _AddressLine2Requirement = DataEntryRequirementLevelSpecifier.Optional;
        private DataEntryRequirementLevelSpecifier _CityRequirement = DataEntryRequirementLevelSpecifier.Optional;
        private DataEntryRequirementLevelSpecifier _LocalityRequirement = DataEntryRequirementLevelSpecifier.Optional;
        private DataEntryRequirementLevelSpecifier _StateRequirement = DataEntryRequirementLevelSpecifier.Optional;
        private DataEntryRequirementLevelSpecifier _PostalCodeRequirement = DataEntryRequirementLevelSpecifier.Optional;

        private string _CityLabel;
        private string _LocalityLabel;
        private string _StateLabel;
        private string _PostalCodeLabel;

        #endregion

        #region Controls

        private TextBox _tbStreet1;
        private TextBox _tbStreet2;
        private TextBox _tbCity;
        private TextBox _tbCounty;
        private TextBox _tbState;
        private DropDownList _ddlState;
        private TextBox _tbPostalCode;
        private RockDropDownList _ddlCountry;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the custom validator.
        /// </summary>
        /// <value>
        /// The custom validator.
        /// </value>
        public CustomValidator CustomValidator { get; set; }

        /// <summary>
        /// Gets or sets the value of the Address Line 1 field.
        /// </summary>
        /// <value>
        /// This field stores the most significant portion of the postal address: Street Address or PO Box.
        /// </value>
        public string Street1
        {
            get
            {
                EnsureChildControls();
                return _tbStreet1.Text;
            }

            set
            {
                EnsureChildControls();
                _tbStreet1.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the value of the Address Line 2 field.
        /// </summary>
        /// <value>
        /// This field stores additional detail about the postal address: Apartment/Building/Block information
        /// </value>
        public string Street2
        {
            get
            {
                EnsureChildControls();
                return _tbStreet2.Text;
            }

            set
            {
                EnsureChildControls();
                _tbStreet2.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>
        /// The city.
        /// </value>
        public string City
        {
            get
            {
                EnsureChildControls();
                return _tbCity.Text;
            }

            set
            {
                EnsureChildControls();
                _tbCity.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the locality of the postal address.
        /// Locality refers to a subdivision of the state or region, such as a district or county.
        /// </summary>
        /// <value>
        /// The county.
        /// </value>
        public string County
        {
            get
            {
                EnsureChildControls();
                return _tbCounty.Text;
            }

            set
            {
                EnsureChildControls();
                _tbCounty.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the state, province or region of the postal address.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public string State
        {
            get
            {
                EnsureChildControls();
                if ( !HasStateList )
                {
                    return _tbState.Text;
                }
                else
                {
                    return _ddlState.SelectedValue;
                }
            }

            set
            {
                EnsureChildControls();
                SetCountryAndState( this.Country, value );
            }
        }

        /// <summary>
        /// Gets or sets the postal code.
        /// </summary>
        /// <value>
        /// The postal code.
        /// </value>
        public string PostalCode
        {
            get
            {
                EnsureChildControls();
                return _tbPostalCode.Text;
            }

            set
            {
                EnsureChildControls();
                _tbPostalCode.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        /// <value>
        /// The country.
        /// </value>
        public string Country
        {
            get
            {
                EnsureChildControls();
                if ( _ddlCountry.SelectedValue.IsNotNullOrWhiteSpace() )
                {
                    return _ddlCountry.SelectedValue;
                }
                else
                {
                    return GetDefaultCountry();
                }
            }

            set
            {
                EnsureChildControls();

                SetCountryAndState( value );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Address Line 2 field should be shown.
        /// This setting has no effect if Address Line 2 is required for the selected Country.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the Address Line 2 field should be visible; otherwise, <c>false</c>.
        /// </value>
        public bool ShowAddressLine2
        {
            get
            {
                return ViewState["ShowAddressLine2"] as bool? ?? true;
            }

            set
            {
                ViewState["ShowAddressLine2"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Locality field should be shown.
        /// This setting has no effect if Locality is required for the selected Country.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show county]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowCounty
        {
            get
            {
                return ViewState["ShowCounty"] as bool? ?? false;
            }

            set
            {
                ViewState["ShowCounty"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Country field should be shown.
        /// </summary>
        public bool ShowCountry
        {
            get
            {
                var show = ViewState["ShowCountry"] as bool?;
                if ( show == null )
                {
                    show = GlobalAttributesCache.Get().GetValue( "SupportInternationalAddresses" ).AsBooleanOrNull() ?? false;
                }
                return show.Value;
            }

            set
            {
                ViewState["ShowCountry"] = value;
            }
        }

        /// <summary>
        /// Display an abbreviated state name
        /// </summary>
        public bool UseStateAbbreviation
        {
            get
            {
                return ViewState["UseStateAbbreviation"] as bool? ?? true;
            }

            set
            {
                var oldValue = this.UseStateAbbreviation;
                ViewState["UseStateAbbreviation"] = value;
                if ( oldValue != value )
                {
                    EnsureChildControls();
                    BindStates( _ddlCountry.SelectedValue );
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [use country abbreviation].
        /// </summary>
        /// <value>
        /// <c>true</c> if [use country abbreviation]; otherwise, <c>false</c>.
        /// </value>
        public bool UseCountryAbbreviation
        {
            get
            {
                return ViewState["UseCountryAbbreviation"] as bool? ?? false;
            }

            set
            {
                if ( ( ViewState["UseCountryAbbreviation"] as bool? ?? false ) != value )
                {
                    EnsureChildControls();
                    RebindCountries();
                }

                ViewState["UseCountryAbbreviation"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if address fields should be set to default values when the control is first shown.
        /// </summary>
        /// <value>
        /// <c>true</c> if default values should be set; otherwise, <c>false</c>.
        /// </value>
        public bool SetDefaultValues
        {
            get
            {
                return ViewState["SetDefaultValues"] as bool? ?? true;
            }

            set
            {
                ViewState["SetDefaultValues"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get
            {
                EnsureChildControls();
                return _tbStreet1.ValidationGroup;
            }

            set
            {
                EnsureChildControls();

                CustomValidator.ValidationGroup = value;

                if ( this.RequiredFieldValidator != null )
                {
                    this.RequiredFieldValidator.ValidationGroup = value;
                }

                _tbStreet1.ValidationGroup = value;
                _tbStreet2.ValidationGroup = value;
                _tbCity.ValidationGroup = value;
                _tbCounty.ValidationGroup = value;
                _tbState.ValidationGroup = value;
                _ddlState.ValidationGroup = value;
                _ddlCountry.ValidationGroup = value;
            }
        }

        /// <summary>
        /// Sets the validation message display mode for the control.
        /// </summary>
        public ValidatorDisplay ValidationDisplay
        {
            get
            {
                // All validators use the same setting, so return the value of an arbitrary validator.
                return CustomValidator.Display;
            }
            set
            {
                EnsureChildControls();

                CustomValidator.Display = value;

                if ( this.RequiredFieldValidator != null )
                {
                    this.RequiredFieldValidator.Display = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether validation is disabled for this control.
        /// </summary>
        /// <value>
        ///   <c>true</c> if validation is disabled; otherwise, <c>false</c>.
        /// </value>
        public bool ValidationIsDisabled
        {
            get
            {
                return ViewState["ValidationIsDisabled"] as bool? ?? false;
            }

            set
            {
                ViewState["ValidationIsDisabled"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if the entry must satisfy the address requirements for the selected country to be considered valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if partial and invalid addresses should be allowed; otherwise, <c>false</c>.
        /// </value>
        public bool PartialAddressIsAllowed
        {
            get
            {
                return ViewState["PartialAddressIsAllowed"] as bool? ?? false;
            }

            set
            {
                ViewState["PartialAddressIsAllowed"] = value;
                RecreateChildControls();
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressControl"/> class.
        /// </summary>
        public AddressControl()
            : base()
        {
            HelpBlock = new HelpBlock();
            WarningBlock = new WarningBlock();

            // Prevent ViewState from being disabled by the container, because it is necessary for this control to operate correctly.
            this.ViewStateMode = ViewStateMode.Enabled;

            // Default validation display mode to use the ValidationSummary control rather than inline.
            this.ValidationDisplay = ValidatorDisplay.None;

        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( System.EventArgs e )
        {
            EnsureChildControls();
            base.OnInit( e );

            BindCountries();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            var selectedCountry = string.Empty;
            var selectedState = string.Empty;

            // Read the existing control values unless we are setting default values for first load.
            if ( !this.SetDefaultValues )
            {
                selectedCountry = _ddlCountry.SelectedValue;
                if ( this.HasStateList )
                {
                    selectedState = _ddlState.SelectedValue;
                }
                else
                {
                    selectedState = _tbState.Text;
                }
            }

            if ( string.IsNullOrWhiteSpace( selectedCountry ) )
            {
                selectedCountry = GetDefaultCountry();
            }
            if ( string.IsNullOrWhiteSpace( selectedState ) )
            { 
                selectedState = GetDefaultState();
            }

            // Reset the default values flag to prevent selected values being overwritten on postback.
            this.SetDefaultValues = false;

            SetCountryAndState( selectedCountry, selectedState );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            this.Attributes["data-itemlabel"] = this.Label != string.Empty ? this.Label : "Address";
            this.Attributes["data-required"] = this.Required.ToTrueFalse().ToLower();

            _ddlCountry = new RockDropDownList();
            _ddlCountry.EnhanceForLongLists = true;
            Controls.Add( _ddlCountry );
            _ddlCountry.ID = "ddlCountry";
            _ddlCountry.DataValueField = "Id";
            _ddlCountry.AutoPostBack = true;
            _ddlCountry.SelectedIndexChanged += _ddlCountry_SelectedIndexChanged;
            _ddlCountry.CssClass = "form-control js-country";

            _tbStreet1 = new TextBox();
            Controls.Add( _tbStreet1 );
            _tbStreet1.ID = "tbStreet1";
            _tbStreet1.CssClass = "form-control js-address-field js-street1";

            _tbStreet2 = new TextBox();
            Controls.Add( _tbStreet2 );
            _tbStreet2.ID = "tbStreet2";
            _tbStreet2.CssClass = "form-control js-address-field js-street2";

            _tbCity = new TextBox();
            Controls.Add( _tbCity );
            _tbCity.ID = "tbCity";
            _tbCity.CssClass = "form-control js-address-field js-city";

            _tbCounty = new TextBox();
            Controls.Add( _tbCounty );
            _tbCounty.ID = "tbCounty";
            _tbCounty.CssClass = "form-control js-address-field js-county";

            _tbState = new TextBox();
            Controls.Add( _tbState );
            _tbState.ID = "tbState";
            _tbState.CssClass = "form-control js-address-field js-state";

            _ddlState = new DropDownList();
            Controls.Add( _ddlState );
            _ddlState.ID = "ddlState";
            _ddlState.DataValueField = "Id";
            _ddlState.CssClass = "form-control js-state";

            _tbPostalCode = new TextBox();
            Controls.Add( _tbPostalCode );
            _tbPostalCode.ID = "tbPostalCode";
            _tbPostalCode.CssClass = "form-control js-postal-code js-postcode js-address-field";

            // Add custom validator
            CustomValidator = new CustomValidator();
            CustomValidator.ID = this.ID + "_cfv";
            CustomValidator.ClientValidationFunction = "Rock.controls.addressControl.clientValidate";
            CustomValidator.CssClass = "validation-error";
            CustomValidator.ServerValidate += _CustomValidator_ServerValidate;

            Controls.Add( CustomValidator );
        }

        private void _CustomValidator_ServerValidate( object source, ServerValidateEventArgs args )
        {
            if ( this.ValidationIsDisabled )
            {
                return;
            }
            if ( !this.HasValue
                 && !this.Required )
            {
                return;
            }

            // Get the edited Location, and include any default values to avoid incorrect validation messages for the fields
            // to which they apply.
            if ( this.PartialAddressIsAllowed )
            {
                return;
            }

            var editedLocation = new Location();
            GetValues( editedLocation, includeDefaultValues:true );

            string validationMessage;
            var isValid = LocationService.ValidateLocationAddressRequirements( editedLocation, out validationMessage );

            if ( !isValid )
            {
                var addressRequirementsValidator = source as CustomValidator;
                if ( addressRequirementsValidator != null )
                {
                    addressRequirementsValidator.ErrorMessage = validationMessage;
                }

                args.IsValid = false;

                return;
            }
        }

        /// <inheritdoc />
        protected override void OnPreRender( EventArgs e )
        {
            // Disable required field validation if partial address input is enabled.
            CustomValidator.Enabled = !( this.ValidationIsDisabled || this.PartialAddressIsAllowed );

            base.OnPreRender( e );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                RockControlHelper.RenderControl( this, writer );
            }
        }

        /// <summary>
        /// Load the address field settings for the specified country.
        /// </summary>
        /// <param name="countryCode"></param>
        private void LoadCountryConfiguration( string countryCode )
        {
            var countryValue = DefinedTypeCache.Get( new Guid( SystemGuid.DefinedType.LOCATION_COUNTRIES ) )
                .DefinedValues
                .Where( v => v.Value.Equals( countryCode, StringComparison.OrdinalIgnoreCase ) )
                .FirstOrDefault();

            if ( countryValue == null )
            {
                _CityLabel = null;
                _LocalityLabel = null;
                _StateLabel = null;
                _PostalCodeLabel = null;
            }
            else
            {
                _CityLabel = countryValue.GetAttributeValue( SystemKey.CountryAttributeKey.AddressCityLabel );
                _LocalityLabel = countryValue.GetAttributeValue( SystemKey.CountryAttributeKey.AddressLocalityLabel );
                _StateLabel = countryValue.GetAttributeValue( SystemKey.CountryAttributeKey.AddressStateLabel );
                _PostalCodeLabel = countryValue.GetAttributeValue( SystemKey.CountryAttributeKey.AddressPostalCodeLabel );

                if ( !this.PartialAddressIsAllowed )
                {
                    // Set the field requirements for valid data entry.
                    var requirementField = new DataEntryRequirementLevelFieldType();

                    _AddressLine1Requirement = requirementField.GetDeserializedValue( countryValue.GetAttributeValue( SystemKey.CountryAttributeKey.AddressLine1Requirement ), DataEntryRequirementLevelSpecifier.Optional );
                    _AddressLine2Requirement = requirementField.GetDeserializedValue( countryValue.GetAttributeValue( SystemKey.CountryAttributeKey.AddressLine2Requirement ), DataEntryRequirementLevelSpecifier.Optional );
                    _CityRequirement = requirementField.GetDeserializedValue( countryValue.GetAttributeValue( SystemKey.CountryAttributeKey.AddressCityRequirement ), DataEntryRequirementLevelSpecifier.Optional );
                    _LocalityRequirement = requirementField.GetDeserializedValue( countryValue.GetAttributeValue( SystemKey.CountryAttributeKey.AddressLocalityRequirement ), DataEntryRequirementLevelSpecifier.Optional );
                    _StateRequirement = requirementField.GetDeserializedValue( countryValue.GetAttributeValue( SystemKey.CountryAttributeKey.AddressStateRequirement ), DataEntryRequirementLevelSpecifier.Optional );
                    _PostalCodeRequirement = requirementField.GetDeserializedValue( countryValue.GetAttributeValue( SystemKey.CountryAttributeKey.AddressPostalCodeRequirement ), DataEntryRequirementLevelSpecifier.Optional );
                }
            }

            _CityLabel = _CityLabel.ToStringOrDefault( "City" );
            _LocalityLabel = _LocalityLabel.ToStringOrDefault( "Locality" );
            _StateLabel = _StateLabel.ToStringOrDefault( "State" );
            _PostalCodeLabel = _PostalCodeLabel.ToStringOrDefault( "Postal Code" );

            if ( !this.PartialAddressIsAllowed )
            {
                // Hide Address Line 2 if it is not required, and not specified to show in the control settings.
                if ( _AddressLine2Requirement == DataEntryRequirementLevelSpecifier.Optional
                     && !this.ShowAddressLine2 )
                {
                    _AddressLine2Requirement = DataEntryRequirementLevelSpecifier.Unavailable;
                }

                // Hide Locality if specified in the control settings and it is not required.
                // The ShowCounty property is probably not necessary now because this setting can be specified per-country.
                if ( _LocalityRequirement == DataEntryRequirementLevelSpecifier.Optional
                     && !this.ShowCounty )
                {
                    _LocalityRequirement = DataEntryRequirementLevelSpecifier.Unavailable;
                }
            }
        }

        /// <summary>
        /// Set the Required status of the address controls according to internal settings.
        /// </summary>
        private void ApplyRequiredFieldConfiguration()
        {
            if ( this.Required )
            {
                _tbStreet1.CssClass += ( _AddressLine1Requirement == DataEntryRequirementLevelSpecifier.Required ? " required" : string.Empty );
                _tbStreet2.CssClass += ( _AddressLine2Requirement == DataEntryRequirementLevelSpecifier.Required ? " required" : string.Empty );
                _tbCity.CssClass += ( _CityRequirement == DataEntryRequirementLevelSpecifier.Required ? " required" : string.Empty );
                _tbCounty.CssClass += ( _LocalityRequirement == DataEntryRequirementLevelSpecifier.Required ? " required" : string.Empty );
                _tbState.CssClass += ( _StateRequirement == DataEntryRequirementLevelSpecifier.Required ? " required" : string.Empty );
                _tbPostalCode.CssClass += ( _PostalCodeRequirement == DataEntryRequirementLevelSpecifier.Required ? " required" : string.Empty );
            }
        }

        /// <summary>
        /// Set the configuration of the address controls according to internal settings.
        /// </summary>
        private void ApplyCountryConfigurationToControls()
        {
            bool addressLine2IsVisible = ( _AddressLine2Requirement != DataEntryRequirementLevelSpecifier.Unavailable );

            _tbStreet1.Attributes["field-name"] = addressLine2IsVisible ? "Address Line 1" : "Address";
            _tbStreet1.Attributes["placeholder"] = addressLine2IsVisible ? "Address Line 1" : "Address";
            _tbStreet1.Attributes["autocomplete"] = addressLine2IsVisible ? "address-line1" : "street-address";

            _tbStreet2.Attributes["field-name"] = "Address Line 2";
            _tbCity.Attributes["field-name"] = _CityLabel;
            _tbCounty.Attributes["field-name"] = _LocalityLabel;
            _tbState.Attributes["field-name"] = _StateLabel;
            _ddlState.Attributes["field-name"] = _StateLabel;
            _tbPostalCode.Attributes["field-name"] = _PostalCodeLabel;

            _tbCity.Attributes["placeholder"] = _CityLabel;
            _tbCounty.Attributes["placeholder"] = _LocalityLabel;
            _tbState.Attributes["placeholder"] = _StateLabel;
            _ddlState.Attributes["placeholder"] = _StateLabel;
            _tbPostalCode.Attributes["placeholder"] = _PostalCodeLabel;

            _ddlCountry.Visible = this.ShowCountry;
        }

        /// <summary>
        /// Renders the base control.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            if ( !this.Visible )
            {
                return;
            }

            this.ApplyRequiredFieldConfiguration();

            // Address Fields
            writer.AddAttribute( "class", "address-control js-addressControl " + this.CssClass );
            writer.AddAttribute( "data-required", this.Required.ToTrueFalse().ToLower() );
            writer.AddAttribute( "data-itemlabel", this.Label != string.Empty ? this.Label : "Address" );
            writer.AddAttribute( "id", this.ClientID );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // Country
            if ( this.ShowCountry )
            {
                writer.AddAttribute( "class", "form-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( "class", "form-row" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( "class", "col-sm-6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _ddlCountry.RenderControl( writer );
                writer.RenderEndTag();

                writer.AddAttribute( "class", "col-sm-6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.RenderEndTag();

                writer.RenderEndTag();  // div.form-row

                writer.RenderEndTag();  // div.form-group
            }

            // Address Line 1
            if ( _AddressLine1Requirement != DataEntryRequirementLevelSpecifier.Unavailable )
            {
                writer.AddAttribute( "class", "form-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _tbStreet1.RenderControl( writer );
                writer.RenderEndTag();  // div.form-group
            }

            // Address Line 2
            if ( _AddressLine2Requirement != DataEntryRequirementLevelSpecifier.Unavailable )
            {
                writer.AddAttribute( "class", "form-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _tbStreet2.Attributes["placeholder"] = "Address Line 2";
                _tbStreet2.Attributes["autocomplete"] = "address-line2";
                _tbStreet2.RenderControl( writer );
                writer.RenderEndTag();  // div.form-group
            }

            bool localityIsVisible = ( _LocalityRequirement != DataEntryRequirementLevelSpecifier.Unavailable );

            writer.AddAttribute( "class", "form-row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // City or Town
            if ( _CityRequirement != DataEntryRequirementLevelSpecifier.Unavailable )
            {
                writer.AddAttribute( "class", "form-group" + ( localityIsVisible ? " col-sm-3" : " col-sm-6" ) );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _tbCity.Attributes["autocomplete"] = ( localityIsVisible ? "address-level3" : "address-level2" );
                _tbCity.RenderControl( writer );
                writer.RenderEndTag();  // div.form-group
            }

            // Locality or County
            if ( _LocalityRequirement != DataEntryRequirementLevelSpecifier.Unavailable )
            {
                writer.AddAttribute( "class", "form-group col-sm-3" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _tbCounty.Attributes["autocomplete"] = "address-level2";
                _tbCounty.RenderControl( writer );
                writer.RenderEndTag();  // div.form-group
            }

            // State or Region
            if ( _StateRequirement != DataEntryRequirementLevelSpecifier.Unavailable )
            {
                writer.AddAttribute( "class", "form-group col-sm-3" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _tbState.Attributes["autocomplete"] = "address-level1";
                _tbState.RenderControl( writer );
                _ddlState.RenderControl( writer );
                writer.RenderEndTag();  // div.form-group
            }

            // Postal Code
            if ( _PostalCodeRequirement != DataEntryRequirementLevelSpecifier.Unavailable )
            {
                writer.AddAttribute( "class", "form-group col-sm-3" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _tbPostalCode.Attributes["autocomplete"] = "postal-code";
                _tbPostalCode.RenderControl( writer );
                writer.RenderEndTag();  // div.form-group
            }

            writer.RenderEndTag();  // div.form-row

            CustomValidator.RenderControl( writer );

            writer.RenderEndTag();      // div
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the _ddlCountry control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void _ddlCountry_SelectedIndexChanged( object sender, EventArgs e )
        {
            EnsureChildControls();

            if ( _ddlCountry.SelectedValue == "------------------------" )
            {
                _ddlCountry.SelectedIndex = 0;
            }

            SetCountryAndState( _ddlCountry.SelectedValue );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the values from the location to the Address Control. Use SetValues(null) to set defaults.
        /// </summary>
        /// <param name="location">The location.</param>
        public void SetValues( Rock.Model.Location location )
        {
            if ( location != null )
            {
                Country = location.Country;
                Street1 = location.Street1;
                Street2 = location.Street2;
                City = location.City;
                County = location.County;
                State = location.State;
                PostalCode = location.PostalCode;
            }
            else
            {
                Country = GetDefaultCountry();
                Street1 = string.Empty;
                Street2 = string.Empty;
                City = string.Empty;
                County = string.Empty;
                State = GetDefaultState();
                PostalCode = string.Empty;
            }
        }

        /// <summary>
        /// Returns a flag indicating if the control contains any non-default values.
        /// </summary>
        /// <returns>True if any field contains a non-default value.</returns>
        public bool HasValue
        {
            get
            {
                // Get field values, nullifying any fields that are not available for the selected country.
                var street1 = GetLocationFieldValue( this.Street1, _AddressLine1Requirement );
                var street2 = GetLocationFieldValue( this.Street2, _AddressLine2Requirement );
                var city = GetLocationFieldValue( this.City, _CityRequirement );
                var county = GetLocationFieldValue( this.County, _LocalityRequirement );
                var state = GetLocationFieldValue( this.State, _StateRequirement );
                var postalCode = GetLocationFieldValue( this.PostalCode, _PostalCodeRequirement );
                var country = this.Country;

                var isEmpty = string.IsNullOrWhiteSpace( street1 )
                         && string.IsNullOrWhiteSpace( street2 )
                         && string.IsNullOrWhiteSpace( city )
                         && string.IsNullOrWhiteSpace( county )
                         && string.IsNullOrWhiteSpace( postalCode )
                         && ( string.IsNullOrWhiteSpace( state ) || state == this.GetDefaultState() )
                         && ( string.IsNullOrWhiteSpace( country ) || country == this.GetDefaultCountry() );

                return !isEmpty;
            }
        }

        /// <summary>
        /// Gets the values from the address control and update the fields of the specified location.
        /// </summary>
        /// <param name="location">The location.</param>
        public void GetValues( Rock.Model.Location location )
        {
            GetValues( location, includeDefaultValues: false );
        }

        /// <summary>
        /// Gets the values from the address control and updates the fields of the specified Location entity.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="includeDefaultValues">If true, </param>
        public void GetValues( Rock.Model.Location location, bool includeDefaultValues )
        {
            if ( location == null )
            {
                return;
            }

            if ( this.HasValue || includeDefaultValues )
            {
                // Get field values, nullifying any fields that are not available for the selected country.
                var street1 = GetLocationFieldValue( this.Street1, _AddressLine1Requirement );
                var street2 = GetLocationFieldValue( this.Street2, _AddressLine2Requirement );
                var city = GetLocationFieldValue( this.City, _CityRequirement );
                var county = GetLocationFieldValue( this.County, _LocalityRequirement );
                var state = GetLocationFieldValue( this.State, _StateRequirement );
                var postalCode = GetLocationFieldValue( this.PostalCode, _PostalCodeRequirement );

                // If country selection is available, get the selected value otherwise use the system default.
                if ( this.ShowCountry )
                {
                    location.Country = _ddlCountry.SelectedValue;
                }
                else
                {
                    location.Country = GetDefaultCountry();
                }

                location.Street1 = street1;
                location.Street2 = street2;
                location.City = city;
                location.County = county;
                location.State = state;
                location.PostalCode = postalCode;
            }
            else
            {
                // No non-default values have been entered, so return an empty location.
                location.Country = null;
                location.Street1 = null;
                location.Street2 = null;
                location.City = null;
                location.County = null;
                location.State = null;
                location.PostalCode = null;
            }
        }

        /// <summary>
        /// Return a value from an address field that is adjusted to meet the associated requirements.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="requirementLevel"></param>
        /// <returns></returns>
        private string GetLocationFieldValue( string value, DataEntryRequirementLevelSpecifier requirementLevel )
        {
            if ( requirementLevel == DataEntryRequirementLevelSpecifier.Unavailable )
            {
                // If the field is unavailable, mask the value.
                return null;
            }

            return value;
        }

        /// <summary>
        /// Sets the organization address defaults.
        /// </summary>
        private void SetOrganizationAddressDefaults()
        {
            if ( _orgCountry != null )
            {
                return;
            }

            var globalAttributesCache = GlobalAttributesCache.Get();
            _orgState = globalAttributesCache.OrganizationState;
            _orgCountry = globalAttributesCache.OrganizationCountry;
        }

        /// <summary>
        /// Binds the countries data source to the selection control.
        /// </summary>
        private void BindCountries()
        {
            var definedType = DefinedTypeCache.Get( SystemGuid.DefinedType.LOCATION_COUNTRIES.AsGuid() );
            var countryValues = DefinedTypeCache.Get( SystemGuid.DefinedType.LOCATION_COUNTRIES.AsGuid() )
                .DefinedValues
                .OrderBy( v => v.Order )
                .ThenBy( v => v.Value )
                .ToList();

            if ( this.PartialAddressIsAllowed )
            {
                // Country is optional, so add a placeholder entry.
                _ddlCountry.Items.Add( new ListItem( "Country", string.Empty ) );
            }

            // Move default country to the top of the list
            var defaultCountryCode = GetDefaultCountry();
            if ( !string.IsNullOrWhiteSpace( defaultCountryCode ) )
            {
                var defaultCountry = countryValues
                    .Where( v => v.Value.Equals( defaultCountryCode, StringComparison.OrdinalIgnoreCase ) )
                    .FirstOrDefault();
                if ( defaultCountry != null )
                {
                    if ( !this.PartialAddressIsAllowed )
                    {
                        _ddlCountry.Items.Add( new ListItem( "Countries", string.Empty ) );
                    }
                    _ddlCountry.Items.Add( new ListItem( UseCountryAbbreviation ? defaultCountry.Value : defaultCountry.Description, defaultCountry.Value ) );
                    _ddlCountry.Items.Add( new ListItem( "------------------------", "------------------------" ) );
                }
            }

            foreach ( var country in countryValues )
            {
                _ddlCountry.Items.Add( new ListItem( UseCountryAbbreviation ? country.Value : country.Description, country.Value ) );
            }
        }

        /// <summary>
        /// Binds the countries data source to the selection control and sets the current value.
        /// </summary>
        private void RebindCountries()
        {
            string currentValue = _ddlCountry.SelectedValue;

            _ddlCountry.Items.Clear();
            _ddlCountry.SelectedIndex = -1;
            _ddlCountry.SelectedValue = null;
            _ddlCountry.ClearSelection();

            BindCountries();

            if ( string.IsNullOrEmpty( currentValue )
                 && this.SetDefaultValues )
            {
                currentValue = this.GetDefaultCountry();
            }

            SetCountryAndState( currentValue );
        }

        /// <summary>
        /// Binds the States data source to the selection control.
        /// </summary>
        /// <param name="country">The currently selected country.</param>
        private void BindStates( string country )
        {
            var showStateList = false;
            var countryGuid = DefinedTypeCache.Get( new Guid( SystemGuid.DefinedType.LOCATION_COUNTRIES ) )
                .DefinedValues
                .Where( v => v.Value.Equals( country, StringComparison.OrdinalIgnoreCase ) )
                .Select( v => v.Guid )
                .FirstOrDefault()
                .ToString();

            List<StateListSelectionItem> stateList = null;

            if ( countryGuid.IsNotNullOrWhiteSpace() )
            {
                var definedType = DefinedTypeCache.Get( new Guid( SystemGuid.DefinedType.LOCATION_ADDRESS_STATE ) );

                stateList = definedType
                    .DefinedValues
                    .Where( v =>
                        (
                            v.AttributeValues.ContainsKey( "Country" ) &&
                            v.AttributeValues["Country"] != null &&
                            v.AttributeValues["Country"].Value.Equals( countryGuid, StringComparison.OrdinalIgnoreCase )
                        ) ||
                        (
                            ( !v.AttributeValues.ContainsKey( "Country" ) || v.AttributeValues["Country"] == null ) &&
                            v.Attributes.ContainsKey( "Country" ) &&
                            v.Attributes["Country"].DefaultValue.Equals( countryGuid, StringComparison.OrdinalIgnoreCase )
                        ) )
                    .OrderBy( v => v.Order )
                    .ThenBy( v => v.Value )
                    .Select( v => new StateListSelectionItem { Id = v.Value, Value = v.Description } )
                    .ToList();

                showStateList = stateList.Any();

                if ( showStateList && this.PartialAddressIsAllowed )
                {
                    // If partial addresses are allowed, add an empty entry to the state list.
                    stateList.Insert( 0, new StateListSelectionItem { Id = string.Empty, Value = string.Empty } );
                }
            }

            this.HasStateList = showStateList;
            if ( showStateList )
            {

                _ddlState.Visible = true;
                _tbState.Visible = false;

                _ddlState.Items.Clear();
                _ddlState.SelectedIndex = -1;
                _ddlState.SelectedValue = null;
                _ddlState.ClearSelection();

                _ddlState.DataTextField = this.UseStateAbbreviation ? "Id" : "Value";
                _ddlState.DataSource = stateList;
                _ddlState.DataBind();
            }
            else
            {
                _ddlState.Visible = false;
                _tbState.Visible = true;
            }
        }

        /// <summary>
        /// Gets the default state.
        /// </summary>
        /// <returns></returns>
        public string GetDefaultState()
        {
            SetOrganizationAddressDefaults();

            // If partial addresses are allowed, the default value is empty.
            if ( this.PartialAddressIsAllowed )
            {
                return string.Empty;
            }

            // Return the default state for the Organization if no Country is selected
            // or the selected Country matches the Organization Address.
            if ( string.IsNullOrWhiteSpace( this.Country )
                 || this.Country == _orgCountry )
            {
                return _orgState;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the default country.
        /// </summary>
        /// <returns></returns>
        public string GetDefaultCountry()
        {
            SetOrganizationAddressDefaults();

            // If partial addresses are allowed, the default value is empty.
            if ( this.PartialAddressIsAllowed )
            {
                return string.Empty;
            }

            return string.IsNullOrWhiteSpace( _orgCountry ) ? "US" : _orgCountry;
        }

        /// <summary>
        /// Set the selected Country and synchronise the State field selection.
        /// </summary>
        private void SetCountryAndState( string selectedCountry = null, string selectedState = null )
        {
            // After this method has been called, do not overwrite the specified values with default settings.
            this.SetDefaultValues = false;

            if ( selectedCountry == null )
            {
                selectedCountry = _ddlCountry.SelectedValue;
            }

            if ( selectedState == null )
            {
                selectedState = this.HasStateList ? _ddlState.SelectedValue : _tbState.Text;
            }

            LoadCountryConfiguration( selectedCountry );

            ApplyCountryConfigurationToControls();

            BindStates( selectedCountry );

            // Set the Country
            if ( _ddlCountry.SelectedValue != selectedCountry )
            {
                _ddlCountry.SetValue( selectedCountry );
            }

            // Set the State to align with the selected Country, unless partial addresses are allowed.
            if ( !this.PartialAddressIsAllowed )
            {
                if ( string.IsNullOrEmpty( selectedCountry ) )
                {
                    // If no country is selected, reset the State field.
                    selectedState = null;
                }
                else if ( string.IsNullOrEmpty( selectedState ) )
                {
                    selectedState = GetDefaultState();
                }
            }

            if ( this.HasStateList )
            {
                _ddlState.SetValue( selectedState );
            }
            else
            {
                _tbState.Text = selectedState;
            }
        }

        private class StateListSelectionItem
        {
            public string Id { get; set; }
            public string Value { get; set; }
        }

        #endregion
    }
}