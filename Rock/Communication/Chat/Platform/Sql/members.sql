-- Chat channel members. Live channels only (active, not archived, currently
-- a chat channel). The same channel predicate as channels.sql, plus activity.

IF OBJECT_ID('tempdb..#live_channels') IS NOT NULL DROP TABLE #live_channels;
SELECT g.Id, g.[Guid] AS channel_id
  INTO #live_channels
  FROM dbo.[Group] g
  JOIN dbo.GroupType gt ON gt.Id = g.GroupTypeId
 WHERE ( g.ChatChannelFirstEnabledDateTime IS NOT NULL
         OR ( gt.IsChatAllowed = 1
              AND COALESCE(g.IsChatEnabledOverride, gt.IsChatEnabledForAllGroups) = 1 ) )
   AND g.IsActive = 1
   AND g.IsArchived = 0;
CREATE UNIQUE CLUSTERED INDEX ix ON #live_channels(Id);

IF OBJECT_ID('tempdb..#member_rows') IS NOT NULL DROP TABLE #member_rows;
SELECT gm.PersonId, gm.GroupRoleId, gm.IsChatBanned, gm.ChatBannedUntil, lc.channel_id
  INTO #member_rows
  FROM dbo.GroupMember gm
  JOIN #live_channels lc ON lc.Id = gm.GroupId
 WHERE gm.GroupMemberStatus = 1
   AND gm.IsArchived = 0;
CREATE CLUSTERED INDEX ix ON #member_rows(PersonId);

IF OBJECT_ID('tempdb..#primary_alias') IS NOT NULL DROP TABLE #primary_alias;
SELECT x.PersonId, x.[Guid]
  INTO #primary_alias
  FROM ( SELECT pa.PersonId, pa.[Guid],
                ROW_NUMBER() OVER (PARTITION BY pa.PersonId
                                   ORDER BY CASE WHEN pa.Id = p.PrimaryAliasId THEN 0 ELSE 1 END, pa.Id) AS rn
           FROM dbo.PersonAlias pa
           JOIN dbo.Person p ON p.Id = pa.PersonId
          WHERE EXISTS (SELECT 1 FROM #member_rows mr WHERE mr.PersonId = pa.PersonId) ) x
 WHERE x.rn = 1;
CREATE UNIQUE CLUSTERED INDEX ix ON #primary_alias(PersonId);

SELECT
    mr.channel_id AS channel_id,
    pa.[Guid] AS person_alias_guid,
    gtr.IsLeader AS is_leader,
    mr.IsChatBanned AS is_banned,
    mr.ChatBannedUntil AS ban_expires_at
FROM #member_rows mr
JOIN dbo.GroupTypeRole gtr ON gtr.Id = mr.GroupRoleId
JOIN #primary_alias pa ON pa.PersonId = mr.PersonId;
