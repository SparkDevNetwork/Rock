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
    public partial class AllowNullBlockIdForHtmlContent : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey( "dbo.HtmlContent", "BlockId", "dbo.Block" );
            // There is no need to drop the index in this case.
            //DropIndex( "dbo.HtmlContent", new[] { "BlockId" } );
            AlterColumn( "dbo.HtmlContent", "BlockId", c => c.Int() );
            //CreateIndex( "dbo.HtmlContent", "BlockId" );

            // Instead of the scaffolded AddForeignKey( "dbo.HtmlContent", "BlockId", "dbo.Block", "Id" );
            // we want a ON DELETE NULL (cascade null).
            Sql( @"
                ALTER TABLE [dbo].[HtmlContent]
                ADD CONSTRAINT [FK_dbo.HtmlContent_dbo.Block_BlockId] FOREIGN KEY ([BlockId])
                REFERENCES [dbo].[Block] ([Id])
                ON DELETE SET NULL;
            " );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Failsafe: Ensure no NULL BlockId values exist before making the column non-nullable again.
            Sql( @"
                UPDATE [dbo].[HtmlContent]
                SET [BlockId] = (SELECT TOP 1 [Id] FROM [dbo].[Block] ORDER BY [Id])
                , ForeignKey='AllowNullBlockIdForHtmlContent'
                WHERE [BlockId] IS NULL
            " );

            DropForeignKey( "dbo.HtmlContent", "BlockId", "dbo.Block" );
            // In this case, we _do_ have to drop the index before altering the column
            DropIndex( "dbo.HtmlContent", new[] { "BlockId" } );
            AlterColumn( "dbo.HtmlContent", "BlockId", c => c.Int( nullable: false ) );
            CreateIndex( "dbo.HtmlContent", "BlockId" );
            AddForeignKey( "dbo.HtmlContent", "BlockId", "dbo.Block", "Id", cascadeDelete: true );
        }
    }
}
