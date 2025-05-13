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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Model;
using Rock.Utility;
using Rock.Web;
using Rock.Web.Cache;

public partial class Http429Error : System.Web.UI.Page
{
    /// <summary>
    /// Handles the Init event of the Page control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    protected void Page_Init(object sender, EventArgs e)
    {
        // If this is an API call, set status code and exit
        if ( Request.Url.Query.Contains( Request.Url.Authority + ResolveUrl( "~/api/" ) ) )
        {
            Response.StatusCode = 429;
            Response.Flush();
            Response.End();
            return;
        }
    }

    /// <summary>
    /// Handles the Load event of the Page control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            // Set form action to pass XSS test
            form1.Action = "/";

            // try to get site's 429 page
            var host = WebRequestHelper.GetHostNameFromRequest( HttpContext.Current );
            SiteCache site = SiteCache.GetSiteByDomain( host );
            if ( site != null && site.PageNotFoundPageId.HasValue )
            {
                site.RedirectToPageNotFoundPage();
            }
            else
            {
                Response.StatusCode = 429;
                lLogoSvg.Text = System.IO.File.ReadAllText( HttpContext.Current.Request.MapPath( "~/Assets/Images/rock-logo-sm.svg" ) );
            }
        }
        catch 
        {
            Response.StatusCode = 429;
            lLogoSvg.Text = System.IO.File.ReadAllText( HttpContext.Current.Request.MapPath( "~/Assets/Images/rock-logo-sm.svg" ) );
        }
        finally
        {
            // Tell the browsers to not cache.
            Response.Cache.SetCacheability( System.Web.HttpCacheability.NoCache );
            Response.Cache.SetExpires( DateTime.UtcNow.AddHours( -1 ) );
            Response.Cache.SetNoStore();
        }
    }
}