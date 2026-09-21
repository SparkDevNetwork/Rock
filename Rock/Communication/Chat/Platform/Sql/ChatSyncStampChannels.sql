-- Marks every group that is a chat channel right now and has never been marked before.

SET NOCOUNT ON;

UPDATE [G]
SET [G].[ChatChannelFirstEnabledDateTime] = @StampedAt
FROM [Group] AS [G]
INNER JOIN [GroupType] AS [GT] ON [GT].[Id] = [G].[GroupTypeId]
WHERE [G].[ChatChannelFirstEnabledDateTime] IS NULL
    AND [GT].[IsChatAllowed] = 1;
