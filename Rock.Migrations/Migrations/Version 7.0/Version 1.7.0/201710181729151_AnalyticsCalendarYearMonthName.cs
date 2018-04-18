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
    public partial class AnalyticsCalendarYearMonthName : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.AnalyticsSourceDate", "CalendarYearMonthName", c => c.String(maxLength: 450));

            // Refresh views that are dependent on the AnalyticsSourceDate schema
            Sql( @"
exec sp_refreshview [AnalyticsDimAttendanceDate]
exec sp_refreshview [AnalyticsDimFamilyHeadOfHouseholdBirthDate]
exec sp_refreshview [AnalyticsDimFinancialTransactionDate]
exec sp_refreshview [AnalyticsDimPersonCurrentBirthDate]
exec sp_refreshview [AnalyticsDimPersonHistoricalBirthDate]
" );

            // MP: Job Notification System Email
            RockMigrationHelper.UpdateSystemEmail( "System", "Job Notification", "", "", "", "", "", "Rock Job Notification [{{ Job.LastStatus }}]: {{ Job.Name }}", @"
{{ 'Global' | Attribute:'EmailHeader' }}
<p>Job: {{ Job.Name }}</p>
<p>Last Status: {{ Job.LastStatus }}</p>
{% if Job.LastStatusMessage %}
<p>Last Status Message: {{ Job.LastStatusMessage }}</p>
{% endif %}
<p>Last Run: {{ Job.LastRunDateTime }}</p>
{% if Job.LastRunDateTime != Job.LastSuccessfulRunDateTime %}</p>
<p>Last Successful Run: {{ Job.LastSuccessfulRunDateTime }}</p>
{% endif %}
<p>Last Run Duration (seconds): {{ Job.LastRunDurationSeconds }}</p>
{% if Exception %}
<p>Error:</p>
<p>{{ Exception.Message }}<p> 
{% endif %}
　
{{ 'Global' | Attribute:'EmailFooter' }}
", "691FEA1B-E5C4-4BF8-A7CD-C588F5C63CA8" );

            // MP: Update Subscribe PreHtml
            Sql( @"Update [Block] set PreHtml = 'Subscribe to the lists of interest below along with your delivery preference. Your changes will be saved automatically.' where [Guid] = 'B2CCF6EC-8C07-4B02-9E3A-6D5674050141' and isnull(PreHtml, '') = ''" );

            // MP: Add Subscribe PageRoute
            RockMigrationHelper.AddPageRoute( "0DC2E79D-3590-45E8-A16B-C720A134BA51", "Subscribe" );

            // MP: Add sample  'Public' Communication Lists
            RockMigrationHelper.UpdateCategory( Rock.SystemGuid.EntityType.GROUP, "Public", null, "", "A0889E77-67D9-418C-B301-1B3924692058" );
            // Set Entity Column/Qualifier for the 'Public' category to be for the 'Communication List' grouptype
            Sql( $@"UPDATE [Category]
SET EntityTypeQualifierColumn = 'GroupTypeId'
 , EntityTypeQualifierValue = (
 SELECT TOP 1 Id
 FROM GroupType
 WHERE[Guid] = '{Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST}'
 )
WHERE [Guid] = '{Rock.SystemGuid.Category.GROUPTYPE_COMMUNICATIONLIST_PUBLIC}' " );

            // Add sample 'Public' communication lists
            RockMigrationHelper.UpdateGroup( null, Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST, "Parents of Children", "", null, 0, "64B63392-503E-41EC-93F9-AE6C613B5EF2", false );
            RockMigrationHelper.AddGroupAttributeValue( "64B63392-503E-41EC-93F9-AE6C613B5EF2", Rock.SystemGuid.Attribute.GROUP_COMMUNICATION_LIST_CATEGORY, "A0889E77-67D9-418C-B301-1B3924692058" );
            RockMigrationHelper.UpdateGroup( null, Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST, "Parents of Youth", "", null, 0, "EC99BA98-FEBB-401B-9940-68758A9AC053", false );
            RockMigrationHelper.AddGroupAttributeValue( "EC99BA98-FEBB-401B-9940-68758A9AC053", Rock.SystemGuid.Attribute.GROUP_COMMUNICATION_LIST_CATEGORY, "A0889E77-67D9-418C-B301-1B3924692058" );
            RockMigrationHelper.UpdateGroup( null, Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST, "Sports Ministry", "", null, 0, "6DA2EBDB-D207-4E4A-B427-E63D453E9BA5", false );
            RockMigrationHelper.AddGroupAttributeValue( "6DA2EBDB-D207-4E4A-B427-E63D453E9BA5", Rock.SystemGuid.Attribute.GROUP_COMMUNICATION_LIST_CATEGORY, "A0889E77-67D9-418C-B301-1B3924692058" );

            // MP: Update External Contribution Statement Lava
            // Set Attrib Value for Block:Contribution Statement List Lava if they haven't customized it yet, Attribute:Lava Template Page: Giving History, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( true, "639943D6-75C2-46B4-B044-F4FD7E42E936", "7B554631-3CD5-40C4-8E67-ECED56D4D7C1", @"{% assign currentYear = 'Now' | Date:'yyyy' %}
<h4>Available Contribution Statements</h4>
<div class=""margin-b-md"">
{% for statementyear in StatementYears %}
 {% if currentYear == statementyear.Year %}
 <a href=""{{ DetailPage }}?StatementYear={{ statementyear.Year }}&PersonGuid={{ PersonGuid }}"" class=""btn btn-primary"">{{ statementyear.Year }} <small>YTD</small></a>
 {% else %}
 <a href=""{{ DetailPage }}?StatementYear={{ statementyear.Year }}&PersonGuid={{ PersonGuid }}"" class=""btn btn-primary"">{{ statementyear.Year }}</a>
 {% endif %}
{% endfor %}
</div>" );



        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.AnalyticsSourceDate", "CalendarYearMonthName");
        }
    }
}
