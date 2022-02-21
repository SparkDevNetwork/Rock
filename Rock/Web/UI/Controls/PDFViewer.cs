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
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// PDF Viewer
    /// </summary>
    public class PDFViewer : CompositeControl, INamingContainer
    {
        private static class ViewStateKey
        {
            public const string SourceUrl = "SourceUrl";
            public const string ViewerHeight = "ViewerHeight";
            public const string FitView = "FitView";
            public const string PageMode = "PageMode";
        }

        #region Controls

        private Literal _lPDFContainer;

        #endregion Controls

        /// <summary>
        /// Gets or sets the URL of the PDF File.
        /// </summary>
        /// <value>The source URL.</value>
        public string SourceUrl
        {
            get => ViewState[ViewStateKey.SourceUrl] as string;
            set => ViewState[ViewStateKey.SourceUrl] = value;
        }

        /// <summary>
        /// The Height of the PDF Viewer
        /// </summary>
        /// <value>The height of the view.</value>
        public string ViewerHeight
        {
            get => ViewState[ViewStateKey.ViewerHeight] as string ?? "600px";
            set => ViewState[ViewStateKey.ViewerHeight] = value;
        }

        /// <summary>
        /// The initial Fit View.
        /// <br />
        /// <p><c>FitH</c> = fit to the width of the PDF</p>
        /// <br />
        /// <p><c>FitV</c> = fit to the height of the PDF</p>
        /// </summary>
        /// <value>The fit view.</value>
        public string FitView
        {
            get => ViewState[ViewStateKey.FitView] as string ?? "FitH";
            set => ViewState[ViewStateKey.FitView] = value;
        }

        /// <summary>
        /// Gets or sets Pdf PageMode option: 'bookmarks, thumbs, none'
        /// </summary>
        /// <value>The page mode.</value>
        public string PageMode
        {
            get => ViewState[ViewStateKey.PageMode] as string ?? "none";
            set => ViewState[ViewStateKey.PageMode] = value;
        }

        /// <summary>
        /// Gets the <see cref="T:System.Web.UI.HtmlTextWriterTag" /> value that corresponds to this Web server control. This property is used primarily by control developers.
        /// </summary>
        /// <value>The tag key.</value>
        protected override HtmlTextWriterTag TagKey => HtmlTextWriterTag.Div;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            if ( !ScriptManager.GetCurrent( this.Page ).IsInAsyncPostBack )
            {
                RockPage.AddScriptLink( Page, "~/Scripts/PDFObject/pdfobject.min.js" );
            }

            base.OnInit( e );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            _lPDFContainer = new Literal
            {
                ID = "_lPDFContainer",
            };

            Controls.Add( _lPDFContainer );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible && this.SourceUrl.IsNotNullOrWhiteSpace() )
            {
                _lPDFContainer.Text = $"<div id='{_lPDFContainer.ClientID}'></div>";
                base.RenderControl( writer );
                RegisterJavaScript();
            }
        }

        /// <summary>
        /// Registers the java script.
        /// </summary>
        protected virtual void RegisterJavaScript()
        {
            if ( ScriptManager.GetCurrent( this.Page ).IsInAsyncPostBack )
            {
                ScriptManager.RegisterClientScriptInclude( this.Page, this.Page.GetType(), "pdf_object-include", ResolveUrl( "~/Scripts/PDFObject/pdfobject.min.js" ) );
            }

            var pdfObjectScript = $@"
(function($) {{
    var options = {{
        height: '{ViewerHeight}',
        pdfOpenParams: {{
            view: '{FitView}',
            pagemode: '{PageMode}'
        }}
    }};

    PDFObject.embed('{SourceUrl}', '#{_lPDFContainer.ClientID}', options);
}})();
";

            ScriptManager.RegisterStartupScript( this, this.GetType(), "pdf_viewer_script" + this.ClientID, pdfObjectScript, true );
        }
    }
}
