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
    public partial class UpdateDocumentType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateBinaryFileTypeRecord( "0AA42802-04FD-4AEC-B011-FEB127FC85CD", "Person Document", "", "fa fa-file-alt", "2C0A9DA7-85B5-4D30-8C8C-638C3902B711", false, true );

            // BinaryFileType: Person Document  Group: <all users>
            RockMigrationHelper.AddSecurityAuthForBinaryFileType( "2C0A9DA7-85B5-4D30-8C8C-638C3902B711", 2, "View", false, "", Model.SpecialRole.AllUsers, "CB53B379-7EE3-4E22-AD41-B009C3EB9DD3" );
            // BinaryFileType: Person Document Group: 300BA2C8-49A3-44BA-A82A-82E3FD8C3745 ( RSR - Staff Like Workers ) 
            RockMigrationHelper.AddSecurityAuthForBinaryFileType( "2C0A9DA7-85B5-4D30-8C8C-638C3902B711", 1, "Edit", true, "300BA2C8-49A3-44BA-A82A-82E3FD8C3745", Model.SpecialRole.None, "F30D030A-4925-47B3-B255-817ED5F27D60" );
            // BinaryFileType: Person Document Group: 300BA2C8-49A3-44BA-A82A-82E3FD8C3745 ( RSR - Staff Like Workers ), 
            RockMigrationHelper.AddSecurityAuthForBinaryFileType( "2C0A9DA7-85B5-4D30-8C8C-638C3902B711", 1, "View", true, "300BA2C8-49A3-44BA-A82A-82E3FD8C3745", Model.SpecialRole.None, "316A0324-EE56-4794-B5C9-9F9B3FEDD9EA" );
            // BinaryFileType: Person Document Group: 2C112948-FF4C-46E7-981A-0257681EADF4 ( RSR - Staff Workers )
            RockMigrationHelper.AddSecurityAuthForBinaryFileType( "2C0A9DA7-85B5-4D30-8C8C-638C3902B711", 0, "Edit", true, "2C112948-FF4C-46E7-981A-0257681EADF4", Model.SpecialRole.None, "4A5B1A11-0FE1-462C-BEB0-631F43B13D5F" );
            // BinaryFileType: Person Document Group: 2C112948-FF4C-46E7-981A-0257681EADF4 ( RSR - Staff Workers )
            RockMigrationHelper.AddSecurityAuthForBinaryFileType( "2C0A9DA7-85B5-4D30-8C8C-638C3902B711", 0, "View", true, "2C112948-FF4C-46E7-981A-0257681EADF4", Model.SpecialRole.None, "FBCB8264-E6FF-465B-BB2C-FBE0D0A679E8" );


            Sql( @" DECLARE @EntityTypeId INT = (
            		    SELECT TOP 1 [Id]
            		    FROM [EntityType]
            		    WHERE [Name] = 'Rock.Model.Person'
            		    )

                    DECLARE @BinaryFileTypeId INT = (
            		    SELECT TOP 1 [Id]
            		    FROM [BinaryFileType]
            		    WHERE [Guid] = '2C0A9DA7-85B5-4D30-8C8C-638C3902B711'
            		    )

                    IF NOT EXISTS (
                        SELECT *
                        FROM [DocumentType]
                        WHERE [EntityTypeId] = @EntityTypeId )
                    BEGIN
                     INSERT INTO
                      [DocumentType]
                      ([IsSystem], [Name],[IconCssClass], [EntityTypeId], [BinaryFileTypeId], [UserSelectable], [Order], [Guid])
                     VALUES 
                         (1, 'General Person Document', 'fa fa-file-alt', @EntityTypeId, @BinaryFileTypeId, 1, 0, '2FACE26D-FC22-4041-AA76-81BE4A914B5E')
                    END" );

            AddColumn( "dbo.DocumentType", "MaxDocumentsPerEntity", c => c.Int() );
            AddColumn( "dbo.DocumentType", "IsImage", c => c.Boolean( nullable: false ) );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn( "dbo.DocumentType", "IsImage" );
            DropColumn( "dbo.DocumentType", "MaxDocumentsPerEntity" );
        }
    }
}
