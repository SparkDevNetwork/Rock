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
    public partial class CampusTimeZone : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.Campus", "TimeZoneId", c => c.String( maxLength: 50 ) );
            AddColumn( "dbo.MetaPersonicxLifestageCluster", "MeanAge", c => c.Int() );
            AddColumn( "dbo.MetaPersonicxLifestageGroup", "MeanAge", c => c.Int() );

            Sql( MigrationSQL._201712191949335_CampusTimeZone_PersonicxMeanAge );

            Sql( @"
UPDATE [Group]
SET [SyncDataViewId] = NULL
 ,[Name] = 'Members and Attendees'
WHERE [Guid] = 'D3DC9A8E-43D9-43AB-BB48-94788F4B1A42'
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn( "dbo.MetaPersonicxLifestageGroup", "MeanAge" );
            DropColumn( "dbo.MetaPersonicxLifestageCluster", "MeanAge" );
            DropColumn( "dbo.Campus", "TimeZoneId" );
        }
    }
}
