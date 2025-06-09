/*
<doc>
    <summary>
        This stored procedure updates the peer network for groups.
    </summary>
    <remarks>
        GROUP PEER NETWORK NOTES
        1.  The group processing will have 3 phases:
            a. Update the peer network connections in the PeerNetwork table.
            b. Recalc the relationship score for each connection with logic for time adjusting. We need to do this as a
               separate step as some of the individuals who previously had peer connections are no longer in those groups.
            c. Clean-up
                - A person could be in a group multiple times (with different roles). In these cases we'll delete the
                  lowest peer network relationship score for that person.
                - Delete all group connections whose scores have gone below 0.
        2.  We only want to connect people who were in the group at the same time. We therefore need to look at the
            archive and inactive dates to ensure they are before the date the source person was added to the group.
        3.  When a person is deleted from a group we don't want to delete their peer network. Instead we want to slowly
            decrease the relationship score until it reaches 0.
        4.  There's logic in here to handle duplicate Group > Person > Roles. This can happen if a person is unarchived.
            The unarchive process could create a new record in the GroupMember table. This is being changed in v17 to
            prevent this, but I think we should keep the logic just in case. To deal with this we set the RelationshipEndDate
            to be 12/31/9999 when the relationship is active (this helps us choose the correct one as null does not work
            with MAX()). In the MERGE we'll convert it back to a null.
    </remarks>
    <code>
        EXEC [dbo].[spPeerNetwork_UpdateGroupConnections]
    </code>
</doc>
*/

