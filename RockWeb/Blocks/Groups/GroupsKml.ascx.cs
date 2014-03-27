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
using System.Text;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
using System.Data.Entity.Spatial;
using System.Collections.Generic;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using SharpKml.Engine;
using SharpKml.Base;
using SharpKml.Dom;

namespace RockWeb.Blocks.Groups
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Groups KML" )]
    [Category( "Groups" )]
    [Description( "Block that allows the import and export of group geographies to a KML/KMZ format." )]
    public partial class GroupsKml : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables
        enum ExportType
        {
            Point,
            GeoFence
        };

        int colorIndex = 0;
        
        Color32[] MapColors =   new Color32[] { 
                                    new Color32( 150, 199, 211, 141 ),
                                    new Color32( 150, 179, 255, 255 ),
                                    new Color32( 150, 218, 186, 190 ),
                                    new Color32( 150, 114, 128, 251 ),
                                    new Color32( 150, 211, 177, 128 ),
                                    new Color32( 150, 98, 180, 253 ),
                                    new Color32( 150, 105, 222, 179 ),
                                    new Color32( 150, 229, 205, 252 ),
                                    new Color32( 150, 217, 217, 217 ),
                                    new Color32( 150, 189, 128, 188 ),
                                    new Color32( 150, 197, 235, 204 ),
                                    new Color32( 150, 111, 237, 255 )
                                };

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods
        
        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                //gtpExportGroupType.GroupTypes =  new GroupTypeService().Queryable().ToList();
                ddlExportGroupTypes.DataValueField = "Id";
                ddlExportGroupTypes.DataTextField = "Name";
                ddlExportGroupTypes.DataSource = new GroupTypeService().Queryable().Select( g => new { g.Id, g.Name } ).ToList();
                ddlExportGroupTypes.DataBind();
                ddlExportGroupTypes.Items.Insert( 0, "" );

                ddlExportGroupLocationType.DataValueField = "Id";
                ddlExportGroupLocationType.DataTextField = "Name";

                lTitle.Text = ("Groups KML Importer and Exporter").FormatAsHtmlTitle();
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        protected void btnPillExport_Click( object sender, EventArgs e )
        {
            liPillExport.RemoveCssClass( "active" );
            liPillExport.AddCssClass( "active" );
            liPillImport.RemoveCssClass( "active" );
            pnlExport.Visible = true;
            pnlImport.Visible = false;
        }
        protected void btnPillImport_Click( object sender, EventArgs e )
        {
            liPillImport.RemoveCssClass( "active" );
            liPillImport.AddCssClass( "active" );
            liPillExport.RemoveCssClass( "active" );
            pnlExport.Visible = false;
            pnlImport.Visible = true;
        }

        protected void ddlExportGroupTypes_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( ddlExportGroupTypes.SelectedValue != string.Empty )
            {
                int selectedGroupTypeId = Int32.Parse( ddlExportGroupTypes.SelectedValue );

                GroupTypeService groupTypeService = new GroupTypeService();
                var selectedGroupType = groupTypeService.Get( selectedGroupTypeId );

                ddlExportGroupLocationType.DataSource = selectedGroupType.LocationTypes.Select( l => new { Id = l.LocationTypeValueId, Name = l.LocationTypeValue.Name } ).ToList();
                ddlExportGroupLocationType.DataBind();
                ddlExportGroupLocationType.Enabled = true;

                if ( ddlExportGroupLocationType.Items.Count > 0 )
                {
                    btnExport.Enabled = true;
                }
                else
                {
                    btnExport.Enabled = false;
                }
            }
            else
            {
                ddlExportGroupLocationType.Items.Clear();
                ddlExportGroupLocationType.Enabled = false;
                btnExport.Enabled = false;
            }
        }

        protected void ddlExportGroupLocationType_SelectedIndexChanged( object sender, EventArgs e )
        {

        }

        protected void fuprImportFile_FileUploaded( object sender, EventArgs e )
        {
            string physicalFileName = this.Request.MapPath( fuprImportFile.UploadedContentFilePath );

            try
            {

                bool importSuccess = true;
                StringBuilder messages = new StringBuilder();

                Guid groupLocationTypeGuid;

                KmlFile file = null;

                if ( physicalFileName.EndsWith( ".kmz" ) )
                {
                    KmzFile kmz = KmzFile.Open( physicalFileName );
                    file = kmz.GetDefaultKmlFile();
                }
                else
                {
                    StreamReader reader = new StreamReader( physicalFileName );
                    file = KmlFile.Load( reader );
                    reader.Close();
                }

                Kml kml = file.Root as Kml;

                // get the location type from the file
                if ( !Guid.TryParse( kml.Feature.Address, out groupLocationTypeGuid ) )
                {
                    importSuccess = false;
                    messages.Append( "<li>Could not determine the location type for this import. Please ensure that the KML file was generated by an export from Rock.</li>" );
                }


                if ( kml != null && importSuccess )
                {
                    DefinedValueCache groupLocationTypeValue = DefinedValueCache.Read( groupLocationTypeGuid );

                    foreach ( var placemark in kml.Flatten().OfType<Placemark>() )
                    {
                        Guid groupGuid;
                        bool placemarkSuccess = true;

                        // get the group guid
                        if ( !Guid.TryParse( placemark.Id, out groupGuid ) )
                        {
                            placemarkSuccess = false;
                            messages.Append( string.Format( "<li>The group {0} does not exist in the database. Please ensure this KML file was generated from an export from Rock.</li>", placemark.Name ) );
                        }
                        else
                        {
                            using ( new Rock.Data.UnitOfWorkScope() )
                            {
                                // we have a group guid
                                GroupService groupService = new GroupService();
                                var group = groupService.Get( groupGuid );

                                if ( group == null )
                                {
                                    messages.Append( string.Format( "<li>The group {0} was not found in the database. Either this file was not exported from the same database or this group has been deleted since the export.</li>", placemark.Name ) );
                                }
                                else
                                {
                                    group.Name = placemark.Name; // update name

                                    // get location
                                    if ( placemark.Geometry != null )
                                    {
                                        Guid locationGuid;
                                        GroupLocation groupLocation;

                                        if ( !Guid.TryParse( placemark.Geometry.Id, out locationGuid ) )
                                        {
                                            // groups does not have a location so add it
                                            groupLocation = new GroupLocation();
                                            groupLocation.Location = new Rock.Model.Location();
                                            groupLocation.Location.LocationTypeValueId = groupLocationTypeValue.Id;
                                            group.GroupLocations.Add( groupLocation );
                                        }
                                        else
                                        {
                                            groupLocation = (GroupLocation)group.GroupLocations.Where( l => l.Location.Guid == locationGuid ).FirstOrDefault();
                                        }


                                        if ( placemark.Geometry.GetType() == typeof( SharpKml.Dom.Polygon ) )
                                        {
                                            Polygon polygon = (SharpKml.Dom.Polygon)placemark.Geometry;
                                            if ( polygon.OuterBoundary.LinearRing.Coordinates.Count() > 0 )
                                            {
                                                var polygonCoordinates = new List<string>();
                                                foreach ( var point in polygon.OuterBoundary.LinearRing.Coordinates )
                                                {
                                                    polygonCoordinates.Add( String.Format( "{0} {1}", point.Longitude, point.Latitude ) );
                                                }

                                                DbGeography geofence = DbGeography.PolygonFromText( String.Format( "POLYGON(({0}))", string.Join( ", ", polygonCoordinates ) ), 4326 );
                                                groupLocation.Location.GeoFence = geofence;
                                            }
                                        }

                                        if ( placemark.Geometry.GetType() == typeof( SharpKml.Dom.Point ) )
                                        {
                                            Point point = (SharpKml.Dom.Point)placemark.Geometry;
                                            groupLocation.Location.GeoPoint = DbGeography.FromText( string.Format( "POINT({0} {1})", point.Coordinate.Longitude, point.Coordinate.Latitude ) ); ;
                                        }
                                    }

                                    groupService.Save( group, CurrentPersonAlias );
                                }
                            }
                        }
                    }
                }

                if ( importSuccess )
                {

                    if ( messages.Length == 0 )
                    {
                        lMessages.Text = "<div class='alert alert-success'><strong>Import Success</strong> The file provided has been imported with no errors or warnings.</div>";
                    }
                    else
                    {
                        lMessages.Text = "<div class='alert alert-warning'><strong>Import Successful With Warnings</strong> The import was successful with the following warnings: <ul>" + messages.ToString() + "</ul></div>";
                    }
                }
                else
                {
                    lMessages.Text = "<div class='alert alert-danger'><strong>File Could Not Be Imported</strong> The file provided could not be imported due to the following:<ul>" + messages.ToString() + "</li></div>";
                }

                // clean up
                kml = null;
                file = null;
            }
            catch ( Exception ex )
            {
                lMessages.Text = "<div class='alert alert-danger'><strong>Error On Import</strong> " + ex.Message + "</div>";
            }
            finally
            {
                File.Delete( physicalFileName );
            }
        }

        protected void btnExport_Click( object sender, EventArgs e )
        {
            int groupTypeId = Int32.Parse(ddlExportGroupTypes.SelectedValue);
            int locationTypeValueId = Int32.Parse(ddlExportGroupLocationType.SelectedValue);
            DefinedValueCache locationTypeValue = DefinedValueCache.Read( locationTypeValueId );
            bool exportAsKmz = (rblExportFileType.SelectedValue == "kmz");

            bool exportPoints = rblExportGeoTypes.Items.Cast<ListItem>().Where(i => i.Value == "points").Select(i => i.Selected).FirstOrDefault();
            bool exportGeofences = rblExportGeoTypes.Items.Cast<ListItem>().Where( i => i.Value == "geofences" ).Select( i => i.Selected ).FirstOrDefault();

            GroupService groupService = new GroupService();
            var groups = groupService.Queryable()
                            .Where( g => g.GroupTypeId == groupTypeId )
                            .Select( g => new { 
                                g.Id,
                                g.Guid,
                                g.Name,
                                g.Description,
                                GroupLocation = g.GroupLocations.Where( l => l.GroupLocationTypeValueId == locationTypeValueId ).FirstOrDefault()
                            } ).ToList();

            // create kml document
            Document document = new Document();
            document.Address = locationTypeValue.Guid.ToString();
            document.Name = String.Format("{0} ({1})", ddlExportGroupTypes.SelectedItem.Text, ddlExportGroupLocationType.SelectedItem.Text);
            document.Open = false;

            // add icon style
            SharpKml.Dom.Style stylePoint = new SharpKml.Dom.Style();
            stylePoint.Id = "style1";
            stylePoint.Icon = new IconStyle();
            stylePoint.Icon.Icon = new IconStyle.IconLink( new Uri( "http://maps.google.com/mapfiles/kml/paddle/red-circle.png" ) );
            stylePoint.Icon.Scale = 1.0;
            document.AddStyle( stylePoint );

            foreach ( var group in groups )
            {
                Placemark placemark = new Placemark();
                placemark.Name = group.Name;
                placemark.Id = group.Guid.ToString();

                Description description = new Description();
                description.Text = group.Description;
                placemark.Description = description;

                if ( group.GroupLocation != null )
                {
                    // note currently group locations cannot have both a GeoPoint and Geofence. If that changes then this will need to change too.
                    if ( exportPoints && group.GroupLocation.Location.GeoPoint != null )
                    {
                        Point point = new Point();
                        point.Id = group.GroupLocation.Location.Guid.ToString();
                        point.Coordinate = new Vector( (double)group.GroupLocation.Location.GeoPoint.Latitude, (double)group.GroupLocation.Location.GeoPoint.Longitude );
                        placemark.Geometry = point;
                        placemark.StyleUrl = new Uri( "#style1", UriKind.Relative );
                    }
                    
                    if ( exportGeofences && group.GroupLocation.Location.GeoFence != null )
                    {

                        //group.GroupLocation.Location.GeoFence.
                        OuterBoundary outerBoundary = new OuterBoundary();
                        outerBoundary.LinearRing = new LinearRing();
                        outerBoundary.LinearRing.Coordinates = new CoordinateCollection();

                        // get points
                        string groupFence = group.GroupLocation.Location.GeoFence.ProviderValue.ToString();
                        string match = @"POLYGON \(\(([0-9\.\-\,\s]+)\)\)";
                        string longSpaceLatComma = Regex.Replace( groupFence, match, "$1", RegexOptions.IgnoreCase );
                        string[] points = longSpaceLatComma.Split( ',' );

                        foreach ( string point in points )
                        {
                            string[] longLat;
                            longLat = point.Trim().Split( ' ' );
                            outerBoundary.LinearRing.Coordinates.Add( new Vector( Convert.ToDouble( longLat[1] ), Convert.ToDouble( longLat[0] ), 0 ) );
                        }

                        Polygon polygon = new Polygon();
                        polygon.Id = group.GroupLocation.Location.Guid.ToString();
                        polygon.OuterBoundary = outerBoundary;

                        // set the color of the polygon
                        var style = new SharpKml.Dom.Style();

                        style.Polygon = new PolygonStyle();
                        style.Polygon.ColorMode = SharpKml.Dom.ColorMode.Normal;
                        style.Polygon.Color = GetColor();

                        placemark.Geometry = polygon;
                        placemark.AddStyle( style );
                    }
                }
                else
                {
                    // adding blank points doesn't work too well in Google Earth 
                    /*if ( exportPoints )
                    {
                        placemark.StyleUrl = new Uri( "#style1", UriKind.Relative );
                    }*/

                    // add blank polygons to file 
                    if ( exportGeofences )
                    {
                        Polygon polygon = new Polygon();

                        // set the color of the polygon
                        var style = new SharpKml.Dom.Style();
                        style.Polygon = new PolygonStyle();
                        style.Polygon.ColorMode = SharpKml.Dom.ColorMode.Normal;
                        style.Polygon.Color = GetColor();

                        placemark.Geometry = polygon;
                        placemark.AddStyle( style );
                    }
                }

                document.AddFeature( placemark );
            }

            Kml root = new Kml();
            root.Feature = document;
            KmlFile kmlFile = KmlFile.Create(root, false);

            if ( exportAsKmz == false )
            {
                System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
                response.ClearHeaders();
                response.ClearContent();
                response.Clear();
                response.ContentType = "application/vnd.google-earth.kml+xml";
                response.AddHeader( "Content-Disposition", "attachment; filename=\"rock-groups.kml\"" ); 
                response.Charset = "";

                var stream = new MemoryStream();
                kmlFile.Save( stream );
                response.BinaryWrite( stream.ToArray() );

                response.Flush();
                response.End();
            }
            else
            {
                System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
                response.ClearHeaders();
                response.ClearContent();
                response.Clear();
                response.ContentType = "application/vnd.google-earth.kmz";
                response.AddHeader( "Content-Disposition", "attachment; filename=\"rock-groups.kmz\"" ); 
                response.Charset = "";

                var stream = new MemoryStream();
                KmzFile kmzFile = KmzFile.Create( kmlFile );
                kmzFile.Save( stream );
                response.BinaryWrite( stream.ToArray() );

                response.Flush();
                response.End();
            }
            
            
        }

        #endregion

        #region Methods

        private Color32 GetColor(  )
        {
            if ( colorIndex == MapColors.Length )
            {
                colorIndex = 0;
            }

            Color32 returnColor = MapColors[colorIndex];
            colorIndex++;

            return returnColor;
        }

       

        #endregion 

        
}
}