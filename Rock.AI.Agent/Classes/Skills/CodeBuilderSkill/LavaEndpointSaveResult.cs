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

namespace Rock.AI.Agent.Classes.Skills.CodeBuilderSkill;

/// <summary>
/// Result model for a Lava endpoint that was created or updated, including
/// the outcome of test-executing its template.
/// </summary>
internal class LavaEndpointSaveResult
{
    /// <summary>
    /// The slug of the Lava application the endpoint belongs to.
    /// </summary>
    public string ApplicationSlug { get; set; }

    /// <summary>
    /// The slug of the endpoint that was saved.
    /// </summary>
    public string EndpointSlug { get; set; }

    /// <summary>
    /// The HTTP method the endpoint answers.
    /// </summary>
    public string Method { get; set; }

    /// <summary>
    /// The URL a client uses to call the endpoint.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// The result of test-executing the saved template, or the reason the
    /// test was skipped.
    /// </summary>
    public TestExecutionResult TestExecution { get; set; }
}
