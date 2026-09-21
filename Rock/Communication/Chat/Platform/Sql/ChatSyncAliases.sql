-- The aliases section, read entirely from the staged sets.
--
-- Two row shapes. A person's primary alias row carries everything about them. Their other alias
-- rows carry the two identifiers and nothing else, which is what lets a client that still holds an
-- old alias resolve it to the person who owns it now. Every alias is restated on every cycle, so a
-- merge in Rock heals itself here with nothing tracking that it happened.
--
-- The last row is chat itself, the author of the messages a conversation generates about its own
-- membership. It is a constant, the same in every installation, and no person in Rock backs it.

SELECT
    [A].[AliasGuid] AS [person_alias_guid],
    [PR].[AliasGuid] AS [primary_person_alias_guid],
    CASE WHEN [A].[IsPrimary] = 1 THEN [P].[NickName] END AS [nick_name],
    CASE WHEN [A].[IsPrimary] = 1 THEN [P].[LastName] END AS [last_name],

    -- A photo behind a binary file type that requires view security is not linked at all, because
    -- the far side serves this URL to every member of every channel the person is in.
    CASE WHEN [A].[IsPrimary] = 1 THEN
        CASE
            WHEN [BF].[Guid] IS NULL OR [BFT].[RequiresViewSecurity] = 1 THEN NULL
            ELSE CAST( @PublicApplicationRoot + N'GetImage.ashx?guid=' + LOWER( CAST( [BF].[Guid] AS NVARCHAR( 36 ) ) ) AS NVARCHAR( 400 ) )
        END
    END AS [avatar_url],

    CASE WHEN [A].[IsPrimary] = 1 THEN [CM].[Guid] END AS [campus_id],

    -- Returned joined, and split into a list before it reaches the wire. The column on the far side
    -- holds a list, and a single string arrives there as no badges at all on a submission that is
    -- otherwise accepted, with nothing reporting it.
    CASE WHEN [A].[IsPrimary] = 1 THEN [BK].[BadgeKeys] END AS [badge_keys],

    CASE WHEN [A].[IsPrimary] = 1 THEN COALESCE( [P].[IsChatProfilePublic], @ProfilesVisibleByDefault ) END AS [show_profile_details],
    CASE WHEN [A].[IsPrimary] = 1 THEN COALESCE( [P].[IsChatOpenDirectMessageAllowed], @OpenDirectMessagesByDefault ) END AS [is_open_dm_allowed],

    CASE WHEN [A].[IsPrimary] = 1 THEN
        CASE WHEN [B].[PersonId] IS NULL THEN CAST( 0 AS BIT ) ELSE CAST( 1 AS BIT ) END
    END AS [is_globally_banned],

    -- Anyone whose record is not active is hidden rather than removed, so reactivating them in Rock
    -- restores every channel and every message they had.
    CASE WHEN [A].[IsPrimary] = 1 THEN
        CASE WHEN [P].[RecordStatusValueId] = @ActiveRecordStatusValueId THEN CAST( 0 AS BIT ) ELSE CAST( 1 AS BIT ) END
    END AS [is_inactive]

FROM #Alias AS [A]
INNER JOIN #Alias AS [PR] ON [PR].[PersonId] = [A].[PersonId] AND [PR].[IsPrimary] = 1
INNER JOIN [Person] AS [P] ON [P].[Id] = [A].[PersonId]
LEFT JOIN [Campus] AS [CM] ON [CM].[Id] = [P].[PrimaryCampusId]
LEFT JOIN [BinaryFile] AS [BF] ON [BF].[Id] = [P].[PhotoId]
LEFT JOIN [BinaryFileType] AS [BFT] ON [BFT].[Id] = [BF].[BinaryFileTypeId]
LEFT JOIN #Badges AS [BK] ON [BK].[PersonId] = [A].[PersonId]
LEFT JOIN (
    SELECT DISTINCT [GM].[PersonId]
    FROM [GroupMember] AS [GM]
    INNER JOIN [Group] AS [G] ON [G].[Id] = [GM].[GroupId]
    WHERE [G].[Guid] = @ChatBanListGroupGuid
        AND [GM].[GroupMemberStatus] = 1
        AND [GM].[IsArchived] = 0
) AS [B] ON [B].[PersonId] = [P].[Id]

UNION ALL

SELECT
    @ChatSystemAuthorGuid,
    @ChatSystemAuthorGuid,
    N'Rock',
    N'Chat',
    CAST( NULL AS NVARCHAR( 400 ) ),
    CAST( NULL AS UNIQUEIDENTIFIER ),
    CAST( NULL AS VARCHAR( MAX ) ),
    CAST( 1 AS BIT ),
    CAST( 0 AS BIT ),
    CAST( 0 AS BIT ),
    CAST( 0 AS BIT );
