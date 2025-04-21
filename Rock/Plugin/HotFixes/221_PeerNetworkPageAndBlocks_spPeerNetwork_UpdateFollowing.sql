/*
<doc>
    <summary>
        This stored procedure updates the peer network for an individual's following.
    </summary>
    <remarks>
        FOLLOWING PEER NETWORK NOTES
        1.  Any already-existing peer network connections will be updated with a current timestamp.
        2.  Any missing connections will be added.
        3.  A static relationship score of 5 will be enforced for all such connections.
        4.  Any no-longer-matching connections will be deleted (if an individual is no longer following someone they
            used to follow).
    </remarks>
    <code>
        EXEC [dbo].[spPeerNetwork_UpdateFollowing]
    </code>
</doc>
*/

CREATE PROCEDURE [dbo].[spPeerNetwork_UpdateFollowing]
AS
BEGIN

    DECLARE @FollowingScore INT = 5;

    DECLARE @PersonAliasEntityId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '90F5E87B-F0D5-4617-8AE9-EB57E673F36F');
    DECLARE @FollowingConnectionTypeValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '84E0360E-0828-E5A5-4BCC-F3113BE338A1');

    ;WITH FollowingDataset AS (
        SELECT 
            followerP.[Id] AS [FollowerPersonId]
            , followingP.[Id] AS [FollowingPersonId]
            , MIN(f.[CreatedDateTime]) AS [RelationshipStartDate]
        FROM [dbo].[Following] f
            INNER JOIN [PersonAlias] followingPA ON followingPA.[Id] = f.[EntityId]
            INNER JOIN [Person] followingP ON followingP.[Id] = followingPA.[PersonId]
            INNER JOIN [PersonAlias] followerPA ON followerPA.[Id] = f.[PersonAliasId]
            INNER JOIN [Person] followerP ON followerP.[Id] = followerPA.[PersonId]
        WHERE [EntityTypeId] = @PersonAliasEntityId
            AND (
                [PurposeKey] IS NULL
                OR [PurposeKey] = ''
            )
        GROUP BY followerP.[Id]
            , followingP.[Id]
    )

    MERGE [PeerNetwork] AS t
    USING [FollowingDataset] AS s
    ON t.[TargetPersonId] = s.[FollowingPersonId]
        AND t.[SourcePersonId] = s.[FollowerPersonId]
        AND t.[RelationshipTypeValueId] = @FollowingConnectionTypeValueId
    WHEN MATCHED THEN
        -- Update each matching [PeerNetwork] record with a current time stamp and ensure they have the expected following score.
        UPDATE SET t.[RelationshipStartDate] = s.[RelationshipStartDate]
            , t.[LastUpdateDateTime] = GETDATE()
            , t.[RelationshipScore] = @FollowingScore
            , t.[RelationshipScoreLastUpdateValue] = @FollowingScore
    WHEN NOT MATCHED BY TARGET THEN
        -- Add new [PeerNetwork] records for following relationships not yet reflected in this table.
        INSERT (
            [RelationshipTypeValueId]
            , [RelationshipStartDate]
            , [RelationshipScore]
            , [RelationshipScoreLastUpdateValue]
            , [RelationshipTrend]
            , [LastUpdateDateTime]
            , [SourcePersonId]
            , [TargetPersonId]
        )
        VALUES (
            @FollowingConnectionTypeValueId     -- [RelationshipTypeValueId]
            , s.[RelationshipStartDate]         -- [RelationshipStartDate]
            , @FollowingScore                   -- [RelationshipScore]
            , @FollowingScore                   -- [RelationshipScoreLastUpdateValue]
            , 0                                 -- [RelationshipTrend]
            , GETDATE()                         -- [LastUpdateDateTime]
            , s.[FollowerPersonId]              -- [SourcePersonId]
            , s.[FollowingPersonId]             -- [TargetPersonId]
        )
    WHEN NOT MATCHED BY SOURCE AND t.[RelationshipTypeValueId] = @FollowingConnectionTypeValueId THEN
        -- Remove any preexisting [PeerNetwork] records that no longer represent active following relationships.
        DELETE;

END