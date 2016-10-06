-- Get the person id for the 'Deleted Person' record
DECLARE @PersonId int = ( SELECT TOP 1 [Id] FROM [Person] WHERE [FirstName] = 'Deleted' and [LastName] = 'Person' ORDER BY [Id] )

-- Find all the person ids that were not synced from Arena and are not a known system person
SELECT [Id]
INTO DeletePersonIds
FROM [Person]
WHERE [Id] NOT IN ( SELECT [foreign_key] FROM [Arena].[dbo].[core_person] ) 
AND [Id] NOT IN ( 1,347459,361042,361190,379595,373932,@PersonId ) 

-- Update any of the person alias records for these people to point now to admin instead
UPDATE [PersonAlias]
SET [PersonId] = @PersonId
WHERE [PersonId] IN ( SELECT [Id] FROM DeletePersonIds )

-- Delete any group members with with these person ids
DELETE [GroupMember]
WHERE [PersonId] IN ( SELECT [Id] FROM DeletePersonIds )

-- Delete the people
DELETE [Person]
WHERE [Id] IN ( SELECT [Id] FROM DeletePersonIds )

DROP TABLE DeletePersonIds
