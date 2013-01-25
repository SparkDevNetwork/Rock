DECLARE 
	@personRecordType int = (SELECT id FROM DefinedValue WHERE guid = '36CF10D6-C695-413D-8E7C-4546EFEF385E'),
	@activeRecordStatus int = (SELECT id FROM DefinedValue WHERE guid = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E'),
	@primaryPhone int = (SELECT id FROM DefinedValue WHERE guid = '407E7E45-7B2E-4FCD-9605-ECB1339F2453'),
	@personId int,
	@firstName nvarchar(50),
	@lastName nvarchar(50),
	@email nvarchar(75),
	@phoneNumber decimal,
 
	@year int,
	@month int,
	@day int,
	@personCounter int = 0,  
	@maxPerson int = 99999

begin

while @personCounter < @maxPerson
	begin
		set @firstName = 'FirstName' + CONVERT(nvarchar(100), ROUND(rand() * 2000, 0));
		set @lastName = 'LastName' + CONVERT(nvarchar(100), ROUND(rand() * 5000, 0));
		set @email = @firstName + '.' + @lastName + '@nowhere.com';
		set @year = CONVERT(nvarchar(100), ROUND(rand() * 80, 0) + 1932);
		set @month = CONVERT(nvarchar(100), ROUND(rand() * 11, 0) + 1);
		set @day = CONVERT(nvarchar(100), ROUND(rand() * 26, 0) + 1);
		set @phoneNumber = ROUND(rand() * 0095551212, 0)+ 6230000000;
		INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
		VALUES (0, @firstName , @lastName, @day, @month, @year, 1, @email, 1, 0, NEWID(), @personRecordType, @activeRecordStatus)
		SET @personId = SCOPE_IDENTITY()

		INSERT INTO [PhoneNumber] (IsSystem, PersonId, Number, IsMessagingEnabled, IsUnlisted, Guid, NumberTypeValueId)
		VALUES (0, @personId, @phoneNumber, 1, 0, newid(), @primaryPhone);

		set @personCounter += 1;
	end

end