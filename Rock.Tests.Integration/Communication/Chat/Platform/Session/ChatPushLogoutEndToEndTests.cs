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
using System.Linq;
using System.Net.Http;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Session;
using Rock.Communication.Chat.Platform.Sync;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.Communication.Chat.Platform.Sync;
using Rock.Tests.Integration.TestFramework.Database;

namespace Rock.Tests.Integration.Communication.Chat.Platform.Session
{
    /// <summary>
    /// Signing out of Rock takes this browser out of push, against a real chat platform.
    /// </summary>
    /// <remarks>
    /// The unit tests stop at a stand-in transport, which agrees with Rock about whatever Rock
    /// sends. This registers a real device for a person the platform knows, then runs the sign-out
    /// path, so a token Rock signs wrongly or a call the platform refuses shows here.
    /// </remarks>
    [TestClass]
    [TestCategory( "ChatPlatformEndToEnd" )]
    public class ChatPushLogoutEndToEndTests : DatabaseTestsBase
    {
        private static readonly HttpClient _http = new HttpClient();

        [TestMethod]
        public void SigningOutRemovesTheBrowsersDeviceFromThePlatformAndNothingElse()
        {
            var platform = LocalChatPlatform.FromEnvironment();

            using ( var fixture = new ChatSyncProjectionFixture() )
            using ( ChatPlatformSyncHelper.OverrideImmediateSync( new HttpClientHandler() ) )
            {
                var configuration = platform.ProvisionChurch();
                fixture.StoreConfiguration( configuration );

                // A membership is what carries the person to the platform as a live alias, and only
                // a live person may register a device.
                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "Push sign-out channel" );
                var personId = fixture.AddPerson( "SignsOut" );
                var alias = fixture.PrimaryAliasGuid( personId );

                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    var group = new GroupService( rockContext ).Queryable( "GroupType" ).Single( g => g.Guid == channel );
                    new GroupMemberService( rockContext ).Add( new GroupMember
                    {
                        Guid = Guid.NewGuid(),
                        GroupId = group.Id,
                        GroupTypeId = group.GroupTypeId,
                        PersonId = personId,
                        GroupRoleId = group.GroupType.DefaultGroupRoleId.Value,
                        GroupMemberStatus = GroupMemberStatus.Active
                    } );
                    rockContext.SaveChanges();
                }

                Assert.IsNotNull( platform.WaitForAlias( configuration.TenantId.Value, alias, r => r != null ), "the person never reached the platform" );

                using ( var rockContext = new RockContext() )
                {
                    var person = new PersonService( rockContext ).Get( personId );
                    var context = ChatSessionHelper.BuildSessionContext( person, configuration, rockContext );

                    var browserToken = "push-signout-" + Guid.NewGuid().ToString( "N" );
                    var otherDevice = "push-signout-other-" + Guid.NewGuid().ToString( "N" );
                    var platformToken = ExchangePersonToken( person, context, rockContext, configuration.ProjectUrl, configuration.PublishableKey );
                    Register( platformToken, browserToken, configuration.ProjectUrl, configuration.PublishableKey );
                    Register( platformToken, otherDevice, configuration.ProjectUrl, configuration.PublishableKey );

                    // The path sign-out runs in the background, which reads the person and the
                    // church's settings itself.
                    Assert.AreEqual( ChatPushUnregisterOutcome.Removed, ChatSessionHelper.UnregisterPushDeviceForPerson( personId, browserToken ) );

                    // Asked again, the platform no longer holds it; the person's other device stays.
                    Assert.AreEqual( ChatPushUnregisterOutcome.NotHeld, ChatSessionHelper.UnregisterPushDeviceForPerson( personId, browserToken ) );
                    Assert.AreEqual( ChatPushUnregisterOutcome.Removed, ChatSessionHelper.UnregisterPushDeviceForPerson( personId, otherDevice ) );
                }
            }
        }

        #region Support

        /// <summary>
        /// Signs a person token with Rock's own code and exchanges it, as the Chat block does.
        /// </summary>
        private static string ExchangePersonToken( Person person, ChatSessionContext context, RockContext rockContext, string projectUrl, string publishableKey )
        {
            var minted = ChatSessionHelper.TryMintChurchToken( person, context, rockContext );
            Assert.AreEqual( ChatMintGate.Ok, minted.Gate, "the person signing out passes the gates" );

            var answer = Post( projectUrl + "/functions/v1/token-exchange", minted.ChurchToken, publishableKey, null );
            return answer["access_token"].ToString();
        }

        /// <summary>
        /// Registers a web device under the person's platform token, as the browser does.
        /// </summary>
        private static void Register( string platformToken, string deviceToken, string projectUrl, string publishableKey )
        {
            var body = new JObject { ["p_token"] = deviceToken, ["p_platform"] = "web" };
            Post( projectUrl + "/rest/v1/rpc/chat_register_device", platformToken, publishableKey, body );
        }

        private static JObject Post( string url, string bearer, string publishableKey, JObject body )
        {
            using ( var request = new HttpRequestMessage( HttpMethod.Post, url ) )
            {
                request.Headers.TryAddWithoutValidation( "apikey", publishableKey );
                request.Headers.TryAddWithoutValidation( "Authorization", "Bearer " + bearer );
                request.Content = new StringContent( body?.ToString() ?? "{}", Encoding.UTF8, "application/json" );

                using ( var response = _http.SendAsync( request ).GetAwaiter().GetResult() )
                {
                    var text = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    Assert.IsTrue( response.IsSuccessStatusCode, $"{url} refused: HTTP {( int ) response.StatusCode} {text}" );
                    return JObject.Parse( text );
                }
            }
        }

        #endregion Support
    }
}
