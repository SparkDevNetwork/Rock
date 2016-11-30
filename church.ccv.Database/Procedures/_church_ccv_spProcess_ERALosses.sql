/*
<doc>
	<summary>
 		This stored procedure builds the data mart table _church_ccv_spProcess_ERALosses
	</summary>
	
	<remarks>	
	</remarks>
	<code>
		EXEC [dbo].[_church_ccv_spProcess_ERALosses]
	</code>
</doc>
*/
CREATE PROC [dbo].[_church_ccv_spProcess_ERALosses]
AS
-- Insert New Losses
INSERT INTO _church_ccv_Datamart_ERALoss (
    FamilyId
    ,LossDate
    ,SendEmail
    )
SELECT NL.FamilyId
    ,NL.LossDate
    ,1
FROM _church_ccv_v_ERALoss NL
LEFT OUTER JOIN _church_ccv_Datamart_ERALoss OL ON OL.FamilyId = NL.FamilyId
WHERE NL.LossDate IS NOT NULL
    AND OL.FamilyId IS NULL

-- Update old losses
UPDATE OL
SET LossDate = NL.LossDate
    ,Processed = 0
    ,SendEmail = 1
    ,sent = 0
FROM _church_ccv_v_ERALoss NL
INNER JOIN _church_ccv_Datamart_ERALoss OL ON OL.FamilyId = NL.FamilyId
    AND OL.LossDate <> NL.LossDate
WHERE NL.LossDate IS NOT NULL

-- Remove any unprocessed losses that are no longer a loss
DELETE _church_ccv_Datamart_ERALoss
WHERE Processed = 0
    AND FamilyId NOT IN (
        SELECT FamilyId
        FROM _church_ccv_v_ERALoss
        )

    -- Queue Communication to nofify pastors
    DECLARE @CommunicationID int

	DECLARE @EmailMessage VARCHAR(MAX)

	-- Add header
	SET @EmailMessage = (SELECT Value FROM AttributeValue WHERE AttributeId = 140)

	-- Add message
	SET @EmailMessage = @EmailMessage + 
	'{{ Person.NickName }},
	<br/>
	<br/>
	The ERA Loss Report has been updated. Please take a moment to process the families in your neighborhoods.
	<br/>
	<br/>
	<a href="http://rock.ccv.church/page/525">ERA Loss Report</a>
	<br/>
	<br/>'

	-- Add footer
	SET @EmailMessage = @EmailMessage + (SELECT Value FROM AttributeValue WHERE AttributeId = 141)

	-- Escape double quotes to prevent errors
    SET @EmailMessage = REPLACE(@EmailMessage, '"', '\"')

	-- Generate JSON
    DECLARE @EmailJson VARCHAR(MAX) = '{
					"HtmlMessage": "' + ISNULL(@EmailMessage, '') + '",
					"FromName": "Kris Simpson",
					"FromAddress": "krissimpson@ccv.church",
					"ReplyTo": "krissimpson@ccv.church",
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
		1,
		37,
		NEWID(),
		0,
		@EmailJson,
		3, --Approved
		'Potential ERA Losses',
		367077, -- Kris Simpson
		DATEADD(Minute, -31, GETDATE())
	)
	
	--Grab the identity number from the last insert
	SET @CommunicationID = SCOPE_IDENTITY()

	;WITH NHPastors AS
	(
		SELECT DISTINCT PersonId
		FROM GroupMember GM
		WHERE GroupRoleId = 45
	)

	INSERT INTO CommunicationRecipient
	(	
		[CommunicationId],
		[Status],
		[Guid],
		[CreatedDateTime],
		[ModifiedDateTime],
		[CreatedByPersonAliasId],
		[PersonAliasId]
	)
	SELECT
		@CommunicationID,
		0, --Pending
		NEWID(),
		DATEADD(Minute, -31, GETDATE()),
		DATEADD(Minute, -31, GETDATE()),
		1,
		PersonId
	FROM NHPastors