/* NOTE: If you also want the Welcome Page to wait longer, run Tool_CheckInWelcomeRefreshIntervalTo9999.sql */

DECLARE @IdleSecondsGuidAttended UNIQUEIDENTIFIER = 'A7F99980-BED4-4A80-AB83-DDAB5C7D7AAD'
    ,@IdleSecondsGuidCore UNIQUEIDENTIFIER = '1CAC7B16-041A-4F40-8AEE-A39DFA076C14'

-- Set default idle redirect attribute value to 9999
UPDATE [Attribute]
SET DefaultValue = '9999'
WHERE Guid IN (
        @IdleSecondsGuidAttended
        ,@IdleSecondsGuidCore
        )
AND [DefaultValue] != '9999'

-- Set all idle redirect attribute values to 9999
UPDATE AttributeValue
SET [Value] = '9999'
WHERE AttributeId IN (
        SELECT Id
        FROM [Attribute]
        WHERE [Guid] IN (
                @IdleSecondsGuidAttended
                ,@IdleSecondsGuidCore
                )
        )
        AND [Value] != '9999'        