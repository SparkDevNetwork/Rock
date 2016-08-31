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
    public partial class EmailAndLinkAttributeTypes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // update 'Organization Address' to not have a name: 
            Sql( @"UPDATE [Location] SET [Name] = NULL WHERE [guid] = '17E57EA8-D942-485B-8B61-233589AAC631'" );
            
            // stark blocks
            RockMigrationHelper.UpdateBlockType( "Stark Detail", "Template block for developers to use to start a new detail block.", "~/Blocks/Utility/StarkDetail.ascx", "Utility", "D6B14847-B652-49E2-9D4B-658D502F0AEC" );
            RockMigrationHelper.UpdateBlockType( "Stark List", "Template block for developers to use to start a new list block.", "~/Blocks/Utility/StarkList.ascx", "Utility", "E333D1CC-CB55-4E73-8568-41DAD296971C" );
            
            // Grant View to Rock Administrators for Check Images
            RockMigrationHelper.AddSecurityAuthForBinaryFileType( "6D18A9C4-34AB-444A-B95B-C644019465AC", 2, Rock.Security.Authorization.VIEW, true, "628C51A8-4613-43ED-A18D-4A6FB999273E" /* Rock Administrators */, Rock.Model.SpecialRole.None, "438F4C14-9A58-414F-A1C6-FB0755F6A2F4" );

            // ensure that "All Users" auth is last
            Sql( "UPDATE [Auth] set [Order] = [Order]+1 where [Guid] = '6C0EC7E2-271E-4DA9-A8A7-C4C4B0840E86'" );
            
            // add email field type and attribute
            Sql( @"
              DELETE FROM [Attribute]
                    WHERE [FieldTypeId] IN (SELECT [Id] FROM [FieldType] WHERE [Class] = 'Rock.Field.Types.EmailFieldType')

               DELETE FROM [FieldType]
                    WHERE [Class] = 'Rock.Field.Types.EmailFieldType'

                INSERT INTO [FieldType]
                        ([IsSystem]
                        ,[Name]
                        ,[Description]
                        ,[Assembly]
                        ,[Class]
                        ,[Guid]
                        ,[CreatedDateTime]
                        ,[ModifiedDateTime])
                    VALUES
                        (1
                        ,'Email'
                        ,''
                        ,'Rock'
                        ,'Rock.Field.Types.EmailFieldType'
                        ,'3D045CAE-EA72-4A04-B7BE-7FD1D6214217'
                        ,getdate()
                        ,getdate())" );

            // insert url link attribute and field type
            Sql( @"
                
                DELETE FROM [FieldType]
                    WHERE [Class] = 'Rock.Field.Types.UrlLinkFieldType'

                DELETE FROM [Attribute]
                    WHERE [FieldTypeId] IN (SELECT [Id] FROM [FieldType] WHERE [Class] = 'Rock.Field.Types.UrlLinkFieldType')

                INSERT INTO [FieldType]
                        ([IsSystem]
                        ,[Name]
                        ,[Description]
                        ,[Assembly]
                        ,[Class]
                        ,[Guid]
                        ,[CreatedDateTime]
                        ,[ModifiedDateTime])
                    VALUES
                        (1
                        ,'Url Link'
                        ,''
                        ,'Rock'
                        ,'Rock.Field.Types.UrlLinkFieldType'
                        ,'C0D0D7E2-C3B0-4004-ABEA-4BBFAD10D5D2'
                        ,getdate()
                        ,getdate())
            " );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteBlockType( "E333D1CC-CB55-4E73-8568-41DAD296971C" ); // Stark List
            RockMigrationHelper.DeleteBlockType( "D6B14847-B652-49E2-9D4B-658D502F0AEC" ); // Stark Detail
        }
    }
}
