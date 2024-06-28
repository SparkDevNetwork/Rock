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

namespace Rock.Enums.CheckIn
{
    /// <summary>
    /// The way the ability level will be asked of the individual during
    /// the check-in process.
    /// </summary>
    public enum AbilityLevelDeterminationMode
    {
        /// <summary>
        /// The individual will be asked as a part of each check-in.
        /// </summary>
        Ask = 0,

        /// <summary>
        /// Never ask for an ability level during check-in.
        /// </summary>
        DoNotAsk = 1,

        /// <summary>
        /// Only ask for an ability level if they already have one on file.
        /// This lets them update an ability level but not set it initially.
        /// </summary>
        DoNotAskIfThereIsNoAbilityLevel = 2
    }
}
