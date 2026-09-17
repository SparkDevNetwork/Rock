-- Chat aliases. Enrolled people: Chat People (any status), Ban List and Chat
-- Administrators (active), and members of any chat channel. Non-primary rows
-- carry the two uuids and nulls. The system chat person is a constant row.

IF OBJECT_ID('tempdb..#enrolled') IS NOT NULL DROP TABLE #enrolled;
SELECT PersonId INTO #enrolled FROM (
    SELECT gm.PersonId
      FROM dbo.GroupMember gm
      JOIN dbo.[Group] g ON g.Id = gm.GroupId
     WHERE g.[Guid] = @ChatPeopleGuid
    UNION
    SELECT gm.PersonId
      FROM dbo.GroupMember gm
      JOIN dbo.[Group] g ON g.Id = gm.GroupId
     WHERE g.[Guid] IN (@BanListGuid, @ChatAdminsGuid)
       AND gm.GroupMemberStatus = 1 AND gm.IsArchived = 0
    UNION
    SELECT gm.PersonId
      FROM dbo.GroupMember gm
      JOIN dbo.[Group] g ON g.Id = gm.GroupId
      JOIN dbo.GroupType gt ON gt.Id = g.GroupTypeId
     WHERE gm.GroupMemberStatus = 1 AND gm.IsArchived = 0
       AND ( g.ChatChannelFirstEnabledDateTime IS NOT NULL
             OR ( gt.IsChatAllowed = 1
                  AND COALESCE(g.IsChatEnabledOverride, gt.IsChatEnabledForAllGroups) = 1 ) )
) q;
CREATE UNIQUE CLUSTERED INDEX ix ON #enrolled(PersonId);

IF OBJECT_ID('tempdb..#alias') IS NOT NULL DROP TABLE #alias;
SELECT pa.PersonId, pa.[Guid],
       CAST(CASE WHEN ROW_NUMBER() OVER (PARTITION BY pa.PersonId
              ORDER BY CASE WHEN pa.Id = p.PrimaryAliasId THEN 0 ELSE 1 END, pa.Id) = 1
            THEN 1 ELSE 0 END AS bit) AS IsPrim
  INTO #alias
  FROM dbo.PersonAlias pa
  JOIN dbo.Person p ON p.Id = pa.PersonId
  JOIN #enrolled e ON e.PersonId = pa.PersonId;
CREATE CLUSTERED INDEX ix ON #alias(PersonId);

IF OBJECT_ID('tempdb..#badge_guids') IS NOT NULL DROP TABLE #badge_guids;
SELECT n.x.value('.', 'uniqueidentifier') AS DvGuid
  INTO #badge_guids
  FROM @BadgeGuidsXml.nodes('/g') n(x);
CREATE UNIQUE CLUSTERED INDEX ix ON #badge_guids(DvGuid);

IF OBJECT_ID('tempdb..#badges') IS NOT NULL DROP TABLE #badges;
SELECT bp.PersonId,
       STUFF((SELECT ',' + LOWER(CAST(dv.[Guid] AS varchar(36)))
                FROM dbo.DataViewPersistedValue d2
                JOIN dbo.DataView dv ON dv.Id = d2.DataViewId
                JOIN #badge_guids bg ON bg.DvGuid = dv.[Guid]
               WHERE d2.EntityId = bp.PersonId
                 AND dv.PersistedScheduleIntervalMinutes IS NOT NULL
               FOR XML PATH(''), TYPE).value('.', 'varchar(max)'), 1, 1, '') AS badge_keys
  INTO #badges
  FROM ( SELECT DISTINCT dvpv.EntityId AS PersonId
           FROM dbo.DataViewPersistedValue dvpv
           JOIN dbo.DataView dv ON dv.Id = dvpv.DataViewId
           JOIN #badge_guids bg ON bg.DvGuid = dv.[Guid]
          WHERE dv.PersistedScheduleIntervalMinutes IS NOT NULL ) bp;
CREATE CLUSTERED INDEX ix ON #badges(PersonId);

SELECT
    a.[Guid] AS person_alias_guid,
    pr.[Guid] AS primary_person_alias_guid,
    CASE WHEN a.IsPrim = 1 THEN p.NickName END AS nick_name,
    CASE WHEN a.IsPrim = 1 THEN p.LastName END AS last_name,
    CASE WHEN a.IsPrim = 1 THEN
        CASE WHEN bf.[Guid] IS NULL OR bft.RequiresViewSecurity = 1 THEN NULL
             ELSE CAST(@Root + N'GetImage.ashx?guid=' + LOWER(CAST(bf.[Guid] AS nvarchar(36))) AS nvarchar(400)) END
    END AS avatar_url,
    CASE WHEN a.IsPrim = 1 THEN cm.[Guid] END AS campus_id,
    CASE WHEN a.IsPrim = 1 THEN bk.badge_keys END AS badge_keys,
    CASE WHEN a.IsPrim = 1
         THEN COALESCE(p.IsChatProfilePublic, @ShowProfileDefault) END AS show_profile_details,
    CASE WHEN a.IsPrim = 1
         THEN COALESCE(p.IsChatOpenDirectMessageAllowed, @OpenDmDefault) END AS is_open_dm_allowed,
    CASE WHEN a.IsPrim = 1
         THEN CASE WHEN b.PersonId IS NULL THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END END AS is_globally_banned,
    CASE WHEN a.IsPrim = 1
         THEN CASE WHEN p.RecordStatusValueId = @RecordStatusActiveId THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END END AS is_inactive
FROM #alias a
JOIN #alias pr ON pr.PersonId = a.PersonId AND pr.IsPrim = 1
JOIN dbo.Person p ON p.Id = a.PersonId
LEFT JOIN dbo.Campus cm ON cm.Id = p.PrimaryCampusId
LEFT JOIN dbo.BinaryFile bf ON bf.Id = p.PhotoId
LEFT JOIN dbo.BinaryFileType bft ON bft.Id = bf.BinaryFileTypeId
LEFT JOIN #badges bk ON bk.PersonId = a.PersonId
LEFT JOIN ( SELECT DISTINCT gm.PersonId
              FROM dbo.GroupMember gm
              JOIN dbo.[Group] g ON g.Id = gm.GroupId
             WHERE g.[Guid] = @BanListGuid AND gm.GroupMemberStatus = 1 AND gm.IsArchived = 0 ) b
       ON b.PersonId = p.Id

UNION ALL

SELECT
    @SystemAliasGuid, @SystemAliasGuid,
    N'Rock', N'Chat',
    CAST(NULL AS nvarchar(400)),
    CAST(NULL AS uniqueidentifier),
    CAST(NULL AS varchar(max)),
    CAST(1 AS bit), CAST(0 AS bit), CAST(0 AS bit), CAST(0 AS bit);
