/*
<doc>
    <summary>
        Reads this church's chat picture for the chat platform: every alias, channel, membership
        and badge, restated whole, or, given a scope, only the rows the few keys one save touched
        reach. First it marks the groups that are chat channels right now.
    </summary>

    <returns>
        Six result sets, in this order, which is the order the Chat Platform Sync job reads them:
        * The moment the reading describes, in UTC, and the identity seed of Person, PersonAlias,
          Group and GroupMember.
        * The aliases section.
        * The channels section.
        * The members section.
        * The badges section.
        * The configured badges left out, by Data View guid, name and reason: missing,
          not_people or not_persisted. Not part of the payload.
        A scoped call returns two more after those six, naming the requested keys that no longer
        qualify, because the platform stamps absent only what a push names:
        * The absent channels: each requested group guid that is not a chat channel, as channel_id.
        * The absent members: each requested membership of a chat channel that the members section
          did not return, as channel_id and the person's primary person_alias_guid.
        Each section's columns are named as the chat wire contract names them.
    </returns>

    <param name='StampedAt' datatype='datetime'>The mark to write on a group that is a chat channel and carries none yet, in the organization's time.</param>
    <param name='ChatPeopleGroupGuid' datatype='uniqueidentifier'>The Chat People group, whose membership enrols a person in chat for good.</param>
    <param name='ChatBanListGroupGuid' datatype='uniqueidentifier'>The chat ban list group.</param>
    <param name='ChatAdministratorsGroupGuid' datatype='uniqueidentifier'>The chat administrators group.</param>
    <param name='ChatSystemAuthorGuid' datatype='uniqueidentifier'>The alias of chat itself, the author of the messages a conversation generates about its own membership.</param>
    <param name='DirectMessageGroupTypeGuid' datatype='uniqueidentifier'>The direct message group type.</param>
    <param name='BadgeDataViewGuidsJson' datatype='nvarchar(max)'>The church's badge Data Views, as a JSON array of guids in the order the church put them.</param>
    <param name='PersonEntityTypeId' datatype='int'>The Person entity type.</param>
    <param name='ActiveRecordStatusValueId' datatype='int'>The Active record status.</param>
    <param name='ProfilesVisibleByDefault' datatype='bit'>Whether a person who never chose shows their profile.</param>
    <param name='OpenDirectMessagesByDefault' datatype='bit'>Whether a person who never chose accepts direct messages from anyone.</param>
    <param name='PublicApplicationRoot' datatype='nvarchar(4000)'>The public address of this Rock, ending in a slash, that photo and icon links are built on.</param>
    <param name='ScopePersonIdsJson' datatype='nvarchar(max)'>Optional. People whose own rows a save touched, as a JSON array of person ids.</param>
    <param name='ScopeGroupGuidsJson' datatype='nvarchar(max)'>Optional. Groups whose channel and whole membership a save touched, as a JSON array of group guids. Guids rather than ids, so a deleted group can still be named.</param>
    <param name='ScopeMemberKeysJson' datatype='nvarchar(max)'>Optional. Single memberships a save touched, as a JSON array of [group guid, person id] pairs.</param>
    <param name='ScopeChannelGuidsJson' datatype='nvarchar(max)'>Optional. Groups whose channel row alone a save touched, as a JSON array of group guids: a save that changed nothing deciding who is in the channel, such as a rename, so none of its memberships is read.</param>

    <remarks>
        This is not a pure read: the marking at the top writes. It only ever touches a group that
        is a chat channel and carries no mark yet, so a run in the steady state writes nothing.
        Call it outside a transaction, as the job does, so that the mark commits on its own.

        With all four scopes null the call reads the whole church. With any of them set it is
        scoped: it marks only the requested groups, returns only the requested rows, and returns
        with each membership its channel row and its person's aliases, because the platform takes
        no membership whose channel or alias it does not hold. A scoped call returns no badges and
        no row for chat's own author, which belong to the whole restatement, but an alias row still
        carries its badge keys.
    </remarks>
</doc>
*/

