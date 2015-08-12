using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock;
using Rock.Plugin;
using Rock.Web.Cache;
namespace com.centralaz.Accountability.Migrations
{
    [MigrationNumber( 4, "1.0.14" )]
    public class AddAccountabilityJob : Migration
    {
        //
        public override void Up()
        {
            //Add Email Template
            Sql( @"
            DECLARE @EmailMediumId int = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '5A653EBE-6803-44B4-85D2-FB7B8146D55D')
            DELETE [CommunicationTemplate] WHERE [Guid] = '28EE785A-D335-42F8-9875-1B867A38FCB1'
            INSERT [dbo].[CommunicationTemplate]
                ([Name]
                ,[Description]
                ,[Subject]
                ,[MediumEntityTypeId]
                ,[MediumDataJson]
                ,[CreatedDateTime]
                ,[ModifiedDateTime]
                ,[Guid] )
            VALUES
                (N'Accountability Report Reminder'
                ,NULL
                ,N'Reminder: Submit your report for {{GroupName}}!'
                ,@EmailMediumId
                ,N'{ ""HtmlMessage"": ""<p>{{ GlobalAttribute.EmailHeader }}</p>\n\n<p>{{ Person.FirstName }},</p>\n\n<p>A report is due today for {{GroupName}}. Please go to the URL below to submit it.<br />\n<a href=''{{GlobalAttribute.PublicApplicationRoot}}{{ReportPageUrl}}''>Submit Report</a></p>\n\n<p>Thank-you,<br />\n{{ GlobalAttribute.OrganizationName }}</p>\n\n<p>{{ GlobalAttribute.EmailFooter }}</p>\n""}'
                ,CAST(0x0000A37C01572215 AS DateTime)
                ,CAST(0x0000A38500F00EE8 AS DateTime)
                ,N'28EE785A-D335-42F8-9875-1B867A38FCB1' )
" );

            //Add System Job
            Sql( @"
            DELETE [ServiceJob] WHERE [Guid] = 'AE6D753D-7B04-4C52-8BC0-3E0A9FF3803E'
            INSERT [dbo].[ServiceJob]
                  ([IsSystem]
                  ,[IsActive]
                  ,[Name]
                  ,[Class]
                  ,[CronExpression]
                  ,[NotificationStatus]
                  ,[Guid] )
            VALUES
                (0
                ,1
                ,N'Accountability Report Reminder'
                ,N'com.centralaz.Accountability.Jobs.SendAccountabilityReportReminder'
                ,N'0 0 7 1/1 * ? *'
                ,1
                ,N'AE6D753D-7B04-4C52-8BC0-3E0A9FF3803E')
" );

            //Add email template attribute
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.ServiceJob", "C3B37465-DCAF-4C8C-930C-9A9B5D066CA9", "Class", "com.centralaz.Accountability.Jobs.SendAccountabilityReportReminder", "Template", null, "", 0, null, "46AD6913-9FD7-4179-8E1B-90F746915B94" );
            RockMigrationHelper.AddAttributeValue( "46AD6913-9FD7-4179-8E1B-90F746915B94", 15, "28ee785a-d335-42f8-9875-1b867a38fcb1", "ACEF48A8-FC26-49E3-9DFA-DC8E2FF96236" );
        }

        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "46AD6913-9FD7-4179-8E1B-90F746915B94" );
            Sql( @"
                DELETE [CommunicationTemplate] WHERE [Guid] = '28EE785A-D335-42F8-9875-1B867A38FCB1'
                DELETE [ServiceJob] WHERE [Guid] = 'AE6D753D-7B04-4C52-8BC0-3E0A9FF3803E'
                " );
        }
    }
}