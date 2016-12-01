/*select 
    foreign_key2 [RockChMS.PersonId],     
    foreign_key [RockRMS.PersonId], 
    first_name, 
    last_name 
from Arena.dbo.core_person where foreign_key2 in (select distinct PersonId from RockChMS.dbo._com_ccvonline_Residency_CompetencyPerson)
order by [RockChMS.PersonId]
*/


-- [_com_ccvonline_Residency_CompetencyPerson]
select 
    concat('INSERT INTO [dbo].[_com_ccvonline_Residency_CompetencyPerson] ([Id], [CompetencyId], [PersonId], [Guid]) VALUES (', [cp].Id, ',',   [cp].CompetencyId, ',',    ap.foreign_key, ', N''',    [cp].[Guid], ''')')
    from RockChMS.dbo._com_ccvonline_Residency_CompetencyPerson [cp]
join Arena.dbo.core_person ap on ap.foreign_key2 = cp.PersonId
order by cp.Id

-- [_com_ccvonline_Residency_Competency]
select 
    concat('INSERT INTO [dbo].[_com_ccvonline_Residency_Competency] ([Id], [TrackId], [TeacherOfRecordPersonId], [FacilitatorPersonId], [Goals], [CreditHours], [SupervisionHours], [ImplementationHours], [Name], [Description], [Guid]) VALUES ('
     , [r].Id, ',', r.TrackId, ',', isnull(cast(apt.foreign_key as nvarchar), 'null'), ',', isnull(cast(apf.foreign_key as nvarchar), 'null'), ', N''', r.Goals, ''',', r.CreditHours, ',', r.SupervisionHours, ',', r.ImplementationHours, ',''', r.Name, ''',''', r.[Description], ''', N''',    [r].[Guid], ''')')
    from RockChMS.dbo._com_ccvonline_Residency_Competency [r]
left join Arena.dbo.core_person apf on apf.foreign_key2 = r.FacilitatorPersonId
left join Arena.dbo.core_person apt on apt.foreign_key2 = r.TeacherOfRecordPersonId
order by r.Id

-- [_com_ccvonline_Residency_CompetencyPersonProjectAssessment]
select 
    concat('INSERT INTO [dbo].[_com_ccvonline_Residency_CompetencyPersonProjectAssessment] ([Id], [CompetencyPersonProjectId], [AssessorPersonId], [AssessmentDateTime], [OverallRating], [RatingNotes], [ResidentComments], [Guid]) VALUES ('
     , [r].Id, ',', r.CompetencyPersonProjectId, ',', isnull(cast(ap.foreign_key as nvarchar), 'null'), ',''', r.AssessmentDateTime, ''',', r.OverallRating, ',''', replace(r.RatingNotes, '''', '''''') , ''',''', replace(r.ResidentComments, '''', ''''''), ''', N''',    [r].[Guid], ''')')
    from RockChMS.dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessment [r]
left join Arena.dbo.core_person ap on ap.foreign_key2 = r.AssessorPersonId
order by r.Id