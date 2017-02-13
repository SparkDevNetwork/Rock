IF OBJECT_ID(N'[dbo].[AnalyticsDimPersonHistorical]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimPersonHistorical
GO

CREATE VIEW AnalyticsDimPersonHistorical
AS
SELECT asph.*
    ,ms.Value [MaritalStatus]
    ,cs.Value [ConnectionStatus]
    ,rr.Value [ReviewReason]
    ,rs.Value [RecordStatus]
    ,rsr.Value [RecordStatusReason]
    ,rt.Value [RecordType]
    ,ps.Value [Suffix]
    ,pt.Value [Title]
    ,CASE asph.Gender
        WHEN 1
            THEN 'Male'
        WHEN 2
            THEN 'Female'
        ELSE 'Unknown'
        END [GenderText]
    ,CASE asph.EmailPreference
        WHEN 0
            THEN 'Email Allowed'
        WHEN 1
            THEN 'No Mass Emails'
        WHEN 2
            THEN 'Do Not Email'
        ELSE 'Unknown'
        END [EmailPreferenceText]
	,fc.Id [PrimaryFamilyKey] 
FROM AnalyticsSourcePersonHistorical asph
LEFT JOIN DefinedValue ms ON ms.Id = asph.MaritalStatusValueId
LEFT JOIN DefinedValue cs ON cs.Id = asph.ConnectionStatusValueId
LEFT JOIN DefinedValue rr ON rr.Id = asph.ReviewReasonValueId
LEFT JOIN DefinedValue rs ON rs.Id = asph.RecordStatusValueId
LEFT JOIN DefinedValue rsr ON rsr.Id = asph.RecordStatusReasonValueId
LEFT JOIN DefinedValue rt ON rt.Id = asph.RecordTypeValueId
LEFT JOIN DefinedValue ps ON ps.Id = asph.SuffixValueId
LEFT JOIN DefinedValue pt ON pt.Id = asph.TitleValueId
left join AnalyticsDimFamilyCurrent fc on fc.FamilyId = asph.PrimaryFamilyId