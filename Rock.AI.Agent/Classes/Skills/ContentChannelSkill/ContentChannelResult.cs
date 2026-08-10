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

namespace Rock.AI.Agent.Classes.Skills.ContentChannelSkill;

/// <summary>
/// Represents a single content channel.
/// </summary>
internal class ContentChannelResult : EntityResultBase
{
    /// <summary>
    /// The name of the content channel.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The type of content channel for this instance.
    /// </summary>
    public ContentChannelTypeResult ContentChannelType { get; set; }

    /// <summary>
    /// A longer description of the content channel for showing internally
    /// in the UI.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Determines if items in this channel require approval before they are
    /// visible to the public.
    /// </summary>
    public bool RequiresApproval { get; set; }
}
