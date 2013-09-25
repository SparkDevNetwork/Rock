//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Spatial;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// This control will create a Google map with drawring tools that
    /// allows the user to define a single point or a polygon which forms a geo-fence
    /// depending on the <see cref="Rock.Web.UI.Controls.GeoPicker.ManagerDrawingMode.Point"/>.
    /// 
    /// To use on a page or usercontrol:
    /// <example>
    /// <code>
    ///     <Rock:GeoPicker ID="gpGeoPoint" runat="server" Required="false" Label="Geo Point" DrawingMode="Point" />
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
    public class GeoPicker : CompositeControl, ILabeledControl
    {
        private Label _label;
        private HiddenField _hfGeoDisplayName;
        private HiddenField _hfGeoPath;
        private LinkButton _btnSelect;
        private LinkButton _btnSelectNone;
        private DbGeography _geoFence;
        private DbGeography _geoPoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoPicker" /> class.
        /// </summary>
        public GeoPicker()
        {
            _label = new Label();
            _btnSelect = new LinkButton();
            _btnSelectNone = new LinkButton();
        }

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        public string Label
        {
            get { return _label.Text; }
            set { _label.Text = value; }
        }

        /// <summary>
        /// Gets or sets the name of the field to display in validation messages
        /// when a Label is not entered
        /// </summary>
        /// <value>
        /// The name of the field.
        /// </value>
        public string FieldName
        {
            get { return _label.Text; }
            set { _label.Text = value; }
        }

        /// <summary>
        /// The required validator
        /// </summary>
        protected HiddenFieldValidator RequiredValidator;

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
        /// Sets the value. Necessary to preload the geo fence or geo point.
        /// </summary>
        /// <param name="person">The dbGeography to plot/edit.</param>
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
                    _geoFence = DbGeography.PolygonFromText( ConvertPolyToWellKnownText( _hfGeoPath.Value ), 4326 );
                }

                return _geoFence;
            }

            set
            {
                EnsureChildControls();
                if ( value == null )
                    return;
                _geoFence = value;
                _hfGeoPath.Value = ConvertPolyFromWellKnownText( _geoFence.AsText() );
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
                    _geoPoint = DbGeography.FromText( ConvertPointToWellKnownText( _hfGeoPath.Value ), 4326 );
                }

                return _geoPoint;
            }

            set
            {
                EnsureChildControls();
                if ( value == null )
                    return;
                _geoPoint = value;
                _hfGeoPath.Value = ConvertPointFromWellKnownText( _geoPoint.AsText() );
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
        /// Gets or sets a value indicating whether this <see cref="GeoPicker"/> is required.
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
                if ( ViewState["Required"] != null )
                    return (bool)ViewState["Required"];
                else
                    return false;
            }
            set
            {
                ViewState["Required"] = value;
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
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            var sm = ScriptManager.GetCurrent( this.Page );

            if ( sm != null )
            {
                sm.RegisterAsyncPostBackControl( _btnSelect );
                sm.RegisterAsyncPostBackControl( _btnSelectNone );
                var googleAPIKey = GlobalAttributesCache.Read().GetValue( "GoogleAPIKey" );
                sm.Scripts.Add( new ScriptReference( string.Format( "https://maps.googleapis.com/maps/api/js?key={0}&sensor=false&libraries=drawing", googleAPIKey )  ) );
            }
        }

        /// <summary>
        /// Registers the java script.
        /// </summary>
        protected virtual void RegisterJavaScript()
        {
            string options = string.Format( "controlId: '{0}', drawingMode: '{1}'", this.ClientID, this.DrawingMode );

            DbGeography centerPoint = CenterPoint;
            if ( centerPoint != null && centerPoint.Latitude != null && centerPoint.Longitude != null )
            {
                options += string.Format( ", centerLatitude: '{0}', centerLongitude: '{1}'", centerPoint.Latitude, centerPoint.Longitude );
            }

            string script = string.Format( "Rock.controls.geoPicker.initialize({{ {0} }});", options );
            
            ScriptManager.RegisterStartupScript( this, this.GetType(), "geo_picker-" + this.ClientID, script, true );
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
                return !Required || RequiredValidator.IsValid;
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();

            // TBD TODO -- do I need this hfGeoDisplayName_???
            _hfGeoDisplayName = new HiddenField();
            _hfGeoDisplayName.ClientIDMode = System.Web.UI.ClientIDMode.Static;
            _hfGeoDisplayName.ID = string.Format( "hfGeoDisplayName_{0}", this.ClientID );
            _hfGeoPath = new HiddenField();
            _hfGeoPath.ClientIDMode = System.Web.UI.ClientIDMode.Static;
            _hfGeoPath.ID = string.Format( "hfGeoPath_{0}", this.ClientID );

            _btnSelect.ClientIDMode = System.Web.UI.ClientIDMode.Static;
            _btnSelect.CssClass = "btn btn-xs btn-primary";
            _btnSelect.ID = string.Format( "btnSelect_{0}", this.ClientID );
            _btnSelect.Text = "Done";
            _btnSelect.CausesValidation = false;
            _btnSelect.Click += btnSelect_Click;

            _btnSelectNone.ClientIDMode = ClientIDMode.Static;
            _btnSelectNone.CssClass = "picker-select-none";
            _btnSelectNone.ID = string.Format( "btnSelectNone_{0}", this.ClientID );
            _btnSelectNone.Text = "<i class='icon-remove'></i>";
            _btnSelectNone.CausesValidation = false;
            _btnSelectNone.Style[HtmlTextWriterStyle.Display] = "none";
            _btnSelectNone.Click += btnSelect_Click;

            Controls.Add( _label );
            Controls.Add( _hfGeoDisplayName );
            Controls.Add( _hfGeoPath );
            Controls.Add( _btnSelect );
            Controls.Add( _btnSelectNone );

            RequiredValidator = new HiddenFieldValidator();
            RequiredValidator.ID = this.ClientID + "_rfv";
            RequiredValidator.InitialValue = "0";
            RequiredValidator.ControlToValidate = _hfGeoPath.ID;
            RequiredValidator.Display = ValidatorDisplay.Dynamic;
            RequiredValidator.CssClass = "validation-error help-inline";
            RequiredValidator.Enabled = false;

            Controls.Add( RequiredValidator );
        }

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
        /// Renders the <see cref="T:System.Web.UI.WebControls.TextBox" /> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> that receives the rendered output.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            RegisterJavaScript();

            bool renderLabel = !string.IsNullOrEmpty( Label );

            if ( renderLabel )
            {
                writer.AddAttribute( "class", "control-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                _label.AddCssClass( "control-label" );

                _label.RenderControl( writer );

                writer.AddAttribute( "class", "controls" );

                writer.RenderBeginTag( HtmlTextWriterTag.Div );
            }

            if ( Required )
            {
                RequiredValidator.Enabled = true;
                RequiredValidator.ErrorMessage = Label + " is Required.";
                RequiredValidator.RenderControl( writer );
            }

            _hfGeoDisplayName.RenderControl( writer );
            _hfGeoPath.RenderControl( writer );

            if ( this.Enabled )
            {
                string controlHtmlFormatStart = @"
        <div class='picker picker-select' id='{0}'> 
            <a class='picker-label' href='#'>
                <i class='icon-map-marker'></i>
                <span id='selectedGeographyLabel_{0}'>{1}</span>
                <b class='caret'></b>
            </a>
";

                writer.Write( string.Format( controlHtmlFormatStart, this.ClientID, this.GeoDisplayName ) );

                // if there is a PostBack registered, create a real LinkButton, otherwise just spit out HTML (to prevent the autopostback)
                if ( SelectGeography != null )
                {
                    _btnSelectNone.RenderControl( writer );
                }
                else
                {
                    writer.Write( "<a class='picker-select-none' id='btnSelectNone_{0}' href='#' style='display:none'><i class='icon-remove'></i></a>", this.ClientID );
                }

                string controlHtmlFormatMiddle = @"
            <div class='picker-menu dropdown-menu picker-geography' style='Width: 500px;'>
                <h4>Geography Picker</h4>
                <!-- Our custom delete button that we add to the map for deleting polygons. -->
                <div style='display: none; z-index: 10; position: absolute; left: 105px; top: 0px; line-height:0;' id='gmnoprint-delete-button_{0}'>
                    <div style='direction: ltr; overflow: hidden; text-align: left; position: relative; color: rgb(51, 51, 51); font-family: Arial, sans-serif; font-size: 13px; background-color: rgb(255, 255, 255); padding: 4px; border-width: 1px 1px 1px 1px; border-style: solid; border-color: rgb(113, 123, 135); -webkit-box-shadow: rgba(0, 0, 0, 0.4) 0px 2px 4px; box-shadow: rgba(0, 0, 0, 0.4) 0px 2px 4px; font-weight: normal; background-position: initial initial; background-repeat: initial initial;' title='Delete selected shape'>
                        <span style='display: inline-block;'><div style='width: 16px; height: 16px; overflow: hidden; position: relative;'><i class='icon-remove' style='font-size: 16px; padding-left: 2px; color: #aaa;'></i></div></span>
                    </div>
                </div>
                <!-- This is where the Google Map (with Drawing Tools) will go. -->
                <div id='geoPicker_{0}' style='height: 300px; width: 500px' /></div>
                <hr />
";

                writer.Write( controlHtmlFormatMiddle, this.ClientID, this.GeoDisplayName );

                // if there is a PostBack registered, create a real LinkButton, otherwise just spit out HTML (to prevent the autopostback)
                if ( SelectGeography != null )
                {
                    _btnSelect.RenderControl( writer );
                }
                else
                {
                    writer.Write( string.Format( "<a class='btn btn-xs btn-primary' id='btnSelect_{0}'>Done</a>", this.ClientID ) );
                }

                string controlHtmlFormatEnd = @"
              <a class='btn btn-xs' id='btnCancel_{0}'>Cancel</a>
          </div>
      </div> 
";

                writer.Write( string.Format( controlHtmlFormatEnd, this.ClientID, this.GeoDisplayName ) );
            }
            else
            {
                string controlHtmlFormatDisabled = @"
        <i class='icon-file-alt'></i>
        <span id='selectedItemLabel_{0}'>{1}</span>
";
                writer.Write( controlHtmlFormatDisabled, this.ClientID, this.GeoDisplayName );
            }

            if ( renderLabel )
            {
                writer.RenderEndTag();

                writer.RenderEndTag();
            }
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
        private string ConvertPolyToWellKnownText( string latCommaLongPipe )
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
        private bool IsClockwisePolygon( PointF[] polygon )
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

        #endregion

        public enum ManagerDrawingMode
        {
            Point = 0,
            Polygon = 1
        };
    }
}