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
    public partial class AddPrimaryAliasGuidToPerson : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.Person", "PrimaryAliasGuid", c => c.Guid() );

            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_170_UPDATE_PERSON_PRIMARY_PERSON_ALIAS_GUID}'
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
                    , 'Rock Update Helper v17.0 - Update Person PrimaryAliasGuid column.'
                    , 'This job updates all empty PrimaryAliasGuid columns on the Person table with their corresponding PrimaryAliasGuid values.'
                    , 'Rock.Jobs.PostUpdateJobs.PostV17UpdatePersonPrimaryAliasGuid'
                    , '0 0 21 1/1 * ? *'
                    , 1
                    , '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_170_UPDATE_PERSON_PRIMARY_PERSON_ALIAS_GUID}'
                );
            END" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn( "dbo.Person", "PrimaryAliasGuid" );
            Sql( $"DELETE FROM ServiceJob WHERE Guid = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_170_UPDATE_PERSON_PRIMARY_PERSON_ALIAS_GUID}'" );
        }
    }
}
