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
using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication.Chat.Platform.Sync;
using Rock.Data;
using Rock.Tests.Integration.TestFramework.Database;

namespace Rock.Tests.Integration.Communication.Chat.Platform
{
    /// <summary>
    /// Projection against SQL Server. The unit suite holds the SQL as text;
    /// these cells need a Rock database that has the restatement columns.
    /// </summary>
    [TestClass]
    public class ChatProjectionTests : DatabaseTestsBase
    {
        [TestMethod]
        public void EmbeddedProjectionSql_IsWhatTheJobRuns()
        {
            Assert.IsFalse( string.IsNullOrWhiteSpace( ChatProjection.ChannelsSql ) );
            Assert.IsFalse( string.IsNullOrWhiteSpace( ChatProjection.MembersSql ) );
            Assert.IsFalse( string.IsNullOrWhiteSpace( ChatProjection.AliasesSql ) );
            StringAssert.Contains( ChatProjection.ChannelsSql, "gt.[Guid] = @DmTypeGuid THEN CAST(0 AS bit)" );
            StringAssert.Contains( ChatProjection.ChannelsSql, "ChatChannelFirstEnabledDateTime IS NOT NULL" );
        }

        [TestMethod]
        public void IdentityMarks_AreReadAsIdentCurrent_NotMaxId()
        {
            using ( var rockContext = new RockContext() )
            {
                var marks = ChatProjection.ReadIdentityMarks( rockContext );
                CollectionAssert.AreEquivalent(
                    new[] { "person", "person_alias", "group", "group_member" },
                    System.Linq.Enumerable.ToArray( marks.Keys ) );
                foreach ( var value in marks.Values )
                {
                    Assert.IsGreaterThanOrEqualTo( 0L, value );
                }
            }
        }
    }
}
