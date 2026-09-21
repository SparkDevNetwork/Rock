-- Stages the sets every other projection query reads.
--
-- The question "is this group a chat channel" is asked here, once, and its answer is written into
-- #ChatGroups. Nothing else in this projection asks it again. That is not only about having one
-- definition of the rule: the four queries run as separate statements seconds apart, and a group
-- that starts qualifying, or a person who joins a group, between two of them would otherwise put a
-- membership in the payload whose channel or whose person is in no other section of it. The far
-- side keys a membership to both, so a row like that fails the whole submission rather than one
-- table, and every retry reproduces it.
--
-- Freezing the sets here does not freeze the column values, which are still read live by each
-- query. A group renamed between two statements ships a slightly stale name and the next cycle
-- corrects it. That is a difference in a value, not a row that cannot be stored.

SET NOCOUNT ON;

-- Every group that is, or ever was, a chat channel. The marker half is what makes turning chat off
-- archive a conversation instead of losing it, so it is deliberately not filtered by the group's
-- own state: an archived or deactivated channel still belongs here and its row still ships.
IF OBJECT_ID( 'tempdb..#ChatGroups' ) IS NOT NULL DROP TABLE #ChatGroups;

SELECT
    [G].[Id] AS [GroupId],
    [G].[Guid] AS [ChannelGuid]
INTO #ChatGroups
FROM [Group] AS [G]
INNER JOIN [GroupType] AS [GT] ON [GT].[Id] = [G].[GroupTypeId]
WHERE [G].[ChatChannelFirstEnabledDateTime] IS NOT NULL
    OR ( [GT].[IsChatAllowed] = 1
         AND COALESCE( [G].[IsChatEnabledOverride], [GT].[IsChatEnabledForAllGroups] ) = 1 );

CREATE UNIQUE CLUSTERED INDEX [IX_ChatGroups] ON #ChatGroups ( [GroupId] );

-- The channels a membership may belong to. A channel that is archived or deactivated keeps its own
-- row but takes no members, which is how chat goes quiet on it without anything being deleted.
IF OBJECT_ID( 'tempdb..#LiveChannels' ) IS NOT NULL DROP TABLE #LiveChannels;

SELECT
    [CG].[GroupId],
    [CG].[ChannelGuid]
INTO #LiveChannels
FROM #ChatGroups AS [CG]
INNER JOIN [Group] AS [G] ON [G].[Id] = [CG].[GroupId]
WHERE [G].[IsActive] = 1
    AND [G].[IsArchived] = 0;

CREATE UNIQUE CLUSTERED INDEX [IX_LiveChannels] ON #LiveChannels ( [GroupId] );

IF OBJECT_ID( 'tempdb..#MemberRows' ) IS NOT NULL DROP TABLE #MemberRows;

SELECT
    [GM].[PersonId],
    [GM].[GroupRoleId],
    [GM].[IsChatBanned],
    [GM].[ChatBannedUntil],
    [LC].[ChannelGuid]
INTO #MemberRows
FROM [GroupMember] AS [GM]
INNER JOIN #LiveChannels AS [LC] ON [LC].[GroupId] = [GM].[GroupId]
WHERE [GM].[GroupMemberStatus] = 1
    AND [GM].[IsArchived] = 0;

CREATE CLUSTERED INDEX [IX_MemberRows] ON #MemberRows ( [PersonId] );

-- Everyone who needs an alias row on the far side.
--
-- The last arm is the one that looks redundant and is not. Anyone in #MemberRows is already in the
-- third arm, because a live channel is a chat group. It is here so that containment holds by
-- construction rather than by the two statements happening to see the same data: without it, a
-- person who joined before #MemberRows was staged and left before this runs is a membership with
-- no alias, and the far side refuses the whole submission over it.
IF OBJECT_ID( 'tempdb..#Enrolled' ) IS NOT NULL DROP TABLE #Enrolled;

