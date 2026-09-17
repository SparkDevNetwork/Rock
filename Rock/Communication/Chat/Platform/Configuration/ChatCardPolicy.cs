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
using Rock.ViewModels.Blocks.Administration.SparkConnectedServices;

namespace Rock.Communication.Chat.Platform.Configuration
{
    /// <summary>
    /// What the chat card on the Connected Services page shows, and whether it may offer
    /// to set the organization up. Kept apart from the block so both questions can be
    /// answered by a test: the block itself only supplies the stored settings.
    /// </summary>
    /// <remarks>
    /// Both questions read whether the organization has been set up, never whether chat
    /// can run. The two disagree on the organization whose stored signing key this
    /// installation cannot decrypt, and reading the second would offer that organization
    /// an Enable that mints a new key and strands the tenant it already has.
    /// </remarks>
    internal static class ChatCardPolicy
    {
        /// <summary>
        /// The card's state as the browser may see it. The signing key that arrived with
        /// these settings is absent, because a bag that carries a key is a bag that can
        /// leak one; nothing on the card needs it.
        /// </summary>
        /// <param name="stored">The settings as stored.</param>
        /// <returns>A bag safe to hand to a browser.</returns>
        public static ChatConfigurationBag ToBag( ChatPlatformConfiguration stored )
        {
            var configuration = stored ?? new ChatPlatformConfiguration();

            if ( !configuration.HasBeenEnabled )
            {
                return new ChatConfigurationBag { IsEnabled = false };
            }

            return new ChatConfigurationBag
            {
                IsEnabled = true,
                TenantId = configuration.TenantId?.ToString(),
                ProjectUrl = configuration.ProjectUrl
            };
        }

        /// <summary>
        /// Whether this organization may be set up on the chat platform. False once it
        /// has been, because enabling a second time mints a second signing key and
        /// orphans the first, and nothing here can rotate or retire one.
        /// </summary>
        /// <param name="stored">The settings as stored.</param>
        /// <returns><c>true</c> when Enable may be offered and acted on.</returns>
        public static bool MayEnable( ChatPlatformConfiguration stored )
        {
            return !( stored ?? new ChatPlatformConfiguration() ).HasBeenEnabled;
        }
    }
}