CREATE PROCEDURE [dbo].[spPeerNetwork_UpdateGroupConnections]
AS
BEGIN

    DECLARE @GroupMemberConnectionTypeValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = 'CB51DC46-FBDB-43DA-B7F3-60E7C6E70F40')

    /*
        STEP 1: MERGE (ADD/UPDATE/DELETE) PEER NETWORK CONNECTIONS
    */

    -- This creates a dataset of all relevant group members. We'll use this to cross join to get a matrix of relationships.
    ;WITH GroupMemberDataSet AS (
        SELECT
            g.[Id] AS [GroupId]
            , gm.[PersonId] AS [PersonId]
            , gm.[GroupRoleId]
            , gm.[DateTimeAdded] AS [GroupStartDateTime]
            , gm.[ArchivedDateTime]
            , gm.[InactiveDateTime]
            , gt.[Id] AS [GroupTypeId]
            , gtr.[IsLeader]
            , gm.[IsArchived]
            , gm.[GroupMemberStatus]
            , CASE 
                WHEN gm.[IsArchived] = 1 THEN gm.[ArchivedDateTime]
                WHEN gm.[GroupMemberStatus] = 0 THEN gm.[InactiveDateTime]
                ELSE '12/31/9999'
              END AS [GroupMemberInactiveDate]
        FROM [Group] g
            INNER JOIN [GroupMember] gm ON gm.[GroupId] = g.[Id]
            INNER JOIN [GroupType] gt ON gt.[Id] = g.[GroupTypeId]
            INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
        WHERE gt.[IsPeerNetworkEnabled] = 1
            AND (
                -- Either the group hasn't overridden the group type's strength and the group type's strength is not "None",
                (g.[RelationshipStrengthOverride] IS NULL AND gt.[RelationshipStrength] != 0)
                -- or the group has overridden with something other than "None".
                OR g.[RelationshipStrengthOverride] != 0
            )
            AND gtr.[IsExcludedFromPeerNetwork] != 1 -- Ignore people in roles that have disabled peer networks.
    ),

    GroupMemberConnectionDataset AS (
        SELECT
            [SourcePersonId]
            , [TargetPersonId]
            , [GroupId]
            , [SourceRoleId]
            , MIN([RelationshipStartDate]) AS [RelationshipStartDate]
            , MAX([RelationshipEndDate]) AS [RelationshipEndDate]
            , MIN([Caption]) AS [Caption]
        FROM (

            SELECT
                s.[PersonId] AS [SourcePersonId]
                , s.[GroupId]
                , s.[GroupRoleId] AS [SourceRoleId]
                , s.[GroupTypeId]
                , s.[IsLeader] AS [SourceIsLeader]
                , t.[TargetPersonId]
                , t.[TargetRoleId]
                , t.[TargetIsLeader]
                , g.[Name] AS [Caption]
                -- The date that the relationship started. This is the newer date that someone was added to the group.
                , CASE
                    WHEN s.[GroupStartDateTime] < t.[GroupStartDateTime] THEN CAST(t.[GroupStartDateTime] AS DATE)
                    ELSE CAST(s.[GroupStartDateTime] AS DATE)
                  END AS [RelationshipStartDate]

                -- The date that the relationship ended.
                , CASE
                    -- If the group is inactive then return the inactivate date of the group.
                    WHEN g.[IsActive] = 0 THEN g.[InactiveDateTime]
                    -- If the group is archived then return the archived date time.
                    WHEN g.[IsArchived] = 1 THEN g.[ArchivedDateTime]
                    -- Both individuals are active so return the max value.
                    WHEN t.[GroupMemberInactiveDate] = '12/31/9999' AND s.[GroupMemberInactiveDate] = '12/31/9999' THEN '12/31/9999'
                    -- If either or both are inactive then we'll take the oldest non-null inactive date since they left first.
                    WHEN t.[GroupMemberInactiveDate] != '12/31/9999' OR s.[GroupMemberInactiveDate] != '12/31/9999' THEN
                        CASE
                            WHEN t.[GroupMemberInactiveDate] = '12/31/9999' THEN s.[GroupMemberInactiveDate]
                            WHEN s.[GroupMemberInactiveDate] = '12/31/9999' THEN t.[GroupMemberInactiveDate]
                            WHEN t.[GroupMemberInactiveDate] < s.[GroupMemberInactiveDate] THEN t.[GroupMemberInactiveDate]
                            ELSE s.[GroupMemberInactiveDate]
                        END
                    ELSE '12/31/9999'
                  END AS [RelationshipEndDate]

            FROM [GroupMemberDataSet] s
                INNER JOIN [Group] g ON g.[Id] = s.[GroupId]
                CROSS APPLY (
                    -- For each group member [s], get all other group members [t].
                    SELECT
                        gdTarget.[PersonId] AS [TargetPersonId]
                        , gdTarget.[GroupStartDateTime] AS [GroupStartDateTime]
                        , gdTarget.[GroupRoleId] AS [TargetRoleId]
                        , gdTarget.[IsLeader] AS [TargetIsLeader]
                        , gdTarget.[GroupMemberInactiveDate]
                    FROM [GroupMemberDataSet] gdTarget
                    WHERE s.[PersonId] <> gdTarget.[PersonId]
                        AND s.[GroupId] = gdTarget.[GroupId]
                ) t

            WHERE
                -- We want to exclude people who were not active in the group at the same time. Active relationships will have a GroupMemberInactiveDate of '12/31/9999'.
                s.[GroupStartDateTime] <= t.[GroupMemberInactiveDate]
                AND t.[GroupStartDateTime] <= s.[GroupMemberInactiveDate]
                AND s.[GroupMemberInactiveDate] >= t.[GroupStartDateTime]
                AND t.[GroupMemberInactiveDate] >= s.[GroupStartDateTime]
        ) x
        -- The group by below is needed as there are duplicate people in the same group with the same role. This should not happen, but it does.
        GROUP BY [SourcePersonId], [TargetPersonId], [GroupId], [SourceRoleId]
    )

    MERGE [PeerNetwork] AS t
    USING [GroupMemberConnectionDataset] AS s
    ON
        t.[TargetPersonId] = s.[TargetPersonId]
        AND t.[SourcePersonId] = s.[SourcePersonId]
        AND t.[RelatedEntityId] = s.[GroupId]
        AND t.[ClassificationEntityId] = s.[SourceRoleId]
        AND t.[RelationshipTypeValueId] = @GroupMemberConnectionTypeValueId
    WHEN MATCHED THEN
        -- Update each matching [PeerNetwork] record to reflect the group members' last-calculated relationship start and end dates.
        UPDATE SET t.[RelationshipEndDate] =
              CASE
                WHEN s.[RelationshipEndDate] = '12/31/9999' THEN NULL
                ELSE s.[RelationshipEndDate]
              END
            , t.[RelationshipStartDate] = s.[RelationshipStartDate]
    WHEN NOT MATCHED BY TARGET THEN
        -- Add new [PeerNetwork] records for group members not yet reflected in this table.
        INSERT (
            [RelationshipTypeValueId]
            , [RelationshipStartDate]
            , [RelationshipEndDate]
            , [RelationshipScore]
            , [RelationshipScoreLastUpdateValue]
            , [RelationshipTrend]
            , [LastUpdateDateTime]
            , [SourcePersonId]
            , [TargetPersonId]
            , [Caption]
            , [RelatedEntityId]
            , [ClassificationEntityId]
        )
        VALUES (
            @GroupMemberConnectionTypeValueId                           -- [RelationshipTypeValueId]
            , s.[RelationshipStartDate]                                 -- [RelationshipStartDate]
            , CASE
                WHEN s.[RelationshipEndDate] = '12/31/9999' THEN NULL   -- [RelationshipEndDate]
                ELSE s.[RelationshipEndDate]
              END
            , 0                                                         -- [RelationshipScore]
            , 0                                                         -- [RelationshipScoreLastUpdateValue]
            , 0                                                         -- [RelationshipTrend]
            , GETDATE()                                                 -- [LastUpdateDateTime]
            , s.[SourcePersonId]                                        -- [SourcePersonId]
            , s.[TargetPersonId]                                        -- [TargetPersonId]
            , s.[Caption]                                               -- [Caption]
            , s.[GroupId]                                               -- [RelatedEntityId]
            , s.[SourceRoleId]                                          -- [ClassificationEntityId]
        )
    WHEN NOT MATCHED BY SOURCE AND t.[RelationshipTypeValueId] = @GroupMemberConnectionTypeValueId AND t.[RelationshipEndDate] IS NULL THEN
        -- Start the relationship score decay process by setting these members' relationship end date to right now.
        UPDATE SET t.[RelationshipEndDate] = GETDATE();

    /*
        STEP 2: UPDATE RELATIONSHIP SCORING
    */

    /*  NOTES
        The calculation for Relationship Score is

            - RelationshipGrowthAdjustmentMultiplier: This field determines the monthly growth rate. If the relationship
              is active, 10% of the base score is added each month. If the relationship is inactive, 20% of the base
              score is subtracted each month.
            - RelationshipGrowthEnabledMultiplier: This determines if the relationship should grow over time. It will be
              a 1 when enabled and 0 if not.
            - RelationshipGrowthDate: This date is used to calculate growth. When the relationship is active, it represents
              the start date. When the relationship is inactive, it is the inactivation date.
    */

    UPDATE [PeerNetwork]
    SET
        [PeerNetwork].[RelationshipScore] =
        CASE
            -- This could happen if the source or target group is deleted.
            WHEN s.[RelationshipScore] IS NULL THEN 0
            ELSE s.[RelationshipScore]
        END
        , [PeerNetwork].[RelationshipScoreLastUpdateValue] = s.[PreviousRelationshipScore]
        , [PeerNetwork].[LastUpdateDateTime] = GETDATE()
        , [PeerNetwork].[RelationshipTrend] =
            CASE
                WHEN [PeerNetwork].[RelationshipScore] > s.[RelationshipScore] THEN -1
                WHEN [PeerNetwork].[RelationshipScore] < s.[RelationshipScore] THEN 1
                ELSE 0
            END
    FROM
    (
        -- [Re-]score each "Group Connection" [PeerNetwork] record based on group/group type configuration & members' roles within the group.
        SELECT
            x.*
            , CASE
                WHEN [IsRelationshipGrowthEnabled] = 1 OR [RelationshipGrowthAdjustmentMultiplier] < 0 THEN
                    -- If relationship growth is enabled OR it's time to start subtracting this member's score, factor in the growth[/decay] adjustment and role-based multipliers.
                    CASE
                        WHEN [SourceIsLeader] = 1 AND [TargetIsLeader] = 1 THEN
                            ([RelationshipStrength] + (DATEDIFF(m, [RelationshipGrowthDate], GETDATE()) * [RelationshipGrowthAdjustmentMultiplier])) * [LeaderToLeaderRelationshipMultiplier]
                        WHEN [SourceIsLeader] = 1 AND [TargetIsLeader] = 0 THEN
                            ([RelationshipStrength] + (DATEDIFF(m, [RelationshipGrowthDate], GETDATE()) * [RelationshipGrowthAdjustmentMultiplier])) * [LeaderToNonLeaderRelationshipMultiplier]
                        WHEN [SourceIsLeader] = 0 AND [TargetIsLeader] = 1 THEN
                            ([RelationshipStrength] + (DATEDIFF(m, [RelationshipGrowthDate], GETDATE()) * [RelationshipGrowthAdjustmentMultiplier])) * [NonLeaderToLeaderRelationshipMultiplier]
                        WHEN [SourceIsLeader] = 0 AND [TargetIsLeader] = 0 THEN
                            ([RelationshipStrength] + (DATEDIFF(m, [RelationshipGrowthDate], GETDATE()) * [RelationshipGrowthAdjustmentMultiplier])) * [NonLeaderToNonLeaderRelationshipMultiplier]
                    END
                ELSE
                    -- If relationship growth is disabled, assign a static score, adjusted only by the role-based multipliers.
                    CASE
                        WHEN [SourceIsLeader] = 1 AND [TargetIsLeader] = 1 THEN [RelationshipStrength] * [LeaderToLeaderRelationshipMultiplier]
                        WHEN [SourceIsLeader] = 1 AND [TargetIsLeader] = 0 THEN [RelationshipStrength] * [LeaderToNonLeaderRelationshipMultiplier]
                        WHEN [SourceIsLeader] = 0 AND [TargetIsLeader] = 1 THEN [RelationshipStrength] * [NonLeaderToLeaderRelationshipMultiplier]
                        WHEN [SourceIsLeader] = 0 AND [TargetIsLeader] = 0 THEN [RelationshipStrength] * [NonLeaderToNonLeaderRelationshipMultiplier]
                    END
              END AS [RelationshipScore]
        FROM (
            -- Get all existing "Group Connection" [PeerNetwork] records, along with their group/group type configuration & members' roles.
            SELECT
                pn.[SourcePersonId]
                , pn.[TargetPersonId]
                , pn.[RelatedEntityId]
                , pn.[ClassificationEntityId]
                , pn.[RelationshipScore] AS [PreviousRelationshipScore]

                , CASE
                    WHEN pn.[RelationshipEndDate] IS NOT NULL THEN pn.[RelationshipEndDate]
                    ELSE pn.[RelationshipStartDate]
                  END AS [RelationshipGrowthDate]

                , CASE WHEN pn.[RelationshipEndDate] IS NULL
                    THEN .1
                    ELSE -.2
                  END AS [RelationshipGrowthAdjustmentMultiplier]

                , CASE
                    WHEN g.[RelationshipGrowthEnabledOverride] IS NOT NULL THEN g.[RelationshipGrowthEnabledOverride]
                    ELSE gt.[RelationshipGrowthEnabled]
                  END AS [IsRelationshipGrowthEnabled]

                , CASE
                    WHEN g.[RelationshipStrengthOverride] IS NOT NULL THEN g.[RelationshipStrengthOverride]
                    ELSE gt.[RelationshipStrength]
                  END AS [RelationshipStrength]

                , CASE
                    WHEN g.[LeaderToLeaderRelationshipMultiplierOverride] IS NOT NULL THEN g.[LeaderToLeaderRelationshipMultiplierOverride]
                    ELSE gt.[LeaderToLeaderRelationshipMultiplier]
                  END AS [LeaderToLeaderRelationshipMultiplier]

                , CASE
                    WHEN g.[LeaderToNonLeaderRelationshipMultiplierOverride] IS NOT NULL THEN g.[LeaderToNonLeaderRelationshipMultiplierOverride]
                    ELSE gt.[LeaderToNonLeaderRelationshipMultiplier]
                  END AS [LeaderToNonLeaderRelationshipMultiplier]

                , CASE
                    WHEN g.[NonLeaderToLeaderRelationshipMultiplierOverride] IS NOT NULL THEN g.[NonLeaderToLeaderRelationshipMultiplierOverride]
                    ELSE gt.[NonLeaderToLeaderRelationshipMultiplier]
                  END AS [NonLeaderToLeaderRelationshipMultiplier]

                , CASE
                    WHEN g.[NonLeaderToNonLeaderRelationshipMultiplierOverride] IS NOT NULL THEN g.[NonLeaderToNonLeaderRelationshipMultiplierOverride]
                    ELSE gt.[NonLeaderToNonLeaderRelationshipMultiplier]
                  END AS [NonLeaderToNonLeaderRelationshipMultiplier]

                , ( SELECT TOP 1 [IsLeader]
                    FROM [GroupTypeRole] tgtr
                        INNER JOIN [GroupMember] tgm
                            ON tgm.[GroupRoleId] = tgtr.[Id]
                            AND tgm.[PersonId] = pn.[TargetPersonId]
                    ORDER BY tgtr.[IsLeader] DESC
                  ) AS [TargetIsLeader]

                , gtr.[IsLeader] AS [SourceIsLeader]
            FROM [PeerNetwork] pn
                LEFT OUTER JOIN [Group] g ON g.[Id] = pn.[RelatedEntityId]
                LEFT OUTER JOIN [GroupTypeRole] gtr ON gtr.[Id] = pn.[ClassificationEntityId]
                LEFT OUTER JOIN [GroupType] gt ON gt.[Id] = g.[GroupTypeId]
            WHERE
                pn.[RelationshipTypeValueId] =  @GroupMemberConnectionTypeValueId
        ) x
    ) s
    WHERE
        s.[SourcePersonId] = [PeerNetwork].[SourcePersonId]
        AND s.[TargetPersonId] = [PeerNetwork].[TargetPersonId]
        AND s.[RelatedEntityId] = [PeerNetwork].[RelatedEntityId]
        AND s.[ClassificationEntityId] = [PeerNetwork].[ClassificationEntityId]
        AND [PeerNetwork].[RelationshipTypeValueId] = @GroupMemberConnectionTypeValueId;

    /*
        STEP 3: CLEAN-UP
    */

    -- Delete relationships that are less than 0.
    DELETE FROM [PeerNetwork]
    WHERE [RelationshipScore] <= 0;

    -- Delete duplicate peer networks (note this is a global delete and should probably be added as a separate procedure when done).
    ;WITH PeerNetworkDuplicates AS (
        SELECT
            [SourcePersonId]
            , [TargetPersonId]
            , [RelationshipStartDate]
            , [RelationshipTypeValueId]
            , [RelatedEntityId]
            , [ClassificationEntityId]
            , ROW_NUMBER() OVER( PARTITION BY [SourcePersonId]
                                     , [TargetPersonId]
                                     , [RelationshipTypeValueId]
                                     , [RelatedEntityId]
                                     , [ClassificationEntityId] 
                                 ORDER BY [RelationshipScore], [RelationshipStartDate] ) AS [DuplicateCount]
        FROM [PeerNetwork] )

    DELETE FROM PeerNetworkDuplicates WHERE [DuplicateCount] > 1;

END