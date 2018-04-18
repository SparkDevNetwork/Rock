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
    public partial class CommunicationQueueAlertJob : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateSystemEmail( "System", "Communication Queue Notice", "", "", "", "", "", "Alert: Your Rock Communications Are Queuing Up", @"
The following communications have been queued to send for longer than 2 hours...<br/>
<br/>
{% for comm in Communications %}
    <a href=""{{ 'Global' | Attribute:'InternalApplicationRoot' }}Communication/ {{ comm.Id }}"">{{ comm.Subject }}</a> from {{ comm.SenderPersonAlias.Person.FullName }}.<br/>
{% endfor %}
", "2FC7D3E3-D85B-4265-8983-970345215DEA" );

            Sql( @"IF NOT EXISTS(SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.CommunicationQueueAlert')
BEGIN
	INSERT INTO [ServiceJob] (
         [IsSystem]
        ,[IsActive]
        ,[Name]
        ,[Description]
        ,[Class]
        ,[CronExpression]
        ,[NotificationStatus]
        ,[Guid] )
    VALUES (
         0
        ,0
        ,'Communication Queue Alert'
        ,'Sends an email to a list of recipients when there are communications that have been queued to send for longer than a specified time period.'
        ,'Rock.Jobs.CommunicationQueueAlert'
        ,'0 0/15 * 1/1 * ? *'
        ,1
        ,'DFBDD4F9-1DB3-4DE0-8FBA-3B9B613D879D');
END" );

            RockMigrationHelper.AddEntityAttribute( "Rock.Model.ServiceJob", SystemGuid.FieldType.INTEGER, "Class", "Rock.Jobs.CommunicationQueueAlert", "Alert Period", "", "The number of minutes to allow for communications to be sent before sending an alert.", 0, "120", "DE6C5659-B339-4120-B403-95BF66D060DD" );
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.ServiceJob", "08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF", "Class", "Rock.Jobs.CommunicationQueueAlert", "Alert Email", "", "The system email to use for sending an alert", 1, "2fc7d3e3-d85b-4265-8983-970345215dea", "60A996DD-B468-47B5-987A-7683D0D5A51B" );
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.ServiceJob", SystemGuid.FieldType.TEXT, "Class", "Rock.Jobs.CommunicationQueueAlert", "Alert Recipients", "", "A comma-delimited list of recipients that should recieve the alert", 2, "", "EB75248F-A21B-4CFF-8584-DB0E56B63DA5" );

            // DT: Grade Transition Date Fix
            Sql( MigrationSQL._201706141234225_CommunicationQueueAlertJob_ufnCrm_GetGradeOffset );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
    DELETE [ServiceJob] WHERE [Class] = 'Rock.Jobs.CommunicationQueueAlert'
" );
            RockMigrationHelper.DeleteAttribute( "DE6C5659-B339-4120-B403-95BF66D060DD" );
            RockMigrationHelper.DeleteAttribute( "60A996DD-B468-47B5-987A-7683D0D5A51B" );
            RockMigrationHelper.DeleteAttribute( "EB75248F-A21B-4CFF-8584-DB0E56B63DA5" );

            RockMigrationHelper.DeleteSystemEmail( "2FC7D3E3-D85B-4265-8983-970345215DEA" );
        }
    }
}
