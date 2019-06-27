-- Scheduled Transactions didn't write to history until v7.4, so convert those into History notes

DECLARE @historyCategoryId INT = (
		SELECT TOP 1 Id
		FROM Category
		WHERE Guid = '477EE3BE-C68F-48BD-B218-FAFC99AF56B3'
		)
	,@entityTypeIdScheduledTransaction INT = (
		SELECT TOP 1 Id
		FROM EntityType
		WHERE [Guid] = '76824E8A-CCC4-4085-84D9-8AF8C0807E20'
		)
	,@noteTypeIdScheduledTransaction INT = (
		SELECT TOP 1 Id
		FROM NoteType
		WHERE [Guid] = '360CFFE2-7FE3-4B0B-85A7-BFDACC9AF588'
		)

BEGIN
	-- convert 'Created Transaction' notes into History if they aren't aleady in History
	INSERT INTO [History] (
		IsSystem
		,CategoryId
		,EntityTypeId
		,EntityId
		,[Guid]
		,CreatedDateTime
		,ModifiedDateTime
		,CreatedByPersonAliasId
		,ModifiedByPersonAliasId
		,[Verb]
		,[ChangeType]
		,[ValueName]
		)
	SELECT n.IsSystem
		,@historyCategoryId [CategoryId]
		,@entityTypeIdScheduledTransaction [EntityTypeId]
		,n.EntityId
		,newid() [Guid]
		,n.CreatedDateTime
		,n.ModifiedDateTime
		,n.CreatedByPersonAliasId
		,n.ModifiedByPersonAliasId
		,'ADD' [Verb]
		,'Record' [ChangeType]
		,'Transaction' [ValueName]
	FROM [Note] n
	WHERE NoteTypeId = @noteTypeIdScheduledTransaction
		AND [Caption] = 'Created Transaction'
		AND EntityId NOT IN (
			SELECT EntityId
			FROM [History]
			WHERE EntityTypeId = @entityTypeIdScheduledTransaction
				AND [Verb] = 'ADD'
			)

	-- convert 'Updated Transaction','Cancelled Transaction','Reactivated Transaction' notes into History if they aren't aleady in History
	INSERT INTO [History] (
		IsSystem
		,CategoryId
		,EntityTypeId
		,EntityId
		,[Summary]
		,[Guid]
		,CreatedDateTime
		,ModifiedDateTime
		,CreatedByPersonAliasId
		,ModifiedByPersonAliasId
		,[Verb]
		,[ChangeType]
		,[ValueName]
		,[NewValue]
		)
	SELECT n.IsSystem
		,@historyCategoryId [CategoryId]
		,@entityTypeIdScheduledTransaction [EntityTypeId]
		,n.EntityId
		,n.Caption [Summary]
		,newid() [Guid]
		,n.CreatedDateTime
		,n.ModifiedDateTime
		,n.CreatedByPersonAliasId
		,n.ModifiedByPersonAliasId
		,'MODIFY' [Verb]
		,'Property' [ChangeType]
		,CASE n.Caption
			WHEN 'Cancelled Transaction'
				THEN 'Is Active'
			WHEN 'Reactivated Transaction'
				THEN 'Is Active'
			ELSE 'Transaction'
			END [ValueName]
		,CASE n.Caption
			WHEN 'Cancelled Transaction'
				THEN 'False'
			WHEN 'Reactivated Transaction'
				THEN 'True'
			ELSE ''
			END [NewValue]
	FROM [Note] n
	WHERE NoteTypeId = @noteTypeIdScheduledTransaction
		AND [Caption] IN (
			'Updated Transaction'
			,'Cancelled Transaction'
			,'Reactivated Transaction'
			)
		AND EntityId NOT IN (
			SELECT EntityId
			FROM [History]
			WHERE EntityTypeId = @entityTypeIdScheduledTransaction
				AND [Verb] = 'MODIFY'
			)
	ORDER BY [Caption]

	DELETE FROM [Note] WHERE NoteTypeId = @noteTypeIdScheduledTransaction
		AND [Caption] IN (
			'Created Transaction'
			,'Updated Transaction'
			,'Cancelled Transaction'
			,'Reactivated Transaction'
			)
END