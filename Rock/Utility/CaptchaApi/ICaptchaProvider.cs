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

using System.Threading.Tasks;

namespace Rock.Utility.CaptchaApi
{
    /// <summary>
    /// Provides methods for initializing, verifying, and validating CAPTCHA tokens.
    /// </summary>
    internal interface ICaptchaProvider
    {
        /// <summary>
        /// Initializes the captcha system and returns the result of the initialization process.
        /// </summary>
        Task<CaptchaInitializeResult> InitializeAsync();

        /// <summary>
        /// Verifies the provided CAPTCHA and returns the result.
        /// </summary>
        Task<CaptchaVerifyResult> VerifyAsync( CaptchaVerifyOptions options );

        /// <summary>
        /// Determines whether the specified token is valid.
        /// </summary>
        /// <param name="token">The token to validate.</param>
        Task<bool> IsTokenValidAsync( string token );
    }
}
