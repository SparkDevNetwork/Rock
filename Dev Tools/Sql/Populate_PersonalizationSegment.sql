DECLARE @SampleAdditionalFilterJsonA NVARCHAR(max) = 
    '{
    "SessionFilterExpressionType": 1,
    "SessionSegmentFilters": [
        {
            "ComparisonType": 256,
            "ComparisonValue": 4,
            "SiteGuids": [
                "c2d29296-6a87-47a9-a753-ee4e9159c4c4",
                "f3f82256-2d66-432b-9d67-3552cd2f4c2b",
                "a5fa7c3c-a238-4e0b-95de-b540144321ec"
            ],
            "SlidingDateRangeDelimitedValues": "Last|15|Year||",
            "Guid": "f26da258-734e-4d6c-b9a4-33e5405c4f01"
        }
    ],
    "PageViewFilterExpressionType": 1,
    "PageViewSegmentFilters": [
        {
            "ComparisonType": 256,
            "ComparisonValue": 4,
            "SiteGuids": [
                "f3f82256-2d66-432b-9d67-3552cd2f4c2b"
            ],
            "PageGuids": [],
            "SlidingDateRangeDelimitedValues": "Last|19|Month||",
            "Guid": "65c68220-997d-47a6-95c2-978360ad7002"
        }
    ],
    "InteractionFilterExpressionType": 1,
    "InteractionSegmentFilters": [
        {
            "ComparisonType": 1024,
            "ComparisonValue": 4,
            "InteractionChannelGuid": "daa17190-7119-4901-b105-26c6b5e4cdb4",
            "InteractionComponentGuid": "131eb836-3ed1-46b7-9fb7-c15d7869efda",
            "Operation": "Form Viewed",
            "SlidingDateRangeDelimitedValues": "All||||",
            "Guid": "130c6b2b-6624-48b8-9fab-c5d154647a26"
        }
    ]
}'
    , @SampleAdditionalFilterJsonNoCriteria NVARCHAR(max) = '{
    "SessionFilterExpressionType": 1,
    "SessionSegmentFilters": [],
    "PageViewFilterExpressionType": 1,
    "PageViewSegmentFilters": [],
    "InteractionFilterExpressionType": 1,
    "InteractionSegmentFilters": []
}'
    , @SampleFilter3 NVARCHAR(max) = '{
    "SessionFilterExpressionType": 1,
    "SessionSegmentFilters": [],
    "PageViewFilterExpressionType": 1,
    "PageViewSegmentFilters": [],
    "InteractionFilterExpressionType": 1,
    "InteractionSegmentFilters": [
        {
            "ComparisonType": 256,
            "ComparisonValue": 1,
            "InteractionChannelGuid": "daa17190-7119-4901-b105-26c6b5e4cdb4",
            "InteractionComponentGuid": "131eb836-3ed1-46b7-9fb7-c15d7869efda",
            "Operation": "",
            "SlidingDateRangeDelimitedValues": "All||||",
            "Guid": "b7dff9b2-a7c6-43e5-8f0b-fed2806a0918"
        }
    ]
}'
    , @Sample1Guid UNIQUEIDENTIFIER = '6b5f4ebf-2c04-49e3-8e4e-5a41a777596d'
    , @Sample2Guid UNIQUEIDENTIFIER = '069e7e6e-56c6-4780-80ad-7b22f86bf8b2'
    , @Sample3Guid UNIQUEIDENTIFIER = 'da29988f-23cf-43ce-995b-1d51dae39982'

IF NOT EXISTS (
        SELECT 1
        FROM PersonalizationSegment
        WHERE [Guid] = @Sample1Guid
        )
BEGIN
    INSERT INTO [PersonalizationSegment] (
        [Name]
        , [SegmentKey]
        , [IsActive]
        , [FilterDataViewId]
        , [AdditionalFilterJson]
        , [Guid]
        )
    VALUES (
        'Sample 1'
        , 'Sample1'
        , 1
        , NULL
        , @SampleAdditionalFilterJsonA
        , @Sample1Guid
        )
END

IF NOT EXISTS (
        SELECT 1
        FROM PersonalizationSegment
        WHERE [Guid] = @Sample2Guid
        )
BEGIN
    INSERT INTO [PersonalizationSegment] (
        [Name]
        , [SegmentKey]
        , [IsActive]
        , [FilterDataViewId]
        , [AdditionalFilterJson]
        , [Guid]
        )
    VALUES (
        'Sample 2'
        , 'Sample2'
        , 1
        , NULL
        , @SampleAdditionalFilterJsonNoCriteria
        , @Sample2Guid
        )
END

IF NOT EXISTS (
        SELECT 1
        FROM PersonalizationSegment
        WHERE [Guid] = @Sample3Guid
        )
BEGIN
    INSERT INTO [PersonalizationSegment] (
        [Name]
        , [SegmentKey]
        , [IsActive]
        , [FilterDataViewId]
        , [AdditionalFilterJson]
        , [Guid]
        )
    VALUES (
        'Sample 3'
        , 'Sample3'
        , 1
        , NULL
        , @SampleFilter3
        , @Sample3Guid
        )
END
