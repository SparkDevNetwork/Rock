-- Marks every group that is a chat channel right now and carries no mark yet.
--
-- The mark is what makes turning chat off on a group archive its conversation instead of losing
-- it: the projection keeps sending a marked group whatever its own settings later say, so the
-- platform keeps the channel and its history rather than reading the group as gone. It is written
-- once and never moved, so a group that had chat, lost it and got it back keeps the date it first
-- had it.
--
-- This runs on its own, before the projection, and commits whether or not the submission that
-- follows works. That is deliberate: a mark rolled back with a failed submission would leave the
-- next run projecting a group as though it had never been a channel, which is the one state this
-- column exists to prevent.
--
-- The condition below is the half of the channel rule that says a group qualifies right now. The
-- staging query holds the same words with the marked groups added to them, and a test fails if the
-- two ever stop agreeing.

SET NOCOUNT ON;

UPDATE [G]
SET [G].[ChatChannelFirstEnabledDateTime] = @StampedAt
FROM [Group] AS [G]
INNER JOIN [GroupType] AS [GT] ON [GT].[Id] = [G].[GroupTypeId]
WHERE [G].[ChatChannelFirstEnabledDateTime] IS NULL
    AND [GT].[IsChatAllowed] = 1
    AND COALESCE( [G].[IsChatEnabledOverride], [GT].[IsChatEnabledForAllGroups] ) = 1;
