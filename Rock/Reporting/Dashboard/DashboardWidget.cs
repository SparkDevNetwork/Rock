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
using System.Collections.Generic;

using Rock.Attribute;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.Dashboard
{
    /// <summary>
    /// 
    /// </summary>
    [TextField( "Title", "The title of the widget", false, Order = 0 )]
    [TextField( "Subtitle", "The subtitle of the widget", false, Order = 1 )]
    [CustomDropdownListField( "Column Width", "The width of the widget.", ",1,2,3,4,5,6,7,8,9,10,11,12", false, "4", Order = 2 )]
    [ContextAware(IsConfigurable=false)]
    public abstract class DashboardWidget : RockBlock
    {
        /// <summary>
        /// Gets or sets the widget error message.
        /// </summary>
        /// <value>
        /// The widget error message.
        /// </value>
        public string WidgetErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the widget error details.
        /// </summary>
        /// <value>
        /// The widget error details.
        /// </value>
        public string WidgetErrorDetails { get; set; }

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
                return GetAttributeValue( "ColumnWidth" ).AsIntegerOrNull();
            }
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( System.Web.UI.HtmlTextWriter writer )
        {
            List<string> widgetCssList = GetDivWidthCssClasses();

            writer.AddAttribute( System.Web.UI.HtmlTextWriterAttribute.Class, widgetCssList.AsDelimited( " " ) );
            writer.RenderBeginTag( System.Web.UI.HtmlTextWriterTag.Div );

            writer.AddAttribute( System.Web.UI.HtmlTextWriterAttribute.Class, "panel-dashboard" );
            writer.RenderBeginTag( System.Web.UI.HtmlTextWriterTag.Div );

            writer.AddAttribute( System.Web.UI.HtmlTextWriterAttribute.Class, "panel-body" );
            writer.RenderBeginTag( System.Web.UI.HtmlTextWriterTag.Div );

            if ( !string.IsNullOrWhiteSpace( WidgetErrorMessage ) )
            {
                var errorBox = new NotificationBox { ID = "nbWidgetError", NotificationBoxType = NotificationBoxType.Danger, Text = WidgetErrorMessage, Title = "Error", Dismissable = true, Details = WidgetErrorDetails };
                errorBox.RenderControl( writer );
            }

            base.RenderControl( writer );

            writer.RenderEndTag();

            writer.RenderEndTag();

            writer.RenderEndTag();
        }

        /// <summary>
        /// Gets the div width CSS classes.
        /// </summary>
        /// <returns></returns>
        private List<string> GetDivWidthCssClasses()
        {
            int? mediumColumnWidth = this.GetAttributeValue( "ColumnWidth" ).AsIntegerOrNull();

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
