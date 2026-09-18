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

using System.ComponentModel;

using Rock.AI.Agent.Annotations;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CmsSkill
{
    #region Tool(s)

    /// <summary>
    /// Gets the attributes that can be set on a page.
    /// </summary>
    /// <remarks>
    /// A page's attributes are qualified by its parent page, layout, and site, so
    /// the set can be resolved either from an existing page or, before the page is
    /// created, from a stub built out of those keys.
    /// </remarks>
    [Description( "Gets the attributes that can be set when adding or updating a page." )]
    [AgentPurpose( "Determines which attribute values AddOrUpdatePage accepts." )]
    [AgentUsage( "To inspect an existing page, pass pageIdKey. To see what a not-yet-created page will accept, omit pageIdKey and pass the parentPageIdKey (and optionally layoutIdKey or siteIdKey) that the page will be created under." )]
    [AgentToolPrerequisite( "Call GetPage, ListPages, or SearchPages to determine the pageIdKey, or ListPages for the parentPageIdKey when the page does not exist yet." )]
    [AgentToolGuid( "DBBE1DDC-7ACC-49B1-AE0A-32919E785D37" )]
    public AgentToolResult GetPageAvailableAttributes(
        [Description( "The IdKey or guid of an existing page. Omit when inspecting the attributes of a page before it is created." )]
        string pageIdKey = null,
        [Description( "The IdKey or guid of the parent page a new page will live under. Used when pageIdKey is not provided." )]
        string parentPageIdKey = null,
        [Description( "The IdKey or guid of the layout a new page will render with. Used when pageIdKey is not provided." )]
        string layoutIdKey = null,
        [Description( "The IdKey or guid of the site a new page will belong to. Used when pageIdKey is not provided." )]
        string siteIdKey = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        Model.Page page;

        if ( pageIdKey.IsNotNullOrWhiteSpace() )
        {
            page = helper.GetRequiredEntity<Model.Page>( pageIdKey, checkSecurity: true );

            if ( page == null )
            {
                return helper.ErrorResult
                    .WithInstructions( "Call the ListPages or SearchPages function to determine the available pages." );
            }
        }
        else
        {
            // Resolve any placement keys the caller supplied so a bad key is a
            // clean error and the stub mirrors the page that would be created.
            // Page attributes are qualified by ParentPageId, LayoutId, and SiteId,
            // so all three are set on the stub before its attributes are loaded.
            var parentPage = helper.GetOptionalEntity<Model.Page>( parentPageIdKey, checkSecurity: false );
            var layout = helper.GetOptionalEntity<Model.Layout>( layoutIdKey, checkSecurity: false );
            var site = helper.GetOptionalEntity<Model.Site>( siteIdKey, checkSecurity: false );

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            // A page's layout defaults to its parent's, and its site is derived
            // from that layout, so fall back through both when they are not given
            // explicitly.
            var effectiveLayoutId = layout?.Id ?? parentPage?.LayoutId;
            var siteId = site?.Id;

            if ( !siteId.HasValue && effectiveLayoutId.HasValue )
            {
                siteId = LayoutCache.Get( effectiveLayoutId.Value, AgentRequestContext.RockContext )?.SiteId;
            }

            page = new Model.Page
            {
                ParentPageId = parentPage?.Id,
                LayoutId = effectiveLayoutId ?? 0
            };

            if ( siteId.HasValue )
            {
                page.SetSiteIdForLoadingAttributes( siteId.Value );
            }
        }

        page.LoadAttributes( AgentRequestContext.RockContext );

        return Success( helper.GetAvailableAttributes( page ) );
    }

    #endregion
}
