// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class SitePageUpdates : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Page", "AllowIndexing", c => c.Boolean(nullable: false, defaultValue: true ) );
            AddColumn("dbo.Site", "PageHeaderContent", c => c.String());
            AddColumn("dbo.Site", "AllowIndexing", c => c.Boolean(nullable: false, defaultValue: true ) );

            // turn off site indexing for specific sites
            Sql( @"UPDATE [Site]
	            SET [AllowIndexing] = 0
	            WHERE [Guid] IN ('C2D29296-6A87-47A9-A753-EE4E9159C4C4','15AEFC01-ACB3-4F5D-B83E-AB3AB7F2A54A','A5FA7C3C-A238-4E0B-95DE-B540144321EC', '05E96F7B-B75E-4987-825A-B6F51F8D9CAA' )" );

            // turn off site indexing on internal pages
            Sql( @"UPDATE [Page]
	                SET [AllowIndexing] = 0
	                WHERE [Guid] IN ('20F97A93-7949-4C2A-8A5E-C756FE8585CA', 'D47858C0-0E6E-46DC-AE99-8EC84BA5F45F', '62C70118-0A6F-432A-9D84-A5296655CB9E', 'AB045324-60A4-4972-8936-7B319FF5D2CE')

                ;WITH ctePages(PageId)
                    AS (
                        SELECT [Id]
                        FROM [Page] p
                        WHERE [Guid] IN ('20F97A93-7949-4C2A-8A5E-C756FE8585CA', 'D47858C0-0E6E-46DC-AE99-8EC84BA5F45F', '62C70118-0A6F-432A-9D84-A5296655CB9E', 'AB045324-60A4-4972-8936-7B319FF5D2CE')
                        UNION ALL
                        SELECT page.[Id]
			                FROM [Page] page
			                    INNER JOIN ctePages r
				                ON page.ParentPageId = r.[PageId]
                    )
                    UPDATE [Page] 
		                SET [AllowIndexing] = 0
		                WHERE [Id] IN (SELECT [PageId] FROM ctePages)" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.Site", "AllowIndexing");
            DropColumn("dbo.Site", "PageHeaderContent");
            DropColumn("dbo.Page", "AllowIndexing");
        }
    }
}
