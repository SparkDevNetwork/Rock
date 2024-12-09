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
    public partial class Rollup_20240801 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateVolGenBuildScriptUp();
            UpdateRegistrationTemplateSuccessTextUp();
            UpdateCacheDurationForPagesUp();
            AddEmailMetricsReminderSystemCommunicationUp();
            RemoveCurrencySymbolGlobalAttributeFromFinancialStatementTemplateUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            UpdateRegistrationTemplateSuccessTextDown();
            AddEmailMetricsReminderSystemCommunicationDown();
        }

        #region JDR: Update Build Script

        ///<summary>
        /// JDR: Updating the build script to get additional Person details
        /// </summary>
        private void UpdateVolGenBuildScriptUp()
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
    p.[Gender],
    p.[Age],
    p.[AgeClassification],
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
    p.[Id], p.[LastName], p.[NickName], p.[PhotoId], p.[Gender], p.[Age], p.[AgeClassification], p.[GivingId], g.[Id], g.[Name], csc.[CampusShortCode], dvcs.[Value], p.[RecordStatusValueId], gm.[IsArchived], GR.[DonationMonthYearBitmask];

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
            ""Gender"": {{ result.Gender | ToJSON }},
            ""Age"": {{ result.Age | ToJSON }},
            ""AgeClassification"": {{ result.AgeClassification | ToJSON }},
            ""PhotoId"": {{ result.PhotoId | ToJSON }},
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

        #endregion

        #region JMH: Update Registration Template Success Text

        private const string RegistrationTemplateSuccessTextFindValue = @"'{% if waitListCount > 0 %}
    <p>
        You have successfully added the following
        {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | PluralizeForQuantity:registrantCount | Downcase }}
        to the waiting list for {{ RegistrationInstance.Name }}:
    </p>

    <ul>
    {% for registrant in waitlist %}
        <li>
            <strong>{{ registrant.PersonAlias.Person.FullName }}</strong>
        </li>
    {% endfor %}
    </ul>
{% endif %}'";
        private const string RegistrationTemplateSuccessTextReplaceValue = @"'{% if waitListCount > 0 %}
    <p>
        The following were added to the wait list:
    </p>
    <ul>
    {% for registrant in waitlist %}
        <li>
            <strong>{{ registrant.PersonAlias.Person.FullName }}</strong> - {{ registrant.Cost | FormatAsCurrency }}{% if registrant.Cost == 0 %} (not charged){% endif %} - <span class=""badge badge-warning"">Waiting List</span>
        </li>
    {% endfor %}
    </ul>
{% endif %}'";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        private void UpdateRegistrationTemplateSuccessTextUp()
        {
            UpdateRegistrationTemplateSuccessTextWaitListSection( RegistrationTemplateSuccessTextFindValue, RegistrationTemplateSuccessTextReplaceValue );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        private void UpdateRegistrationTemplateSuccessTextDown()
        {
            UpdateRegistrationTemplateSuccessTextWaitListSection( RegistrationTemplateSuccessTextReplaceValue, RegistrationTemplateSuccessTextFindValue );
        }

        /// <summary>
        /// JMH: Updates the wait list section of the RegistrationTemplate.SuccessText Lava template.
        /// </summary>
        private void UpdateRegistrationTemplateSuccessTextWaitListSection( string findValue, string replaceValue )
        {
            Sql( $"UPDATE [RegistrationTemplate] SET [SuccessText] = REPLACE( [SuccessText], {NormalizeWhiteSpace( findValue )}, {replaceValue} )" );
        }

        private string NormalizeWhiteSpace( string sql )
        {
            return $@"REPLACE( 
                REPLACE( 
                    REPLACE( {sql}, CHAR(13)+CHAR(10), CHAR(29) )
                    , CHAR(10), CHAR(13)+CHAR(10) )
                , CHAR(29), CHAR(13)+CHAR(10) )";
        }

        #endregion

        #region SK: Update Cache Duration setting to "0" on two stock pages

        private void UpdateCacheDurationForPagesUp()
        {
            // Use NormalizeColumnCRLF when attempting to do a WHERE clause or REPLACE using multi line strings!
            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "Content" );

            Sql( $@"
DECLARE @EmployeeResBlockId INT = (SELECT [Id] FROM [Block] WHERE [Guid]='718C516F-0A1D-4DBC-A939-1D9777208FEC')
DECLARE @SharedDocBlockId INT = (SELECT [Id] FROM [Block] WHERE [Guid]='B8224C72-4168-40F0-96BE-38F2AFD525F5')
DECLARE @CacheDurationAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid]='4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4')

