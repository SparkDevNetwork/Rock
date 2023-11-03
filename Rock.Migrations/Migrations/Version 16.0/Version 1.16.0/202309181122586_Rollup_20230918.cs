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
    public partial class Rollup_20230918 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RemoveLegacyLavaGlobalAttribute_Up();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// DL: Data Migration to Remove LavaSupportLevel Attribute
        /// Remove the Legacy Lava Attribute Global Setting.
        /// Legacy Lava is no longer supported as of v1.16.
        /// </summary>
        private void RemoveLegacyLavaGlobalAttribute_Up()
        {
            Sql( @"
                DELETE FROM [AttributeQualifier] WHERE [AttributeId] IN ( SELECT [Id] FROM [Attribute] WHERE [key] = 'core.LavaSupportLevel' );
                " );

            Sql( @"
                DELETE FROM [Attribute] WHERE [key] = 'core.LavaSupportLevel';
                " );
        }
    }
}
