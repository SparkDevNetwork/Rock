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

using System.Collections.Generic;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.CmsSkill;

/// <summary>
/// Result model for a CMS page. List tools fill the summary properties;
/// GetPage and AddOrUpdatePage fill the detail properties as well.
/// </summary>
internal class PageResult : EntityResultBase
{
    /// <summary>
    /// The internal (administrative) name of the page.
    /// </summary>
    public string InternalName { get; set; }

    /// <summary>
    /// The title shown to visitors when the page renders.
    /// </summary>
    public string PageTitle { get; set; }

    /// <summary>
    /// The title shown in the browser tab. Detail only.
    /// </summary>
    public string BrowserTitle { get; set; }

    /// <summary>
    /// The administrative description of the page. Detail only.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The name of the site the page belongs to, when known.
    /// </summary>
    public string SiteName { get; set; }

    /// <summary>
    /// The relative URL the page is reachable at. This is the first friendly
    /// route when one exists, otherwise the /page/id fallback.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// The parent page, when the page is not a root page.
    /// </summary>
    public KeyNameResult ParentPage { get; set; }

    /// <summary>
    /// The layout the page renders with. Detail only.
    /// </summary>
    public LayoutResult Layout { get; set; }

    /// <summary>
    /// The friendly routes configured for the page. Detail only.
    /// </summary>
    public List<string> Routes { get; set; }

    /// <summary>
    /// When the page is shown in navigation menus, e.g. <c>WhenAllowed</c>.
    /// Detail only.
    /// </summary>
    public string DisplayInNavWhen { get; set; }

    /// <summary>
    /// The number of immediate child pages. Lets the caller decide whether to
    /// walk deeper with ListPages.
    /// </summary>
    public int? ChildPageCount { get; set; }

    /// <summary>
    /// The blocks placed on the page, summarized. Detail only.
    /// </summary>
    public List<BlockResult> Blocks { get; set; }
}
