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

namespace Rock.Utility.CaptchaApi
{
    /// <summary>
    /// Represents the result of initializing a CAPTCHA proof-of-work challenge, including the challenge token and parameters.
    /// </summary>
    internal sealed class CaptchaInitializeProofOfWorkResult
    {
        /// <summary>
        /// Gets or sets the challenge token used for authentication purposes.
        /// </summary>
        internal string ChallengeToken { get; set; }

        /// <summary>
        /// Gets or sets the number of challenges attempted.
        /// </summary>
        internal int ChallengeCount { get; set; }

        /// <summary>
        /// Gets or sets the size of the challenge.
        /// </summary>
        internal int ChallengeSize { get; set; }

        /// <summary>
        /// Gets or sets the difficulty level of the challenge.
        /// </summary>
        internal int ChallengeDifficulty { get; set; }
    }
}
