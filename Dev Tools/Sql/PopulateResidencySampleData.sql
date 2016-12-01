delete from [_com_ccvonline_Residency_CompetencyPersonProjectAssessmentPointOfAssessment]
delete from [_com_ccvonline_Residency_CompetencyPersonProjectAssessment]
delete from [_com_ccvonline_Residency_CompetencyPersonProject]
delete from [_com_ccvonline_Residency_CompetencyPerson]
delete from [_com_ccvonline_Residency_ProjectPointOfAssessment]
delete from [_com_ccvonline_Residency_Project]
delete from [_com_ccvonline_Residency_Competency]
delete from [_com_ccvonline_Residency_Track]
delete from [_com_ccvonline_Residency_Period]

INSERT INTO [_com_ccvonline_Residency_Period] ([StartDate] ,[EndDate] ,[Name] ,[Description] ,[Guid]) VALUES 
  ('2013-08-01','2014-05-01','Fall 2013/Spring 2014','First year of Residency Program at CCV',newid()),
  ('2014-08-01','2015-05-01','Fall 2014/Spring 2015','Second year of Residency Program at CCV',newid()),
  ('2015-08-01','2016-05-01','Fall 2015/Spring 2016','Third year of Residency Program at CCV',newid()),
  (null,null,'TBD','Some Period yet to be determind',newid())

INSERT INTO [_com_ccvonline_Residency_Track] ([Name], [Description], [DisplayOrder], [PeriodId], [Guid]) 
  select 'General', 'Required', 0, [Id], newid() from _com_ccvonline_Residency_Period
  union
  select 'Children and Family', '', 1, [Id], newid() from _com_ccvonline_Residency_Period
  union
  select 'Church Administration', '', 2, [Id], newid() from _com_ccvonline_Residency_Period
  union  
  select 'Church Planting', '', 3, [Id], newid() from _com_ccvonline_Residency_Period
  union
  select 'Inter-Cultural Studies', '', 4, [Id], newid() from _com_ccvonline_Residency_Period
  union
  select 'Pastoral Ministry', '', 5, [Id], newid() from _com_ccvonline_Residency_Period

insert into [_com_ccvonline_Residency_Competency] ([Name], [TrackId], [Guid]) 
  select 'Events and Projects Management', [Id], newid() from _com_ccvonline_Residency_Track where Name = 'General'
  union
  select 'First Impressions', [Id], newid() from _com_ccvonline_Residency_Track where Name = 'General'
  union
  select 'Leadership Practices 500', [Id], newid() from _com_ccvonline_Residency_Track where Name = 'General'
  union
  select 'Leadership Practices 600', [Id], newid() from _com_ccvonline_Residency_Track where Name = 'General'
  union
  select 'Life of Christ (Israel Trip)', [Id], newid() from _com_ccvonline_Residency_Track where Name = 'General'

insert into [_com_ccvonline_Residency_Competency] ([Name], [TrackId], [Guid]) 
  select 'Applied Homiletics', [Id], newid() from _com_ccvonline_Residency_Track where Name = 'Pastoral Ministry'
  union
  select 'Neighborhood Ministry', [Id], newid() from _com_ccvonline_Residency_Track where Name = 'Pastoral Ministry'
  union
  select 'Practical Ministry', [Id], newid() from _com_ccvonline_Residency_Track where Name = 'Pastoral Ministry'

