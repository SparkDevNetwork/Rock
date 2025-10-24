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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// Represents a configuration bag for initializing proof-of-work challenges in a CAPTCHA system.
    /// </summary>
    public class CaptchaInitializeProofOfWorkBag
    {
        /// <summary>
        /// Gets or sets the number of challenges to generate.
        /// </summary>
        public int ChallengeCount { get; set; }
        
        /// <summary>
        /// Gets or sets the size of each challenge in bytes.
        /// </summary>
        public int ChallengeSize { get; set; }
        
        /// <summary>
        /// Gets or sets the difficulty level of the challenge.
        /// </summary>
        public int ChallengeDifficulty { get; set; }

        /// <summary>
        /// Gets or sets the challenge token that should be redeemed, along with the challenge solution, for a verified token.
        /// </summary>
        public string ChallengeToken { get; set; }
    }
}
