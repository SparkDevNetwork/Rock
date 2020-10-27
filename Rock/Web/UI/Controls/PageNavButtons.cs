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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Web.UI;

using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Renders an &lt;a&gt; element for each child page based on a root page.
    /// </summary>
    public class PageNavButtons : Control
    {
        #region ViewState Keys

        /// <summary>
        /// Keys to use for ViewState.
        /// </summary>
        private class ViewStateKey
        {
            public const string RootPageId = "RootPageId";
            public const string IncludeCurrentParameters = "IncludeCurrentParameters";
            public const string IncludeCurrentQueryString = "IncludeCurrentQueryString";
            public const string QueryStringParametersToAdd = "QueryStringParametersToAdd";
            public const string CssClass = "CssClass";
            public const string CssClassActive = "CssClassActive";
        }

        #endregion ViewState Keys

        #region Properties

        /// <summary>
        /// The root page ID to use for the child page collection. Defaults to the current page instance if not set.
        /// </summary>
        [
        Bindable( true ),
        Description( "The root page ID to use for the child page collection. Defaults to the current page instance if not set." )
        ]
        public int RootPageId
        {
            get
            {
                return ( ViewState[ViewStateKey.RootPageId] as string ).AsInteger();
            }

            set
            {
                ViewState[ViewStateKey.RootPageId] = value.ToString();
            }
        }

        /// <summary>
        /// Flag indicating if the current page's route parameters should be used when building the URL for child pages. Default is false.
        /// </summary>
        [
        Bindable( true ),
        Description( "Flag indicating if the current page's route parameters should be used when building the URL for child pages. Default is false." )
        ]
        public bool IncludeCurrentParameters
        {
            get
            {
                return ( ViewState[ViewStateKey.IncludeCurrentParameters] as string ).AsBoolean();
            }

            set
            {
                ViewState[ViewStateKey.IncludeCurrentParameters] = value.ToString();
            }
        }

        /// <summary>
        /// Flag indicating if the current page's QueryString should be used when building the URL for child pages. Default is false.
        /// </summary>
        [
        Bindable( true ),
        Description( "Flag indicating if the current page's QueryString should be used when building the URL for child pages. Default is false." )
        ]
        public bool IncludeCurrentQueryString
        {
            get
            {
                return ( ViewState[ViewStateKey.IncludeCurrentQueryString] as string ).AsBoolean();
            }

            set
            {
                ViewState[ViewStateKey.IncludeCurrentQueryString] = value.ToString();
            }
        }

        private NameValueCollection _queryStringParametersToAdd;

        /// <summary>
        /// Any query string parameters that should be added to each &lt;a&gt; element's href attribute. If a matching key is found in the current query string, it's value will be replaced with the value specified here.
        /// </summary>
        [
        Bindable( true ),
        Description( "Any query string parameters that should be added to each <a> element's href attribute. If a matching key is found in the current query string, it's value will be replaced with the value specified here." )
        ]
        public NameValueCollection QueryStringParametersToAdd
        {
            get
            {
                if ( _queryStringParametersToAdd == null )
                {
                    object parms = ViewState[ViewStateKey.QueryStringParametersToAdd];
                    _queryStringParametersToAdd = ( parms == null ? null : ( NameValueCollection ) parms );
                }

                return _queryStringParametersToAdd;
            }

            set
            {
                _queryStringParametersToAdd = value;
                ViewState[ViewStateKey.QueryStringParametersToAdd] = _queryStringParametersToAdd;
            }
        }

        private readonly string CssClassDefault = "btn";

        /// <summary>
        /// The CSS class(es) to be added to each &lt;a&gt; element created (one per page in the child page collection). Default is 'btn'.
        /// </summary>
        [
        Bindable( true ),
        Description( "The CSS class(es) to be added to each <a> element created (one per page in the child page collection). Default is 'btn'." )
        ]
        public string CssClass
        {
            get
            {
                var cssClass = ViewState[ViewStateKey.CssClass] as string;
                if ( cssClass.IsNotNullOrWhiteSpace() )
                {
                    return cssClass;
                }

                return CssClassDefault;
            }
        }

        private readonly string CssClassActiveDefault = "btn-primary";

        /// <summary>
        /// The CSS class(es) to be added to the &lt;a&gt; element created for any active page within the child page collection (Page.Id == RockPage.PageId). Default is 'btn-primary'.
        /// </summary>
        [
        Bindable( true ),
        Description( "The CSS class(es) to be added to the <a> element created for any active page within the child page collection (Page.Id == RockPage.PageId). Default is 'btn-primary'." )
        ]
        public string CssClassActive
        {
            get
            {
                var cssClass = ViewState[ViewStateKey.CssClassActive] as string;
                if ( cssClass.IsNotNullOrWhiteSpace() )
                {
                    return cssClass;
                }

                return CssClassActiveDefault;
            }
        }

        #endregion Properties

        #region Base Control Overrides

        /// <summary>
        /// Sends server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object, which writes the content to be rendered on the client.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the server control content.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            RockPage rockPage = this.Page as RockPage;
            PageReference currentPageReference = rockPage?.PageReference;
            Person currentPerson = rockPage?.CurrentPerson;

            PageCache rootPage = RootPageId > 0 ? PageCache.Get( RootPageId ) : PageCache.Get( rockPage?.PageId ?? 0 );
            if ( rootPage == null )
            {
                return;
            }

            foreach ( PageCache page in GetChildPages( rootPage, currentPerson ) )
            {
                // href
                var pageReference = new PageReference( page.Id );
                if ( IncludeCurrentParameters )
                {
                    pageReference.Parameters = currentPageReference?.Parameters;
                }

                if ( IncludeCurrentQueryString )
                {
                    pageReference.QueryString = currentPageReference?.QueryString;
                }

                if ( QueryStringParametersToAdd != null )
                {
                    NameValueCollection mergedQueryString = new NameValueCollection( pageReference.QueryString ?? new NameValueCollection() );
                    foreach ( string key in QueryStringParametersToAdd )
                    {
                        mergedQueryString[key] = QueryStringParametersToAdd[key];
                    }

                    pageReference.QueryString = mergedQueryString;
                }

                writer.AddAttribute( HtmlTextWriterAttribute.Href, pageReference.BuildUrl() );

                // class
                string cssClass = CssClass;
                if ( rockPage?.PageId == page.Id && CssClassActive.IsNotNullOrWhiteSpace() )
                {
                    cssClass = cssClass.IsNotNullOrWhiteSpace()
                        ? string.Join( " ", cssClass, CssClassActive )
                        : CssClassActive;
                }

                if ( cssClass.IsNotNullOrWhiteSpace() )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, cssClass );
                }

                // <a>
                writer.RenderBeginTag( HtmlTextWriterTag.A );
                writer.Write( page.PageTitle );
                writer.RenderEndTag();
            }
        }

        #endregion Base Control Overrides

        #region Internal Methods

        /// <summary>
        /// Gets the child pages.
        /// </summary>
        /// <param name="rootPage">The root page.</param>
        /// <param name="currentPerson">The current person.</param>
        private List<PageCache> GetChildPages( PageCache rootPage, Person currentPerson )
        {
            var pages = new List<PageCache>();

            using ( var rockContext = new RockContext() )
            {
                foreach ( PageCache page in rootPage.GetPages( rockContext ) )
                {
                    // IsAuthorized() knows how to handle a null person argument.
                    if ( page.DisplayInNavWhen == DisplayInNavWhen.WhenAllowed && !page.IsAuthorized( Authorization.VIEW, currentPerson ) )
                    {
                        continue;
                    }

                    if ( page.DisplayInNavWhen == DisplayInNavWhen.Never )
                    {
                        continue;
                    }

                    pages.Add( page );
                }
            }

            return pages;
        }

        #endregion Internal Methods
    }
}