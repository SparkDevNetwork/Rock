DECLARE globalAttributeCursor CURSOR
FOR
SELECT [Key]
FROM Attribute
WHERE EntityTypeId IS NULL
    AND [Key] NOT LIKE '%.%'

DECLARE @globalAttributeKey NVARCHAR(max)

BEGIN
    OPEN globalAttributeCursor

    FETCH NEXT
    FROM globalAttributeCursor
    INTO @globalAttributeKey

    WHILE @@FETCH_STATUS = 0
    BEGIN
        DECLARE @origValue NVARCHAR(max) = CONCAT (
                'GlobalAttribute.'
                ,@globalAttributeKey
                )
            ,@newValue NVARCHAR(max) = CONCAT (
                '''Global'' | Attribute:'''
                ,@globalAttributeKey
                ,''''
                )
            ,@likeValue NVARCHAR(max) = CONCAT (
                '%GlobalAttribute.'
                ,@globalAttributeKey
                ,'%'
                )

        /* HtmlContent */
        UPDATE HtmlContent
        SET Content = REPLACE(Content, @origValue, @newValue)
        WHERE Content LIKE @likeValue

		/* Attribute */
        UPDATE Attribute
        SET DefaultValue = REPLACE(DefaultValue, @origValue, @newValue)
        WHERE DefaultValue LIKE @likeValue

        /* AttributeValue */
        UPDATE AttributeValue
        SET Value = REPLACE(Value, @origValue, @newValue)
        WHERE Value LIKE @likeValue

        /* CommunicationTemplate Subject */
        UPDATE CommunicationTemplate
        SET [Subject] = REPLACE([Subject], @origValue, @newValue)
        WHERE [Subject] LIKE @likeValue

        /* CommunicationTemplate MediumDataJson OrganizationName*/
        UPDATE CommunicationTemplate
        SET MediumDataJson = REPLACE(MediumDataJson, @origValue, @newValue)
        WHERE MediumDataJson LIKE @likeValue

        /* SystemEmail Body */
        UPDATE SystemEmail
        SET [Body] = REPLACE([Body], @origValue, @newValue)
        WHERE [Body] LIKE @likeValue

		/* SystemEmail Subject */
        UPDATE SystemEmail
        SET [Subject] = REPLACE([Subject], @origValue, @newValue)
        WHERE [Subject] LIKE @likeValue

        /* WorkflowActionForm */
        UPDATE WorkflowActionForm
        SET Header = REPLACE(Header, @origValue, @newValue)
        WHERE Header LIKE @likeValue

        /* WorkflowActionForm */
        UPDATE WorkflowActionForm
        SET Footer = REPLACE(Footer, @origValue, @newValue)
        WHERE Footer LIKE @likeValue

        FETCH NEXT
        FROM globalAttributeCursor
        INTO @globalAttributeKey
    END

    CLOSE globalAttributeCursor

    DEALLOCATE globalAttributeCursor
END
