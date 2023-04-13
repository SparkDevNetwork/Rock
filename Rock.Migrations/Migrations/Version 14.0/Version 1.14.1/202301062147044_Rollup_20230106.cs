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
    public partial class Rollup_20230106 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AutoAcceptGroupScheduleRequests();
            RecreateMetricAnalyticsViews();
            UpdateAnalyticsRelatedStoredProcedures();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// SK: Auto-Accept Group Schedule Requests
        /// </summary>
        private void AutoAcceptGroupScheduleRequests()
        {
            var previousEmailTemplate = @"<!--[if mso]><v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}ScheduleConfirmation?attendanceIds={{attendance.Id}}&Person={{AttendancePerson | PersonActionIdentifier:'ScheduleConfirm'}}&isConfirmed=true"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""5%"" strokecolor=""#339933"" fillcolor=""#669966"">
    			<w:anchorlock/>
    			<center style=""color:#ffffff;font-family:sans-serif;font-size:18px;font-weight:normal;"">{{ acceptText }}</center>
    		  </v:roundrect>
    		<![endif]--><a style=""mso-hide:all; background-color:#669966;border:1px solid #339933;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:18px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"" href=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}ScheduleConfirmation?attendanceIds={{attendanceIdList | UrlEncode}}&Person={{AttendancePerson| PersonActionIdentifier:'ScheduleConfirm'}}&isConfirmed=true"">{{ acceptText }}</a>&nbsp;";
            var newEmailTemplate = @"{% if scheduleConfirmationLogic != 'AutoAccept' %}" + previousEmailTemplate + "{% endif %}";
            newEmailTemplate = newEmailTemplate.Replace( "'", "''" );
            previousEmailTemplate = previousEmailTemplate.Replace( "'", "''" );

            // Use NormalizeColumnCRLF when attempting to do a WHERE clause or REPLACE using multi line strings!
            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "Body" );

            Sql( $@"UPDATE [dbo].[SystemCommunication] 
                    SET [Body] = REPLACE({targetColumn}, '{previousEmailTemplate}', '{newEmailTemplate}')
                    WHERE {targetColumn} LIKE '%{previousEmailTemplate.Replace( "[", @"\[" ).Replace( "]", @"\]" )}%'
                           escape '\' AND [Guid] = 'F8E4CE07-68F5-4169-A865-ECE915CF421C'" );

            previousEmailTemplate = @"{% for attendance in Attendances %}

  {% assign currentDate = attendance.Occurrence.OccurrenceDate | Date:'dddd, MMMM d, yyyy' %}";
            newEmailTemplate = @"{% assign scheduleConfirmationLogic = 'AutoAccept' %}
{% for attendance in Attendances %}
{% if attendance.RSVP != 'Yes' and attendance.Occurrence.Group.GroupType.ScheduleConfirmationLogic == 'Ask' and attendance.Occurrence.Group.ScheduleConfirmationLogic == null %}
    {% assign scheduleConfirmationLogic = 'Ask' %}
{% elseif attendance.RSVP != 'Yes' and attendance.Occurrence.Group.ScheduleConfirmationLogic == 'Ask' %} 
  {% assign scheduleConfirmationLogic = 'Ask' %}
{% endif %}

{% assign currentDate = attendance.Occurrence.OccurrenceDate | Date:'dddd, MMMM d, yyyy' %}";
            newEmailTemplate = newEmailTemplate.Replace( "'", "''" );
            previousEmailTemplate = previousEmailTemplate.Replace( "'", "''" );

            // Use NormalizeColumnCRLF when attempting to do a WHERE clause or REPLACE using multi line strings!
            targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "Body" );

            Sql( $@"UPDATE [dbo].[SystemCommunication] 
                    SET [Body] = REPLACE({targetColumn}, '{previousEmailTemplate}', '{newEmailTemplate}')
                    WHERE {targetColumn} LIKE '%{previousEmailTemplate.Replace( "[", @"\[" ).Replace( "]", @"\]" )}%'
                           escape '\' AND [Guid] = 'F8E4CE07-68F5-4169-A865-ECE915CF421C'" );

            Sql( $@"UPDATE [dbo].[SystemCommunication] 
                    SET [Body] = REPLACE([Body], 'attendanceId=', 'attendanceIds=')
                    WHERE [Guid] = 'F8E4CE07-68F5-4169-A865-ECE915CF421C'" );
        }

        /// <summary>
        /// SK: Add  job to recreate metric analytics views
        /// </summary>
        private void RecreateMetricAnalyticsViews()
        {
            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Class] = 'Rock.Jobs.PostV141RecreateMetricAnalyticsViews'
                    AND [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_141_RECREATE_METRIC_ANALYTICS_VIEWS}'
            )
            BEGIN
                INSERT INTO [ServiceJob] (
                      [IsSystem]
                    , [IsActive]
                    , [Name]
                    , [Description]
                    , [Class]
                    , [CronExpression]
                    , [NotificationStatus]
                    , [Guid]
                ) VALUES (
                      1
                    , 1
                    , 'Rock Update Helper v14.1 - Recreate Metric Analytics Views'
                    , 'Recreate the views for Metric Analytics.'
                    , 'Rock.Jobs.PostV141RecreateMetricAnalyticsViews'
                    , '0 0 21 1/1 * ? *'
                    , 1
                    , '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_141_RECREATE_METRIC_ANALYTICS_VIEWS}'
                );
            END" );
        }

        /// <summary>
        /// SK: Update Analytics related Stored Procedures to populate AttributeValue ValueAs* columns
        /// </summary>
        private void UpdateAnalyticsRelatedStoredProcedures()
        {
            Sql( MigrationSQL._202301062147044_Rollup_20230106_UpdateAnalyticsStoredProcedure_spCrm_FamilyAnalyticsAttendance );
            Sql( MigrationSQL._202301062147044_Rollup_20230106_UpdateAnalyticsStoredProcedure_spCrm_FamilyAnalyticsEraDataset );
            Sql( MigrationSQL._202301062147044_Rollup_20230106_UpdateAnalyticsStoredProcedure_spCrm_FamilyAnalyticsGiving );
            Sql( MigrationSQL._202301062147044_Rollup_20230106_UpdateAnalyticsStoredProcedure_spCrm_FamilyAnalyticsUpdateVisitDates );
        }

        private void UpdateAppleDeviceDefinedValues()
        {

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad13,18", "iPad 10th Gen", "", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad13,19", "iPad 10th Gen", "", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone14,3", "iPhone 13 Pro Max", "50655253-B65A-4CDA-8B82-77D547F0EBEA", true );
            RockMigrationHelper.DeleteDefinedValue( "22C1DA23-C126-4C2C-8142-DB313453151C" );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPod7,1", "6th Gen iPod", "AF883B41-ED81-44C7-A69E-3D4BDD9BEF71", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch1,2", "Apple Watch 42mm case", "B05F49EC-2026-466C-9F4C-2779A57715AB", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch5,10", "Apple Watch SE 44mm case (GPS)", "1EEEFC4E-3BDD-4852-84D6-552B9B512C7A", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch5,11", "Apple Watch SE 40mm case (GPS+Cellular)", "9F574260-D240-408D-94A7-E72BAD1ABE74", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch5,12", "Apple Watch SE 44mm case (GPS+Cellular)", "BED75931-A35A-49E6-BF66-E8F8FFAE3864", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch5,9", "Apple Watch SE 40mm case (GPS)", "EFFF3BAD-58D6-40FF-A8EA-B66F31087CF5", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,1", "Apple Watch Series 6 40mm case (GPS)", "072EB602-236A-4EF8-907D-38709873E463", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,2", "Apple Watch Series 6 44mm case (GPS)", "55961288-307F-40CB-A388-09C033F71E03", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,3", "Apple Watch Series 6 40mm case (GPS+Cellular)", "854BBB1E-F2B2-4224-92BB-7AD9296F4858", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,4", "Apple Watch Series 6 44mm case (GPS+Cellular)", "C66C9680-C2AF-4841-AA69-224054E8D0EC", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,6", "Apple Watch Series 7 41mm case (GPS)", "82BDAEB1-E2CE-4A2E-ADCE-98EA1148277F", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,7", "Apple Watch Series 7 45mm case (GPS)", "A2F46F18-FB52-4685-9953-E84E46873DB6", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,8", "Apple Watch Series 7 41mm case (GPS+Cellular)", "38C5B568-63BE-486D-BCE4-19183DB15EDA", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,9", "Apple Watch Series 7 45mm case (GPS+Cellular)", "8DCF48E6-00E4-446D-A4D7-30236D95AF92", true );

            Sql( @"
                DECLARE @AppleDeviceDefinedTypeId INT = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = 'DAE31F78-7AB9-4ACE-9EE1-C1E6A734562C')
                
                -- iPod6,1 was removed. iPod7,1 is the 6th Gen iPod. Replace where neccessary. The description for already updated rows is already correct.
                UPDATE [PersonalDevice]
                SET [Model] = 'iPod7,1'
                WHERE pd.[Manufacturer] = 'Apple'
                  AND pd.[Model] = 'iPod6,1'
                  AND dv.DefinedTypeId = @AppleDeviceDefinedTypeId

                UPDATE [PersonalDevice]
                SET [Model] = dv.[Description]
                FROM [PersonalDevice] pd
                JOIN [DefinedValue] dv ON pd.[Model] = dv.[Value]
                WHERE pd.[Manufacturer] = 'Apple'
                  AND pd.[Model] like '%,%'
                  AND dv.DefinedTypeId = @AppleDeviceDefinedTypeId" );
        }
    }
}
