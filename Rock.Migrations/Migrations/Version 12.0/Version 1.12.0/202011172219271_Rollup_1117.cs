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
    public partial class Rollup_1117 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateAccountConfirmationCommunicationUp();
            PersonEntryHideIfCurrentPersonKnown();
            CleanupMigrationHistory();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            UpdateAccountConfirmationCommunicationDown();
        }

        /// <summary>
        /// SK: Update Account Confirmation system communication
        /// Updates the account confirmation communication to show more details about the user login such as when it was created and show a clear button for deleting it.
        /// </summary>
        private void UpdateAccountConfirmationCommunicationUp()
        {
            string oldTemplateValue = @"Thank you for creating an account at {{ 'Global' | Attribute:'OrganizationName' }}. Please <a href='{{ ConfirmAccountUrl }}?cc={{ User.ConfirmationCodeEncoded }}&action=confirm'>confirm</a> that you are the owner of this email address.<br/><br/>If you did not create this account, you can <a href='{{ ConfirmAccountUrl }}?cc={{ User.ConfirmationCodeEncoded }}&action=delete'>Delete It</a>.<br/><br/>If the above links do not work, you can also go to {{ ConfirmAccountUrl }} and enter the following confirmation code:<br/>{{ User.ConfirmationCode }}<br/><br/>".Replace( "'", "''" );

            string newTemplateValue = @"Your account was recently updated and needs to be confirmed for security purposes. Please confirm that the login below is one that you created and have recently attempted to login with.<br/><br/>
<strong>UserName:</strong>{{ User.UserName }}<br/>
<strong>Created:</strong>{{ User.CreatedDateTime | Date:'dddd, MMMM d, yyyy @ h:mm tt' }}<br/><br/>
<table class='confirm-outerwrap' border='0' cellpadding='0' width='100%' style='min-width:100%;'>
	<tbody>
		<tr>
			<td style='padding-top:0; padding-right:0; padding-bottom:0; padding-left:0;' valign='top' align='center'>
				<table border='0' cellpadding='0' cellspacing='0'>
					<tr>
						<td>
							<table border='0' cellpadding='0' cellspacing='0' style='display: inline-table; border-collapse: separate !important; border-radius: 3px; background-color: #16C98D;'>
								<tbody>
									<tr>
										<td style='font-family: Arial; font-size: 16px; padding: 15px;'>
											<a title='Confirm Account' href='{{ ConfirmAccountUrl }}?cc={{ User.ConfirmationCodeEncoded }}&action=confirm' target='_blank' style='font-weight: bold; letter-spacing: normal; line-height: 100%; text-align: center; text-decoration: none; color: #FFFFFF;'>Confirm Account</a>
										</td>
									</tr>
								</tbody>
							</table>
						</td>
						<td style='padding-left: 10px;'>
							<table border='0' cellpadding='0' cellspacing='0' class='decline-button-shell' style='display: inline-table; border-collapse: separate !important; border-radius: 3px; background-color: #D4442E;'>
								<tbody>
									<tr>
										<td  style='font-family: Arial; font-size: 16px; padding: 15px;'>
											<a title='Delete Account' href='{{ ConfirmAccountUrl }}?cc={{ User.ConfirmationCodeEncoded }}&action=delete' target='_blank' style='font-weight: bold; letter-spacing: normal; line-height: 100%; text-align: center; text-decoration: none; color: #FFFFFF;'>Delete Account</a>
										</td>
									</tr>
								</tbody>
							</table>
						</td>
					</tr>
				</table>
			</td>
		</tr>
	</tbody>
</table><br/><br/><!--Endv12_20201117-->".Replace( "'", "''" );


            // Use NormalizeColumnCRLF when attempting to do a WHERE clause or REPLACE using multi line strings!
            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "Body" );
            Sql( $@"UPDATE
                        [dbo].[SystemCommunication] 
                    SET [Body] = REPLACE({targetColumn}, '{oldTemplateValue}', '{newTemplateValue}')
                    WHERE 
                        {targetColumn} LIKE '%{oldTemplateValue}%'
                        AND [Guid] = '17aaceef-15ca-4c30-9a3a-11e6cf7e6411'" );
        }

        /// <summary>
        /// SK: Update Account Confirmation system communication
        /// Updates the account confirmation communication to show more details about the user login such as when it was created and show a clear button for deleting it.
        /// </summary>
        private void UpdateAccountConfirmationCommunicationDown()
        {
            string newTemplateValue = @"{{ 'Global' | Attribute:'EmailHeader' }}
{{ Person.FirstName }},<br/><br/>

Thank you for creating an account at {{ 'Global' | Attribute:'OrganizationName' }}. Please <a href='{{ ConfirmAccountUrl }}?cc={{ User.ConfirmationCodeEncoded }}&action=confirm'>confirm</a> that you are the owner of this email address.<br/><br/>If you did not create this account, you can <a href='{{ ConfirmAccountUrl }}?cc={{ User.ConfirmationCodeEncoded }}&action=delete'>Delete It</a>.<br/><br/>If the above links do not work, you can also go to {{ ConfirmAccountUrl }} and enter the following confirmation code:<br/>{{ User.ConfirmationCode }}<br/><br/>

Thank you,<br/>
{{ 'Global' | Attribute:'OrganizationName' }}  

{{ 'Global' | Attribute:'EmailFooter' }}
".Replace( "'", "''" );

            string oldTemplateValue = @"Your account was recently updated and needs to be confirmed for security purposes. Please confirm that the login below is one that you created and have recently attempted to login with.<br/><br/>%<strong>UserName:</strong>{{ User.UserName }}<br/>%<!--Endv12_20201117-->".Replace( "'", "''" );

            // Use NormalizeColumnCRLF when attempting to do a WHERE clause or REPLACE using multi line strings!
            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "Body" );
            Sql( $@"UPDATE
                        [dbo].[SystemCommunication] 
                    SET [Body] = '{newTemplateValue}'
                    WHERE 
                        {targetColumn} LIKE '%{oldTemplateValue}%'
                        AND [Guid] = '17aaceef-15ca-4c30-9a3a-11e6cf7e6411'" );
        }


        /// <summary>
        /// MP: WorkflowActionForm PersonEntryHideIfCurrentPersonKnown default
        /// </summary>
        private void PersonEntryHideIfCurrentPersonKnown()
        {
            // Drop the default constraint on WorkflowActionForm.PersonEntryHideIfCurrentPersonKnown if an older version of the 202011052358368_WorkflowActionFormAllowPersonEntry and 202011121849471_WorkflowActionFormChanges migration was run
            Sql( @"
                DECLARE @Sql NVARCHAR(max)
                    ,@constraintName NVARCHAR(max) = (
                        SELECT TOP 1 obj_Constraint.NAME AS 'constraint'
                        FROM sys.objects obj_table
                        JOIN sys.objects obj_Constraint ON obj_table.object_id = obj_Constraint.parent_object_id
                        JOIN sys.sysconstraints constraints ON constraints.constid = obj_Constraint.object_id
                        JOIN sys.columns columns ON columns.object_id = obj_table.object_id AND columns.column_id = constraints.colid
                        WHERE obj_table.NAME = 'WorkflowActionForm' AND columns.name = 'PersonEntryHideIfCurrentPersonKnown' AND obj_Constraint.type = 'D'
                        )

                SELECT @constraintName

                IF @constraintName IS NOT NULL
                BEGIN
                    SET @Sql = CONCAT (
                            'ALTER TABLE [dbo].[WorkflowActionForm] DROP CONSTRAINT ['
                            ,@constraintName
                            ,']'
                            )

                    EXECUTE (@Sql)

                    UPDATE WorkflowActionForm
                    SET PersonEntryHideIfCurrentPersonKnown = 0
                    WHERE AllowPersonEntry = 0 AND PersonEntryHideIfCurrentPersonKnown = 1
                END" );

                            // recreate the constraint with a default of 0 (false)
            Sql( @"ALTER TABLE [WorkflowActionForm]
                ADD CONSTRAINT [df_PersonEntryHideIfCurrentPersonKnown]
                DEFAULT 0 FOR [PersonEntryHideIfCurrentPersonKnown];" );
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
