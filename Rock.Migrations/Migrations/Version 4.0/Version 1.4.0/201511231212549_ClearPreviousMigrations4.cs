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
    public partial class ClearPreviousMigrations4 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddGlobalAttribute( "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Enable Auditing", "Enable the saving of audit information for every row/field change made in Rock.", 0, "false", "66B13C02-CBA0-4427-9D60-8B331A51CC96" );

            Sql( @"
    UPDATE[SystemEmail]
    SET[Body] = REPLACE( [Body], '?GroupId={{ Group.Id }}&Occurrence=', '?{{ Person.ImpersonationParameter }}&GroupId={{ Group.Id }}&Occurrence=' )
    WHERE[Guid] = 'ED567FDE-A3B4-4827-899D-C2740DF3E5DA'

    UPDATE [__MigrationHistory] SET [Model] = 0x

" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
