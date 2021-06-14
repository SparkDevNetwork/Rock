/*
 Script to populate a Rock RMS database with sample interaction data quickly. The data is not very random, but quickly generates interaction
 combinations of components and person aliases sequentially.

 This script assumes the following:
 - The Person table is populated.
 - Interaction components are populated.
*/

-- Parameters
declare
    -- Set this value to the number of interactions that will be created.
    @interactionsToAddCount int = 100000
    -- Days of spread for the interaction to occur. 30 means 30 days ago until today
    ,@daySpread int = 365
    -- Set this value to place a tag in the ForeignKey field of the sample data records for easier identification.
    ,@foreignKey nvarchar(100) = 'Interactions Sample Data'

-- Local variables
declare
    @createdDateTime datetime = (select convert(date, SYSDATETIME()))
    ,@createdByPersonAliasId int = (select pa.id from PersonAlias pa inner join Person p on pa.PersonId = p.Id where p.FirstName = 'Admin' and p.LastName = 'Admin');

set nocount on

print N'Create Interactions Sample Data: started.';

--
-- Create Interactions.
--
declare @interactionCounter int = 0;
declare @personAliasIds table ( id Int not null );
declare @componentIds table ( id Int not null );

-- Loop for each interaction
WHILE @interactionCounter < @interactionsToAddCount
BEGIN
    SET @interactionCounter += 1;

    IF(NOT EXISTS(SELECT 1 FROM @personAliasIds))
    BEGIN
        -- Get a list of person alias ids
        insert into @personAliasIds
        select pa.Id
        from 
            PersonAlias pa
            inner join Person p on pa.PersonId = p.Id
        where 
            p.IsSystem = 0
            and p.LastName <> 'Anonymous';
    END

    IF(NOT EXISTS(SELECT 1 FROM @componentIds))
    BEGIN
        -- Get a list of component ids
        insert into @componentIds
        select ic.Id
        from InteractionComponent ic;
    END

    DECLARE @interactionDateTime AS DATETIME = (SELECT DATEADD(DAY, -1 * FLOOR(RAND() * @daySpread + 1), GETDATE()))

    INSERT INTO Interaction (
        InteractionComponentId,
        PersonAliasId,
        InteractionDateTime,
        Guid,
        ForeignKey,
        CreatedDateTime,
        CreatedByPersonAliasId,
        InteractionDateKey
    )
    SELECT
        (SELECT TOP 1 Id FROM @componentIds),
        (SELECT TOP 1 Id FROM @personAliasIds),
        @interactionDateTime,
        NEWID(),
        @foreignKey,
        @createdDateTime,
        @createdByPersonAliasId,
        CAST(DATEPART(yyyy, @interactionDateTime) AS VARCHAR(4)) + CAST(DATEPART(mm, @interactionDateTime) AS VARCHAR(2)) + CAST(DATEPART(dd, @interactionDateTime) AS VARCHAR(2));

    -- Delete top component
    WITH cte AS (
        SELECT TOP 1 * FROM @componentIds
    ) DELETE FROM cte;

    -- Delete top person alias
    WITH cte AS (
        SELECT TOP 1 * FROM @personAliasIds
    ) DELETE FROM cte;

    -- Print if a multiple of 500
    IF (@interactionCounter % 500 = 0)
	BEGIN
  		PRINT concat(@interactionCounter, '/', @interactionsToAddCount);
    END
END

print N'Create Interactions Sample Data: completed.';
