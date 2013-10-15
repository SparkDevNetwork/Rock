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
        'TODO: Address Person Names' [AddressPersonNames],
        [l].[Street1],
        [l].[Street2],
        [l].[City],
        [l].[State],
        [l].[Zip]
    from (
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
    ) [pg]
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
    

    

end
go


-- DEBUG
begin
  declare @accountIdList varchar(max) = '1,2,3';
  declare @startDate datetime = dateadd(day,-365, sysdatetime());
  declare @endDate datetime = sysdatetime();

  execute sp_get_contribution_person_group_address @startDate, @endDate, @accountIdList, null, 1

end;
