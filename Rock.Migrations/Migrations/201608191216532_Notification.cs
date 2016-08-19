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
    public partial class Notification : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.Notification",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 100),
                        Message = c.String(nullable: false),
                        SentDateTime = c.DateTime(nullable: false),
                        IconCssClass = c.String(maxLength: 100),
                        Classification = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId)
                .Index(t => t.ForeignGuid)
                .Index(t => t.ForeignKey);
            
            CreateTable(
                "dbo.NotificationRecipient",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        NotificationId = c.Int(nullable: false),
                        PersonAliasId = c.Int(nullable: false),
                        Read = c.Boolean(nullable: false),
                        ReadDateTime = c.DateTime(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.Notification", t => t.NotificationId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.PersonAliasId)
                .Index(t => t.NotificationId)
                .Index(t => t.PersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId)
                .Index(t => t.ForeignGuid)
                .Index(t => t.ForeignKey);

            RockMigrationHelper.UpdateBlockType( "Notification List", "Displays notifications from the Spark Link.", "~/Blocks/Administration/NotificationList.ascx", "Utility", "9C0FD17D-677D-4A37-A61F-54C370954E83" );
            // Add Block to Page: Internal Homepage, Site: Rock RMS            
            RockMigrationHelper.AddBlock( "20F97A93-7949-4C2A-8A5E-C756FE8585CA", "", "9C0FD17D-677D-4A37-A61F-54C370954E83", "Notification List", "Main", "", "", 0, "60469A41-5180-446F-9935-0A09D81CD319" );
            // update block order for pages with new blocks if the page,zone has multiple blocks

            Sql( @"
    DECLARE @PageId int = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '20F97A93-7949-4C2A-8A5E-C756FE8585CA' )
    UPDATE [Block] SET [Order] = [Order] + 1
    WHERE [PageId] = @PageId
    AND [Guid] <> '60469A41-5180-446F-9935-0A09D81CD319'
" );

            Random rnd = new Random();
            int minute = rnd.Next( 0, 60 );

            string insertJob = @"
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
        ,'Spark Link'
        ,'Fetches Rock notifications from the Spark Development Network'
        ,'Rock.Jobs.SparkLink'
        ,'0 {0} 0/7 1/1 * ? *'
        ,1
        ,'645b1230-0c53-4fe3-91e2-8601ff00cbb5');
";
            Sql( string.Format( insertJob, minute ) );

            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "Class", "Rock.Jobs.SparkLink", "Notification Group", "The group that should receive incoming notifications.", 0, "628C51A8-4613-43ED-A18D-4A6FB999273E", "62472CB9-6807-4759-9FD9-66A07EF921D2" );
            Sql( @"
        DECLARE @JobId int = ( SELECT TOP 1 [Id] FROM [ServiceJob] WHERE [Guid] = '645b1230-0c53-4fe3-91e2-8601ff00cbb5' )
	    DECLARE @AttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '62472CB9-6807-4759-9FD9-66A07EF921D2' )
	    IF @JobId IS NOT NULL AND @AttributeId IS NOT NULL
	    BEGIN
            IF NOT EXISTS ( SELECT [Id] FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @JobId )
            BEGIN
		        INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
		        VALUES ( 0, @AttributeId, @JobId, '628c51a8-4613-43ed-a18d-4a6fb999273e', NEWID() )
            END
	    END
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Notification List, from Page: Internal Homepage, Site: Rock RMS       
            RockMigrationHelper.DeleteBlock( "60469A41-5180-446F-9935-0A09D81CD319" );
            RockMigrationHelper.DeleteBlockType( "9C0FD17D-677D-4A37-A61F-54C370954E83" ); // Notification List

            // Delete Job
            Sql( "DELETE FROM [ServiceJob] WHERE [Guid]='645b1230-0c53-4fe3-91e2-8601ff00cbb5'" );

            DropForeignKey("dbo.NotificationRecipient", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.NotificationRecipient", "NotificationId", "dbo.Notification");
            DropForeignKey("dbo.NotificationRecipient", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.NotificationRecipient", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Notification", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Notification", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.NotificationRecipient", new[] { "ForeignKey" });
            DropIndex("dbo.NotificationRecipient", new[] { "ForeignGuid" });
            DropIndex("dbo.NotificationRecipient", new[] { "ForeignId" });
            DropIndex("dbo.NotificationRecipient", new[] { "Guid" });
            DropIndex("dbo.NotificationRecipient", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.NotificationRecipient", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.NotificationRecipient", new[] { "PersonAliasId" });
            DropIndex("dbo.NotificationRecipient", new[] { "NotificationId" });
            DropIndex("dbo.Notification", new[] { "ForeignKey" });
            DropIndex("dbo.Notification", new[] { "ForeignGuid" });
            DropIndex("dbo.Notification", new[] { "ForeignId" });
            DropIndex("dbo.Notification", new[] { "Guid" });
            DropIndex("dbo.Notification", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.Notification", new[] { "CreatedByPersonAliasId" });
            DropTable("dbo.NotificationRecipient");
            DropTable("dbo.Notification");
        }
    }
}
