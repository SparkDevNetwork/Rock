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
    public partial class Rollup_20211019 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            MigratePlacementGroupsData();
            ConnectionRequestServiceJobUp();
            FixFooterLink();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            ConnectionRequestServiceJobDown();
        }

        /// <summary>
        /// MP - Migrate Placement Groups Data
        /// </summary>
        private void MigratePlacementGroupsData()
        {
            /*

                        Set RelatedEntity.Qualifier to the first matching RegistrationTemplatePlacement                                                                    
                        for 'PLACEMENT' RelatedEntities that have a null Qualifier (due to being 
                        created prior to v12.5).
                        We can make a reasonable guess on which RegistrationTemplatePlacement it is, 
                        based on the RegistrationTemplatePlacement that has 
                        the same GroupTypeId as the Placement Group
                        This will un-disappear pre-12.5 (non-shared) placement groups 
                        into the first matching Placement Type

                        */

            Sql( @" 
                UPDATE RelatedEntity
                SET QualifierValue = (
                        SELECT TOP 1 x.[registrationTemplatePlacement.Id]
                        FROM (
                            SELECT relatedEntity.Id [RelatedEntityId]
                                , registrationTemplatePlacement.Id [registrationTemplatePlacement.Id]
                            FROM RelatedEntity relatedEntity
                            JOIN RegistrationInstance registrationInstance
                                ON registrationInstance.Id = relatedEntity.SourceEntityId
                                    AND relatedEntity.PurposeKey = 'PLACEMENT'
                            JOIN RegistrationTemplatePlacement registrationTemplatePlacement
                                ON registrationTemplatePlacement.RegistrationTemplateId = registrationInstance.RegistrationTemplateId
                            JOIN [GroupType] placementGroupType 
                                ON placementGroupType.Id = registrationTemplatePlacement.GroupTypeId
                            JOIN [Group] placementGroup -- Placement Group
                                ON relatedEntity.TargetEntityId = placementGroup.Id
                                    AND placementGroup.GroupTypeId = placementGroupType.Id
                            ) x
                        WHERE x.RelatedEntityId = RelatedEntity.Id
                        )
                WHERE PurposeKey = 'PLACEMENT'
                    AND QualifierValue IS NULL" );
        }
    
        /// <summary>
        /// SK: Add Connection Requests Automation ServiceJob
        /// </summary>
        private void ConnectionRequestServiceJobUp()
        {
            // add ServiceJob: Connection Requests Automation
            Sql( @"
            IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.ConnectionRequestsAutomation' AND [Guid] = 'D164844D-ACBC-40E1-8D08-5EC61CF811DD' ) 
            BEGIN 
                INSERT INTO [ServiceJob] (
                    [IsSystem],
                    [IsActive],
                    [Name],
                    [Description],
                    [Class],
                    [CronExpression],
                    [NotificationStatus],
                    [Guid] )
                VALUES (
                    0,
                    1,
                    'Connection Requests Automation',
                    'This job will perform routine operations on active connection requests. This includes processing any configured Status Automation rules that are configured on the Connection Type.',
                    'Rock.Jobs.ConnectionRequestsAutomation',
                    '0 0 23 * * ?',
                    1,
                    'D164844D-ACBC-40E1-8D08-5EC61CF811DD' );
            END" );

        }

        /// <summary>
        /// SK: Add Connection Requests Automation ServiceJob
        /// </summary>
        private void ConnectionRequestServiceJobDown()
        {
            // remove ServiceJob: Connection Requests Automation
            Sql( @"IF EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.ConnectionRequestsAutomation' AND [Guid] = 'D164844D-ACBC-40E1-8D08-5EC61CF811DD' ) 
            BEGIN
                DELETE [ServiceJob]  WHERE [Guid] = 'D164844D-ACBC-40E1-8D08-5EC61CF811DD';
            END" );
        }

        /// <summary>
        /// GJ: Fix Footer Link
        /// </summary>
        private void FixFooterLink()
        {
            // Add/Update HtmlContent for Block: Footer Text
            RockMigrationHelper.UpdateHtmlContentBlock("9BBF6F1D-261F-4E95-8652-1C34BD42C1A8",@"<p>Crafted by <a href=""https://www.rockrms.com"" tabindex=""0"">Spark Development Network</a> / <a href=""~/License.aspx"" tabindex=""0"">License</a></p>","0AF1D334-13F8-4799-9158-304B9FDA235F"); 
        }
    }
}
