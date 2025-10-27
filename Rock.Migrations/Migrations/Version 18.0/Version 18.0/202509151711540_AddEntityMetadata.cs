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
    /// <summary>
    ///
    /// </summary>
    public partial class AddEntityMetadata : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.EntityMetadata",
                c => new
                {
                    EntityTypeId = c.Int( nullable: false ),
                    EntityId = c.Int( nullable: false ),
                    Key = c.String( nullable: false, maxLength: 100 ),
                    Value = c.String( nullable: false ),
                } )
                .PrimaryKey( t => new { t.EntityTypeId, t.EntityId, t.Key } )
                .ForeignKey( "dbo.EntityType", t => t.EntityTypeId, cascadeDelete: true );

            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", Rock.SystemGuid.FieldType.SINGLE_SELECT, "Class", "Rock.Jobs.UpdateEntityUsage", "Usage Type", "Usage Type", "The type of usage metrics to calculate.", 0, string.Empty, "ac305916-98bf-438b-8795-3812e610ce8f", "UsageType" );

            var jobClass = "Rock.Jobs.UpdateEntityUsage";
            var cronSchedule = "0 0 3 1/1 * ? *"; // 3:00am daily.

            Sql( $@"
            IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = '{jobClass}' AND [Guid] = '{SystemGuid.ServiceJob.UPDATE_MEDIA_ELEMENT_USAGE}' )
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
                    'Update Media Element Usage',
                    'Job that update the usage information of Media Elements by other entities.',
                    '{jobClass}',
                    '{cronSchedule}',
                    1,
                    '{SystemGuid.ServiceJob.UPDATE_MEDIA_ELEMENT_USAGE}' );
            END
            ELSE
            BEGIN
	            UPDATE	[ServiceJob]
	            SET
		              [IsSystem] = 1
		            , [IsActive] = 1
		            , [Name] = 'Update Media Element Usage'
		            , [Description] = 'Job that update the usage information of Media Elements by other entities.'
		            , [Class] = '{jobClass}'
		            , [CronExpression] = '{cronSchedule}'
		            , [NotificationStatus] = 1
	            WHERE
		              [Guid] = '{SystemGuid.ServiceJob.UPDATE_MEDIA_ELEMENT_USAGE}';
            END" );

            RockMigrationHelper.AddServiceJobAttributeValue( SystemGuid.ServiceJob.UPDATE_MEDIA_ELEMENT_USAGE, "ac305916-98bf-438b-8795-3812e610ce8f", "0" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( $"DELETE FROM [ServiceJob] WHERE [Guid] = '{SystemGuid.ServiceJob.UPDATE_MEDIA_ELEMENT_USAGE}'" );
            RockMigrationHelper.DeleteAttribute( "ac305916-98bf-438b-8795-3812e610ce8f" );

            DropTable( "dbo.EntityMetadata" );
        }
    }
}
