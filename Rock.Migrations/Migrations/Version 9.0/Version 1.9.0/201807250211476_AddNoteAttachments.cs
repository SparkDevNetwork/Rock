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
    public partial class AddNoteAttachments : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.NoteAttachment",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    NoteId = c.Int( nullable: false ),
                    BinaryFileId = c.Int( nullable: false ),
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
                .ForeignKey( "dbo.BinaryFile", t => t.BinaryFileId )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.Note", t => t.NoteId, cascadeDelete: true )
                .Index( t => t.NoteId )
                .Index( t => t.BinaryFileId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            AddColumn( "dbo.NoteType", "AllowsAttachments", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.NoteType", "BinaryFileTypeId", c => c.Int() );
            CreateIndex( "dbo.NoteType", "BinaryFileTypeId" );
            AddForeignKey( "dbo.NoteType", "BinaryFileTypeId", "dbo.BinaryFileType", "Id" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey( "dbo.NoteType", "BinaryFileTypeId", "dbo.BinaryFileType" );
            DropForeignKey( "dbo.NoteAttachment", "NoteId", "dbo.Note" );
            DropForeignKey( "dbo.NoteAttachment", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.NoteAttachment", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.NoteAttachment", "BinaryFileId", "dbo.BinaryFile" );
            DropIndex( "dbo.NoteType", new[] { "BinaryFileTypeId" } );
            DropIndex( "dbo.NoteAttachment", new[] { "Guid" } );
            DropIndex( "dbo.NoteAttachment", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.NoteAttachment", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.NoteAttachment", new[] { "BinaryFileId" } );
            DropIndex( "dbo.NoteAttachment", new[] { "NoteId" } );
            DropColumn( "dbo.NoteType", "BinaryFileTypeId" );
            DropColumn( "dbo.NoteType", "AllowsAttachments" );
            DropTable( "dbo.NoteAttachment" );
        }
    }
}
