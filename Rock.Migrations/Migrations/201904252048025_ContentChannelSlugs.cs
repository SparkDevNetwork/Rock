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
    public partial class ContentChannelSlugs : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateModelUp();
            ItemGlobalKeyUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            UpdateModelDown();
        }

        /// <summary>
        /// Make needed changes to the model
        /// </summary>
        private void UpdateModelUp()
        {
            AddColumn("dbo.ContentChannelItem", "ItemGlobalKey", c => c.String(maxLength: 100));
        }

        /// <summary>
        /// Undo changes made to the model
        /// </summary>
        private void UpdateModelDown()
        {
            DropColumn("dbo.ContentChannelItem", "ItemGlobalKey");
        }

        /// <summary>
        /// Populates ContentChannelItem.ItemGlobalKey with the slug string for the channel that has the lowest Id.
        /// This does not need a down because in that event the column is dropped.
        /// </summary>
        private void ItemGlobalKeyUp()
        {
            Sql( @"
                WITH cte AS
                (
	                SELECT allChannelItemsSlugs.*
	                FROM 
		                (SELECT [ContentChannelItemId], MIN( [Id] ) AS Id
		                FROM [dbo].[ContentChannelItemSlug]
		                Group BY [ContentChannelItemId]) firstChannelItemSlug
	                JOIN [ContentChannelItemSlug] allChannelItemsSlugs ON firstChannelItemSlug.[Id] = allChannelItemsSlugs.[Id]
                )

                UPDATE [dbo].[ContentChannelItem]
                SET [ContentChannelItem].[ItemGlobalKey] = t.[Slug]
                FROM [dbo].[ContentChannelItem] cci
                JOIN cte t ON cci.[Id] = t.[ContentChannelItemId]" );
        }

    }
}
