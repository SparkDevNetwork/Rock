//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class LocationPicker : CompositeControl, IRockControl
    {
        #region Controls

        private Panel _pnlModeSelection;
        private RadioButton _radNamedLocation;
        private RadioButton _radAddress;
        private RadioButton _radLatLong;

        private Panel _pickersPanel;
        private LocationItemPicker _locationItemPicker;
        private LocationAddressPicker _locationAddressPicker;
        private GeoPicker _locationGeoPicker;

        #endregion

        #region class enums

        /// <summary>
        /// 
        /// </summary>
        public enum LocationPickerMode
        {
            /// <summary>
            /// The named location
            /// </summary>
            NamedLocation,

            /// <summary>
            /// The address
            /// </summary>
            Address,

            /// <summary>
            /// The lat long
            /// </summary>
            LatLong
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether [limit automatic named locations].
        /// </summary>
        /// <value>
        /// <c>true</c> if [limit automatic named locations]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowModeSelection
        {
            get
            {
                return ( ViewState["AllowModeSelection"] as bool? ) ?? true;
            }

            set
            {
                ViewState["AllowModeSelection"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the current picker mode.
        /// </summary>
        /// <value>
        /// The current picker mode.
        /// </value>
        public LocationPickerMode PickerMode
        {
            get
            {
                return ViewState["CurrentPickerMode"] as LocationPickerMode? ?? LocationPickerMode.NamedLocation;
            }

            set
            {
                ViewState["CurrentPickerMode"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        public Location Location
        {
            get
            {
                EnsureChildControls();
                switch ( PickerMode )
                {
                    case LocationPickerMode.Address:
                        {
                            return _locationAddressPicker.Location;
                        }
                    case LocationPickerMode.LatLong:
                        {
                            if ( _locationGeoPicker.SelectedValue != null )
                            {
                                return new LocationService().GetByGeoLocation( _locationGeoPicker.SelectedValue );
                            }
                            else
                            {
                                return null;
                            }
                        }
                    default:
                        {
                            return new LocationService().Get( _locationItemPicker.SelectedValueAsId() ?? 0 );
                        }
                }
            }

            set
            {
                EnsureChildControls();
                _locationAddressPicker.SetValue( value );
                _locationItemPicker.SetValue( value );
                if ( value != null )
                {
                    _locationGeoPicker.SetValue( value.GeoPoint ?? value.GeoFence );
                }
                else
                {
                    _locationGeoPicker.SetValue( null );
                }
            }
        }

        /// <summary>
        /// Gets the best picker mode for location.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        public LocationPickerMode GetBestPickerModeForLocation( Location location )
        {
            if ( location != null )
            {
                if ( location.IsNamedLocation )
                {
                    return LocationPickerMode.NamedLocation;
                }
                else
                {
                    if ( !string.IsNullOrWhiteSpace( location.GetFullStreetAddress().Replace( ",", string.Empty ) ) )
                    {
                        return LocationPickerMode.Address;
                    }
                    else
                    {
                        var geo = location.GeoPoint ?? location.GeoFence;
                        if ( geo != null )
                        {
                            return LocationPickerMode.LatLong;
                        }
                        else
                        {
                            return LocationPickerMode.NamedLocation;
                        }
                    }
                }
            }

            return LocationPickerMode.NamedLocation;
        }


        #endregion

        #region Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            EnsureChildControls();

            // set the "onclick" attributes manually so that we can consistently handle the postbacks even though they are coming from any of the 3 pickers
            _radNamedLocation.Attributes["onclick"] = this.Page.ClientScript.GetPostBackEventReference( new PostBackOptions( this, "NamedLocationMode" ) );
            _radAddress.Attributes["onclick"] = this.Page.ClientScript.GetPostBackEventReference( new PostBackOptions( this, "AddressMode" ) );
            _radLatLong.Attributes["onclick"] = this.Page.ClientScript.GetPostBackEventReference( new PostBackOptions( this, "LatLongMode" ) );

            if ( Page.IsPostBack )
            {
                HandleModePostback();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            _radNamedLocation.Checked = this.PickerMode == LocationPickerMode.NamedLocation;
            _radAddress.Checked = this.PickerMode == LocationPickerMode.Address;
            _radLatLong.Checked = this.PickerMode == LocationPickerMode.LatLong;
            _locationItemPicker.Visible = this.PickerMode == LocationPickerMode.NamedLocation;
            _locationAddressPicker.Visible = this.PickerMode == LocationPickerMode.Address;
            _locationGeoPicker.Visible = this.PickerMode == LocationPickerMode.LatLong;
            _pnlModeSelection.Visible = this.AllowModeSelection;
            

            base.OnPreRender( e );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();

            // Mode Selection Panel and Controls
            _pnlModeSelection = new Panel { ID = "pnlModeSelection" };
            _pnlModeSelection.CssClass = "picker-mode-options";
            _pnlModeSelection.ViewStateMode = ViewStateMode.Enabled;

            _radNamedLocation = new RadioButton { ID = "radNamedLocation" };
            _radNamedLocation.Text = "Named Location";
            _radNamedLocation.GroupName = "radiogroup-location-mode_" + this.ClientID;
            _pnlModeSelection.Controls.Add( _radNamedLocation );

            _radAddress = new RadioButton { ID = "radAddress" };
            _radAddress.Text = "Address";
            _radAddress.GroupName = "radiogroup-location-mode_" + this.ClientID;
            _pnlModeSelection.Controls.Add( _radAddress );

            _radLatLong = new RadioButton { ID = "radLatLong" };
            _radLatLong.Text = "Lat/Long";
            _radLatLong.GroupName = "radiogroup-location-mode_" + this.ClientID;
            _pnlModeSelection.Controls.Add( _radLatLong );

            _pickersPanel = new Panel { ID = "pickersPanel" };
            _pickersPanel.ViewStateMode = ViewStateMode.Disabled;
            this.Controls.Add( _pickersPanel );

            _locationItemPicker = new LocationItemPicker();
            _locationItemPicker.ID = this.ID + "_locationItemPicker";
            _locationAddressPicker = new LocationAddressPicker();
            _locationAddressPicker.ID = this.ID + "_locationAddressPicker";
            _locationGeoPicker = new GeoPicker();
            _locationGeoPicker.ID = this.ID + "_locationGeoPicker";
            _locationGeoPicker.SelectGeography += _locationGeoPicker_SelectGeography;

            _locationItemPicker.ModePanel = _pnlModeSelection;
            _locationGeoPicker.ModePanel = _pnlModeSelection;
            _locationAddressPicker.ModePanel = _pnlModeSelection;

            _pickersPanel.Controls.Add( _locationItemPicker );
            _pickersPanel.Controls.Add( _locationAddressPicker );
            _pickersPanel.Controls.Add( _locationGeoPicker );
        }

        /// <summary>
        /// Handles the SelectGeography event of the _locationGeoPicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void _locationGeoPicker_SelectGeography( object sender, EventArgs e )
        {
            // this will autosave the GeoPicker result as Location if it isn't in the database already
            LocationService locationService = new LocationService();
            Location location = null;
            location = locationService.GetByGeoLocation( _locationGeoPicker.GeoPoint ?? _locationGeoPicker.GeoFence );

            Location = location;
        }

        /// <summary>
        /// Handles the CheckedChanged event of the _radMode control.
        /// </summary>
        protected void HandleModePostback()
        {
            // Note:  We have to manually wire up the PostBacks since these controls are injected into all three of the pickers and that messes up the normal postback stuff
            string eventTarget = this.Page.Request.Params["__EVENTTARGET"];
            string eventArgument = this.Page.Request.Params["__EVENTARGUMENT"];
            EnsureChildControls();

            // jump out if we this isn't an EventTarget we are expecting
            if ( eventTarget != this.UniqueID )
            {
                return;
            }

            _radNamedLocation.Checked = eventArgument == "NamedLocationMode";
            _radAddress.Checked = eventArgument == "AddressMode";
            _radLatLong.Checked = eventArgument == "LatLongMode";

            _locationItemPicker.Visible = _radNamedLocation.Checked;
            _locationAddressPicker.Visible = _radAddress.Checked;
            _locationGeoPicker.Visible = _radLatLong.Checked;

            _locationItemPicker.ShowDropDown = _radNamedLocation.Checked;
            _locationAddressPicker.ShowDropDown = _radAddress.Checked;
            _locationGeoPicker.ShowDropDown = _radLatLong.Checked;

            if ( _radAddress.Checked )
            {
                this.PickerMode = LocationPickerMode.Address;
            }
            else if ( _radLatLong.Checked )
            {
                this.PickerMode = LocationPickerMode.LatLong;
            }
            else
            {
                this.PickerMode = LocationPickerMode.NamedLocation;
            }
        }

        #endregion

        /// <summary>
        /// Renders the <see cref="T:System.Web.UI.WebControls.TextBox" /> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> that receives the rendered output.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            //
        }

        #region IRockControl implementation (much different than others)

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label text
        /// </value>
        public string Label
        {
            get
            {
                EnsureChildControls();
                return _locationItemPicker.Label;
            }
            set
            {
                EnsureChildControls();
                _locationItemPicker.Label = value;
                _locationAddressPicker.Label = value;
                _locationGeoPicker.Label = value;
            }
        }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        public string Help
        {
            get
            {
                EnsureChildControls();
                return _locationItemPicker.Help;
            }
            set
            {
                EnsureChildControls();
                _locationItemPicker.Help = value;
                _locationAddressPicker.Help = value;
                _locationGeoPicker.Help = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IRockControl" /> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        public bool Required
        {
            get
            {
                EnsureChildControls();
                return _locationItemPicker.Required;
            }
            set
            {
                EnsureChildControls();
                _locationItemPicker.Required = value;
                _locationAddressPicker.Required = value;
                _locationGeoPicker.Required = value;
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
                EnsureChildControls();
                return _locationItemPicker.RequiredErrorMessage;
            }
            set
            {
                EnsureChildControls();
                _locationItemPicker.RequiredErrorMessage = value;
                _locationAddressPicker.RequiredErrorMessage = value;
                _locationGeoPicker.RequiredErrorMessage = value;
            }
        }

        /// <summary>
        /// Gets or sets an optional validation group to use.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get
            {
                EnsureChildControls();
                return _locationItemPicker.ValidationGroup;
            }
            set
            {
                EnsureChildControls();
                _locationItemPicker.ValidationGroup = value;
                _locationAddressPicker.ValidationGroup = value;
                _locationGeoPicker.ValidationGroup = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid
        {
            get
            {
                EnsureChildControls();
                switch ( PickerMode )
                {
                    case LocationPickerMode.Address:
                        return _locationAddressPicker.IsValid;
                    case LocationPickerMode.LatLong:
                        return _locationGeoPicker.IsValid;
                    default:
                        return _locationItemPicker.IsValid;
                }
            }
        }

        /// <summary>
        /// Gets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public HelpBlock HelpBlock
        {
            get
            {
                EnsureChildControls();
                return _locationItemPicker.HelpBlock;
            }
            set
            {
                EnsureChildControls();
                _locationItemPicker.HelpBlock = value;
                _locationAddressPicker.HelpBlock = value;
                _locationGeoPicker.HelpBlock = value;
            }
        }

        /// <summary>
        /// Gets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator
        {
            get
            {
                EnsureChildControls();
                return _locationItemPicker.RequiredFieldValidator;
            }
            set
            {
                EnsureChildControls();
                _locationItemPicker.RequiredFieldValidator = value;
                _locationAddressPicker.RequiredFieldValidator = value;
                _locationGeoPicker.RequiredFieldValidator = value;
            }
        }

        #endregion
    }
}