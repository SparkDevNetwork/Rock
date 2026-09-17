-- Chat channels. The WHERE clause is the only definition of "is a chat
-- channel": currently enabled, or stamped the first time it was. Direct
-- messages force public, always-shown and search-indexed off, because Rock
-- does not enforce those and the platform refuses a public DM.

SELECT
    g.[Guid] AS channel_id,
    NULLIF(g.[Name], N'') AS [name],
    CASE WHEN bf.[Guid] IS NULL THEN NULL
         ELSE @Root + N'GetImage.ashx?guid=' + LOWER(CAST(bf.[Guid] AS nvarchar(36)))
              + N'&maxwidth=120&maxheight=120' END AS icon_url,
    CASE WHEN gt.[Guid] = @DmTypeGuid THEN 'dm'
         WHEN gt.[Guid] = @LivestreamTypeGuid THEN 'livestream'
         ELSE 'shared' END AS channel_type,
    CASE WHEN g.IsActive = 0 OR g.IsArchived = 1 OR gt.[Guid] = @DmTypeGuid THEN CAST(0 AS bit)
         ELSE COALESCE(g.IsChatChannelPublicOverride, gt.IsChatChannelPublic) END AS is_public,
    CASE WHEN g.IsActive = 0 OR g.IsArchived = 1 OR gt.[Guid] = @DmTypeGuid THEN CAST(0 AS bit)
         ELSE COALESCE(g.IsChatChannelAlwaysShownOverride, gt.IsChatChannelAlwaysShown) END AS always_shown,
    COALESCE(g.IsLeavingChatChannelAllowedOverride, gt.IsLeavingChatChannelAllowed) AS leave_allowed,
    CASE WHEN gt.[Guid] = @DmTypeGuid THEN CAST(1 AS bit)
         ELSE COALESCE(g.CanViewMembersOverride, gt.CanViewMembers) END AS can_view_members,
    CASE WHEN gt.[Guid] = @DmTypeGuid THEN NULL ELSE c.[Guid] END AS campus_id,
    CASE COALESCE(g.ChatPushNotificationModeOverride, gt.ChatPushNotificationMode)
         WHEN 1 THEN 'mentions' WHEN 2 THEN 'silent' ELSE 'all' END AS notify_mode_default,
    CASE WHEN g.IsActive = 0 OR g.IsArchived = 1 OR gt.[Guid] = @DmTypeGuid THEN CAST(0 AS bit)
         ELSE COALESCE(g.IsChatSearchIndexedOverride, gt.IsChatSearchIndexed) END AS is_search_indexed
FROM dbo.[Group] g
JOIN dbo.GroupType gt ON gt.Id = g.GroupTypeId
LEFT JOIN dbo.Campus c ON c.Id = g.CampusId
LEFT JOIN dbo.BinaryFile bf ON bf.Id = g.ChatChannelAvatarBinaryFileId
WHERE
    g.ChatChannelFirstEnabledDateTime IS NOT NULL
    OR ( gt.IsChatAllowed = 1
         AND COALESCE(g.IsChatEnabledOverride, gt.IsChatEnabledForAllGroups) = 1 );
