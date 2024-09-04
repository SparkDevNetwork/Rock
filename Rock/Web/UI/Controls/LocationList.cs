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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control used to show a searchable list of locations or add a new one.
    /// </summary>
    /// <seealso cref="System.Web.UI.WebControls.CompositeControl" />
    /// <seealso cref="Rock.Web.UI.Controls.IRockControl" />
    public class LocationList : CompositeControl, IRockControl
    {
        #region Private Controls
        private RockDropDownList _ddlLocations;
        private LinkButton _btnShowAddAddressForm;

        private Panel _pnlAddAddress;
        private AddressControl _acNewAddress;
        private AttributeValuesContainer _avcAddressAttributeValues;
        private BootstrapButton _btnAdd;
        private LinkButton _btnCancel;
        private RockTextBox _txtName;

        #endregion

        #region IRockControl Properties
        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label text
        /// </value>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        public string Help { get; set; }

        /// <summary>
        /// Gets or sets the warning text.
        /// </summary>
        /// <value>
        /// The warning text.
        /// </value>
        public string Warning { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IRockControl" /> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        public bool Required { get; set; }

        /// <summary>
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>
        /// The required error message.
        /// </value>
        public string RequiredErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the form group class.
        /// </summary>
        /// <value>
        /// The form group class.
        /// </value>
        public string FormGroupCssClass { get; set; }

        /// <summary>
        /// Gets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public HelpBlock HelpBlock { get; set; }

        /// <summary>
        /// Gets the warning block.
        /// </summary>
        /// <value>
        /// The warning block.
        /// </value>
        public WarningBlock WarningBlock { get; set; }

        /// <summary>
        /// Gets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup { get; set; }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        /// <exception cref="NotImplementedException"></exception>
        public bool IsValid => !Required || RequiredFieldValidator == null || RequiredFieldValidator.IsValid;
        #endregion

        #region LocationList Properties
        /// <summary>
        /// Gets or sets the location type value identifier.
        /// </summary>
        /// <value>
        /// The location type value identifier.
        /// </value>
        public int? LocationTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the location type value identifier.
        /// </summary>
        /// <value>
        /// The location type value identifier.
        /// </value>
        public int? ParentLocationId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show city state].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show city state]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowCityState { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow add].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow add]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowAdd { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is address required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is address required; otherwise, <c>false</c>.
        /// </value>
        public bool IsAddressRequired { get; set; }

        /// <summary>
        /// Gets or sets the selected value.
        /// </summary>
        /// <value>
        /// The selected value.
        /// </value>
        public string SelectedValue
        {
            get
            {
                EnsureChildControls();
                return _ddlLocations.SelectedValue;
            }

            set
            {
                EnsureChildControls();
                _ddlLocations.SelectedValue = value;
            }
        }

        #endregion

        #region Private Control Handling
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
        public virtual void RenderBaseControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( "id", this.ClientID );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "input-group col-sm-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
          
            _ddlLocations.RenderControl( writer );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "input-group-btn" );
            writer.RenderBeginTag( HtmlTextWriterTag.Span );
            _btnShowAddAddressForm.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            _pnlAddAddress.RenderControl( writer );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();
         
            _ddlLocations = new RockDropDownList
            {
                ID = $"{this.ClientID}{nameof( _ddlLocations )}",
                EnhanceForLongLists = true
            };

            Controls.Add( _ddlLocations );

            _btnShowAddAddressForm = new LinkButton
            {
                ID = $"{this.ClientID}{nameof( _btnShowAddAddressForm )}",
                CssClass = "btn btn-default btn-square",
                CausesValidation = false,
                Visible = AllowAdd,
            };
            _btnShowAddAddressForm.Click += btnShowAddAddressForm_Click;
            _btnShowAddAddressForm.Controls.Add( new HtmlGenericControl { InnerHtml = "<i class='fa fa-plus'></i>" } );
            Controls.Add( _btnShowAddAddressForm );

            GenerateAddAddressPanel();
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

            BindLocations( null );
        }
        #endregion

        #region Control Event Handlers
        private void btnAdd_Click( object sender, EventArgs e )
        {
            EnsureChildControls();

            var location = SaveLocationFromForm();

            BindLocations( location.Id );

            ShowAddressPicker();
        }

        private void btnCancel_Click( object sender, EventArgs e )
        {
            EnsureChildControls();

            ShowAddressPicker();
        }

        private void btnShowAddAddressForm_Click( object sender, EventArgs e )
        {
            EnsureChildControls();

            ShowAddressAdder();
        }
        #endregion

        #region Private Helper Methods
        private void BindLocations( int? currentLocationId )
        {
            ListItem[] locations = null;

            using ( var rockContext = new RockContext() )
            {
                var locationService = new LocationService( rockContext );
                var locationQuery = locationService
                    .Queryable()
                    .AsNoTracking()
                    .Where( l => l.IsActive )
                    .Where( l => LocationTypeValueId == null || l.LocationTypeValueId == LocationTypeValueId )
                    .Where( l => ParentLocationId == null || l.ParentLocationId == ParentLocationId )
                    .Select( l => new { l.Name, l.City, l.State, l.Id } )
                    .ToList()
                    .OrderBy( l => l.Name );

                if ( ShowCityState )
                {
                    locations = locationQuery
                        .Select( l => new ListItem( $"{l.Name} ({l.City}, {l.State})", l.Id.ToString() ) )
                        .ToArray();
                }
                else
                {
                    locations = locationQuery
                        .Select( l => new ListItem( $"{l.Name}", l.Id.ToString() ) )
                        .ToArray();
                }
            }

            if ( !Required )
            {
                _ddlLocations.Items.Add( new ListItem( string.Empty, string.Empty ) );
            }

            _ddlLocations.Items.AddRange( locations );

            if ( currentLocationId != null )
            {
                _ddlLocations.SelectedValue = currentLocationId.Value.ToString();
            }
        }

        private void GenerateAddAddressPanel()
        {
            _pnlAddAddress = new Panel
            {
                ID = $"{this.ClientID}{nameof( _pnlAddAddress )}",
                CssClass = "mt-3 well",
                Visible = false,
            };
            Controls.Add( _pnlAddAddress );

            _txtName = new RockTextBox
            {
                ID = $"{this.ClientID}{nameof( _txtName )}",
                CssClass = "mb-3",
                Placeholder = "Location Name",
            };
            _pnlAddAddress.Controls.Add( _txtName );

            _acNewAddress = new AddressControl
            {
                ID = $"{this.ClientID}{nameof( _acNewAddress )}",
                Required = IsAddressRequired,
            };
            _pnlAddAddress.Controls.Add( _acNewAddress );

            _avcAddressAttributeValues = new AttributeValuesContainer
            {
                ID = $"{this.ClientID}{nameof( _avcAddressAttributeValues )}",
            };
            _pnlAddAddress.Controls.Add( _avcAddressAttributeValues );

            _btnAdd = new BootstrapButton
            {
                ID = $"{this.ClientID}{nameof( _btnAdd )}",
                Text = "Add",
                CssClass = "btn btn-xs btn-primary mr-2",
            };
            _btnAdd.Click += btnAdd_Click;
            _pnlAddAddress.Controls.Add( _btnAdd );

            _btnCancel = new LinkButton
            {
                ID = $"{this.ClientID}{nameof( _btnCancel )}",
                Text = "Cancel",
                CssClass = "btn btn-xs btn-link",
                CausesValidation = false,
            };
            _btnCancel.Click += btnCancel_Click;
            _pnlAddAddress.Controls.Add( _btnCancel );

            _avcAddressAttributeValues.AddEditControls( new Location { LocationTypeValueId = LocationTypeValueId, ParentLocationId = ParentLocationId } );
        }

        private void ShowAddressAdder()
        {
            _ddlLocations.Visible = false;
            _btnShowAddAddressForm.Visible = false;

            _pnlAddAddress.Visible = true;
        }

        private void ShowAddressPicker()
        {
            _ddlLocations.Visible = true;
            _btnShowAddAddressForm.Visible = AllowAdd;

            _pnlAddAddress.Visible = false;
            _txtName.Text = string.Empty;
            _acNewAddress.Street1 = string.Empty;
            _acNewAddress.Street2 = string.Empty;
            _acNewAddress.City = string.Empty;
            _acNewAddress.State = string.Empty;
            _acNewAddress.PostalCode = string.Empty;
            _acNewAddress.Country = string.Empty;
        }

        private Location SaveLocationFromForm()
        {
            var location = new Location
            {
                Name = _txtName.Text,
                ParentLocationId = ParentLocationId,
                LocationTypeValueId = LocationTypeValueId,
                Street1 = _acNewAddress.Street1,
                Street2 = _acNewAddress.Street2,
                City = _acNewAddress.City,
                State = _acNewAddress.State,
                Country = _acNewAddress.Country,
                PostalCode = _acNewAddress.PostalCode,
                IsActive = true,
            };

            using ( var rockContext = new RockContext() )
            {
                var locationService = new LocationService( rockContext );

                locationService.Add( location );

                rockContext.SaveChanges();

                _avcAddressAttributeValues.GetEditValues( location );
                location.SaveAttributeValues( rockContext );
            }

            return location;
        }
        #endregion
    }
}