insert into [_com_ccvonline_Residency_Project] ([Name], [Description], [CompetencyId], [Guid])
    select 'Project A', 'Participate in 3 initial scope sessions for planning an event or project.', [Id], NEWID() from [_com_ccvonline_Residency_Competency] where Name = 'Events and Projects Management'
    union
    select 'Project B', 'Develop 3 event/project budgets.', [Id], NEWID() from [_com_ccvonline_Residency_Competency] where Name = 'Events and Projects Management'
    union
    select 'Project C', 'Identify resources and lead times needed to execute 3 events/projects.', [Id], NEWID() from [_com_ccvonline_Residency_Competency] where Name = 'Events and Projects Management'
    union
    select 'Project D', 'Attend 3 ministry support coordination meetings.', [Id], NEWID() from [_com_ccvonline_Residency_Competency] where Name = 'Events and Projects Management'
    union
    select 'Project E', 'Recruit volunteers to support 3 events/projects.', [Id], NEWID() from [_com_ccvonline_Residency_Competency] where Name = 'Events and Projects Management'
    union
    select 'Project F', 'Evaluate with planning team a post-mortem of each event/project.', [Id], NEWID() from [_com_ccvonline_Residency_Competency] where Name = 'Events and Projects Management'
    union
    select 'Project G', 'Document activities based upon observations of 3 events/projects.', [Id], NEWID() from [_com_ccvonline_Residency_Competency] where Name = 'Events and Projects Management'

insert into [_com_ccvonline_Residency_ProjectPointOfAssessment] ([ProjectId], [AssessmentOrder], [AssessmentText], [IsPassFail], [Guid])
    select [Id], 1,	'Received, responded to, and provided appropriate communication while engaging leaders prior to the initial session, ensuring the resident knew what the event/project was in advance.', 0, NEWID() from [_com_ccvonline_Residency_Project] where [Name] = 'Project A'
    union
    select [Id], 2,	'Thoroughly previewed all elements of the previous event (if reoccurring) and was prepared to participate with clarity and accuracy.', 0, NEWID() from [_com_ccvonline_Residency_Project] where [Name] = 'Project A'
    union
    select [Id], 3,	'On time (5 minutes early) to assigned ministry area and designated location.', 0, NEWID() from [_com_ccvonline_Residency_Project] where [Name] = 'Project A'
    union
    select [Id], 4,	'Displayed appropriate demeanor, respect, diction, decorum, and positive attitude with all participants throughout entire session.', 0, NEWID() from [_com_ccvonline_Residency_Project] where [Name] = 'Project A'
    union
    select [Id], 5,	'Identified two ideas that were good, but not implemented, and the reasons why they were not.', 0, NEWID() from [_com_ccvonline_Residency_Project] where [Name] = 'Project A'
    union
    select [Id], 6,	'Correctly analyzed and appropriated the different types of staff involved needed to plan and influence the event/project.', 0, NEWID() from [_com_ccvonline_Residency_Project] where [Name] = 'Project A'
    union
    select [Id], 7,	'Accurately determined what goals and results are trying to be achieved by the team planning the event/project and be able to articulate how they are being measured on a given project.', 1, NEWID() from [_com_ccvonline_Residency_Project] where [Name] = 'Project A'
    union
    select [Id], 8,	'Effectively assessed and articulated how timelines and milestones are used in the initial scope meeting and evaluated if they were met.', 0, NEWID() from [_com_ccvonline_Residency_Project] where [Name] = 'Project A'
    union
    select [Id], 9,	'Aware of others and their surroundings to promote an environment for a full discussion on the event/project and went out of their way to do more than what was expected of them.', 1, NEWID() from [_com_ccvonline_Residency_Project] where [Name] = 'Project A'
    union
    select [Id], 10, 'Accepted critique and criticism with gratitude and humility and implemented suggestions appropriately.', 0, NEWID() from [_com_ccvonline_Residency_Project] where [Name] = 'Project A'

declare
  @groupTypeId int

select  @groupTypeId = [Id] from dbo.GroupType where Guid = '00043CE6-EB1B-43B5-A12A-4552B91A3E28';

delete from [dbo].[Group] where [Guid] = '4B7D22E8-B08C-42DC-B1F1-F2834BC8D1DF';

INSERT INTO [dbo].[Group] ([IsSystem],[ParentGroupId],[GroupTypeId],[CampusId],[Name],[Description],[IsSecurityRole],[IsActive],[Order],[Guid])
                            VALUES (0,null,@groupTypeId,null,'Residents - Fall 2013 to Spring 2014','Residents in the Residency program for the Fall 2013 to Spring 2014 period.',0,1,0,'4B7D22E8-B08C-42DC-B1F1-F2834BC8D1DF');
