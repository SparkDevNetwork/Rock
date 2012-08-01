CREATE PROC [dbo].[cust_rock_sp_update_person]
@PersonId int

AS

DECLARE @Id int
DECLARE @RecordStatusId int
DECLARE @RecordStatusReasonId int
DECLARE @PersonStatusId int
DECLARE @TitleId int
DECLARE @GivenName nvarchar(50)
DECLARE @NickName nvarchar(50)
DECLARE @LastName nvarchar(50)
DECLARE @SuffixId int
DECLARE @BirthDay int
DECLARE @BirthMonth int
DECLARE @BirthYear int
DECLARE @Gender int
DECLARE @MaritalStatusId int
DECLARE @AnniversaryDate datetime
DECLARE @GraduationDate datetime
DECLARE @Email nvarchar(75)

SELECT
	 @Id = P.[foreign_key]
	,@RecordStatusId = 
		CASE P.[record_status]
			WHEN 0 THEN 3	-- Active 
			WHEN 1 THEN 4	-- Inactive
			ELSE 5			-- Pending
		END
	,@RecordStatusReasonId = RSR.[foreign_key]
	,@PersonStatusId = S.[foreign_key]
	,@TitleId = T.[foreign_key]
	,@GivenName = P.[first_name]
	,@NickName = CASE WHEN P.[nick_name] <> P.[first_name] THEN P.[nick_name] ELSE NULL END 
	,@LastName = P.[last_name]
	,@SuffixId = SFX.[foreign_key]
	,@BirthDay = CASE WHEN P.[birth_date] = '1/1/1900' THEN NULL ELSE DAY(P.[birth_date]) END
	,@BirthMonth = CASE WHEN P.[birth_date] = '1/1/1900' THEN NULL ELSE MONTH(P.[birth_date]) END
	,@BirthYear = CASE WHEN P.[birth_date] = '1/1/1900' THEN NULL ELSE YEAR(P.[birth_date]) END
	,@Gender = 
		CASE P.Gender
			WHEN 0 THEN 1	-- Male
			WHEN 1 THEN 2	-- Female
			ELSE 0			-- Unknown
		END
	,@MaritalStatusId = MS.[foreign_key]
	,@AnniversaryDate = CASE WHEN P.[anniversary_date] <> '1/1/1900' THEN P.[anniversary_date] ELSE NULL END
	,@GraduationDate = CASE WHEN P.[graduation_date] <> '1/1/1900' THEN P.[graduation_date] ELSE NULL END
	,@Email = [dbo].[core_funct_primary_email](P.[person_id])
FROM [core_person] P WITH (NOLOCK)
LEFT OUTER JOIN [core_lookup] RSR WITH (NOLOCK)
	ON RSR.[lookup_id] = P.[inactive_reason_luid]
LEFT OUTER JOIN [core_lookup] S WITH (NOLOCK)
	ON S.[lookup_id] = P.[member_status]
LEFT OUTER JOIN [core_lookup] T WITH (NOLOCK)
	ON T.[lookup_id] = P.[title_luid]
LEFT OUTER JOIN [core_lookup] SFX WITH (NOLOCK)
	ON SFX.[lookup_id] = P.[suffix_luid]
LEFT OUTER JOIN [core_lookup] MS WITH (NOLOCK)
	ON MS.[lookup_id] = P.[marital_status]
WHERE P.[person_id] = @PersonID

IF @Id IS NULL OR
	NOT EXISTS ( SELECT [Id] FROM [RockChMS].[dbo].[crmPerson] WITH (NOLOCK) WHERE [Id] = @Id)
BEGIN

	INSERT INTO [RockChMS].[dbo].[crmPerson] (
		 [IsSystem]
		,[RecordTypeId]
		,[RecordStatusId]
		,[RecordStatusReasonId]
		,[PersonStatusId]
		,[TitleId]
		,[GivenName]
		,[NickName]
		,[LastName]
		,[SuffixId]
		,[BirthDay]
		,[BirthMonth]
		,[BirthYear]
		,[Gender]
		,[MaritalStatusId]
		,[AnniversaryDate]
		,[GraduationDate]
		,[Email]
		,[DoNotEmail]
		,[CreatedDateTime]
		,[ModifiedDateTime]
		,[CreatedByPersonId]
		,[ModifiedByPersonId]
		,[Guid]
	)
	VALUES (
		 0
		,1	-- Person Record Type
		,@RecordStatusId
		,@RecordStatusReasonId
		,@PersonStatusId
		,@TitleId
		,@GivenName
		,@NickName
		,@LastName
		,@SuffixId
		,@BirthDay
		,@BirthMonth
		,@BirthYear
		,@Gender
		,@MaritalStatusId
		,@AnniversaryDate
		,@GraduationDate
		,@Email
		,0
		,GETDATE()
		,GETDATE()
		,1
		,1
		,NEWID()
	)
	
	SET @Id = SCOPE_IDENTITY()
	
	UPDATE [core_person]
	SET 
		[foreign_key] = @Id
	WHERE [person_id] = @PersonId
	
END
ELSE
BEGIN

	UPDATE [RockChMS].[dbo].[crmPerson]
	SET
		 [RecordStatusId] = @RecordStatusId
		,[RecordStatusReasonId] = @RecordStatusReasonId
		,[PersonStatusId] = @PersonStatusId
		,[TitleId] = @TitleId
		,[GivenName] = @GivenName 
		,[NickName] = @NickName
		,[LastName] = @LastName
		,[SuffixId] = @SuffixId
		,[BirthDay] = @BirthDay
		,[BirthMonth] = @BirthMonth
		,[BirthYear] = @BirthYear
		,[Gender] = @Gender
		,[MaritalStatusId] = @MaritalStatusId
		,[AnniversaryDate] = @AnniversaryDate
		,[GraduationDate] = @GraduationDate
		,[Email] = @Email
		,[ModifiedDateTime] = GETDATE()
		,[ModifiedByPersonId] = 1
	WHERE [Id] = @Id

END
