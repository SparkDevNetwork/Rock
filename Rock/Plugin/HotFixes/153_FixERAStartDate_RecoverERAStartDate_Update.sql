-- Recover using History table
-- When a person enters era, the ERA Start Date attribute is set with current date time
-- then a History table gets 'ENTERED' with a CreateDateTime of the ERA Start Date.
-- So we can recover the ERA Start Date by looking for when the most recent time that ENTERED history record was logged.
-- In the unlikely case there wasn't a ENTERED history record, but they have a Era StartDate, we can recover using the AttributeValue.CreatedDateTime.
-- That edge case would be for database that ran a pre-alpha version of v1.5 in 2016.
-- We'll limit this recover to ERA Start Dates that are currently after 5/1/2022 since anything older than that couldn't have been running on v13.4.
DECLARE @attributeEntityTypeId INT = (
        SELECT TOP 1 Id
        FROM EntityType et
        WHERE et.Guid = '5997c8d3-8840-4591-99a5-552919f90cbd'
        )
    , @personEntityTypeId INT = (
        SELECT TOP 1 Id
        FROM EntityType et
        WHERE et.Guid = '72657ED8-D16E-492E-AC12-144C5E7567E7'
        )
    , @IsEraAttributeId INT = (
        SELECT Id
        FROM Attribute
        WHERE Guid = 'CE5739C5-2156-E2AB-48E5-1337C38B935E'
        )
    , @EraStartDateAttributeId INT = (
        SELECT Id
        FROM Attribute
        WHERE Guid = 'A106610C-A7A1-469E-4097-9DE6400FDFC2'
        )

UPDATE av
SET [Value] = convert(VARCHAR(max), convert(DATE, ISNULL(h.MostRecentERAEnteredDateTime, av.CreatedDateTime)), 127)
    , [ValueAsDateTime] = convert(DATE, ISNULL(h.MostRecentERAEnteredDateTime, av.CreatedDateTime))
FROM AttributeValue av
JOIN Person p
    ON p.Id = av.EntityId
CROSS APPLY (
    SELECT MAX(CreatedDateTime) [MostRecentERAEnteredDateTime]
    FROM [History]
    WHERE Verb = 'ENTERED'
        AND RelatedEntityTypeId = @attributeEntityTypeId
        AND EntityTypeId = @personEntityTypeId
        AND EntityId = av.EntityId
        AND RelatedEntityId = @IsEraAttributeId
    ) H
LEFT OUTER JOIN [AttributeValue] av_era
    ON av_era.AttributeId = @IsEraAttributeId
        AND av_era.EntityId = av.EntityId
WHERE av.AttributeId = @EraStartDateAttributeId
    AND av_era.ValueAsBoolean = 1
    AND (
        av.Value <> ''
        AND isnull(try_convert(DATETIMEOFFSET, av.Value), try_convert(DATETIME, av.Value)) IS NOT NULL
        )
    AND convert(DATE, isnull(try_convert(DATETIMEOFFSET, av.Value), try_convert(DATETIME, av.Value))) != convert(DATE, ISNULL(h.MostRecentERAEnteredDateTime, av.CreatedDateTime))
    -- and the current value must be after May 2022, because anything under this date couldn't have been broken by v13.4
    AND av.ValueAsDateTime >= convert(DATE, '2022-05-01 00:00:00.000')
