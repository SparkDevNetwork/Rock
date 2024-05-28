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
    public partial class Rollup_20240425 : Rock.Migrations.RockMigration
    {
        public override void Up()
        {
            UpdateBuildScript();
            AddGenerosityPageRoute();
        }

        public override void Down()
        {

        }

        /// <summary>
        /// JR: Data Migration to Add "finance/generosity-report" Page Route and update the build script - (DONE - Updated 4/24)
        /// </summary>
        private void UpdateBuildScript()
        {
            string newBuildScript = @"//- Retrieve the base URL for linking photos from a global attribute 
{% assign publicApplicationRoot = 'Global' | Attribute:'PublicApplicationRoot' %}

//- Initialize a string to track which person-group combinations have been processed 
{% assign personKeys = """" %}

{% sql %}
//- Define necessary variables for date calculation and entity identification
DECLARE @NumberOfDays INT = 365;
DECLARE @NumberOfMonths INT = 13;
DECLARE @ServingAreaDefinedValueGuid UNIQUEIDENTIFIER = '36a554ce-7815-41b9-a435-93f3d52a2828';
DECLARE @ActiveRecordStatusValueId INT = (SELECT Id FROM DefinedValue WHERE Guid = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E');
DECLARE @ConnectionStatusDefinedTypeId INT = (SELECT Id FROM DefinedType WHERE [Guid] = '2e6540ea-63f0-40fe-be50-f2a84735e600');

//- Calculate date keys for filtering attendance and giving data
DECLARE @StartDateKey INT = (SELECT TOP 1 [DateKey] FROM [AnalyticsSourceDate] WHERE [Date] = CAST(DATEADD(DAY, -@NumberOfDays, GETDATE()) AS DATE));
DECLARE @CurrentMonth DATE = DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0);
DECLARE @StartingDateKeyForGiving INT = (SELECT [DateKey] FROM [AnalyticsSourceDate] WHERE [Date] = DATEADD(MONTH, -@NumberOfMonths, @CurrentMonth));

//- Define the main query for attendance data
;WITH CTE_Attendance AS (
    SELECT
        p.[Id] AS PersonId
        , p.[LastName]
        , p.[NickName]
        , p.[PhotoId]
        , p.[GivingId]
        , g.[Id] AS GroupId
        , g.[Name] AS GroupName
        , ISNULL(c.[Id], 0) AS CampusId
        , ISNULL(c.[ShortCode], '') AS CampusShortCode
        , c.[Name] AS CampusName
        , MAX(ao.[OccurrenceDate]) AS LastAttendanceDate
        , gm.[GroupRoleId]
        , dvcs.[Value] AS ConnectionStatus
        , CAST(CASE WHEN p.[RecordStatusValueId] = @ActiveRecordStatusValueId AND gm.[IsArchived] = 0 THEN 1 ELSE 0 END AS BIT) AS IsActive
    FROM
        [Person] p INNER JOIN [PersonAlias] pa ON pa.[PersonId] = p.[Id]
        INNER JOIN [Attendance] a ON a.[PersonAliasId] = pa.[Id]
        INNER JOIN [AttendanceOccurrence] ao ON ao.[Id] = a.[OccurrenceId]
        INNER JOIN [Group] g ON g.[Id] = ao.[GroupId]
        LEFT JOIN [GroupMember] gm ON gm.[PersonId] = p.[Id] AND gm.[GroupId] = g.[Id]
        LEFT JOIN [GroupTypeRole] gr ON gr.[Id] = gm.[GroupRoleId]
        INNER JOIN [GroupType] gt ON gt.[Id] = g.[GroupTypeId]
        INNER JOIN [DefinedValue] dvp ON dvp.[Id] = gt.[GroupTypePurposeValueId] AND dvp.[Guid] = @ServingAreaDefinedValueGuid
        LEFT JOIN [Campus] c ON c.[Id] = g.[CampusId]
        LEFT JOIN [DefinedValue] dvcs ON dvcs.[Id] = p.[ConnectionStatusValueId] AND dvcs.[DefinedTypeId] = @ConnectionStatusDefinedTypeId
    WHERE
        ao.[OccurrenceDateKey] >= @StartDateKey AND a.[DidAttend] = 1
    GROUP BY
        p.[Id], p.[LastName], p.[NickName], p.[PhotoId], dvcs.[Value], p.[GivingId], g.[Id], g.[Name], c.[Id], c.[ShortCode], c.[Name], gm.[GroupRoleId], gr.[Name], p.[RecordStatusValueId], gm.[IsArchived]
),
//- Define the query for giving data
CTE_Giving AS (
    SELECT
        p.[GivingId]
        , asd.[CalendarMonthNameAbbreviated]
        , asd.[CalendarYear]
        , asd.[CalendarMonth]
    FROM
        [Person] p
        INNER JOIN [PersonAlias] pa ON pa.[PersonId] = p.[Id]
        INNER JOIN [FinancialTransaction] ft ON ft.[AuthorizedPersonAliasId] = pa.[Id]
        INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
        INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.[AccountId]
        INNER JOIN [AnalyticsSourceDate] asd ON asd.[DateKey] = ft.[TransactionDateKey]
    WHERE
        fa.[IsTaxDeductible] = 1
        AND ft.[TransactionDateKey] >= @StartingDateKeyForGiving
    GROUP BY
        p.[GivingId], asd.[CalendarMonthNameAbbreviated], asd.[CalendarYear], asd.[CalendarMonth]
    HAVING
        SUM(ftd.[Amount]) > 0
)
//- Retrieve final data merging attendance and giving records
SELECT
    AD.PersonId,
    AD.LastName,
    AD.NickName,
    AD.PhotoId,
    AD.GivingId,
    AD.GroupId,
    AD.GroupName,
    AD.CampusId,
    AD.CampusShortCode,
    AD.CampusName,
    AD.LastAttendanceDate,
    AD.GroupRoleId,
    AD.ConnectionStatus,
    AD.IsActive,
    GD.CalendarMonthNameAbbreviated,
    GD.CalendarYear,
    GD.CalendarMonth
FROM
    CTE_Attendance AD
    LEFT JOIN CTE_Giving GD ON AD.GivingId = GD.GivingId;
{% endsql %}

//- JSON structure starting with PeopleData as the root object
{
    ""PeopleData"": [
    {% for result in results %}
        {% assign personGroupKey = result.PersonId | Append: '-' | Append: result.GroupId %}
        
        //- Ensure each person-group combination is processed only once
        {% unless personKeys contains personGroupKey %}
            {% assign personKeys = personKeys | Append: personGroupKey | Append: ',' %}
            {% assign donations = results | Where: 'PersonId', result.PersonId | Where: 'GroupId', result.GroupId %}
     
            //- Comma handling for multiple entries in the JSON array
            {% if forloop.first != true %},{% endif %}
            
            //- Here we start building the JSON object representing a person with detailed information and donations
            {
                ""PersonGroupKey"": ""{{ personGroupKey }}"",
                ""PersonDetails"": {
                    ""PersonId"": {{ result.PersonId | ToJSON }},
                    ""LastName"": {{ result.LastName | ToJSON }},
                    ""NickName"": {{ result.NickName | ToJSON }},
                    ""PhotoUrl"": ""{% if result.PhotoId %}{{ publicApplicationRoot }}GetAvatar.ashx?PhotoId={{ result.PhotoId }}{% else %}{{ publicApplicationRoot }}GetAvatar.ashx?AgeClassification=Adult&Gender=Male&RecordTypeId=1&Text={{ result.NickName | Slice: 0, 1 | UrlEncode }}{{ result.LastName | Slice: 0, 1 | UrlEncode }}{% endif %}"",
                    ""GivingId"": {{ result.GivingId | ToJSON }},
                    ""LastAttendanceDate"": ""{{ result.LastAttendanceDate | Date: 'yyyy-MM-dd' }}"",
                    ""GroupId"": {{ result.GroupId | ToJSON }},
                    ""GroupName"": {{ result.GroupName | ToJSON }},
                    ""CampusId"": {{ result.CampusId | ToJSON }},
                    ""CampusShortCode"": {{ result.CampusShortCode | Default: result.CampusName | ToJSON }},
                    ""ConnectionStatus"": {{ result.ConnectionStatus | ToJSON }},
                    ""IsActive"": {{ result.IsActive | ToJSON }}
                },
                ""Donations"": [
                    {% for donation in donations %}
                    {% if forloop.first != true %},{% endif %}
                    {
                        ""MonthNameAbbreviated"": {{ donation.CalendarMonthNameAbbreviated | ToJSON }},
                        ""Year"": {{ donation.CalendarYear | ToJSON }},
                        ""Month"": {{ donation.CalendarMonth | ToJSON }}
                    }
                    
                    {% endfor %}
                ]
            }
        {% endunless %}
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

            Sql( $@"UPDATE [PersistedDataset]
                SET[EnabledLavaCommands] = 'Sql'
                WHERE [Guid] = '10539E72-B5D3-48E2-B9C6-DB43AFDAD55F'"
                );
        }

        private void AddGenerosityPageRoute()
        {
            var volunteerGenerosityPageGuid = "16DD0891-E3D4-4FF3-9857-0869A6CCBA39";
#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.AddPageRoute( volunteerGenerosityPageGuid, "finance/generosity-report" );
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
