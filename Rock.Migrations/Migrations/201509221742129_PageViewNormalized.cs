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
    public partial class PageViewNormalized : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.PageViewSession",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    PageViewUserAgentId = c.Int( nullable: false ),
                    SessionId = c.Guid(),
                    IpAddress = c.String( maxLength: 45 ),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PageViewUserAgent", t => t.PageViewUserAgentId )
                .Index( t => t.PageViewUserAgentId )
                .Index( t => t.SessionId )
                .Index( t => t.Guid, unique: true )
                .Index( t => t.ForeignId )
                .Index( t => t.ForeignGuid )
                .Index( t => t.ForeignKey );

            CreateTable(
                "dbo.PageViewUserAgent",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    UserAgent = c.String( maxLength: 450 ),
                    ClientType = c.String( maxLength: 25 ),
                    OperatingSystem = c.String( maxLength: 100 ),
                    Browser = c.String( maxLength: 100 ),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .Index( t => t.UserAgent, unique: true )
                .Index( t => t.Guid, unique: true )
                .Index( t => t.ForeignId )
                .Index( t => t.ForeignGuid )
                .Index( t => t.ForeignKey );

            AddColumn( "dbo.PageView", "PageViewSessionId", c => c.Int( nullable: true ) );

            // Populate PageViewUserAgent from pre-converted PageView table
            Sql( @"INSERT INTO PageViewUserAgent (
    UserAgent
    ,[Guid]
    ) (
    SELECT UserAgent
    ,NEWID() FROM PageView WHERE UserAgent NOT IN (
        SELECT UserAgent
        FROM PageViewUserAgent
        ) GROUP BY UserAgent
    )

INSERT INTO PageViewSession (
    PageViewUserAgentId
    ,SessionId
    ,IPAddress
    ,[Guid]
    )
SELECT ua.Id
    ,pv.SessionId
    ,pv.IpAddress
    ,NEWID() [Guid]
FROM (
    SELECT DISTINCT UserAgent
        ,SessionId
        ,IpAddress
    FROM PageView
    ) pv
JOIN PageViewUserAgent ua ON isnull(ua.UserAgent,'') = isnull(pv.UserAgent,'')" );

            Sql( @"UPDATE PageView
SET PageViewSessionId = j.Id
FROM PageView pv
JOIN (
    SELECT s.Id
        ,ua.UserAgent
        ,s.SessionId
        ,s.IpAddress
    FROM PageViewSession s
    JOIN PageViewUserAgent ua ON s.PageViewUserAgentId = ua.Id
    ) j ON isnull(j.UserAgent, '') = isnull(pv.UserAgent, '')
    AND j.SessionId = pv.SessionId
    AND j.IpAddress = pv.IpAddress" );


            DropColumn( "dbo.PageView", "UserAgent" );
            DropColumn( "dbo.PageView", "ClientType" );
            DropColumn( "dbo.PageView", "SessionId" );
            DropColumn( "dbo.PageView", "IpAddress" );

            AlterColumn( "dbo.PageView", "PageViewSessionId", c => c.Int( nullable: false ) );
            CreateIndex( "dbo.PageView", "PageViewSessionId" );
            AddForeignKey( "dbo.PageView", "PageViewSessionId", "dbo.PageViewSession", "Id" );

            AddColumn( "dbo.Site", "EnablePageViews", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.Site", "PageViewRetentionPeriodDays", c => c.Int() );

            Sql( "UPDATE [Site] SET [EnablePageViews] = 1" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn( "dbo.Site", "PageViewRetentionPeriodDays" );
            DropColumn( "dbo.Site", "EnablePageViews" );

            AddColumn( "dbo.PageView", "IpAddress", c => c.String( maxLength: 45 ) );
            AddColumn( "dbo.PageView", "SessionId", c => c.Guid() );
            AddColumn( "dbo.PageView", "ClientType", c => c.String( maxLength: 25 ) );
            AddColumn( "dbo.PageView", "UserAgent", c => c.String( maxLength: 500 ) );
            DropForeignKey( "dbo.PageView", "PageViewSessionId", "dbo.PageViewSession" );
            DropForeignKey( "dbo.PageViewSession", "PageViewUserAgentId", "dbo.PageViewUserAgent" );
            DropIndex( "dbo.PageViewUserAgent", new[] { "ForeignKey" } );
            DropIndex( "dbo.PageViewUserAgent", new[] { "ForeignGuid" } );
            DropIndex( "dbo.PageViewUserAgent", new[] { "ForeignId" } );
            DropIndex( "dbo.PageViewUserAgent", new[] { "Guid" } );
            DropIndex( "dbo.PageViewUserAgent", new[] { "UserAgent" } );
            DropIndex( "dbo.PageViewSession", new[] { "ForeignKey" } );
            DropIndex( "dbo.PageViewSession", new[] { "ForeignGuid" } );
            DropIndex( "dbo.PageViewSession", new[] { "ForeignId" } );
            DropIndex( "dbo.PageViewSession", new[] { "Guid" } );
            DropIndex( "dbo.PageViewSession", new[] { "SessionId" } );
            DropIndex( "dbo.PageViewSession", new[] { "PageViewUserAgentId" } );
            DropIndex( "dbo.PageView", new[] { "DateTimeViewed" } );
            DropIndex( "dbo.PageView", new[] { "PageViewSessionId" } );
            DropColumn( "dbo.PageView", "PageViewSessionId" );
            DropTable( "dbo.PageViewUserAgent" );
            DropTable( "dbo.PageViewSession" );
        }
    }
}
