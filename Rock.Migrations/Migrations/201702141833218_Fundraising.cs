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
    public partial class Fundraising : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // NOTE: There needs to be a SQL script that does the equivalent of this migration so that it can be run on 1.6.x versions of Rock

            // TODO: Attribute Categories
            // Fundraising Public | Group

            // TODO: 'Fundraising Opportunity Term' Defined Value

            // TODO: GroupType: Fundraising Opportunity
            // A bunch of group and groupmember Attributes: for Fundraising Opportunity grouptype

            // TODO: Add TransactionType of Fundraising

            // TODO: Add NoteType 'Fundraising Opportunity Comment' Guid:9BB1A7B6-0E51-4E0E-BFC0-1E42F4F2DA95
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
