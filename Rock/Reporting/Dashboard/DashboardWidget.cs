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
using System.Text;
using Rock.Attribute;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.Dashboard
{
    /// <summary>
    /// 
    /// </summary>
    [TextField( "Title", "The title of the widget", false )]
    [TextField( "Subtitle", "The subtitle of the widget", false )]
    [CustomDropdownListField( "Column Width", "The width of the widget.", ",1,2,3,4,5,6,7,8,9,10,11,12", false, "4" )]
    [ContextAware()]
    public abstract class DashboardWidget : RockBlock
    {
        /// <summary>
        /// Gets the Title attribute value
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title
        {
            get
            {
                return GetAttributeValue( "Title" );
            }
        }

        /// <summary>
        /// Gets the Subtitle attribute value
        /// </summary>
        /// <value>
        /// The subtitle.
        /// </value>
        public string Subtitle
        {
            get
            {
                return GetAttributeValue( "Subtitle" );
            }
        }

        /// <summary>
        /// Gets the Column Width attribute value
        /// This will be a value from 1-12 (or null) that represents the col-md- width of this Dashboard Widget
        /// </summary>
        /// <value>
        /// The width of the column.
        /// </value>
        public int? ColumnWidth
        {
            get
            {
                return GetAttributeValue( "ColumnWidth" ).AsInteger( false );
            }
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( System.Web.UI.HtmlTextWriter writer )
        {
            List<string> widgetCssList = GetDivWidthCssClasses();
            
            writer.AddAttribute( System.Web.UI.HtmlTextWriterAttribute.Class, widgetCssList.AsDelimited(" ") );
            writer.RenderBeginTag( System.Web.UI.HtmlTextWriterTag.Div );
            
            writer.AddAttribute( System.Web.UI.HtmlTextWriterAttribute.Class, "panel panel-dashboard" );
            writer.RenderBeginTag( System.Web.UI.HtmlTextWriterTag.Div );

            writer.AddAttribute( System.Web.UI.HtmlTextWriterAttribute.Class, "panel-body" );
            writer.RenderBeginTag( System.Web.UI.HtmlTextWriterTag.Div );

            base.RenderControl( writer );

            writer.RenderEndTag();

            writer.RenderEndTag();

            writer.RenderEndTag();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            /*
            List<string> widgetCssList = GetDivWidthCssClasses();

            // find the Block Wrapper div that RockPage creates and add additional our special css classes to it 
            var parent = this.Parent;
            while ( parent != null )
            {
                if ( parent is HtmlGenericContainer )
                {
                    HtmlGenericContainer container = parent as HtmlGenericContainer;
                    if ( container.ID == string.Format( "bid_{0}", this.BlockId ) )
                    {
                        foreach ( var widgetCss in widgetCssList )
                        {
                            container.AddCssClass( widgetCss );
                        }

                        break;
                    }
                }

                parent = parent.Parent;
            }
            */
        }

        private List<string> GetDivWidthCssClasses()
        {
            int? mediumColumnWidth = this.GetAttributeValue( "ColumnWidth" ).AsInteger( false );

            // add additional css to the block wrapper (if mediumColumnWidth is specified)
            List<string> widgetCssList = new List<string>();
            if ( mediumColumnWidth.HasValue )
            {

                // Table to use to derive col-xs and col-sm from the selected medium width
                /*
                XS	SM	MD
                4	2	1
                6	4	2
                6	4	3
                    6	4
            	        5
            	        6
            	        7
            	        8
            	        9
            	        10
            	        11
            	        12 */

                int? xsmallColumnWidth;
                int? smallColumnWidth;

                // logic to set reasonable col-xs- and col-sm- classes from the selected mediumColumnWidth (col-md-X)
                switch ( mediumColumnWidth.Value )
                {
                    case 1:
                        xsmallColumnWidth = 4;
                        smallColumnWidth = 2;
                        break;
                    case 2:
                    case 3:
                        xsmallColumnWidth = 6;
                        smallColumnWidth = 4;
                        break;
                    case 4:
                        xsmallColumnWidth = null;
                        smallColumnWidth = 6;
                        break;
                    default:
                        xsmallColumnWidth = null;
                        smallColumnWidth = null;
                        break;
                }


                widgetCssList.Add( string.Format( "col-md-{0}", mediumColumnWidth ) );
                if ( xsmallColumnWidth.HasValue )
                {
                    widgetCssList.Add( string.Format( "col-xs-{0}", xsmallColumnWidth ) );
                }

                if ( smallColumnWidth.HasValue )
                {
                    widgetCssList.Add( string.Format( "col-sm-{0}", smallColumnWidth ) );
                }
            }
            return widgetCssList;
        }
    }
}
