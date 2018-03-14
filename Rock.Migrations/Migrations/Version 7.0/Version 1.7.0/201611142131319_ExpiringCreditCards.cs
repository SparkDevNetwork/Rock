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
    public partial class ExpiringCreditCards : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // add "give" page route
            Sql( @"
    -- Add a 'Give' route if one does not already exist
	IF NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [Route] = 'Give')
	BEGIN
		DECLARE @PageId int
		SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '8BB303AF-743C-49DC-A7FF-CC1236B4B1D9')
		INSERT INTO [PageRoute] (
			[IsSystem],[PageId],[Route],[Guid])
		VALUES(
			1, @PageId, 'Give', '27203DD3-04FE-4607-8249-A301399C01C3' )
	END
" );

            // Add the new "Expiring Credit Card Notice" System Email
            RockMigrationHelper.UpdateSystemEmail( "Finance", "Expiring Credit Card Notice", "", "", "", "", "", "NOTICE: Your card is going to expire soon", @"{{ 'Global' | Attribute:'EmailHeader' }}
<p>
    {{ Person.FirstName }},
</p>
<p>
    This is a courtesy reminder that the credit card (*******{{ Card }}) for your {{ 'Global' | Attribute:'OrganizationName' }}
    gift will expire soon ({{ Expiring }}). In order to prevent an interruption with your recurring gift,
    it is necessary that you update this information accordingly. To update your credit card information go to:
</p>
<p>
    <a href='{{ 'Global' | Attribute:'PublicApplicationRoot' }}Give'>{{ 'Global' | Attribute:'OrganizationWebsite' }}/Give</a>
</p>
<p>
    Thank you for your prompt attention to this matter.
</p>

{{ 'Global' | Attribute:'EmailFooter' }}
", "C07ACD2E-7B9D-400A-810F-BC0EBB9A60DD" );

            // Add the new "Expiring Credit Card Notices" Rock Job
            Sql( @"
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
        ,1
        ,'Expiring Credit Card Notices'
        ,'Checks scheduled transactions for credit cards that are expiring next month and sends an email notice to the person.'
        ,'Rock.Jobs.SendCreditCardExpirationNotices'
        ,'0 30 7 1 1/1 ? *'
        ,1
        ,'F156C063-F080-4AEF-8B2B-27604F2C0E80');
" );

            // Add the job attribute
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.ServiceJob", "08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF", "Class", "Rock.Jobs.SendCreditCardExpirationNotices", "Expiring Credit Card Email", "", "The system email template to use for the credit card expiration notice. The attributes 'Person', 'Card' (the last four digits of the credit card), and 'Expiring' (the MM/YYYY of expiration) will be passed to the email.", 0, "C07ACD2E-7B9D-400A-810F-BC0EBB9A60DD", "074E32E2-99E3-4962-80C3-4025CC934AB1", "ExpiringCreditCardEmail" );

            // Add the job attribute value
            Sql( @"
        DECLARE @JobId int = ( SELECT TOP 1 [Id] FROM [ServiceJob] WHERE [Guid] = 'F156C063-F080-4AEF-8B2B-27604F2C0E80' )
	    DECLARE @AttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '074E32E2-99E3-4962-80C3-4025CC934AB1' )
	    IF @JobId IS NOT NULL AND @AttributeId IS NOT NULL
	    BEGIN
            IF NOT EXISTS ( SELECT [Id] FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @JobId )
            BEGIN
		        INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
		        VALUES ( 0, @AttributeId, @JobId, 'c07acd2e-7b9d-400a-810f-bc0ebb9a60dd', NEWID() )
            END
	    END
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "074E32E2-99E3-4962-80C3-4025CC934AB1" );
            Sql( "DELETE [ServiceJob] WHERE [Guid] = 'F156C063-F080-4AEF-8B2B-27604F2C0E80'" );
            RockMigrationHelper.DeleteSystemEmail( "C07ACD2E-7B9D-400A-810F-BC0EBB9A60DD" );
        }
    }
}
