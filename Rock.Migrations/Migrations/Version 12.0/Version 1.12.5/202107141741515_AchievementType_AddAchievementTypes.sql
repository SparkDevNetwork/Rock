DECLARE @EntityTypeIdAccumulativeAchievementComponent INT = (
        SELECT TOP 1 Id
        FROM EntityType
        WHERE [Guid] = '05D8CD17-E07D-4927-B9C4-5018F7C4B715'
        )
    , @EntityTypeIdStreakAchievementComponent INT = (
        SELECT TOP 1 Id
        FROM EntityType
        WHERE [Guid] = '174F0AFF-3A5E-4A20-AE8B-D8D83D43BACD'
        )
    , @EntityTypeIdStreak INT = (
        SELECT TOP 1 Id
        FROM EntityType
        WHERE [Guid] = 'D953B0A5-0065-4624-8844-10010DE01E5C'
        )
    , @EntityTypeIdPersonAlias INT = (
        SELECT TOP 1 Id
        FROM EntityType
        WHERE [Guid] = '90F5E87B-F0D5-4617-8AE9-EB57E673F36F'
        )
    , @StreakTypeIdWeeklyAttendance INT = (
        SELECT TOP 1 Id
        FROM StreakType
        WHERE [Guid] = 'B9FADD97-38A4-4141-B6DB-48154563A2A9'
        )
    , @BinaryFileIdMedal INT = (
        SELECT TOP 1 Id
        FROM BinaryFile
        WHERE [Guid] = '80331F03-4F4B-46B3-B789-8D34C12B4F42'
        )
    , @BinaryFileIdTrophy INT = (
        SELECT TOP 1 Id
        FROM BinaryFile
        WHERE [Guid] = '9A1503BC-D965-4BD4-AEA4-8039D4657201'
        )
    , @AchievementTypeGuidTenWeeksInARow UNIQUEIDENTIFIER = '21E6CC63-702B-4A5D-BC92-503B0F5CAF5D'
    , @AchievementTypeGuidTwentyWeeksInAYear UNIQUEIDENTIFIER = '67EA551D-C3A6-4339-9F39-F6F4E4DAB4EA'

BEGIN
    IF NOT EXISTS (
            SELECT *
            FROM AchievementType
            WHERE [Guid] = @AchievementTypeGuidTenWeeksInARow
            )
    BEGIN
        INSERT INTO [dbo].[AchievementType] (
            [ComponentEntityTypeId]
            , [MaxAccomplishmentsAllowed]
            , [AllowOverAchievement]
            , [IsActive]
            , [Name]
            , [Description]
            , [SourceEntityTypeId]
            , [AchieverEntityTypeId]
            , [ComponentConfigJson]
            , [IsPublic]
            , [ImageBinaryFileId]
            , [CustomSummaryLavaTemplate]
            , [Guid]
            )
        VALUES (
            @EntityTypeIdStreakAchievementComponent --[ComponentEntityTypeId]
            , 5 --[MaxAccomplishmentsAllowed]
            , 0 -- [AllowOverAchievement]
            , 1 -- [IsActive]
            , 'Ten Weeks in a row' -- [Name]
            , '' -- [Description]
            , @EntityTypeIdStreak -- [SourceEntityTypeId]
            , @EntityTypeIdPersonAlias -- [AchieverEntityTypeId]
            , '{"StreakType":"B9FADD97-38A4-4141-B6DB-48154563A2A9"}' -- [ComponentConfigJson]
            , 1 -- [IsPublic]
            , @BinaryFileIdTrophy -- [ImageBinaryFileId]
            , '' -- [CustomSummaryLavaTemplate]
            , @AchievementTypeGuidTenWeeksInARow -- [Guid]
            )
    END

    IF NOT EXISTS (
            SELECT *
            FROM AchievementType
            WHERE [Guid] = @AchievementTypeGuidTwentyWeeksInAYear
            )
    BEGIN
        INSERT INTO [dbo].[AchievementType] (
            [ComponentEntityTypeId]
            , [MaxAccomplishmentsAllowed]
            , [AllowOverAchievement]
            , [IsActive]
            , [Name]
            , [Description]
            , [SourceEntityTypeId]
            , [AchieverEntityTypeId]
            , [ComponentConfigJson]
            , [IsPublic]
            , [ImageBinaryFileId]
            , [CustomSummaryLavaTemplate]
            , [Guid]
            )
        VALUES (
            @EntityTypeIdAccumulativeAchievementComponent --[ComponentEntityTypeId]
            , 5 --[MaxAccomplishmentsAllowed]
            , 0 -- [AllowOverAchievement]
            , 1 -- [IsActive]
            , 'Twenty Weeks in a Year' -- [Name]
            , '' -- [Description]
            , @EntityTypeIdStreak -- [SourceEntityTypeId]
            , @EntityTypeIdPersonAlias -- [AchieverEntityTypeId]
            , '{"StreakType":"B9FADD97-38A4-4141-B6DB-48154563A2A9"}' -- [ComponentConfigJson]
            , 1 -- [IsPublic]
            , @BinaryFileIdMedal -- [ImageBinaryFileId]
            , '' -- [CustomSummaryLavaTemplate]
            , @AchievementTypeGuidTwentyWeeksInAYear -- [Guid]
            )
    END
END
