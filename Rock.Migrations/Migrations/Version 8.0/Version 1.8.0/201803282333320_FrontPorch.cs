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
    public partial class FrontPorch : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"INSERT INTO DefinedValue( IsSystem, DefinedTypeId, [Order], [Value], [Description], [Guid] )
                VALUES( 0, ( SELECT Id FROM DefinedType WHERE[Guid] = 'A55849D7-5C7B-4B36-9024-A672169E4C9C' ), 3, 'Windows', 'A Windows device', NEWID())" );

            Sql( @"INSERT INTO DefinedValue( IsSystem, DefinedTypeId, [Order], [Value], [Description], [Guid] )
                VALUES( 0, ( SELECT Id FROM DefinedType WHERE[Guid] = 'A55849D7-5C7B-4B36-9024-A672169E4C9C' ), 3, 'Mac', 'A Macintosh device', NEWID())" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
