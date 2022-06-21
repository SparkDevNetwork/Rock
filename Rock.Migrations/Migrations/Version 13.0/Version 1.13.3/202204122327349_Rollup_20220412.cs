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
    public partial class Rollup_20220412 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateContentChannelViewDetail();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }


        /// <summary>
        /// SK: Update the existing ContentChannelViewDetail with Content Channel Item View
        /// </summary>
        private void UpdateContentChannelViewDetail()
        {
            Sql( @"
                -- Get the old ContentChannelViewDetail block type
                DECLARE @ContentChannelViewDetailId int = ( SELECT TOP 1 [Id] FROM [BlockType] WHERE [Path]='~/Blocks/Cms/ContentChannelViewDetail.ascx' )
                
                -- Get the new ContentChannelItemView block type
                DECLARE @ContentChannelItemViewId int = ( SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = '63659EBE-C5AF-4157-804A-55C7D565110E' )

                DECLARE @EntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block')

                DECLARE @BlockId int
                DECLARE cursor_table CURSOR FOR SELECT a.[Id] as BlockId FROM [Block] a INNER JOIN [BlockType] b on a.[BlockTypeId]=b.[Id] WHERE [Path]='~/Blocks/Cms/ContentChannelViewDetail.ascx'
                OPEN cursor_table
                FETCH NEXT FROM cursor_table INTO @BlockId
                WHILE @@FETCH_STATUS = 0
                BEGIN
	                IF @BlockId IS NOT NULL
	                BEGIN
		                -- update block of ContentChannelItemView block type with Content Channel Item View Block Type Id
		                UPDATE 
			                [dbo].[Block]
		                SET [BlockTypeId] = @ContentChannelItemViewId
		                WHERE
			                [Id] = @BlockId

		                UPDATE a
		                SET a.AttributeId=c.[Id]
		                FROM [dbo].[AttributeValue] a
                        INNER JOIN [dbo].[Attribute] b ON a.AttributeId = b.[Id] AND b.[EntityTypeQualifierColumn] = 'BlockTypeId' AND b.[EntityTypeQualifierValue] = @ContentChannelViewDetailId
		                INNER JOIN [dbo].[Attribute] c ON c.[EntityTypeQualifierColumn] = 'BlockTypeId' AND c.[EntityTypeQualifierValue] = @ContentChannelItemViewId AND c.[Key] = b.[Key]
		                WHERE a.[EntityId] = @BlockId
	                END

	                FETCH NEXT FROM cursor_table INTO @BlockId
                END

                CLOSE cursor_table
                DEALLOCATE cursor_table

                DELETE FROM [BlockType] WHERE [Path]='~/Blocks/Cms/ContentChannelViewDetail.ascx'" );
        }
    }
}
