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
using Rock.Model;

namespace Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;

/// <summary>
/// The target of an authorization rule. A rule targets exactly one of three
/// things: a special role, a specific person, or a security role group.
/// </summary>
internal class AuthorizationSubjectResult
{
    /// <summary>
    /// Which kind of subject the rule targets: <c>SpecialRole</c>, <c>Person</c>,
    /// or <c>Group</c>.
    /// </summary>
    public string Kind { get; set; }

    /// <summary>
    /// The special role, when <see cref="Kind"/> is <c>SpecialRole</c>.
    /// </summary>
    public SpecialRole? SpecialRole { get; set; }

    /// <summary>
    /// The person, when <see cref="Kind"/> is <c>Person</c>.
    /// </summary>
    public KeyNameResult Person { get; set; }

    /// <summary>
    /// The security role group, when <see cref="Kind"/> is <c>Group</c>.
    /// </summary>
    public KeyNameResult Group { get; set; }
}
