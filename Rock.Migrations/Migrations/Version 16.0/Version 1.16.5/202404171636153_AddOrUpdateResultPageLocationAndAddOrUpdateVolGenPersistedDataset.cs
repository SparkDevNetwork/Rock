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
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class AddOrUpdateResultPageLocationAndAddOrUpdateVolGenPersistedDataset : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Create the "Reports" page under the "Finance" tab
            RockMigrationHelper.AddPage( true, "7BEB7569-C485-40A0-A609-B0678F6F7240", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Reports", "", "8D5917F1-4E0E-4F18-8815-62EFBF808995", "" );

            // Move the Transaction Fee Report under the newly created "Reports" page
            RockMigrationHelper.MovePage( "A3E321E9-2FBB-4BB9-8AEE-E810B7CC5914", "8D5917F1-4E0E-4F18-8815-62EFBF808995" );

            // Create the "Volunteer Generosity" page under the "Reports" page
            RockMigrationHelper.AddPage( true, "8D5917F1-4E0E-4F18-8815-62EFBF808995", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Volunteer Generosity", "", "16DD0891-E3D4-4FF3-9857-0869A6CCBA39", "" );

            // Move "Reports" page under "Administration"
            RockMigrationHelper.MovePage( "8D5917F1-4E0E-4F18-8815-62EFBF808995", "18C9E5C3-3E28-4AA3-84F6-78CD4EA2DD3C" );

            // Add the Page Menu block to the Reports page with Template {% include '~~/Assets/Lava/PageListAsBlocks.lava' %}
            string reportsPageGuid = "8D5917F1-4E0E-4F18-8815-62EFBF808995";
            RockMigrationHelper.AddBlock( true, reportsPageGuid.AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), Rock.SystemGuid.BlockType.PAGE_MENU.AsGuid(), "Page Menu", "Main", @"", @"", 0, "EB16AA76-63C9-43B1-BFD1-C10B6E48603C" );

            // Update default attribute value
            RockMigrationHelper.AddBlockAttributeValue( false, "EB16AA76-63C9-43B1-BFD1-C10B6E48603C", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsBlocks.lava' %}" );

            // Set order of "Reports" page to be 8
            Sql( $"UPDATE [Page] SET [Order] = 8 WHERE [Guid] = '8D5917F1-4E0E-4F18-8815-62EFBF808995'" );

            // Set icons (IconCssClass) for child pages
            RockMigrationHelper.UpdatePageIcon( "A3E321E9-2FBB-4BB9-8AEE-E810B7CC5914", "fa fa-file-invoice-dollar" ); // Transaction Fee Report
            RockMigrationHelper.UpdatePageIcon( "16DD0891-E3D4-4FF3-9857-0869A6CCBA39", "fa fa-hand-holding-heart" ); // Volunteer Generosity

            // Register the EntityType for the Volunteer Generosity Analysis Block
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Reporting.VolunteerGenerosityAnalysis", "4C55BFE1-7E97-4CFB-BCB7-2015AA25D9B9", false, true );

            // Add or Update the Block Type
            RockMigrationHelper.AddOrUpdateEntityBlockType(
                "Volunteer Generosity Analysis",
                "Displays an analysis of volunteer generosity based on a persisted dataset.",
                "Rock.Blocks.Reporting.VolunteerGenerosityAnalysis",
                "Reporting",
                "586A26F1-8A9C-4AB4-B788-9B44895B9D40"
            );

            // Parameters for the schedule and persisted dataset
            string scheduleName = "Volunteer Generosity Schedule";
            string scheduleDescription = "Schedule used to run the persisted dataset job for the Volunteer Generosity block.";
            string iCalendarContent = "BEGIN:VCALENDAR\r\nPRODID:-//github.com/SparkDevNetwork/Rock//NONSGML Rock//EN\r\nVERSION:2.0\r\nBEGIN:VEVENT\r\nDTEND:20240304T060100\r\nDTSTAMP:20240305T095232\r\nDTSTART:20240304T060000\r\nRRULE:FREQ=WEEKLY;BYDAY=MO\r\nSEQUENCE:0\r\nUID:577ed439-4e7e-4445-a5a3-fcf52de2174c\r\nEND:VEVENT\r\nEND:VCALENDAR";
            DateTime effectiveStartDate = DateTime.Parse( "2024-03-04" );
            bool scheduleIsActive = true;
            string scheduleGuid = "ACE62853-0A10-4523-8BA2-CF7597F1D190";
            string datasetAccessKey = "VolunteerGenerosity";
            string datasetName = "Volunteer Generosity";
            string datasetDescription = "An in-depth dataset focusing on volunteer engagement and giving behaviors, designed to support strategic decision-making for Church Leaders. This dataset is used by the Volunteer Generosity Analysis block and undergoes regular updates for optimization and improved insights during future Rock updates; editing is not advised.";
            bool datasetAllowManualRefresh = true;
            int resultFormat = 0;
            string datasetBuildScript = @"{% assign monthNames = ""Jan,Feb,Mar,Apr,May,Jun,Jul,Aug,Sep,Oct,Nov,Dec"" | Split: "","" %}
{% assign publicApplicationRoot = 'Global' | Attribute:'PublicApplicationRoot' %}

{% sql %}
DECLARE @NumberOfDays INT = 365;
DECLARE @NumberOfMonths INT = 13;
DECLARE @ServingAreaDefinedValueGuid UNIQUEIDENTIFIER = '36a554ce-7815-41b9-a435-93f3d52a2828';
DECLARE @ActiveRecordStatusValueId INT = (SELECT Id FROM DefinedValue WHERE Guid = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E');

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
    WHERE
        ao.OccurrenceDateKey >= @StartDateKey
        AND a.DidAttend = 1
    GROUP BY
        p.Id, p.LastName, p.NickName, p.PhotoId, p.GivingId, g.Id, g.Name, c.Id, c.ShortCode, c.Name, gm.GroupRoleId, gr.Name, p.RecordStatusValueId, gm.IsArchived
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
}";

            int datasetBuildScriptType = 0;
            bool datasetIsSystem = true;
            bool datasetIsActive = true;
            string datasetEnabledLavaCommands = "All";
            string datasetGuid = "10539E72-B5D3-48E2-B9C6-DB43AFDAD55F";

            // Create the schedule and persisted dataset
            RockMigrationHelper.AddOrUpdatePersistedDatasetWithSchedule(
                scheduleGuid,
                scheduleName,
                scheduleDescription,
                iCalendarContent,
                effectiveStartDate,
                scheduleIsActive,
                datasetGuid,
                datasetAccessKey,
                datasetName,
                datasetDescription,
                datasetAllowManualRefresh,
                resultFormat,
                datasetBuildScript,
                datasetBuildScriptType,
                datasetIsSystem,
                datasetIsActive,
                datasetEnabledLavaCommands );

            // Add the Volunteer Generosity Analysis block to the Volunteer Generosity page
            RockMigrationHelper.AddBlock(
                true,
                "16DD0891-E3D4-4FF3-9857-0869A6CCBA39".AsGuid(),
                null,
                "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),
                "586A26F1-8A9C-4AB4-B788-9B44895B9D40".AsGuid(),
                "Volunteer Generosity Analysis",
                "Main",
                @"",
                @"",
                0,
                "3252A755-01C1-497A-9D37-8ED91D23E061"
            );

            // Hide the Volunteer Generosity page from navigation
            Sql( $"UPDATE [Page] SET [DisplayInNavWhen] = {( int ) Model.DisplayInNavWhen.Never} WHERE [Guid] = '16DD0891-E3D4-4FF3-9857-0869A6CCBA39';" );
            Sql( $"UPDATE [Page] SET [DisplayInNavWhen] = {( int ) Model.DisplayInNavWhen.Always} WHERE [Guid] = '8D5917F1-4E0E-4F18-8815-62EFBF808995';" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
