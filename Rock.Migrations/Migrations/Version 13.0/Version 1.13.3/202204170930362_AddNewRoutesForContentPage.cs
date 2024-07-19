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
    public partial class AddNewRoutesForContentPage : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.AddPageRoute( "117B547B-9D71-4EE9-8047-176676F5DC8C", "web/content/{CategoryGuid}/{ContentChannelGuid}", "6EAA6912-A21B-485A-B4B3-AFD95058A7C0" );
            RockMigrationHelper.AddPageRoute( "117B547B-9D71-4EE9-8047-176676F5DC8C", "web/content/category/{CategoryGuid}", "51BA3717-E15E-471A-83A6-A2FEC2D23898" );
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql(@"
            DELETE FROM PageRoute WHERE Route = 'web/content/{CategoryGuid}/{ContentChannelGuid}'
            DELETE FROM PageRoute WHERE Route = 'web/content/category/{CategoryGuid}'
            ");
        }
    }
}