SELECT [PersonId]
INTO #Enrolled
FROM (
    -- The sticky marker. Any status, any archive state: enrolment is never withdrawn.
    SELECT [GM].[PersonId]
    FROM [GroupMember] AS [GM]
    INNER JOIN [Group] AS [G] ON [G].[Id] = [GM].[GroupId]
    WHERE [G].[Guid] = @ChatPeopleGroupGuid

    UNION

    -- The ban list and the administrators are ordinary memberships, so their status matters.
    SELECT [GM].[PersonId]
    FROM [GroupMember] AS [GM]
    INNER JOIN [Group] AS [G] ON [G].[Id] = [GM].[GroupId]
    WHERE [G].[Guid] IN ( @ChatBanListGroupGuid, @ChatAdministratorsGroupGuid )
        AND [GM].[GroupMemberStatus] = 1
        AND [GM].[IsArchived] = 0

    UNION

    -- Anyone in a chat-capable group, whatever state that group is in.
    SELECT [GM].[PersonId]
    FROM [GroupMember] AS [GM]
    INNER JOIN #ChatGroups AS [CG] ON [CG].[GroupId] = [GM].[GroupId]
    WHERE [GM].[GroupMemberStatus] = 1
        AND [GM].[IsArchived] = 0

    UNION

    SELECT [MR].[PersonId]
    FROM #MemberRows AS [MR]
) AS [Q];

CREATE UNIQUE CLUSTERED INDEX [IX_Enrolled] ON #Enrolled ( [PersonId] );

-- One pass over the aliases of the enrolled population, staged because it is read twice below and
-- a common table expression referenced twice is evaluated twice.
--
-- The ranking reproduces the documented fallback: the person's own primary alias, or the lowest
-- alias id when they have none.
IF OBJECT_ID( 'tempdb..#Alias' ) IS NOT NULL DROP TABLE #Alias;

SELECT
    [PA].[PersonId],
    [PA].[Guid] AS [AliasGuid],
    CAST( CASE WHEN ROW_NUMBER() OVER (
                    PARTITION BY [PA].[PersonId]
                    ORDER BY CASE WHEN [PA].[Id] = [P].[PrimaryAliasId] THEN 0 ELSE 1 END, [PA].[Id] ) = 1
               THEN 1 ELSE 0 END AS BIT ) AS [IsPrimary]
INTO #Alias
FROM [PersonAlias] AS [PA]
INNER JOIN [Person] AS [P] ON [P].[Id] = [PA].[PersonId]
INNER JOIN #Enrolled AS [E] ON [E].[PersonId] = [PA].[PersonId];

CREATE CLUSTERED INDEX [IX_Alias] ON #Alias ( [PersonId] );

-- The badge keys a person holds, ordered by the church's configured badge order, aggregated once
-- per person who holds one rather than once per alias row.
IF OBJECT_ID( 'tempdb..#BadgeViews' ) IS NOT NULL DROP TABLE #BadgeViews;

SELECT
    CAST( [J].[value] AS UNIQUEIDENTIFIER ) AS [DataViewGuid],
    CAST( [J].[key] AS INT ) AS [SortOrder]
INTO #BadgeViews
FROM OPENJSON( @BadgeDataViewGuidsJson ) AS [J];

CREATE UNIQUE CLUSTERED INDEX [IX_BadgeViews] ON #BadgeViews ( [DataViewGuid] );

IF OBJECT_ID( 'tempdb..#Badges' ) IS NOT NULL DROP TABLE #Badges;

SELECT
    [BP].[PersonId],
    STUFF( (
        SELECT ',' + LOWER( CAST( [DV].[Guid] AS VARCHAR( 36 ) ) )
        FROM [DataViewPersistedValue] AS [DVPV]
        INNER JOIN [DataView] AS [DV] ON [DV].[Id] = [DVPV].[DataViewId]
        INNER JOIN #BadgeViews AS [BV] ON [BV].[DataViewGuid] = [DV].[Guid]
        WHERE [DVPV].[EntityId] = [BP].[PersonId]
            AND [DV].[PersistedScheduleIntervalMinutes] IS NOT NULL
        ORDER BY [BV].[SortOrder]
        FOR XML PATH( '' ), TYPE ).value( '.', 'VARCHAR(MAX)' ), 1, 1, '' ) AS [BadgeKeys]
INTO #Badges
FROM (
    SELECT DISTINCT [DVPV].[EntityId] AS [PersonId]
    FROM [DataViewPersistedValue] AS [DVPV]
    INNER JOIN [DataView] AS [DV] ON [DV].[Id] = [DVPV].[DataViewId]
    INNER JOIN #BadgeViews AS [BV] ON [BV].[DataViewGuid] = [DV].[Guid]
    INNER JOIN #Enrolled AS [E] ON [E].[PersonId] = [DVPV].[EntityId]
    WHERE [DV].[PersistedScheduleIntervalMinutes] IS NOT NULL
) AS [BP];

CREATE UNIQUE CLUSTERED INDEX [IX_Badges] ON #Badges ( [PersonId] );
