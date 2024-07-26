// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20240709 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddCalendarBlockAttributesUp();
            AddNewLavaFilterToVolunteerGenerosityBuildScriptUp();
            UpdateAppleDevicesDefinedType();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddCalendarBlockAttributesDown();
        }

        ///<summary>
        /// JDR: Adding the AvatarUrl Lava filter to consider the new security setting when getting Person Avatars from the GetAvatar.ashx endpoint
        /// </summary>
        private void AddNewLavaFilterToVolunteerGenerosityBuildScriptUp()
        {
            string newBuildScript = @"//- Retrieve the base URL for linking photos from a global attribute 
{% assign publicApplicationRoot = 'Global' | Attribute:'PublicApplicationRoot' %}

{% sql %}
DECLARE @NumberOfDays INT = 365;
DECLARE @NumberOfMonths INT = 13;
DECLARE @ServingAreaDefinedValueGuid UNIQUEIDENTIFIER = '36a554ce-7815-41b9-a435-93f3d52a2828';
DECLARE @ActiveRecordStatusValueId INT = (SELECT Id FROM DefinedValue WHERE Guid = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E');
DECLARE @ConnectionStatusDefinedTypeId INT = (SELECT Id FROM DefinedType WHERE [Guid] = '2e6540ea-63f0-40fe-be50-f2a84735e600');
DECLARE @StartDateKey INT = (SELECT TOP 1 [DateKey] FROM [AnalyticsSourceDate] WHERE [Date] = CAST(DATEADD(DAY, -@NumberOfDays, GETDATE()) AS DATE));
DECLARE @CurrentMonth DATE = DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0);
DECLARE @StartingDateKeyForGiving INT = (SELECT [DateKey] FROM [AnalyticsSourceDate] WHERE [Date] = DATEADD(MONTH, -@NumberOfMonths, @CurrentMonth));

;WITH CTE_Giving AS (
    SELECT
        p.[GivingId],
        asd.[CalendarYear],
        asd.[CalendarMonth],
        POWER(2, asd.[CalendarMonth] - 1) AS DonationMonthBitmask,
        SUM(ftd.[Amount]) AS TotalAmount
    FROM
        [Person] p
        INNER JOIN [PersonAlias] pa ON pa.[PersonId] = p.[Id]
        INNER JOIN [FinancialTransaction] ft ON ft.[AuthorizedPersonAliasId] = pa.[Id]
        INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
        INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.[AccountId]
        INNER JOIN [AnalyticsSourceDate] asd ON asd.[DateKey] = ft.[TransactionDateKey]
    WHERE
        fa.[IsTaxDeductible] = 1 AND ft.[TransactionDateKey] >= @StartingDateKeyForGiving
    GROUP BY
        p.[GivingId], asd.[CalendarYear], asd.[CalendarMonth]
    HAVING
        SUM(ftd.[Amount]) > 0
),
CTE_GivingAggregated AS (
    SELECT
        GD.[GivingId],
        GD.[CalendarYear],
        SUM(GD.[DonationMonthBitmask]) AS DonationMonthBitmask
    FROM
        CTE_Giving GD
    GROUP BY
        GD.[GivingId], GD.[CalendarYear]
),
CTE_CampusShortCode AS (
    SELECT
        g.[Id] AS GroupId,
        CASE WHEN c.[ShortCode] IS NOT NULL AND c.[ShortCode] != '' THEN c.[ShortCode] ELSE c.[Name] END AS CampusShortCode
    FROM
        [Group] g
        LEFT JOIN [Campus] c ON c.[Id] = g.[CampusId]
),
CTE_GivingResult AS (
    SELECT
        GDA.[GivingId],
        STUFF((
            SELECT '|' + CAST(GDA2.[CalendarYear] AS VARCHAR(4)) + '-' + RIGHT('000' + CAST(GDA2.[DonationMonthBitmask] AS VARCHAR(11)), 11)
            FROM CTE_GivingAggregated GDA2
            WHERE GDA2.[GivingId] = GDA.[GivingId]
            FOR XML PATH('')
        ), 1, 1, '') AS DonationMonthYearBitmask
    FROM
        CTE_GivingAggregated GDA
    GROUP BY
        GDA.[GivingId]
)
SELECT DISTINCT
    p.[Id] AS PersonId,
    CONCAT(CAST(p.[Id] AS NVARCHAR(12)), '-', CAST(g.[Id] AS NVARCHAR(12))) AS PersonGroupKey,
    p.[LastName],
    p.[NickName],
    p.[PhotoId],
    p.[GivingId],
    g.[Id] AS GroupId,
    g.[Name] AS GroupName,
    csc.CampusShortCode,
    MAX(ao.[OccurrenceDate]) AS LastAttendanceDate,
    dvcs.[Value] AS ConnectionStatus,
    CAST(CASE WHEN p.[RecordStatusValueId] = @ActiveRecordStatusValueId AND gm.[IsArchived] = 0 THEN 1 ELSE 0 END AS BIT) AS IsActive,
    GR.DonationMonthYearBitmask
FROM
    [Person] p
    INNER JOIN [PersonAlias] pa ON pa.[PersonId] = p.[Id]
    INNER JOIN [Attendance] a ON a.[PersonAliasId] = pa.[Id]
    INNER JOIN [AttendanceOccurrence] ao ON ao.[Id] = a.[OccurrenceId]
    INNER JOIN [Group] g ON g.[Id] = ao.[GroupId]
    INNER JOIN [GroupType] gt ON gt.[Id] = g.[GroupTypeId]
    INNER JOIN [DefinedValue] dvp ON dvp.[Id] = gt.[GroupTypePurposeValueId] AND dvp.[Guid] = @ServingAreaDefinedValueGuid
    LEFT JOIN [GroupMember] gm ON gm.[PersonId] = p.[Id] AND gm.[GroupId] = g.[Id]
    LEFT JOIN CTE_CampusShortCode csc ON csc.GroupId = g.[Id]
    LEFT JOIN [DefinedValue] dvcs ON dvcs.[Id] = p.[ConnectionStatusValueId] AND dvcs.[DefinedTypeId] = @ConnectionStatusDefinedTypeId
    LEFT JOIN CTE_GivingResult GR ON p.[GivingId] = GR.GivingId
WHERE
    ao.[OccurrenceDateKey] >= @StartDateKey AND a.[DidAttend] = 1
GROUP BY
    p.[Id], p.[LastName], p.[NickName], p.[PhotoId], p.[GivingId], g.[Id], g.[Name], csc.CampusShortCode, dvcs.[Value], p.[RecordStatusValueId], gm.[IsArchived], GR.DonationMonthYearBitmask;

{% endsql %}
{
    ""PeopleData"": [
    {% for result in results %}
        {% if forloop.first != true %},{% endif %}
        {
            ""PersonGroupKey"": {{ result.PersonGroupKey | ToJSON }},
            ""PersonId"": {{ result.PersonId }},
            ""LastName"": {{ result.LastName | ToJSON }},
            ""NickName"": {{ result.NickName | ToJSON }},
            ""PhotoUrl"": ""{% if result.PhotoId %}{{ result.PhotoId | AvatarUrl }}{% else %}{{ publicApplicationRoot }}GetAvatar.ashx?AgeClassification=Adult&Gender=Male&RecordTypeId=1&Text={{ result.NickName | Slice: 0, 1 | UrlEncode }}{{ result.LastName | Slice: 0, 1 | UrlEncode }}{% endif %}"",
            ""GivingId"": {{ result.GivingId | ToJSON }},
            ""LastAttendanceDate"": ""{{ result.LastAttendanceDate | Date: 'yyyy-MM-dd' }}"",
            ""GroupId"": {{ result.GroupId }},
            ""GroupName"": {{ result.GroupName | ToJSON }},
            ""CampusShortCode"": {{ result.CampusShortCode | ToJSON }},
            ""ConnectionStatus"": {{ result.ConnectionStatus | ToJSON }},
            ""IsActive"": {{ result.IsActive }},
            ""DonationMonthYearBitmask"": {% if result.DonationMonthYearBitmask != null %}{{ result.DonationMonthYearBitmask | ToJSON}}{% else %}null{% endif %}
        }
    {% endfor %}
    ]
}
";

            Sql( $@"UPDATE [PersistedDataset]
           SET [BuildScript] = '{newBuildScript.Replace( "'", "''" )}'
           WHERE [Guid] = '10539E72-B5D3-48E2-B9C6-DB43AFDAD55F'" );

            Sql( $@"UPDATE [PersistedDataset]
               SET [ResultData] = null
               WHERE [Guid] = '10539E72-B5D3-48E2-B9C6-DB43AFDAD55F'" );
        }

        /// <summary>
        /// KH: Add Calendar Block Attributes Up
        /// </summary>
        private void AddCalendarBlockAttributesUp()
        {
            // Attribute for  BlockType
            //   BlockType: Calendar Lava
            //   Category: Event
            //   Attribute: Show Only Events With Registrations
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8760D668-8ADF-48C8-9D90-09461FB75B88", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Only Events With Registrations", "ShowOnlyEventsWithRegistrations", "Show Only Events With Registrations", @"Determines whether the events shown must have registrations.", 23, @"False", "3FC89249-B234-4F61-A853-22BD68CB95E5" );

            // Attribute for  BlockType
            //   BlockType: Calendar Item Occurrence List By Audience Lava
            //   Category: Event
            //   Attribute: Max Occurrences Per Event Item
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E4703964-7717-4C93-BD40-7DFF85EAC5FD", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Occurrences Per Event Item", "MaxOccurrencesPerEventItem", "Max Occurrences per Event Item", @"The maximum number of occurrences to show per Event Item. Set to 0 to show all occurrences for each Event Item.", 6, @"0", "7401465A-02DE-4A2D-8704-F9593A023DF8" );

            Sql( $@"UPDATE [Attribute] SET [Order] = 7 WHERE [Guid] = 'F6FC53C3-BC30-489E-B507-301B225B3567'" );

            Sql( $@"UPDATE [Attribute] SET [Order] = 8 WHERE [Guid] = '1C26B933-5BB7-4CA9-9804-05FDF4474FF2'" );

            Sql( $@"UPDATE [Attribute] SET [Order] = 9 WHERE [Guid] = '9755F457-8185-49FE-861E-9F87A1D5AC02'" );
        }

        /// <summary>
        /// KH: Add Calendar Block Attributes Down
        /// </summary>
        private void AddCalendarBlockAttributesDown()
        {
            // Attribute for BlockType
            //   BlockType: Calendar Lava
            //   Category: Event
            //   Attribute: Show Only Events With Registrations
            RockMigrationHelper.DeleteAttribute( "3FC89249-B234-4F61-A853-22BD68CB95E5" );

            // Attribute for BlockType
            //   BlockType: Calendar Item Occurrence List By Audience Lava
            //   Category: Event
            //   Attribute: Max Occurrences Per Event Item
            RockMigrationHelper.DeleteAttribute( "7401465A-02DE-4A2D-8704-F9593A023DF8" );

            Sql( $@"UPDATE [Attribute] SET [Order] = 6 WHERE [Guid] = 'F6FC53C3-BC30-489E-B507-301B225B3567'" );

            Sql( $@"UPDATE [Attribute] SET [Order] = 7 WHERE [Guid] = '1C26B933-5BB7-4CA9-9804-05FDF4474FF2'" );

            Sql( $@"UPDATE [Attribute] SET [Order] = 8 WHERE [Guid] = '9755F457-8185-49FE-861E-9F87A1D5AC02'" );
        }

        /// <summary>
        /// PA: Update Apple Devices
        /// </summary>
        private void UpdateAppleDevicesDefinedType()
        {

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad16,6", "iPad Pro 12.9 inch 7th Gen", "A6954EC7-E1C0-459E-A852-BCE4056ADCD0", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad16,5", "iPad Pro 12.9 inch 7th Gen", "FBFCC7B2-EC3F-496F-8C42-F7A0910567C4", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad16,4", "iPad Pro 11 inch 5th Gen", "95B01FFE-C49B-4A93-B9D2-679A7AA0AB25", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad16,3", "iPad Pro 11 inch 5th Gen", "E3973B42-3BEE-4966-9C58-3BD6D978AD27", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad14,9", "iPad Air 6th Gen", "808345FE-3F2B-4B16-B254-D3A72119E8AA", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad14,8", "iPad Air 6th Gen", "265712E5-1C3C-4364-884F-A29A306A4A65", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad14,11", "iPad Air 7th Gen", "BE61E10C-9BD9-4807-9D85-6DD00B86C07A", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad14,10", "iPad Air 7th Gen", "F7132478-7ED0-47EA-9509-53E55E091BAF", true );

            Sql( @"
DECLARE @AppleDeviceDefinedTypeId INT = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = 'DAE31F78-7AB9-4ACE-9EE1-C1E6A734562C')

UPDATE [PersonalDevice] SET [Model] = dv.[Description]
FROM [PersonalDevice] pd
JOIN [DefinedValue] dv ON pd.[Model] = dv.[Value]
WHERE pd.[Manufacturer] = 'Apple'
  AND pd.[Model] like '%,%'
  AND dv.DefinedTypeId = @AppleDeviceDefinedTypeId
" );
        }
    }
}
