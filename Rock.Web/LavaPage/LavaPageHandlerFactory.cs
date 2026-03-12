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
using System.IO;
using System.Web;

using Rock.Configuration;
using Rock.Model;
using Rock.Net;
using Rock.Web.Cache;

namespace Rock.Web.LavaPage;

/// <summary>
/// Provides a page handler factory that creates HTTP handlers for pages
/// using Lava templates.
/// </summary>
internal class LavaPageHandlerFactory : IPageHandlerFactory
{
    /// <inheritdoc/>
    public IHttpHandler CreateHandler( PageCache page, PageReference pageReference, HttpContextBase httpContext )
    {
        var theme = page.Layout.Site.Theme;
        var layout = page.Layout.FileName;
        var layoutPath = PageCache.FormatPath( theme, layout );

        var filePath = RockApp.Current.MapPath( layoutPath ).Replace( ".aspx", ".lava" );

        if ( !File.Exists( filePath ) )
        {
            return null;
        }

        var requestWrapper = new HttpRequestBaseWrapper( httpContext.Request );
        var responseWrapper = new RockResponseBase();
        var user = UserLoginService.GetCurrentUser( false );
        var rockRequestContext = new RockRequestContext( requestWrapper, responseWrapper, user );

        rockRequestContext.PrepareRequestForPage( page, pageReference );

        return new LavaPageHandler( filePath, rockRequestContext );
    }
}
