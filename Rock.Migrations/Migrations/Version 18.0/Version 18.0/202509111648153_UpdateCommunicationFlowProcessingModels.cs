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
    public partial class UpdateCommunicationFlowProcessingModels : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            SchemaChangesUp();
            AddFieldTypeUp();
            UpdateMetricsPageRoutesUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            UpdateMetricsPageRoutesDown();
            SchemaChangesDown();
        }

        private void SchemaChangesUp()
        {
            AddColumn( "dbo.CommunicationFlow", "IsMessagingClosed", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.CommunicationFlow", "IsConversionGoalTrackingClosed", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.CommunicationFlowInstance", "IsMessagingCompleted", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.CommunicationFlowInstance", "IsConversionGoalTrackingCompleted", c => c.Boolean( nullable: false ) );
        }

        private void SchemaChangesDown()
        {
            DropColumn( "dbo.CommunicationFlowInstance", "IsConversionGoalTrackingCompleted" );
            DropColumn( "dbo.CommunicationFlowInstance", "IsMessagingCompleted" );
            DropColumn( "dbo.CommunicationFlow", "IsConversionGoalTrackingClosed" );
            DropColumn( "dbo.CommunicationFlow", "IsMessagingClosed" );
        }

        private void AddFieldTypeUp()
        {
            RockMigrationHelper.UpdateFieldType( "Communication Flow", "", "Rock", "Rock.Field.Types.CommunicationFlowFieldType", "DD16B493-2588-436A-8C99-8771206ED28D" );
        }

        private void UpdateMetricsPageRoutesUp()
        {
            // Attribute for BlockType
            //   BlockType: Communication Flow Performance
            //   Category: Communication
            //   Attribute: Message Metrics Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "72B92AA4-2AA7-4FDD-9CD7-7DD84018B21E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Message Metrics Page", "MessageMetricsPage", "Message Metrics Page", @"The page that will show the communication flow instance message metrics.", 0, @"", "F466A748-9917-4A61-9B45-42821E2DCA6C" );

            RockMigrationHelper.AddOrUpdatePageRoute(
                "C70E547B-F058-4CDD-9A97-55A2D1F8650A",
                "CommunicationFlows/{CommunicationFlow}/Templates/{CommunicationFlowCommunication}/Messages/Metrics",
                "BEDF81F7-27D1-4058-B12A-294A7ECCDAE6"
            );

            // Add Block Attribute Value
            //   Block: Communication Flow Performance
            //   BlockType: Communication Flow Performance
            //   Category: Communication
            //   Block Location: Page=Communication Flow Performance, Site=Rock RMS
            //   Attribute: Message Metrics Page
            /*   Attribute Value: c70e547b-f058-4cdd-9a97-55a2d1f8650a */
            //   Skip If Already Exists: false
            var skipIfAlreadyExists = false;
            RockMigrationHelper.AddBlockAttributeValue( skipIfAlreadyExists, "337770AE-6D2B-4C05-B8C1-125896CEB06A", "F466A748-9917-4A61-9B45-42821E2DCA6C", @"c70e547b-f058-4cdd-9a97-55a2d1f8650a" );
        }

        private void UpdateMetricsPageRoutesDown()
        {
            RockMigrationHelper.DeletePageRoute( "BEDF81F7-27D1-4058-B12A-294A7ECCDAE6" );

            // Add Block Attribute Value
            //   Block: Communication Flow Performance
            //   BlockType: Communication Flow Performance
            //   Category: Communication
            //   Block Location: Page=Communication Flow Performance, Site=Rock RMS
            //   Attribute: Message Metrics Page
            /*   Attribute Value: c70e547b-f058-4cdd-9a97-55a2d1f8650a,f031eafe-959b-4ccc-897b-290784689fd1 */
            //   Skip If Already Exists: false
            var skipIfAlreadyExists = false;
            RockMigrationHelper.AddBlockAttributeValue( skipIfAlreadyExists, "337770AE-6D2B-4C05-B8C1-125896CEB06A", "F466A748-9917-4A61-9B45-42821E2DCA6C", @"c70e547b-f058-4cdd-9a97-55a2d1f8650a,f031eafe-959b-4ccc-897b-290784689fd1" );
        }
    }
}
