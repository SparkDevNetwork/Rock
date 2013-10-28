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
    public partial class ContributionStatements : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // ufn_person_group_to_person_names
            Sql( @"
create function [dbo].[ufn_person_group_to_person_names] 
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
    declare @GROUPROLE_FAMILY_MEMBER_ADULT uniqueidentifier = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42';

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
            [GroupRole] [gr]
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
            [GroupRoleGuid] = @GROUPROLE_FAMILY_MEMBER_ADULT;  

        if @adultLastNameCount > 0 
        begin
            -- get the FirstNames and Adult FullNames for use in the cases of families with Adults
            select 
                @groupFirstNames = @groupFirstNames + [FirstName] + ' & '
                ,@groupAdultFullNames = @groupAdultFullNames + [FullName] + ' & '
            from      
                @groupMemberTable
            where
                [GroupRoleGuid] = @GROUPROLE_FAMILY_MEMBER_ADULT;

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
end
" );

            // ufn_csv_to_table
            Sql( @"
create function [dbo].[ufn_csv_to_table] ( @input varchar(max) )
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
            // sp_get_contribution_person_group_address
            Sql( @"
create procedure [sp_get_contribution_person_group_address]
	@startDate datetime,
    @endDate datetime,
    @accountIds varchar(max), -- comma delimited list of integers. NULL means all
    @personId int, -- NULL means all persons
    @orderByZipCode bit
as
begin
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
                (ftd.AccountId in (select * from ufn_csv_to_table(@accountIds)))
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
    cross apply [ufn_person_group_to_person_names]([pg].[PersonId], [pg].[GroupId]) [pn]
    join 
        [GroupLocation] [gl] 
    on 
        [gl].[GroupId] = [pg].[GroupId]
    join
        [Location] [l]
    on 
        [l].[Id] = [gl].[LocationId]
    where 
        [gl].IsMailing = 1
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
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
drop procedure [sp_get_contribution_person_group_address];

drop function [dbo].[ufn_csv_to_table];

drop function [dbo].[ufn_person_group_to_person_names];

" );
        }
    }
}
