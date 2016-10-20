/* CREATE the table using this template and helper sql to help construct the CREATE TABLE

Template:
CREATE TABLE [dbo].[Analytics_Dim_Person](
	[PersonKey] [int] IDENTITY(1,1) NOT NULL,
	[HistoryHash] varbinary(64) null,
	[PersonId] [int] NOT NULL,
	[FirstName] [nvarchar](50) NULL,
	[LastName] [nvarchar](50) NULL,
	##(copy/paste the results of *helper sql)##
	CONSTRAINT [PK_dbo.Analytics_Dim_Person] PRIMARY KEY CLUSTERED 
	(
		[PersonKey] ASC
	)
	)

-- *helper sql
SELECT CONCAT (
        '[attribute_'
        ,a.[Key]
        ,'] '
        ,CASE a.FieldTypeId
            WHEN 11
                THEN 'datetime null'
            ELSE 'nvarchar(max) null'
            END
        ,','
        )
FROM Attribute a
WHERE a.EntityTypeId = 15
ORDER BY a.[Key]
*/

/* ##Initial Populate##

use these helpers
-- FROM clauses
select 
concat('OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = ', Id, ') attribute_', [Id] )
from Attribute where EntityTypeId = 15

-- SELECT fields for attributes
select 
concat(',attribute_', Id, '.Value as [attribute_', [Key], ']')
from Attribute where EntityTypeId = 15
order by [Key]

Example:
insert into Analytics_Dim_Person 
SELECT 
	p.Id AS [PersonId]
	,null [HistoryHash]
    ,p.FirstName
	,p.LastName
    ,attribute_553.Value as [attribute_AbilityLevel]
,attribute_1274.Value as [attribute_AdaptiveC]
,attribute_1271.Value as [attribute_AdaptiveD]
,attribute_1272.Value as [attribute_AdaptiveI]
,attribute_1273.Value as [attribute_AdaptiveS]
,attribute_676.Value as [attribute_Allergy]
,attribute_1298.Value as [attribute_BackgroundCheckDate]
,attribute_1300.Value as [attribute_BackgroundCheckDocument]
,attribute_1297.Value as [attribute_BackgroundChecked]
,attribute_1299.Value as [attribute_BackgroundCheckResult]
,attribute_174.Value as [attribute_BaptismDate]
,attribute_714.Value as [attribute_BaptizedHere]
,attribute_1758.Value as [attribute_com.sparkdevnetwork.DLNumber]
,attribute_1783.Value as [attribute_core_CurrentlyAnEra]
,attribute_1785.Value as [attribute_core_EraEndDate]
,attribute_1786.Value as [attribute_core_EraFirstCheckin]
,attribute_1789.Value as [attribute_core_EraFirstGave]
,attribute_1787.Value as [attribute_core_EraLastCheckin]
,attribute_1788.Value as [attribute_core_EraLastGave]
,attribute_1784.Value as [attribute_core_EraStartDate]
,attribute_1791.Value as [attribute_core_EraTimesGiven52Wks]
,attribute_1792.Value as [attribute_core_EraTimesGiven6Wks]
,attribute_1790.Value as [attribute_core_TimesCheckedIn16Wks]
,attribute_740.Value as [attribute_Employer]
,attribute_969.Value as [attribute_Facebook]
,attribute_717.Value as [attribute_FirstVisit]
,attribute_971.Value as [attribute_Instagram]
,attribute_1781.Value as [attribute_LastDiscRequestDate]
,attribute_1279.Value as [attribute_LastSaveDate]
,attribute_715.Value as [attribute_LegalNotes]
,attribute_906.Value as [attribute_MembershipDate]
,attribute_1278.Value as [attribute_NaturalC]
,attribute_1275.Value as [attribute_NaturalD]
,attribute_1276.Value as [attribute_NaturalI]
,attribute_1277.Value as [attribute_NaturalS]
,attribute_1358.Value as [attribute_PersonalityType]
,attribute_741.Value as [attribute_Position]
,attribute_716.Value as [attribute_PreviousChurch]
,attribute_739.Value as [attribute_School]
,attribute_718.Value as [attribute_SecondVisit]
,attribute_719.Value as [attribute_SourceofVisit]
,attribute_970.Value as [attribute_Twitter]

FROM dbo.Person p
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 174
    ) attribute_174
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 553
    ) attribute_553
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 676
    ) attribute_676
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 714
    ) attribute_714
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 715
    ) attribute_715
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 716
    ) attribute_716
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 717
    ) attribute_717
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 718
    ) attribute_718
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 719
    ) attribute_719
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 739
    ) attribute_739
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 740
    ) attribute_740
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 741
    ) attribute_741
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 906
    ) attribute_906
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 969
    ) attribute_969
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 970
    ) attribute_970
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 971
    ) attribute_971
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 1271
    ) attribute_1271
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 1272
    ) attribute_1272
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 1273
    ) attribute_1273
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 1274
    ) attribute_1274
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 1275
    ) attribute_1275
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 1276
    ) attribute_1276
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 1277
    ) attribute_1277
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 1278
    ) attribute_1278
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 1279
    ) attribute_1279
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 1297
    ) attribute_1297
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 1298
    ) attribute_1298
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 1299
    ) attribute_1299
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 1300
    ) attribute_1300
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 1358
    ) attribute_1358
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 1758
    ) attribute_1758
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 1781
    ) attribute_1781
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 1783
    ) attribute_1783
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 1784
    ) attribute_1784
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 1785
    ) attribute_1785
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 1786
    ) attribute_1786
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 1787
    ) attribute_1787
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 1788
    ) attribute_1788
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 1789
    ) attribute_1789
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 1790
    ) attribute_1790
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 1791
    ) attribute_1791
OUTER APPLY (
    SELECT TOP 1 av.Value
    FROM dbo.AttributeValue av
    WHERE av.EntityId = p.Id
        AND AttributeId = 1792
    ) attribute_1792

*/ 


/* update the HistoryHash
update Analytics_Dim_Person set [HistoryHash] = CONVERT(varchar(max), HASHBYTES('SHA2_512', (select top 1 FirstName, LastName, attribute_AbilityLevel /* ... */ from Analytics_Dim_Person where PersonId = adp.PersonId for xml raw)), 2)
from Analytics_Dim_Person adp where [HistoryHash] is null
*/



