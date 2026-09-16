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

namespace Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;

/// <summary>
/// A single authorization rule for one action on an entity.
/// </summary>
/// <remarks>
/// Rules are evaluated in <see cref="Order"/>; the first rule that applies to a
/// person settles allow or deny. An inherited rule comes from a parent authority
/// (such as the entity's category) and cannot be edited here; edit the parent to
/// change it.
/// </remarks>
internal class AuthorizationRuleResult : EntityResultBase
{
    /// <summary>
    /// The action this rule governs, such as View, Edit, or Administrate.
    /// </summary>
    public string Action { get; set; }

    /// <summary>
    /// Whether the rule grants or denies access.
    /// </summary>
    public AllowOrDeny AllowOrDeny { get; set; }

    /// <summary>
    /// The evaluation order relative to the other rules for this action. Lower
    /// wins.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Who the rule applies to.
    /// </summary>
    public AuthorizationSubjectResult Subject { get; set; }

    /// <summary>
    /// Indicates that the rule is inherited from a parent authority rather than
    /// set on this entity. Inherited rules are read-only here.
    /// </summary>
    public bool IsInherited { get; set; }

    /// <summary>
    /// The parent authority an inherited rule comes from, such as the category or
    /// parent entity. Null for a rule set directly on this entity.
    /// </summary>
    public string InheritedFrom { get; set; }
}
