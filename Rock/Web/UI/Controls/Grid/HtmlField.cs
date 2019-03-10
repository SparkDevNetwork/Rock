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
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Web.UI.Controls.RockBoundField" />
    [ToolboxData( "<{0}:HtmlField runat=server></{0}:HtmlField>" )]
    public class HtmlField : RockBoundField
    {
        /// <summary>
        /// Gets or sets the display mode.
        /// </summary>
        /// <value>
        /// The display mode.
        /// </value>
        public HtmlFieldDisplayMode DisplayMode
        {
            get
            {
                object displayMode = ViewState["DisplayMode"];
                return displayMode != null ? ( HtmlFieldDisplayMode ) displayMode : HtmlFieldDisplayMode.PlainText;
            }
            set
            {
                ViewState["DisplayMode"] = value;
            }
        }

        /// <summary>
        /// Formats the specified field value for a cell in the <see cref="T:System.Web.UI.WebControls.BoundField" /> object.
        /// </summary>
        /// <param name="dataValue">The field value to format.</param>
        /// <param name="encode">true to encode the value; otherwise, false.</param>
        /// <returns>
        /// The field value converted to the format specified by <see cref="P:System.Web.UI.WebControls.BoundField.DataFormatString" />.
        /// </returns>
        protected override string FormatDataValue( object dataValue, bool encode )
        {
            string cleanHtmlString = dataValue.ToString();

            if ( string.IsNullOrWhiteSpace( cleanHtmlString ) )
            {
                return base.FormatDataValue( cleanHtmlString, encode );
            }

            switch ( ( HtmlFieldDisplayMode ) ViewState["DisplayMode"] )
                {
                case HtmlFieldDisplayMode.Raw:
                    HtmlEncode = true;
                    break;

                case HtmlFieldDisplayMode.PlainText:
                    cleanHtmlString = dataValue.ToString().ScrubHtmlForGridDisplay();
                    HtmlEncode = false;
                    break;

                case HtmlFieldDisplayMode.Rendered:
                    HtmlEncode = false;
                    break;

                default:
                    cleanHtmlString = dataValue.ToString().ScrubHtmlForGridDisplay();
                    HtmlEncode = false;
                    break;
            }

            return base.FormatDataValue( cleanHtmlString, encode );
        }

        /// <summary>
        /// Gets the value that should be exported to Excel
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns></returns>
        public override object GetExportValue( GridViewRow row )
        {
            if ( row.DataItem is System.Data.DataRowView )
            {
                var dataRow = ( ( System.Data.DataRowView ) row.DataItem ).Row;
                return dataRow[this.DataField];
            }

            var htmlString = row.DataItem.GetPropertyValue( this.DataField );
            return FormatDataValue( htmlString );
        }
    }

    /// <summary>
    /// Raw will display all text including HTML tags
    /// PlainText will strip out HTML but display the text with line breaks
    /// Rendered will display the text with HTML formatting
    /// </summary>
    public enum HtmlFieldDisplayMode
    {
        /// <summary>
        /// Display all text including HTML tags as text
        /// </summary>
        Raw,

        /// <summary>
        /// Remove HTML tags and display the plain text with line brakes
        /// </summary>
        PlainText,

        /// <summary>
        /// Render with HTML formatting
        /// </summary>
        Rendered
    }
}
