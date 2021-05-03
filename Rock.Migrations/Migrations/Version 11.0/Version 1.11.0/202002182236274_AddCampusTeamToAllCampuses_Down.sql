-- un-link any 'Team Group' Groups from all existing Campuses
UPDATE [Campus]
SET [TeamGroupId] = NULL;
GO

-- delete all Groups of GroupType 'TeamGroup'
DECLARE @GroupTypeId [int] = (SELECT [Id] FROM [GroupType] WHERE [Guid] = 'BADD7A6C-1FB3-4E11-A721-6D1377C6958C');

IF (@GroupTypeId IS NOT NULL)
BEGIN
    DELETE FROM [Group]
    WHERE ([GroupTypeId] = @GroupTypeId);
END
