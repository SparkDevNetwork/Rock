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
        /// <summary>
        /// JR: Data Migration to Add "finance/generosity-report" Page Route and update the build script - (DONE - Updated 4/24)
        /// </summary>
        public override void Up()
        {
            UpdateBuildScript();
            AddGenerosityPageRoute();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        private void UpdateBuildScript()
        {
            string newBuildScript = @"{% assign monthNames = ""Jan,Feb,Mar,Apr,May,Jun,Jul,Aug,Sep,Oct,Nov,Dec"" | Split: "","" %}
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

;WITH CTE_Attendance AS (
    SELECT
        p.Id AS PersonId,
        p.LastName,
        p.NickName,
        p.PhotoId,
        p.GivingId,
        g.Id AS GroupId,
        g.Name AS GroupName,
        ISNULL(c.Id, 0) AS CampusId,
        ISNULL(c.ShortCode, '') AS CampusShortCode,
        c.Name AS CampusName,
        MAX(ao.OccurrenceDate) AS LastAttendanceDate,
        gm.GroupRoleId,
        gr.Name AS GroupRoleName,
		dvcs.Value AS ConnectionStatus,
        CAST(CASE WHEN p.RecordStatusValueId = @ActiveRecordStatusValueId AND gm.IsArchived = 0 THEN 1 ELSE 0 END AS BIT) AS IsActive
    FROM
        Person p
        INNER JOIN PersonAlias pa ON pa.PersonId = p.Id
        INNER JOIN Attendance a ON a.PersonAliasId = pa.Id
        INNER JOIN AttendanceOccurrence ao ON ao.Id = a.OccurrenceId
        INNER JOIN [Group] g ON g.Id = ao.GroupId
        LEFT JOIN GroupMember gm ON gm.PersonId = p.Id AND gm.GroupId = g.Id
        LEFT JOIN GroupTypeRole gr ON gr.Id = gm.GroupRoleId
        INNER JOIN GroupType gt ON gt.Id = g.GroupTypeId
        INNER JOIN DefinedValue dvp ON dvp.Id = gt.GroupTypePurposeValueId AND dvp.Guid = @ServingAreaDefinedValueGuid
        LEFT JOIN Campus c ON c.Id = g.CampusId
		LEFT JOIN DefinedValue dvcs ON dvcs.Id = p.ConnectionStatusValueId and dvcs.DefinedTypeId = @ConnectionStatusDefinedTypeId
    WHERE
        ao.OccurrenceDateKey >= @StartDateKey
        AND a.DidAttend = 1
    GROUP BY
        p.Id, p.LastName, p.NickName, p.PhotoId, dvcs.Value, p.GivingId, g.Id, g.Name, c.Id, c.ShortCode, c.Name, gm.GroupRoleId, gr.Name, p.RecordStatusValueId, gm.IsArchived
),
CTE_Giving AS (
    SELECT
        p.GivingId,
        asd.CalendarMonthNameAbbreviated,
        asd.CalendarYear,
        asd.CalendarMonth
    FROM
        Person p
        INNER JOIN PersonAlias pa ON pa.PersonId = p.Id
        INNER JOIN FinancialTransaction ft ON ft.AuthorizedPersonAliasId = pa.Id
        INNER JOIN FinancialTransactionDetail ftd ON ftd.TransactionId = ft.Id
        INNER JOIN FinancialAccount fa ON fa.Id = ftd.AccountId
        INNER JOIN AnalyticsSourceDate asd ON asd.DateKey = ft.TransactionDateKey
    WHERE
        fa.IsTaxDeductible = 1
        AND ft.TransactionDateKey >= @StartingDateKeyForGiving
    GROUP BY
        p.GivingId, asd.CalendarMonthNameAbbreviated, asd.CalendarYear, asd.CalendarMonth
    HAVING
        SUM(ftd.Amount) > 0
)

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
    AD.GroupRoleName,
    AD.IsActive,
	AD.ConnectionStatus,
	AD.IsActive,
    GD.CalendarMonthNameAbbreviated,
    GD.CalendarYear,
    GD.CalendarMonth
FROM
    CTE_Attendance AD
    LEFT JOIN CTE_Giving GD ON AD.GivingId = GD.GivingId;
{% endsql %}
{% assign jsonData = """" %}
{% for result in results %}
  {% capture personGroupKey %}{{ result.PersonId }}-{{ result.GroupId }}{% endcapture %}
  {% unless jsonData contains personGroupKey %}
    {% assign donations = results | Where: ""PersonId"", result.PersonId | Where: ""GroupId"", result.GroupId %}
    {% capture donationsJson %}
      {% for donation in donations %}
        {
          ""MonthNameAbbreviated"": ""{{ donation.CalendarMonthNameAbbreviated }}"",
          ""Year"": ""{{ donation.CalendarYear }}"",
          ""Month"": ""{{ donation.CalendarMonth }}""
        }{% unless forloop.last %},{% endunless %}
      {% endfor %}
    {% endcapture %}
    {% capture personJson %}
      {
        ""PersonGroupKey"": ""{{ personGroupKey }}"",
        ""PersonDetails"": {
          ""PersonId"": ""{{ result.PersonId }}"",
          ""LastName"": ""{{ result.LastName | Escape }}"",
          ""NickName"": ""{{ result.NickName | Escape }}"",
          ""PhotoUrl"": ""{% if result.PhotoId %}{{ publicApplicationRoot }}GetAvatar.ashx?PhotoId={{ result.PhotoId }}{% else %}{{ publicApplicationRoot }}GetAvatar.ashx?AgeClassification=Adult&Gender=Male&RecordTypeId=1&Text={{ result.NickName | Slice: 0, 1 | UrlEncode }}{{ result.LastName | Slice: 0, 1 | UrlEncode }}{% endif %}"",
          ""GivingId"": ""{{ result.GivingId }}"",
          ""LastAttendanceDate"": ""{{ result.LastAttendanceDate | Date: 'yyyy-MM-dd' }}"",
          ""GroupId"": ""{{ result.GroupId }}"",
          ""GroupName"": ""{{ result.GroupName | Escape }}"",
          ""CampusId"": ""{{ result.CampusId | Append: '' }}"",
          ""CampusShortCode"": ""{% if result.CampusShortCode != '' %}{{ result.CampusShortCode }}{% else %}{{ result.CampusName | Escape }}{% endif %}"",
          ""ConnectionStatus"": ""{{ result.ConnectionStatus }}"",
          ""IsActive"": {{ result.IsActive }}
        },
        ""Donations"": [{{ donationsJson | Strip_Newlines }}]
      }
    {% endcapture %}
    {% assign jsonData = jsonData | Append: personJson %}
    {% if forloop.last == false %}
      {% assign jsonData = jsonData | Append: "","" %}
    {% endif %}
  {% endunless %}
{% endfor %}

{
  ""PeopleData"": [{{ jsonData | Strip_Newlines }}]
}
            ";

            Sql( $@"UPDATE [dbo].[PersistedDataset]
           SET [BuildScript] = '{newBuildScript.Replace( "'", "''" )}'
           WHERE [Guid] = '10539E72-B5D3-48E2-B9C6-DB43AFDAD55F'" );

            Sql( $@"UPDATE [dbo].[PersistedDataset]
           SET [ResultData] = null
           WHERE [Guid] = '10539E72-B5D3-48E2-B9C6-DB43AFDAD55F'" );
        }

        private void AddGenerosityPageRoute()
        {
            var volunteerGenerosityPageGuid = "16DD0891-E3D4-4FF3-9857-0869A6CCBA39";
            RockMigrationHelper.AddPageRoute( volunteerGenerosityPageGuid, "finance/generosity-report" );
        }
    }
}
