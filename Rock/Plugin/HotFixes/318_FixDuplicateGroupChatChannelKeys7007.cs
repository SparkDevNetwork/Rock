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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Clears the chat channel key on groups that inherited another group's key via the Copy Group
    /// action, so each affected group can be assigned its own unique chat channel. Fix for issue #7007.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 318, "19.5" )]
    public class FixDuplicateGroupChatChannelKeys7007 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /*
                8/31/2026 - NA

                The "Copy Group" button previously cloned the source group's ChatChannelKey onto the new
                group, so copies shared the source group's external chat channel. That caused the Chat Sync
                job to add members to the wrong group's roster and to throw "An item with the same key has
                already been added". The code fix (GroupService.GenerateGroupCopy) stops new copies from
                getting the same key, but existing databases still hold the duplicated keys, and there is
                no UI to correct them.

                A Rock-originated key encodes the group's OWN Id in the form rock-group-{Id}. Any group whose
                key is rock-group-{n} with n <> its own Id received that key from the source group during a
                copy. This migration nulls those copies so the next Chat Sync assigns each its own
                rock-group-{ownId} key and channel. Within a shared-key set, the true owner (Id = embedded
                Id) is left untouched, keeping its existing channel and message history.

                We intentionally limit this to keys that are CURRENTLY shared by more than one group. A
                group with a mismatched key that is NOT shared with a live group is syncing correctly
                against its existing channel; nulling it would migrate it to a new empty channel and cause
                the old channel (and its message history) to be deleted as an orphan by the Chat-to-Rock
                sync. Those lone survivors are left alone.

                Non-integer suffixes (channels that originated in the external chat system) are excluded by
                TRY_CONVERT, so only true Rock-origin keys with a mismatched Id are cleared.

                Reason: https://github.com/SparkDevNetwork/Rock/issues/7007
            */

            Sql( @"
;WITH Parsed AS (
    SELECT
        [Id],
        [ChatChannelKey],
        TRY_CONVERT( INT, SUBSTRING( [ChatChannelKey], LEN( 'rock-group-' ) + 1, 50 ) ) AS [EmbeddedId]
    FROM [Group]
    WHERE [ChatChannelKey] LIKE 'rock-group-%'
        AND TRY_CONVERT( INT, SUBSTRING( [ChatChannelKey], LEN( 'rock-group-' ) + 1, 50 ) ) IS NOT NULL
),
DuplicateKeys AS (
    SELECT [ChatChannelKey]
    FROM Parsed
    GROUP BY [ChatChannelKey]
    HAVING COUNT(*) > 1
)
UPDATE g
SET g.[ChatChannelKey] = NULL
FROM [Group] g
INNER JOIN Parsed p ON p.[Id] = g.[Id]
INNER JOIN DuplicateKeys d ON d.[ChatChannelKey] = p.[ChatChannelKey]
WHERE p.[EmbeddedId] <> g.[Id];
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //
        }
    }
}
