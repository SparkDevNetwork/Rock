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
    public partial class AddSecurityToWebAds : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateEntityType( "Rock.Model.ContentChannel", "Content Channel", "Rock.Model.ContentChannel, Rock, Version=1.1.0.0, Culture=neutral, PublicKeyToken=null", true, true, "44484685-477E-4668-89A6-84F29739EB68" );

            Sql( @" 
                    DECLARE @EntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.ContentChannel' ) 
                    DECLARE @ChannelId int = ( SELECT TOP 1 [Id] FROM [ContentChannel] WHERE [Guid] = '8E213BB1-9E6F-40C1-B468-B3F8A60D5D24' ) 
                    DECLARE @GroupId int = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = 'B1906B7D-1A1E-41B9-BBA4-F4482CECAF7B' ) 

                    IF @EntityTypeId IS NOT NULL AND @ChannelID IS NOT NULL AND @GroupId IS NOT NULL
                    BEGIN

                        INSERT INTO [Auth] 
	                        ([EntityTypeId]
                              ,[EntityId]
                              ,[Order]
                              ,[Action]
                              ,[AllowOrDeny]
                              ,[GroupId]
	                          ,[SpecialRole]
                              ,[Guid] )
                        VALUES 
	                        (@EntityTypeId, @ChannelId, 0, 'Edit', 'A', @GroupId, 0, 'B7AD0E8E-263A-4BD7-83F4-BB87C7A170F3')

                        INSERT INTO [Auth] 
	                        ([EntityTypeId]
                              ,[EntityId]
                              ,[Order]
                              ,[Action]
                              ,[AllowOrDeny]
                              ,[GroupId]
	                          ,[SpecialRole]
                              ,[Guid] )
                        VALUES 
	                        (@EntityTypeId, @ChannelId, 1, 'Administrate', 'A', @GroupId, 0, 'F49ECF94-510F-47AE-A204-07995BC9EA16')


                        INSERT INTO [Auth] 
	                        ([EntityTypeId]
                              ,[EntityId]
                              ,[Order]
                              ,[Action]
                              ,[AllowOrDeny]
                              ,[GroupId]
	                          ,[SpecialRole]
                              ,[Guid] )
                        VALUES 
	                        (@EntityTypeId, @ChannelId, 2, 'Approve', 'A', @GroupId, 0, '40798AFD-E4A6-46BC-BBB5-A11171087791')

                    END
" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
