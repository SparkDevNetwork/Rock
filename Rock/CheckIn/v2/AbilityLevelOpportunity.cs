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
//

using Rock.ViewModels.CheckIn;

namespace Rock.CheckIn.v2
{
    /// <summary>
    /// A representation of a single ability level.
    /// </summary>
    /// <remarks>
    /// This exists for future compatibility so if we need to add any
    /// properties we don't need to create a new type and invalidate all
    /// existing method signatures.
    /// </remarks>
    internal class AbilityLevelOpportunity : AbilityLevelOpportunityBag
    {
    }
}
