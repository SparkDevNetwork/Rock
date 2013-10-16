-- drop/create the function
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufn_person_group_to_person_names]') and [type] = 'TF')
BEGIN
    DROP FUNCTION [dbo].[ufn_person_group_to_person_names];
END
GO

CREATE FUNCTION [dbo].[ufn_person_group_to_person_names] 
( 
@personId int, -- NULL means generate person names from Group Members. NOT-NULL means just get FullName from Person
@groupId int
)
RETURNS @personNamesTable TABLE ( PersonNames varchar(max))
AS
BEGIN
    declare @personNames varchar(max); 
    declare @adultLastNameCount int;
    declare @groupFirstNames varchar(max) = '';
    declare @groupLastName varchar(max);
    declare @groupAdultFullNames varchar(max) = '';
    declare @groupNonAdultFullNames varchar(max) = '';

    if (@personId is not null) 
    begin
        -- just getting the Person Names portion of the address for an individual person
        select @personNames = FullName from Person where Id = @personId;
    end
    else
    begin
        -- determine if we can use the same lastname for everybody, or if there some of the persons have different lastnames, and get the firstnames and lastname, and adult fullnames while we are at it
        select 
            @adultLastNameCount = count(distinct [p].[LastName]),
            @groupFirstNames = @groupFirstNames + [p].[FirstName] + ' & ',
            @groupLastName = max([p].[LastName]),
            @groupAdultFullNames = @groupAdultFullNames + [p].[FullName] + ' & '
        from 
            [GroupMember] [gm] 
        join 
            [Person] [p] 
        on 
            [p].[Id] = [gm].[PersonId] 
        where 
            [GroupId] = @groupId
        and
            [GroupRoleId] = (select [Id] from [GroupRole] where [Guid] =  '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42' /* GROUPROLE_FAMILY_MEMBER_ADULT */)
        group by p.FirstName, p.FullName, p.Gender
        order by p.Gender

        if (@adultLastNameCount = 1)
        begin
            -- just one lastname and at least one adult. Get the Person Names portion of the address in the format "<MaleAdult> & <FemaleAdult> <LastName>"
            if len(@groupFirstNames) > 2 begin
                -- trim the extra ' &' off the end 
                set @groupFirstNames = SUBSTRING(@groupFirstNames, 0, len(@groupFirstNames) - 1)
            end 
            set @personNames = @groupFirstNames + ' ' + @groupLastName;
        end
        else if (@adultLastNameCount = 0)
        begin
             -- no adults in family, list all members of the family in "Fullname & FullName & ..." format, order by oldest kid first
            select 
                @groupNonAdultFullNames = @groupNonAdultFullNames + [p].[FullName] + ' & '
            from 
                [GroupMember] [gm] 
            join 
                [Person] [p] 
            on 
                [p].[Id] = [gm].[PersonId] 
            where 
                [GroupId] = @groupId
            order by p.BirthYear, p.BirthMonth, p.BirthDay

            if len(@groupNonAdultFullNames) > 2 begin
                -- trim the extra ' &' off the end 
                set @groupNonAdultFullNames = SUBSTRING(@groupNonAdultFullNames, 0, len(@groupNonAdultFullNames) - 1)  
            end
            set @personNames = @groupNonAdultFullNames;
        end
        else
        begin
            -- multiple adult lastnames
            if len(@groupAdultFullNames) > 2 begin
              -- trim the extra ' &' off the end 
              set @groupAdultFullNames = SUBSTRING(@groupAdultFullNames, 0, len(@groupAdultFullNames) - 1)  
            end
            set @personNames = @groupAdultFullNames;
        end 
    end

    insert into @personNamesTable ( [PersonNames] ) values ( @personNames);

  return
END
GO
