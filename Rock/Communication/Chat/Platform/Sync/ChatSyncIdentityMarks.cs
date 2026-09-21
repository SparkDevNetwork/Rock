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
namespace Rock.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// The identity high-water value of each table the projection reads, sent with every
    /// submission so the platform can tell a restored database from a live one.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These are identity seeds, not maximum ids. The maximum drops when the newest rows are
    /// deleted and the seed does not, so a church that deletes the person it just created would
    /// otherwise look like a database restored from an older backup and be refused on every cycle
    /// until someone intervened. A restore does carry the seed backwards with the table metadata,
    /// which is the case this exists to catch.
    /// </para>
    /// <para>
    /// It does not catch a live clone. A staging copy starts with values equal to production's, and
    /// equal is not backwards; worse, staging advances its own as people test on it, so production
    /// is the one that ends up refused. Staging installations are kept off chat or pointed at a
    /// staging project instead.
    /// </para>
    /// </remarks>
    internal sealed class ChatSyncIdentityMarks
    {
        #region Properties

        /// <summary>
        /// The identity high-water value of the person table.
        /// </summary>
        public long Person { get; set; }

        /// <summary>
        /// The identity high-water value of the person alias table.
        /// </summary>
        public long PersonAlias { get; set; }

        /// <summary>
        /// The identity high-water value of the group table.
        /// </summary>
        public long Group { get; set; }

        /// <summary>
        /// The identity high-water value of the group member table.
        /// </summary>
        public long GroupMember { get; set; }

        #endregion
    }
}
