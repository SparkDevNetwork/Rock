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

namespace Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;

/// <summary>
/// A single category as it appears in a list.
/// </summary>
internal class CategoryResult : EntityResultBase
{
    /// <summary>
    /// The name of the category.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The parent category, or <c>null</c> when this is a root category. This
    /// stays in the list result because without it a flat rendering of a deep
    /// tree is unreadable.
    /// </summary>
    public KeyNameResult ParentCategory { get; set; }
}
