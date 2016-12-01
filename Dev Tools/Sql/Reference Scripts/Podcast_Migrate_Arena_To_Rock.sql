
--Run to purge test data--
--DELETE FROM [dbo].AttributeValue
--WHERE [dbo].AttributeValue.AttributeId IN
--   (SELECT [dbo].[Attribute].Id
--    FROM [dbo].[Attribute]
--    WHERE [dbo].[Attribute].EntityTypeQualifierColumn = 'ContentChannelTypeId' AND [dbo].[Attribute].EntityTypeQualifierValue = 8)

--DELETE FROM [dbo].ContentChannelItem
--WHERE [dbo].ContentChannelItem.ContentChannelTypeId = 8

--DELETE TOP (130) FROM [dbo].ContentChannel
--WHERE [dbo].ContentChannel.ContentChannelTypeId = 8

-- Setup all the variables for the Rock Table Rows
DECLARE @CC_Title varchar(MAX) = 'Jereds Title Test'
DECLARE @CC_Desc varchar(MAX) = 'Jereds Details Test'
DECLARE @CC_DateRange varchar(MAX) = '2016-05-14T00:00:00.0000000,2016-06-04T00:00:00.0000000'
DECLARE @CC_ExtraDetailsAttrib varchar(MAX) = ''

DECLARE @ContentChannelItemTitle varchar(MAX) = 'Van Halen'
DECLARE @SpeakerAttrib varchar(MAX) = 'Sammy Hagar'
DECLARE @PublishDateAttrib varchar(MAX) = '2016-05-14'
DECLARE @ArenaIDAttrib varchar(MAX) = '0'
DECLARE @HostedAudioUrlAttrib varchar(MAX) = 'http://www.hostedaudiourl.com/audio.mp3'
DECLARE @HostedVideoUrlAttrib varchar(MAX) = 'http://www.hostedvideourl.com/'
DECLARE @ThirdPartyMessageVideoAttrib varchar(MAX) = 'http://www.3rdPartyMessageVideoUrl.com/'
DECLARE @ThirdPartyFullVideoAttrib varchar(MAX) = 'http://www.3rdPartyFULLVideoUrl.com/'
DECLARE @SermonNotePDFUrlAttrib varchar(MAX) = 'http://www.SERMONNOTEURL.com/'
DECLARE @HostedAudioLengthAttrib varchar(MAX) = '33:33'
DECLARE @HostedVideoLengthAttrib varchar(MAX) = '44:44'

DECLARE @PodcastSeriesId INT = 8

--Define Variables needed for copying from Arena into Rock
DECLARE @ArenaTopicId INT

DECLARE @Image_31_20_Binary varbinary(MAX)
DECLARE @Image_31_20_Rock_Guid uniqueidentifier
DECLARE @Image_31_20_FileName varchar(MAX)
DECLARE @Image_31_20_MimeType varchar(MAX)
DECLARE @Image_31_20_BinaryFile_TableId INT

DECLARE @Image_Header_Binary varbinary(MAX)
DECLARE @Image_Header_Rock_Guid uniqueidentifier
DECLARE @Image_Header_FileName varchar(MAX)
DECLARE @Image_Header_MimeType varchar(MAX)
DECLARE @Image_Header_BinaryFile_TableId INT

