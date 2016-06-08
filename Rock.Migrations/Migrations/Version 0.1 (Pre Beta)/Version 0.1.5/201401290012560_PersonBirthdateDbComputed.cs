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
    public partial class PersonBirthdateDbComputed : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
ALTER TABLE PERSON ADD [BirthDate] as (
  case when isdate(ISNULL(CAST(BirthYear AS varchar), '0001') + '-' + CAST(BirthMonth AS varchar) + '-' + CAST(BirthDay AS varchar)) > 0 then
    CAST(ISNULL(CAST(BirthYear AS varchar), '0001') + '-' + CAST(BirthMonth AS varchar) + '-' + CAST(BirthDay AS varchar) AS DATE)
  else null
  end)
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.Person", "BirthDate");
        }
    }
}
