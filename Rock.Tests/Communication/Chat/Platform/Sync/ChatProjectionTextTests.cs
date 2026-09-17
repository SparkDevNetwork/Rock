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
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication.Chat.Platform.Sync;

namespace Rock.Tests.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// The projection SQL is the only definition of a chat channel, and it
    /// forces the DM invariants Rock itself does not.
    /// </summary>
    [TestClass]
    public class ChatProjectionTextTests
    {
        [TestMethod]
        public void ChannelsSql_ForcesPublicAlwaysShownAndSearchIndexedOffOnADirectMessage()
        {
            var sql = ChatProjection.ChannelsSql;

            StringAssert.Contains( sql, "gt.[Guid] = @DmTypeGuid THEN CAST(0 AS bit)" );
            StringAssert.Contains( sql, "is_public" );
            StringAssert.Contains( sql, "always_shown" );
            StringAssert.Contains( sql, "is_search_indexed" );
            StringAssert.Contains( sql, "ChatChannelFirstEnabledDateTime IS NOT NULL" );
        }

        [TestMethod]
        public void AliasesSql_ThinRowsAreTheTwoUuids_AndPersonColumnsAreTheShippedNames()
        {
            var sql = ChatProjection.AliasesSql;

            StringAssert.Contains( sql, "CASE WHEN a.IsPrim = 1 THEN p.NickName END" );
            StringAssert.Contains( sql, "IsChatProfilePublic" );
            StringAssert.Contains( sql, "IsChatOpenDirectMessageAllowed" );
            StringAssert.Contains( sql, "UNION ALL" );
            StringAssert.Contains( sql, "@SystemAliasGuid" );
        }

        [TestMethod]
        public void MembersSql_UsesTheSameChannelPredicate()
        {
            StringAssert.Contains( ChatProjection.MembersSql, "ChatChannelFirstEnabledDateTime IS NOT NULL" );
            StringAssert.Contains( ChatProjection.MembersSql, "ChatBannedUntil" );
        }
    }
}