-- Create the outer "Series" cursor
DECLARE c1 CURSOR
FOR
    -- Select all the relevant columns from Arena that make up a "Series"
    SELECT ft.topic_id, 
           ft.title, 
           ft.[description], 
           ft.date_created, 
           ft.extra_details, 
           ub1.blob, 
           ub1.mime_type, 
           ub1.original_file_name, 
           ub2.blob, 
           ub2.mime_type, 
           ub2.original_file_name
    FROM [CCVSQL1].[Arena].[dbo].[feed_topic] ft
    LEFT OUTER JOIN [CCVSQL1].[Arena].[dbo].[util_blob] ub1 ON ub1.blob_id = ft.image_blob_id
    LEFT OUTER JOIN [CCVSQL1].[Arena].[dbo].[util_blob] ub2 ON ub2.blob_id = ft.image3_blob_id

    OPEN c1

    -- Store the values that will be used for copying into Rock
    FETCH NEXT
    FROM c1
    INTO @ArenaTopicId, 
         @CC_Title, 
         @CC_Desc, 
         @CC_DateRange, 
         @CC_ExtraDetailsAttrib, 
         @Image_31_20_Binary, 
         @Image_31_20_MimeType, 
         @Image_31_20_FileName, 
         @Image_Header_Binary, 
         @Image_Header_MimeType, 
         @Image_Header_FileName

    WHILE @@FETCH_STATUS = 0
    BEGIN
    
    --First, we need to insert the image binaries into Rock's BinaryFile / BinaryFileData tables.

    --Header Image
    SET @Image_Header_Rock_Guid = NULL
    IF @Image_Header_Binary IS NOT NULL
    BEGIN
        SET @Image_Header_Rock_Guid = NEWID()
        INSERT INTO [dbo].[BinaryFile]
        (
            IsTemporary,
            IsSystem,
            BinaryFileTypeId,
            [FileName],
            MimeType,
            StorageEntityTypeId,
            [Guid],
            CreatedDateTime,
            CreatedByPersonAliasId
        )
        VALUES
        (
            0,
            0,
            3,
            CASE WHEN @Image_Header_FileName IS NULL THEN 'temp.jpg' ELSE @Image_Header_FileName END,
            CASE WHEN @Image_Header_MimeType IS NULL THEN 'image/jpeg' ELSE @Image_Header_MimeType END,
            51,
            @Image_Header_Rock_Guid,
            GETDATE(),
            409725
        )
        SET @Image_Header_BinaryFile_TableId = SCOPE_IDENTITY( )

        INSERT INTO [dbo].[BinaryFileData]
        (
            Id,
            Content,
            [Guid],
            CreatedDateTime,
            CreatedByPersonAliasId
        )
        VALUES
        (
            @Image_Header_BinaryFile_TableId,
            @Image_Header_Binary,
            NEWID( ),
            GETDATE( ),
            409725
        )
    END


    --31_20 Image
    SET @Image_31_20_Rock_Guid = NULL
    IF @Image_31_20_Binary IS NOT NULL
    BEGIN
        SET @Image_31_20_Rock_Guid = NEWID()
        INSERT INTO [dbo].[BinaryFile]
        (
            IsTemporary,
            IsSystem,
            BinaryFileTypeId,
            [FileName],
            MimeType,
            StorageEntityTypeId,
            [Guid],
            CreatedDateTime,
            CreatedByPersonAliasId
        )
        VALUES
        (
            0,
            0,
            3,
            CASE WHEN @Image_31_20_FileName IS NULL THEN 'temp.jpg' ELSE @Image_31_20_FileName END,
            CASE WHEN @Image_31_20_MimeType IS NULL THEN 'image/jpeg' ELSE @Image_31_20_MimeType END,
            51,
            @Image_31_20_Rock_Guid,
            GETDATE(),
            409725
        )
        SET @Image_31_20_BinaryFile_TableId = SCOPE_IDENTITY( )

        INSERT INTO [dbo].[BinaryFileData]
        (
            Id,
            Content,
            [Guid],
            CreatedDateTime,
            CreatedByPersonAliasId
        )
        VALUES
        (
            @Image_31_20_BinaryFile_TableId,
            @Image_31_20_Binary,
            NEWID( ),
            GETDATE( ),
            409725
        )
    END


    -- Now Create the Content Channel in Rock that represents the "Series"
    INSERT INTO [dbo].[ContentChannel]
    (
        IconCssClass,
        ContentChannelTypeId,
        Name,
        [Description],
        RequiresApproval,
        EnableRSS,
        TimeToLive,
        CreatedDateTime,
        ModifiedDateTime,
        CreatedByPersonAliasId,
        ModifiedByPersonAliasId,
        [Guid],
        ContentControlType
    )
    VALUES
    (
        'fa fa-play-circle-o',
        @PodcastSeriesId,
        @CC_Title,
        @CC_Desc,
        1,
        0,
        0,
        GETDATE(),
        GETDATE(),
        409725,
        409725,
        NEWID( ),
        0
    )

    -- Now Insert the Attributes
    DECLARE @ContentChannelId INT = SCOPE_IDENTITY( )

    --Image_Header
    INSERT INTO [dbo].AttributeValue
    (
        IsSystem,
        AttributeId,
        EntityId,
        Value,
        [Guid],
        CreatedDateTime,
        ModifiedDateTime,
        CreatedByPersonAliasId,
        ModifiedByPersonAliasId
    )
    VALUES
    (
        0,
        21229,
        @ContentChannelId,
        CASE WHEN @Image_Header_Rock_Guid IS NULL THEN '' ELSE CONVERT(nvarchar(MAX), @Image_Header_Rock_Guid) END,
        NEWID(),
        GETDATE(),
        GETDATE(),
        409725,
        409725
    )

    --Image_31_20
    INSERT INTO [dbo].AttributeValue
    (
        IsSystem,
        AttributeId,
        EntityId,
        Value,
        [Guid],
        CreatedDateTime,
        ModifiedDateTime,
        CreatedByPersonAliasId,
        ModifiedByPersonAliasId
    )
    VALUES
    (
        0,
        19917,
        @ContentChannelId,
        CASE WHEN @Image_31_20_Rock_Guid IS NULL THEN '' ELSE CONVERT(nvarchar(MAX), @Image_31_20_Rock_Guid) END,
        NEWID(),
        GETDATE(),
        GETDATE(),
        409725,
        409725
    )

    --CATEGORY
    INSERT INTO [dbo].AttributeValue
    (
        IsSystem,
        AttributeId,
        EntityId,
        Value,
        [Guid],
        CreatedDateTime,
        ModifiedDateTime,
        CreatedByPersonAliasId,
        ModifiedByPersonAliasId
    )
    VALUES
    (
        0,
        19800,
        @ContentChannelId,
        '1c4de8fc-4f98-4a78-bf44-fcff74124cab',
        NEWID(),
        GETDATE(),
        GETDATE(),
        409725,
        409725
    )

    --Active
    INSERT INTO [dbo].AttributeValue
    (
        IsSystem,
        AttributeId,
        EntityId,
        Value,
        [Guid],
        CreatedDateTime,
        ModifiedDateTime,
        CreatedByPersonAliasId,
        ModifiedByPersonAliasId
    )
    VALUES
    (
        0,
        19804,
        @ContentChannelId,
        'True',
        NEWID(),
        GETDATE(),
        GETDATE(),
        409725,
        409725
    )

    --Series Arena ID
    INSERT INTO [dbo].AttributeValue
    (
        IsSystem,
        AttributeId,
        EntityId,
        Value,
        [Guid],
        CreatedDateTime,
        ModifiedDateTime,
        CreatedByPersonAliasId,
        ModifiedByPersonAliasId
    )
    VALUES
    (
        0,
        23305,
        @ContentChannelId,
        @ArenaTopicId,
        NEWID(),
        GETDATE(),
        GETDATE(),
        409725,
        409725
    )

    --Series Slug
    INSERT INTO [dbo].AttributeValue
    (
        IsSystem,
        AttributeId,
        EntityId,
        Value,
        [Guid],
        CreatedDateTime,
        ModifiedDateTime,
        CreatedByPersonAliasId,
        ModifiedByPersonAliasId
    )
    VALUES
    (
        0,
        23446,
        @ContentChannelId,
        [dbo].ufnUtility_RemoveNonAlphaCharacters( LOWER( REPLACE( @CC_Title, ' ', '-' ) ) ),
        NEWID(),
        GETDATE(),
        GETDATE(),
        409725,
        409725
    )

    --DateRange
    INSERT INTO [dbo].AttributeValue
    (
        IsSystem,
        AttributeId,
        EntityId,
        Value,
        [Guid],
        CreatedDateTime,
        ModifiedDateTime,
        CreatedByPersonAliasId,
        ModifiedByPersonAliasId
    )
    VALUES
    (
        0,
        19803,
        @ContentChannelId,
        @CC_DateRange + ',' + @CC_DateRange,
        NEWID(),
        GETDATE(),
        GETDATE(),
        409725,
        409725
    )

    --EXTRA DETAILS
    INSERT INTO [dbo].AttributeValue
    (
        IsSystem,
        AttributeId,
        EntityId,
        Value,
        [Guid],
        CreatedDateTime,
        ModifiedDateTime,
        CreatedByPersonAliasId,
        ModifiedByPersonAliasId
    )
    VALUES
    (
        0,
        19926,
        @ContentChannelId,
        CASE WHEN @CC_ExtraDetailsAttrib IS NULL THEN '' ELSE @CC_ExtraDetailsAttrib END,
        NEWID(),
        GETDATE(),
        GETDATE(),
        409725,
        409725
    )

    -- These values are intentionally blank because there's no equivalent in Arena
    --Image_16_9
    INSERT INTO [dbo].AttributeValue( IsSystem, AttributeId, EntityId, Value, [Guid], CreatedDateTime, ModifiedDateTime, CreatedByPersonAliasId, ModifiedByPersonAliasId )
    VALUES( 0, 19916, @ContentChannelId, '', NEWID(), GETDATE(), GETDATE(), 409725, 409725 )

    --Image_1_1
    INSERT INTO [dbo].AttributeValue( IsSystem, AttributeId, EntityId, Value, [Guid], CreatedDateTime, ModifiedDateTime, CreatedByPersonAliasId, ModifiedByPersonAliasId )
    VALUES( 0, 19802, @ContentChannelId, '', NEWID(), GETDATE(), GETDATE(), 409725, 409725 )

    --Copy Messages
    DECLARE c2 CURSOR
    FOR 
        -- For each "Message", we need the title, speaker, and then each media item
        SELECT fi.title, fa.name, fi.publish_date, fi.item_id,
  
        --MP4 URL AND DURATION
        (SELECT enclosure_url
        FROM [CCVSQL1].[Arena].[dbo].[feed_item_format] fif
        WHERE fif.item_id = fi.item_id AND fif.format_id = 1) AS mp4,

    
        (SELECT fif.[time]
        FROM [CCVSQL1].[Arena].[dbo].[feed_item_format] fif
        WHERE fif.item_id = fi.item_id AND fif.format_id = 1) AS mp4_time,
        --

        --VIMEO MESSAGE ONLY
        (SELECT enclosure_url
        FROM [CCVSQL1].[Arena].[dbo].[feed_item_format] fif
        WHERE fif.item_id = fi.item_id AND fif.format_id = 2) AS vimeo_message,
        --

        --MP3 URL AND DURATION
        (SELECT enclosure_url
        FROM [CCVSQL1].[Arena].[dbo].[feed_item_format] fif
        WHERE fif.item_id = fi.item_id AND fif.format_id = 3) AS mp3,

    
        (SELECT fif.[time]
        FROM [CCVSQL1].[Arena].[dbo].[feed_item_format] fif
        WHERE fif.item_id = fi.item_id AND fif.format_id = 3) AS mp3_time,
        --

        --PDF
        (SELECT enclosure_url
        FROM [CCVSQL1].[Arena].[dbo].[feed_item_format] fif
        WHERE fif.item_id = fi.item_id AND fif.format_id = 4) AS pdf,
        --

        --VIMEO FULL 
        (SELECT enclosure_url
        FROM [CCVSQL1].[Arena].[dbo].[feed_item_format] fif
        WHERE fif.item_id = fi.item_id AND fif.format_id = 5) AS vimeo_full
        --

        FROM [CCVSQL1].[Arena].[dbo].[feed_topic] ft
        INNER JOIN [CCVSQL1].[Arena].[dbo].[feed_item] fi ON ft.topic_id = fi.topic_id
        LEFT OUTER JOIN [CCVSQL1].[Arena].[dbo].[feed_author] fa ON fi.author_id = fa.author_id

        WHERE ft.topic_id = @ArenaTopicId
        ORDER BY (ft.date_modified)

        OPEN c2

        FETCH NEXT
        FROM c2

        INTO  @ContentChannelItemTitle, 
              @SpeakerAttrib, 
              @PublishDateAttrib,
              @ArenaIDAttrib,
              @HostedVideoUrlAttrib, 
              @HostedVideoLengthAttrib, 
              @ThirdPartyMessageVideoAttrib, 
              @HostedAudioUrlAttrib, 
              @HostedAudioLengthAttrib,
              @SermonNotePDFUrlAttrib,
              @ThirdPartyFullVideoAttrib

        WHILE @@FETCH_STATUS = 0
        BEGIN

        INSERT INTO [dbo].[ContentChannelItem] 
        (
            ContentChannelId, 
            ContentChannelTypeId, 
            Title, 
            Content,
            [Status], 
            [Priority],
            ApprovedByPersonAliasId, 
            ApprovedDateTime, 
            StartDateTime, 
            CreatedDateTime,
            ModifiedDateTime,
            CreatedByPersonAliasId,
            ModifiedByPersonAliasId,
            [Guid]
        )
        VALUES 
        (
            @ContentChannelId, --Truish
            @PodcastSeriesId, --Podcast Series
            @ContentChannelItemTitle, --Title
            '', --Content (Description)
            2, --Status Approved
            0, --Priority
            409725,
            @PublishDateAttrib, --ApprovedDateTime
            @PublishDateAttrib, --StartDateTime
            @PublishDateAttrib, --CreatedDateTime
            @PublishDateAttrib, --ModifiedDateTime
            409725, --CreatedByPersonAliasId
            409725, --ModifiedByPersonAliasId,
            NEWID() --Guid
        )

        -- CONTENT CHANNEL ITEM ATTRIBUTES
        DECLARE @ContentChannelItemId INT = SCOPE_IDENTITY( )
  
        --Speaker
        INSERT INTO [dbo].AttributeValue
        (
            IsSystem,
            AttributeId,
            EntityId,
            Value,
            [Guid],
            CreatedDateTime,
            ModifiedDateTime,
            CreatedByPersonAliasId,
            ModifiedByPersonAliasId
        )
        VALUES
        (
            0,
            19805,
            @ContentChannelItemId,
            CASE WHEN @SpeakerAttrib IS NULL THEN '' ELSE @SpeakerAttrib END,
            NEWID(),
            GETDATE(),
            GETDATE(),
            409725,
            409725
        )

        --MessageArenaID
        INSERT INTO [dbo].AttributeValue
        (
            IsSystem,
            AttributeId,
            EntityId,
            Value,
            [Guid],
            CreatedDateTime,
            ModifiedDateTime,
            CreatedByPersonAliasId,
            ModifiedByPersonAliasId
        )
        VALUES
        (
            0,
            23306,
            @ContentChannelItemId,
            CASE WHEN @ArenaIDAttrib IS NULL THEN '' ELSE @ArenaIDAttrib END,
            NEWID(),
            GETDATE(),
            GETDATE(),
            409725,
            409725
        )

        --HostedAudioURL
        INSERT INTO [dbo].AttributeValue
        (
            IsSystem,
            AttributeId,
            EntityId,
            Value,
            [Guid],
            CreatedDateTime,
            ModifiedDateTime,
            CreatedByPersonAliasId,
            ModifiedByPersonAliasId
        )
        VALUES
        (
            0,
            19806,
            @ContentChannelItemId,
            CASE WHEN @HostedAudioUrlAttrib IS NULL THEN '' ELSE @HostedAudioUrlAttrib END,
            NEWID(),
            GETDATE(),
            GETDATE(),
            409725,
            409725
        )
  
        --HostedVideoUrl
        INSERT INTO [dbo].AttributeValue
        (
            IsSystem,
            AttributeId,
            EntityId,
            Value,
            [Guid],
            CreatedDateTime,
            ModifiedDateTime,
            CreatedByPersonAliasId,
            ModifiedByPersonAliasId
        )
        VALUES
        (
            0,
            19921,
            @ContentChannelItemId,
            CASE WHEN @HostedVideoUrlAttrib IS NULL THEN '' ELSE @HostedVideoUrlAttrib END,
            NEWID(),
            GETDATE(),
            GETDATE(),
            409725,
            409725
        )

        --3rdPartyMessageVideoUrl
        INSERT INTO [dbo].AttributeValue
        (
            IsSystem,
            AttributeId,
            EntityId,
            Value,
            [Guid],
            CreatedDateTime,
            ModifiedDateTime,
            CreatedByPersonAliasId,
            ModifiedByPersonAliasId
        )
        VALUES
        (
            0,
            19924,
            @ContentChannelItemId,
            CASE WHEN @ThirdPartyMessageVideoAttrib IS NULL THEN '' ELSE @ThirdPartyMessageVideoAttrib END,
            NEWID(),
            GETDATE(),
            GETDATE(),
            409725,
            409725
        )

        --3rdPartyFullVideoUrl
        INSERT INTO [dbo].AttributeValue
        (
            IsSystem,
            AttributeId,
            EntityId,
            Value,
            [Guid],
            CreatedDateTime,
            ModifiedDateTime,
            CreatedByPersonAliasId,
            ModifiedByPersonAliasId
        )
        VALUES
        (
            0,
            19923,
            @ContentChannelItemId,
            CASE WHEN @ThirdPartyFullVideoAttrib IS NULL THEN '' ELSE @ThirdPartyFullVideoAttrib END,
            NEWID(),
            GETDATE(),
            GETDATE(),
            409725,
            409725
        )

        --SermonNotePDFUrl
        INSERT INTO [dbo].AttributeValue
        (
            IsSystem,
            AttributeId,
            EntityId,
            Value,
            [Guid],
            CreatedDateTime,
            ModifiedDateTime,
            CreatedByPersonAliasId,
            ModifiedByPersonAliasId
        )
        VALUES
        (
            0,
            19925,
            @ContentChannelItemId,
            CASE WHEN @SermonNotePDFUrlAttrib IS NULL THEN '' ELSE @SermonNotePDFUrlAttrib END,
            NEWID(),
            GETDATE(),
            GETDATE(),
            409725,
            409725
        )

        --HostedAudioLength
        INSERT INTO [dbo].AttributeValue
        (
            IsSystem,
            AttributeId,
            EntityId,
            Value,
            [Guid],
            CreatedDateTime,
            ModifiedDateTime,
            CreatedByPersonAliasId,
            ModifiedByPersonAliasId
        )
        VALUES
        (
            0,
            20202,
            @ContentChannelItemId,
            CASE WHEN @HostedAudioLengthAttrib IS NULL THEN '' ELSE @HostedAudioLengthAttrib END,
            NEWID(),
            GETDATE(),
            GETDATE(),
            409725,
            409725
        )

        --HostedVideoLength
        INSERT INTO [dbo].AttributeValue
        (
            IsSystem,
            AttributeId,
            EntityId,
            Value,
            [Guid],
            CreatedDateTime,
            ModifiedDateTime,
            CreatedByPersonAliasId,
            ModifiedByPersonAliasId
        )
        VALUES
        (
            0,
            20205,
            @ContentChannelItemId,
            CASE WHEN @HostedVideoLengthAttrib IS NULL THEN '' ELSE @HostedVideoLengthAttrib END,
            NEWID(),
            GETDATE(),
            GETDATE(),
            409725,
            409725
        )

        --Message Slug
        INSERT INTO [dbo].AttributeValue
        (
            IsSystem,
            AttributeId,
            EntityId,
            Value,
            [Guid],
            CreatedDateTime,
            ModifiedDateTime,
            CreatedByPersonAliasId,
            ModifiedByPersonAliasId
        )
        VALUES
        (
            0,
            23447,
            @ContentChannelItemId,
            [dbo].ufnUtility_RemoveNonAlphaCharacters( LOWER( REPLACE( @ContentChannelItemTitle, ' ', '-' ) ) ),
            NEWID(),
            GETDATE(),
            GETDATE(),
            409725,
            409725
        )

        -- The following values need to exist, but don't have values in Arena, so create them as blank.
        --WatchURL
        INSERT INTO [dbo].AttributeValue( IsSystem, AttributeId, EntityId, Value, [Guid], CreatedDateTime, ModifiedDateTime, CreatedByPersonAliasId, ModifiedByPersonAliasId )
        VALUES( 0, 19807, @ContentChannelItemId, '', NEWID(), GETDATE(), GETDATE(), 409725, 409725 )

        --ShareURL
        INSERT INTO [dbo].AttributeValue( IsSystem, AttributeId, EntityId, Value, [Guid], CreatedDateTime, ModifiedDateTime, CreatedByPersonAliasId, ModifiedByPersonAliasId )
        VALUES( 0, 19808, @ContentChannelItemId, '', NEWID(), GETDATE(), GETDATE(), 409725, 409725 )

        --NoteURL
        INSERT INTO [dbo].AttributeValue( IsSystem, AttributeId, EntityId, Value, [Guid], CreatedDateTime, ModifiedDateTime, CreatedByPersonAliasId, ModifiedByPersonAliasId )
        VALUES( 0, 19809, @ContentChannelItemId, '', NEWID(), GETDATE(), GETDATE(), 409725, 409725 )

        --Extra Details
        INSERT INTO [dbo].AttributeValue( IsSystem, AttributeId, EntityId, Value, [Guid], CreatedDateTime, ModifiedDateTime, CreatedByPersonAliasId, ModifiedByPersonAliasId )
        VALUES( 0, 21266, @ContentChannelItemId, '', NEWID(), GETDATE(), GETDATE(), 409725, 409725 )
        
        FETCH NEXT
        FROM c2
        INTO @ContentChannelItemTitle, 
             @SpeakerAttrib, 
             @PublishDateAttrib,
             @ArenaIDAttrib,
             @HostedVideoUrlAttrib, 
             @HostedVideoLengthAttrib, 
             @ThirdPartyMessageVideoAttrib, 
             @HostedAudioUrlAttrib, 
             @HostedAudioLengthAttrib,
             @SermonNotePDFUrlAttrib,
             @ThirdPartyFullVideoAttrib

        END
    CLOSE c2
    DEALLOCATE c2
    --End Copy Messages


    FETCH NEXT
    FROM c1
    INTO @ArenaTopicId, 
         @CC_Title, 
         @CC_Desc, 
         @CC_DateRange, 
         @CC_ExtraDetailsAttrib, 
         @Image_31_20_Binary, 
         @Image_31_20_MimeType, 
         @Image_31_20_FileName, 
         @Image_Header_Binary, 
         @Image_Header_MimeType, 
         @Image_Header_FileName

END
CLOSE c1
DEALLOCATE c1
