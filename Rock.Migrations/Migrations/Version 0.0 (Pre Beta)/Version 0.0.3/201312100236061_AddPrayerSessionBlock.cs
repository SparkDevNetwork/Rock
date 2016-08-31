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
    public partial class AddPrayerSessionBlock : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddBlockType( "Prayer Session", "Allows a user to start a session to pray for prayer requests.", "~/Blocks/Prayer/PrayerSession.ascx", "FD294789-3B72-4D83-8006-FA50B5087D06" );
            AddBlock( "59C38C86-AAB2-4864-AE05-04508BD783F0", "", "FD294789-3B72-4D83-8006-FA50B5087D06", "Prayer Session", "Main", 0, "5B14A661-6F79-43ED-96C3-009EBC29AE5E" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteBlock( "5B14A661-6F79-43ED-96C3-009EBC29AE5E" );
            DeleteBlockType( "FD294789-3B72-4D83-8006-FA50B5087D06" );
        }
    }
}
