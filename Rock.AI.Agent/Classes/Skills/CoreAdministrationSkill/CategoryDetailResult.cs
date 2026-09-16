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
/// A single category in full detail.
/// </summary>
internal class CategoryDetailResult : EntityResultBase
{
    /// <summary>
    /// The name of the category.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The description of the category.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The order of the category relative to its siblings.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// The CSS class of the icon shown for the category.
    /// </summary>
    public string IconCssClass { get; set; }

    /// <summary>
    /// The highlight color of the category.
    /// </summary>
    public string HighlightColor { get; set; }

    /// <summary>
    /// The entity type this category can be applied to.
    /// </summary>
    public KeyNameResult EntityType { get; set; }

    /// <summary>
    /// The parent category, or <c>null</c> when this is a root category.
    /// </summary>
    public KeyNameResult ParentCategory { get; set; }

    /// <summary>
    /// How many categories sit directly beneath this one. This tells a caller
    /// whether descending further is worthwhile without a second call.
    /// </summary>
    public int ChildCategoryCount { get; set; }
}
