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

namespace Rock.AI.Agent.Classes.Skills.LavaDataSkill;

/// <summary>
/// The outcome of rendering an endpoint's template so the agent can see
/// whether it works before anyone visits the page.
/// </summary>
internal class TestExecutionResult
{
    /// <summary>
    /// Whether the template rendered without an error. This is <c>false</c>
    /// when <see cref="IsSkipped"/> is <c>true</c>, because nothing was
    /// rendered; it does not mean the template is broken.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Whether the template was deliberately not executed because it can
    /// write. Distinguishes "we did not look" from "we looked and it failed".
    /// </summary>
    public bool IsSkipped { get; set; }

    /// <summary>
    /// The rendered output when <see cref="IsSuccess"/> is <c>true</c>. This
    /// is only the first part of it when <see cref="IsOutputTruncated"/> is
    /// <c>true</c>.
    /// </summary>
    public string Output { get; set; }

    /// <summary>
    /// How many characters the template actually produced, which is larger
    /// than the length of <see cref="Output"/> when the output was truncated.
    /// </summary>
    public int? OutputLength { get; set; }

    /// <summary>
    /// Whether <see cref="Output"/> was cut short, so the agent does not
    /// mistake the visible tail for where the template stopped producing text.
    /// </summary>
    public bool IsOutputTruncated { get; set; }

    /// <summary>
    /// How to see the rest of the output when it was truncated.
    /// </summary>
    public string TruncationAdvice { get; set; }

    /// <summary>
    /// The reason the render failed when <see cref="IsSuccess"/> is <c>false</c>.
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// What the test did and did not exercise, so the agent does not
    /// over-trust a passing render.
    /// </summary>
    public string Coverage { get; set; }
}
