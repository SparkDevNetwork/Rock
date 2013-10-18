-- drop the procedure
if exists (select * from sys.procedures where object_id = OBJECT_ID(N'[dbo].[sp_get_contribution_person_group_address]'))
    drop procedure [sp_get_contribution_person_group_address]
go

-- create procedure
create procedure [sp_get_contribution_person_group_address]
	@startDate datetime,
    @endDate datetime,
    @accountIds varchar(max), -- comma delimited list if integers. NULL means all
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
    order by
    case when @orderByZipCode = 1 then Zip end
    
end
go


-- DEBUG
begin
  declare @accountIdList varchar(max) = '1,2,3';
  declare @startDate datetime = dateadd(day,-365, sysdatetime());
  declare @endDate datetime = sysdatetime();

  execute sp_get_contribution_person_group_address @startDate, @endDate, @accountIdList, null, 1

end;
