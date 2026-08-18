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

using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.LavaApplicationSkill;

/// <summary>
/// Trimmed reference to a Lava application, used as the history content for
/// results whose full payload (the endpoint list) is too large to keep in
/// session context.
/// </summary>
internal class LavaApplicationReferenceResult : EntityResultBase
{
    /// <summary>
    /// The name of the application.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The slug the application is addressed by.
    /// </summary>
    public string ApplicationSlug { get; set; }
}
