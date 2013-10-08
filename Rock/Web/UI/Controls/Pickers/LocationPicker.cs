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
    public class LocationPicker : CompositeControl
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
            NamedLocation,
            Address,
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
        public bool LimitToNamedLocations
        {
            get
            {
                return ( ViewState["LimitToNamedLocations"] as bool? ) ?? true;
            }

            set
            {
                ViewState["LimitToNamedLocations"] = value;
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
                if ( LimitToNamedLocations )
                {
                    return LocationPickerMode.NamedLocation;
                }
                else
                {
                    return ViewState["CurrentPickerMode"] as LocationPickerMode? ?? LocationPickerMode.NamedLocation;
                }
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
                            return new LocationService().GetByGeoLocation( _locationGeoPicker.SelectedValue );
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
                _locationGeoPicker.SetValue( value.GeoPoint ?? value.GeoFence );
            }
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
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();

            // Mode Selection Panel and Controls
            _pnlModeSelection = new Panel { ID = "pnlModeSelection" };
            _pnlModeSelection.CssClass = "picker-mode-options";
            _pnlModeSelection.Visible = !this.LimitToNamedLocations;
            _pnlModeSelection.ViewStateMode = ViewStateMode.Enabled;

            _radNamedLocation = new RadioButton { ID = "radNamedLocation" };
            _radNamedLocation.Text = "Named Location";
            _radNamedLocation.Checked = this.PickerMode == LocationPickerMode.NamedLocation;
            _radNamedLocation.GroupName = "radiogroup-location-mode_" + this.ClientID;
            _pnlModeSelection.Controls.Add( _radNamedLocation );

            _radAddress = new RadioButton { ID = "radAddress" };
            _radAddress.Text = "Address";
            _radAddress.Checked = this.PickerMode == LocationPickerMode.Address;
            _radAddress.GroupName = "radiogroup-location-mode_" + this.ClientID;
            _pnlModeSelection.Controls.Add( _radAddress );

            _radLatLong = new RadioButton { ID = "radLatLong" };
            _radLatLong.Text = "Lat/Long";
            _radLatLong.Checked = this.PickerMode == LocationPickerMode.LatLong;
            _radLatLong.GroupName = "radiogroup-location-mode_" + this.ClientID;
            _pnlModeSelection.Controls.Add( _radLatLong );

            _pickersPanel = new Panel { ID = "pickersPanel" };
            _pickersPanel.ViewStateMode = ViewStateMode.Disabled;
            this.Controls.Add( _pickersPanel );

            _locationItemPicker = new LocationItemPicker();
            _locationItemPicker.ID = this.ID + "_locationItemPicker";
            _locationItemPicker.Visible = this.PickerMode == LocationPickerMode.NamedLocation;
            _locationAddressPicker = new LocationAddressPicker();
            _locationAddressPicker.ID = this.ID + "_locationAddressPicker";
            _locationAddressPicker.Visible = this.PickerMode == LocationPickerMode.Address;
            _locationGeoPicker = new GeoPicker();
            _locationGeoPicker.ID = this.ID + "_locationGeoPicker";
            _locationGeoPicker.Visible = this.PickerMode == LocationPickerMode.LatLong;

            _locationItemPicker.ModePanel = _pnlModeSelection;
            _locationGeoPicker.ModePanel = _pnlModeSelection;
            _locationAddressPicker.ModePanel = _pnlModeSelection;

            _pickersPanel.Controls.Add( _locationItemPicker );
            _pickersPanel.Controls.Add( _locationAddressPicker );
            _pickersPanel.Controls.Add( _locationGeoPicker );
        }

        /// <summary>
        /// Handles the CheckedChanged event of the _radMode control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
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
    }


}