-- Fund Attributes
IF NOT EXISTS(SELECT Id FROM EntityType WHERE Name = 'Rock.Model.Fund')
INSERT INTO EntityType (Name, Guid, IsEntity, IsSecured)
VALUES ('Rock.Model.Fund', NEWID(), 0, 0)
DECLARE @FundEntityTypeId int
SET @FundEntityTypeId = (SELECT Id FROM EntityType WHERE Name = 'Rock.Model.Fund')

-- Funds
declare @FundId int

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
set @FundId = scope_identity()

-- Person


-- Pledges
declare @PledgeId int

