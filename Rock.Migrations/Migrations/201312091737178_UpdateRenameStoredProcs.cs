//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class UpdateRenameStoredProcs : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( "drop function [ufn_person_group_to_person_names]" );
            Sql( "drop function [ufn_csv_to_table]" );
            Sql( "drop procedure [sp_get_contribution_person_group_address]" );
            Sql( "drop procedure [Person_sp_Merge]" );
            Sql( "drop procedure [BinaryFile_sp_getByID]" );

            Sql( @"create function [dbo].[ufnPersonGroupToPersonName] 
( 
@personId int, -- NULL means generate person names from Group Members. NOT-NULL means just get FullName from Person
@groupId int
)
returns @personNamesTable table ( PersonNames varchar(max))
as
begin
    declare @personNames varchar(max); 
    declare @adultLastNameCount int;
    declare @groupFirstNames varchar(max) = '';
    declare @groupLastName varchar(max);
    declare @groupAdultFullNames varchar(max) = '';
    declare @groupNonAdultFullNames varchar(max) = '';
    declare @groupMemberTable table (LastName varchar(max), FirstName varchar(max), FullName varchar(max), GroupRoleGuid uniqueidentifier );
    declare @GROUPTYPEROLE_FAMILY_MEMBER_ADULT uniqueidentifier = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42';

    if (@personId is not null) 
    begin
        -- just getting the Person Names portion of the address for an individual person
        select @personNames = [FullName] from [Person] where [Id] = @personId;
    end
    else
    begin
        -- populate a table variable with the data we'll need for the different cases
        insert into @groupMemberTable 
        select 
            [p].[LastName], [p].[FirstName], [p].[FullName], [gr].[Guid]
        from 
            [GroupMember] [gm] 
        join 
            [Person] [p] 
        on 
            [p].[Id] = [gm].[PersonId] 
        join
            [GroupTypeRole] [gr]
        on 
            [gm].[GroupRoleId] = [gr].[Id]
        where 
            [GroupId] = @groupId;
        
        -- determine adultCount and if we can use the same lastname for all adults, and get lastname while we are at it
        select 
            @adultLastNameCount = count(distinct [LastName])
            ,@groupLastName = max([LastName])
        from 
            @groupMemberTable
        where
            [GroupRoleGuid] = @GROUPTYPEROLE_FAMILY_MEMBER_ADULT;  

        if @adultLastNameCount > 0 
        begin
            -- get the FirstNames and Adult FullNames for use in the cases of families with Adults
            select 
                @groupFirstNames = @groupFirstNames + [FirstName] + ' & '
                ,@groupAdultFullNames = @groupAdultFullNames + [FullName] + ' & '
            from      
                @groupMemberTable
            where
                [GroupRoleGuid] = @GROUPTYPEROLE_FAMILY_MEMBER_ADULT;

            -- cleanup the trailing ' &'s
            if len(@groupFirstNames) > 2 begin
                -- trim the extra ' &' off the end 
                set @groupFirstNames = SUBSTRING(@groupFirstNames, 0, len(@groupFirstNames) - 1)
            end 

            if len(@groupAdultFullNames) > 2 begin
                -- trim the extra ' &' off the end 
                set @groupAdultFullNames = SUBSTRING(@groupAdultFullNames, 0, len(@groupAdultFullNames) - 1)  
            end
        end             

        if @adultLastNameCount = 0        
        begin
            -- get the NonAdultFullNames for use in the case of families without adults 
            select 
                @groupNonAdultFullNames = @groupNonAdultFullNames + [FullName] + ' & '
            from 
                @groupMemberTable
            order by [FullName]

            if len(@groupNonAdultFullNames) > 2 begin
                -- trim the extra ' &' off the end 
                set @groupNonAdultFullNames = SUBSTRING(@groupNonAdultFullNames, 0, len(@groupNonAdultFullNames) - 1)  
            end
        end

        if (@adultLastNameCount = 1)
        begin
            -- just one lastname and at least one adult. Get the Person Names portion of the address in the format <MaleAdult> & <FemaleAdult> <LastName>
            set @personNames = @groupFirstNames + ' ' + @groupLastName;
        end
        else if (@adultLastNameCount = 0)
        begin
             -- no adults in family, list all members of the family in 'Fullname & FullName & ...' format
            set @personNames = @groupNonAdultFullNames;
        end
        else
        begin
            -- multiple adult lastnames
            set @personNames = @groupAdultFullNames;
        end 
    end

    insert into @personNamesTable ( [PersonNames] ) values ( @personNames);

  return
end" );
            Sql( @"create function [dbo].[ufnCsvToTable] ( @input varchar(max) )
returns @outputTable table ( [id] int )
as
begin
    declare @numericstring varchar(10)

    while LEN(@input) > 0
    begin
        set @numericString      = LEFT(@input, 
                                ISNULL(NULLIF(CHARINDEX(',', @input) - 1, -1),
                                LEN(@input)))
        set @input = SUBSTRING(@input,
                                     ISNULL(NULLIF(CHARINDEX(',', @input), 0),
                                     LEN(@input)) + 1, LEN(@input))

        insert into @OutputTable ( [id] )
        values ( CAST(@numericString as int) )
    end
    
    return
end
" );
            Sql( @"CREATE PROCEDURE [dbo].[spPersonMerge]
@OldId int, 
@NewId int, 
@DeleteOldPerson bit

AS

DECLARE @OldGuid uniqueidentifier
DECLARE @NewGuid uniqueidentifier

SET @OldGuid = ( SELECT [Guid] FROM [Person] WHERE [Id] = @OldId )
SET @NewGuid = ( SELECT [Guid] FROM [Person] WHERE [Id] = @NewId )

IF @OldGuid IS NOT NULL AND @NewGuid IS NOT NULL
BEGIN

	DECLARE @PersonEntityTypeId INT
	SET @PersonEntityTypeId = ( SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Person' )

	DECLARE @PersonFieldTypeId INT
	SET @PersonFieldTypeId = ( SELECT [Id] FROM [FieldType] WHERE [Class] = 'Rock.Field.Types.PersonFieldType' )


	--BEGIN TRANSACTION


	-- Attribute Value
	-----------------------------------------------------------------------------------------------
	-- Update Attribute Values associated with person 
	-- The new user's attribute value will only get updated if the old user has a value, and the 
	-- new user does not (determining the correct value will eventually be decided by user in a UI)
	UPDATE AVO
		SET [EntityId] = @NewId
	FROM [Attribute] A
	INNER JOIN [AttributeValue] AVO
		ON AVO.[EntityId] = @OldId
		AND AVO.[AttributeId] = A.[Id]
	LEFT OUTER JOIN [AttributeValue] AVN
		ON AVO.[EntityId] = @NewId
		AND AVN.[AttributeId] = A.[Id]
	WHERE A.[EntityTypeId] = @PersonEntityTypeId
	AND AVN.[Id] IS NULL

	-- Delete any attribute values that were not updated (due to new person already having existing 
	-- value)
	DELETE AV
	FROM [Attribute] A
	INNER JOIN [AttributeValue] AV
		ON AV.[EntityId] = @OldId
		AND AV.[AttributeId] = A.[Id]
	WHERE A.[EntityTypeId] = @PersonEntityTypeId

	-- Update Attribute Values that have person as a value
	-- NOTE: BECAUSE VALUE IS A VARCHAR(MAX) COLUMN WE CANT ADD AN INDEX FOR ATTRIBUTEID AND
	-- VALUE.  THIS UPDATE COULD POTENTIALLY BE A BOTTLE-NECK FOR MERGES
	UPDATE AV
		SET [Value] = CAST( @NewGuid AS VARCHAR(64) )
	FROM [Attribute] A
	INNER JOIN [AttributeValue] AV
		ON AV.[AttributeId] = A.[Id]
		AND AV.[Value] = CAST( @OldGuid AS VARCHAR(64) )
	WHERE A.[FieldTypeId] = @PersonFieldTypeId


	-- Authorization
	-----------------------------------------------------------------------------------------------
	-- Update any authorizations associated to old person that do not already have a matching 
	-- authorization for the new person
	UPDATE AO
		SET [PersonId] = @NewId
	FROM [Auth] AO
	LEFT OUTER JOIN [Auth] AN
		ON AN.[PersonId] = @NewId
		AND AN.[EntityTypeId] = AO.[EntityTypeId]
		AND AN.[EntityId] = AO.[EntityId]
		AND AN.[Action] = AO.[Action]
		AND AN.[AllowOrDeny] = AO.[AllowOrDeny]
		AND AN.[SpecialRole] = AO.[SpecialRole]
	WHERE AO.[PersonId] = @OldId
	AND AN.[Id] IS NULL

	-- Delete any authorizations not updated to new person
	DELETE [Auth]
	WHERE [PersonId] = @OldId


	-- Category
	-----------------------------------------------------------------------------------------------
	-- Currently UI does not allow categorizing people, but if it does in the future, would need 
	-- to add script to handle merge


	-- Communication Recipient
	-----------------------------------------------------------------------------------------------
	-- Update any communication recipients associated to old person to the new person where the new
	-- person does not already have the recipient record
	UPDATE CRO
		SET [PersonId] = @NewId
	FROM [CommunicationRecipient] CRO
	LEFT OUTER JOIN [CommunicationRecipient] CRN
		ON CRN.[CommunicationId] = CRO.[CommunicationId]
		AND CRN.[PersonId] = @NewId
	WHERE CRO.[PersonId] = @OldId
	AND CRN.[Id] IS NULL

	-- Delete any remaining recipents that were not updated
	DELETE [CommunicationRecipient]
	WHERE [PersonId] = @OldId

	-- Group Member
	-----------------------------------------------------------------------------------------------
	-- Update any group members associated to old person to the new person where the new is not 
	-- already in the group with the same role
	UPDATE GMO
		SET [PersonId] = @NewId
	FROM [GroupMember] GMO
	LEFT OUTER JOIN [GroupMember] GMN
		ON GMN.[GroupId] = GMO.[GroupId]
		AND GMN.[PersonId] = @NewId
		AND GMN.[GroupRoleId] = GMO.[GroupRoleId] -- If person can be in group twice with diff role
	WHERE GMO.[PersonId] = @OldId
	AND GMN.[Id] IS NULL

	-- Delete any group members not updated (already existed with new id)
	DELETE [GroupMember]
	WHERE [PersonId] = @OldId


	-- Note
	-----------------------------------------------------------------------------------------------
	-- Update any note that is associated to the old person to be associated to the new person
	UPDATE N
		SET [EntityId] = @NewId
	FROM [NoteType] NT
	INNER JOIN [Note] N
		ON N.[NoteTypeId] = NT.[Id]
		AND N.[EntityId] = @OldId
	WHERE NT.[EntityTypeId] = @PersonEntityTypeId


	-- Phone Numbers
	-----------------------------------------------------------------------------------------------
	-- Update any phone numbers associated to the old person that do not already exist for the new
	-- person
	UPDATE PNO
		SET [PersonId] = @NewId
	FROM [PhoneNumber] PNO
	INNER JOIN [PhoneNumber] PNN
		ON PNN.[PersonId] = @NewId
		AND PNN.[Number] = PNO.[Number]
		AND PNN.[Extension] = PNO.[Extension]
		AND PNN.[NumberTypeValueId] = PNO.[NumberTypeValueId]
	WHERE PNO.[PersonId] = @OldId
	AND PNN.[Id] IS NULL

	-- Delete any numbers not updated (new person already had same number)
	DELETE [PhoneNumber]
	WHERE [PersonId] = @OldId


	-- Tags
	-----------------------------------------------------------------------------------------------
	-- Update any tags associated to the old person to be associated to the new person as long as 
	-- same tag does not already exist for new person
	UPDATE TIO
		SET [EntityGuid] = @NewGuid
	FROM [Tag] T
	INNER JOIN [TaggedItem] TIO
		ON TIO.[TagId] = T.[Id]
		AND TIO.[EntityGuid] = @OldGuid
	LEFT OUTER JOIN [TaggedItem] TIN
		ON TIN.[TagId] = T.[Id]
		AND TIN.[EntityGuid] = @NewGuid
	WHERE T.[EntityTypeId] = @PersonEntityTypeId
	AND TIN.[Id] IS NULL

	-- Delete any tagged items still associated with old person (new person had same tag)
	DELETE TIO
	FROM [Tag] T
	INNER JOIN [TaggedItem] TIO
		ON TIO.[TagId] = T.[Id]
		AND TIO.[EntityGuid] = @OldGuid
	WHERE T.[EntityTypeId] = @PersonEntityTypeId

	-- If old person and new person have tags with the same name for the same entity type,
	-- update the old person's tagged items to use the new person's tag
	UPDATE TIO
		SET [TagId] = TIN.[Id]
	FROM [Tag] T
	INNER JOIN [Tag] TN
		ON TN.[EntityTypeId] = T.[EntityTypeId]
		AND TN.[EntityTypeQualifierColumn] = T.[EntityTypeQualifierColumn]
		AND TN.[EntityTypeQualifierValue] = T.[EntityTypeQualifierValue]
		AND TN.[Name] = T.[Name]
		AND TN.[OwnerId] = @NewId
	INNER JOIN [TaggedItem] TIO
		ON TIO.[TagId] = T.[Id]
	LEFT OUTER JOIN [TaggedItem] TIN
		ON TIN.[TagId] = TN.[Id]
	WHERE T.[OwnerId] = @OldId
	AND TIN.[Id] IS NULL

	-- Delete any of the old person's tags that have the same name and are associated to same 
	-- entity type as a tag used bo the new person
	DELETE T
	FROM [Tag] T
	INNER JOIN [Tag] TN
		ON TN.[EntityTypeId] = T.[EntityTypeId]
		AND TN.[EntityTypeQualifierColumn] = T.[EntityTypeQualifierColumn]
		AND TN.[EntityTypeQualifierValue] = T.[EntityTypeQualifierValue]
		AND TN.[Name] = T.[Name]
		AND TN.[OwnerId] = @NewId
	WHERE T.[OwnerId] = @OldId


	-- Person Merged
	-----------------------------------------------------------------------------------------------
	-- Add a record to indicate that person was merged with old and new id's and guid's
	INSERT INTO PersonMerged (
		 [Guid]
		,[PreviousPersonId]
		,[PreviousPersonGuid]
		,[NewPersonId]
		,[NewPersonGuid]
	)
	VALUES (
		 NEWID()
		,@OldId
		,@OldGuid
		,@NewId
		,@NewGuid
	)


	-- Remaining Tables
	-----------------------------------------------------------------------------------------------
	-- Update any column on any table that has a foreign key relationship to the Person table's Id
	-- column  

	DECLARE @Sql varchar(max)

	DECLARE ForeignKeyCursor INSENSITIVE CURSOR FOR
	SELECT 
		' UPDATE ' + tso.name +
		' SET ' + tac.name + ' = ' + CAST(@NewId as varchar) +
		' WHERE ' + tac.name + ' = ' + CAST(@OldId as varchar) 
	FROM sys.foreign_key_columns kc
	INNER JOIN sys.foreign_keys k ON kc.constraint_object_id = k.object_id
	INNER JOIN sys.all_objects so ON so.object_id = kc.referenced_object_id
	INNER JOIN sys.all_columns rac ON rac.column_id = kc.referenced_column_id AND rac.object_id = so.object_id
	INNER JOIN sys.all_objects tso ON tso.object_id = kc.parent_object_id
	INNER JOIN sys.all_columns tac ON tac.column_id = kc.parent_column_id AND tac.object_id = tso.object_id
	WHERE so.name = 'Person'
	AND rac.name = 'Id'
	AND tso.name NOT IN (
		 'Auth'
		,'CommunicationRecipient'
		,'GroupMember'
		,'PhoneNumber'
	)

	OPEN ForeignKeyCursor

	FETCH NEXT
	FROM ForeignKeyCursor
	INTO @Sql

	WHILE (@@FETCH_STATUS <> -1)
	BEGIN

		IF (@@FETCH_STATUS = 0)
		BEGIN

			EXEC(@Sql)
			
		END
		
		FETCH NEXT
		FROM ForeignKeyCursor
		INTO @Sql

	END

	CLOSE ForeignKeyCursor
	DEALLOCATE ForeignKeyCursor


	-- Person
	-----------------------------------------------------------------------------------------------
	-- Optionally delete the old person record.  By this time it should not have any relationships 
	-- with other tables (if Rock is being synced with other data source, the sync may handle the
	-- delete)
	
	IF @DeleteOldPerson = 1 
	BEGIN
		DELETE Person
		WHERE [Id] = @OldId
	END
	
	--COMMIT TRANSACTION


END" );
            Sql( @"create procedure [spContributionStatementQuery]
	@startDate datetime,
    @endDate datetime,
    @accountIds varchar(max), -- comma delimited list of integers. NULL means all
    @personId int, -- NULL means all persons
    @orderByZipCode bit
as
begin
    /*  Note:  This stored procedure returns the Mailing Addresses and any CustomMessages for the Contribution Statement, but not the actual transactions */	

    -- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	set nocount on;

    ;with tranListCTE
    as
    (
        select  
            [AuthorizedPersonId] 
         from 
            [FinancialTransaction] [ft]
         inner join [FinancialTransactionDetail] [ftd]
         on [ft].[Id] = [ftd].[TransactionId]
         where 
            ([TransactionDateTime] >= @startDate and [TransactionDateTime] < @endDate)
         and 
            (
                (@accountIds is null)
                or
                (ftd.AccountId in (select * from ufnCsvToTable(@accountIds)))
            )
    )

    select 
        [pg].[PersonId],
        [pg].[GroupId],
        [pn].[PersonNames] [AddressPersonNames],
        [l].[Street1],
        [l].[Street2],
        [l].[City],
        [l].[State],
        [l].[Zip],
        @startDate [StartDate],
        @endDate [EndDate],
        null [CustomMessage1],
        null [CustomMessage2]
    from (
        -- Get distinct Giving Groups for Persons that have a specific GivingGroupId and have transactions that match the filter
        -- These are Persons that give as part of a Group.  For example, Husband and Wife
        select distinct
            null [PersonId], 
            [g].[Id] [GroupId]
        from 
            [Person] [p]
        inner join 
            [Group] [g]
        on 
            [p].[GivingGroupId] = [g].[Id]
        where [p].[Id] in (select * from tranListCTE)
        union
        -- Get Persons and their GroupId(s) that do not have GivingGroupId and have transactions that match the filter.        
        -- These are the persons that give as individuals vs as part of a group. We need the Groups (families they belong to) in order 
        -- to determine which address(es) the statements need to be mailed to 
        select  
            [p].[Id] [PersonId],
            [g].[Id] [GroupId]
        from
            [Person] [p]
        join 
            [GroupMember] [gm]
        on 
            [gm].[PersonId] = [p].[Id]
        join 
            [Group] [g]
        on 
            [gm].[GroupId] = [g].[Id]
        where
            [p].[GivingGroupId] is null
        and
            [g].[GroupTypeId] = (select Id from GroupType where Guid = '790E3215-3B10-442B-AF69-616C0DCB998E' /* GROUPTYPE_FAMILY */)
        and [p].[Id] in (select * from tranListCTE)
    ) [pg]
    cross apply [ufnPersonGroupToPersonName]([pg].[PersonId], [pg].[GroupId]) [pn]
    join 
        [GroupLocation] [gl] 
    on 
        [gl].[GroupId] = [pg].[GroupId]
    join
        [Location] [l]
    on 
        [l].[Id] = [gl].[LocationId]
    where 
        [gl].[IsMailingLocation] = 1
    and
        [gl].[GroupLocationTypeValueId] = (select Id from DefinedValue where Guid = '8C52E53C-2A66-435A-AE6E-5EE307D9A0DC' /* LOCATION_TYPE_HOME */)
    and
        (
            (@personId is null) 
        or 
            ([pg].[PersonId] = @personId)
        )
    order by
    case when @orderByZipCode = 1 then Zip end
end
" );
            Sql( @"CREATE PROCEDURE [spBinaryFileGet]
    @Id int,
    @Guid uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    /* NOTE!: Column Order cannot be changed without changing BinaryFileService.partial.cs due to CommandBehavior.SequentialAccess */
    SELECT 
        bf.Id,
        bf.IsTemporary, 
        bf.IsSystem,
        bf.BinaryFileTypeId,
        bf.Url,
        bf.[FileName], 
        bf.MimeType,
        bf.LastModifiedDateTime,
        bf.[Description],
        bf.StorageEntityTypeId,
        bf.[Guid],
        /* if the BinaryFile as StorageEntityTypeId set, use that. Otherwise use the default StorageEntityTypeId from BinaryFileType  */
        COALESCE (bfse.Name,bftse.Name ) as StorageEntityTypeName,
        bfd.Content
    FROM BinaryFile bf 
    LEFT JOIN BinaryFileData bfd
        ON bf.Id = bfd.Id
    LEFT JOIN EntityType bfse
        ON bf.StorageEntityTypeId = bfse.Id
    LEFT JOIN BinaryFileType bft
        on bf.BinaryFileTypeId = bft.Id
    LEFT JOIN EntityType bftse
        ON bft.StorageEntityTypeId = bftse.Id
    WHERE 
        (@Id > 0 and bf.Id = @Id)
        or
        (bf.[Guid] = @Guid)
END" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //
        }
    }
}
