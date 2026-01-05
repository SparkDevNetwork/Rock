/* =============================================================================================
 Author:        Code Copilot
 Created:       2025-09-15
 Description:   SQL Server 2016–compatible populate script for [dbo].[AuthAuditLog].
                - Inserts N configurable fake audit rows with semi-coherent pre/post logic.
                - The EntityType/EntityId data is not 100% correct (this is mostly for load testing)
                - Weighted Action distribution: View/Edit roughly equal and ~10% Administrate.
                - SpecialRole handling: 0 => target is either a Group or a Person (mutually exclusive);
                  >0 => target Person/Group are NULL.
 ============================================================================================= */

SET NOCOUNT ON;

/* ===============================
   Configuration
   =============================== */
DECLARE @RowCount INT = 5000;                  -- how many rows to insert
DECLARE @DateStart DATETIME = '2023-01-01';    -- audit time window start
DECLARE @DateEnd   DATETIME = GETDATE();       -- audit time window end
DECLARE @DoCommit  BIT = 1;                    -- 0 = preview/rollback; 1 = commit

-- Weighted Action distribution (must sum ~100; we normalize if not)
DECLARE @PctView INT = 45;         -- target ~45%
DECLARE @PctEdit INT = 45;         -- target ~45%
DECLARE @PctAdministrate INT = 10; -- target ~10%

/* ===============================
   Preconditions
   =============================== */


/* Normalize weights to 100 (integer safe) */
DECLARE @Sum INT = @PctView + @PctEdit + @PctAdministrate;
IF @Sum <= 0
BEGIN
    SET @PctView = 45; SET @PctEdit = 45; SET @PctAdministrate = 10; SET @Sum = 100;
END
ELSE IF @Sum <> 100
BEGIN
    -- scale and fix residual to ensure sum=100
    DECLARE @V INT = (100 * @PctView) / @Sum;
    DECLARE @E INT = (100 * @PctEdit) / @Sum;
    DECLARE @A INT = 100 - (@V + @E);
    SET @PctView = @V; SET @PctEdit = @E; SET @PctAdministrate = @A;
END

/* ===============================
   Seed sets (actors, entity types, groups, person targets)
   =============================== */
IF OBJECT_ID('tempdb..#Actors') IS NOT NULL DROP TABLE #Actors;
CREATE TABLE #Actors (RowNum INT IDENTITY(1,1) PRIMARY KEY, PersonAliasId INT NOT NULL);
INSERT INTO #Actors(PersonAliasId)
SELECT pa.Id
FROM dbo.PersonAlias pa
WHERE pa.PersonId = pa.AliasPersonId; -- primary alias only

IF OBJECT_ID('tempdb..#TargetsPersons') IS NOT NULL DROP TABLE #TargetsPersons;
CREATE TABLE #TargetsPersons (RowNum INT IDENTITY(1,1) PRIMARY KEY, PersonAliasId INT NOT NULL);
INSERT INTO #TargetsPersons(PersonAliasId)
SELECT pa.Id
FROM dbo.PersonAlias pa
WHERE pa.PersonId = pa.AliasPersonId;

IF OBJECT_ID('tempdb..#TargetsGroups') IS NOT NULL DROP TABLE #TargetsGroups;
CREATE TABLE #TargetsGroups (RowNum INT IDENTITY(1,1) PRIMARY KEY, GroupId INT NOT NULL);
IF OBJECT_ID('dbo.[Group]') IS NOT NULL
BEGIN
    INSERT INTO #TargetsGroups(GroupId)
    SELECT g.Id FROM dbo.[Group] g;
END
ELSE
BEGIN
    -- Fallback: synthesize a few ids if Group table does not exist
    INSERT INTO #TargetsGroups(GroupId)
    SELECT TOP (100) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) FROM sys.all_objects;
END

IF OBJECT_ID('tempdb..#EntityTypes') IS NOT NULL DROP TABLE #EntityTypes;
CREATE TABLE #EntityTypes (RowNum INT IDENTITY(1,1) PRIMARY KEY, EntityTypeId INT NOT NULL);
INSERT INTO #EntityTypes(EntityTypeId)
SELECT et.Id
FROM dbo.EntityType et; -- take all available entity types

