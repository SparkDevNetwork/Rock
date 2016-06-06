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
using System.Web.UI.HtmlControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// An HtmlGenericContainer that implements the INamingContainer interface
    /// </summary>
    public class HtmlGenericContainer : HtmlGenericControl, INamingContainer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlGenericContainer"/> class.
        /// </summary>
        public HtmlGenericContainer()
            : base( "div" )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlGenericContainer" /> class.
        /// </summary>
        /// <param name="tag">The name of the element for which this instance of the class is created.</param>
        /// <param name="cssClass">The CSS class.</param>
        public HtmlGenericContainer( string tag, string cssClass = null )
            : base( tag )
        {
            if ( cssClass != null )
            {
                CssClass = cssClass;
            }
        }

        /// <summary>
        /// Gets or sets the CSS class.
        /// </summary>
        /// <value>
        /// The CSS class.
        /// </value>
        public string CssClass
        {
            get
            {
                return this.Attributes["class"];
            }

            set
            {
                this.Attributes["class"] = value;
            }
        }
    }
}