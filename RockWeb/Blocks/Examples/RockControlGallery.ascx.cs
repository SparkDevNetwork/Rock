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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Examples
{
    /// <summary>
    /// A sample block that uses many of the Rock UI controls.
    /// </summary>
    [DisplayName( "Rock Control Gallery" )]
    [Category( "Examples" )]
    [Description( "Allows you to see and try various Rock UI controls." )]

    [DefinedValueField( Rock.SystemGuid.DefinedType.MAP_STYLES, "Map Style", "The map theme that should be used for styling the GeoPicker map.", true, false, Rock.SystemGuid.DefinedValue.MAP_STYLE_ROCK )]
    public partial class RockControlGallery : RockBlock
    {
        private Regex specialCharsRegex = new Regex( "[^a-zA-Z0-9-]" );

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            InitSyntaxHighlighting();

            gExample.DataKeyNames = new string[] { "Id" };
            gExample.GridRebind += gExample_GridRebind;

            geopExamplePoint.SelectGeography += geoPicker_SelectGeography;
            geopExamplePolygon.SelectGeography += geoPicker1_SelectGeography;
            geopExamplePoint.MapStyleValueGuid = GetAttributeValue( "MapStyle" ).AsGuid();

            htmlEditorLight.MergeFields.Add( "GlobalAttribute" );
            htmlEditorLight.MergeFields.Add( "Rock.Model.Person" );

            mfpExample.MergeFields.Add( "GlobalAttribute,Rock.Model.Person" );

            List<string> list = ReadExamples();
            int i = -1;
            foreach ( var example in pnlDetails.ControlsOfTypeRecursive<HtmlControl>() )
            {
                if ( example.Attributes["class"] == "r-example" )
                {
                    i++;
                    example.Controls.Add( new LiteralControl( string.Format( "<pre class='prettyprint'>{0}</pre>", Server.HtmlEncode( list[i] ) ) ) );
                }

                if ( example.NamingContainer == this && (example.TagName == "h1" || example.TagName == "h2" || example.TagName == "h3") )
                {
                    example.Attributes["class"] = "rollover-container";
                    example.Controls.AddAt( 0, new LiteralControl( string.Format( "<a name='{0}' class='anchor rollover-item' href='#{0}'><i class='fa fa-link rlink icon-link'></i></a>", BuildAnchorForHref( (HtmlGenericControl)example ) ) ) );
                }
            }
        }

        private string BuildAnchorForHref( HtmlGenericControl item )
        {
            return specialCharsRegex.Replace( item.InnerText, "-" ).ToLower();
        }

        /// <summary>
        /// Initialize stuff required for syntax highlighting.
        /// </summary>
        private void InitSyntaxHighlighting()
        {
            RockPage.AddCSSLink( ResolveUrl( "~/Blocks/Examples/prettify.css" ) );
            RockPage.AddScriptLink( "//cdnjs.cloudflare.com/ajax/libs/prettify/r298/prettify.js", false );
        }

        /// <summary>
        /// Reads this block to find embedded examples and returns them in a indexed list.
        /// </summary>
        /// <returns>code examples by postion index</returns>
        private List<string> ReadExamples()
        {
            var list = new List<string>();
            string[] lines = System.IO.File.ReadAllLines( Server.MapPath( "~/Blocks/Examples/RockControlGallery.ascx" ) );
            var foundExample = false;
            var firstLine = false;
            int numSpaces = 0;
            int examplesDivCount = 0;
            Regex rgx = new Regex( @"^\s+" );
            Regex divExample = new Regex( @"<div (id=.* )*runat=""server"" (id=.* )*class=""r-example"">", RegexOptions.IgnoreCase );
            StringBuilder sb = new StringBuilder();
            foreach ( string line in lines )
            {
                if ( divExample.IsMatch( line ) )
                {
                    foundExample = true;
                    firstLine = true;
                    continue;
                }
                else if ( foundExample && line.Contains( "</div>" ) )
                {
                    // once we've eaten all the example's ending </div> tags then the example is over.
                    if ( examplesDivCount == 0 )
                    {
                        foundExample = false;
                        list.Add( sb.ToString() );
                        sb.Clear();
                    }
                    else
                    {
                        // eat another example </div>
                        examplesDivCount--;
                    }
                }
                else if ( foundExample && line.Contains( "<div" ) )
                {
                    // keep track of each <div> we encounter while in the example
                    examplesDivCount++;
                }

                if ( foundExample )
                {
                    // build regex used to trim off the correct number of spaces we see
                    // in the first line of the example.
                    if ( firstLine )
                    {
                        numSpaces = line.Length - line.TrimStart( ' ' ).Length;
                        rgx = new Regex( @"^\s{" + numSpaces + "}" );
                        firstLine = false;
                    }

                    sb.AppendLine( rgx.Replace( line, string.Empty ) );
                }
            }

            return list;
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
                bddlExample.Items.Clear();
                bddlExample.Items.Add( new ListItem( "Pickles", "44" ) );
                bddlExample.Items.Add( new ListItem( "Onions", "88" ) );
                bddlExample.Items.Add( new ListItem( "Ketchup", "150" ) );
                bddlExample.Items.Add( new ListItem( "Mustard", "654" ) );
                bddlExample.SelectedValue = "44";

                ddlDataExample.Items.AddRange( bddlExample.Items.OfType<ListItem>().ToArray() );

                cblExample.Items.AddRange( bddlExample.Items.OfType<ListItem>().ToArray() );
                cblExampleHorizontal.Items.AddRange( bddlExample.Items.OfType<ListItem>().ToArray() );
                rblExample.Items.AddRange( bddlExample.Items.OfType<ListItem>().ToArray() );
                rblExampleHorizontal.Items.AddRange( bddlExample.Items.OfType<ListItem>().ToArray() );

                campsExample.Campuses = Rock.Web.Cache.CampusCache.All();
                
                var rockContext = new RockContext();
                var allGroupTypes = new GroupTypeService( rockContext ).Queryable().OrderBy( a => a.Name ).ToList();
                gpGroupType.GroupTypes = allGroupTypes;
                gpGroupTypes.GroupTypes = allGroupTypes;

                sdrpExample.SlidingDateRangeMode = SlidingDateRangePicker.SlidingDateRangeType.Last;
                sdrpExample.TimeUnit = SlidingDateRangePicker.TimeUnitType.Week;
                sdrpExample.NumberOfTimeUnits = 16;

                BindGrid();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnShowAttributeEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnShowAttributeEditor_Click( object sender, EventArgs e )
        {
            edtExample.FieldTypeId = Rock.Web.Cache.FieldTypeCache.Read( Rock.SystemGuid.FieldType.TEXT ).Id;
            pnlAttributeEditor.Visible = !pnlAttributeEditor.Visible;
        }

        /// <summary>
        /// Handles the CancelClick event of the aeExample control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void aeExample_CancelClick( object sender, EventArgs e )
        {
            pnlAttributeEditor.Visible = false;
        }

        /// <summary>
        /// Handles the SaveClick event of the aeExample control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void aeExample_SaveClick( object sender, EventArgs e )
        {
            pnlAttributeEditor.Visible = false;
        }

        /// <summary>
        /// Handles the CheckedChanged event of the toggleShowPreview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void toggleShowPreview_CheckedChanged( object sender, EventArgs e )
        {
            toggleShowPreview.Help = "you just set it to : " + ( toggleShowPreview.Checked ? "on" : "off" );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the binaryFileTypePicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void binaryFileTypePicker_SelectedIndexChanged( object sender, EventArgs e )
        {
            bfpExample.BinaryFileTypeId = bftpExample.SelectedValueAsInt();
        }

        /// <summary>
        /// Handles the GridRebind event of the gExample control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gExample_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// 
        /// </summary>
        private class ExampleDataItem
        {
            /// <summary>
            /// Gets or sets the id.
            /// </summary>
            /// <value>
            /// The id.
            /// </value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the color of the defined value.
            /// </summary>
            /// <value>
            /// The color of the defined value.
            /// </value>
            public string DefinedValueColor { get; set; }

            /// <summary>
            /// Gets or sets the name of the defined value type.
            /// </summary>
            /// <value>
            /// The name of the defined value type.
            /// </value>
            public string DefinedValueTypeName { get; set; }

            /// <summary>
            /// Gets or sets some date time.
            /// </summary>
            /// <value>
            /// Some date time.
            /// </value>
            public DateTime SomeDateTime { get; set; }

            /// <summary>
            /// Gets or sets some boolean.
            /// </summary>
            /// <value>
            /// Some boolean.
            /// </value>
            public bool SomeBoolean { get; set; }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            Random random = new Random();

            List<ExampleDataItem> dataList = new List<ExampleDataItem>();
            dataList.Add( new ExampleDataItem { Id = 1, DefinedValueColor = "green", DefinedValueTypeName = "Pickles", SomeDateTime = RockDateTime.Now, SomeBoolean = random.Next( 2 ) == 1 } );
            dataList.Add( new ExampleDataItem { Id = 2, DefinedValueColor = "#ff0000", DefinedValueTypeName = "Ketchup", SomeDateTime = RockDateTime.Now, SomeBoolean = random.Next( 2 ) == 1 } );
            dataList.Add( new ExampleDataItem { Id = 3, DefinedValueColor = "white", DefinedValueTypeName = "Onions", SomeDateTime = RockDateTime.Now, SomeBoolean = random.Next( 2 ) == 1 } );
            dataList.Add( new ExampleDataItem { Id = 4, DefinedValueColor = "rgb(255,255,0)", DefinedValueTypeName = "Mustard", SomeDateTime = RockDateTime.Now, SomeBoolean = random.Next( 2 ) == 1 } );

            SortProperty sortProperty = gExample.SortProperty ?? new SortProperty( new GridViewSortEventArgs( "DefinedValueTypeName", SortDirection.Ascending ) );

            gExample.DataSource = dataList.AsQueryable().Sort( sortProperty ).ToList();
            gExample.DataBind();
        }

        /// <summary>
        /// Handles the SelectedMonthYearChanged event of the monthYearPicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void monthYearPicker_SelectedMonthYearChanged( object sender, EventArgs e )
        {
            var dateTime = mypExample.SelectedDate;
        }

        /// <summary>
        /// Handles the SelectedMonthDayChanged event of the monthDayPicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void monthDayPicker_SelectedMonthDayChanged( object sender, EventArgs e )
        {
            var dateTime = mdpExample.SelectedDate;
        }

        /// <summary>
        /// Handles the SelectedBirthdayChanged event of the birthdayPicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void birthdayPicker_SelectedBirthdayChanged( object sender, EventArgs e )
        {
            var dateTime = bdaypExample.SelectedDate;
        }

        /// <summary>
        /// Handles the SaveSchedule event of the scheduleBuilder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void scheduleBuilder_SaveSchedule( object sender, EventArgs e )
        {
            string debug = schedbExample.iCalendarContent;
        }

        /// <summary>
        /// Handles the SelectGeography event of the geoPicker1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void geoPicker1_SelectGeography( object sender, EventArgs e )
        {
            string debug = geopExamplePoint.SelectedValue.AsText();
        }

        /// <summary>
        /// Handles the SelectGeography event of the geoPicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void geoPicker_SelectGeography( object sender, EventArgs e )
        {
            string debug = geopExamplePolygon.SelectedValue.AsText();
        }

        /// <summary>
        /// Handles the FileUploaded event of the fupContentFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fupContentFile_FileUploaded( object sender, EventArgs e )
        {
            string physicalFileName = this.Request.MapPath( fuprExampleContentFile.UploadedContentFilePath );
            lblPhysicalFileName.Text = "Uploaded File: " + physicalFileName;
        }
}
}