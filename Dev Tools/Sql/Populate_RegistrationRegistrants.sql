-- Populates Registration and RegistrationRegistrant for the existing RegistrationInstances with the Person records in the database

/* Set this @registrationInstanceId variable here and down below */
DECLARE @registrationInstanceId INT = 1;


BEGIN TRANSACTION

    INSERT INTO [dbo].[Registration] (
	    [RegistrationInstanceId]
	    ,[PersonAliasId]
	    ,[FirstName]
	    ,[LastName]
	    ,[ConfirmationEmail]
	    ,[GroupId]
	    ,[CreatedDateTime]
	    ,[ModifiedDateTime]
	    ,[CreatedByPersonAliasId]
	    ,[ModifiedByPersonAliasId]
	    ,[Guid]
	    ,[ForeignKey]
	    ,[DiscountCode]
	    ,[DiscountPercentage]
	    ,[DiscountAmount]
	    ,[ForeignGuid]
	    ,[ForeignId]
	    ,[IsTemporary]
	    ,[LastPaymentReminderDateTime]
	    )
    SELECT ri.Id
	    ,pa.Id
	    ,p.FirstName
	    ,p.LastName
	    ,p.Email
	    ,NULL [GroupId]
	    ,GetDate() [CreatedDateTime]
	    ,GetDate() [ModifiedDateTime]
	    ,NULL [CreatedByPersonAliasId]
	    ,NULL [ModifiedByPersonAliasId]
	    ,newid() [Guid]
	    ,NULL  [ForeignKey]
	    ,NULL [DiscountCode]
	    ,0.0 [DiscountPercentage]
	    ,0.0 [DiscountAmount]
	    ,NULL [ForeignGuid]
	    ,NULL [ForeignId]
	    ,0 [IsTemporary]
	    ,NULL [LastPaymentReminderDateTime]
    FROM RegistrationInstance ri
    JOIN PersonAlias pa ON 1 = 1
    JOIN Person p ON pa.PersonId = p.Id
    WHERE pa.Id NOT IN (
		    SELECT PersonAliasId
		    FROM Registration
		    WHERE RegistrationInstanceId = ri.Id
		    )
    AND ri.Id = @registrationInstanceId
    GO

    IF CURSOR_STATUS('global', 'personAliasIdCursor') >= - 1
    BEGIN
	    DEALLOCATE personAliasIdCursor;
    END

    BEGIN

        /******************************************/
        /* ALSO SET THIS VARIABLE HERE TOO   vvvv */
        DECLARE @registrationInstanceId INT = 1;
        /* ALSO SET THIS VARIABLE HERE TOO   ^^^^ */
        /******************************************/

        DECLARE @registrantPersonAliasId INT;

	    -- put all personIds in randomly ordered cursor to speed up getting a random personAliasId for each attendance
	    DECLARE personAliasIdCursor CURSOR LOCAL FAST_FORWARD
	    FOR
	    SELECT Id
	    FROM PersonAlias
	    WHERE Id NOT IN (
			    SELECT PersonAliasId
			    FROM RegistrationRegistrant
                WHERE RegistrationId = @registrationInstanceId
			    )
	    ORDER BY newid()

	    OPEN personAliasIdCursor;
	    SET NOCOUNT ON

	    FETCH NEXT
	    FROM personAliasIdCursor
	    INTO @registrantPersonAliasId;

	    WHILE (@@FETCH_STATUS = 0)
	    BEGIN
		    INSERT INTO [dbo].[RegistrationRegistrant] (
			    [RegistrationId]
			    ,[PersonAliasId]
			    ,[GroupMemberId]
			    ,[Cost]
			    ,[CreatedDateTime]
			    ,[ModifiedDateTime]
			    ,[CreatedByPersonAliasId]
			    ,[ModifiedByPersonAliasId]
			    ,[Guid]
			    ,[ForeignKey]
			    ,[ForeignGuid]
			    ,[ForeignId]
			    ,[OnWaitList]
			    ,[DiscountApplies]
                ,[RegistrationTemplateId]
			    )
		    SELECT TOP 1 r.Id [RegistrationId]
			    ,@registrantPersonAliasId
			    ,NULL [GroupMemberId]
			    ,0.0 [Cost]
			    ,GetDate() [CreatedDateTime]
			    ,GetDate() [ModifiedDateTime]
			    ,NULL [CreatedByPersonAliasId]
			    ,NULL [ModifiedByPersonAliasId]
			    ,NEWID() [Guid]
			    ,NULL [ForeignKey]
			    ,NULL [ForeignGuid]
			    ,NULL [ForeignId]
			    ,0 [OnWaitList]
			    ,0 [DiscountApplies]
                ,ri.RegistrationTemplateId [RegistrationTemplateId]
		    FROM [Registration] r
            INNER JOIN [RegistrationInstance] ri ON ri.Id = r.RegistrationInstanceId
		    ORDER BY NEWID()

		    FETCH NEXT
		    FROM personAliasIdCursor
		    INTO @registrantPersonAliasId;
	    END
    END

    SELECT max(ri.Name) [RegistrationInstance], count(*) [Number of RegistrationRegistrant records]
    FROM RegistrationRegistrant rr
    join Registration r on rr.RegistrationId = r.Id
    join RegistrationInstance ri on r.RegistrationInstanceId = ri.Id
    group by ri.Id

COMMIT TRANSACTION