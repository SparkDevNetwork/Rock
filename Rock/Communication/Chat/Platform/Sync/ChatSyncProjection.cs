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

namespace Rock.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// The queries that read a church's chat rows out of Rock.
    /// </summary>
    /// <remarks>
    /// <para>
    /// There is one staging query and one query per payload section, and they run in that order on
    /// one connection, because the staging query leaves its results in temporary tables that the
    /// other four read.
    /// </para>
    /// <para>
    /// Splitting it that way is not only tidiness. The question "is this group a chat channel" is
    /// asked once, in the staging query, so there is one definition of it rather than four. Rock
    /// disagrees with itself about that question in two places today, and the answer decides which
    /// rows exist at all.
    /// </para>
    /// <para>
    /// It also closes a failure the four queries would otherwise share. They are separate
    /// statements seconds apart, and the far side keys a membership to both its channel and its
    /// person. A group that starts qualifying, or a person who joins a group, between two of those
    /// statements puts a membership in the payload whose channel or whose person is in no other
    /// section of it, which fails the whole submission rather than that one row, on every retry.
    /// Reading the frozen sets makes the containment hold by construction.
    /// </para>
    /// </remarks>
    internal static class ChatSyncProjection
    {
        #region Fields

        /// <summary>
        /// The columns that decide whether a group is a chat channel. They belong to the staging
        /// query and to nothing else.
        /// </summary>
        internal static readonly string[] QualificationColumns = new[]
        {
            "ChatChannelFirstEnabledDateTime",
            "IsChatAllowed",
            "IsChatEnabledOverride",
            "IsChatEnabledForAllGroups"
        };

        #endregion

        #region Methods

        /// <summary>
        /// The query that stages the sets the section queries read.
        /// </summary>
        /// <returns>The query text.</returns>
        public static string GetStagingSql()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The query that reads one payload section.
        /// </summary>
        /// <param name="section">The payload section, as the wire contract names it.</param>
        /// <returns>The query text.</returns>
        public static string GetSectionSql( string section )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The column aliases a section query returns, in the order it returns them.
        /// </summary>
        /// <param name="section">The payload section.</param>
        /// <returns>The aliases.</returns>
        public static IList<string> GetSectionColumns( string section )
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
