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

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.CmsSkill;

/// <summary>
/// Result model for a Rock RMS Site (website, mobile app, or TV app).
/// LookupSites fills the summary properties; GetSite fills the detail
/// properties as well.
/// </summary>
internal class SiteResult : EntityResultBase
{
    /// <summary>
    /// Human-readable name of the site.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Short description of the site’s purpose or audience.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Type label, e.g., <c>Website</c>, <c>MobileApp</c>, <c>TvApp</c>.
    /// </summary>
    public string SiteType { get; set; }

    /// <summary>
    /// Optional external URL for the site, if applicable (e.g., public website).
    /// </summary>
    public string ExternalUrl { get; set; }

    /// <summary>
    /// Whether the site is active. Detail only.
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// The theme the site renders with. Detail only.
    /// </summary>
    public string Theme { get; set; }

    /// <summary>
    /// The default (home) page of the site. Detail only.
    /// </summary>
    public KeyNameResult DefaultPage { get; set; }

    /// <summary>
    /// The login page of the site, when one is configured. Detail only.
    /// </summary>
    public KeyNameResult LoginPage { get; set; }
}
