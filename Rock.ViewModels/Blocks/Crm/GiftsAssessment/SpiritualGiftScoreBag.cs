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

namespace Rock.ViewModels.Blocks.Crm.GiftsAssessment
{
    /// <summary>
    /// Represents the ranked score for a single spiritual gift.
    /// </summary>
    [Serializable]
    public class SpiritualGiftScoreBag
    {
        /// <summary>
        /// Gets or sets the name of the spiritual gift.
        /// </summary>
        public string SpiritualGiftName { get; set; }

        /// <summary>
        /// Gets or sets the percentage score (0-100) for the spiritual gift.
        /// </summary>
        public double Percentage { get; set; }
    }
}
