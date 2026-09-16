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

using System;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Crm.ConflictProfile
{
    /// <summary>
    /// Contains the scored results of a completed Conflict Profile assessment.
    /// </summary>
    [Serializable]
    public class ConflictProfileResultBag
    {
        /// <summary>
        /// Gets or sets the personalized greeting shown above the results.
        /// </summary>
        public string Greeting { get; set; }

        /// <summary>
        /// Gets or sets the five conflict engagement mode scores (Winning, Resolving, Compromising, Avoiding, Yielding).
        /// </summary>
        public List<ConflictProfileScoreBag> Modes { get; set; }

        /// <summary>
        /// Gets or sets the three conflict engagement theme scores (Solving, Accommodating, Winning).
        /// </summary>
        public List<ConflictProfileScoreBag> Themes { get; set; }
    }
}
