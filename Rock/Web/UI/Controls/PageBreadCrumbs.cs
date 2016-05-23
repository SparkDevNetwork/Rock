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

namespace Rock.Web.UI.Controls
{
    
    /// <summary>
    /// Displays a bootstrap badge
    /// </summary>
    [ToolboxData( "<{0}:PageBreadCrumbs runat=server></{0}:PageBreadCrumbs>" )]
    public class PageBreadCrumbs : Control
    {

        /// <summary>
        /// Gets or sets the javascript that will get executed when the ckeditor 'on key' event occurs
        /// </summary>
        /// <value>
        /// The on key press script.
        /// </value>
        public string PreText
        {
            get
            {
                return ViewState["PreText"] as string;
            }

            set
            {
                ViewState["PreText"] = value;
            }
        }
        
        
        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            var rockPage = this.Page as RockPage;
            if ( rockPage != null )
            {
                var pageCache = Rock.Web.Cache.PageCache.Read( rockPage.PageId );
                if ( pageCache != null && pageCache.PageDisplayBreadCrumb )
                {
                    var crumbs = rockPage.BreadCrumbs;
                    if ( crumbs != null )
                    {
                        writer.Write(this.PreText);
                        writer.AddAttribute( HtmlTextWriterAttribute.Class, "breadcrumb" );
                        writer.RenderBeginTag( HtmlTextWriterTag.Ul );

                        foreach ( var crumb in crumbs )
                        {
                            if (!crumb.Active )
                            {
                                writer.RenderBeginTag( HtmlTextWriterTag.Li );
                                writer.AddAttribute( HtmlTextWriterAttribute.Href, crumb.Url );
                                writer.RenderBeginTag( HtmlTextWriterTag.A );
                                writer.Write( crumb.Name );
                                writer.RenderEndTag();
                                writer.RenderEndTag();
                            }
                            else
                            {
                                writer.AddAttribute( HtmlTextWriterAttribute.Class, "active" );
                                writer.RenderBeginTag( HtmlTextWriterTag.Li );
                                writer.Write( crumb.Name );
                                writer.RenderEndTag();
                            }
                        }

                        writer.RenderEndTag();
                    }
                }
            }
        }
    }
}
