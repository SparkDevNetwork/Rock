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

namespace Rock.AI.Agent.Classes.Skills.PageSkill;

/// <summary>
/// Result model for a block added to a page by the AddBlock tool. The IdKey
/// is the block id to pass to the CustomComponent skill's
/// AddOrUpdateCustomComponent tool.
/// </summary>
internal class AddBlockResult : EntityResultBase
{
    /// <summary>
    /// The zone on the page the block was placed in.
    /// </summary>
    public string Zone { get; set; }

    /// <summary>
    /// The relative URL of the page the block was added to.
    /// </summary>
    public string PageUrl { get; set; }
}
