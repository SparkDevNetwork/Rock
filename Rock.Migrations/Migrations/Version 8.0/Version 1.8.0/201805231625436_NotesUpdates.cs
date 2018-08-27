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
    public partial class NotesUpdates : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.NoteWatch",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    NoteTypeId = c.Int(),
                    EntityTypeId = c.Int(),
                    EntityId = c.Int(),
                    NoteId = c.Int(),
                    IsWatching = c.Boolean( nullable: false ),
                    WatchReplies = c.Boolean( nullable: false ),
                    AllowOverride = c.Boolean( nullable: false ),
                    WatcherPersonAliasId = c.Int(),
                    WatcherGroupId = c.Int(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.EntityType", t => t.EntityTypeId, cascadeDelete: true )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.Note", t => t.NoteId, cascadeDelete: true )
                .ForeignKey( "dbo.NoteType", t => t.NoteTypeId )
                .ForeignKey( "dbo.Group", t => t.WatcherGroupId, cascadeDelete: true )
                .ForeignKey( "dbo.PersonAlias", t => t.WatcherPersonAliasId, cascadeDelete: true )
                .Index( t => t.NoteTypeId )
                .Index( t => t.EntityTypeId )
                .Index( t => t.NoteId )
                .Index( t => t.WatcherPersonAliasId )
                .Index( t => t.WatcherGroupId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            AddColumn( "dbo.Note", "ParentNoteId", c => c.Int() );
            AddColumn( "dbo.Note", "ApprovalStatus", c => c.Int( nullable: false ) );
            AddColumn( "dbo.Note", "ApprovedByPersonAliasId", c => c.Int() );
            AddColumn( "dbo.Note", "ApprovedDateTime", c => c.DateTime() );
            AddColumn( "dbo.Note", "NotificationsSent", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.Note", "ApprovalsSent", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.Note", "NoteUrl", c => c.String() );
            AddColumn( "dbo.Note", "EditedDateTime", c => c.DateTime() );
            AddColumn( "dbo.Note", "EditedByPersonAliasId", c => c.Int() );
            AddColumn( "dbo.NoteType", "RequiresApprovals", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.NoteType", "AllowsWatching", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.NoteType", "AllowsReplies", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.NoteType", "MaxReplyDepth", c => c.Int() );
            AddColumn( "dbo.NoteType", "BackgroundColor", c => c.String( maxLength: 100 ) );
            AddColumn( "dbo.NoteType", "FontColor", c => c.String( maxLength: 100 ) );
            AddColumn( "dbo.NoteType", "BorderColor", c => c.String( maxLength: 100 ) );
            AddColumn( "dbo.NoteType", "SendApprovalNotifications", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.NoteType", "AutoWatchAuthors", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.NoteType", "ApprovalUrlTemplate", c => c.String() );
            CreateIndex( "dbo.Note", "ParentNoteId" );
            CreateIndex( "dbo.Note", "EditedByPersonAliasId" );
            AddForeignKey( "dbo.Note", "EditedByPersonAliasId", "dbo.PersonAlias", "Id" );
            AddForeignKey( "dbo.Note", "ParentNoteId", "dbo.Note", "Id" );

            // Update all current notes to Approved since approve is a new thing 
            Sql( "UPDATE [Note] SET [ApprovalStatus] = 1 WHERE [ApprovalStatus] != 1" );

            // Fix any Notes that have still have caption of 'You - Personal Note' but have IsPrivateNote = false (this fixes an issue where Notes that were created as IsPrivate but changed to Not Private have the wrong caption)
            Sql( @"
UPDATE [Note]
SET [Caption] = ''
WHERE [Caption] = 'You - Personal Note'
	AND [IsPrivateNote] = 0
" );

            // Add a Route and ApprovalUrlTemplate to Prayer Comment NoteType
            RockMigrationHelper.AddPageRoute( Rock.SystemGuid.Page.PRAYER_REQUEST_DETAIL, "PrayerRequestDetail/{PrayerRequestId}" );

            Sql( $@"
UPDATE [NoteType]
SET [ApprovalUrlTemplate] = '{{ ''Global'' | Attribute:''InternalApplicationRoot'' }}PrayerRequestDetail/{{ Note.EntityId }}#{{ Note.NoteAnchorId }}'
WHERE [Guid] = '{Rock.SystemGuid.NoteType.PRAYER_COMMENT}'" );


            // Delete old NoteTypes block (it has been refactoed into NoteTypeList/NoteTypeDetail)
            RockMigrationHelper.DeleteBlock( "F3805956-9A24-4FBF-8370-F3D29D788445" );
            RockMigrationHelper.DeleteBlockType( "44d2dab8-6dca-4dc3-b1ff-52da224b2d5c" );

            // Migrate Pages/Blocks for Notes Updates
            MigrateNotesUpdatesBlocksAndPagesUp();

            // Create the Note Notifications and Approval System Email templates
            MigrateSystemEmailsUp();

            // Add Note Notifications Job
            // add ServiceJob: Send Note Notifications
            Sql( @"IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.SendNoteNotifications' AND [Guid] = '3DB284D8-F8A9-4A49-959B-0629F9802CE0' )
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
                  ,1
                  ,'Send Note Notifications'
                  ,'Send note watch and note approval notifications'
                  ,'Rock.Jobs.SendNoteNotifications'
                  ,'0 0 0/2 1/1 * ? *'
                  ,1
                  ,'3DB284D8-F8A9-4A49-959B-0629F9802CE0'
                  );
            END" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF", "Class", "Rock.Jobs.SendNoteNotifications", "Note Watch Notification Email", "", 1, @"21B92DE2-6825-45F3-BD27-43B47FE490D8", "CE0CE2F6-B3F1-4F4D-853B-FCFE51187328", "NoteWatchNotificationEmail" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF", "Class", "Rock.Jobs.SendNoteNotifications", "Note Approval Notification Email", "", 2, @"B2E3D75F-681E-430F-82C9-D0D681040FAF", "99FAB257-D55B-4432-9F4A-4449CDA75F83", "NoteApprovalNotificationEmail" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Class", "Rock.Jobs.SendNoteNotifications", "Cutoff Days", "Just in case the Note Notification service hasn't run for a while, this is the max number of days between the note edited date and the notification.", 3, @"7", "FA2BA8D9-F525-44B9-84B0-B63F9E3E04D3", "CutoffDays" );
            RockMigrationHelper.AddAttributeValue( "CE0CE2F6-B3F1-4F4D-853B-FCFE51187328", 37, @"21b92de2-6825-45f3-bd27-43b47fe490d8", "CE0CE2F6-B3F1-4F4D-853B-FCFE51187328" ); // Send Note Notifications: Note Watch Notification Email
            RockMigrationHelper.AddAttributeValue( "99FAB257-D55B-4432-9F4A-4449CDA75F83", 37, @"b2e3d75f-681e-430f-82c9-d0d681040faf", "99FAB257-D55B-4432-9F4A-4449CDA75F83" ); // Send Note Notifications: Note Approval Notification Email
            RockMigrationHelper.AddAttributeValue( "FA2BA8D9-F525-44B9-84B0-B63F9E3E04D3", 37, @"7", "FA2BA8D9-F525-44B9-84B0-B63F9E3E04D3" ); // Send Note Notifications: Cutoff Days
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "CE0CE2F6-B3F1-4F4D-853B-FCFE51187328" ); // Rock.Jobs.SendNoteNotifications: Note Watch Notification Email
            RockMigrationHelper.DeleteAttribute( "99FAB257-D55B-4432-9F4A-4449CDA75F83" ); // Rock.Jobs.SendNoteNotifications: Note Approval Notification Email
            RockMigrationHelper.DeleteAttribute( "FA2BA8D9-F525-44B9-84B0-B63F9E3E04D3" ); // Rock.Jobs.SendNoteNotifications: Cutoff Days

            // remove ServiceJob: Send Note Notifications
            Sql( @"IF EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.SendNoteNotifications' AND [Guid] = '3DB284D8-F8A9-4A49-959B-0629F9802CE0' )
            BEGIN
               DELETE [ServiceJob]  WHERE [Guid] = '3DB284D8-F8A9-4A49-959B-0629F9802CE0';
            END" );


            DropForeignKey( "dbo.NoteWatch", "WatcherPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.NoteWatch", "WatcherGroupId", "dbo.Group" );
            DropForeignKey( "dbo.NoteWatch", "NoteTypeId", "dbo.NoteType" );
            DropForeignKey( "dbo.NoteWatch", "NoteId", "dbo.Note" );
            DropForeignKey( "dbo.NoteWatch", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.NoteWatch", "EntityTypeId", "dbo.EntityType" );
            DropForeignKey( "dbo.NoteWatch", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Note", "ParentNoteId", "dbo.Note" );
            DropForeignKey( "dbo.Note", "EditedByPersonAliasId", "dbo.PersonAlias" );
            DropIndex( "dbo.NoteWatch", new[] { "Guid" } );
            DropIndex( "dbo.NoteWatch", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.NoteWatch", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.NoteWatch", new[] { "WatcherGroupId" } );
            DropIndex( "dbo.NoteWatch", new[] { "WatcherPersonAliasId" } );
            DropIndex( "dbo.NoteWatch", new[] { "NoteId" } );
            DropIndex( "dbo.NoteWatch", new[] { "EntityTypeId" } );
            DropIndex( "dbo.NoteWatch", new[] { "NoteTypeId" } );
            DropIndex( "dbo.Note", new[] { "EditedByPersonAliasId" } );
            DropIndex( "dbo.Note", new[] { "ParentNoteId" } );
            DropColumn( "dbo.NoteType", "ApprovalUrlTemplate" );
            DropColumn( "dbo.NoteType", "AutoWatchAuthors" );
            DropColumn( "dbo.NoteType", "SendApprovalNotifications" );
            DropColumn( "dbo.NoteType", "BorderColor" );
            DropColumn( "dbo.NoteType", "FontColor" );
            DropColumn( "dbo.NoteType", "BackgroundColor" );
            DropColumn( "dbo.NoteType", "MaxReplyDepth" );
            DropColumn( "dbo.NoteType", "AllowsReplies" );
            DropColumn( "dbo.NoteType", "AllowsWatching" );
            DropColumn( "dbo.NoteType", "RequiresApprovals" );
            DropColumn( "dbo.Note", "EditedByPersonAliasId" );
            DropColumn( "dbo.Note", "EditedDateTime" );
            DropColumn( "dbo.Note", "NoteUrl" );
            DropColumn( "dbo.Note", "ApprovalsSent" );
            DropColumn( "dbo.Note", "NotificationsSent" );
            DropColumn( "dbo.Note", "ApprovedDateTime" );
            DropColumn( "dbo.Note", "ApprovedByPersonAliasId" );
            DropColumn( "dbo.Note", "ApprovalStatus" );
            DropColumn( "dbo.Note", "ParentNoteId" );
            DropTable( "dbo.NoteWatch" );

            MigrateNotesUpdatesBlocksPagesDown();
        }

        #region Private Methods for this migration

        /// <summary>
        /// Migrates the notes update blocks and pages up.
        /// </summary>
        private void MigrateNotesUpdatesBlocksAndPagesUp()
        {
            RockMigrationHelper.AddPage( true, "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Note Watches", "", "74FB3214-8F11-4D40-A0E9-1AEA377E9217", "fa fa-binoculars" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "74FB3214-8F11-4D40-A0E9-1AEA377E9217", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Note Watch Detail", "", "6717F2F8-85C8-404A-B4CD-683379A2A487", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "B0E5876F-E29E-477B-8874-482DEDD3A6C5", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Note Type Detail", "", "421C838D-F6BA-46C5-8DBF-36CA0CC17B77", "fa fa-edit" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Note Watch Detail", "Displays the details of a note watch.", "~/Blocks/Core/NoteWatchDetail.ascx", "Core", "361F15FC-4C08-4A26-B482-CC260E708F7C" );
            RockMigrationHelper.UpdateBlockType( "Note Watch List", "Block for viewing a list of note watches", "~/Blocks/Core/NoteWatchList.ascx", "Core", "E8718774-3DCA-4AFF-9F4C-FBA50A00BB57" );
            RockMigrationHelper.UpdateBlockType( "Note Type Detail", "Block for managing a note type", "~/Blocks/Core/NoteTypeDetail.ascx", "Core", "5DA1D088-2142-4645-AF9C-EF52DA5B4EEA" );
            RockMigrationHelper.UpdateBlockType( "Note Type List", "Allows note types to be managed.", "~/Blocks/Core/NoteTypeList.ascx", "Core", "BEC5B592-9E9E-4C55-BD0D-2B8065A1802E" );
            // Add Block to Page: Note Types, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "B0E5876F-E29E-477B-8874-482DEDD3A6C5", "", "BEC5B592-9E9E-4C55-BD0D-2B8065A1802E", "Note Type List", "Main", @"", @"", 1, "585D53CB-80BC-4D93-84E0-3F589CD62CCE" );
            // Add Block to Page: Note Watches, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "74FB3214-8F11-4D40-A0E9-1AEA377E9217", "", "E8718774-3DCA-4AFF-9F4C-FBA50A00BB57", "Note Watch List", "Main", @"", @"", 0, "EE043ED9-DCA3-4096-9580-264DA8CA459E" );
            // Add Block to Page: Note Watch Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6717F2F8-85C8-404A-B4CD-683379A2A487", "", "361F15FC-4C08-4A26-B482-CC260E708F7C", "Note Watch Detail", "Main", @"", @"", 0, "7434D256-2719-4EE2-9D22-78A136D29132" );
            // Add Block to Page: Note Type Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "421C838D-F6BA-46C5-8DBF-36CA0CC17B77", "", "5DA1D088-2142-4645-AF9C-EF52DA5B4EEA", "Note Type Detail", "Main", @"", @"", 0, "F41CCB84-4A69-496B-BF6F-B56BDC919006" );
            // Attrib for BlockType: Notes:Note View Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Note View Lava Template", "NoteViewLavaTemplate", "", @"The Lava Template to use when rendering the readonly view of all the notes.", 14, @"{% include '~~/Assets/Lava/NoteViewList.lava' %}", "328DDE3F-6FFF-4CA4-B6D0-C1BD4D643307" );
            // Attrib for BlockType: Note Watch Detail:Note Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "361F15FC-4C08-4A26-B482-CC260E708F7C", "E3FF88AC-13F6-4DF8-8371-FC0D7FD9A571", "Note Type", "NoteType", "", @"Set Note Type to limit this block to a specific note type", 1, @"", "88515FFF-3910-4347-901E-DDFA05F5B4AD" );
            // Attrib for BlockType: Note Watch Detail:Entity Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "361F15FC-4C08-4A26-B482-CC260E708F7C", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "EntityType", "", @"Set an Entity Type to limit this block to Note Types and Entities for a specific entity type.", 0, @"", "3735C382-038D-43B6-9B1A-3A0F98030669" );
            // Attrib for BlockType: Note Watch List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "E8718774-3DCA-4AFF-9F4C-FBA50A00BB57", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", @"", 0, @"", "847F2DF0-E3B7-4693-8D2F-D62924B85FCF" );
            // Attrib for BlockType: Note Watch List:Entity Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "E8718774-3DCA-4AFF-9F4C-FBA50A00BB57", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "EntityType", "", @"Set an Entity Type to limit this block to Note Types and Entities for a specific entity type.", 0, @"", "4D27A46A-E052-4F7D-8B2C-74206F3B7B50" );
            // Attrib for BlockType: Note Watch List:Note Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "E8718774-3DCA-4AFF-9F4C-FBA50A00BB57", "E3FF88AC-13F6-4DF8-8371-FC0D7FD9A571", "Note Type", "NoteType", "", @"Set Note Type to limit this block to a specific note type", 1, @"", "062FD392-325D-40C6-B176-4FB881FA8FCF" );
            // Attrib for BlockType: Note Watch List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "E8718774-3DCA-4AFF-9F4C-FBA50A00BB57", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "FA082186-A581-488F-BA2C-D30EAECA15F5" );

            // Attrib for BlockType: Note Type List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "BEC5B592-9E9E-4C55-BD0D-2B8065A1802E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", @"", 0, @"", "37A23D75-DF97-485B-AD9F-949D2717537C" );
            // Attrib for BlockType: Note Type List:Entity Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "BEC5B592-9E9E-4C55-BD0D-2B8065A1802E", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "EntityType", "", @"Select an entity type to only show note types for the selected entity type, or leave blank to show all", 0, @"", "6AF551AE-139A-4F0B-A337-87F10FD818B4" );
            // Attrib Value for Block:Note Watch List, Attribute:core.CustomGridColumnsConfig Page: Note Watches, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "EE043ED9-DCA3-4096-9580-264DA8CA459E", "FA082186-A581-488F-BA2C-D30EAECA15F5", @"" );
            // Attrib Value for Block:Note Watch List, Attribute:Detail Page Page: Note Watches, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "EE043ED9-DCA3-4096-9580-264DA8CA459E", "847F2DF0-E3B7-4693-8D2F-D62924B85FCF", @"6717f2f8-85c8-404a-b4cd-683379a2a487" );
            // Attrib Value for Block:Note Type List, Attribute:Entity Type Page: Note Types, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "585D53CB-80BC-4D93-84E0-3F589CD62CCE", "6AF551AE-139A-4F0B-A337-87F10FD818B4", @"" );
            // Attrib Value for Block:Note Type List, Attribute:Detail Page Page: Note Types, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "585D53CB-80BC-4D93-84E0-3F589CD62CCE", "37A23D75-DF97-485B-AD9F-949D2717537C", @"421c838d-f6ba-46c5-8dbf-36ca0cc17b77" );
            // Add/Update PageContext for Page:Prayer Request Detail, Entity: Rock.Model.PrayerRequest, Parameter: PrayerRequestId
            RockMigrationHelper.UpdatePageContext( "89C3DB4A-BAFD-45C8-88C6-45D8FEC48B48", "Rock.Model.PrayerRequest", "PrayerRequestId", "CC3CF931-7B42-4307-80CB-AD7A633DFBD5" );
        }

        /// <summary>
        /// Migrates new system emails up.
        /// Code Generated from Dev Tools\Sql\CodeGen_SystemEmail.sql
        /// </summary>
        private void MigrateSystemEmailsUp()
        {
            // Note Watch Notification
            RockMigrationHelper.UpdateSystemEmail( "System", "Note Watch Notification", "", "", "", "", "", "Note Watch Digest | {{ 'Global' | Attribute:'OrganizationName' }}", @"{{ 'Global' | Attribute:'EmailHeader' }}

<p>
    {{ Person.NickName }}:
</p>

<p>
    Here are some updates on some notes you're watching:
</p>


{% for note in NoteList %}
    {% if note.EditedDateTime > note.CreatedDateTime %}
        {% assign noteAction = 'edited' %}
    {% else %}
        {% assign noteAction = 'added' %}
    {% endif %}
    
    {% if note.ParentNoteId != null %}
        {% assign noteTypeText = 'reply to a note' %}
    {% else %}
        {% assign noteTypeText = note.NoteType.Name %}
    {% endif %}

    <table style=""border: 1px solid #c4c4c4; border-collapse:collapse; mso-table-lspace:0pt; mso-table-rspace:0pt; width: 100%; margin-bottom: 24px;"" cellspacing=""0"" cellpadding=""4"">
        <tr style=""border: 1px solid #c4c4c4;"">
            <td colspan=""2"" bgcolor=""#a6a5a5"" align=""left"" style=""color: #ffffff; padding: 4px 8px; border: 1px solid #c4c4c4;"">
                <h4 style=""color: #ffffff; line-height: 1.2em;"">
                    {{ note.EditedByPersonName }} {{ noteAction }} a {{ noteTypeText  }} on {{ note.EntityName }}
                </h4>
                <p>
                    <a style=""color: #ffffff;"" href=""{{ 'Global' | Attribute:'InternalApplicationRoot' }}{{ note.NoteUrl }}#{{ note.NoteAnchorId }}"">{{ note.Text }}</a> 
                </9>
            </td>
        </tr>
    </table>
    &nbsp;

{% endfor %}

<p>&nbsp;</p>

{{ 'Global' | Attribute:'EmailFooter' }}", "21B92DE2-6825-45F3-BD27-43B47FE490D8" );

            // Note Approval Email
            RockMigrationHelper.UpdateSystemEmail( "System", "Note Approval Email", "", "", "", "", "", "Note Approvals Request | {{ 'Global' | Attribute:'OrganizationName' }}", @"{{ 'Global' | Attribute:'EmailHeader' }}

<p>
    {{ Person.NickName }}:
</p>

<p>
    Here are some notes that require your approval:
</p>


{% for note in NoteList %}
    {% if note.EditedDateTime > note.CreatedDateTime %}
        {% assign noteAction = 'edited' %}
    {% else %}
        {% assign noteAction = 'added' %}
    {% endif %}
    
    {% if note.ParentNoteId != null %}
        {% assign noteTypeText = 'reply to a note' %}
    {% else %}
        {% assign noteTypeText = note.NoteType.Name %}
    {% endif %}

    <table style=""border: 1px solid #c4c4c4; border-collapse:collapse; mso-table-lspace:0pt; mso-table-rspace:0pt; width: 100%; margin-bottom: 24px;"" cellspacing=""0"" cellpadding=""4"">
        <tr style=""border: 1px solid #c4c4c4;"">
            <td colspan=""2"" bgcolor=""#a6a5a5"" align=""left"" style=""color: #ffffff; padding: 4px 8px; border: 1px solid #c4c4c4;"">
                <h4 style=""color: #ffffff; line-height: 1.2em;"">
                    {{ note.EditedByPersonName }} {{ noteAction }} a {{ noteTypeText  }} on {{ note.EntityName }}
                </h4>
                <p>
                   Note: {{ note.Text }}
                </p>
                <p>
                <a style=""color: #ffffff;"" href=""{{ 'Global' | Attribute:'InternalApplicationRoot' }}{{ note.NoteUrl }}#{{ note.NoteAnchorId }}"">View Note</a>
                {% if note.NoteType.ApprovalUrlTemplate != '' %}
                | <a style=""color: #ffffff;"" href=""{{ note.ApprovalUrl }}"">Note Approval</a> 
                {% endif %}
            </td>
        </tr>
    </table>
    &nbsp;

{% endfor %}

<p>&nbsp;</p>

{{ 'Global' | Attribute:'EmailFooter' }}", "B2E3D75F-681E-430F-82C9-D0D681040FAF" );
        }

        /// <summary>
        /// Migrates the notes updates blocks pages down.
        /// </summary>
        private void MigrateNotesUpdatesBlocksPagesDown()
        {
            // Note Watch Notification
            RockMigrationHelper.DeleteSystemEmail( "21B92DE2-6825-45F3-BD27-43B47FE490D8" );
            // Note Approval Email
            RockMigrationHelper.DeleteSystemEmail( "B2E3D75F-681E-430F-82C9-D0D681040FAF" );

            // Attrib for BlockType: Note Type List:Detail Page
            RockMigrationHelper.DeleteAttribute( "37A23D75-DF97-485B-AD9F-949D2717537C" );
            // Attrib for BlockType: Note Type List:Entity Type
            RockMigrationHelper.DeleteAttribute( "6AF551AE-139A-4F0B-A337-87F10FD818B4" );

            // Attrib for BlockType: Note Watch Detail:Note Type
            RockMigrationHelper.DeleteAttribute( "88515FFF-3910-4347-901E-DDFA05F5B4AD" );
            // Attrib for BlockType: Note Watch Detail:Entity Type
            RockMigrationHelper.DeleteAttribute( "3735C382-038D-43B6-9B1A-3A0F98030669" );
            // Attrib for BlockType: Note Watch List:Note Type
            RockMigrationHelper.DeleteAttribute( "062FD392-325D-40C6-B176-4FB881FA8FCF" );
            // Attrib for BlockType: Note Watch List:Entity Type
            RockMigrationHelper.DeleteAttribute( "4D27A46A-E052-4F7D-8B2C-74206F3B7B50" );
            // Attrib for BlockType: Note Watch List:Detail Page
            RockMigrationHelper.DeleteAttribute( "847F2DF0-E3B7-4693-8D2F-D62924B85FCF" );
            // Attrib for BlockType: Note Watch List:core.CustomGridColumnsConfig
            RockMigrationHelper.DeleteAttribute( "FA082186-A581-488F-BA2C-D30EAECA15F5" );
            // Attrib for BlockType: Notes:Note View Lava Template
            RockMigrationHelper.DeleteAttribute( "328DDE3F-6FFF-4CA4-B6D0-C1BD4D643307" );
            // Remove Block: Note Type Detail, from Page: Note Type Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "F41CCB84-4A69-496B-BF6F-B56BDC919006" );
            // Remove Block: Note Type List, from Page: Note Types, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "585D53CB-80BC-4D93-84E0-3F589CD62CCE" );
            // Remove Block: Note Watch Detail, from Page: Note Watch Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "7434D256-2719-4EE2-9D22-78A136D29132" );
            // Remove Block: Note Watch List, from Page: Note Watches, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "EE043ED9-DCA3-4096-9580-264DA8CA459E" );
            RockMigrationHelper.DeleteBlockType( "BEC5B592-9E9E-4C55-BD0D-2B8065A1802E" ); // Note Type List
            RockMigrationHelper.DeleteBlockType( "5DA1D088-2142-4645-AF9C-EF52DA5B4EEA" ); // Note Type Detail

            RockMigrationHelper.DeleteBlockType( "E8718774-3DCA-4AFF-9F4C-FBA50A00BB57" ); // Note Watch List
            RockMigrationHelper.DeleteBlockType( "361F15FC-4C08-4A26-B482-CC260E708F7C" ); // Note Watch Detail
            RockMigrationHelper.DeletePage( "421C838D-F6BA-46C5-8DBF-36CA0CC17B77" ); //  Page: Note Type Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "6717F2F8-85C8-404A-B4CD-683379A2A487" ); //  Page: Note Watch Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "74FB3214-8F11-4D40-A0E9-1AEA377E9217" ); //  Page: Note Watches, Layout: Full Width, Site: Rock RMS
        }

        #endregion 
    }
}
