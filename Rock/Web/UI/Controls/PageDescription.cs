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

using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Renders the description of a page
    /// </summary>
    public class PageDescription : Control
    {
        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            var rockPage = this.Page as RockPage;
            if ( rockPage != null )
            {
                var pageCache = PageCache.Get( rockPage.PageId );
                if ( pageCache != null && pageCache.PageDisplayDescription && !string.IsNullOrWhiteSpace( pageCache.Description ) )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "pageoverview-description" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    writer.Write( pageCache.Description );
                    writer.RenderEndTag();
                }
            }
        }
    }
}