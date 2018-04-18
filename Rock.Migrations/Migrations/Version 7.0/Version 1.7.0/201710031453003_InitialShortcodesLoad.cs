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
    public partial class InitialShortcodesLoad : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // load shortcodes into Rock
            Sql( MigrationSQL._201710031453003_InitialShortcodesLoad );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"DELETE FROM [LavaShortcode] WHERE [Guid] IN ('2FA4D446-3F63-4DFD-8C6A-55DBA76AEB83','C74AC163-0D90-4E9A-8BFB-A13DFA053CA2','FE298210-1307-49DF-B28B-3735A414CCA0','2DD53FE6-6EB2-4EC8-A965-3F71054F7983','18F87671-A848-4509-8058-C95682E7BAD4','43819A34-4819-4507-8FEA-2E406B5474EA','ADB1F75D-500D-4305-9805-99AF04A2CD88','CA9B54BF-EF0A-4B08-884F-7042A6B3EAF4')" );
        }
    }
}
