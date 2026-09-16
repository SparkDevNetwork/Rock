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

namespace Rock.ViewModels.Blocks.Crm.GiftsAssessment
{
    /// <summary>
    /// Contains the results of the Spiritual Gifts assessment, grouped by dominance and including the ranked scores.
    /// </summary>
    [Serializable]
    public class SpiritualGiftsResultBag
    {
        /// <summary>
        /// Gets or sets the dominant spiritual gifts (the individual's top gifts).
        /// </summary>
        public List<SpiritualGiftBag> DominantGifts { get; set; }

        /// <summary>
        /// Gets or sets the supportive spiritual gifts.
        /// </summary>
        public List<SpiritualGiftBag> SupportiveGifts { get; set; }

        /// <summary>
        /// Gets or sets the other (remaining) spiritual gifts.
        /// </summary>
        public List<SpiritualGiftBag> OtherGifts { get; set; }

        /// <summary>
        /// Gets or sets the score for every spiritual gift, used to render the ranked gifts chart.
        /// </summary>
        public List<SpiritualGiftScoreBag> GiftScores { get; set; }
    }
}
