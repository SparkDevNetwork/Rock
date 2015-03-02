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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Web.Cache;

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
        /// Gets or sets a value indicating whether this <see cref="RockTextBox"/> is required.
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
                return RequiredFieldValidator != null ? RequiredFieldValidator.ErrorMessage : string.Empty;
            }

            set
            {
                if ( RequiredFieldValidator != null )
                {
                    RequiredFieldValidator.ErrorMessage = value;
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
                return !Required || RequiredFieldValidator == null || RequiredFieldValidator.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public HelpBlock HelpBlock { get; set; }

        /// <summary>
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        #endregion

        #region Fields

        private string _orgState = string.Empty;
        private string _orgCountry = string.Empty;

        #endregion

        #region Controls

        private TextBox _tbStreet1;
        private TextBox _tbStreet2;
        private TextBox _tbCity;
        private TextBox _tbState;
        private DropDownList _ddlState;
        private TextBox _tbPostalCode;
        private DropDownList _ddlCountry;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the street1.
        /// </summary>
        /// <value>
        /// The street1.
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
        /// Gets or sets the street2.
        /// </summary>
        /// <value>
        /// The street2.
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
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public string State
        {
            get
            {
                EnsureChildControls();
                if ( _tbState.Visible )
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

                string defaultState = GetDefaultState();
                string state = value ?? defaultState;
                _tbState.Text = state;
                _ddlState.SetValue( value, defaultState );
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
                return _ddlCountry.SelectedValue;
            }

            set
            {
                EnsureChildControls();

                string country = value ?? GetDefaultCountry();
                _ddlCountry.SetValue( country );
                BindStates( country );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show address line2].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show address line2]; otherwise, <c>false</c>.
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
                if ( ( ViewState["UseStateAbbreviation"] as bool? ?? false ) != value )
                {
                    EnsureChildControls();
                    BindStates( _ddlCountry.SelectedValue );
                }

                ViewState["UseStateAbbreviation"] = value;
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
                    BindCountries();
                }

                ViewState["UseCountryAbbreviation"] = value;
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
                _tbStreet1.ValidationGroup = value;
                _tbStreet2.ValidationGroup = value;
                _tbCity.ValidationGroup = value;
                _tbState.ValidationGroup = value;
                _ddlState.ValidationGroup = value;
                _ddlCountry.ValidationGroup = value;
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
            RockControlHelper.Init( this );
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            _tbStreet1 = new TextBox();
            Controls.Add( _tbStreet1 );
            _tbStreet1.ID = "tbStreet1";
            _tbStreet1.CssClass = "form-control";

            this.RequiredFieldValidator.ControlToValidate = _tbStreet1.ID;

            _tbStreet2 = new TextBox();
            Controls.Add( _tbStreet2 );
            _tbStreet2.ID = "tbStreet2";
            _tbStreet2.CssClass = "form-control";

            _tbCity = new TextBox();
            Controls.Add( _tbCity );
            _tbCity.ID = "tbCity";
            _tbCity.CssClass = "form-control";

            _tbState = new TextBox();
            Controls.Add( _tbState );
            _tbState.ID = "tbState";
            _tbState.CssClass = "form-control";

            _ddlState = new DropDownList();
            Controls.Add( _ddlState );
            _ddlState.ID = "ddlState";
            _ddlState.DataValueField = "Id";
            _ddlState.CssClass = "form-control";

            _tbPostalCode = new TextBox();
            Controls.Add( _tbPostalCode );
            _tbPostalCode.ID = "tbPostalCode";
            _tbPostalCode.CssClass = "form-control";

            _ddlCountry = new DropDownList();
            Controls.Add( _ddlCountry );
            _ddlCountry.ID = "ddlCountry";
            _ddlCountry.DataValueField = "Id";
            _ddlCountry.AutoPostBack = true;
            _ddlCountry.SelectedIndexChanged += _ddlCountry_SelectedIndexChanged;
            _ddlCountry.CssClass = "form-control";

            string defaultCountry = GetDefaultCountry();
            string defaultState = GetDefaultState();

            BindCountries();
            _ddlCountry.SetValue( defaultCountry );

            BindStates( defaultCountry );
            if ( _ddlState.Visible )
            {
                _ddlState.SetValue( defaultState );
            }

            _tbState.Text = defaultState;
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
        /// Renders the base control.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                bool showAddressLine2 = ShowAddressLine2;
                string cityLabel = "City";
                string stateLabel = "Region";
                string postalCodeLabel = "Postal Code";

                var countryValue = DefinedTypeCache.Read( new Guid( SystemGuid.DefinedType.LOCATION_COUNTRIES ) )
                    .DefinedValues
                    .Where( v => v.Value.Equals( _ddlCountry.SelectedValue, StringComparison.OrdinalIgnoreCase ) )
                    .FirstOrDefault();
                if ( countryValue != null )
                {
                    cityLabel = countryValue.GetAttributeValue( "CityLabel" );
                    stateLabel = countryValue.GetAttributeValue( "StateLabel" );
                    postalCodeLabel = countryValue.GetAttributeValue( "PostalCodeLabel" );
                }

                writer.AddAttribute( "id", this.ClientID );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( "class", "form-group " + ( this.Required ? "required" : string.Empty ) );

                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "control-label" );
                writer.AddAttribute( HtmlTextWriterAttribute.For, _tbStreet1.ClientID );
                writer.RenderBeginTag( HtmlTextWriterTag.Label );
                writer.Write( showAddressLine2 ? "Address Line 1" : "Address" );
                writer.RenderEndTag();  // label
                _tbStreet1.RenderControl( writer );
                writer.RenderEndTag();  // div.form-group

                if ( showAddressLine2 )
                {
                    writer.AddAttribute( "class", "form-group" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "control-label" );
                    writer.AddAttribute( HtmlTextWriterAttribute.For, _tbStreet2.ClientID );
                    writer.RenderBeginTag( HtmlTextWriterTag.Label );
                    writer.Write( "Address Line 2" );
                    writer.RenderEndTag();  // label
                    _tbStreet2.RenderControl( writer );
                    writer.RenderEndTag();  // div.form-group
                }

                writer.AddAttribute( "class", "row" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( "class", "form-group col-sm-6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "control-label" );
                writer.AddAttribute( HtmlTextWriterAttribute.For, _tbCity.ClientID );
                writer.RenderBeginTag( HtmlTextWriterTag.Label );
                writer.Write( cityLabel );
                writer.RenderEndTag();  // label
                _tbCity.RenderControl( writer );
                writer.RenderEndTag();  // div.form-group

                writer.AddAttribute( "class", "form-group col-sm-3" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "control-label" );
                writer.AddAttribute( HtmlTextWriterAttribute.For, _tbState.ClientID );
                writer.RenderBeginTag( HtmlTextWriterTag.Label );
                writer.Write( stateLabel );
                writer.RenderEndTag();  // label
                _tbState.RenderControl( writer );
                _ddlState.RenderControl( writer );
                writer.RenderEndTag();  // div.form-group

                writer.AddAttribute( "class", "form-group col-sm-3" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "control-label" );
                writer.AddAttribute( HtmlTextWriterAttribute.For, _tbPostalCode.ClientID );
                writer.RenderBeginTag( HtmlTextWriterTag.Label );
                writer.Write( postalCodeLabel );
                writer.RenderEndTag();  // label
                _tbPostalCode.RenderControl( writer );
                writer.RenderEndTag();  // div.form-group

                writer.RenderEndTag();  // row

                if ( _ddlCountry.Visible )
                {
                    writer.AddAttribute( "class", "row" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );

                    writer.AddAttribute( "class", "form-group col-sm-6" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "control-label" );
                    writer.AddAttribute( HtmlTextWriterAttribute.For, _tbStreet1.ClientID );
                    writer.RenderBeginTag( HtmlTextWriterTag.Label );
                    writer.Write( "Country" );
                    writer.RenderEndTag();  // label
                    _ddlCountry.RenderControl( writer );
                    writer.RenderEndTag();  // div.form-group

                    writer.AddAttribute( "class", "form-group col-sm-6" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    writer.RenderEndTag();  // div.form-group

                    writer.RenderEndTag();  // div.row
                }

                writer.RenderEndTag();      // div
            }
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

            if ( string.IsNullOrWhiteSpace( _ddlCountry.SelectedValue ) )
            {
                _ddlCountry.SelectedIndex = 0;
            }

            string selectedStateFromEdit = _tbState.Text;
            string selectedStateFromDownDrop = _ddlState.SelectedValue;

            BindStates( _ddlCountry.SelectedValue );

            if ( _tbState.Visible )
            {
                State = selectedStateFromEdit;
            }
            else
            {
                State = selectedStateFromDownDrop;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the values.
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
                State = location.State;
                PostalCode = location.PostalCode;
            }
            else
            {
                Country = GetDefaultCountry();
                Street1 = string.Empty;
                Street2 = string.Empty;
                City = string.Empty;
                State = GetDefaultState();
                PostalCode = string.Empty;
            }
        }

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <param name="location">The location.</param>
        public void GetValues( Rock.Model.Location location )
        {
            if ( location != null )
            {
                if ( !string.IsNullOrWhiteSpace( this.Street1 ) ||
                    !string.IsNullOrWhiteSpace( this.Street2 ) ||
                    !string.IsNullOrWhiteSpace( this.City ) )
                {
                    location.Country = Country;
                    location.Street1 = Street1;
                    location.Street2 = Street2;
                    location.City = City;
                    location.State = State;
                    location.PostalCode = PostalCode;
                }
                else
                {
                    location.Country = null;
                    location.Street1 = null;
                    location.Street2 = null;
                    location.City = null;
                    location.State = null;
                    location.PostalCode = null;
                }
            }
        }

        /// <summary>
        /// Sets the organization address defaults.
        /// </summary>
        private void SetOrganizationAddressDefaults()
        {
            var globalAttributesCache = GlobalAttributesCache.Read();
            _orgState = globalAttributesCache.OrganizationState;
            _orgCountry = globalAttributesCache.OrganizationCountry;
        }

        /// <summary>
        /// Binds the countries.
        /// </summary>
        private void BindCountries()
        {
            string currentValue = _ddlCountry.SelectedValue;

            _ddlCountry.Items.Clear();
            _ddlCountry.SelectedIndex = -1;
            _ddlCountry.SelectedValue = null;
            _ddlCountry.ClearSelection();

            var definedType = DefinedTypeCache.Read( new Guid( SystemGuid.DefinedType.LOCATION_COUNTRIES ) );
            var countryValues = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.LOCATION_COUNTRIES.AsGuid() )
                .DefinedValues
                .OrderBy( v => v.Order )
                .ThenBy( v => v.Value )
                .ToList();

            // Move default country to the top of the list
            string defaultCountryCode = GetDefaultCountry();
            if ( !string.IsNullOrWhiteSpace( defaultCountryCode ) )
            {
                var defaultCountry = countryValues
                    .Where( v => v.Value.Equals( defaultCountryCode, StringComparison.OrdinalIgnoreCase ) )
                    .FirstOrDefault();
                if ( defaultCountry != null )
                {
                    _ddlCountry.Items.Add( new ListItem( UseCountryAbbreviation ? defaultCountry.Value : defaultCountry.Description, defaultCountry.Value ) );
                    _ddlCountry.Items.Add( new ListItem( "------------------------", string.Empty ) );
                }
            }

            foreach ( var country in countryValues )
            {
                _ddlCountry.Items.Add( new ListItem( UseCountryAbbreviation ? country.Value : country.Description, country.Value ) );
            }

            bool? showCountry = GlobalAttributesCache.Read().GetValue( "SupportInternationalAddresses" ).AsBooleanOrNull();
            _ddlCountry.Visible = showCountry.HasValue && showCountry.Value;

            if ( !string.IsNullOrWhiteSpace( currentValue ) )
            {
                _ddlCountry.SetValue( currentValue );
            }
        }

        /// <summary>
        /// Binds the states.
        /// </summary>
        /// <param name="country">The country.</param>
        private void BindStates( string country )
        {
            string countryGuid = DefinedTypeCache.Read( new Guid( SystemGuid.DefinedType.LOCATION_COUNTRIES ) )
                .DefinedValues
                .Where( v => v.Value.Equals( country, StringComparison.OrdinalIgnoreCase ) )
                .Select( v => v.Guid )
                .FirstOrDefault()
                .ToString();

            var definedType = DefinedTypeCache.Read( new Guid( SystemGuid.DefinedType.LOCATION_ADDRESS_STATE ) );
            var stateList = definedType
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
                .Select( v => new { Id = v.Value, Value = v.Description } )
                .ToList();

            if ( stateList.Any() )
            {
                _ddlState.Visible = true;
                _tbState.Visible = false;

                string currentValue = _ddlState.SelectedValue;

                _ddlState.Items.Clear();
                _ddlState.SelectedIndex = -1;
                _ddlState.SelectedValue = null;
                _ddlState.ClearSelection();

                _ddlState.DataTextField = UseStateAbbreviation ? "Id" : "Value";
                _ddlState.DataSource = stateList;
                _ddlState.DataBind();

                if ( !string.IsNullOrWhiteSpace( currentValue ) )
                {
                    _ddlState.SetValue( currentValue, GetDefaultState() );
                }
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
            return _orgState;
        }

        /// <summary>
        /// Gets the default country.
        /// </summary>
        /// <returns></returns>
        public string GetDefaultCountry()
        {
            SetOrganizationAddressDefaults();
            return string.IsNullOrWhiteSpace( _orgCountry ) ? "US" : _orgCountry;
        }

        #endregion
    }
}