CREATE PROCEDURE [dbo].[spChat_SyncProjection]
    @StampedAt DATETIME,
    @ChatPeopleGroupGuid UNIQUEIDENTIFIER,
    @ChatBanListGroupGuid UNIQUEIDENTIFIER,
    @ChatAdministratorsGroupGuid UNIQUEIDENTIFIER,
    @ChatSystemAuthorGuid UNIQUEIDENTIFIER,
    @DirectMessageGroupTypeGuid UNIQUEIDENTIFIER,
    @BadgeDataViewGuidsJson NVARCHAR(MAX),
    @PersonEntityTypeId INT,
    @ActiveRecordStatusValueId INT,
    @ProfilesVisibleByDefault BIT,
    @OpenDirectMessagesByDefault BIT,
    @PublicApplicationRoot NVARCHAR(4000),
    @ScopePersonIdsJson NVARCHAR(MAX) = NULL,
    @ScopeGroupGuidsJson NVARCHAR(MAX) = NULL,
    @ScopeMemberKeysJson NVARCHAR(MAX) = NULL,
    @ScopeChannelGuidsJson NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- The scope.

    -- A scoped call reads the few keys one save touched and an unscoped one the whole church, and
    -- both run this one procedure, so a cached plan would be shared between them: a plan built for
    -- three keys reused for every row of a large church, or the reverse. So every statement that
    -- reads Rock's tables through a scope predicate is recompiled on each call, which lets the
    -- optimizer read @IsScoped as a constant. Unscoped, each (@IsScoped = 0 OR ...) folds away and
    -- the statement gets the plan it had before there was a scope. The tables those statements fill
    -- are declared first rather than made by SELECT INTO, because a table a recompiled SELECT INTO
    -- makes is a new object on every call, and every later statement that reads it would then
    -- recompile too. A statement where the scope only turns the whole result on or off keeps its
    -- cached plan, since the plan tests @IsScoped before it reads a row. The keys are staged once
    -- in small temporary tables, so no predicate parses JSON per row.
    DECLARE @IsScoped BIT = CASE
        WHEN @ScopePersonIdsJson IS NULL AND @ScopeGroupGuidsJson IS NULL AND @ScopeMemberKeysJson IS NULL AND @ScopeChannelGuidsJson IS NULL THEN 0
        ELSE 1
    END;

    SELECT DISTINCT CAST( [J].[value] AS INT ) AS [PersonId]
    INTO #ScopePeople
    FROM OPENJSON( @ScopePersonIdsJson ) AS [J];

    SELECT DISTINCT CAST( [J].[value] AS UNIQUEIDENTIFIER ) AS [GroupGuid]
    INTO #ScopeGroupGuids
    FROM OPENJSON( @ScopeGroupGuidsJson ) AS [J];

    SELECT DISTINCT
        CAST( JSON_VALUE( [J].[value], '$[0]' ) AS UNIQUEIDENTIFIER ) AS [GroupGuid],
        CAST( JSON_VALUE( [J].[value], '$[1]' ) AS INT ) AS [PersonId]
    INTO #ScopeMemberKeys
    FROM OPENJSON( @ScopeMemberKeysJson ) AS [J];

    -- Every group the call asks about: each group key, each channel key, and the group of each
    -- membership key. A channel key is staged here alone, which is what keeps its memberships
    -- out: the members and the absent members read #ScopeGroupGuids instead. It is read straight
    -- into this table rather than staged in one of its own, because each temporary table is one
    -- more object every call creates, the whole restatement's included.
    SELECT [SGG].[GroupGuid]
    INTO #ScopeGroups
    FROM #ScopeGroupGuids AS [SGG]
    UNION
    SELECT CAST( [J].[value] AS UNIQUEIDENTIFIER )
    FROM OPENJSON( @ScopeChannelGuidsJson ) AS [J]
    UNION
    SELECT [SMK].[GroupGuid]
    FROM #ScopeMemberKeys AS [SMK];

    -- Every person the call asks about by name: each person key, and the person of each
    -- membership key. Whether they are enrolled is decided by the whole rule below, so the
    -- channels they are live members of are staged for them too.
    SELECT [SP].[PersonId]
    INTO #ScopeReach
    FROM #ScopePeople AS [SP]
    UNION
    SELECT [SMK].[PersonId]
    FROM #ScopeMemberKeys AS [SMK];

    -- The channel mark.

    -- Marks every group that is a chat channel right now and carries no mark yet. A marked group is
    -- projected whatever its settings later say, so turning chat off archives a conversation rather
    -- than losing it. Written once and never moved.
    --
    -- The first statement that writes, and it commits on its own: a mark rolled back with a failed
    -- submission would leave the next run treating the group as though it had never been a channel.
    --
    -- The condition is the half of the channel rule that says a group qualifies now. #ChatGroups
    -- holds the same words, and a test fails if the two stop agreeing.
    UPDATE [G]
    SET [G].[ChatChannelFirstEnabledDateTime] = @StampedAt
    FROM [Group] AS [G]
    INNER JOIN [GroupType] AS [GT] ON [GT].[Id] = [G].[GroupTypeId]
    WHERE [G].[ChatChannelFirstEnabledDateTime] IS NULL
        -- The groups Rock ships to run chat are never channels, whatever their type allows.
        AND [G].[Guid] NOT IN ( @ChatPeopleGroupGuid, @ChatBanListGroupGuid, @ChatAdministratorsGroupGuid )
        AND [GT].[IsChatAllowed] = 1
        AND COALESCE( [G].[IsChatEnabledOverride], [GT].[IsChatEnabledForAllGroups] ) = 1
        -- A scoped call marks only the groups it asks about, so a group that starts qualifying
        -- through a save is marked by that save's push, and nothing else is touched.
        AND ( @IsScoped = 0 OR [G].[Guid] IN ( SELECT [SG].[GroupGuid] FROM #ScopeGroups AS [SG] ) )
    OPTION ( RECOMPILE );

    -- The moment and the identity marks.

    -- The moment in UTC from the database, because it is compared against the platform's clock, and
    -- each table's identity seed. The seed, not the largest id: deleting the newest rows lowers the
    -- largest id, while a restored database moves the seed back, which the platform refuses.
    SELECT
        SYSUTCDATETIME() AS [read_at],
        CAST( IDENT_CURRENT( 'Person' ) AS BIGINT ) AS [person],
        CAST( IDENT_CURRENT( 'PersonAlias' ) AS BIGINT ) AS [person_alias],
        CAST( IDENT_CURRENT( '[Group]' ) AS BIGINT ) AS [group],
        CAST( IDENT_CURRENT( 'GroupMember' ) AS BIGINT ) AS [group_member];

    -- The staged sets.

    -- Whether a group is a chat channel is asked once, here. Read straight from the tables, the four
    -- sections would be four moments, and a membership could ship without its channel or person,
    -- which fails the whole submission on every retry. So the sections read their rows from these
    -- sets; only values hung off a staged row, such as a campus or a photo, are read live, where a
    -- change costs one stale cycle. A temporary table lives until the procedure returns.

    -- Every group that is, or ever was, a chat channel. Not filtered by the group's own state, so an
    -- archived channel still ships. Rock's own chat groups are left out, even with a mark.
    --
    -- A scoped call stages the groups it asks about, which are the only channels it returns, and
    -- the channels its people are live members of, which it reads only to decide whether they are
    -- enrolled.
    CREATE TABLE #ChatGroups (
        [GroupId] INT NOT NULL,
        [ChannelGuid] UNIQUEIDENTIFIER NOT NULL,
        [Name] NVARCHAR( 100 ) NOT NULL,
        [IsActive] BIT NOT NULL,
        [IsArchived] BIT NOT NULL,
        [CampusId] INT NULL,
        [ChatChannelAvatarBinaryFileId] INT NULL,
        [GroupTypeGuid] UNIQUEIDENTIFIER NOT NULL,
        [IsChatChannelPublicOverride] BIT NULL,
        [IsChatChannelPublic] BIT NOT NULL,
        [IsChatChannelAlwaysShownOverride] BIT NULL,
        [IsChatChannelAlwaysShown] BIT NOT NULL,
        [IsLeavingChatChannelAllowedOverride] BIT NULL,
        [IsLeavingChatChannelAllowed] BIT NOT NULL,
        [CanViewMembersOverride] BIT NULL,
        [CanViewMembers] BIT NOT NULL,
        [ChatPushNotificationModeOverride] INT NULL,
        [ChatPushNotificationMode] INT NOT NULL,
        [IsChatSearchIndexedOverride] BIT NULL,
        [IsChatSearchIndexed] BIT NOT NULL,
        [IsRequested] BIT NOT NULL
    );

    INSERT INTO #ChatGroups
    SELECT
        [G].[Id] AS [GroupId],
        [G].[Guid] AS [ChannelGuid],
        [G].[Name],
        [G].[IsActive],
        [G].[IsArchived],
        [G].[CampusId],
        [G].[ChatChannelAvatarBinaryFileId],
        [GT].[Guid] AS [GroupTypeGuid],
        [G].[IsChatChannelPublicOverride],
        [GT].[IsChatChannelPublic],
        [G].[IsChatChannelAlwaysShownOverride],
        [GT].[IsChatChannelAlwaysShown],
        [G].[IsLeavingChatChannelAllowedOverride],
        [GT].[IsLeavingChatChannelAllowed],
        [G].[CanViewMembersOverride],
        [GT].[CanViewMembers],
        [G].[ChatPushNotificationModeOverride],
        [GT].[ChatPushNotificationMode],
        [G].[IsChatSearchIndexedOverride],
        [GT].[IsChatSearchIndexed],
        CAST( CASE
            WHEN @IsScoped = 0 OR [G].[Guid] IN ( SELECT [SG].[GroupGuid] FROM #ScopeGroups AS [SG] ) THEN 1
            ELSE 0
        END AS BIT ) AS [IsRequested]
    FROM [Group] AS [G]
    INNER JOIN [GroupType] AS [GT] ON [GT].[Id] = [G].[GroupTypeId]
    WHERE [G].[Guid] NOT IN ( @ChatPeopleGroupGuid, @ChatBanListGroupGuid, @ChatAdministratorsGroupGuid )
        AND ( [G].[ChatChannelFirstEnabledDateTime] IS NOT NULL
              OR ( [GT].[IsChatAllowed] = 1
                   AND COALESCE( [G].[IsChatEnabledOverride], [GT].[IsChatEnabledForAllGroups] ) = 1 ) )
        AND ( @IsScoped = 0
              OR [G].[Guid] IN ( SELECT [SG].[GroupGuid] FROM #ScopeGroups AS [SG] )
              OR [G].[Id] IN (
                  SELECT [GM].[GroupId]
                  FROM [GroupMember] AS [GM]
                  INNER JOIN #ScopeReach AS [SR] ON [SR].[PersonId] = [GM].[PersonId]
                  WHERE [GM].[GroupMemberStatus] = 1
                      AND [GM].[IsArchived] = 0 ) )
    OPTION ( RECOMPILE );

    CREATE UNIQUE CLUSTERED INDEX [IX_ChatGroups] ON #ChatGroups ( [GroupId] );

    -- The channels a membership may belong to. A channel that is archived or deactivated keeps its own
    -- row but takes no members, which is how chat goes quiet on it without anything being deleted.
    SELECT
        [CG].[GroupId],
        [CG].[ChannelGuid]
    INTO #LiveChannels
    FROM #ChatGroups AS [CG]
    WHERE [CG].[IsActive] = 1
        AND [CG].[IsArchived] = 0;

    CREATE UNIQUE CLUSTERED INDEX [IX_LiveChannels] ON #LiveChannels ( [GroupId] );

    CREATE TABLE #MemberRows (
        [PersonId] INT NOT NULL,
        [GroupRoleId] INT NOT NULL,
        [IsChatBanned] BIT NOT NULL,
        [ChatBannedUntil] DATETIME NULL,
        [ChannelGuid] UNIQUEIDENTIFIER NOT NULL
    );

    INSERT INTO #MemberRows
    SELECT
        [GM].[PersonId],
        [GM].[GroupRoleId],
        [GM].[IsChatBanned],
        [GM].[ChatBannedUntil],
        [LC].[ChannelGuid]
    FROM [GroupMember] AS [GM]
    INNER JOIN #LiveChannels AS [LC] ON [LC].[GroupId] = [GM].[GroupId]
    WHERE [GM].[GroupMemberStatus] = 1
        AND [GM].[IsArchived] = 0
        -- A scoped call reads each requested group's whole live membership and each requested
        -- membership, and nothing from a channel it staged only to decide enrolment.
        AND ( @IsScoped = 0
              OR [LC].[ChannelGuid] IN ( SELECT [SGG].[GroupGuid] FROM #ScopeGroupGuids AS [SGG] )
              OR EXISTS (
                  SELECT 1
                  FROM #ScopeMemberKeys AS [SMK]
                  WHERE [SMK].[GroupGuid] = [LC].[ChannelGuid]
                      AND [SMK].[PersonId] = [GM].[PersonId] ) )
    OPTION ( RECOMPILE );

    CREATE CLUSTERED INDEX [IX_MemberRows] ON #MemberRows ( [PersonId] );

    -- The people a scoped call returns aliases for: those it asks about, and those in the
    -- memberships it returns, so that no membership ships without its alias.
    SELECT [SR].[PersonId]
    INTO #ScopeCandidates
    FROM #ScopeReach AS [SR]
    UNION
    SELECT [MR].[PersonId]
    FROM #MemberRows AS [MR]
    WHERE @IsScoped = 1;

    -- Everyone who needs an alias row on the far side. The last arm looks redundant, since a live
    -- channel is a chat group, but it makes containment hold by construction: without it, a person
    -- who left between the two statements is a membership with no alias.
    CREATE TABLE #Enrolled (
        [PersonId] INT NOT NULL
    );

    INSERT INTO #Enrolled
    SELECT [PersonId]
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
    ) AS [Q]
    -- A scoped call enrols, by the same rule, only the people it returns aliases for.
    WHERE @IsScoped = 0
        OR [Q].[PersonId] IN ( SELECT [SC].[PersonId] FROM #ScopeCandidates AS [SC] )
    OPTION ( RECOMPILE );

    CREATE UNIQUE CLUSTERED INDEX [IX_Enrolled] ON #Enrolled ( [PersonId] );

    -- Staged because it is read twice below, and a common table expression is evaluated per use.
    -- The primary is the person's own primary alias, or the lowest alias id when they have none.
    SELECT
        [PA].[PersonId],
        [PA].[Guid] AS [AliasGuid],
        -- Repeated in the absent members set below; the two must pick the same alias.
        CAST( CASE WHEN ROW_NUMBER() OVER (
                        PARTITION BY [PA].[PersonId]
                        ORDER BY CASE WHEN [PA].[Id] = [P].[PrimaryAliasId] THEN 0 ELSE 1 END, [PA].[Id] ) = 1
                   THEN 1 ELSE 0 END AS BIT ) AS [IsPrimary],
        [P].[NickName],
        [P].[LastName],
        [P].[PhotoId],
        [P].[PrimaryCampusId],
        [P].[IsChatProfilePublic],
        [P].[IsChatOpenDirectMessageAllowed],
        [P].[RecordStatusValueId]
    INTO #Alias
    FROM [PersonAlias] AS [PA]
    INNER JOIN [Person] AS [P] ON [P].[Id] = [PA].[PersonId]
    INNER JOIN #Enrolled AS [E] ON [E].[PersonId] = [PA].[PersonId];

    CREATE CLUSTERED INDEX [IX_Alias] ON #Alias ( [PersonId] );

    -- The badge Data Views in the church's order. A badge listed twice keeps its first place, and a
    -- Data View of anything but people is left out, because holders are matched by person id.
    SELECT
        [DV].[Guid] AS [DataViewGuid],
        MIN( CAST( [J].[key] AS INT ) ) AS [SortOrder]
    INTO #BadgeViews
    FROM OPENJSON( @BadgeDataViewGuidsJson ) AS [J]
    INNER JOIN [DataView] AS [DV] ON [DV].[Guid] = CAST( [J].[value] AS UNIQUEIDENTIFIER )
    WHERE [DV].[EntityTypeId] = @PersonEntityTypeId
    GROUP BY [DV].[Guid];

    CREATE UNIQUE CLUSTERED INDEX [IX_BadgeViews] ON #BadgeViews ( [DataViewGuid] );

    -- FOR XML needs QUOTED_IDENTIFIER on, which a procedure takes from its creation, not its caller.
    SELECT
        [BP].[PersonId],
        STUFF( (
            SELECT ',' + LOWER( CAST( [DV].[Guid] AS VARCHAR( 36 ) ) )
            FROM [DataViewPersistedValue] AS [DVPV]
            INNER JOIN [DataView] AS [DV] ON [DV].[Id] = [DVPV].[DataViewId]
            INNER JOIN #BadgeViews AS [BV] ON [BV].[DataViewGuid] = [DV].[Guid]
            WHERE [DVPV].[EntityId] = [BP].[PersonId]
                AND ( [DV].[PersistedScheduleIntervalMinutes] IS NOT NULL OR [DV].[PersistedScheduleId] IS NOT NULL )
            ORDER BY [BV].[SortOrder]
            FOR XML PATH( '' ), TYPE ).value( '.', 'VARCHAR(MAX)' ), 1, 1, '' ) AS [BadgeKeys]
    INTO #Badges
    FROM (
        SELECT DISTINCT [DVPV].[EntityId] AS [PersonId]
        FROM [DataViewPersistedValue] AS [DVPV]
        INNER JOIN [DataView] AS [DV] ON [DV].[Id] = [DVPV].[DataViewId]
        INNER JOIN #BadgeViews AS [BV] ON [BV].[DataViewGuid] = [DV].[Guid]
        INNER JOIN #Enrolled AS [E] ON [E].[PersonId] = [DVPV].[EntityId]
        WHERE ( [DV].[PersistedScheduleIntervalMinutes] IS NOT NULL OR [DV].[PersistedScheduleId] IS NOT NULL )
    ) AS [BP];

    CREATE UNIQUE CLUSTERED INDEX [IX_Badges] ON #Badges ( [PersonId] );

    -- The aliases section.

    -- A person's primary alias row carries everything; their other aliases carry only the two ids,
    -- so a client holding an old alias resolves it and a merge heals itself on the next cycle. The
    -- last row is chat itself, the author of membership messages, with no person behind it, which
    -- only the whole restatement carries.
    SELECT
        [A].[AliasGuid] AS [person_alias_guid],
        [PR].[AliasGuid] AS [primary_person_alias_guid],
        -- A blank name is sent as none: the far side refuses an empty one, and the church with it.
        CASE WHEN [A].[IsPrimary] = 1 AND LTRIM( RTRIM( [A].[NickName] ) ) <> N'' THEN [A].[NickName] END AS [nick_name],
        CASE WHEN [A].[IsPrimary] = 1 AND LTRIM( RTRIM( [A].[LastName] ) ) <> N'' THEN [A].[LastName] END AS [last_name],

        -- A photo behind a binary file type that requires view security is not linked at all, because
        -- the far side serves this URL to every member of every channel the person is in.
        CASE WHEN [A].[IsPrimary] = 1 THEN
            CASE
                WHEN [BF].[Guid] IS NULL OR [BFT].[RequiresViewSecurity] = 1 THEN NULL
                ELSE CAST( @PublicApplicationRoot + N'GetImage.ashx?guid=' + LOWER( CAST( [BF].[Guid] AS NVARCHAR( 36 ) ) ) AS NVARCHAR( 400 ) )
            END
        END AS [avatar_url],

        CASE WHEN [A].[IsPrimary] = 1 THEN [CM].[Guid] END AS [campus_id],

        -- Joined here, and split into a list before it reaches the wire.
        CASE WHEN [A].[IsPrimary] = 1 THEN [BK].[BadgeKeys] END AS [badge_keys],

        CASE WHEN [A].[IsPrimary] = 1 THEN COALESCE( [A].[IsChatProfilePublic], @ProfilesVisibleByDefault ) END AS [show_profile_details],
        CASE WHEN [A].[IsPrimary] = 1 THEN COALESCE( [A].[IsChatOpenDirectMessageAllowed], @OpenDirectMessagesByDefault ) END AS [is_open_dm_allowed],

        CASE WHEN [A].[IsPrimary] = 1 THEN
            CASE WHEN [B].[PersonId] IS NULL THEN CAST( 0 AS BIT ) ELSE CAST( 1 AS BIT ) END
        END AS [is_globally_banned],

        -- Anyone whose record is not active is hidden rather than removed, so reactivating them in Rock
        -- restores every channel and every message they had.
        CASE WHEN [A].[IsPrimary] = 1 THEN
            CASE WHEN [A].[RecordStatusValueId] = @ActiveRecordStatusValueId THEN CAST( 0 AS BIT ) ELSE CAST( 1 AS BIT ) END
        END AS [is_inactive]

    FROM #Alias AS [A]
    INNER JOIN #Alias AS [PR] ON [PR].[PersonId] = [A].[PersonId] AND [PR].[IsPrimary] = 1
    LEFT JOIN [Campus] AS [CM] ON [CM].[Id] = [A].[PrimaryCampusId]
    LEFT JOIN [BinaryFile] AS [BF] ON [BF].[Id] = [A].[PhotoId]
    LEFT JOIN [BinaryFileType] AS [BFT] ON [BFT].[Id] = [BF].[BinaryFileTypeId]
    LEFT JOIN #Badges AS [BK] ON [BK].[PersonId] = [A].[PersonId]
    LEFT JOIN (
        SELECT DISTINCT [GM].[PersonId]
        FROM [GroupMember] AS [GM]
        INNER JOIN [Group] AS [G] ON [G].[Id] = [GM].[GroupId]
        WHERE [G].[Guid] = @ChatBanListGroupGuid
            AND [GM].[GroupMemberStatus] = 1
            AND [GM].[IsArchived] = 0
    ) AS [B] ON [B].[PersonId] = [A].[PersonId]

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
        CAST( 0 AS BIT )
    WHERE @IsScoped = 0;

    -- The channels section.

    -- In the wire contract's column order and no other, because rows travel as positional arrays.
    --
    -- Five direct message settings are forced, because the far side refuses a direct message that is
    -- public, always shown, search indexed or campused, and any channel without a name, and Rock
    -- enforces none of them. One such row would fail the church's submission on every cycle.
    SELECT
        [CG].[ChannelGuid] AS [channel_id],

        -- Never null: a blank name would fail the whole submission.
        COALESCE( NULLIF( LTRIM( RTRIM( [CG].[Name] ) ), N'' ), N'Channel ' + CAST( [CG].[GroupId] AS NVARCHAR( 20 ) ) ) AS [name],

        -- Not linked behind a file type that requires view security, for the same reason as a photo.
        CASE
            WHEN [BF].[Guid] IS NULL OR [BFT].[RequiresViewSecurity] = 1 THEN NULL
            ELSE @PublicApplicationRoot + N'GetImage.ashx?guid=' + LOWER( CAST( [BF].[Guid] AS NVARCHAR( 36 ) ) ) + N'&maxwidth=120&maxheight=120'
        END AS [icon_url],

        CASE WHEN [CG].[GroupTypeGuid] = @DirectMessageGroupTypeGuid THEN 'dm' ELSE 'shared' END AS [channel_type],

        -- An archived or deactivated channel keeps its row and history but loses its reach: not
        -- public, not always shown and not searchable.
        CASE
            WHEN [CG].[GroupTypeGuid] = @DirectMessageGroupTypeGuid OR [CG].[IsActive] = 0 OR [CG].[IsArchived] = 1 THEN CAST( 0 AS BIT )
            ELSE COALESCE( [CG].[IsChatChannelPublicOverride], [CG].[IsChatChannelPublic] )
        END AS [is_public],

        CASE
            WHEN [CG].[GroupTypeGuid] = @DirectMessageGroupTypeGuid OR [CG].[IsActive] = 0 OR [CG].[IsArchived] = 1 THEN CAST( 0 AS BIT )
            ELSE COALESCE( [CG].[IsChatChannelAlwaysShownOverride], [CG].[IsChatChannelAlwaysShown] )
        END AS [always_shown],

        COALESCE( [CG].[IsLeavingChatChannelAllowedOverride], [CG].[IsLeavingChatChannelAllowed] ) AS [leave_allowed],

        -- A direct message has no roster to hide, so the setting does not apply to it.
        CASE
            WHEN [CG].[GroupTypeGuid] = @DirectMessageGroupTypeGuid THEN CAST( 1 AS BIT )
            ELSE COALESCE( [CG].[CanViewMembersOverride], [CG].[CanViewMembers] )
        END AS [can_view_members],

        CASE WHEN [CG].[GroupTypeGuid] = @DirectMessageGroupTypeGuid THEN NULL ELSE [C].[Guid] END AS [campus_id],

        -- Rock numbers these modes and the wire names them, so the mapping is the thing that can be
        -- wrong. A test holds it against the values the contract publishes.
        CASE COALESCE( [CG].[ChatPushNotificationModeOverride], [CG].[ChatPushNotificationMode] )
            WHEN 1 THEN 'mentions'
            WHEN 2 THEN 'silent'
            ELSE 'all'
        END AS [notify_mode_default],

        CASE
            WHEN [CG].[GroupTypeGuid] = @DirectMessageGroupTypeGuid OR [CG].[IsActive] = 0 OR [CG].[IsArchived] = 1 THEN CAST( 0 AS BIT )
            ELSE COALESCE( [CG].[IsChatSearchIndexedOverride], [CG].[IsChatSearchIndexed] )
        END AS [is_search_indexed]

    FROM #ChatGroups AS [CG]
    LEFT JOIN [Campus] AS [C] ON [C].[Id] = [CG].[CampusId]
    LEFT JOIN [BinaryFile] AS [BF] ON [BF].[Id] = [CG].[ChatChannelAvatarBinaryFileId]
    LEFT JOIN [BinaryFileType] AS [BFT] ON [BFT].[Id] = [BF].[BinaryFileTypeId]
    -- Every staged group, unscoped; a scoped call leaves out the ones it staged for enrolment.
    WHERE [CG].[IsRequested] = 1;

    -- The members section.

    -- Every row's channel and person are in the staged sets by construction, not by timing.
    --
    -- The ban expiry is returned in the organization's time zone, and the job moves it to UTC.
    --
    -- A person may hold several roles in one group and the far side keeps one row per person per
    -- channel, so the rows are folded: a leader in any role leads, a capability any role grants is
    -- granted, a ban in any role bans, and a ban without an end outlasts every dated one.
    SELECT
        [F].[ChannelGuid] AS [channel_id],
        [A].[AliasGuid] AS [person_alias_guid],
        [F].[IsLeader] AS [is_leader],
        [F].[CanMentionAll] AS [can_mention_all],
        [F].[CanPostAnnouncements] AS [can_post_announcements],
        [F].[IsBanned] AS [is_banned],
        [F].[BanExpiresAt] AS [ban_expires_at]
    FROM (
        SELECT
            [MR].[PersonId],
            [MR].[ChannelGuid],
            CAST( MAX( CAST( [GTR].[IsLeader] AS INT ) ) AS BIT ) AS [IsLeader],
            CAST( MAX( CAST( [GTR].[CanMentionAll] AS INT ) ) AS BIT ) AS [CanMentionAll],
            CAST( MAX( CAST( [GTR].[CanPostAnnouncements] AS INT ) ) AS BIT ) AS [CanPostAnnouncements],
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

    -- The badges section.

    -- The sort order is the church's own badge order, so every client renders the same one. The
    -- highlight colour is returned raw, and the job derives the colour pair once for every client.
    -- A push never carries badges, so a scoped call returns none.
    SELECT
        [DV].[Guid] AS [badge_key],
        -- Never blank: the far side refuses a badge with no name, and the whole church with it.
        CASE WHEN LTRIM( RTRIM( [DV].[Name] ) ) <> N'' THEN [DV].[Name] ELSE N'Badge ' + CAST( [DV].[Id] AS NVARCHAR( 20 ) ) END AS [name],
        [DV].[IconCssClass] AS [icon_css],
        [DV].[HighlightColor] AS [highlight_color],
        [BV].[SortOrder] AS [sort_order]
    FROM #BadgeViews AS [BV]
    INNER JOIN [DataView] AS [DV] ON [DV].[Guid] = [BV].[DataViewGuid]
    -- Persisted as Rock counts it, interval or schedule: holders are read from persisted values alone.
    WHERE ( [DV].[PersistedScheduleIntervalMinutes] IS NOT NULL OR [DV].[PersistedScheduleId] IS NOT NULL )
        AND @IsScoped = 0
    ORDER BY [BV].[SortOrder];

    -- The badges left out.

    -- Every configured badge whose Data View no longer exists, does not list people or is not
    -- persisted, so the job can name each one rather than let it quietly never appear. Only the
    -- whole restatement reports them, since only it carries badges.
    SELECT
        [J].[DataViewGuid] AS [badge_key],
        [DV].[Name] AS [name],
        CASE
            WHEN [DV].[Id] IS NULL THEN 'missing'
            WHEN [DV].[EntityTypeId] <> @PersonEntityTypeId THEN 'not_people'
            ELSE 'not_persisted'
        END AS [reason]
    FROM (
        SELECT CAST( [value] AS UNIQUEIDENTIFIER ) AS [DataViewGuid], MIN( CAST( [key] AS INT ) ) AS [SortOrder]
        FROM OPENJSON( @BadgeDataViewGuidsJson )
        GROUP BY CAST( [value] AS UNIQUEIDENTIFIER )
    ) AS [J]
    LEFT JOIN [DataView] AS [DV] ON [DV].[Guid] = [J].[DataViewGuid]
    WHERE @IsScoped = 0
        AND ( [DV].[Id] IS NULL
              OR [DV].[EntityTypeId] <> @PersonEntityTypeId
              OR ( [DV].[PersistedScheduleIntervalMinutes] IS NULL AND [DV].[PersistedScheduleId] IS NULL ) )
    ORDER BY [J].[SortOrder];

    -- The keys that no longer qualify.

    -- Returned by a scoped call alone, after the six every call returns, so the whole restatement
    -- reads exactly what it always has. The platform stamps absent only the keys a push names, so
    -- each requested key the sections did not return is named here. Each set is its own IF with
    -- one statement in it, so that neither reads as a result set every call returns.
    IF @IsScoped = 1
    BEGIN
        -- Each requested group that is not a chat channel, by the guid its save recorded, which
        -- names it even when the group itself has been deleted.
        SELECT [SG].[GroupGuid] AS [channel_id]
        FROM #ScopeGroups AS [SG]
        WHERE NOT EXISTS (
            SELECT 1
            FROM #ChatGroups AS [CG]
            WHERE [CG].[ChannelGuid] = [SG].[GroupGuid] );
    END

    IF @IsScoped = 1
    BEGIN
        -- Each requested membership of a chat channel that the members section did not return:
        -- the membership keys, and every membership of each requested group in any status,
        -- archived or not. Compared per person and channel, as that section folds its rows, and
        -- keyed by the person's primary alias by the rule #Alias uses, whether or not the person is
        -- still enrolled. A person with no alias left has no row on the platform to stamp.
        SELECT
            [R].[ChannelGuid] AS [channel_id],
            [PA].[AliasGuid] AS [person_alias_guid]
        FROM (
            SELECT
                [SMK].[GroupGuid] AS [ChannelGuid],
                [SMK].[PersonId]
            FROM #ScopeMemberKeys AS [SMK]

            UNION

            SELECT
                [SGG].[GroupGuid],
                [GM].[PersonId]
            FROM #ScopeGroupGuids AS [SGG]
            INNER JOIN [Group] AS [G] ON [G].[Guid] = [SGG].[GroupGuid]
            INNER JOIN [GroupMember] AS [GM] ON [GM].[GroupId] = [G].[Id]
        ) AS [R]
        -- Must pick the primary alias exactly as #Alias does: a removal keyed to a different alias
        -- never finds the row it is meant to remove. Repeated rather than shared so the full
        -- sync's #Alias stays a single join.
        CROSS APPLY (
            SELECT TOP 1 [PA].[Guid] AS [AliasGuid]
            FROM [PersonAlias] AS [PA]
            INNER JOIN [Person] AS [P] ON [P].[Id] = [PA].[PersonId]
            WHERE [PA].[PersonId] = [R].[PersonId]
            ORDER BY CASE WHEN [PA].[Id] = [P].[PrimaryAliasId] THEN 0 ELSE 1 END, [PA].[Id]
        ) AS [PA]
        WHERE EXISTS (
                SELECT 1
                FROM #ChatGroups AS [CG]
                WHERE [CG].[ChannelGuid] = [R].[ChannelGuid] )
            AND NOT EXISTS (
                SELECT 1
                FROM #MemberRows AS [MR]
                WHERE [MR].[ChannelGuid] = [R].[ChannelGuid]
                    AND [MR].[PersonId] = [R].[PersonId] );
    END
END
