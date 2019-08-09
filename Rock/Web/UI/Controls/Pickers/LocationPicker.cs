﻿// <copyright>
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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
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

        #region Properties

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
                // first try to determine it from ViewState 
                var currentPickerMode = ViewState["CurrentPickerMode"] as LocationPickerMode?;

                // if ViewState didn't know, try to get it from _hfCurrentPickerMode 
                if ( !currentPickerMode.HasValue )
                {
                    currentPickerMode = _hfCurrentPickerMode.Value.ConvertToEnumOrNull<LocationPickerMode>();
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
                            _namedPicker.SetValue( value );
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
            base.LoadViewState( savedState );

            var currentPickerMode = ViewState["CurrentPickerMode"] as LocationPickerMode?;
            if (currentPickerMode.HasValue)
            {
                this.CurrentPickerMode = currentPickerMode.Value;
            }

            var locationId = ViewState["LocationId"] as int?;
            if ( locationId.HasValue )
            {
                var location = new LocationService( new RockContext() ).Get( locationId.Value );
                if ( location != null )
                {
                    this.Location = location;
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
            
            var location = this.Location;
            if ( location != null )
            {
                ViewState["LocationId"] = location.Id;
            }
            else
            {
                ViewState["LocationId"] = null;
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
                return _namedPicker.Label;
            }
            set
            {
                EnsureChildControls();
                _namedPicker.Label = value;
                _addressPicker.Label = value;
                _pointPicker.Label = value;
                _polygonPicker.Label = value;
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
                EnsureChildControls();
                return _namedPicker.Help;
            }
            set
            {
                EnsureChildControls();
                _namedPicker.Help = value;
                _addressPicker.Help = value;
                _pointPicker.Help = value;
                _polygonPicker.Help = value;
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
                EnsureChildControls();
                return _namedPicker.Warning;
            }
            set
            {
                EnsureChildControls();
                _namedPicker.Warning = value;
                _addressPicker.Warning = value;
                _pointPicker.Warning = value;
                _polygonPicker.Warning = value;
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
                return _namedPicker.Required;
            }
            set
            {
                EnsureChildControls();
                _namedPicker.Required = value;
                _addressPicker.Required = value;
                _pointPicker.Required = value;
                _polygonPicker.Required = value;
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
                return _namedPicker.RequiredErrorMessage;
            }
            set
            {
                EnsureChildControls();
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
                        return _addressPicker.IsValid;
                    case LocationPickerMode.Point:
                        return _pointPicker.IsValid;
                    case LocationPickerMode.Polygon:
                        return _polygonPicker.IsValid;
                    default:
                        return _namedPicker.IsValid;
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
                return _namedPicker.HelpBlock;
            }
            set
            {
                EnsureChildControls();
                _namedPicker.HelpBlock = value;
                _addressPicker.HelpBlock = value;
                _pointPicker.HelpBlock = value;
                _polygonPicker.HelpBlock = value;
            }
        }

        /// <summary>
        /// Gets the warning block.
        /// </summary>
        /// <value>
        /// The warning block.
        /// </value>
        public WarningBlock WarningBlock
        {
            get
            {
                EnsureChildControls();
                return _namedPicker.WarningBlock;
            }
            set
            {
                EnsureChildControls();
                _namedPicker.WarningBlock = value;
                _addressPicker.WarningBlock = value;
                _pointPicker.WarningBlock = value;
                _polygonPicker.WarningBlock = value;
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
                return _namedPicker.RequiredFieldValidator;
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