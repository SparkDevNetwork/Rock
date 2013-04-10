begin transaction

declare @FundEntityTypeId int
declare @TypeId int
declare @ValueId int
declare @FundId int
declare @MissionsFundId int
declare @BuildingFundId int
declare @PledgyPersonId int
declare @DoraPersonId int
declare @PledgeId int

-- Fund Attributes
if not exists( select Id from EntityType where Name = 'Rock.Model.FinancialAccount' )
begin
	insert into EntityType ( Name, [Guid], IsEntity, IsSecured )
	values ( 'Rock.Model.FinancialAccount', newid(), 0, 0 )
end
set @FundEntityTypeId = ( select Id from EntityType where Name = 'Rock.Model.FinancialAccount' )

-- Defined Types & Defined Values
if not exists ( select Id from DefinedType where [Guid] = '2E6540EA-63F0-40FE-BE50-F2A84735E600' )
begin
	insert into DefinedType ( IsSystem, FieldTypeId, [Order], Category, Name, [Description], [Guid] )
	values ( 1, 1, 0, 'Person', 'Status', 'Status', '2E6540EA-63F0-40FE-BE50-F2A84735E600' )
end
set @TypeId = ( select Id from DefinedType where [Guid] = '2E6540EA-63F0-40FE-BE50-F2A84735E600' )

insert into DefinedValue ( IsSystem, DefinedTypeid, [Order], Name, [Description], [Guid] )
values ( 1, @TypeId, 0, 'Active', '', newid() )

insert into DefinedValue ( IsSystem, DefinedTypeid, [Order], Name, [Description], [Guid] )
values ( 1, @TypeId, 0, 'Inactive', '', newid() )

insert into DefinedValue ( IsSystem, DefinedTypeid, [Order], Name, [Description], [Guid] )
values ( 1, @TypeId, 0, 'Web Prospect', '', newid() )

insert into DefinedValue ( IsSystem, DefinedTypeid, [Order], Name, [Description], [Guid] )
values ( 1, @TypeId, 0, 'Unknown', '', newid() )

if not exists ( select Id from DefinedType where [Guid] = '059F69C0-BF9B-4D53-B7CD-2D3B7C647C5F' )
begin
	insert into DefinedType ( IsSystem, FieldTypeId, [Order], Category, Name, [Description], [Guid] )
	values ( 1, 1, 0, 'Financial', 'Frequency Type', 'Types of payment frequencies', '059F69C0-BF9B-4D53-B7CD-2D3B7C647C5F' )
end
set @TypeId = ( select Id from DefinedType where [Guid] = '059F69C0-BF9B-4D53-B7CD-2D3B7C647C5F' )

if not exists ( select Id from DefinedValue where [Guid] = 'C53509B1-FC2B-46C8-A00E-58392FBE9408' )
begin
	insert into DefinedValue ( IsSystem, DefinedTypeid, [Order], Name, [Description], [Guid] )
	values ( 1, @TypeId, 0, 'Monthly', 'Every Monthly', 'C53509B1-FC2B-46C8-A00E-58392FBE9408' )
end
set @ValueId = ( select Id from DefinedValue where [Guid] = 'C53509B1-FC2B-46C8-A00E-58392FBE9408' )

-- Funds
-- Create "General Fund" 
insert into FinancialAccount ( Name, PublicName, [Description], ParentAccountId, IsTaxDeductible, [Order], IsActive, StartDate, EndDate, GlCode, [Guid] )
values ( 'general', 'General Fund', 'This is the general fund', null, 1, 0, 1, dateadd(day, -90, getdate()), dateadd(day, 90, getdate()), 0, newid() )

-- "Building Campaign" Fund that also doesn't take pledges
insert into FinancialAccount ( Name, PublicName, [Description], ParentAccountId, IsTaxDeductible, [Order], IsActive, StartDate, EndDate, GlCode, [Guid] )
values ( 'buildings', 'Building Campaigns Fund', 'This a fund for building campaigns', null, 1, 1, 1, dateadd(day, -90, getdate()), dateadd(day, 90, getdate()), 0, newid())
set @FundId = scope_identity()