IF @SharedDocBlockId IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT [Id]
        FROM [HtmlContent]
        WHERE 
             {targetColumn} LIKE '%Page for posting common documents that are used often.%'
              AND [BlockId]=@SharedDocBlockId)
    BEGIN
        UPDATE
            [AttributeValue]
        SET [Value]='0'
        WHERE [EntityId]=@SharedDocBlockId AND [AttributeId]=@CacheDurationAttributeId
    END
END

IF @EmployeeResBlockId IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT [Id]
        FROM [HtmlContent]
        WHERE 
             {targetColumn} LIKE '%A starter page for HR information from the organization.%'
              AND [BlockId]=@EmployeeResBlockId)
    BEGIN
        UPDATE
            [AttributeValue]
        SET [Value]='0'
        WHERE [EntityId]=@EmployeeResBlockId AND [AttributeId]=@CacheDurationAttributeId
    END
END" );
        }

        #endregion

        #region JMH: Add Email Metrics Reminder System Communication

        /// <summary>
        /// JMH: Adds the Email Metrics Reminder System Communication.
        /// </summary>
        private void AddEmailMetricsReminderSystemCommunicationUp()
        {
            RockMigrationHelper.UpdateSystemCommunication( "Communication",
                "Email Metrics Reminder", // title
                "", // from
                "", // fromName
                "", // to
                "", // cc
                "", // bcc
                "Email Metrics Reminder", // subject
                                          // body
                @"{{ 'Global' | Attribute:'EmailHeader' }}

<h1>Email Metrics Reminder</h1>

<p>This is your reminder to check the impact of an email you recently sent.</p>

<p>
    <strong>Sent On:</strong> {{ Communication.SendDateTime | Date:'dddd, MMMM d' }} {{ Communication.SendDateTime | Date:'h:mmtt' | Downcase }}<br/>
    <strong>Subject:</strong> {{ Communication.Subject }}<br/>
    <strong>Recipients:</strong> {{ RecipientsCount | Format:'N0' }}<br/>
</p>

<p><!--[if mso]>
<v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ MetricsUrl }}"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""11%"" strokecolor=""#269abc"" fillcolor=""#31b0d5"">
<w:anchorlock/>
<center style=""color:#ffffff;font-family:sans-serif;font-size:14px;font-weight:normal;"">View Metrics</center>
</v:roundrect>
<![endif]-->
    <a href=""{{ MetricsUrl }}"" style=""background-color:#31b0d5;border:1px solid #269abc;border-radius:4px;color:#ffffff !important;display:inline-block;font-family:sans-serif;font-size:14px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"">View metrics</a>
</p>

