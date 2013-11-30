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
        private RadioButton _radNamed;
        private RadioButton _radAddress;
        private RadioButton _radPoint;
        private RadioButton _radPolygon;

        private Panel _pickersPanel;
        private LocationItemPicker _locationItemPicker;
        private LocationAddressPicker _locationAddressPicker;
        private GeoPicker _locationGeoPicker;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the allowed picker modes.
        /// </summary>
        /// <value>
        /// The allowed picker modes.
        /// </value>
        public LocationPickerMode AllowedPickerModes
        {
            get
            {
                return ViewState["AllowedPickerModes"] as LocationPickerMode? ?? LocationPickerMode.All;
            }

            set
            {
                ViewState["AllowedPickerModes"] = value;
            }
        }
        
        /// <summary>
        /// Gets or sets the current picker mode.
        /// </summary>
        /// <value>
        /// The current picker mode.
        /// </value>
        public LocationPickerMode CurrentPickerMode
        {
            get
            {
                return ViewState["CurrentPickerMode"] as LocationPickerMode? ?? LocationPickerMode.Named;
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
                switch ( CurrentPickerMode )
                {
                    case LocationPickerMode.Address:
                        {
                            return _locationAddressPicker.Location;
                        }
                    case LocationPickerMode.Point:
                    case LocationPickerMode.Polygon:
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
                    return LocationPickerMode.Named;
                }

                if ( !string.IsNullOrWhiteSpace( location.GetFullStreetAddress().Replace( ",", string.Empty ) ) )
                {
                    return LocationPickerMode.Address;
                }

                if (location.GeoPoint != null)
                {
                    return LocationPickerMode.Point;
                }

                if (location.GeoFence != null)
                {
                    return LocationPickerMode.Polygon;
                }
            }

            return LocationPickerMode.Named;
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
            _radNamed.Attributes["onclick"] = this.Page.ClientScript.GetPostBackEventReference( new PostBackOptions( this, "NamedMode" ) );
            _radAddress.Attributes["onclick"] = this.Page.ClientScript.GetPostBackEventReference( new PostBackOptions( this, "AddressMode" ) );
            _radPoint.Attributes["onclick"] = this.Page.ClientScript.GetPostBackEventReference( new PostBackOptions( this, "PointMode" ) );
            _radPolygon.Attributes["onclick"] = this.Page.ClientScript.GetPostBackEventReference( new PostBackOptions( this, "PolygonMode" ) );

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
            var addressEnabled = (this.AllowedPickerModes & LocationPickerMode.Named) == LocationPickerMode.Named;
            var nameEnabled = ( this.AllowedPickerModes & LocationPickerMode.Address ) == LocationPickerMode.Address;
            var pointEnabled = ( this.AllowedPickerModes & LocationPickerMode.Point ) == LocationPickerMode.Point;
            var polygonEnabled = ( this.AllowedPickerModes & LocationPickerMode.Polygon ) == LocationPickerMode.Polygon;

            int modesEnabled = 0;

            _radNamed.Visible = nameEnabled;
            _radNamed.Checked = this.CurrentPickerMode == LocationPickerMode.Named;
            modesEnabled = nameEnabled ? modesEnabled + 1 : modesEnabled;

            _radAddress.Visible = addressEnabled;
            _radAddress.Checked = this.CurrentPickerMode == LocationPickerMode.Address;
            modesEnabled = addressEnabled ? modesEnabled + 1 : modesEnabled;

            _radPoint.Visible = pointEnabled;
            _radPoint.Checked = this.CurrentPickerMode == LocationPickerMode.Point;

            _radPolygon.Visible = polygonEnabled;
            _radPolygon.Checked = this.CurrentPickerMode == LocationPickerMode.Polygon;
            modesEnabled = (pointEnabled || polygonEnabled) ? modesEnabled + 1 : modesEnabled;

            _locationItemPicker.Visible = nameEnabled && this.CurrentPickerMode == LocationPickerMode.Named;
            _locationAddressPicker.Visible = addressEnabled && this.CurrentPickerMode == LocationPickerMode.Address;
            _locationGeoPicker.Visible = (pointEnabled && this.CurrentPickerMode == LocationPickerMode.Point) || (polygonEnabled && this.CurrentPickerMode == LocationPickerMode.Polygon);

            _pnlModeSelection.Visible = modesEnabled > 1;

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

            _radNamed = new RadioButton { ID = "radNamed" };
            _radNamed.Text = "Location";
            _radNamed.GroupName = "radiogroup-location-mode_" + this.ClientID;
            _pnlModeSelection.Controls.Add( _radNamed );

            _radAddress = new RadioButton { ID = "radAddress" };
            _radAddress.Text = "Address";
            _radAddress.GroupName = "radiogroup-location-mode_" + this.ClientID;
            _pnlModeSelection.Controls.Add( _radAddress );
            
            _radPoint = new RadioButton { ID = "radPoint" };
            _radPoint.Text = "Point";
            _radPoint.GroupName = "radiogroup-location-mode_" + this.ClientID;
            _pnlModeSelection.Controls.Add( _radPoint );

            _radPolygon = new RadioButton { ID = "radPolygon" };
            _radPolygon.Text = "Polygon";
            _radPolygon.GroupName = "radiogroup-location-mode_" + this.ClientID;
            _pnlModeSelection.Controls.Add( _radPolygon ); 
            
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
            location = locationService.GetByGeoLocation( _locationGeoPicker.SelectedValue );

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

            _radNamed.Checked = eventArgument == "NamedMode";
            _radAddress.Checked = eventArgument == "AddressMode";
            _radPoint.Checked = eventArgument == "PointMode";
            _radPolygon.Checked = eventArgument == "PolygonMode";

            _locationItemPicker.Visible = _radNamed.Checked;
            _locationItemPicker.ShowDropDown = _radNamed.Checked;

            _locationAddressPicker.Visible = _radAddress.Checked;
            _locationAddressPicker.ShowDropDown = _radAddress.Checked;

            _locationGeoPicker.Visible = _radPoint.Checked || _radPolygon.Checked;
            _locationGeoPicker.ShowDropDown = _radPoint.Checked || _radPolygon.Checked;

            if ( _radAddress.Checked )
            {
                this.CurrentPickerMode = LocationPickerMode.Address;
            }
            else if ( _radNamed.Checked )
            {
                this.CurrentPickerMode = LocationPickerMode.Named;
            }
            else if ( _radPoint.Checked )
            {
                if (this.CurrentPickerMode != LocationPickerMode.Point)
                {
                    _locationGeoPicker.SelectedValue = null;
                }

                this.CurrentPickerMode = LocationPickerMode.Point;
                _locationGeoPicker.DrawingMode = GeoPicker.ManagerDrawingMode.Point;
            }
            else if (_radPolygon.Checked)
            {
                if (this.CurrentPickerMode != LocationPickerMode.Polygon)
                {
                    _locationGeoPicker.SelectedValue = null;
                }

                this.CurrentPickerMode = LocationPickerMode.Polygon;
                _locationGeoPicker.DrawingMode = GeoPicker.ManagerDrawingMode.Polygon;
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
                switch ( CurrentPickerMode )
                {
                    case LocationPickerMode.Address:
                        return _locationAddressPicker.IsValid;
                    case LocationPickerMode.Point:
                    case LocationPickerMode.Polygon:
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

    #region class enums

    /// <summary>
    /// 
    /// </summary>
    /// <summary>
    /// Represents the type of <see cref="Rock.Model.Location">Locations</see> that should be allowed to be selected using the location picker.
    /// </summary>
    [Flags]
    public enum LocationPickerMode
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// An Address
        /// </summary>
        Address = 1,

        /// <summary>
        /// A Named location (Building, Room)
        /// </summary>
        Named = 2,

            /// <summary>
        /// A Geographic point (Latitude/Longitude)
        /// </summary>
        Point = 4,

        /// <summary>
        /// A Geographic Polygon
        /// </summary>
        Polygon = 8,

        /// <summary>
        /// All
        /// </summary>
        All = Address | Named | Point | Polygon,

    }

    #endregion
}