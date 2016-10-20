
-- Migrations

-- Create table for Adult Totals 
CREATE TABLE [dbo]._church_ccv_Actions_History_Adult( 
 [Id] int IDENTITY(1,1) NOT NULL
,[Date] date
,[CampusId] int
,[RegionId] int
,[TotalPeople] int
,[Baptised] int 
,[ERA] int
,[Give] int
,[Member] int
,[Serving] int
,[PeerLearning] int
,[Mentored] int
,[Teaching] int
,[PeerLearning_NH] int
,[PeerLearning_YA] int
,[Mentored_NH] int
,[Mentored_YA] int
,[Mentored_NS] int
,[Teaching_NH_SubSection] int
,[Teaching_NH] int
,[Teaching_YA_Section] int
,[Teaching_YA] int
,[Teaching_NS_SubSection] int 
,[Teaching_NS] int
,[Teaching_NG_Section] int
,[Teaching_NG] int
,CreatedDateTime datetime
,ModifiedDateTime datetime
,CreatedByPersonAliasId datetime
,ModifiedByPersonAliasId datetime
,[Guid] uniqueidentifier NOT NULL
,ForeignId int
,ForeignGuid uniqueidentifier
,ForeignKey varchar(100)
    CONSTRAINT [PK_dbo._church_ccv_ActionsHistoryAdult] PRIMARY KEY CLUSTERED 
    (
	    [Id] ASC
    )
)

-- Create table for Adult Individuals
CREATE TABLE [dbo]._church_ccv_Actions_History_Adult_Person( 
  [Id] int IDENTITY(1,1) NOT NULL,
  [Date] date NOT NULL, 
  PersonAliasId int NOT NULL, 
  Baptised date, 
  ERA bit NOT NULL, 
  Give bit NOT NULL, 
  Member date, 
  Serving bit NOT NULL, 
  PeerLearning bit NOT NULL, 
  Mentored bit NOT NULL, 
  Teaching bit NOT NULL, 

  PeerLearning_NH bit NOT NULL, 
  PeerLearning_YA bit NOT NULL, 
  Mentored_NH bit NOT NULL,
  Mentored_YA bit NOT NULL,
  Mentored_NS bit NOT NULL,
  Teaching_NH_SubSection bit NOT NULL,
  Teaching_NH bit NOT NULL,
  Teaching_YA_Section bit NOT NULL,
  Teaching_YA bit NOT NULL,
  Teaching_NS_SubSection bit NOT NULL,
  Teaching_NS bit NOT NULL,
  Teaching_NG_Section bit NOT NULL,
  Teaching_NG bit NOT NULL,
  
  CreatedDateTime datetime,
  ModifiedDateTime datetime,
  CreatedByPersonAliasId datetime,
  ModifiedByPersonAliasId datetime,
  [Guid] uniqueidentifier NOT NULL,
  ForeignId int,
  ForeignGuid uniqueidentifier,
  ForeignKey varchar(100)
  CONSTRAINT [PK_dbo._church_ccv_ActionsHistoryAdultPerson] PRIMARY KEY CLUSTERED 
  (
	[Id] ASC
  )
)



-- Create table for Student Totals
CREATE TABLE [dbo]._church_ccv_Actions_History_Student( 
 [Id] int IDENTITY(1,1) NOT NULL
,[Date] date
,[CampusId] int
,[RegionId] int
,[TotalPeople] int
,[Baptised] int 
,[ERA] int
,[Give] int
,[Member] int
,[Serving] int
,[PeerLearning] int
,[Mentored] int
,[Teaching] int
,[PeerLearning_NG] int
,[Mentored_NG] int
,[Teaching_Undefined] int
,CreatedDateTime datetime
,ModifiedDateTime datetime
,CreatedByPersonAliasId datetime
,ModifiedByPersonAliasId datetime
,[Guid] uniqueidentifier NOT NULL
,ForeignId int
,ForeignGuid uniqueidentifier
,ForeignKey varchar(100)
	CONSTRAINT [PK_dbo._church_ccv_ActionsHistoryStudent] PRIMARY KEY CLUSTERED 
    (
	    [Id] ASC
    )
)

-- Create table for Student Individuals
  CREATE TABLE [dbo]._church_ccv_Actions_History_Student_Person( 
  [Id] int IDENTITY(1,1) NOT NULL,
  [Date] date NOT NULL, 
  PersonAliasId int NOT NULL, 

  Baptised date, 
  ERA bit NOT NULL, 
  Give bit NOT NULL, 
  Member date, 
  Serving bit NOT NULL, 
  PeerLearning bit NOT NULL, 
  Mentored bit NOT NULL, 
  Teaching bit NOT NULL, 

  PeerLearning_NG bit NOT NULL, 
  Mentored_NG bit NOT NULL,
  Teaching_Undefined bit NOT NULL,
  
  CreatedDateTime datetime,
  ModifiedDateTime datetime,
  CreatedByPersonAliasId datetime,
  ModifiedByPersonAliasId datetime,
  [Guid] uniqueidentifier NOT NULL,
  ForeignId int,
  ForeignGuid uniqueidentifier,
  ForeignKey varchar(100)
  CONSTRAINT [PK_dbo._church_ccv_ActionsHistoryStudentPerson] PRIMARY KEY CLUSTERED 
    (
	    [Id] ASC
    )
)