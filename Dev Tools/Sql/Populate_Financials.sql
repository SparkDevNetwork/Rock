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
if not exists(select Id from EntityType where Name = 'Rock.Model.Fund')
begin
	insert into EntityType (Name, [Guid], IsEntity, IsSecured)
	values ('Rock.Model.Fund', newid(), 0, 0)
end
set @FundEntityTypeId = (select Id from EntityType where Name = 'Rock.Model.Fund')

-- Defined Types
-- TODO: This DefinedType will probably need to move out of this script and be included in a Migration
insert into DefinedType ( IsSystem, FieldTypeId [Order], Category, Name, [Description], [Guid] )
values ( 1, 1, 0, 'Financial', 'Frequency Type', 'Types of payment frequencies', newid() )
set @TypeId = scope_identity()

-- Defined Values
-- TODO: These DefinedValues will probably need to move out of this script and be included in a Migration
insert into DefinedValue ( IsSystem, DefinedTypeid, [Order], Name, [Description], [Guid] )
values ( 1, @TypeId, 0, 'Weekly', 'Weekly', newid() )

insert into DefinedValue ( IsSystem, DefinedTypeid, [Order], Name, [Description], [Guid] )
values ( 1, @TypeId, 0, 'Bi-Weekly', 'Bi-Weekly', newid() )

insert into DefinedValue ( IsSystem, DefinedTypeid, [Order], Name, [Description], [Guid] )
values ( 1, @TypeId, 0, 'Monthly', 'Monthly', newid() )
set @ValueId = scope_identity()

-- Funds
-- Create "General Fund" that doesn't take pledges
insert into Fund ( Name, PublicName, [Description], ParentFundId, IsTaxDeductible, [Order], IsActive, StartDate, EndDate, IsPledgable, GlCode,	Entity,	EntityId, [Guid], FundTypeValueId )
values ( 'general', 'General Fund', 'This is the general fund', null, 1, 1, 1, dateadd(day, -90, getdate()), dateadd(day, 90, getdate()), 0, '1234', 'Rock.Model.Fund', @FundEntityTypeId, newid(), null )

-- "Building Campaign" Fund that also doesn't take pledges
insert into Fund ( Name, PublicName, [Description], ParentFundId, IsTaxDeductible, [Order], IsActive, StartDate, EndDate, IsPledgable, GlCode,	Entity,	EntityId, [Guid], FundTypeValueId )
values ( 'buildings', 'Building Campaigns Fund', 'This a fund for building campaigns', null, 1, 1, 1, dateadd(day, -90, getdate()), dateadd(day, 90, getdate()), 0, '5678', 'Rock.Model.Fund', @FundEntityTypeId, newid(), null )
set @FundId = scope_identity()

-- Campus-specific "Building Campaign" that allows pledges, child of "Building Campaign"
insert into Fund ( Name, PublicName, [Description], ParentFundId, IsTaxDeductible, [Order], IsActive, StartDate, EndDate, IsPledgable, GlCode,	Entity,	EntityId, [Guid], FundTypeValueId )
values ( 'gilbertbuildings', 'Gilbert Building Campaign Fund', 'This is the Gilbert Building Campaign fund', @FundId, 1, 1, 1, dateadd(day, -90, getdate()), dateadd(day, 90, getdate()), 1, '9101112', 'Rock.Model.Fund', @FundEntityTypeId, newid(), null )
set @BuildingFundId = scope_identity()

-- Missions Fund that takes pledges
insert into Fund ( Name, PublicName, [Description], ParentFundId, IsTaxDeductible, [Order], IsActive, StartDate, EndDate, IsPledgable, GlCode,	Entity,	EntityId, [Guid], FundTypeValueId )
values ( 'missions', 'Missions Fund', 'This is the Missions fund', null, 1, 1, 1, dateadd(day, -90, getdate()), dateadd(day, 90, getdate()), 1, '13141516', 'Rock.Model.Fund', @FundEntityTypeId, newid(), null )
set @MissionsFundId = scope_identity()

-- Person
insert into Person ( IsSystem, GivenName, NickName, LastName, PhotoId, BirthDay, BirthMonth, BirthYear, Gender, AnniversaryDate, GraduationDate, Email, IsEmailActive, EmailNote, ViewedCount, [Guid], RecordTypeValueId, RecordStatusValueId, RecordStatusReasonId, PersonStatusValueId, SuffixValueId, TitleValueId, MaritalStatusValueId, IsDeceased )
values (  0, 'Pledgy', 'The Pledgester', 'McPledgerson', null, 1, 1, 1980, 0, null, null, 'pledgy@thepledgester.com', 1, 'Personal Email', null, newid(), 1, 3, null, null, null, null, null, 0 )
set @PledgyPersonId = scope_identity()

insert into Person ( IsSystem, GivenName, NickName, LastName, PhotoId, BirthDay, BirthMonth, BirthYear, Gender, AnniversaryDate, GraduationDate, Email, IsEmailActive, EmailNote, ViewedCount, [Guid], RecordTypeValueId, RecordStatusValueId, RecordStatusReasonId, PersonStatusValueId, SuffixValueId, TitleValueId, MaritalStatusValueId, IsDeceased )
values (  0, 'Dora', null, 'Donatesalot', null, 1, 1, 1980, 0, null, null, 'dora@thepledgester.com', 1, 'Personal Email', null, newid(), 1, 3, null, null, null, null, null, 0 )
set @DoraPersonId = scope_identity()

-- Pledges
insert into Pledge ( PersonId, FundId, Amount, StartDate, EndDate, FrequencyAmount, [Guid], FrequencyTypeValueId )
values ( @PledgyPersonId, @BuildingFundId, 2000.00, dateadd(day, -30, getdate()), dateadd(day, 30, getdate()), 100.00, newid(), @ValueId )

insert into Pledge ( PersonId, FundId, Amount, StartDate, EndDate, FrequencyAmount, [Guid], FrequencyTypeValueId )
values ( @PledgyPersonId, @MissionsFundId, 5000.00, dateadd(day, -30, getdate()), dateadd(day, 30, getdate()), 50.00, newid(), @ValueId )

insert into Pledge ( PersonId, FundId, Amount, StartDate, EndDate, FrequencyAmount, [Guid], FrequencyTypeValueId )
values ( @DoraPersonId, @BuildingFundId, 2000.00, dateadd(day, -30, getdate()), dateadd(day, 30, getdate()), 100.00, newid(), @ValueId )

insert into Pledge ( PersonId, FundId, Amount, StartDate, EndDate, FrequencyAmount, [Guid], FrequencyTypeValueId )
values ( @DoraPersonId, @MissionsFundId, 5000.00, dateadd(day, -30, getdate()), dateadd(day, 30, getdate()), 50.00, newid(), @ValueId )