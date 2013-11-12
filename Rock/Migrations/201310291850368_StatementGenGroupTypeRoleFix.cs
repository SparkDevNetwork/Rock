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
    public partial class StatementGenGroupTypeRoleFix : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
alter function [dbo].[ufn_person_group_to_person_names]
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
end

" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
alter function [dbo].[ufn_person_group_to_person_names] 
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
        }
    }
}
