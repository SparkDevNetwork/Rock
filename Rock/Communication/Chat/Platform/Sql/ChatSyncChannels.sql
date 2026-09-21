-- The channels section, read entirely from the staged set.
--
-- The columns are selected in the order the wire contract lists them and in no other, because rows
-- travel as positional arrays: two columns of the same type swapped here shift every value one
-- place and nothing on the far side can see it.
--
-- Direct messages have five settings forced rather than read. The far side refuses a direct message
-- that is public, always shown, search indexed or campused, and it refuses any channel with no
-- name, and Rock enforces none of those five. One administrator editing one direct message would
-- otherwise fail this church's whole submission on every cycle until someone found it.

SELECT
    [G].[Guid] AS [channel_id],

    -- Never null. The far side requires a name on anything that is not a direct message, and a
    -- group whose name is blank would fail the entire submission rather than that one row.
    COALESCE( NULLIF( LTRIM( RTRIM( [G].[Name] ) ), N'' ), N'Channel ' + CAST( [G].[Id] AS NVARCHAR( 20 ) ) ) AS [name],

    CASE
        WHEN [BF].[Guid] IS NULL THEN NULL
        ELSE @PublicApplicationRoot + N'GetImage.ashx?guid=' + LOWER( CAST( [BF].[Guid] AS NVARCHAR( 36 ) ) ) + N'&maxwidth=120&maxheight=120'
    END AS [icon_url],

    CASE WHEN [GT].[Guid] = @DirectMessageGroupTypeGuid THEN 'dm' ELSE 'shared' END AS [channel_type],

    -- A channel that is archived or deactivated keeps its row and loses its reach: it is not
    -- public, not always shown and not searchable, so it stops appearing to anyone who was not
    -- already in it while its history stays intact.
    CASE
        WHEN [GT].[Guid] = @DirectMessageGroupTypeGuid OR [G].[IsActive] = 0 OR [G].[IsArchived] = 1 THEN CAST( 0 AS BIT )
        ELSE COALESCE( [G].[IsChatChannelPublicOverride], [GT].[IsChatChannelPublic] )
    END AS [is_public],

    CASE
        WHEN [GT].[Guid] = @DirectMessageGroupTypeGuid OR [G].[IsActive] = 0 OR [G].[IsArchived] = 1 THEN CAST( 0 AS BIT )
        ELSE COALESCE( [G].[IsChatChannelAlwaysShownOverride], [GT].[IsChatChannelAlwaysShown] )
    END AS [always_shown],

    COALESCE( [G].[IsLeavingChatChannelAllowedOverride], [GT].[IsLeavingChatChannelAllowed] ) AS [leave_allowed],

    -- A direct message has no roster to hide, so the setting does not apply to it.
    CASE
        WHEN [GT].[Guid] = @DirectMessageGroupTypeGuid THEN CAST( 1 AS BIT )
        ELSE COALESCE( [G].[CanViewMembersOverride], [GT].[CanViewMembers] )
    END AS [can_view_members],

    CASE WHEN [GT].[Guid] = @DirectMessageGroupTypeGuid THEN NULL ELSE [C].[Guid] END AS [campus_id],

    -- Rock numbers these modes and the wire names them, so the mapping is the thing that can be
    -- wrong. A test holds it against the values the contract publishes.
    CASE COALESCE( [G].[ChatPushNotificationModeOverride], [GT].[ChatPushNotificationMode] )
        WHEN 1 THEN 'mentions'
        WHEN 2 THEN 'silent'
        ELSE 'all'
    END AS [notify_mode_default],

    CASE
        WHEN [GT].[Guid] = @DirectMessageGroupTypeGuid OR [G].[IsActive] = 0 OR [G].[IsArchived] = 1 THEN CAST( 0 AS BIT )
        ELSE COALESCE( [G].[IsChatSearchIndexedOverride], [GT].[IsChatSearchIndexed] )
    END AS [is_search_indexed]

FROM #ChatGroups AS [CG]
INNER JOIN [Group] AS [G] ON [G].[Id] = [CG].[GroupId]
INNER JOIN [GroupType] AS [GT] ON [GT].[Id] = [G].[GroupTypeId]
LEFT JOIN [Campus] AS [C] ON [C].[Id] = [G].[CampusId]
LEFT JOIN [BinaryFile] AS [BF] ON [BF].[Id] = [G].[ChatChannelAvatarBinaryFileId];
