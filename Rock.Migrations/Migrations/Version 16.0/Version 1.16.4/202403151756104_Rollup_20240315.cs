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
    public partial class Rollup_20240315 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CleanupNameJunkUp();
            UpdatRegistrationTemplateConforimationEmailUp();
            CleanupMigrationHistory();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// NA: Migration Rollup to Cleanup Junk
        /// </summary>
        private void CleanupNameJunkUp()
        {
            Sql( @"
UPDATE [Person] SET [FirstName] = REPLACE(REPLACE([FirstName],' ',''),'<script>','') WHERE [FirstName] LIKE '%<%script%>%'
UPDATE [Person] SET [NickName] = REPLACE(REPLACE([NickName],' ',''),'<script>','') WHERE [NickName] LIKE '%<%script%>%'
UPDATE [Person] SET [LastName] = REPLACE(REPLACE([LastName],' ',''),'<script>','') WHERE [LastName] LIKE '%<%script%>%'
UPDATE [Person] SET [FirstName] = REPLACE(REPLACE([FirstName],' ',''),'</script>','') WHERE [FirstName] LIKE '%<%script%>%'
UPDATE [Person] SET [NickName] = REPLACE(REPLACE([NickName],' ',''),'</script>','') WHERE [NickName] LIKE '%<%script%>%'
UPDATE [Person] SET [LastName] = REPLACE(REPLACE([LastName],' ',''),'</script>','') WHERE [LastName] LIKE '%<%script%>%'
" );
        }

        /// <summary>
        /// PA - Updated Registration Template Confirmation Email to not include Additional Confirmation Details if all registrants are in wait list
        /// </summary>
        private void UpdatRegistrationTemplateConforimationEmailUp()
        {
            // Replace the old Templates to include check for waitlist while getting the count of registrants.
            Sql( @"UPDATE [RegistrationTemplate]
    SET [ConfirmationEmailTemplate] = REPLACE([ConfirmationEmailTemplate],
	    '{% assign registrantCount = Registration.Registrants | Size %}',
	    '{% assign registrantCount = Registration.Registrants | Where:''OnWaitList'', false | Size %}')" );

            // Enclose the AdditionalConfirmationDetails in a p tag for the old templates which originally did not have the tag.
            var confirmationEmailTemplateColumn = RockMigrationHelper.NormalizeColumnCRLF( "ConfirmationEmailTemplate" );
            var additionalConfirmationDetailsWithoutPTag = @"{{ RegistrationInstance.AdditionalConfirmationDetails }}

<p>";
            var additionalConfirmationDetailsWithPTag = @"<p>
    {{ RegistrationInstance.AdditionalConfirmationDetails }}
</p>

<p>";

            Sql( $@"UPDATE [RegistrationTemplate]
    SET [ConfirmationEmailTemplate] =
	    REPLACE({confirmationEmailTemplateColumn},
		    '{additionalConfirmationDetailsWithoutPTag}',
		    '{additionalConfirmationDetailsWithPTag}')" );

            // Add the check to not send the Additional Confirmation Details if all the registrants are in waitlist.
            var additionalConfirmationDetailsWithCheck = @"//- 16.4 fix
{% if registrantCount > 0 %}
    <p>
        {{ RegistrationInstance.AdditionalConfirmationDetails }}
    </p>
{% endif %}

<p>";
            Sql( $@"UPDATE [RegistrationTemplate]
    SET [ConfirmationEmailTemplate] = REPLACE({confirmationEmailTemplateColumn},
		    '{additionalConfirmationDetailsWithPTag}',
		    '{additionalConfirmationDetailsWithCheck}')
    WHERE {confirmationEmailTemplateColumn} NOT LIKE '%//- 16.4 fix%';
" );

            // Update the WaitListTransitionEmailTemplate to include the Additional Confirmation Details if needed.
            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "WaitListTransitionEmailTemplate" );
            var previousTemplate = @"<p>
    If you have any questions please contact {{ RegistrationInstance.ContactPersonAlias.Person.FullName }} at {{ RegistrationInstance.ContactEmail }}.
</p>";
            var newTemplate = @"//- 16.4 fix
{% if AdditionalFieldsNeeded == false and Registration.BalanceDue <= 0 %}
    <p>
        {{ RegistrationInstance.AdditionalConfirmationDetails }}
    </p>
{% endif %}

<p>
    If you have any questions please contact {{ RegistrationInstance.ContactPersonAlias.Person.FullName }} at {{ RegistrationInstance.ContactEmail }}.
</p>";

            Sql( $@"UPDATE [RegistrationTemplate]
    SET [WaitListTransitionEmailTemplate] =
	    REPLACE({targetColumn},
		    '{previousTemplate}',
		    '{newTemplate}')
    WHERE {targetColumn} NOT LIKE '%//- 16.4 fix%'" );

            // Add the migration for older Footer Text of WaitList Transition Email Template.
            previousTemplate = @"<p>
    If you have any questions please contact {{ RegistrationInstance.ContactName }} at {{ RegistrationInstance.ContactEmail }}.
</p>";

            Sql( $@"UPDATE [RegistrationTemplate]
    SET [WaitListTransitionEmailTemplate] =
	    REPLACE({targetColumn},
		    '{previousTemplate}',
		    '{newTemplate}')
    WHERE {targetColumn} NOT LIKE '%//- 16.4 fix%'" );

            // Fix typo in Wait List Transition Email Template.
            Sql( @"UPDATE [RegistrationTemplate]
    SET [WaitListTransitionEmailTemplate] = REPLACE([WaitListTransitionEmailTemplate],
	    'Addition information is needed in order to process this registration',
	    'Additional information is needed in order to process this registration')" );
        }

        /// <summary>
        /// Cleanups the migration history records except the last one.
        /// </summary>
        private void CleanupMigrationHistory()
        {
            Sql( @"
UPDATE [dbo].[__MigrationHistory]
SET [Model] = 0x
WHERE MigrationId < (SELECT TOP 1 MigrationId FROM __MigrationHistory ORDER BY MigrationId DESC)" );
        }
    }
}
