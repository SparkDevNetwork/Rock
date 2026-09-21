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

SELECT
    [MR].[ChannelGuid] AS [channel_id],
    [A].[AliasGuid] AS [person_alias_guid],
    [GTR].[IsLeader] AS [is_leader],
    [MR].[IsChatBanned] AS [is_banned],
    [MR].[ChatBannedUntil] AS [ban_expires_at]
FROM #MemberRows AS [MR]
INNER JOIN [GroupTypeRole] AS [GTR] ON [GTR].[Id] = [MR].[GroupRoleId]
INNER JOIN #Alias AS [A] ON [A].[PersonId] = [MR].[PersonId] AND [A].[IsPrimary] = 1;