{{ 'Global' | Attribute:'EmailFooter' }}",
                Rock.SystemGuid.SystemCommunication.COMMUNICATION_EMAIL_METRICS_REMINDER );
        }

        /// <summary>
        /// JMH: Removes the Email Metrics Reminder System Communication.
        /// </summary>
        private void AddEmailMetricsReminderSystemCommunicationDown()
        {
            RockMigrationHelper.DeleteSystemCommunication( Rock.SystemGuid.SystemCommunication.COMMUNICATION_EMAIL_METRICS_REMINDER );
        }

        #endregion

        #region PA: (Default Contribution Template) Replaced deprecated Global Attribute Currency Symbol with FormatAsCurrency lava filter

        /// <summary>
        /// PA: Replaced deprecated Global Attribute Currency Symbol with FormatAsCurrency lava filter for printing currency values in the Rock Default Contribution Template.
        /// </summary>
        private void RemoveCurrencySymbolGlobalAttributeFromFinancialStatementTemplateUp()
        {
            Sql( @"UPDATE [FinancialStatementTemplate]
SET [ReportTemplate] = REPLACE([ReportTemplate],
    '{% assign currencySymbol = ''Global'' | Attribute:''CurrencySymbol'' %}',
    '{% comment %} The Global Attribute ''CurrencySymbol'' is deprecated and should not be used. Please use the ''FormatAsCurrency'' Lava filter instead. {% endcomment %}
{% assign currencySymbol = ''Global'' | Attribute:''CurrencySymbol'' %}')
WHERE [Guid]='4B93657A-DD5F-4D8A-A13F-1B4E9ADBDAD0'
    AND [ReportTemplate] NOT LIKE '%The Global Attribute ''CurrencySymbol'' is deprecated%' 

UPDATE [FinancialStatementTemplate]
    SET [ReportTemplate] = REPLACE([ReportTemplate],
        '{{ currencySymbol }}{{ TotalContributionAmount }}',
        '{{ TotalContributionAmount | FormatAsCurrency }}')
    WHERE [Guid]='4B93657A-DD5F-4D8A-A13F-1B4E9ADBDAD0';

UPDATE [FinancialStatementTemplate]
    SET [ReportTemplate] = REPLACE([ReportTemplate],
        '{{ currencySymbol }}{{ transactionDetail.Amount }}',
        '{{ transactionDetail.Amount | FormatAsCurrency }}')
    WHERE [Guid]='4B93657A-DD5F-4D8A-A13F-1B4E9ADBDAD0';

UPDATE [FinancialStatementTemplate]
    SET [ReportTemplate] = REPLACE([ReportTemplate],
        '{{ currencySymbol }}{{ accountsummary.Total }}',
        '{{ accountsummary.Total | FormatAsCurrency }}')
    WHERE [Guid]='4B93657A-DD5F-4D8A-A13F-1B4E9ADBDAD0';

UPDATE [FinancialStatementTemplate]
    SET [ReportTemplate] = REPLACE([ReportTemplate],
        '{{ currencySymbol }}{{ transactionDetailNonCash.Amount }}',
        '{{ transactionDetailNonCash.Amount | FormatAsCurrency }}')
    WHERE [Guid]='4B93657A-DD5F-4D8A-A13F-1B4E9ADBDAD0';

UPDATE [FinancialStatementTemplate]
    SET [ReportTemplate] = REPLACE([ReportTemplate],
        '{{ currencySymbol }}{{ pledge.AmountPledged }}',
        '{{ pledge.AmountPledged | FormatAsCurrency }}')
    WHERE [Guid]='4B93657A-DD5F-4D8A-A13F-1B4E9ADBDAD0';

UPDATE [FinancialStatementTemplate]
    SET [ReportTemplate] = REPLACE([ReportTemplate],
        '{{ currencySymbol }}{{ pledge.AmountGiven }}',
        '{{ pledge.AmountGiven | FormatAsCurrency }}')
    WHERE [Guid]='4B93657A-DD5F-4D8A-A13F-1B4E9ADBDAD0';

UPDATE [FinancialStatementTemplate]
    SET [ReportTemplate] = REPLACE([ReportTemplate],
        '{{ currencySymbol }}{{ pledge.AmountRemaining }}',
        '{{ pledge.AmountRemaining | FormatAsCurrency }}')
    WHERE [Guid]='4B93657A-DD5F-4D8A-A13F-1B4E9ADBDAD0';

" );
        }

        #endregion
    }
}
