-- The memberships section, read entirely from the staged sets.
--
-- Every row's channel is in #ChatGroups and every row's person has an alias in #Alias, both by
-- construction rather than by timing, which is the whole reason those sets are staged. The far side
-- keys a membership to a channel and to an alias, so a row whose channel or person is missing from
-- this same payload fails the entire submission rather than that one row.
--
-- The ban expiry is returned as Rock stores it, in the organisation's own time zone. It is
-- converted to UTC before it reaches the wire, because a time sent without a zone is read on the
-- far side as UTC and would be wrong by this church's offset, in the direction that lifts a ban
-- early for any church behind it.
--
-- A person may hold several roles in one group, each its own group member, and the far side keeps
-- one row for each person in each channel. So the rows are folded: a leader in any role leads, a
-- ban in any role bans, and of the banned roles a ban without an end outlasts every dated one.

SELECT
    [F].[ChannelGuid] AS [channel_id],
    [A].[AliasGuid] AS [person_alias_guid],
    [F].[IsLeader] AS [is_leader],
    [F].[IsBanned] AS [is_banned],
    [F].[BanExpiresAt] AS [ban_expires_at]
FROM (
    SELECT
        [MR].[PersonId],
        [MR].[ChannelGuid],
        CAST( MAX( CAST( [GTR].[IsLeader] AS INT ) ) AS BIT ) AS [IsLeader],
        CAST( MAX( CAST( [MR].[IsChatBanned] AS INT ) ) AS BIT ) AS [IsBanned],
        CASE
            WHEN MAX( CASE WHEN [MR].[IsChatBanned] = 1 AND [MR].[ChatBannedUntil] IS NULL THEN 1 ELSE 0 END ) = 1 THEN NULL
            ELSE MAX( CASE WHEN [MR].[IsChatBanned] = 1 THEN [MR].[ChatBannedUntil] END )
        END AS [BanExpiresAt]
    FROM #MemberRows AS [MR]
    INNER JOIN [GroupTypeRole] AS [GTR] ON [GTR].[Id] = [MR].[GroupRoleId]
    GROUP BY [MR].[PersonId], [MR].[ChannelGuid]
) AS [F]
INNER JOIN #Alias AS [A] ON [A].[PersonId] = [F].[PersonId] AND [A].[IsPrimary] = 1;
