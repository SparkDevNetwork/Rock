// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace com.ccvonline.Residency.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    ///
    /// </summary>
    public partial class UpdateResidencyGroupBlockAttributes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Residency Groups - Group List: Limit to GroupType Residency
            AddBlockAttributeValue( "AD59F37C-97EC-4F07-A604-3AAF8270C737", "C3FD6CE3-D37F-4A53-B0D7-AB1B1F252324", "00043ce6-eb1b-43b5-a12a-4552b91a3e28" );

            // Residency Groups - Group Detail: Limit to GroupType Residency
            AddBlockAttributeValue( "746B438D-75E3-495A-9678-C3C14629511A", "15AC7A62-7BF2-44B7-93CD-EA8F96BF529A", "00043ce6-eb1b-43b5-a12a-4552b91a3e28" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
