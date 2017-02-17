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

            // TODO: GroupType: Fundraising Opportunity 4BE7FC44-332D-40A8-978E-47B7035D7A0C
            // A bunch of group and groupmember Attributes: for Fundraising Opportunity grouptype
            // Migration for GroupRoles too
            // * Participant F82DF077-9664-4DA8-A3D9-7379B690124D
            // * Leader 253973A5-18F2-49B6-B2F1-F8F84294AAB2

            // TODO: Add TransactionType of Fundraising

            // TODO: Add NoteType 'Fundraising Opportunity Comment' Guid:9BB1A7B6-0E51-4E0E-BFC0-1E42F4F2DA95
            // NOTE: make sure AllUsers have EDIT auth (the block will control when Edit/Add is allowed)
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
