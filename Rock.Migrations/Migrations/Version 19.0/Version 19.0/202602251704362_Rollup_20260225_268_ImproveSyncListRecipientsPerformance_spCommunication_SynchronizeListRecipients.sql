/*
<doc>
    <summary>
        Synchronizes the pending recipients of a communication with the current, active members of its list group,
        respecting scheduled send cutoffs, recipient/person communication preferences, and optional personalization
        segment filters.
    </summary>
    <params>
        <param name="@CommunicationId" type="INT" required="true">
            The Id of the [Communication] to synchronize.
        </param>
    </params>
    <examples>
        <code>
            -- Synchronize recipients for communication Id 123
            EXEC [dbo].[spCommunication_SynchronizeListRecipients] 123;
        </code>
    </examples>
</doc>
*/

CREATE PROCEDURE [dbo].[spCommunication_SynchronizeListRecipients]
    @CommunicationId INT
AS
BEGIN

    -- If any runtime error occurs inside this transaction, automatically abort the transaction.
    SET XACT_ABORT ON;

    -- Get data from the specified communication record needed to operate below.
    DECLARE @SendDateTime DATETIME
        , @FutureSendDateTime DATETIME
        , @ListGroupId INT
        , @HasLegacySegments BIT
        , @PersonalizationSegmentString NVARCHAR(MAX)
        , @SegmentCriteria INT
        , @CommunicationType INT;

    SELECT @SendDateTime = [SendDateTime]
        , @FutureSendDateTime = [FutureSendDateTime]
        , @ListGroupId = [ListGroupId]
        , @HasLegacySegments = CASE
            WHEN LEN([Segments]) > 0 THEN 1
            ELSE 0
          END
        , @PersonalizationSegmentString = [PersonalizationSegments]
        , @SegmentCriteria = [SegmentCriteria]
        , @CommunicationType = [CommunicationType]
    FROM [Communication]
    WHERE [Id] = @CommunicationId;

    -- If the specified communication:
    --  1) has already been sent;
    --  2) is NOT tied to a list group;
    --  3) has any legacy segments.
    -- Then there's nothing to do. Exit early.
    IF @SendDateTime IS NOT NULL OR @ListGroupId IS NULL OR @HasLegacySegments = 1
        RETURN;

    -- Ensure only a single process can synchronize a given communication's recipients at once.
    DECLARE @LockResult INT
        , @LockResource NVARCHAR(100) = CONCAT('spCommunication_SynchronizeListRecipients_', @CommunicationId);

    BEGIN TRY
        BEGIN TRAN;

        EXEC @LockResult = sp_getapplock
            @Resource = @LockResource
            , @LockMode = 'Exclusive'
            , @LockOwner = 'Transaction'
            , @LockTimeout = 90000;

        IF @LockResult < 0
            RAISERROR('Unable to acquire application lock for communication with ID %d.', 16, 1, @CommunicationId);

        -- Declare constants (Enum values, Etc.).
        DECLARE @PersonalizationTypeSegment INT = 0
            , @SegmentCriteriaAll INT = 0
            , @SegmentCriteriaAny INT = 1

            , @GroupMemberStatusActive INT = 1

            , @RecipientStatusPending INT = 0

            , @CommunicationTypeRecipientPreference INT = 0
            , @CommunicationTypeEmail INT = 1
            , @CommunicationTypeSms INT = 2
            , @CommunicationTypePush INT = 3

            , @NoCommunicationPreference INT = 0

            , @EmailMediumEntityTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '5A653EBE-6803-44B4-85D2-FB7B8146D55D')
            , @SmsMediumEntityTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '4BC02764-512A-4A10-ACDE-586F71D8A8BD')
            , @PushMediumEntityTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '3638C6DF-4FF3-4A52-B4B8-AFB754991597');

        DECLARE @MediumEntityTypeByCommunicationTypes TABLE
        (
            [CommunicationType] INT NOT NULL
            , [MediumEntityTypeId]INT NULL
        );

        INSERT INTO @MediumEntityTypeByCommunicationTypes VALUES
            (@CommunicationTypeRecipientPreference, NULL)
            , (@CommunicationTypeEmail, @EmailMediumEntityTypeId)
            , (@CommunicationTypeSms, @SmsMediumEntityTypeId)
            , (@CommunicationTypePush, @PushMediumEntityTypeId);

        DECLARE @CommunicationMediumEntityTypeId INT = (
            SELECT TOP 1 [MediumEntityTypeId]
            FROM @MediumEntityTypeByCommunicationTypes
            WHERE [CommunicationType] = @CommunicationType
        );

        IF @CommunicationMediumEntityTypeId IS NULL
        BEGIN
            -- Default to email in case a recipient doesn't have a preference.
            SET @CommunicationMediumEntityTypeId = @EmailMediumEntityTypeId;
        END

        -- Split the communication's personalization segment IDs into a table variable for easy reference.
        DECLARE @PersonalizationSegments TABLE
        (
            [PersonalizationSegmentId] INT NOT NULL
        );

        INSERT INTO @PersonalizationSegments
        SELECT TRY_CONVERT(int, value)
        FROM STRING_SPLIT(@PersonalizationSegmentString, ',')
        WHERE TRY_CONVERT(int, value) IS NOT NULL;

        DECLARE @PersonalizationSegmentCount INT = (
            SELECT COUNT(*)
            FROM @PersonalizationSegments
            WHERE [PersonalizationSegmentId] > 0
        );

        -- Create a map from person IDs to personalization segments for this communication.
        IF OBJECT_ID('tempdb..#PersonSegmentList') IS NOT NULL
        BEGIN
            DROP TABLE #PersonSegmentList;
        END

        SELECT psl.[PersonId]
            , psl.[PersonalizationEntityId]
        INTO #PersonSegmentList
        FROM (
            SELECT DISTINCT pa.[PersonId]
                , pap.[PersonalizationEntityId]
            FROM [PersonAliasPersonalization] pap
            INNER JOIN [PersonAlias] pa
                ON pa.[Id] = pap.[PersonAliasId]
            WHERE pap.[PersonalizationType] = @PersonalizationTypeSegment
                AND pap.[PersonalizationEntityId] IN (
                    SELECT [PersonalizationSegmentId]
                    FROM @PersonalizationSegments
                )
        ) psl;

        CREATE CLUSTERED INDEX CX_PersonSegmentList
            ON #PersonSegmentList([PersonId]);

        -- Select person + primary alias ID along with communication preference for current, active list group members.
        IF OBJECT_ID('tempdb..#ListMembers') IS NOT NULL
        BEGIN
            DROP TABLE #ListMembers;
        END

        SELECT lm.[PersonId]
            , lm.[PrimaryAliasId]
            , lm.[MediumEntityTypeId]
        INTO #ListMembers
        FROM (
            SELECT p.[Id] AS [PersonId]
                , p.[PrimaryAliasId]
                , met.[MediumEntityTypeId]
            FROM [GroupMember] gm
            INNER JOIN [Person] p
                ON p.[Id] = gm.[PersonId]
            LEFT OUTER JOIN @MediumEntityTypeByCommunicationTypes met
                ON met.[CommunicationType] =
                    CASE
                        WHEN gm.[CommunicationPreference] = @NoCommunicationPreference THEN p.[CommunicationPreference]
                        ELSE gm.[CommunicationPreference]
                    END
            WHERE gm.[GroupId] = @ListGroupId
                AND gm.[GroupMemberStatus] = @GroupMemberStatusActive
                AND (
                    @FutureSendDateTime IS NULL
                    -- If this is a scheduled communication, don't include Members
                    -- that were added after the scheduled FutureSendDateTime.
                    OR COALESCE(gm.[DateTimeAdded], gm.[CreatedDateTime]) < @FutureSendDateTime
                    OR (
                        gm.[DateTimeAdded] IS NULL
                        AND gm.[CreatedDateTime] IS NULL
                    )
                )
                AND p.[PrimaryAliasId] IS NOT NULL
                AND (
                    -- Either the communication isn't tied to any personalization segments.
                    @PersonalizationSegmentCount = 0
                    -- OR.. filter down to only those list members who are in the specified
                    -- personalization segments.
                    OR (
                        -- They must belong to at least one of the specified segments.
                        @SegmentCriteria = @SegmentCriteriaAny
                        AND EXISTS (
                            SELECT 1
                            FROM #PersonSegmentList
                            WHERE [PersonId] = gm.[PersonId]
                        )
                    )
                    OR (
                        -- They must belong to ALL of the specified segments.
                        @SegmentCriteria = @SegmentCriteriaAll
                        AND (
                            SELECT COUNT(*)
                            FROM #PersonSegmentList
                            WHERE [PersonId] = gm.[PersonId]
                        ) = @PersonalizationSegmentCount
                    )
                )
        ) lm;

        CREATE CLUSTERED INDEX CX_ListMembers
            ON #ListMembers([PersonId]);

        CREATE NONCLUSTERED INDEX IX_ListMembers_PrimaryAliasId
            ON #ListMembers([PrimaryAliasId])
            INCLUDE ([MediumEntityTypeId]);

        -- Create a map from person IDs to person alias IDs to minimize the risk of excessive table locking during the upsert.
        IF OBJECT_ID('tempdb..#PersonIdMappings') IS NOT NULL
        BEGIN
            DROP TABLE #PersonIdMappings;
        END

        SELECT pa.[PersonId]
            , pa.[Id] AS [PersonAliasId]
        INTO #PersonIdMappings
        FROM [PersonAlias] pa
        INNER JOIN #ListMembers lm
            ON lm.[PersonId] = pa.[PersonId];

        CREATE CLUSTERED INDEX CX_PersonIdMappings
            ON #PersonIdMappings([PersonId]);

        CREATE NONCLUSTERED INDEX IX_PersonIdMappings_PersonAliasId
            ON #PersonIdMappings([PersonAliasId]);

        -- Upsert current, active list members to [CommunicationRecipient] records.
        MERGE [CommunicationRecipient] AS t
        USING #ListMembers AS s
            ON t.[CommunicationId] = @CommunicationId
                AND EXISTS (
                    SELECT 1
                    FROM #PersonIdMappings pm
                    WHERE pm.[PersonId] = s.[PersonId]
                        AND pm.[PersonAliasId] = t.[PersonAliasId]
                )
        WHEN MATCHED AND t.[Status] = @RecipientStatusPending THEN
            -- Update each pending recipient to reflect their current list member or person communication
            -- preference IF the commmunication type is recipient preference; otherwise, ensure the selected
            -- medium type matches that of the communication.
            UPDATE SET t.[MediumEntityTypeId] = CASE
                    WHEN @CommunicationType = @CommunicationTypeRecipientPreference
                        AND s.[MediumEntityTypeId] IS NOT NULL
                        THEN s.[MediumEntityTypeId]
                    ELSE @CommunicationMediumEntityTypeId
                    END
                , [ModifiedDateTime] = GETDATE()
        WHEN NOT MATCHED BY TARGET THEN
            -- Add new [CommunicationRecipient] records for list members not yet reflected in this table.
            INSERT
            (
                [CommunicationId]
                , [Status]
                , [Guid]
                , [CreatedDateTime]
                , [ModifiedDateTime]
                , [PersonAliasId]
                , [MediumEntityTypeId]
            )
            VALUES
            (
                @CommunicationId
                , @RecipientStatusPending
                , NEWID()
                , GETDATE()
                , GETDATE()
                , s.[PrimaryAliasId]
                , CASE
                    WHEN @CommunicationType = @CommunicationTypeRecipientPreference
                        AND s.[MediumEntityTypeId] IS NOT NULL
                        THEN s.[MediumEntityTypeId]
                    ELSE @CommunicationMediumEntityTypeId
                    END
            )
        OPTION (RECOMPILE);

        -- Batch delete pending [CommunicationRecipient] records that do not have a corresponding list member record.
        WHILE 1 = 1
        BEGIN
            WITH [DeleteRecipients] AS (
                SELECT TOP 1500 cr.[Id]
                FROM [CommunicationRecipient] cr WITH (READPAST)
                WHERE cr.[CommunicationId] = @CommunicationId
                    AND cr.[Status] = @RecipientStatusPending
                    AND NOT EXISTS (
                        SELECT 1
                        FROM #ListMembers lm
                        WHERE lm.[PrimaryAliasId] = cr.[PersonAliasId]
                    )
                ORDER BY cr.[Id]
            )
            DELETE cr
            FROM [CommunicationRecipient] cr WITH (ROWLOCK, READPAST)
            INNER JOIN [DeleteRecipients] dr
                ON dr.[Id] = cr.[Id];

            IF @@ROWCOUNT > 0 CONTINUE;

            -- Break only if there are truly no more recipients to delete.
            IF NOT EXISTS (
                SELECT 1
                FROM [CommunicationRecipient] cr
                WHERE cr.[CommunicationId] = @CommunicationId
                    AND cr.[Status] = @RecipientStatusPending
                    AND NOT EXISTS (
                        SELECT 1
                        FROM #ListMembers lm
                        WHERE lm.[PrimaryAliasId] = cr.[PersonAliasId]
                    )
            ) BREAK;

            WAITFOR DELAY '00:00:00.100';
        END

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRAN;

        THROW;
    END CATCH

END