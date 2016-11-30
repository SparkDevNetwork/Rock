/*
<doc>
	<summary>
 		This stored procedure sends follow up emails to people who have been baptized for 6 weeks.
	</summary>

	<returns></returns>
	<remarks>	
	</remarks>
	<code>
		EXEC [dbo].[_church_ccv_spProcess_SendPostBaptismEmail]
	</code>
</doc>
*/

ALTER PROCEDURE [dbo].[_church_ccv_spProcess_SendPostBaptismEmail]
	
AS
BEGIN

	SET NOCOUNT ON;

	-- SETTINGS -------------------------------------------
	DECLARE @DefaultNHPastorAlias INT = 398844 -- Kris Simpson
	DECLARE @EmailSentAttributeId INT = 2517

	-- VARIABLES -------------------------------------------
	DECLARE @CommunicationID INT
	DECLARE @HTMLMessage VARCHAR(MAX) = ''
	DECLARE @HTMLNextSteps VARCHAR(MAX) = ''
	DECLARE @EmailJson VARCHAR(MAX) = ''
	DECLARE @EmailMergeFields VARCHAR(MAX) = ''
	DECLARE @EmailMergeValues VARCHAR(MAX) = ''

	DECLARE @PersonID INT
	DECLARE @NickName VARCHAR(25) = ''
	DECLARE @LastName VARCHAR(25) = ''
	DECLARE @Email VARCHAR(80) = ''
	DECLARE @InStartingPoint VARCHAR(5) = ''
	DECLARE @InNHGroup VARCHAR(5) = ''
	DECLARE @IsServing VARCHAR(5) = ''
	DECLARE @NHPastorID INT
	DECLARE @NHPastorName VARCHAR(50) = ''
	DECLARE @NHPastorEmail VARCHAR(80) = ''
	DECLARE @NHPastorPhone VARCHAR(50) = ''
	DECLARE @DefaultNHPastorID INT
	DECLARE @DefaultNHPastorName VARCHAR(50) = ''
	DECLARE @DefaultNHPastorEmail VARCHAR(80) = ''
	DECLARE @DefaultNHPastorPhone VARCHAR(50) = ''



	-- Get default NH Pastor information
	SELECT @DefaultNHPastorID = P.Id,
		@DefaultNHPastorName = P.FirstName + ' ' + P.LastName,
		@DefaultNHPastorEmail = P.Email,
		@DefaultNHPastorPhone = PN.NumberFormatted
	FROM Person P
	INNER JOIN PhoneNumber PN ON PN.PersonId = P.Id AND PN.NumberTypeValueId = 613
	WHERE P.Id = dbo.ufnUtility_GetPersonIdFromPersonAlias(@DefaultNHPastorAlias)


	DECLARE c1 CURSOR FOR
		SELECT DISTINCT P.Id, 
			P.NickName, 
			P.LastName, 
			P.Email,
			DP.TakenStartingPoint,
			DP.InNeighborhoodGroup,
			DP.IsServing,
			CASE WHEN dbo._church_ccv_ufnGetNHPastorName(P.Id) = ''
			THEN @DefaultNHPastorName ELSE dbo._church_ccv_ufnGetNHPastorName(P.Id) END,
			CASE WHEN dbo._church_ccv_ufnGetNHPastorEmail(P.Id) = ''
			THEN 'info@ccv.church' ELSE dbo._church_ccv_ufnGetNHPastorEmail(P.Id) END,
			CASE WHEN dbo._church_ccv_ufnGetNHPastorPhone(P.Id) = ''
			THEN @DefaultNHPastorPhone ELSE dbo._church_ccv_ufnGetNHPastorPhone(P.Id) END
		FROM Person P
		INNER JOIN _church_ccv_Datamart_Person DP ON DP.PersonId = P.Id
		INNER JOIN [GroupMember] FM ON FM.PersonId = P.ID AND FM.GroupRoleId = 3
		INNER JOIN [Group] F ON F.Id = FM.GroupId AND F.GroupTypeId = 10 
		INNER JOIN [AttributeValue] BD ON BD.EntityId = P.Id AND BD.AttributeId = 174 -- baptism date
		INNER JOIN [AttributeValue] BBC ON BBC.EntityId = P.Id AND BBC.AttributeId = 714 -- baptized by ccv
		LEFT OUTER JOIN [AttributeValue] BS ON BS.EntityId = P.Id AND BS.AttributeId = 2517 -- baptism photo
		WHERE P.Email != ''
			AND BD.ValueAsDateTime <= DATEADD(WEEK, -6, dbo.ufnUtility_GetPreviousSundayDate())
			AND BD.ValueAsDateTime >= DATEADD(WEEK, 12, dbo.ufnUtility_GetPreviousSundayDate())
			AND BD.ValueAsDateTime != '1900-01-01 00:00:00.000'
			AND (BS.Value IS NULL OR BS.Value != 'True')
			AND BBC.Value = 'True'

		OPEN c1
		FETCH NEXT FROM c1 INTO @PersonID, @NickName, @LastName, @Email, @InStartingPoint, 
								@InNHGroup, @IsServing, @NHPastorName, @NHPastorEmail, @NHPastorPhone
	
		WHILE @@FETCH_STATUS = 0
		BEGIN
			
			--Generate personal Next Steps
			SET @HTMLNextSteps = ''
			IF (@InStartingPoint = 0)
			BEGIN
				SET @HTMLNextSteps += '<table style=\"color: #505050; font-family: helvetica; font-size: 14px; line-height: 150%; border-top: 1px solid #ddd; padding-top: 10px;\">
											<tr>
												<td style=\"padding-top: 5px; padding-right: 5px; padding-bottom: 2px; padding-left: 0;\">
													<img style=\"float: left; margin: 0 5px 5px 0\" width=100 src=\"https://s3.amazonaws.com/media.ccvonline.com/images/emails/nextsteps/post-baptism/starting-point.png\">
												</td>
												<td style=\"padding-top: 10px; padding-bottom: 10px;\">
													<b>Starting Point</b>
													<br>Sign up for the next Starting Point class and take a deep look at the Christian faith.  Find out what CCV is all about and learn how to get involved.  Sign up <a href=\"http://ccv.church/startingpoint\">here.</a>
												</td>
											</tr>
										</table>'
			END

			IF (@InStartingPoint = 1 AND @InNHGroup = 0)
			BEGIN
				SET @HTMLNextSteps += '<table style=\"color: #505050; font-family: helvetica; font-size: 14px; line-height: 150%; border-top: 1px solid #ddd; padding-top: 10px;\">
											<tr>
												<td style=\"padding-top: 5px; padding-right: 5px; padding-bottom: 2px; padding-left: 0;\">
													<img style=\"float: left; margin: 0 5px 5px 0\" width=100 src=\"https://s3.amazonaws.com/media.ccvonline.com/images/emails/nextsteps/post-baptism/neighborhoods.png\">
												</td>
												<td style=\"padding-top: 10px; padding-bottom: 10px;\">
													<b>Neighborhood Group</b>
													<br>Don''t live life alone. Gather weekly with people from the community and make an impact. Find your neighborhood group <a href=\"http://ccv.church/neighborhoods\">here.</a>
												</td>
											</tr>
										</table>'
			END

			IF (@InStartingPoint = 1 AND @InNHGroup = 1 AND @IsServing = 0)
			BEGIN
				SET @HTMLNextSteps += '<table style=\"color: #505050; font-family: helvetica; font-size: 14px; line-height: 150%; border-top: 1px solid #ddd; padding-top: 10px;\">
											<tr>
												<td style=\"padding-top: 5px; padding-right: 5px; padding-bottom: 2px; padding-left: 0;\">
													<img style=\"float: left; margin: 0 5px 5px 0\" width=100 src=\"https://s3.amazonaws.com/media.ccvonline.com/images/emails/nextsteps/post-baptism/serve.png\">
												</td>
												<td style=\"padding-top: 10px; padding-bottom: 10px;\">
													<b>Serving</b>
													<br>Join the team and get involved at CCV through serving.  We have opportunities for every age and interest.  Check them out <a href=\"http://ccv.church/serve\">here.</a>
												</td>
											</tr>
										</table>'
			END

			IF (@InStartingPoint = 1 AND @InNHGroup = 1 AND @IsServing = 1)
			BEGIN
				SET @HTMLNextSteps += '<table style=\"color: #505050; font-family: helvetica; font-size: 14px; line-height: 150%; border-top: 1px solid #ddd; padding-top: 10px;\">
											<tr>
												<td style=\"padding-top: 5px; padding-right: 5px; padding-bottom: 2px; padding-left: 0;\">
													<img style=\"float: left; margin: 0 10px 5px 5px\" width=90 src=\"https://s3.amazonaws.com/media.ccvonline.com/images/emails/nextsteps/post-baptism/foundations.png\">
												</td>
												<td style=\"padding-top: 10px; padding-bottom: 10px;\">
													<b>Foundations</b>
													<br>This online, video-based course on the basics of faith and ministry is a great way to start your journey.  Check out the classes <a href=\"http://ccv.church/foundations\">Watch now</a>
												</td>
											</tr>
										</table>'
			END

			 


			-- Generate message
			SET @HTMLMessage = (SELECT Body FROM SystemEmail WHERE Id = 17)
		
			-- Escape double quotes to prevent errors
			SET @HTMLMessage = REPLACE(@HTMLMessage, '"', '\"')

			-- Generate merge fields
			SET @EmailMergeValues =
				'{
					"NickName": "' + ISNULL(@NickName, '') + '",
					"HTMLNextSteps": "' + ISNULL(@HTMLNextSteps, '') + '",
					"NHPastorName": "' + ISNULL(@NHPastorName, '') + '",
					"NHPastorPhone": "' + ISNULL(@NHPastorPhone, '') + '",
					"NHPastorEmail": "' + ISNULL(@NHPastorEmail, '') + '"
				}'

			-- Generate JSON
			SET @EmailJson =
				'{
					"HtmlMessage": "' + ISNULL(@HTMLMessage, '') + '",
					"FromName": "' + ISNULL(@NHPastorName, '') + '",
					"FromAddress": "' + ISNULL(@NHPastorEmail, '') + '",
					"ReplyTo": "' + ISNULL(@NHPastorEmail, '') + '",
					"DefaultPlainText": ""
				}'


			--Insert email message into database
			INSERT INTO Communication
			(
				[CreatedDateTime],
				[CreatedByPersonAliasId],
				[MediumEntityTypeId],
				[Guid],
				[IsBulkCommunication],
				[MediumDataJson],
				[Status],
				[Subject],
				[SenderPersonAliasId],
				[ReviewedDateTime]
			)
			VALUES
			(
				DATEADD(Minute, -31, GETDATE()),
				1,--todo
				37,
				NEWID(),
				0,
				@EmailJson,
				3, --Approved
				'CCV: Baptism Follow Up',
				1,
				DATEADD(Minute, -31, GETDATE())
			)
	
	
			--Grab the identity number from the last insert
			SET @CommunicationID = SCOPE_IDENTITY()

			INSERT INTO CommunicationRecipient
			(	
				[CommunicationId],
				[Status],
				[Guid],
				[CreatedDateTime],
				[ModifiedDateTime],
				[CreatedByPersonAliasId],
				[PersonAliasId],
				[AdditionalMergeValuesJson]
			)
			VALUES
			(	
				@CommunicationID,
				0, --Pending
				NEWID(),
				DATEADD(Minute, -31, GETDATE()),
				DATEADD(Minute, -31, GETDATE()),
				1,
				dbo.ufnUtility_GetPrimaryPersonAliasId(@PersonID),
				@EmailMergeValues
			)

			-- Update Baptism sent to true
			INSERT INTO AttributeValue
			(
				[IsSystem],
				[AttributeId],
				[EntityId],
				[Value],
				[Guid],
				[CreatedDateTime],
				[ModifiedDateTime]
			)
			VALUES
			(
				0,
				@EmailSentAttributeId,
				@PersonID,
				'True',
				NEWID(),
				GETDATE(),
				GETDATE()
			)

		FETCH NEXT FROM c1 INTO @PersonID, @NickName, @LastName, @Email, @InStartingPoint, 
								@InNHGroup, @IsServing, @NHPastorName, @NHPastorEmail, @NHPastorPhone

	END
	
	CLOSE c1
	DEALLOCATE c1


   
END