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
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace RockWeb.Blocks.Examples
{
    /// <summary>
    /// A sample block that uses many of the Rock UI controls.
    /// </summary>
    public partial class RockControlGalleryMini : RockBlock
    {
        Regex specialCharsRegex = new Regex( "[^a-zA-Z0-9-]" );

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            InitSyntaxHighlighting();

            htmlEditorLight.MergeFields.Add( "GlobalAttribute" );
            htmlEditorLight.MergeFields.Add( "Rock.Model.Person" );


            List<string> list = ReadExamples();
            int i = -1;
            foreach ( var example in pnlDetails.ControlsOfTypeRecursive<HtmlControl>() )
            {
                if ( example.Attributes["class"] == "r-example" )
                {
                    i++;
                    example.Controls.Add( new LiteralControl( string.Format( "<pre class='prettyprint'>{0}</pre>", Server.HtmlEncode( list[i] ) ) ) );
                }

                if ( example.TagName == "h1" || example.TagName == "h2" || example.TagName == "h3" )
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
            RockPage.AddScriptLink( "//cdnjs.cloudflare.com/ajax/libs/prettify/r298/prettify.js" );
        }

        /// <summary>
        /// Reads this block to find embedded examples and returns them in a indexed list.
        /// </summary>
        /// <returns>code examples by postion index</returns>
        private List<string> ReadExamples()
        {
            var list = new List<string>();
            string[] lines = System.IO.File.ReadAllLines( Server.MapPath( "~/Blocks/Examples/RockControlGalleryMini.ascx" ) );
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
                    sb.AppendLine( rgx.Replace( line, "" ) );
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
            }
        }
    }
}