-- Campus-specific "Building Campaign", child of "Building Campaign"
insert into FinancialAccount ( Name, PublicName, [Description], ParentAccountId, IsTaxDeductible, [Order], IsActive, StartDate, EndDate, GlCode, [Guid] )
values ( 'gilbertbuildings', 'Gilbert Building Campaign Fund', 'This is the Gilbert Building Campaign fund', @FundId, 1, 0, 1, dateadd(day, -90, getdate()), dateadd(day, 90, getdate()), 1, newid() )
set @BuildingFundId = scope_identity()

-- Missions Fund 
insert into FinancialAccount ( Name, PublicName, [Description], ParentAccountId, IsTaxDeductible, [Order], IsActive, StartDate, EndDate, GlCode, [Guid] )
values ( 'missions', 'Missions Fund', 'This is the Missions fund', null, 1, 2, 1, dateadd(day, -90, getdate()), dateadd(day, 90, getdate()), 1, newid() )
set @MissionsFundId = scope_identity()

-- Create a couple of people to attach pledges to
insert into Person ( IsSystem, GivenName, NickName, LastName, PhotoId, BirthDay, BirthMonth, BirthYear, Gender, AnniversaryDate, GraduationDate, Email, IsEmailActive, EmailNote, DoNotEmail, SystemNote, ViewedCount, [Guid], RecordTypeValueId, RecordStatusValueId, RecordStatusReasonValueId, PersonStatusValueId, SuffixValueId, TitleValueId, MaritalStatusValueId, IsDeceased )
values (  0, 'Pledgy', 'The Pledgester', 'McPledgerson', null, 1, 1, 1980, 0, null, null, 'pledgy@thepledgester.com', 1, 'Personal Email', 0, null, null, newid(), 1, 3, null, null, null, null, null, 0 )
set @PledgyPersonId = scope_identity()

insert into Person ( IsSystem, GivenName, NickName, LastName, PhotoId, BirthDay, BirthMonth, BirthYear, Gender, AnniversaryDate, GraduationDate, Email, IsEmailActive, EmailNote, DoNotEmail, SystemNote, ViewedCount, [Guid], RecordTypeValueId, RecordStatusValueId, RecordStatusReasonValueId, PersonStatusValueId, SuffixValueId, TitleValueId, MaritalStatusValueId, IsDeceased )
values (  0, 'Dora', null, 'Donatesalot', null, 1, 1, 1980, 0, null, null, 'dora@thepledgester.com', 1, 'Personal Email', 0, null, null, newid(), 1, 3, null, null, null, null, null, 0 )
set @DoraPersonId = scope_identity()

-- Add a couple pledges to each person
insert into FinancialPledge ( PersonId, AccountId, TotalAmount, StartDate, EndDate, [Guid], PledgeFrequencyValueId )
values ( @PledgyPersonId, @BuildingFundId, 2000.00, dateadd(day, -30, getdate()), dateadd(day, 30, getdate()), newid(), @ValueId )

insert into FinancialPledge ( PersonId, AccountId, TotalAmount, StartDate, EndDate, [Guid], PledgeFrequencyValueId )
values ( @PledgyPersonId, @MissionsFundId, 5000.00, dateadd(day, -30, getdate()), dateadd(day, 30, getdate()), newid(), @ValueId )

insert into FinancialPledge ( PersonId, AccountId, TotalAmount, StartDate, EndDate, [Guid], PledgeFrequencyValueId )
values ( @DoraPersonId, @BuildingFundId, 2000.00, dateadd(day, -30, getdate()), dateadd(day, 30, getdate()), newid(), @ValueId )

insert into FinancialPledge ( PersonId, AccountId, TotalAmount, StartDate, EndDate, [Guid], PledgeFrequencyValueId )
values ( @DoraPersonId, @MissionsFundId, 5000.00, dateadd(day, -30, getdate()), dateadd(day, 30, getdate()), newid(), @ValueId )

commit transaction