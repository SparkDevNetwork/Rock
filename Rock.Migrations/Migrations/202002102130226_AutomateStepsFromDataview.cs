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
    public partial class AutomateStepsFromDataview : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( $@"
                INSERT INTO ServiceJob (
                    IsSystem,
                    IsActive,
                    Name,
                    Description,
                    Class,
                    Guid,
                    CreatedDateTime,
                    NotificationStatus,
                    CronExpression
                ) VALUES (
                    1, -- IsSystem
                    1, -- IsActive
                    'Steps Automation', -- Name
                    'Creates steps for people within a dataview', -- Description
                    'Rock.Jobs.StepsAutomation', -- Class
                    '{Rock.SystemGuid.ServiceJob.STEPS_AUTOMATION}', -- Guid
                    GETDATE(), -- Created
                    1, -- All notifications
                    '0 0 4 1/1 * ? *' -- At 4:00 am
                );" );

            UpdateAttribute(
                "Duplicate Prevention Day Range",
                description: "If duplicates are enabled above, this setting will keep steps from being added if a previous step was within the number of days provided.",
                defaultValue: 7.ToString(),
                order: 1,
                key: "DuplicatePreventionDayRange",
                fieldTypeGuid: Rock.SystemGuid.FieldType.INTEGER,
                guid: "87144EF1-F902-4293-A416-06E09D4963AE" );
    }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "87144EF1-F902-4293-A416-06E09D4963AE" );
            Sql( $"DELETE FROM ServiceJob WHERE Guid = '{Rock.SystemGuid.ServiceJob.STEPS_AUTOMATION}';" );
        }

        /// <summary>
        /// Add an attribute for the AutomateStepsFromDataview job
        /// </summary>
        /// <param name="fieldTypeGuid"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="order"></param>
        /// <param name="defaultValue"></param>
        /// <param name="guid"></param>
        /// <param name="key"></param>
        private void UpdateAttribute( string name, string fieldTypeGuid, string description, int order, string guid, string key, string defaultValue = "", bool isRequired = false )
        {
            RockMigrationHelper.UpdateEntityAttribute( 
                "Rock.Model.ServiceJob",
                fieldTypeGuid, 
                "Class", 
                "Rock.Jobs.StepsAutomation", 
                name,
                description, 
                order,
                defaultValue, 
                guid,
                key,
                isRequired );
        }
    }
}
