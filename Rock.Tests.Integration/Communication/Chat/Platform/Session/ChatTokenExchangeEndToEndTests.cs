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
using System.Net.Http;

using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Session;
using Rock.Tests.Integration.Communication.Chat.Platform.Sync;

namespace Rock.Tests.Integration.Communication.Chat.Platform.Session
{
    /// <summary>
    /// A church token Rock signs, exchanged at a real platform.
    /// </summary>
    /// <remarks>
    /// The platform's own tests mint their church tokens in the platform's language, so they agree
    /// with the exchange by construction. This signs with Rock's own code and makes the real call,
    /// so a claim Rock forgets or spells differently shows here.
    /// </remarks>
    [TestClass]
    public class ChatTokenExchangeEndToEndTests
    {
        private static readonly HttpClient _http = new HttpClient();

        /// <summary>
        /// The exchange answers the church's settings beside the token, and the platform token it
        /// grants carries the Rock version Rock signed.
        /// </summary>
        [TestMethod]
        [TestCategory( "ChatPlatformEndToEnd" )]
        public void AnExchangedTokenCarriesTheSettingsAndRocksVersion()
        {
            var platform = LocalChatPlatform.FromEnvironment();
            var configuration = platform.ProvisionChurch();
            var minted = ChatSessionHelper.TryMintSyncToken( new ChatSessionContext { Configuration = configuration } );
            Assert.AreEqual( ChatMintGate.Ok, minted.Gate );

            JObject answer;
            using ( var request = new HttpRequestMessage( HttpMethod.Post, configuration.ProjectUrl + "/functions/v1/token-exchange" ) )
            {
                request.Headers.TryAddWithoutValidation( "apikey", configuration.PublishableKey );
                request.Headers.TryAddWithoutValidation( "Authorization", "Bearer " + minted.ChurchToken );

                using ( var response = _http.SendAsync( request ).GetAwaiter().GetResult() )
                {
                    var text = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    Assert.IsTrue( response.IsSuccessStatusCode, $"the exchange refused Rock's token: HTTP {( int ) response.StatusCode} {text}" );
                    answer = JObject.Parse( text );
                }
            }

            var settings = answer["settings"] as JObject;
            Assert.IsNotNull( settings, "the exchange answers settings beside the token" );
            Assert.IsNotNull( settings["service"]?["state"], "the settings carry the service state" );

            // Read, not verified: the platform verified it, and this only checks what it copied.
            var payload = JObject.Parse( Base64UrlEncoder.Decode( answer["access_token"].ToString().Split( '.' )[1] ) );
            Assert.AreEqual( global::Rock.VersionInfo.VersionInfo.GetRockSemanticVersionNumber(), payload["client_version"]?.ToString() );
        }
    }
}