DECLARE @ActorCount INT = (SELECT COUNT(*) FROM #Actors);
DECLARE @TargetPersonCount INT = (SELECT COUNT(*) FROM #TargetsPersons);
DECLARE @TargetGroupCount INT = (SELECT COUNT(*) FROM #TargetsGroups);
DECLARE @EntityTypeCount INT = (SELECT COUNT(*) FROM #EntityTypes);

IF @ActorCount = 0 OR @TargetPersonCount = 0 OR @EntityTypeCount = 0
BEGIN
    RAISERROR('Insufficient seed data: need PersonAlias (actors/targets) and EntityType rows.', 16, 1);
    RETURN;
END;

/* ===============================
   Stage N rows with coherent fake audits
   =============================== */
IF OBJECT_ID('tempdb..#NewRows') IS NOT NULL DROP TABLE #NewRows;
CREATE TABLE #NewRows (
    EntityTypeId INT NOT NULL,
    EntityId INT NOT NULL,
    [Action] NVARCHAR(50) NOT NULL,
    ChangeType INT NOT NULL,
    ChangeDateTime DATETIME NOT NULL,
    ChangeByPersonAliasId INT NOT NULL,
    PreAllowOrDeny NCHAR(1) NULL,
    PostAllowOrDeny NCHAR(1) NULL,
    PreOrder INT NULL,
    PostOrder INT NULL,
    GroupId INT NULL,
    SpecialRole INT NOT NULL,
    [Guid] UNIQUEIDENTIFIER NOT NULL,
    PersonAliasId INT NULL
);

DECLARE @TotalSeconds INT = CASE WHEN DATEDIFF(SECOND, @DateStart, @DateEnd) < 0 THEN 0 ELSE DATEDIFF(SECOND, @DateStart, @DateEnd) END;

-- Build a tally of N rows
IF OBJECT_ID('tempdb..#N') IS NOT NULL DROP TABLE #N;
SELECT TOP (@RowCount) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
INTO #N
FROM sys.all_objects a CROSS JOIN sys.all_objects b;

-- Generate rows
INSERT INTO #NewRows (
    EntityTypeId, EntityId, [Action], ChangeType, ChangeDateTime, ChangeByPersonAliasId,
    PreAllowOrDeny, PostAllowOrDeny, PreOrder, PostOrder, GroupId, SpecialRole, [Guid], PersonAliasId
)
SELECT 
    et.EntityTypeId,
    1 + ABS(CHECKSUM(NEWID())) % 5000 AS EntityId, -- arbitrary positive id (no FK)
    act.ActionName AS [Action],
    ct.ChangeType,
    DATEADD(SECOND, CASE WHEN @TotalSeconds = 0 THEN 0 ELSE ABS(CHECKSUM(NEWID())) % (@TotalSeconds + 1) END, @DateStart) AS ChangeDateTime,
    actor.PersonAliasId AS ChangeByPersonAliasId,
    -- PreAllow / PostAllow with ChangeType semantics
    CASE 
        WHEN ct.ChangeType = 0 THEN NULL                               -- Add: previously nothing
        WHEN ct.ChangeType = 1 THEN pre.AllowOrDeny                    -- Modify: had a value
        WHEN ct.ChangeType = 2 THEN pre.AllowOrDeny                    -- Delete: had a value
    END AS PreAllowOrDeny,
    CASE 
        WHEN ct.ChangeType = 2 THEN NULL                               -- Delete: becomes nothing
        WHEN ct.ChangeType = 0 THEN post.AddAllowOrDeny                -- Add: becomes a value
        WHEN ct.ChangeType = 1 THEN post.ModAllowOrDeny                -- Modify: could flip or stay
    END AS PostAllowOrDeny,
    CASE WHEN ct.ChangeType IN (1,2) THEN pre.Ord ELSE NULL END AS PreOrder,
    CASE WHEN ct.ChangeType IN (0,1) THEN post.Ord ELSE NULL END AS PostOrder,
    target.GroupId,
    sr.SpecialRole,
    NEWID() AS [Guid],
    target.PersonAliasId
FROM #N n
CROSS APPLY (
    SELECT EntityTypeId FROM #EntityTypes WHERE RowNum = ((ABS(CHECKSUM(NEWID())) % @EntityTypeCount) + 1)
) et
CROSS APPLY (
    SELECT PersonAliasId FROM #Actors WHERE RowNum = ((ABS(CHECKSUM(NEWID())) % @ActorCount) + 1)
) actor
CROSS APPLY (
    SELECT ABS(CHECKSUM(NEWID())) % 3 AS ChangeType
) ct
CROSS APPLY (
    -- Weighted action selection: View/Edit roughly equal; 10% Administrate
    SELECT ABS(CHECKSUM(NEWID())) % 100 AS r
) ar
CROSS APPLY (
    SELECT CASE 
             WHEN ar.r < @PctView THEN N'View'
             WHEN ar.r < (@PctView + @PctEdit) THEN N'Edit'
             ELSE N'Administrate'
           END AS ActionName
) act
CROSS APPLY (
    -- Decide SpecialRole (0=None, else 1..3)
    SELECT CAST(CASE WHEN ABS(CHECKSUM(NEWID())) % 4 = 0 THEN 0 ELSE (1 + (ABS(CHECKSUM(NEWID())) % 3)) END AS INT) AS SpecialRole
) sr
CROSS APPLY (
    -- Target: if SpecialRole=0 then 50/50 Person vs Group, else both NULL
    SELECT 
        CASE WHEN sr.SpecialRole = 0 AND ABS(CHECKSUM(NEWID())) % 2 = 0 THEN 
            (SELECT PersonAliasId FROM #TargetsPersons WHERE RowNum = ((ABS(CHECKSUM(NEWID())) % @TargetPersonCount) + 1))
        ELSE NULL END AS PersonAliasId,
        CASE WHEN sr.SpecialRole = 0 AND ABS(CHECKSUM(NEWID())) % 2 = 1 THEN 
            (SELECT GroupId FROM #TargetsGroups WHERE RowNum = ((ABS(CHECKSUM(NEWID())) % @TargetGroupCount) + 1))
        ELSE NULL END AS GroupId
) target
CROSS APPLY (
    -- Pre state for Modify/Delete
    SELECT 
        CASE WHEN ABS(CHECKSUM(NEWID())) % 2 = 0 THEN N'A' ELSE N'D' END AS AllowOrDeny,
        ABS(CHECKSUM(NEWID())) % 5 AS Ord
) pre
CROSS APPLY (
    -- Post state for Add/Modify
    SELECT 
        -- For Add: choose A/D independently; For Modify: 50% flip, 50% keep
        CASE WHEN ct.ChangeType = 0 THEN CASE WHEN ABS(CHECKSUM(NEWID())) % 2 = 0 THEN N'A' ELSE N'D' END
             WHEN ct.ChangeType = 1 THEN CASE WHEN ABS(CHECKSUM(NEWID())) % 2 = 0 THEN pre.AllowOrDeny ELSE (CASE WHEN pre.AllowOrDeny = N'A' THEN N'D' ELSE N'A' END) END
        END AS ModAllowOrDeny,
        CASE WHEN ct.ChangeType = 0 THEN CASE WHEN ABS(CHECKSUM(NEWID())) % 2 = 0 THEN N'A' ELSE N'D' END END AS AddAllowOrDeny,
        ABS(CHECKSUM(NEWID())) % 5 AS Ord
) post;

/* ===============================
   Insert
   =============================== */
DECLARE @Inserted INT = 0;
BEGIN TRAN;
BEGIN TRY
    INSERT INTO dbo.AuthAuditLog (
          EntityTypeId
        , EntityId
        , [Action]
        , ChangeType
        , ChangeDateTime
        , ChangeByPersonAliasId
        , PreAllowOrDeny
        , PostAllowOrDeny
        , PreOrder
        , PostOrder
        , GroupId
        , SpecialRole
        , [Guid]
        , PersonAliasId
    )
    SELECT 
          EntityTypeId
        , EntityId
        , [Action]
        , ChangeType
        , ChangeDateTime
        , ChangeByPersonAliasId
        , PreAllowOrDeny
        , PostAllowOrDeny
        , PreOrder
        , PostOrder
        , GroupId
        , SpecialRole
        , [Guid]
        , PersonAliasId
    FROM #NewRows;

    SET @Inserted = @@ROWCOUNT;

    IF (@DoCommit = 1)
    BEGIN
        COMMIT TRAN;
        PRINT CONCAT('Inserted AuthAuditLog rows: ', @Inserted);
    END
    ELSE
    BEGIN
        ROLLBACK TRAN;
        PRINT CONCAT('Preview only. Would have inserted AuthAuditLog rows: ', @Inserted);
    END
END TRY
BEGIN CATCH
    DECLARE @Err NVARCHAR(4000) = ERROR_MESSAGE();
    IF XACT_STATE() <> 0 ROLLBACK TRAN;
    RAISERROR('Populate_AuthAuditLog failed: %s', 16, 1, @Err);
    RETURN;
END CATCH;

/* ===============================
   Verify sample + distribution
   =============================== */
SELECT TOP (25)
      [Id]
    , [EntityTypeId]
    , [EntityId]
    , [Action]
    , [ChangeType]
    , [ChangeDateTime]
    , [ChangeByPersonAliasId]
    , [PreAllowOrDeny]
    , [PostAllowOrDeny]
    , [PreOrder]
    , [PostOrder]
    , [GroupId]
    , [SpecialRole]
    , [Guid]
    , [PersonAliasId]
FROM dbo.AuthAuditLog WITH (NOLOCK)
ORDER BY [Id] DESC;

-- Quick distribution check (optional)
SELECT [Action], COUNT(*) AS Cnt
FROM dbo.AuthAuditLog WITH (NOLOCK)
GROUP BY [Action]
ORDER BY [Action];
