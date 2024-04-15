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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control that can be used to add a new location or select an existing location
    /// </summary>
    public class LocationPicker : CompositeControl, IRockControl, IDisplayRequiredIndicator
    {
        #region Controls

        private Panel _pnlModeSelection;
        private RockRadioButton _radNamed;
        private RockRadioButton _radAddress;
        private RockRadioButton _radPoint;
        private RockRadioButton _radPolygon;

        private Panel _pickersPanel;
        private LocationItemPicker _namedPicker;
        private LocationAddressPicker _addressPicker;
        private GeoPicker _pointPicker;
        private GeoPicker _polygonPicker;
        private HiddenField _hfCurrentPickerMode;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationPicker" /> class.
        /// </summary>
        public LocationPicker()
            : base()
        {
            HelpBlock = new HelpBlock();
            WarningBlock = new WarningBlock();
        }

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
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsValid
        {
            get
            {
                return CustomValidator == null || CustomValidator.IsValid;
            }
        }

        /// <summary>
        /// Indicates whether inactive named locations should be included.  This only affects named locations.
        /// (Note:  prior to Rock 9.1, the default behavior of this control was to include inactive locations.)
        /// </summary>
        public bool IncludeInactiveNamedLocations
        {
            get
            {
                return ViewState["IncludeInactiveNamedLocations"] as bool? ?? false;
            }
            set
            {
                ViewState["IncludeInactiveNamedLocations"] = value;
                if ( _namedPicker != null )
                {
                    _namedPicker.IncludeInactive = value;
                }
            }
        }

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
                var allowedPickerModes = ViewState["AllowedPickerModes"] as LocationPickerMode?;
                if ( !allowedPickerModes.HasValue )
                {
                    allowedPickerModes = LocationPickerMode.All;
                    AllowedPickerModes = allowedPickerModes.Value;
                }

                return allowedPickerModes.Value;
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
                /* [DL] 2020-06-02
                 * The picker mode is stored both in ViewState and in a hidden field that is updated by client script to reflect any changes.
                 * To retrieve the current picker mode we need to use the value stored in the hidden field if it is available, in preference to ViewState.
                 * In some circumstances, such as when this control is used as a child control of the AttributeMatrixEditor, ViewState does not correctly
                 * reflect the picker mode if a postback is triggered by another editor control and the picker has not been previously accessed.
                 */
                EnsureChildControls();

                var currentPickerMode = _hfCurrentPickerMode?.Value.ConvertToEnumOrNull<LocationPickerMode>();

                if ( !currentPickerMode.HasValue )
                {
                    currentPickerMode = ViewState["CurrentPickerMode"] as LocationPickerMode?;
                }

                if ( !currentPickerMode.HasValue )
                {
                    if ( ( this.AllowedPickerModes & LocationPickerMode.Address ) == LocationPickerMode.Address )
                    {
                        currentPickerMode = LocationPickerMode.Address;
                    }
                    else if ( ( this.AllowedPickerModes & LocationPickerMode.Point ) == LocationPickerMode.Point )
                    {
                        currentPickerMode = LocationPickerMode.Point;
                    }
                    else if ( ( this.AllowedPickerModes & LocationPickerMode.Polygon ) == LocationPickerMode.Polygon )
                    {
                        currentPickerMode = LocationPickerMode.Polygon;
                    }
                    else
                    {
                        currentPickerMode = LocationPickerMode.Named;
                    }

                    CurrentPickerMode = currentPickerMode.Value;
                }

                return currentPickerMode.Value;
            }

            set
            {
                ViewState["CurrentPickerMode"] = value;
                if ( _hfCurrentPickerMode != null )
                {
                    _hfCurrentPickerMode.Value = value.ConvertToString();
                }
            }

        }

        /// <summary>
        /// Gets or sets the named picker root location identifier, which will be passed to the Rest endpoint. Leave the default value of 0 to get all locations.
        /// Note: Setting this property will overwrite any value currently in the ItemPicker.ItemRestUrlExtraParams property.
        /// </summary>
        /// <value>
        /// The named picker root location identifier.
        /// </value>
        public int NamedPickerRootLocationId
        {
            get
            {
                return ( ViewState["NamedPickerRootLocationId"] as string ).AsInteger();
            }
            set
            {
                ViewState["NamedPickerRootLocationId"] = value.ToString();
                if ( _namedPicker != null )
                {
                    _namedPicker.RootLocationId = value;
                }
            }
        }

        /// <summary>
        /// Sets the named location.
        /// Does nothing if <seealso cref="CurrentPickerMode"/> is not <seealso cref="LocationPickerMode.Named"/>
        /// </summary>
        /// <param name="namedLocation">The named location.</param>
        public void SetNamedLocation( NamedLocationCache namedLocation )
        {
            _namedPicker?.SetValueFromLocationId( namedLocation?.Id );
        }

        /// <summary>
        /// Gets the named location.
        /// Returns null if <seealso cref="CurrentPickerMode"/> is not <seealso cref="LocationPickerMode.Named"/>
        /// </summary>
        /// <value>
        /// The named location.
        /// </value>
        public NamedLocationCache NamedLocation
        {
            get
            {
                var namedLocationId = _namedPicker?.SelectedValueAsId();
                if ( namedLocationId.HasValue )
                {
                    return NamedLocationCache.Get( namedLocationId.Value );
                }

                return null;
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
                            return _addressPicker.Location;
                        }
                    case LocationPickerMode.Point:
                        {
                            if ( _pointPicker.SelectedValue != null )
                            {
                                return new LocationService( new RockContext() ).GetByGeoPoint( _pointPicker.SelectedValue );
                            }
                            else
                            {
                                return null;
                            }
                        }
                    case LocationPickerMode.Polygon:
                        {
                            if ( _polygonPicker.SelectedValue != null )
                            {
                                return new LocationService( new RockContext() ).GetByGeoFence( _polygonPicker.SelectedValue );
                            }
                            else
                            {
                                return null;
                            }
                        }
                    default:
                        {
                            return new LocationService( new RockContext() ).Get( _namedPicker.SelectedValueAsId() ?? 0 );
                        }
                }
            }

            set
            {
                EnsureChildControls();
                switch ( CurrentPickerMode )
                {
                    case LocationPickerMode.Address:
                        {
                            _addressPicker.SetValue( value );
                            break;
                        }
                    case LocationPickerMode.Point:
                        {
                            if ( value != null )
                            {
                                _pointPicker.SetValue( value.GeoPoint );
                            }
                            break;
                        }
                    case LocationPickerMode.Polygon:
                        {
                            if ( value != null )
                            {
                                _polygonPicker.SetValue( value.GeoFence );
                            }
                            break;
                        }
                    default:
                        {
                            _namedPicker.SetValueFromLocationId( value?.Id );
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Sets the best picker mode for location.
        /// </summary>
        /// <param name="location">The location.</param>
        public void SetBestPickerModeForLocation( Location location )
        {
            this.CurrentPickerMode = this.GetBestPickerModeForLocation( location );
        }

        /// <summary>
        /// Gets the best picker mode for location.
        /// NOTE: Use SetBestPickerModeForLocation to *set* the CurrentPickerMode based on Location 
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

                if ( location.GeoPoint != null )
                {
                    return LocationPickerMode.Point;
                }

                if ( location.GeoFence != null )
                {
                    return LocationPickerMode.Polygon;
                }
            }

            // If there is no location, then base the best picker mode based on the allowed picker modes and the current picker mode
            if ( ( this.AllowedPickerModes & CurrentPickerMode ) == CurrentPickerMode )
            {
                // if the current picker mode is allowed, just use that
                return CurrentPickerMode;
            }

            if ( ( this.AllowedPickerModes & LocationPickerMode.Named ) == LocationPickerMode.Named )
                return LocationPickerMode.Named;
            if ( ( this.AllowedPickerModes & LocationPickerMode.Address ) == LocationPickerMode.Address )
                return LocationPickerMode.Address;
            if ( ( this.AllowedPickerModes & LocationPickerMode.Point ) == LocationPickerMode.Point )
                return LocationPickerMode.Point;
            if ( ( this.AllowedPickerModes & LocationPickerMode.Polygon ) == LocationPickerMode.Polygon )
                return LocationPickerMode.Polygon;

            // probably won't happen unless a new LocationPickerMode is added later, but just in case we get this far, return CurrentPickerMode
            return CurrentPickerMode;
        }

        /// <summary>
        /// Gets or sets the map style.
        /// </summary>
        /// <value>
        /// The map style.
        /// </value>
        public Guid MapStyleValueGuid
        {
            get
            {
                EnsureChildControls();
                return _pointPicker.MapStyleValueGuid;
            }
            set
            {
                EnsureChildControls();
                _pointPicker.MapStyleValueGuid = value;
                _polygonPicker.MapStyleValueGuid = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Restores view-state information from a previous request that was saved with the <see cref="M:System.Web.UI.WebControls.WebControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An object that represents the control state to restore.</param>
        protected override void LoadViewState( object savedState )
        {
            EnsureChildControls();

            base.LoadViewState( savedState );

            var currentPickerMode = ViewState["CurrentPickerMode"] as LocationPickerMode?;
            if ( currentPickerMode.HasValue )
            {
                this.CurrentPickerMode = currentPickerMode.Value;
            }

            var locationId = ViewState["LocationId"] as int?;
            if ( locationId.HasValue )
            {
                if ( currentPickerMode == LocationPickerMode.Named )
                {
                    SetNamedLocation( NamedLocationCache.Get( locationId.Value ) );
                }
                else
                {
                    var location = new LocationService( new RockContext() ).Get( locationId.Value );
                    if ( location != null )
                    {
                        this.Location = location;
                    }
                }
            }
        }

        /// <summary>
        /// Saves any state that was modified after the <see cref="M:System.Web.UI.WebControls.Style.TrackViewState" /> method was invoked.
        /// </summary>
        /// <returns>
        /// An object that contains the current view state of the control; otherwise, if there is no view state associated with the control, null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["CurrentPickerMode"] = this.CurrentPickerMode;

            if ( CurrentPickerMode == LocationPickerMode.Named )
            {
                ViewState["LocationId"] = NamedLocation?.Id;
            }
            else
            {
                ViewState["LocationId"] = Location?.Id;
            }

            return base.SaveViewState();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            EnsureChildControls();

            // set the "onclick" attributes manually so that we can consistently handle the postbacks even though they are coming from any of the 3 pickers
            string postBackScriptFormat = "$('#{2}').val('{1}');  __doPostBack('{0}','{1}');";

            _radNamed.Attributes["onclick"] = string.Format( postBackScriptFormat, this.UniqueID, "Named", _hfCurrentPickerMode.ClientID );
            _radAddress.Attributes["onclick"] = string.Format( postBackScriptFormat, this.UniqueID, "Address", _hfCurrentPickerMode.ClientID );
            _radPoint.Attributes["onclick"] = string.Format( postBackScriptFormat, this.UniqueID, "Point", _hfCurrentPickerMode.ClientID );
            _radPolygon.Attributes["onclick"] = string.Format( postBackScriptFormat, this.UniqueID, "Polygon", _hfCurrentPickerMode.ClientID );

            // Disable the Address Picker validation by default, it should only be activated when that control is displayed.
            _addressPicker.ValidationIsDisabled = true;

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
            var nameEnabled = ( this.AllowedPickerModes & LocationPickerMode.Named ) == LocationPickerMode.Named;
            var addressEnabled = ( this.AllowedPickerModes & LocationPickerMode.Address ) == LocationPickerMode.Address;
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
            modesEnabled = pointEnabled ? modesEnabled + 1 : modesEnabled;

            _radPolygon.Visible = polygonEnabled;
            _radPolygon.Checked = this.CurrentPickerMode == LocationPickerMode.Polygon;
            modesEnabled = polygonEnabled ? modesEnabled + 1 : modesEnabled;

            _namedPicker.Visible = nameEnabled && this.CurrentPickerMode == LocationPickerMode.Named;
            _addressPicker.Visible = addressEnabled && this.CurrentPickerMode == LocationPickerMode.Address;
            _pointPicker.Visible = pointEnabled && this.CurrentPickerMode == LocationPickerMode.Point;
            _polygonPicker.Visible = polygonEnabled && this.CurrentPickerMode == LocationPickerMode.Polygon;

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

            _hfCurrentPickerMode = new HiddenField();
            _hfCurrentPickerMode.ID = this.ID + "_hfCurrentPickerMode";
            this.Controls.Add( _hfCurrentPickerMode );

            _radNamed = new RockRadioButton { ID = "radNamed" };
            _radNamed.Text = "Location";
            _radNamed.GroupName = "radiogroup-location-mode_" + this.ClientID;
            _pnlModeSelection.Controls.Add( _radNamed );

            _radAddress = new RockRadioButton { ID = "radAddress" };
            _radAddress.Text = "Address";
            _radAddress.GroupName = "radiogroup-location-mode_" + this.ClientID;
            _pnlModeSelection.Controls.Add( _radAddress );

            _radPoint = new RockRadioButton { ID = "radPoint" };
            _radPoint.Text = "Point";
            _radPoint.GroupName = "radiogroup-location-mode_" + this.ClientID;
            _pnlModeSelection.Controls.Add( _radPoint );

            _radPolygon = new RockRadioButton { ID = "radPolygon" };
            _radPolygon.Text = "Geo-fence";
            _radPolygon.GroupName = "radiogroup-location-mode_" + this.ClientID;
            _pnlModeSelection.Controls.Add( _radPolygon );

            _pickersPanel = new Panel { ID = "pickersPanel" };
            _pickersPanel.ViewStateMode = ViewStateMode.Disabled;
            this.Controls.Add( _pickersPanel );

            _namedPicker = new LocationItemPicker();
            _namedPicker.ID = this.ID + "_namedPicker";
            _namedPicker.SelectItem += _namedPicker_SelectItem;
            _namedPicker.IncludeInactive = this.IncludeInactiveNamedLocations;
            _namedPicker.RootLocationId = this.NamedPickerRootLocationId;

            _addressPicker = new LocationAddressPicker();
            _addressPicker.ID = this.ID + "_addressPicker";
            _addressPicker.SelectGeography += _addressPicker_SelectGeography;

            _pointPicker = new GeoPicker();
            _pointPicker.ID = this.ID + "_pointPicker";
            _pointPicker.DrawingMode = GeoPicker.ManagerDrawingMode.Point;
            _pointPicker.SelectGeography += _pointPicker_SelectGeography;

            _polygonPicker = new GeoPicker();
            _polygonPicker.ID = this.ID + "_polygonPicker";
            _polygonPicker.DrawingMode = GeoPicker.ManagerDrawingMode.Polygon;
            _polygonPicker.SelectGeography += _polygonPicker_SelectGeography;

            _namedPicker.ModePanel = _pnlModeSelection;
            _pointPicker.ModePanel = _pnlModeSelection;
            _polygonPicker.ModePanel = _pnlModeSelection;
            _addressPicker.ModePanel = _pnlModeSelection;

            _pickersPanel.Controls.Add( _namedPicker );
            _pickersPanel.Controls.Add( _addressPicker );
            _pickersPanel.Controls.Add( _pointPicker );
            _pickersPanel.Controls.Add( _polygonPicker );

            // Add custom validator
            CustomValidator = new CustomValidator();
            CustomValidator.ID = this.ID + "_cfv";
            CustomValidator.CssClass = "validation-error";
            CustomValidator.Enabled = true;
            CustomValidator.ServerValidate += _CustomValidator_ServerValidate;
            CustomValidator.Display = ValidatorDisplay.None;
            this.Controls.Add( CustomValidator );
        }

        private void _CustomValidator_ServerValidate( object source, ServerValidateEventArgs args )
        {
            if ( this.Required
                 && this.Location == null )
            {
                args.IsValid = false;

                var controlName = string.IsNullOrWhiteSpace( this.Label ) ? "Location" : this.Label;
                CustomValidator.ErrorMessage = $"{controlName} is required.";

                return;
            }

            switch ( this.CurrentPickerMode )
            {
                case LocationPickerMode.Address:
                    args.IsValid = _addressPicker.IsValid;
                break;
                case LocationPickerMode.Point:
                    args.IsValid = _pointPicker.IsValid;
                    break;
                case LocationPickerMode.Polygon:
                    args.IsValid = _polygonPicker.IsValid;
                    break;
                case LocationPickerMode.Named:
                    args.IsValid = _namedPicker.IsValid;
                    break;
            }
        }

        /// <summary>
        /// Handles the SelectItem event of the _namedPicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _namedPicker_SelectItem( object sender, EventArgs e )
        {
            LocationSelected( sender, e );
        }

        /// <summary>
        /// Locations the selected.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void LocationSelected( object sender, EventArgs e )
        {
            SelectLocation?.Invoke( sender, e );
        }

        /// <summary>
        /// Handles the SelectGeography event of the _addressPicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _addressPicker_SelectGeography( object sender, EventArgs e )
        {
            Location = _addressPicker.Location;
            LocationSelected( sender, e );
        }

        /// <summary>
        /// Handles the SelectGeography event of the _pointPicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _pointPicker_SelectGeography( object sender, EventArgs e )
        {
            Location = new LocationService( new RockContext() ).GetByGeoPoint( _pointPicker.SelectedValue );
            LocationSelected( sender, e );
        }

        /// <summary>
        /// Handles the SelectGeography event of the _polygonPicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _polygonPicker_SelectGeography( object sender, EventArgs e )
        {
            Location = new LocationService( new RockContext() ).GetByGeoFence( _polygonPicker.SelectedValue );
            LocationSelected( sender, e );
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

            _hfCurrentPickerMode.Value = eventArgument;
            CurrentPickerMode = eventArgument.ConvertToEnum<LocationPickerMode>( LocationPickerMode.Named );

            _radNamed.Checked = CurrentPickerMode == LocationPickerMode.Named;
            _radAddress.Checked = CurrentPickerMode == LocationPickerMode.Address;
            _radPoint.Checked = CurrentPickerMode == LocationPickerMode.Point;
            _radPolygon.Checked = CurrentPickerMode == LocationPickerMode.Polygon;

            _namedPicker.Visible = CurrentPickerMode == LocationPickerMode.Named;
            _namedPicker.ShowDropDown = CurrentPickerMode == LocationPickerMode.Named;
            _namedPicker.IncludeInactive = this.IncludeInactiveNamedLocations;

            _addressPicker.Visible = CurrentPickerMode == LocationPickerMode.Address;
            _addressPicker.ShowDropDown = CurrentPickerMode == LocationPickerMode.Address;

            _pointPicker.Visible = CurrentPickerMode == LocationPickerMode.Point;
            _pointPicker.ShowDropDown = CurrentPickerMode == LocationPickerMode.Point;

            _polygonPicker.Visible = CurrentPickerMode == LocationPickerMode.Polygon;
            _polygonPicker.ShowDropDown = CurrentPickerMode == LocationPickerMode.Polygon;
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when [select location].
        /// </summary>
        public event EventHandler SelectLocation;

        #endregion

        /// <summary>
        /// Renders the <see cref="T:System.Web.UI.WebControls.TextBox" /> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> that receives the rendered output.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            base.RenderControl( writer );
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

        #region IRockControl implementation (much different than others)

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
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
                return ViewState["Required"] as bool? ?? false;
            }

            set
            {
                EnsureChildControls();
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
                EnsureChildControls();
                return ViewState["RequiredErrorMessage"] as string;
            }
            set
            {
                EnsureChildControls();
                ViewState["RequiredErrorMessage"] = value;

                _namedPicker.RequiredErrorMessage = value;
                _addressPicker.RequiredErrorMessage = value;
                _pointPicker.RequiredErrorMessage = value;
                _polygonPicker.RequiredErrorMessage = value;
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
                return _namedPicker.ValidationGroup;
            }
            set
            {
                EnsureChildControls();
                _namedPicker.ValidationGroup = value;
                _addressPicker.ValidationGroup = value;
                _pointPicker.ValidationGroup = value;
                _polygonPicker.ValidationGroup = value;
            }
        }

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
        public RequiredFieldValidator RequiredFieldValidator
        {
            get
            {
                EnsureChildControls();

                switch ( this.CurrentPickerMode )
                {
                    case LocationPickerMode.Address:
                        return _addressPicker.RequiredFieldValidator;
                    case LocationPickerMode.Point:
                        return _pointPicker.RequiredFieldValidator;
                    case LocationPickerMode.Polygon:
                        return _polygonPicker.RequiredFieldValidator;
                    case LocationPickerMode.Named:
                        return _namedPicker.RequiredFieldValidator;
                }

                return null;
            }
            set
            {
                EnsureChildControls();
                _namedPicker.RequiredFieldValidator = value;
                _addressPicker.RequiredFieldValidator = value;
                _pointPicker.RequiredFieldValidator = value;
                _polygonPicker.RequiredFieldValidator = value;
            }
        }

        #endregion

        #region IDisplayRequiredIndicator

        /// <summary>
        /// Gets or sets a value indicating whether to show the Required indicator when Required=true
        /// </summary>
        /// <value>
        /// <c>true</c> if [display required indicator]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayRequiredIndicator
        {
            get { return ViewState["DisplayRequiredIndicator"] as bool? ?? true; }
            set { ViewState["DisplayRequiredIndicator"] = value; }
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