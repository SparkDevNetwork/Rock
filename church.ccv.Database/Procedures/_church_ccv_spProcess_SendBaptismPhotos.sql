/*
<doc>
	<summary>
 		This stored procedure sends baptism photo emails to people 
		who have a baptism date, baptism photo and have not already
		received this email.
	</summary>

	<returns></returns>
	<remarks>	
	</remarks>
	<code>
		EXEC [dbo].[_church_ccv_spProcess_SendBaptismPhotos]
	</code>
</doc>
*/
ALTER PROCEDURE [dbo].[_church_ccv_spProcess_SendBaptismPhotos]
AS
BEGIN
    SET NOCOUNT ON;

    -- SETTINGS -------------------------------------------
    DECLARE @EmailFromName VARCHAR(100) = 'Christ''s Church of the Valley'
    DECLARE @EmailFromAddress VARCHAR(100) = 'info@ccv.church'
    DECLARE @EmailSentAttributeId INT = 2519
    -- VARIABLES ------------------------------------------
    DECLARE @CommunicationID INT
    DECLARE @EmailMessage VARCHAR(MAX) = ''
    DECLARE @EmailMergeValues VARCHAR(MAX) = ''
    DECLARE @EmailJson VARCHAR(MAX) = ''
    DECLARE @PersonAliasID INT = 0
	DECLARE @PersonID INT = 0
    DECLARE @PersonFirstName VARCHAR(100) = ''
    DECLARE @PersonLastName VARCHAR(100) = ''
    DECLARE @PersonEmail VARCHAR(100) = ''
    DECLARE @PersonGuid VARCHAR(100) = ''
    DECLARE @PersonBaptismPhotoGuid VARCHAR(100) = ''

    DECLARE baptismphoto_cursor CURSOR FOR

    WITH CTE AS
	(

	SELECT dbo.ufnUtility_GetPrimaryPersonAliasId(P.Id) AS [PersonAliasId]
		,P.Id
        ,P.FirstName
        ,P.LastName
        ,P.Email
        ,P.[Guid]
        ,BP.Value
		,DP.FamilyId
	FROM Person P 
	INNER JOIN _church_ccv_Datamart_Person DP ON DP.PersonId = P.Id
    INNER JOIN AttributeValue BP ON BP.EntityId = P.Id
        AND BP.AttributeId = 2627 -- Baptism Photo
    INNER JOIN AttributeValue BD ON BD.EntityId = P.Id
        AND BD.AttributeId = 174 -- Baptism Date
    LEFT OUTER JOIN AttributeValue BS ON BS.EntityId = P.Id
        AND BS.AttributeId = 2519 -- Sent Baptism Photo Email
	WHERE (
            BP.Value IS NOT NULL
            AND BP.Value != ''
            )
        AND (
            P.Email IS NOT NULL
            AND P.Email != ''
            )
        AND BS.Value != 'True'
        AND BD.ValueAsDateTime < GETDATE()
        AND BD.ValueAsDateTime != '1900-01-01 00:00:00.000'
		AND DP.FamilyRole = 'Child'

	)

	SELECT C.PersonAliasId
			,C.Id
			,C.FirstName
			,C.LastName
			,DP.Email
			,C.[Guid]
			,C.Value
	FROM _church_ccv_Datamart_Person DP
	INNER JOIN CTE C ON C.FamilyId = DP.FamilyId
	WHERE DP.FamilyRole = 'Adult'

	UNION

	SELECT dbo.ufnUtility_GetPrimaryPersonAliasId(P.Id)  AS [PersonAliasId]
		,P.Id
		,P.FirstName
		,P.LastName
		,P.Email
		,P.[Guid]
		,BP.Value
	FROM Person P 
	INNER JOIN _church_ccv_Datamart_Person DP ON DP.PersonId = P.Id
	INNER JOIN AttributeValue BP ON BP.EntityId = P.Id
		AND BP.AttributeId = 2627 -- Baptism Photo
	INNER JOIN AttributeValue BD ON BD.EntityId = P.Id
		AND BD.AttributeId = 174 -- Baptism Date
	LEFT OUTER JOIN AttributeValue BS ON BS.EntityId = P.Id
		AND BS.AttributeId = 2519 -- Sent Baptism Photo Email
	WHERE (
				BP.Value IS NOT NULL
				AND BP.Value != ''
				)
			AND (
				P.Email IS NOT NULL
				AND P.Email != ''
				)
			AND BS.Value != 'True'
			AND BD.ValueAsDateTime < GETDATE()
			AND BD.ValueAsDateTime != '1900-01-01 00:00:00.000'
			AND DP.FamilyRole = 'Adult'

    OPEN baptismphoto_cursor

    FETCH NEXT
    FROM baptismphoto_cursor
    INTO @PersonAliasID
		,@PersonId
        ,@PersonFirstName
        ,@PersonLastName
        ,@PersonEmail
        ,@PersonGuid
        ,@PersonBaptismPhotoGuid

    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Grab email template
        SET @EmailMessage = (
                SELECT Body
                FROM SystemEmail
                WHERE Id = 16
                )
        -- Escape double quotes to prevent errors
        SET @EmailMessage = REPLACE(@EmailMessage, '"', '\"')
        -- Generate merge fields
        SET @EmailMergeValues = '{
					"NickName": "' + ISNULL(@PersonFirstName, '') + '",
					"BaptismPhotoGuid": "' + ISNULL(@PersonBaptismPhotoGuid, '') + '",
					"PersonGuid": "' + ISNULL(@PersonGuid, '') + '"
				}'
        -- Generate JSON
        SET @EmailJson = '{
					"HtmlMessage": "' + ISNULL(@EmailMessage, '') + '",
					"FromName": "' + ISNULL(@EmailFromName, '') + '",
					"FromAddress": "' + ISNULL(@EmailFromAddress, '') + '",
					"ReplyTo": "' + ISNULL(@EmailFromAddress, '') + '",
					"DefaultPlainText": ""
				}'

        -- Insert email message into database
        INSERT INTO Communication (
            [CreatedDateTime]
            ,[CreatedByPersonAliasId]
            ,[MediumEntityTypeId]
            ,[Guid]
            ,[IsBulkCommunication]
            ,[MediumDataJson]
            ,[Status]
            ,[Subject]
            ,[SenderPersonAliasId]
            ,[ReviewedDateTime]
            )
        VALUES (
            DATEADD(Minute, - 31, GETDATE())
            ,1
            ,37
            ,NEWID()
            ,0
            ,@EmailJson
            ,3 --Approved
            ,ISNULL(@PersonFirstName, '') + ' ' + ISNULL(@PersonLastName, '') + ' Baptism Photo'
            ,1
            ,DATEADD(Minute, - 31, GETDATE())
            )

        -- Grab the identity number from the last insert
        SET @CommunicationID = SCOPE_IDENTITY()

        INSERT INTO CommunicationRecipient (
            [CommunicationId]
            ,[Status]
            ,[Guid]
            ,[CreatedDateTime]
            ,[ModifiedDateTime]
            ,[PersonAliasId]
            ,[AdditionalMergeValuesJson]
            )
        VALUES (
            @CommunicationID
            ,0 --Pending
            ,NEWID()
            ,DATEADD(Minute, - 31, GETDATE())
            ,DATEADD(Minute, - 31, GETDATE())
            ,@PersonAliasID
            ,@EmailMergeValues
            )

        --Mark the email as sent
        IF EXISTS (
                SELECT *
                FROM AttributeValue
                WHERE AttributeId = @EmailSentAttributeId
                    AND EntityId = @PersonID
                )
        BEGIN
            UPDATE AttributeValue
            SET Value = 'True'
                ,CreatedDateTime = GETDATE()
            WHERE AttributeId = @EmailSentAttributeId
                AND EntityId = @PersonID
        END
        ELSE
        BEGIN
            INSERT INTO AttributeValue (
                [IsSystem]
                ,[AttributeId]
                ,[EntityId]
                ,[Value]
                ,[Guid]
                ,[CreatedDateTime]
                ,[ModifiedDateTime]
                )
            VALUES (
                0
                ,@EmailSentAttributeId
                ,@PersonID
                ,'True'
                ,NEWID()
                ,GETDATE()
                ,GETDATE()
                )
        END

        FETCH NEXT
        FROM baptismphoto_cursor
        INTO @PersonAliasID
			,@PersonId
            ,@PersonFirstName
            ,@PersonLastName
            ,@PersonEmail
            ,@PersonGuid
            ,@PersonBaptismPhotoGuid
    END

    CLOSE baptismphoto_cursor

    DEALLOCATE baptismphoto_cursor
END