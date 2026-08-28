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

using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.LavaApplicationBuilderSkill;

/// <summary>
/// Result model for a Lava application, returned by GetLavaApplication and
/// AddOrUpdateLavaApplication. The IdKey is what AddOrUpdateLavaApplication
/// takes to update the application later.
/// </summary>
internal class LavaApplicationDetailResult : EntityResultBase
{
    /// <summary>
    /// The name of the application.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The slug the application is addressed by, both in endpoint tool calls
    /// and in the component's useLavaApp binding.
    /// </summary>
    public string ApplicationSlug { get; set; }

    /// <summary>
    /// What the application is for.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Whether the application and its endpoints can be called.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// The endpoints the application contains, summarized. Read one in full
    /// with GetLavaEndpoint.
    /// </summary>
    public List<LavaEndpointSummaryResult> Endpoints { get; set; }
}
