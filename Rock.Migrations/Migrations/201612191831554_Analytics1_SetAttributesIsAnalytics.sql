UPDATE Attribute
SET IsAnalytic = 1
WHERE [Key] IN (
        'BackgroundCheckDate'
        ,'BackgroundChecked'
        ,'BaptismDate'
        ,'core_CurrentlyAnEra'
        ,'core_EraStartDate'
        ,'core_EraFirstGave'
        ,'core_EraLastGave'
        ,'core_EraTimesGiven52Wks'
        ,'core_EraTimesGiven6Wks'
        ,'core_TimesCheckedIn16Wks'
        ,'Employer'
        ,'FirstVisit'
        ,'SecondVisit'
        )
    AND EntityTypeId IN (
        SELECT Id
        FROM EntityType
        WHERE NAME = 'Rock.Model.Person'
        )

UPDATE Attribute
SET IsAnalyticHistory = 1
WHERE [Key] IN ('core_CurrentlyAnEra')
    AND EntityTypeId IN (
        SELECT Id
        FROM EntityType
        WHERE NAME = 'Rock.Model.Person'
        )
