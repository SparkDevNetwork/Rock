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
using System.ComponentModel;
using System.Data.Entity.Spatial;
using System.Data.SqlTypes;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Microsoft.SqlServer.Types;

using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// This control will create a Google map with drawing tools that
    /// allows the user to define a single point or a polygon which forms a geo-fence
    /// depending on the <see cref="Rock.Web.UI.Controls.GeoPicker.ManagerDrawingMode.Point"/>.
    ///
    /// To use on a page or usercontrol:
    /// <example>
    /// <code>
    ///     <![CDATA[<Rock:GeoPicker ID="gpGeoPoint" runat="server" Required="false" Label="Geo Point" DrawingMode="Point" />]]>
    /// </code>
    /// </example>
    /// To set an initial value:
    /// <example>
    /// <code>
    ///     gpGeoPoint.SetValue( DbGeography.FromText("POINT(-122.335197 47.646711)") );
    /// </code>
    /// </example>
    /// To access the value after it's been set use the <see cref="SelectedValue"/> property:
    /// <example>
    /// <code>
    ///    DbGeography point = gpGeoPoint.SelectedValue;
    /// </code>
    /// </example>
    ///
    /// If you wish to set an appropriate, initial center point you can use the <see cref="CenterPoint"/> property.
    /// </summary>
    public class GeoPicker : CompositeControl, IRockControl, IDisplayRequiredIndicator
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
            get { return ViewState["Label"] as string ?? string.Empty; }
            set { ViewState["Label"] = value; }
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
            get { return ViewState["Required"] as bool? ?? false; }
            set { ViewState["Required"] = value; }
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
        /// Gets or sets an optional validation group to use.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get { return ViewState["ValidationGroup"] as string; }
            set
            {
                EnsureChildControls();

                ViewState["ValidationGroup"] = value;
                _validator.ValidationGroup = value;
            }
        }

        /// <summary>
        /// Sets the validation message display mode for the control.
        /// </summary>
        public ValidatorDisplay ValidationDisplay
        {
            get { return ViewState["ValidationDisplay"].ToStringSafe().ConvertToEnum<ValidatorDisplay>( ValidatorDisplay.None ); }
            set { ViewState["ValidationDisplay"] = value; }
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
                EnsureChildControls();
                var isValid = ( !Required || RequiredFieldValidator == null || RequiredFieldValidator.IsValid )
                              && _validator.IsValid;
                return isValid;
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

        #region Controls

        private HiddenField _hfGeoDisplayName;
        private HiddenFieldWithClass _hfGeoPath;
        private HtmlButton _btnSelect;
        private HtmlButton _btnSelectNone;

        #endregion

        // Server-side validator.
        private CustomValidator _validator = null;

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the control should be displayed Full-Width
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable full width]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableFullWidth
        {
            get
            {
                return ViewState["EnableFullWidth"] as bool? ?? false;
            }

            set
            {
                ViewState["EnableFullWidth"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the point that map should initially be centered on
        /// </summary>
        /// <value>
        /// The center point.
        /// </value>
        public DbGeography CenterPoint
        {
            get
            {
                string centerLat = ViewState["CenterLat"] as string;
                string centerLong = ViewState["CenterLong"] as string;
                if (!string.IsNullOrWhiteSpace(centerLat) && !string.IsNullOrWhiteSpace(centerLong))
                {
                    return DbGeography.FromText( string.Format( "POINT({0} {1})", centerLong, centerLat ) );
                }
                return null;
            }

            set
            {
                string centerLat = string.Empty;
                string centerLong = string.Empty;
                if (value != null)
                {
                    centerLat = value.Latitude.HasValue ? value.Latitude.ToString() : string.Empty;
                    centerLong = value.Longitude.HasValue ? value.Longitude.ToString() : string.Empty;
                }

                ViewState["CenterLat"] = centerLat;
                ViewState["CenterLong"] = centerLong;
            }
        }

        /// <summary>
        /// Gets or sets the selected value.
        /// </summary>
        /// <value>
        /// The selected value.
        /// </value>
        public DbGeography SelectedValue
        {
            get
            {
                if ( this.DrawingMode == ManagerDrawingMode.Point )
                {
                    return GeoPoint;
                }
                else if ( this.DrawingMode == ManagerDrawingMode.Polygon )
                {
                    return GeoFence;
                }
                return null;
            }

            set
            {
                if ( this.DrawingMode == ManagerDrawingMode.Point )
                {
                    GeoPoint = value;
                }
                else if ( this.DrawingMode == ManagerDrawingMode.Polygon )
                {
                    GeoFence = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the Geography's display name.  This is what's shown
        /// to the user before they actually edit the GeoPicker to change its value.
        /// </summary>
        /// <value>
        /// The name of the geography.
        /// </value>
        public string GeoDisplayName
        {
            get
            {
                EnsureChildControls();
                if ( string.IsNullOrWhiteSpace( _hfGeoDisplayName.Value ) )
                {
                    _hfGeoDisplayName.Value = Rock.Constants.None.TextHtml;
                }

                return _hfGeoDisplayName.Value;
            }

            set
            {
                EnsureChildControls();
                _hfGeoDisplayName.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the path of the Geography.
        /// </summary>
        /// <value>
        /// The path/fence of the geography.
        /// </value>
        public DbGeography GeoFence
        {
            get
            {
                EnsureChildControls();
                if ( string.IsNullOrWhiteSpace( _hfGeoPath.Value ) )
                {
                    //_hfGeoPath.Value = Rock.Constants.None.TextHtml;
                    return null;
                }
                else
                {
                    // Now we split the lat1,long1|lat2,long2|... stored in the hidden
                    // into something that's usable by DbGeography's PolygonFromText
                    // Well Known Text (http://en.wikipedia.org/wiki/Well-known_text) representation.

                    /* 8/7/2020 - NA
                     *  I tried using this DbSpatialServices.Default.GeographyFromProviderValue( sqlGeography );
                     *  but it will use the MULTIPOLYGON format which is not compatible with our single
                     *  POLYGON implementation.
                     */
                    return DbGeography.PolygonFromText( ConvertPolyToWellKnownText( _hfGeoPath.Value ), DbGeography.DefaultCoordinateSystemId );
                }
            }

            set
            {
                EnsureChildControls();
                _hfGeoPath.Value = value != null ? ConvertPolyFromWellKnownText( value.AsText() ) : string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the point of the Geography.
        /// </summary>
        /// <value>
        /// The path/fence of the geography.
        /// </value>
        public DbGeography GeoPoint
        {
            get
            {
                EnsureChildControls();
                if ( string.IsNullOrWhiteSpace( _hfGeoPath.Value ) )
                {
                    return null;
                }
                else
                {
                    // Now split the lat1,long1 stored in the hidden into something
                    // that's usable by DbGeography's PolygonFromText Well Known Text (WKT)
                    // (http://en.wikipedia.org/wiki/Well-known_text) representation.
                    return DbGeography.FromText( ConvertPointToWellKnownText( _hfGeoPath.Value ), DbGeography.DefaultCoordinateSystemId );
                }
            }

            set
            {
                EnsureChildControls();
                _hfGeoPath.Value = value != null ? ConvertPointFromWellKnownText( value.AsText() ) : string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets a drawing mode indicating whether this <see cref="GeoPicker"/> is for points or polygons.
        /// </summary>
        /// <value>
        ///   DrawingMode.Point or DrawingMode.Polygon
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( ManagerDrawingMode.Point ),
        Description( "The drawing mode: Point, Polygon, etc." )
        ]
        public ManagerDrawingMode DrawingMode
        {
            get
            {
                object mode = this.ViewState["Mode"];
                return mode != null ? (ManagerDrawingMode)mode : ManagerDrawingMode.Point;
            }
            set
            {
                ViewState["Mode"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the drawing style to use on the map.
        /// </summary>
        /// <value>
        ///   A style guid as found in the defined values (e.g., Rock, Retro, Old Timey, etc.) for the Map Styles
        ///   defined type (<see cref="Rock.SystemGuid.DefinedType.MAP_STYLES" />).
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( Rock.SystemGuid.DefinedValue.MAP_STYLE_ROCK ),
        Description( "The style to use for the Google map." )
        ]
        public Guid MapStyleValueGuid
        {
            get
            {
                string guid = ViewState["MapStyleValueGuid"] as string;
                return ( guid == null ) ? Rock.SystemGuid.DefinedValue.MAP_STYLE_ROCK.AsGuid() : guid.AsGuid();
            }
            set { ViewState["MapStyleValueGuid"] = value.ToString(); }
        }

        /// <summary>
        /// Gets or sets the mode panel.
        /// </summary>
        /// <value>
        /// The mode panel.
        /// </value>
        public Panel ModePanel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show drop down].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show drop down]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowDropDown { get; set; }

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

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoPicker" /> class.
        /// </summary>
        public GeoPicker()
        {
            RockControlHelper.Init( this );

            RequiredFieldValidator = new HiddenFieldValidator();
            _btnSelect = new HtmlButton();
            _btnSelectNone = new HtmlButton();

            // Default validation display mode to use the ValidationSummary control rather than inline.
            ValidationDisplay = ValidatorDisplay.None;
        }

        #endregion

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            var sm = ScriptManager.GetCurrent( this.Page );
            this.RockBlock().RockPage.LoadGoogleMapsApi();

            if ( sm != null )
            {
                sm.RegisterAsyncPostBackControl( _btnSelect );
                sm.RegisterAsyncPostBackControl( _btnSelectNone );
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            // TBD TODO -- do I need this hfGeoDisplayName_???
            _hfGeoDisplayName = new HiddenField();
            _hfGeoDisplayName.ID = string.Format( "hfGeoDisplayName_{0}", this.ClientID );
            _hfGeoPath = new HiddenFieldWithClass();
            _hfGeoPath.ID = "hfGeoPath";

            if ( ModePanel != null )
            {
                this.Controls.Add( ModePanel );
            }

            _btnSelect.ClientIDMode = System.Web.UI.ClientIDMode.Static;
            _btnSelect.Attributes["role"] = "button";
            _btnSelect.Attributes["type"] = "button";
            _btnSelect.Attributes["class"] = "btn btn-xs btn-primary js-geopicker-select";
            _btnSelect.ID = string.Format( "btnSelect_{0}", this.ClientID );
            _btnSelect.InnerText = "Done";
            _btnSelect.CausesValidation = false;

            // we only need the postback on Select if SelectItem is assigned
            if ( SelectGeography != null )
            {
                _btnSelect.ServerClick += btnSelect_Click;
            }

            _btnSelectNone.ClientIDMode = ClientIDMode.Static;
            _btnSelectNone.Attributes["role"] = "button";
            _btnSelectNone.Attributes["type"] = "button";
            _btnSelectNone.Attributes["aria-label"] = "Clear selection";
            _btnSelectNone.Attributes["class"] = "btn picker-select-none";
            _btnSelectNone.ID = string.Format( "btnSelectNone_{0}", this.ClientID );
            _btnSelectNone.InnerHtml = "<i class='fa fa-times'></i>";
            _btnSelectNone.CausesValidation = false;
            _btnSelectNone.Style[HtmlTextWriterStyle.Display] = "none";

            // we only need the postback on SelectNone if SelectItem is assigned
            if ( SelectGeography != null )
            {
                _btnSelectNone.ServerClick += btnSelect_Click;
            }

            Controls.Add( _hfGeoDisplayName );
            Controls.Add( _hfGeoPath );
            Controls.Add( _btnSelect );
            Controls.Add( _btnSelectNone );

            RequiredFieldValidator.InitialValue = "0";
            RequiredFieldValidator.ControlToValidate = _hfGeoPath.ID;

            _validator = new CustomValidator();
            _validator.ID = this.ClientID + "_CV";
            _validator.Display = ValidatorDisplay.None;
            _validator.CssClass = "validation-error help-inline";
            _validator.EnableClientScript = false;
            _validator.ServerValidate += _validator_ServerValidate;
            Controls.Add( _validator );
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
            RegisterJavaScript();

            // controls div
            writer.AddAttribute( "class", "controls" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            _hfGeoDisplayName.RenderControl( writer );
            _hfGeoPath.RenderControl( writer );

            if ( this.Enabled )
            {
                writer.AddAttribute( "id", this.ClientID.ToString() );

                List<string> pickerClasses = new List<string>();
                pickerClasses.Add( "picker" );
                if ( EnableFullWidth )
                {
                    pickerClasses.Add( "picker-fullwidth" );
                }

                pickerClasses.Add( "picker-geography rollover-container" );

                writer.AddAttribute( "class", pickerClasses.AsDelimited( " " ) );

                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.Write( string.Format( @"
                    <a class='picker-label' href='#'>
                        <i class='fa fa-map-marker'></i>
                        <span id='selectedGeographyLabel_{0}'>{1}</span>", this.ClientID, this.GeoDisplayName ) );
                writer.WriteLine();

                _btnSelectNone.RenderControl( writer );

                writer.Write( @"
                        <b class='fa fa-caret-down'></b>
                    </a>" );
                writer.WriteLine();


                // picker menu
                writer.AddAttribute( "class", "picker-menu dropdown-menu" );
                if ( ShowDropDown )
                {
                    writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "block" );
                }

                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                // mode panel
                if ( ModePanel != null )
                {
                    ModePanel.RenderControl( writer );
                }

                // map
                writer.Write( @"
                    <div class='picker-search-header'><h4>Geography Picker</h4> <a class='pull-right ml-auto btn btn-link btn-minimal' title='Toggle Fullscreen' id='btnExpandToggle_{0}'><i class='fa fa-expand'></i></a></div>
                    <!-- Our custom delete button that we add to the map for deleting polygons. -->
                    <div style='display:none; z-index: 10; position: absolute; left: 200px; margin-top: 5px; line-height:0;' id='gmnoprint-delete-button_{0}'>
                        <div onmouseover=""this.style.background='WhiteSmoke';"" onmouseout=""this.style.background='white';"" style='direction: ltr; overflow: hidden; text-align: left; position: relative; color: rgb(140, 75, 75); font-family: Arial, sans-serif; font-size: 13px; background-color: rgb(255, 255, 255); padding: 4px; border-radius: 2px; -webkit-background-clip: padding-box; background-clip: padding-box; -webkit-box-shadow: rgba(0, 0, 0, 0.3) 0px 1px 4px -1px; box-shadow: rgba(0, 0, 0, 0.3) 0px 1px 4px -1px; font-weight: 500; background-position: initial initial; background-repeat: initial initial;' title='Delete selected shape'>
                            <span style='display: inline-block;'><div style='width: 16px; height: 16px; overflow: hidden; position: relative;'><i class='fa fa-times' style='font-size: 16px; padding-left: 2px; color: #aaa;'></i></div></span>
                        </div>
                    </div>
                    <!-- This is where the Google Map (with Drawing Tools) will go. -->
                    <div id='geoPicker_{0}' style='height: 300px; width: 500px' ></div>", this.ClientID );
                writer.WriteLine();

                // picker actions
                writer.AddAttribute( "class", "picker-actions" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _btnSelect.RenderControl( writer );
                writer.Write( "<a class='btn btn-link btn-xs' id='btnCancel_{0}'>Cancel</a>", this.ClientID );
                writer.WriteLine();
                writer.RenderEndTag();

                // closing div of picker-menu
                writer.RenderEndTag();

                // closing div of picker
                writer.RenderEndTag();
            }
            else
            {
                // this picker is not enabled (readonly), so just render a readonly version
                List<string> pickerClasses = new List<string>();
                pickerClasses.Add( "picker" );
                if ( EnableFullWidth )
                {
                    pickerClasses.Add( "picker-fullwidth" );
                }

                pickerClasses.Add( "picker-select" );

                writer.AddAttribute( "class", pickerClasses.AsDelimited( " " ) );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                LinkButton linkButton = new LinkButton();
                linkButton.CssClass = "picker-label";
                linkButton.Text = string.Format( "<i class='{1}'></i><span>{0}</span>", this.GeoDisplayName, "fa fa-map-marker" );
                linkButton.Enabled = false;
                linkButton.RenderControl( writer );
                writer.WriteLine();
                writer.RenderEndTag();
            }

            if ( this.RequiredFieldValidator != null )
            {
                this.RequiredFieldValidator.Display = this.ValidationDisplay;
            }

            _validator.Display = this.ValidationDisplay;
            _validator.RenderControl( writer );

            // controls div
            writer.RenderEndTag();

        }

        /// <summary>
        /// Sets the value. Necessary to preload the geo fence or geo point.
        /// </summary>
        /// <param name="dbGeography">The db geography.</param>
        public void SetValue( DbGeography dbGeography )
        {
            if ( dbGeography != null )
            {
                SelectedValue = dbGeography;
            }
            else
            {
                GeoDisplayName = Rock.Constants.None.TextHtml;
            }
        }

        /// <summary>
        /// Registers the java script.
        /// </summary>
        protected virtual void RegisterJavaScript()
        {
            string mapStyle = "null";
            string markerColor = "";

            try
            {
                DefinedValueCache dvcMapStyle = DefinedValueCache.Get( this.MapStyleValueGuid );
                if ( dvcMapStyle != null )
                {
                    mapStyle = dvcMapStyle.GetAttributeValue( "DynamicMapStyle" );
                    var colors = dvcMapStyle.GetAttributeValue( "Colors" ).Split( new char[] {'|'}, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if ( colors.Any() )
                    {
                        markerColor = colors.First().Replace( "#", "" );
                    }
                }
            }
            catch { } // oh well...

            string options = string.Format( "controlId: '{0}', drawingMode: '{1}', strokeColor: '{2}', fillColor: '{2}', mapStyle: {3}", this.ClientID, this.DrawingMode, markerColor, mapStyle );

            DbGeography centerPoint = CenterPoint;
            if ( centerPoint != null && centerPoint.Latitude != null && centerPoint.Longitude != null )
            {
                options += string.Format( ", centerLatitude: '{0}', centerLongitude: '{1}'", centerPoint.Latitude, centerPoint.Longitude );
            }
            else
            {
                // If no centerpoint was defined, try to get it from organization address
                var globalAttributes = GlobalAttributesCache.Get();
                Guid guid = globalAttributes.GetValue( "OrganizationAddress" ).AsGuid();
                if ( !guid.Equals( Guid.Empty ) )
                {
                    var location = new Rock.Model.LocationService( new Rock.Data.RockContext() ).Get( guid );
                    if (location != null && location.GeoPoint != null && location.GeoPoint.Latitude != null && location.GeoPoint.Longitude != null )
                    {
                        CenterPoint = location.GeoPoint;
                        options += string.Format( ", centerLatitude: '{0}', centerLongitude: '{1}'", location.GeoPoint.Latitude, location.GeoPoint.Longitude );
                    }
                }
            }

            string script = string.Format( @"
// if the geoPicker was rendered, initialize it
if ($('#{1}').length > 0)
{{
    Rock.controls.geoPicker.initialize({{ {0} }});
}}

", options, this.ClientID );

            ScriptManager.RegisterStartupScript( this, this.GetType(), "geo_picker-" + this.ClientID, script, true );
        }

        #region utility methods

        /// <summary>
        /// Converts single coordinate set (lat,long) into the Well Known Text (WKT) POINT format.
        /// http://en.wikipedia.org/wiki/Well-known_text
        /// </summary>
        /// <param name="latCommaLong"></param>
        /// <returns></returns>
        private string ConvertPointToWellKnownText( string latCommaLong )
        {
            string[] latlong = latCommaLong.Split( ',' );
            // NOTE: It's the lesser used Longitude and then Latitude which is why you see {1} and then {0}:
            return string.Format( "POINT({1} {0})", latlong[0], latlong[1] );
        }

        /// <summary>
        /// Convert string from "lat1,long1|lat2,long2|..." to Well Known Text (WKT)
        /// http://en.wikipedia.org/wiki/Well-known_text
        /// format "POLYGON(( long1 lat1, long2 lat2, ...))".  It is expected that the input is a single
        /// polygon (not a polygon with an inner polygon).
        /// It will also correct the orientation (clockwise-ness) of the points because DbGeography needs
        /// them to be in counter-clockwise order.
        /// </summary>
        /// <param name="latCommaLongPipe">string of "lat1,long1|lat2,long2|..."</param>
        /// <returns>A Well Known Text (WKT) POLYGON string suitable for use by DbGeography</returns>
        public static string ConvertPolyToWellKnownText( string latCommaLongPipe )
        {
            var coords = latCommaLongPipe.Split( '|' );
            var convertedCoords = new List<string>();
            PointF[] polygon = new PointF[coords.Length];
            string[] latlong;

            for ( int i = 0; i < coords.Length; i++ )
            {
                latlong = coords[i].Split( ',' );
                // lovely -- please do a double take before you think you should change this:
                convertedCoords.Add( string.Format( "{1} {0}", latlong[0], latlong[1] ) );

                // Now add it to the polygon array so we can determine if
                // the coordinates are clockwise or counterclockwise.
                polygon[i] = new PointF( float.Parse( latlong[0] ), float.Parse( latlong[1]) );
            }

            if ( IsClockwisePolygon( polygon ) )
            {
                convertedCoords.Reverse();
            }

            return string.Format( "POLYGON(({0}))", string.Join( ", ", convertedCoords ) );
        }

        /// <summary>
        /// Convert from WKT format:
        /// "POINT (long1 lat1)" to "lat1,long1"
        /// </summary>
        /// <param name="wkt">a POINT in Well Known Text format</param>
        /// <returns></returns>
        private string ConvertPointFromWellKnownText( string wkt )
        {
            string match = @"POINT \(([0-9\.\-\,]+) ([0-9\.\-\,]+)\)";

            if ( !Regex.IsMatch( wkt, match, RegexOptions.IgnoreCase ) )
            {
                throw new ArgumentException( "Expected 'POINT (long1 lat1)'", "wkt" );
            }

            string lng = Regex.Replace( wkt, match, "$1", RegexOptions.IgnoreCase );
            string lat = Regex.Replace( wkt, match, "$2", RegexOptions.IgnoreCase );

            return string.Format("{0},{1}", lat, lng);
        }

        /// <summary>
        /// Convert from WKT format:
        /// "POLYGON ((long1 lat1, long2 lat2, ...))" to "lat1,long1|lat2,long2|..."
        /// </summary>
        /// <param name="wkt">a POLYGON in Well Known Text format</param>
        /// <returns>string suitable for Google Maps polygon geoPicker "lat1,long1|lat2,long2|..."</returns>
        private string ConvertPolyFromWellKnownText( string wkt )
        {
            string match = @"POLYGON \(\(([0-9\.\-\,\s]+)\)\)";

            if ( ! Regex.IsMatch( wkt, match, RegexOptions.IgnoreCase ) )
            {
                throw new ArgumentException( "Expected 'POLYGON ((long1 lat1, long2 lat2, ...))'", "wkt" );
            }

            string longSpaceLatComma = Regex.Replace( wkt, match, "$1", RegexOptions.IgnoreCase );
            string[] longSpaceLat = longSpaceLatComma.Split( ',' );
            var convertedCoords = new List<string>();
            string[] longLat;
            for ( int i = 0; i < longSpaceLat.Length; i++ )
            {
                longLat = longSpaceLat[i].Trim().Split( ' ' );
                // again -- please do a double take before you think you should change this:
                convertedCoords.Add( string.Format( "{1},{0}", longLat[0], longLat[1] ) );
            }

            return string.Join( "|", convertedCoords );
        }

        /// <summary>
        /// Attempt to determine if the polygon is clockwise or counter-clockwise.
        /// Thank you dominoc!
        /// http://dominoc925.blogspot.com/2012/03/c-code-to-determine-if-polygon-vertices.html
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static bool IsClockwisePolygon( PointF[] polygon )
        {
            bool isClockwise = false;
            double sum = 0;
            for ( int i = 0; i < polygon.Length - 1; i++ )
            {
                sum += ( Math.Abs(polygon[i + 1].X) - Math.Abs(polygon[i].X) ) * ( Math.Abs(polygon[i + 1].Y) + Math.Abs(polygon[i].Y) );
            }
            isClockwise = ( sum > 0 ) ? true : false;
            return isClockwise;
        }

        /// <summary>
        /// Determines whether [is geo fence valid] [the specified error message].
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if [is geo fence valid] [the specified error message]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsGeoFenceValid( out string errorMessage )
        {
            EnsureChildControls();
            if ( string.IsNullOrWhiteSpace( _hfGeoPath.Value ) )
            {
                errorMessage = string.Empty;
                return true;
            }

            try
            {
                /*
                 * 8/7/2020 - NA
                 * We use the SqlGeography as an intermediate conversion because it has
                 * methods to determine if the fence is a single polygon (has 1 geometry).
                 * If that passes, then we can consider the geo-fence valid.
                 *
                 * Reason: Some administrators were creating incompatible shapes using the
                 * GeoPicker that will otherwise break the check-in geo-location kiosk matching.
                 */
                var polygonText = ConvertPolyToWellKnownText( _hfGeoPath.Value );
                var sqlGeography = SqlGeography.STGeomFromText( new SqlChars( polygonText ), DbGeography.DefaultCoordinateSystemId ).MakeValid();

                if ( sqlGeography == null || ! sqlGeography.STIsValid() )
                {
                    errorMessage = "The selected geo-fence path is invalid.";
                    return false;
                }

                if ( sqlGeography.STNumGeometries() > 1 )
                {
                    errorMessage=  "The geo-fence has overlapping lines or is made up of multiple polygons. Only one polygon is allowed.";
                    return false;
                }

                var dbGeography = DbGeography.PolygonFromText( polygonText, DbGeography.DefaultCoordinateSystemId );
                if ( dbGeography == null )
                {
                    errorMessage = "Unable to convert the given geo-fence path to a compatible format.";
                    return false;
                }
            }
            catch
            {
                errorMessage = "Unable to convert the given geo-fence path to a compatible format.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnSelect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void btnSelect_Click( object sender, EventArgs e )
        {
            if ( SelectGeography != null )
            {
                SelectGeography( sender, e );
            }
        }

        /// <summary>
        /// Gets or sets the select dbGeography.
        /// </summary>
        /// <value>
        /// The select dbGeography.
        /// </value>
        public event EventHandler SelectGeography;

        /// <summary>
        /// Handles the ServerValidate event for the custom validator.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        private void _validator_ServerValidate( object source, ServerValidateEventArgs args )
        {
            string validationMessage = null;
            bool isValid = false;

            var controlName = this.Label ?? "GeoPicker";

            // Check if a required value is missing.
            if ( this.Required
                 && string.IsNullOrWhiteSpace( _hfGeoPath.Value ) )
            {
                validationMessage = $"{controlName} is required.";
            }
            else
            {
                // Validate the selection.
                if ( this.DrawingMode == ManagerDrawingMode.Polygon )
                {
                    // Verify that a valid geofence has been selected.
                    isValid = IsGeoFenceValid( out validationMessage );
                    if ( !isValid )
                    {
                        validationMessage = $"{controlName} is invalid. {validationMessage}";
                    }
                }
                else
                {
                    // Try to create a DbGeography point from the control settings.
                    // This will fail if the settings don't represent a valid map location.
                    // There is no way to create an invalid point on the map selector,
                    // so this check only exists to prevent invalid values from being injected by any other means.
                    DbGeography gp;
                    try
                    {
                        gp = this.GeoPoint;
                        isValid = true;
                    }
                    catch ( TargetInvocationException ex )
                    {
                        validationMessage = $"{controlName} value is invalid. { ex?.InnerException?.Message }";
                    }
                    catch ( Exception ex )
                    {
                        validationMessage = $"{controlName} value is invalid. { ex.Message }";
                    }
                }
            }

            // Set the error message for both in-line display and validation summary.
            _validator.Text = null;
            _validator.ErrorMessage = validationMessage;

            args.IsValid = isValid;
        }

        #endregion

        #region Enums

        /// <summary>
        /// Which type of selection to enable
        /// </summary>
        public enum ManagerDrawingMode
        {
            /// <summary>
            /// point
            /// </summary>
            Point = 0,

            /// <summary>
            /// polygon
            /// </summary>
            Polygon = 1
        };

        #endregion
    }
}